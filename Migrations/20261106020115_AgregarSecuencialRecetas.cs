using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAmelita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSecuencialRecetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "RecetasSecuencial",
                schema: "clinica");

            migrationBuilder.AddColumn<long>(
                name: "Secuencial",
                schema: "clinica",
                table: "Recetas",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE clinica."Recetas" SET "Secuencial" = "Id";

                SELECT setval(
                    'clinica."RecetasSecuencial"'::regclass,
                    COALESCE((SELECT MAX("Secuencial") FROM clinica."Recetas"), 0) + 1,
                    false
                );
                """);

            migrationBuilder.AlterColumn<long>(
                name: "Secuencial",
                schema: "clinica",
                table: "Recetas",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('clinica.\"RecetasSecuencial\"'::regclass)",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_Secuencial",
                schema: "clinica",
                table: "Recetas",
                column: "Secuencial",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recetas_Secuencial",
                schema: "clinica",
                table: "Recetas");

            migrationBuilder.DropColumn(
                name: "Secuencial",
                schema: "clinica",
                table: "Recetas");

            migrationBuilder.DropSequence(
                name: "RecetasSecuencial",
                schema: "clinica");
        }
    }
}
