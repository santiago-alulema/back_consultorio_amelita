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
                        rutaLogo
                    ));

                    row.ConstantItem(14).Element(DibujarLineaCorte);

                    row.RelativeItem().Element(c => DibujarReceta(
                        c,
                        receta,
                        medicoImprime,
                        configuracion,
                        rutaLogo
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
        string? rutaLogo)
    {
        container
            .Border(1)
            .BorderColor("#2B5D87")
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
                            $"Dr(a). {medicoImprime.Nombre} {medicoImprime.Apellido}"
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
                    .Text("Rp.:")
                    .FontSize(13)
                    .Bold()
                    .Italic()
                    .FontColor("#2B5D87");

                column.Item()
                    .MinHeight(285)
                    .Border(1)
                    .BorderColor("#B8C7D3")
                    .Padding(10)
                    .Column(contenido =>
                    {
                        contenido.Spacing(8);

                        foreach (var detalle in receta.Detalles.Where(x => x.EsActivo))
                        {
                            contenido.Item().Column(item =>
                            {
                                item.Item().Text(detalle.Medicamento)
                                    .Bold()
                                    .FontSize(10);

                                var indicaciones = string.Join(
                                    " · ",
                                    new[]
                                    {
                                        detalle.Dosis,
                                        string.IsNullOrWhiteSpace(detalle.Frecuencia)
                                            ? null
                                            : $"Cada {detalle.Frecuencia}",
                                        string.IsNullOrWhiteSpace(detalle.Duracion)
                                            ? null
                                            : $"por {detalle.Duracion}",
                                        detalle.Indicaciones
                                    }.Where(x => !string.IsNullOrWhiteSpace(x))
                                );

                                if (!string.IsNullOrWhiteSpace(indicaciones))
                                {
                                    item.Item().Text(indicaciones).FontSize(8.5f);
                                }
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(receta.IndicacionesGenerales))
                        {
                            contenido.Item().PaddingTop(4).Text(texto =>
                            {
                                texto.Span("Indicaciones: ").Bold();
                                texto.Span(receta.IndicacionesGenerales);
                            });
                        }
                    });

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
                    texto.Span(DateTime.UtcNow.AddHours(-5).ToString("dd/MM/yyyy"));
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
