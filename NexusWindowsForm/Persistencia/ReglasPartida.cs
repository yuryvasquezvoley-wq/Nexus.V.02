namespace NexusWindowsForm.Persistencia;

public readonly record struct ResultadoRecuperacion(bool Aplicada, bool YaSeUso, int Energia, int Conexion);

public static class ReglasPartida
{
    public static ResultadoRecuperacion RecuperarEnergia(int energia, int conexion, bool yaSeUso)
    {
        if (energia is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(energia), "La energía debe estar entre 0 y 100.");
        if (conexion is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(conexion), "La conexión debe estar entre 0 y 100.");
        if (yaSeUso) return new(false, true, energia, conexion);

        int energiaNueva = Math.Min(100, energia + 20);
        int conexionNueva = Math.Min(100, conexion + 10);
        if (energiaNueva == energia && conexionNueva == conexion)
            return new(false, false, energia, conexion);

        return new(true, false, energiaNueva, conexionNueva);
    }
}
