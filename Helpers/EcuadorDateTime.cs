namespace ClinicaAmelita.Api.Helpers;

public static class EcuadorDateTime
{
    private static readonly TimeSpan EcuadorOffset = TimeSpan.FromHours(-5);

    public static DateOnly Hoy()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.Add(EcuadorOffset));
    }

    public static (DateTime DesdeUtc, DateTime HastaUtc) RangoUtc(DateOnly fecha)
    {
        var inicioLocal = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var inicioUtc = new DateTimeOffset(inicioLocal, EcuadorOffset).UtcDateTime;

        return (inicioUtc, inicioUtc.AddDays(1));
    }
}
