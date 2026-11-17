using System.Drawing;
using System.Windows.Forms;

namespace NexusWindowsForm;

public sealed class Form3 : Form
{
    private readonly string nombreCadete;
    private readonly bool narracionActiva;
    private readonly Panel panelDialogo = new();
    private readonly Panel panelSelector = new();
    private readonly Label lblCapitulo = new();
    private readonly Label lblNarracion = new();
    private readonly Label lblIndicacion = new();
    private int paginaHistoria;
    private bool selectorVisible;

    public string RealidadSeleccionada { get; private set; }

    public Form3(string cadete, string realidadActual, bool mostrarHistoria)
    {
        nombreCadete = string.IsNullOrWhiteSpace(cadete) ? "CADETE" : cadete.Trim();
        narracionActiva = mostrarHistoria;
        RealidadSeleccionada = realidadActual;
        Text = "NEXUS | Archivo de misión";
        ClientSize = new Size(1280, 720);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        KeyPreview = true;
        BackColor = Color.FromArgb(4, 7, 13);
        ForeColor = Color.FromArgb(238, 238, 224);
        Font = new Font("Consolas", 11, FontStyle.Bold);
        BackgroundImageLayout = ImageLayout.Zoom;
        KeyDown += Form3_KeyDown;

        panelDialogo.BackColor = Color.FromArgb(222, 5, 10, 20);
        panelDialogo.Location = new Point(28, 525);
        panelDialogo.Size = new Size(1224, 170);
        panelDialogo.Paint += (_, e) =>
        {
            using Pen borde = new(Color.FromArgb(125, 197, 220), 2);
            e.Graphics.DrawRectangle(borde, 1, 1, panelDialogo.Width - 3, panelDialogo.Height - 3);
        };
        lblCapitulo.Location = new Point(20, 12);
        lblCapitulo.Size = new Size(1180, 30);
        lblCapitulo.ForeColor = Color.FromArgb(117, 225, 245);
        lblCapitulo.Font = new Font("Consolas", 17, FontStyle.Bold);
        lblCapitulo.TextAlign = ContentAlignment.MiddleCenter;
        lblNarracion.Location = new Point(35, 48);
        lblNarracion.Size = new Size(1150, 66);
        lblNarracion.ForeColor = Color.FromArgb(244, 240, 222);
        lblNarracion.Font = new Font("Consolas", 12, FontStyle.Regular);
        lblNarracion.TextAlign = ContentAlignment.MiddleCenter;
        lblIndicacion.Location = new Point(150, 120);
        lblIndicacion.Size = new Size(1050, 34);
        lblIndicacion.ForeColor = Color.FromArgb(255, 206, 119);
        lblIndicacion.Font = new Font("Consolas", 10, FontStyle.Bold);
        lblIndicacion.TextAlign = ContentAlignment.MiddleCenter;
        var cancelar = new Button
        {
            Text = "VOLVER",
            Location = new Point(18, 121),
            Size = new Size(118, 30),
            BackColor = Color.FromArgb(24, 32, 43),
            ForeColor = Color.Gainsboro,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9, FontStyle.Bold),
            DialogResult = DialogResult.Cancel
        };
        cancelar.FlatAppearance.BorderColor = Color.FromArgb(105, 126, 139);
        cancelar.Click += (_, _) => Close();
        panelDialogo.Controls.AddRange(new Control[] { lblCapitulo, lblNarracion, lblIndicacion, cancelar });

        ConfigurarSelector();
        Controls.Add(panelDialogo);
        Controls.Add(panelSelector);
        CancelButton = cancelar;

        if (mostrarHistoria)
        {
            paginaHistoria = 0;
            MostrarEscena();
        }
        else
        {
            paginaHistoria = 2;
            selectorVisible = true;
            MostrarEscena();
        }
    }

    private void Form3_KeyDown(object? sender, KeyEventArgs e)
    {
        if (selectorVisible || e.KeyCode != Keys.Space) return;
        if (paginaHistoria == 0)
            paginaHistoria = 1;
        else
        {
            paginaHistoria = 2;
            selectorVisible = true;
        }
        MostrarEscena();
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void MostrarEscena()
    {
        BackgroundImage = paginaHistoria switch
        {
            0 => Properties.Resources.HistoriaNave,
            1 => Properties.Resources.HistoriaReactor,
            _ => Properties.Resources.EncrucijadaNexus
        };

        (lblCapitulo.Text, lblNarracion.Text) = paginaHistoria switch
        {
            0 => ("NARRADOR // ASIGNACIÓN DE MISIÓN",
                $"Cadete {nombreCadete}: la comandante Vega te ha asignado una misión. La nave Nexus quedó varada tras un salto entre realidades."),
            1 => ("NARRADOR // EL REACTOR",
                "El núcleo principal y sus tres reservas están destruidos. Recupera los 3 núcleos abandonados: dos en la realidad 102 y uno en la 999."),
            _ => ("NARRADOR // ENCRUCIJADA DEL NEXUS",
                $"Y aquí comienza la misión del cadete {nombreCadete}. Elige una realidad, reúne los núcleos y repara la nave para volver a casa.")
        };
        lblIndicacion.Text = selectorVisible
            ? "ELIGE UNA REALIDAD PARA INICIAR LA MISIÓN"
            : "PULSA ESPACIO PARA CONTINUAR";
        panelDialogo.Visible = narracionActiva;
        panelSelector.Visible = selectorVisible;
        panelDialogo.Controls.OfType<Button>().First().Visible = !selectorVisible;
        if (selectorVisible) CancelButton = panelSelector.Controls.OfType<Button>().Last();
        else CancelButton = panelDialogo.Controls.OfType<Button>().First();
    }

    private void ConfigurarSelector()
    {
        panelSelector.Location = new Point(175, 38);
        panelSelector.Size = new Size(930, 445);
        panelSelector.BackColor = Color.FromArgb(232, 5, 9, 18);
        panelSelector.Visible = false;
        panelSelector.Paint += (_, e) =>
        {
            using Pen borde = new(Color.FromArgb(90, 199, 229), 2);
            e.Graphics.DrawRectangle(borde, 1, 1, panelSelector.Width - 3, panelSelector.Height - 3);
        };
        panelSelector.Controls.Add(new Label
        {
            Text = "ELIGE EL PORTAL QUE CRUZARÁ EL CADETE",
            Location = new Point(20, 8),
            Size = new Size(890, 34),
            ForeColor = Color.FromArgb(238, 238, 224),
            Font = new Font("Consolas", 18, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        });
        panelSelector.Controls.Add(CrearTarjeta(
            "REALIDAD 102", "HAWKINS // MUNDO INFECTADO",
            "Dos núcleos abandonados. Las raíces del Otro Lado invadieron las calles.",
            Properties.Resources.HawkinsPortalPreview, Color.FromArgb(243, 69, 77), new Point(14, 48)));
        panelSelector.Controls.Add(CrearTarjeta(
            "REALIDAD 999", "HAWKINS // REALIDAD VERDE",
            "Un núcleo abandonado aguarda entre el bosque y las ruinas.",
            Properties.Resources.Realidad999Verde, Color.FromArgb(69, 239, 111), new Point(476, 48)));

        var cancelar = new Button
        {
            Text = "VOLVER AL REGISTRO",
            Location = new Point(355, 408),
            Size = new Size(220, 30),
            BackColor = Color.FromArgb(24, 32, 43),
            ForeColor = Color.Gainsboro,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9, FontStyle.Bold),
            DialogResult = DialogResult.Cancel
        };
        cancelar.FlatAppearance.BorderColor = Color.FromArgb(105, 126, 139);
        cancelar.Click += (_, _) => Close();
        panelSelector.Controls.Add(cancelar);
    }

    private Panel CrearTarjeta(string realidad, string titulo, string descripcion, Image imagen, Color acento, Point ubicacion)
    {
        var tarjeta = new Panel
        {
            Location = ubicacion,
            Size = new Size(440, 350),
            BackColor = Color.FromArgb(237, 13, 18, 27)
        };
        tarjeta.Paint += (_, e) =>
        {
            using Pen borde = new(acento, 2);
            e.Graphics.DrawRectangle(borde, 1, 1, tarjeta.Width - 3, tarjeta.Height - 3);
        };
        tarjeta.Controls.Add(new PictureBox
        {
            Location = new Point(12, 12),
            Size = new Size(416, 214),
            Image = imagen,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(3, 5, 9)
        });
        tarjeta.Controls.Add(new Label
        {
            Text = titulo,
            Location = new Point(14, 232),
            Size = new Size(412, 28),
            ForeColor = acento,
            Font = new Font("Consolas", 13, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        });
        tarjeta.Controls.Add(new Label
        {
            Text = descripcion,
            Location = new Point(18, 262),
            Size = new Size(404, 44),
            ForeColor = Color.FromArgb(211, 216, 208),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleCenter
        });
        var viajar = new Button
        {
            Text = "VIAJAR A " + realidad,
            Location = new Point(20, 312),
            Size = new Size(400, 34),
            BackColor = Color.FromArgb(28, 34, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 11, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        viajar.FlatAppearance.BorderColor = acento;
        viajar.FlatAppearance.BorderSize = 2;
        viajar.Click += (_, _) =>
        {
            RealidadSeleccionada = realidad;
            DialogResult = DialogResult.OK;
            Close();
        };
        tarjeta.Controls.Add(viajar);
        return tarjeta;
    }
}
