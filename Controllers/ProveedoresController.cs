using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/proveedores")]
public class ProveedoresController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProveedoresController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var proveedores = await _db.Proveedores
            .AsNoTracking()
            .Where(x => x.EsActivo)
            .OrderBy(x => x.RazonSocial)
            .ToListAsync();

        return Ok(proveedores);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Proveedor proveedor)
    {
        proveedor.Id = 0;

        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();

        return Ok(proveedor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Proveedor request)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);

        if (proveedor is null)
        {
            return NotFound();
        }

        proveedor.Ruc = request.Ruc;
        proveedor.RazonSocial = request.RazonSocial;
        proveedor.Telefono = request.Telefono;
        proveedor.Email = request.Email;
        proveedor.Direccion = request.Direccion;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
