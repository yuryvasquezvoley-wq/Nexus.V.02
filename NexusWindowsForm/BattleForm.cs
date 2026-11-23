namespace NexusWindowsForm;

public static class BattleDialog
{
    // Retry means attack; Abort means escape.
    public static DialogResult Show(int playerHealth, int monsterHealth, bool zombie = false)
    {
        using var dialog = new Form
        {
            Text = "NEXUS | Encuentro",
            ClientSize = new Size(900, 650),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(8, 12, 18),
            ForeColor = Color.FromArgb(237, 230, 198),
            Font = new Font("Consolas", 12, FontStyle.Bold)
        };

        var monsterImage = new PictureBox
        {
            Image = zombie ? LoadZombieImage(monsterHealth) : Properties.Resources.Demogorgon,
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(18, 18),
            Size = new Size(864, 430),
            BackColor = Color.Black
        };
        dialog.Controls.Add(monsterImage);

        // Efecto visual breve del rasguño cuando el cadete pulsa ATACAR.
        var scratch = new Label
        {
            Text = "╱╲╱",
            Location = new Point(270, 145),
            Size = new Size(360, 110),
            ForeColor = Color.FromArgb(245, 55, 55),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 58, FontStyle.Bold),
            Visible = false
        };
        monsterImage.Controls.Add(scratch);

        dialog.Controls.Add(new Label
        {
            Text = zombie ? "¡ZOMBIE DE LA REALIDAD 102!" : "¡UNA CRIATURA DEL OTRO LADO!",
            Location = new Point(30, 462),
            Size = new Size(840, 34),
            ForeColor = Color.FromArgb(226, 63, 70),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 17, FontStyle.Bold)
        });

        dialog.Controls.Add(new Label
        {
            Text = $"CADETE   {HealthBar(playerHealth, 100)}  {playerHealth}/100     {(zombie ? "ZOMBIE" : "DEMOGORGON")}   {HealthBar(monsterHealth, 30)}  {monsterHealth}/30",
            Location = new Point(32, 507),
            Size = new Size(836, 36),
            ForeColor = Color.FromArgb(164, 220, 156),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 11, FontStyle.Bold)
        });

        var attack = new Button
        {
            Text = "ATACAR",
            Location = new Point(235, 566),
            Size = new Size(190, 52),
            BackColor = Color.FromArgb(119, 36, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        attack.FlatAppearance.BorderColor = Color.FromArgb(222, 74, 70);
        attack.Click += (_, _) =>
        {
            attack.Enabled = false;
            scratch.Visible = true;
            scratch.BringToFront();

            var animation = new System.Windows.Forms.Timer { Interval = 450 };
            animation.Tick += (_, _) =>
            {
                animation.Stop();
                animation.Dispose();
                dialog.DialogResult = DialogResult.Retry;
            };
            animation.Start();
        };
        dialog.Controls.Add(attack);

        var flee = new Button
        {
            Text = "ESCAPAR",
            DialogResult = DialogResult.Abort,
            Location = new Point(475, 566),
            Size = new Size(190, 52),
            BackColor = Color.FromArgb(34, 53, 61),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        flee.FlatAppearance.BorderColor = Color.FromArgb(112, 155, 153);
        dialog.Controls.Add(flee);
        dialog.AcceptButton = attack;
        dialog.CancelButton = flee;
        return dialog.ShowDialog();
    }

    private static Image LoadZombieImage(int monsterHealth)
    {
        string nombreArchivo = monsterHealth * 100 < 30 * 60
            ? "Zombie102_Transformado.png"
            : "Zombie102_Normal.png";

        string ruta = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            nombreArchivo
        );

        // Si todavía no se copiaron las imágenes al proyecto, evita que el juego falle.
        return File.Exists(ruta)
            ? Image.FromFile(ruta)
            : Properties.Resources.Demogorgon;
    }

    private static string HealthBar(int health, int max)
    {
        int filled = Math.Clamp((health * 10 + max - 1) / max, 0, 10);
        return $"[{new string('█', filled)}{new string('░', 10 - filled)}]";
    }
}
