using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //var migraciones = await db.Database.GetMigrationsAsync();

        //if (!migraciones.Any())
        //{
        //    throw new InvalidOperationException(
        //        "No existen migraciones de Entity Framework. Ejecuta 'dotnet ef migrations add InitialCreate' y luego inicia nuevamente la API."
        //    );
        //}

        //await db.Database.MigrateAsync();

        var permisos = new[]
        {
            ("DASHBOARD_VER", "Ver dashboard", "Dashboard"),
            ("PACIENTES_VER", "Ver pacientes", "Consultorio"),
            ("PACIENTES_EDITAR", "Crear y editar pacientes", "Consultorio"),
            ("HISTORIA_VER", "Ver historia clínica", "Consultorio"),
            ("HISTORIA_EDITAR", "Editar historia clínica", "Consultorio"),
            ("CONSULTAS_VER", "Ver consultas", "Consultorio"),
            ("CONSULTAS_CREAR", "Registrar consultas", "Consultorio"),
            ("RECETAS_VER", "Ver recetas", "Consultorio"),
            ("RECETAS_CREAR", "Emitir recetas", "Consultorio"),
            ("AGENDA_VER", "Ver agenda", "Consultorio"),
            ("AGENDA_EDITAR", "Administrar citas", "Consultorio"),
            ("PRODUCTOS_VER", "Ver productos", "Farmacia"),
            ("PRODUCTOS_EDITAR", "Editar productos", "Farmacia"),
            ("INVENTARIO_VER", "Ver inventario", "Farmacia"),
            ("INVENTARIO_EDITAR", "Ajustar inventario", "Farmacia"),
            ("INVENTARIO_IMPORTAR", "Importar inventario Excel", "Farmacia"),
            ("VENTAS_VER", "Ver ventas", "Farmacia"),
            ("VENTAS_CREAR", "Registrar ventas", "Farmacia"),
            ("PEDIDOS_VER", "Ver pedidos", "Farmacia"),
            ("PEDIDOS_EDITAR", "Crear y recibir pedidos", "Farmacia"),
            ("SRI_VER", "Ver documentos SRI", "Farmacia"),
            ("SRI_CARGAR", "Cargar XML/RIDE", "Farmacia"),
            ("CAJA_VER", "Ver caja", "Caja"),
            ("CAJA_EDITAR", "Aperturar, mover y cerrar caja", "Caja"),
            ("REPORTES_VER", "Ver reportes", "Reportes"),
            ("USUARIOS_VER", "Ver usuarios", "Seguridad"),
            ("USUARIOS_EDITAR", "Administrar usuarios", "Seguridad"),
            ("ROLES_EDITAR", "Administrar roles y permisos", "Seguridad")
        };

        foreach (var permiso in permisos)
        {
            var existe = await db.Permisos.AnyAsync(x => x.Codigo == permiso.Item1);

            if (!existe)
            {
                db.Permisos.Add(new Permiso
                {
                    Codigo = permiso.Item1,
                    Nombre = permiso.Item2,
                    Modulo = permiso.Item3
                });
            }
        }

        await db.SaveChangesAsync();

        var admin = await db.Roles
            .Include(x => x.RolPermisos)
            .FirstOrDefaultAsync(x => x.Nombre == "Administrador");

        if (admin is null)
        {
            admin = new Rol
            {
                Nombre = "Administrador",
                Descripcion = "Acceso completo"
            };

            db.Roles.Add(admin);
            await db.SaveChangesAsync();
        }

        var todosLosPermisos = await db.Permisos.ToListAsync();
        var permisosActuales = admin.RolPermisos
            .Select(x => x.PermisoId)
            .ToHashSet();

        db.RolesPermisos.AddRange(
            todosLosPermisos
                .Where(x => !permisosActuales.Contains(x.Id))
                .Select(x => new RolPermiso
                {
                    RolId = admin.Id,
                    PermisoId = x.Id
                })
        );

        var medico = await db.Roles
            .Include(x => x.RolPermisos)
            .FirstOrDefaultAsync(x => x.Nombre == "Medico");

        if (medico is null)
        {
            medico = new Rol
            {
                Nombre = "Medico",
                Descripcion = "Atención médica"
            };

            db.Roles.Add(medico);
            await db.SaveChangesAsync();
        }

        var permisosMedico = new[]
        {
            "DASHBOARD_VER",
            "PACIENTES_VER",
            "PACIENTES_EDITAR",
            "HISTORIA_VER",
            "HISTORIA_EDITAR",
            "CONSULTAS_VER",
            "CONSULTAS_CREAR",
            "RECETAS_VER",
            "RECETAS_CREAR",
            "AGENDA_VER",
            "AGENDA_EDITAR",
            "PRODUCTOS_VER"
        };

        var permisosMedicoActuales = medico.RolPermisos
            .Select(x => x.PermisoId)
            .ToHashSet();

        db.RolesPermisos.AddRange(
            todosLosPermisos
                .Where(x =>
                    permisosMedico.Contains(x.Codigo)
                    && !permisosMedicoActuales.Contains(x.Id)
                )
                .Select(x => new RolPermiso
                {
                    RolId = medico.Id,
                    PermisoId = x.Id
                })
        );

        if (!await db.Usuarios.AnyAsync())
        {
            var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();
            var (hash, salt) = passwordService.HashPassword("Admin123*");

            var usuario = new Usuario
            {
                Nombre = "Administrador",
                Apellido = "Consultorio Amelita",
                NombreUsuario = "admin",
                PasswordHash = hash,
                PasswordSalt = salt
            };

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            db.UsuariosRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = admin.Id
            });
        }

        await db.SaveChangesAsync();
    }
}
