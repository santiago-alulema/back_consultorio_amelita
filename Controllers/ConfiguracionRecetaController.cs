using ClinicaAmelita.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/configuracion-receta")]
public class ConfiguracionRecetaController : ControllerBase
{
    private readonly RecetaPdfService _recetaPdfService;

    public ConfiguracionRecetaController(RecetaPdfService recetaPdfService)
    {
        _recetaPdfService = recetaPdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var configuracion = await _recetaPdfService.ObtenerConfiguracionAsync();
        return Ok(configuracion);
    }

    [HttpPut]
    public async Task<IActionResult> Update(RecetaPdfService.ConfiguracionReceta request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreClinica))
        {
            return BadRequest("El nombre del consultorio es obligatorio.");
        }

        await _recetaPdfService.GuardarConfiguracionAsync(request);
        return NoContent();
    }

    [HttpPost("plantilla")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadPlantilla(IFormFile archivo)
    {
        if (archivo.Length == 0)
        {
            return BadRequest("Seleccione una imagen para la plantilla.");
        }

        try
        {
            var nombre = await _recetaPdfService.GuardarPlantillaAsync(archivo);

            return Ok(new
            {
                archivo = nombre
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("plantilla")]
    public async Task<IActionResult> GetPlantilla()
    {
        var plantilla = await _recetaPdfService.ObtenerPlantillaAsync();

        if (plantilla is null)
        {
            return NotFound();
        }

        return File(
            plantilla.Value.Bytes,
            plantilla.Value.ContentType,
            plantilla.Value.NombreArchivo
        );
    }
}
