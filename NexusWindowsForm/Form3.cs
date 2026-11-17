namespace NexusWindowsForm;

public sealed class Form3 : Form
{
    public string RealidadSeleccionada { get; private set; } = "REALIDAD 102";

    public Form3(string realidadActual = "REALIDAD 102")
    {
        RealidadSeleccionada = realidadActual;
        Text = "NEXUS | Selección de realidades";
        ClientSize = new Size(1100, 650);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(8, 11, 17);
        ForeColor = Color.FromArgb(236, 232, 210);
        Font = new Font("Consolas", 11, FontStyle.Bold);

        Controls.Add(new Label
        {
            Text = "ELIGE TU REALIDAD",
            Font = new Font("Consolas", 25, FontStyle.Bold),
            ForeColor = Color.FromArgb(225, 54, 67),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(30, 18),
            Size = new Size(1040, 46)
        });
        Controls.Add(new Label
        {
            Text = "Selecciona el destino del cadete. Puedes cruzar entre dimensiones durante la misión.",
            Font = new Font("Consolas", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(176, 184, 183),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(45, 68),
            Size = new Size(1010, 30)
        });

        Controls.Add(CrearTarjeta(
            "REALIDAD 102",
            "HAWKINS // PUEBLO EN RUINAS",
            "Calles, casas y zonas verdes. La anomalía ya alcanzó el bosque.",
            Properties.Resources.MapaHawkins,
            Color.FromArgb(123, 165, 105)));
        Controls.Add(CrearTarjeta(
            "REALIDAD 999",
            "EL REVÉS // DIMENSIÓN CORROMPIDA",
            "Una versión oscura del pueblo, invadida por energía y criaturas.",
            Properties.Resources.realidad_desconocida,
            Color.FromArgb(202, 54, 89)));

        var volver = new Button
        {
            Text = "VOLVER AL REGISTRO",
            Location = new Point(420, 604),
            Size = new Size(260, 34),
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(27, 32, 41),
            ForeColor = Color.FromArgb(218, 221, 216),
            Font = new Font("Consolas", 9, FontStyle.Bold)
        };
        volver.FlatAppearance.BorderColor = Color.FromArgb(91, 104, 115);
        Controls.Add(volver);
        CancelButton = volver;
    }

    private Panel CrearTarjeta(string id, string titulo, string descripcion, Image imagen, Color acento)
    {
        var tarjeta = new Panel
        {
            Location = id.EndsWith("102", StringComparison.Ordinal) ? new Point(32, 122) : new Point(568, 122),
            Size = new Size(500, 466),
            BackColor = Color.FromArgb(18, 24, 30)
        };
        tarjeta.Paint += (_, e) =>
        {
            using Pen borde = new(acento, 2);
            e.Graphics.DrawRectangle(borde, 1, 1, tarjeta.Width - 3, tarjeta.Height - 3);
            using SolidBrush brillo = new(Color.FromArgb(35, acento));
            e.Graphics.FillRectangle(brillo, 2, 2, tarjeta.Width - 4, 4);
        };

        var preview = new PictureBox
        {
            Location = new Point(14, 14),
            Size = new Size(472, 248),
            Image = imagen,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(5, 8, 12)
        };
        tarjeta.Controls.Add(preview);
        tarjeta.Controls.Add(new Label
        {
            Text = titulo,
            Location = new Point(18, 278),
            Size = new Size(464, 34),
            ForeColor = acento,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 14, FontStyle.Bold)
        });
        tarjeta.Controls.Add(new Label
        {
            Text = descripcion,
            Location = new Point(24, 317),
            Size = new Size(452, 48),
            ForeColor = Color.FromArgb(203, 207, 195),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 9, FontStyle.Regular)
        });

        var entrar = new Button
        {
            Text = "ENTRAR A " + id,
            Location = new Point(42, 386),
            Size = new Size(416, 54),
            BackColor = Color.FromArgb(34, 42, 47),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 12, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        entrar.FlatAppearance.BorderColor = acento;
        entrar.FlatAppearance.BorderSize = 2;
        entrar.Click += (_, _) =>
        {
            RealidadSeleccionada = id;
            DialogResult = DialogResult.OK;
            Close();
        };
        tarjeta.Controls.Add(entrar);
        return tarjeta;
    }
}
