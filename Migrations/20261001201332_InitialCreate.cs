using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicaAmelita.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clinica");

            migrationBuilder.CreateTable(
                name: "Auditorias",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidad = table.Column<string>(type: "text", nullable: false),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    RegistroId = table.Column<string>(type: "text", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Datos = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosSri",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaveAcceso = table.Column<string>(type: "text", nullable: false),
                    NumeroFactura = table.Column<string>(type: "text", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RucEmisor = table.Column<string>(type: "text", nullable: false),
                    RazonSocialEmisor = table.Column<string>(type: "text", nullable: false),
                    IdentificacionComprador = table.Column<string>(type: "text", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    XmlPath = table.Column<string>(type: "text", nullable: false),
                    RidePdfPath = table.Column<string>(type: "text", nullable: true),
                    InventarioProcesado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosSri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identificacion = table.Column<string>(type: "text", nullable: false),
                    Nombres = table.Column<string>(type: "text", nullable: false),
                    Apellidos = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Sexo = table.Column<string>(type: "text", nullable: true),
                    TipoSangre = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    ContactoEmergencia = table.Column<string>(type: "text", nullable: true),
                    TelefonoEmergencia = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Modulo = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    PrincipioActivo = table.Column<string>(type: "text", nullable: true),
                    Laboratorio = table.Column<string>(type: "text", nullable: true),
                    Presentacion = table.Column<string>(type: "text", nullable: true),
                    Concentracion = table.Column<string>(type: "text", nullable: true),
                    UnidadMedida = table.Column<string>(type: "text", nullable: true),
                    UnidadesPorCaja = table.Column<decimal>(type: "numeric", nullable: true),
                    PrecioCaja = table.Column<decimal>(type: "numeric", nullable: true),
                    PrecioCajaDescuento = table.Column<decimal>(type: "numeric", nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ruc = table.Column<string>(type: "text", nullable: false),
                    RazonSocial = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    NombreUsuario = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaVenta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdentificacionCliente = table.Column<string>(type: "text", nullable: true),
                    NombreCliente = table.Column<string>(type: "text", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPago = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosSriDetalles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentoSriId = table.Column<int>(type: "integer", nullable: false),
                    CodigoPrincipal = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioTotalSinImpuesto = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosSriDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosSriDetalles_DocumentosSri_DocumentoSriId",
                        column: x => x.DocumentoSriId,
                        principalSchema: "clinica",
                        principalTable: "DocumentosSri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoriasClinicas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    Alergias = table.Column<string>(type: "text", nullable: true),
                    AntecedentesPersonales = table.Column<string>(type: "text", nullable: true),
                    AntecedentesFamiliares = table.Column<string>(type: "text", nullable: true),
                    Cirugias = table.Column<string>(type: "text", nullable: true),
                    MedicacionHabitual = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriasClinicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriasClinicas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalSchema: "clinica",
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotesProductos",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroLote = table.Column<string>(type: "text", nullable: false),
                    FechaCaducidad = table.Column<DateOnly>(type: "date", nullable: true),
                    StockActual = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    PrecioCompra = table.Column<decimal>(type: "numeric", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotesProductos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "clinica",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProveedorId = table.Column<int>(type: "integer", nullable: true),
                    FechaPedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalSchema: "clinica",
                        principalTable: "Proveedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                schema: "clinica",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    PermisoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalSchema: "clinica",
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "clinica",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontoApertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoEsperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoContado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Diferencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    ObservacionesApertura = table.Column<string>(type: "text", nullable: true),
                    ObservacionesCierre = table.Column<string>(type: "text", nullable: true),
                    UsuarioAperturaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioCierreId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cajas_Usuarios_UsuarioAperturaId",
                        column: x => x.UsuarioAperturaId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cajas_Usuarios_UsuarioCierreId",
                        column: x => x.UsuarioCierreId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    MedicoUsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FechaConsulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "text", nullable: false),
                    EnfermedadActual = table.Column<string>(type: "text", nullable: true),
                    PesoKg = table.Column<decimal>(type: "numeric", nullable: true),
                    TallaCm = table.Column<decimal>(type: "numeric", nullable: true),
                    Temperatura = table.Column<decimal>(type: "numeric", nullable: true),
                    PresionArterial = table.Column<string>(type: "text", nullable: true),
                    FrecuenciaCardiaca = table.Column<int>(type: "integer", nullable: true),
                    SaturacionOxigeno = table.Column<int>(type: "integer", nullable: true),
                    ExamenFisico = table.Column<string>(type: "text", nullable: true),
                    Diagnostico = table.Column<string>(type: "text", nullable: true),
                    Tratamiento = table.Column<string>(type: "text", nullable: true),
                    Indicaciones = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    ValorConsulta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPago = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalSchema: "clinica",
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultas_Usuarios_MedicoUsuarioId",
                        column: x => x.MedicoUsuarioId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                schema: "clinica",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "clinica",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentasDetalles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentasDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "clinica",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VentasDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "clinica",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    LoteProductoId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    StockAnterior = table.Column<decimal>(type: "numeric", nullable: false),
                    StockPosterior = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Referencia = table.Column<string>(type: "text", nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_LotesProductos_LoteProductoId",
                        column: x => x.LoteProductoId,
                        principalSchema: "clinica",
                        principalTable: "LotesProductos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "clinica",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidosDetalles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "numeric", nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosDetalles_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalSchema: "clinica",
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidosDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "clinica",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesPedidos",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesPedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesPedidos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalSchema: "clinica",
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConteosCaja",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CajaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Denominacion = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalSchema: "clinica",
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CajaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Referencia = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalSchema: "clinica",
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    MedicoUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    FechaHoraInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    ConsultaId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalSchema: "clinica",
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalSchema: "clinica",
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Usuarios_MedicoUsuarioId",
                        column: x => x.MedicoUsuarioId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recetas",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    ConsultaId = table.Column<int>(type: "integer", nullable: true),
                    MedicoUsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IndicacionesGenerales = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recetas_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalSchema: "clinica",
                        principalTable: "Consultas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recetas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalSchema: "clinica",
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recetas_Usuarios_MedicoUsuarioId",
                        column: x => x.MedicoUsuarioId,
                        principalSchema: "clinica",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesPedidosDetalles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecepcionPedidoId = table.Column<int>(type: "integer", nullable: false),
                    PedidoDetalleId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: false),
                    NumeroLote = table.Column<string>(type: "text", nullable: false),
                    FechaCaducidad = table.Column<DateOnly>(type: "date", nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "numeric", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesPedidosDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesPedidosDetalles_RecepcionesPedidos_RecepcionPedi~",
                        column: x => x.RecepcionPedidoId,
                        principalSchema: "clinica",
                        principalTable: "RecepcionesPedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecetasDetalles",
                schema: "clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecetaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: true),
                    Medicamento = table.Column<string>(type: "text", nullable: false),
                    Presentacion = table.Column<string>(type: "text", nullable: true),
                    Dosis = table.Column<string>(type: "text", nullable: false),
                    Frecuencia = table.Column<string>(type: "text", nullable: false),
                    Duracion = table.Column<string>(type: "text", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: true),
                    Indicaciones = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "integer", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioActualizacionId = table.Column<int>(type: "integer", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetasDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "clinica",
                        principalTable: "Productos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecetasDetalles_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalSchema: "clinica",
                        principalTable: "Recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_Fecha",
                schema: "clinica",
                table: "Cajas",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioAperturaId",
                schema: "clinica",
                table: "Cajas",
                column: "UsuarioAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioCierreId",
                schema: "clinica",
                table: "Cajas",
                column: "UsuarioCierreId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_ConsultaId",
                schema: "clinica",
                table: "Citas",
                column: "ConsultaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoUsuarioId",
                schema: "clinica",
                table: "Citas",
                column: "MedicoUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteId",
                schema: "clinica",
                table: "Citas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_MedicoUsuarioId",
                schema: "clinica",
                table: "Consultas",
                column: "MedicoUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PacienteId",
                schema: "clinica",
                table: "Consultas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteosCaja_CajaId",
                schema: "clinica",
                table: "ConteosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSri_ClaveAcceso",
                schema: "clinica",
                table: "DocumentosSri",
                column: "ClaveAcceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSriDetalles_DocumentoSriId",
                schema: "clinica",
                table: "DocumentosSriDetalles",
                column: "DocumentoSriId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriasClinicas_PacienteId",
                schema: "clinica",
                table: "HistoriasClinicas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_LotesProductos_ProductoId",
                schema: "clinica",
                table: "LotesProductos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaId",
                schema: "clinica",
                table: "MovimientosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_LoteProductoId",
                schema: "clinica",
                table: "MovimientosInventario",
                column: "LoteProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                schema: "clinica",
                table: "MovimientosInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_Identificacion",
                schema: "clinica",
                table: "Pacientes",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ProveedorId",
                schema: "clinica",
                table: "Pedidos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosDetalles_PedidoId",
                schema: "clinica",
                table: "PedidosDetalles",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosDetalles_ProductoId",
                schema: "clinica",
                table: "PedidosDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                schema: "clinica",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesPedidos_PedidoId",
                schema: "clinica",
                table: "RecepcionesPedidos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesPedidosDetalles_RecepcionPedidoId",
                schema: "clinica",
                table: "RecepcionesPedidosDetalles",
                column: "RecepcionPedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_ConsultaId",
                schema: "clinica",
                table: "Recetas",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_MedicoUsuarioId",
                schema: "clinica",
                table: "Recetas",
                column: "MedicoUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_PacienteId",
                schema: "clinica",
                table: "Recetas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasDetalles_ProductoId",
                schema: "clinica",
                table: "RecetasDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasDetalles_RecetaId",
                schema: "clinica",
                table: "RecetasDetalles",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_PermisoId",
                schema: "clinica",
                table: "RolesPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                schema: "clinica",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RolId",
                schema: "clinica",
                table: "UsuariosRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_ProductoId",
                schema: "clinica",
                table: "VentasDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_VentaId",
                schema: "clinica",
                table: "VentasDetalles",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Citas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "ConteosCaja",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "DocumentosSriDetalles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "HistoriasClinicas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "MovimientosCaja",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "PedidosDetalles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "RecepcionesPedidosDetalles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "RecetasDetalles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "RolesPermisos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "UsuariosRoles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "VentasDetalles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "DocumentosSri",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Cajas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "LotesProductos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "RecepcionesPedidos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Recetas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Permisos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Ventas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Productos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Pedidos",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Consultas",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Proveedores",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Pacientes",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "clinica");
        }
    }
}
