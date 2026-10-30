using System.Text.Json;
using ClinicaAmelita.Api.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicaAmelita.Api.Services;

public class RecetaPdfService
{
    private readonly IWebHostEnvironment _environment;

    public RecetaPdfService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public record ConfiguracionReceta(
        string NombreClinica,
        string? Subtitulo,
        string? Ruc,
        string? Direccion,
        string? Telefono,
        string? PiePagina,
        string? PlantillaArchivo
    );

    public async Task<ConfiguracionReceta> ObtenerConfiguracionAsync()
    {
        var ruta = ObtenerRutaConfiguracion();

        if (!File.Exists(ruta))
        {
            return ConfiguracionPredeterminada();
        }

        var json = await File.ReadAllTextAsync(ruta);
        var configuracion = JsonSerializer.Deserialize<ConfiguracionReceta>(json);

        return configuracion ?? ConfiguracionPredeterminada();
    }

    public async Task GuardarConfiguracionAsync(ConfiguracionReceta configuracion)
    {
        CrearDirectorioSiNoExiste();

        var actual = await ObtenerConfiguracionAsync();
        var nuevaConfiguracion = configuracion with
        {
            PlantillaArchivo = actual.PlantillaArchivo
        };

        var json = JsonSerializer.Serialize(
            nuevaConfiguracion,
            new JsonSerializerOptions { WriteIndented = true }
        );

        await File.WriteAllTextAsync(ObtenerRutaConfiguracion(), json);
    }

    public async Task<string> GuardarPlantillaAsync(IFormFile archivo)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg" };

        if (!extensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("La plantilla debe ser una imagen PNG o JPG.");
        }

        CrearDirectorioSiNoExiste();

        var nombreArchivo = $"plantilla-receta{extension}";
        var rutaArchivo = Path.Combine(ObtenerDirectorio(), nombreArchivo);

        foreach (var archivoAnterior in Directory.GetFiles(ObtenerDirectorio(), "plantilla-receta.*"))
        {
            File.Delete(archivoAnterior);
        }

        await using (var stream = File.Create(rutaArchivo))
        {
            await archivo.CopyToAsync(stream);
        }

        var configuracion = await ObtenerConfiguracionAsync();
        var nuevaConfiguracion = configuracion with
        {
            PlantillaArchivo = nombreArchivo
        };

        var json = JsonSerializer.Serialize(
            nuevaConfiguracion,
            new JsonSerializerOptions { WriteIndented = true }
        );

        await File.WriteAllTextAsync(ObtenerRutaConfiguracion(), json);

        return nombreArchivo;
    }

    public async Task<(byte[] Bytes, string ContentType, string NombreArchivo)?> ObtenerPlantillaAsync()
    {
        var configuracion = await ObtenerConfiguracionAsync();

        if (string.IsNullOrWhiteSpace(configuracion.PlantillaArchivo))
        {
            return null;
        }

        var ruta = Path.Combine(ObtenerDirectorio(), configuracion.PlantillaArchivo);

        if (!File.Exists(ruta))
        {
            return null;
        }

        var extension = Path.GetExtension(ruta).ToLowerInvariant();
        var contentType = extension == ".png" ? "image/png" : "image/jpeg";

        return (
            await File.ReadAllBytesAsync(ruta),
            contentType,
            configuracion.PlantillaArchivo
        );
    }

    public async Task<byte[]> GenerarAsync(Receta receta, Usuario medicoImprime)
    {
        var configuracion = await ObtenerConfiguracionAsync();
        var rutaPlantilla = ObtenerRutaPlantilla(configuracion);
        var rutaLogo = ObtenerRutaLogo();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(12);
                page.DefaultTextStyle(x => x.FontSize(9));

                if (!string.IsNullOrWhiteSpace(rutaPlantilla) && File.Exists(rutaPlantilla))
                {
                    page.Background().Image(rutaPlantilla).FitArea();
                }

                page.Content().Row(row =>
                {
                    row.RelativeItem().Element(c => DibujarReceta(
                        c,
                        receta,
                        medicoImprime,
                        configuracion,
                        rutaLogo,
                        false
                    ));

                    row.ConstantItem(14).Element(DibujarLineaCorte);

                    row.RelativeItem().Element(c => DibujarReceta(
                        c,
                        receta,
                        medicoImprime,
                        configuracion,
                        rutaLogo,
                        true
                    ));
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void DibujarReceta(
        IContainer container,
        Receta receta,
        Usuario medicoImprime,
        ConfiguracionReceta configuracion,
        string? rutaLogo,
        bool esIndicaciones)
    {
        container
            .Padding(12)
            .Column(column =>
            {
                column.Spacing(7);

                column.Item().Height(82).Row(header =>
                {
                    header.ConstantItem(84).AlignMiddle().AlignCenter().Element(logo =>
                    {
                        if (!string.IsNullOrWhiteSpace(rutaLogo) && File.Exists(rutaLogo))
                        {
                            logo.Image(rutaLogo).FitArea();
                        }
                    });

                    header.RelativeItem().PaddingLeft(8).AlignMiddle().Column(datos =>
                    {
                        datos.Item().AlignCenter().Text(configuracion.NombreClinica)
                            .FontSize(19)
                            .Bold()
                            .FontColor("#174F7E");

                        if (!string.IsNullOrWhiteSpace(configuracion.Subtitulo))
                        {
                            datos.Item().AlignCenter().Text(configuracion.Subtitulo)
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken2);
                        }

                        datos.Item().PaddingTop(3).AlignCenter().Text(
                            $"Dr(a). {receta.MedicoUsuario.Nombre} {receta.MedicoUsuario.Apellido}"
                        )
                            .FontSize(9)
                            .Bold();

                        var contacto = string.Join(
                            "  •  ",
                            new[] { configuracion.Direccion, configuracion.Telefono }
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                        );

                        if (!string.IsNullOrWhiteSpace(contacto))
                        {
                            datos.Item().AlignCenter().Text(contacto).FontSize(7);
                        }
                    });
                });

                column.Item().LineHorizontal(1).LineColor("#7C99B2");
                column.Item().Element(c => DatosPaciente(c, receta));

                column.Item()
                    .PaddingTop(3)
                    .Text(esIndicaciones ? "Indicaciones:" : "Rp.:")
                    .FontSize(13)
                    .Bold()
                    .Italic()
                    .FontColor("#2B5D87");

                column.Item()
                    .MinHeight(230)
                    .PaddingVertical(10)
                    .Column(contenido =>
                    {
                        contenido.Spacing(8);

                        foreach (var detalle in receta.Detalles.Where(x => x.EsActivo))
                        {
                            contenido.Item().Column(item =>
                            {
                                if (esIndicaciones)
                                {
                                    item.Item().DefaultTextStyle(x => x.FontSize(10)).Text(texto =>
                                    {
                                        texto.Span(detalle.Medicamento + ": ").Bold();
                                        texto.Span(ObtenerIndicaciones(detalle));
                                    });
                                }
                                else
                                {
                                    var cantidad = detalle.Cantidad.HasValue
                                        ? $" # {detalle.Cantidad.Value:0.##} ({CantidadEnLetras(detalle.Cantidad.Value)})"
                                        : "";
                                    item.Item().Text($"{detalle.Medicamento} {detalle.Presentacion}{cantidad}")
                                        .Bold().FontSize(11);
                                    if (!string.IsNullOrWhiteSpace(detalle.Producto?.PrincipioActivo))
                                    {
                                        item.Item().Text(detalle.Producto.PrincipioActivo).FontSize(10);
                                    }
                                }
                            });
                        }

                        if (esIndicaciones && !string.IsNullOrWhiteSpace(receta.IndicacionesGenerales))
                        {
                            contenido.Item().PaddingTop(4).Text(texto =>
                            {
                                texto.Span("Indicaciones: ").Bold();
                                texto.Span(receta.IndicacionesGenerales);
                            });
                        }
                    });

                column.Item().PaddingTop(12).AlignCenter().Text($"Dr(a). {receta.MedicoUsuario.Nombre} {receta.MedicoUsuario.Apellido}").Bold();
                column.Item().AlignCenter().Text("Firma: __________________________").FontSize(8);
                if (!string.IsNullOrWhiteSpace(receta.Consulta?.Diagnostico))
                {
                    column.Item().Text($"Diagnóstico / CIE 10: {receta.Consulta.Diagnostico}").FontSize(9);
                }

                column.Item().PaddingTop(5).AlignCenter().Text(
                    string.IsNullOrWhiteSpace(configuracion.PiePagina)
                        ? "RECETARIO"
                        : configuracion.PiePagina
                )
                    .FontSize(7)
                    .LetterSpacing(2)
                    .FontColor("#2B5D87");
            });
    }

    private static string ObtenerIndicaciones(RecetaDetalle detalle)
    {
        var frecuencia = detalle.Frecuencia.Trim();
        if (!string.IsNullOrWhiteSpace(frecuencia) && !frecuencia.StartsWith("cada", StringComparison.OrdinalIgnoreCase))
        {
            frecuencia = $"cada {frecuencia}";
        }
        var duracion = detalle.Duracion.Trim();
        if (!string.IsNullOrWhiteSpace(duracion) && !duracion.StartsWith("por", StringComparison.OrdinalIgnoreCase))
        {
            duracion = $"por {duracion}";
        }
        var pauta = string.Join(" ", new[] { detalle.Dosis, frecuencia, duracion }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.Join(". ", new[] { pauta, detalle.Indicaciones }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    public static string CantidadEnLetras(decimal cantidad)
    {
        if (cantidad < 0) return "MENOS " + CantidadEnLetras(-cantidad);
        if (cantidad != decimal.Truncate(cantidad))
        {
            var partes = cantidad.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture).Split('.');
            return CantidadEnLetras(decimal.Parse(partes[0], System.Globalization.CultureInfo.InvariantCulture))
                + " COMA " + string.Join(" ", partes[1].Select(x => CantidadEnLetras(x - '0')));
        }
        string[] unidades = { "CERO", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE", "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUNO", "VEINTIDÓS", "VEINTITRÉS", "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE" };
        if (cantidad < 30) return unidades[(int)cantidad];
        if (cantidad < 100)
        {
            string[] decenas = { "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
            return decenas[(int)(cantidad / 10)] + (cantidad % 10 == 0 ? "" : " Y " + CantidadEnLetras(cantidad % 10));
        }
        if (cantidad == 100) return "CIEN";
        if (cantidad < 1000)
        {
            string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };
            return centenas[(int)(cantidad / 100)] + (cantidad % 100 == 0 ? "" : " " + CantidadEnLetras(cantidad % 100));
        }
        if (cantidad < 1000000)
        {
            var miles = decimal.Truncate(cantidad / 1000);
            return (miles == 1 ? "MIL" : Apocopar(CantidadEnLetras(miles)) + " MIL")
                + (cantidad % 1000 == 0 ? "" : " " + CantidadEnLetras(cantidad % 1000));
        }
        if (cantidad < 1000000000000m)
        {
            var millones = decimal.Truncate(cantidad / 1000000);
            return (millones == 1 ? "UN MILLÓN" : Apocopar(CantidadEnLetras(millones)) + " MILLONES")
                + (cantidad % 1000000 == 0 ? "" : " " + CantidadEnLetras(cantidad % 1000000));
        }
        var billones = decimal.Truncate(cantidad / 1000000000000m);
        return (billones == 1 ? "UN BILLÓN" : Apocopar(CantidadEnLetras(billones)) + " BILLONES")
            + (cantidad % 1000000000000m == 0 ? "" : " " + CantidadEnLetras(cantidad % 1000000000000m));
    }

    private static string Apocopar(string numero)
    {
        if (numero.EndsWith("VEINTIUNO")) return numero[..^9] + "VEINTIÚN";
        return numero.EndsWith("UNO") ? numero[..^3] + "UN" : numero;
    }

    private static void DatosPaciente(IContainer container, Receta receta)
    {
        var alergias = receta.Paciente.HistoriasClinicas
            .Where(x => x.EsActivo && !string.IsNullOrWhiteSpace(x.Alergias))
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => x.Alergias)
            .FirstOrDefault() ?? "No registradas";

        container.Column(datos =>
        {
            datos.Spacing(5);

            datos.Item().Text(texto =>
            {
                texto.Span("Nombre: ").Bold();
                texto.Span($"{receta.Paciente.Nombres} {receta.Paciente.Apellidos}");
            });

            datos.Item().Row(row =>
            {
                row.RelativeItem().Text(texto =>
                {
                    texto.Span("Edad: ").Bold();
                    texto.Span(CalcularEdad(receta.Paciente.FechaNacimiento));
                });

                row.RelativeItem().Text(texto =>
                {
                    texto.Span("Fecha: ").Bold();
                    texto.Span(receta.FechaEmision.AddHours(-5).ToString("dd/MM/yyyy"));
                });

                row.RelativeItem().Text(texto =>
                {
                    texto.Span("C.I.: ").Bold();
                    texto.Span(receta.Paciente.Identificacion);
                });
            });

            datos.Item().Text(texto =>
            {
                texto.Span("Alergias: ").Bold();
                texto.Span(alergias);
            });
        });
    }

    private static void DibujarLineaCorte(IContainer container)
    {
        container.AlignCenter().Column(column =>
        {
            for (var i = 0; i < 52; i++)
            {
                column.Item()
                    .AlignCenter()
                    .Width(1)
                    .Height(5)
                    .Background(Colors.Grey.Lighten1);

                column.Item().Height(3);
            }
        });
    }

    private static string CalcularEdad(DateOnly? fechaNacimiento)
    {
        if (!fechaNacimiento.HasValue)
        {
            return "-";
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
        var edad = hoy.Year - fechaNacimiento.Value.Year;

        if (fechaNacimiento.Value > hoy.AddYears(-edad))
        {
            edad--;
        }

        return $"{edad} años";
    }

    private ConfiguracionReceta ConfiguracionPredeterminada()
    {
        return new ConfiguracionReceta(
            "Consultorio Amelita",
            "Medicina general",
            null,
            null,
            null,
            "RECETARIO",
            null
        );
    }

    private string? ObtenerRutaPlantilla(ConfiguracionReceta configuracion)
    {
        if (string.IsNullOrWhiteSpace(configuracion.PlantillaArchivo))
        {
            return null;
        }

        return Path.Combine(ObtenerDirectorio(), configuracion.PlantillaArchivo);
    }

    private string? ObtenerRutaLogo()
    {
        var ruta = Path.Combine(ObtenerDirectorio(), "logo-consultorio-amelita.png");
        return File.Exists(ruta) ? ruta : null;
    }

    private string ObtenerDirectorio()
    {
        return Path.Combine(_environment.ContentRootPath, "App_Data", "receta-template");
    }

    private string ObtenerRutaConfiguracion()
    {
        return Path.Combine(ObtenerDirectorio(), "config.json");
    }

    private void CrearDirectorioSiNoExiste()
    {
        Directory.CreateDirectory(ObtenerDirectorio());
    }
}
