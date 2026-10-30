namespace ClinicaAmelita.Api.Entities;

public class Enfermedad
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaCodigo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}
