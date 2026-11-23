using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NexusWindowsForm;

public sealed class Form3 : Form
{
    private readonly string nombreCadete;
    private readonly Image[] imagenesHistoria;
    private readonly Panel panelDialogo = new();
    private readonly Label lblCapitulo = new();
    private readonly Label lblNarracion = new();
    private readonly Label lblIndicacion = new();
    private int paginaHistoria;

    private readonly record struct Escena(int Imagen, string Capitulo, string Texto);

    private Escena[] Escenas => new[]
    {
        new Escena(0, "NARRADOR // REGRESO A CASA",
            $"Aquí empieza la historia de un cadete llamado {nombreCadete}, el cual viajaba de regreso a casa."),
        new Escena(1, "NARRADOR // LLUVIA DE METEORITOS",
            "Durante su viaje a casa, el cadete se encontró con una lluvia de meteoritos que venían directo hacia él."),
        new Escena(2, "NARRADOR // IMPACTO EN EL CASCO",
            "Un meteorito golpeó la nave Nexus. Los controles fallaron y el cadete perdió el control de la nave."),
        new Escena(3, "NARRADOR // NÚCLEOS A LA DERIVA",
            "Con la sacudida, los núcleos que mantenían activo el motor dimensional se soltaron y salieron disparados al espacio."),
        new Escena(4, "NARRADOR // DOS REALIDADES",
            "Los cuatro núcleos se dispersaron: dos cayeron en una realidad y los otros dos en una realidad diferente."),
        new Escena(5, "CADETE // SEÑAL DE EMERGENCIA",
            "«¡Mayday, mayday! ¡Estoy perdiendo el control! La nave va a caer…»"),
        new Escena(6, "NARRADOR // DESPUÉS DEL IMPACTO",
            "Tras el aterrizaje forzoso, el cadete encontró la radio de emergencia e intentó contactar con su coronel."),
        new Escena(6, "COMUNICACIÓN // CORONEL PITA SILVA",
            $"CADETE {nombreCadete}: «Coronel Pita Silva, mi nave se estrelló. Perdí los cuatro núcleos y quedé varado».\r\n\r\n" +
            "CORONEL PITA SILVA: «Recibido, cadete. Te asigno una nueva misión: encuentra los cuatro núcleos, dos en cada realidad. Sin ellos no podrás volver a viajar por el espacio»."),
        // No se adjuntó una imagen 12; el cierre reutiliza la ilustración de los núcleos
        // viajando entre realidades para presentar la misión antes de iniciar el juego.
        new Escena(4, "NARRADOR // MISIÓN ASIGNADA",
            "La orden está registrada. Recupera los cuatro núcleos abandonados y repara la nave Nexus. La misión del cadete empieza ahora.")
    };

    public string RealidadSeleccionada { get; private set; } = "REALIDAD 102";

    public Form3(string cadete, string realidadActual, bool mostrarHistoria)
    {
        nombreCadete = string.IsNullOrWhiteSpace(cadete) ? "CADETE" : cadete.Trim();
        Text = "NEXUS | Historia de misión";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        MinimizeBox = false;
        KeyPreview = true;
        BackColor = Color.FromArgb(4, 7, 13);
        ForeColor = Color.FromArgb(238, 238, 224);
        Font = new Font("Consolas", 11, FontStyle.Bold);
        BackgroundImageLayout = ImageLayout.Stretch;
        KeyDown += Form3_KeyDown;
        Resize += (_, _) => AjustarLayout();
        Shown += (_, _) => AjustarLayout();
        FormClosed += (_, _) =>
        {
            BackgroundImage = null;
            foreach (Image imagen in imagenesHistoria) imagen.Dispose();
        };

        imagenesHistoria = Enumerable.Range(1, 7)
            .Select(i => CargarImagenHistoria($"HistoriaViaje{i:D2}.jpg"))
            .ToArray();

        panelDialogo.BackColor = Color.FromArgb(224, 5, 10, 20);
        panelDialogo.Paint += (_, e) =>
        {
            using Pen borde = new(Color.FromArgb(125, 197, 220), 2);
            e.Graphics.DrawRectangle(borde, 1, 1, panelDialogo.Width - 3, panelDialogo.Height - 3);
        };
        lblCapitulo.ForeColor = Color.FromArgb(117, 225, 245);
        lblCapitulo.Font = new Font("Consolas", 17, FontStyle.Bold);
        lblCapitulo.TextAlign = ContentAlignment.MiddleCenter;
        lblNarracion.ForeColor = Color.FromArgb(244, 240, 222);
        lblNarracion.Font = new Font("Consolas", 12, FontStyle.Regular);
        lblNarracion.TextAlign = ContentAlignment.MiddleCenter;
        lblIndicacion.ForeColor = Color.FromArgb(255, 206, 119);
        lblIndicacion.Font = new Font("Consolas", 10, FontStyle.Bold);
        lblIndicacion.TextAlign = ContentAlignment.MiddleCenter;

        var cancelar = new Button
        {
            Text = "VOLVER",
            BackColor = Color.FromArgb(24, 32, 43),
            ForeColor = Color.Gainsboro,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9, FontStyle.Bold),
            DialogResult = DialogResult.Cancel
        };
        cancelar.FlatAppearance.BorderColor = Color.FromArgb(105, 126, 139);
        cancelar.Click += (_, _) => Close();
        panelDialogo.Controls.AddRange(new Control[] { lblCapitulo, lblNarracion, lblIndicacion, cancelar });
        Controls.Add(panelDialogo);
        CancelButton = cancelar;

        paginaHistoria = mostrarHistoria ? 0 : Escenas.Length - 1;
        MostrarEscena();
        AjustarLayout();
    }

    private static Image CargarImagenHistoria(string nombre)
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "Resources", nombre);
        if (!File.Exists(ruta)) return new Bitmap(Properties.Resources.HistoriaNave);
        using Image original = Image.FromFile(ruta);
        return new Bitmap(original);
    }

    private void Form3_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Space) return;
        if (paginaHistoria == 0)
        {
            using (var secuenciaMeteoritos = new Form4())
                secuenciaMeteoritos.ShowDialog(this);

            // La escena siguiente es la lluvia de meteoritos; continúa tanto si
            // el cadete sobrevive como si la nave recibe un impacto.
            paginaHistoria++;
            MostrarEscena();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        if (paginaHistoria < Escenas.Length - 1)
        {
            paginaHistoria++;
            MostrarEscena();
        }
        else
        {
            RealidadSeleccionada = "REALIDAD 102";
            DialogResult = DialogResult.OK;
            Close();
        }
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void MostrarEscena()
    {
        Escena escena = Escenas[paginaHistoria];
        BackgroundImage = imagenesHistoria[escena.Imagen];
        lblCapitulo.Text = escena.Capitulo;
        lblNarracion.Text = escena.Texto;
        lblIndicacion.Text = paginaHistoria == Escenas.Length - 1
            ? "PULSA ESPACIO PARA INICIAR LA MISIÓN"
            : "PULSA ESPACIO PARA CONTINUAR";
        panelDialogo.Visible = true;
        panelDialogo.Controls.OfType<Button>().First().Visible = true;
        panelDialogo.BringToFront();
    }

    private void AjustarLayout()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
        int ancho = ClientSize.Width;
        int alto = ClientSize.Height;
        int margen = Math.Clamp(ancho / 40, 24, 48);
        int anchoDialogo = Math.Min(1320, ancho - margen * 2);
        int altoDialogo = Math.Min(204, Math.Max(166, alto / 4));
        panelDialogo.SetBounds((ancho - anchoDialogo) / 2, alto - altoDialogo - margen / 2, anchoDialogo, altoDialogo);
        lblCapitulo.SetBounds(20, 10, anchoDialogo - 40, 30);
        lblNarracion.SetBounds(32, 43, anchoDialogo - 64, altoDialogo - 91);
        lblIndicacion.SetBounds(145, altoDialogo - 43, anchoDialogo - 165, 30);
        panelDialogo.Controls.OfType<Button>().FirstOrDefault()?.SetBounds(16, altoDialogo - 45, 118, 30);
    }
}
