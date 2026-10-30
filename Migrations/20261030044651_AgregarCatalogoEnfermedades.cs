using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAmelita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogoEnfermedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS clinica;
                CREATE TABLE IF NOT EXISTS clinica."Enfermedades" (
                    "Codigo" varchar(10) NOT NULL,
                    "Nombre" varchar(1000) NOT NULL,
                    "CategoriaCodigo" varchar(3) NOT NULL,
                    "Categoria" varchar(1000) NOT NULL,
                    CONSTRAINT "PK_Enfermedades" PRIMARY KEY ("Codigo")
                );
                CREATE INDEX IF NOT EXISTS "IX_Enfermedades_CategoriaCodigo"
                    ON clinica."Enfermedades" ("CategoriaCodigo");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enfermedades",
                schema: "clinica");
        }
    }
}
