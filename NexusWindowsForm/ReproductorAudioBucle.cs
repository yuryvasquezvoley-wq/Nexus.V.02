using System.IO;
using MediaPlayer = System.Windows.Media.MediaPlayer;

namespace NexusWindowsForm;

internal sealed class ReproductorAudioBucle : IDisposable
{
    private readonly MediaPlayer reproductor;
    private readonly Uri archivo;
    private bool activo;
    private bool abierto;
    private bool desechado;

    public ReproductorAudioBucle(string ruta, double volumen)
    {
        archivo = new Uri(Path.GetFullPath(ruta), UriKind.Absolute);
        reproductor = new MediaPlayer { Volume = Math.Clamp(volumen, 0, 1) };
        reproductor.MediaOpened += (_, _) =>
        {
            abierto = true;
            if (activo && !desechado) reproductor.Play();
        };
        reproductor.MediaEnded += (_, _) =>
        {
            if (!activo || desechado) return;
            reproductor.Position = TimeSpan.Zero;
            reproductor.Play();
        };
        reproductor.MediaFailed += (_, _) => activo = false;
    }

    public void Iniciar()
    {
        if (desechado) return;
        activo = true;
        if (abierto)
        {
            reproductor.Position = TimeSpan.Zero;
            reproductor.Play();
        }
        else
        {
            reproductor.Open(archivo);
        }
    }

    public void Detener()
    {
        activo = false;
        if (abierto && !desechado) reproductor.Stop();
    }

    public void Dispose()
    {
        if (desechado) return;
        activo = false;
        desechado = true;
        reproductor.Stop();
        reproductor.Close();
    }
}
