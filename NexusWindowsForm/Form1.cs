using System;
using System.Drawing;
using System.Windows.Forms;

namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        private readonly Color background = Color.FromArgb(10, 15, 22);
        private readonly Color surface = Color.FromArgb(20, 29, 40);
        private readonly Color surfaceLight = Color.FromArgb(31, 43, 57);
        private readonly Color accent = Color.FromArgb(220, 55, 65);
        private readonly Color textMain = Color.FromArgb(235, 240, 245);
        private readonly Color textMuted = Color.FromArgb(150, 164, 178);

        private Panel sidebar = null!;
        private Panel content = null!;
        private Panel topbar = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Label explorerLabel = null!;
        private ProgressBar lifeBar = null!;
        private ProgressBar energyBar = null!;
        private ProgressBar connectionBar = null!;
        private Label lifeValue = null!;
        private Label energyValue = null!;
        private Label connectionValue = null!;
        private NexusGameEngine? game;
        private Button[,] mapButtons = new Button[10, 10];

        private TextBox nameInput = null!;
        private NumericUpDown ageInput = null!;
        private NumericUpDown energyInput = null!;
        private NumericUpDown connectionInput = null!;

        public Form1()
        {
            InitializeComponent();
            BuildInterface();
            ShowRegistration();
        }

        private void BuildInterface()
        {
            Text = "NEXUS V2.0 - Dimensional System";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 720);
            Size = new Size(1360, 850);
            BackColor = background;
            ForeColor = textMain;
            Font = new Font("Segoe UI", 10F);
            KeyPreview = true;
            KeyDown += Form1_KeyDown;

            topbar = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.FromArgb(15, 22, 31), Padding = new Padding(22, 0, 22, 0) };
            var brand = new Label { Text = "N E X U S", AutoSize = true, Font = new Font("Segoe UI", 19F, FontStyle.Bold), ForeColor = accent, Location = new Point(22, 14) };
            var version = new Label { Text = "SYSTEM V2.0  /  DIMENSIONAL EXPLORATION", AutoSize = true, ForeColor = textMuted, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Location = new Point(190, 25) };
            var online = new Label { Text = "● SISTEMA LISTO", AutoSize = true, ForeColor = Color.FromArgb(72, 201, 132), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            topbar.Controls.Add(brand);
            topbar.Controls.Add(version);
            topbar.Controls.Add(online);
            topbar.Resize += (_, _) => online.Location = new Point(topbar.ClientSize.Width - online.Width - 24, 25);

            sidebar = new Panel { Dock = DockStyle.Left, Width = 225, BackColor = Color.FromArgb(14, 20, 29), Padding = new Padding(14, 18, 14, 14) };
            var navTitle = new Label { Text = "CENTRO DE MANDO", AutoSize = true, ForeColor = textMuted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Location = new Point(18, 20) };
            sidebar.Controls.Add(navTitle);
            int y = 58;
            AddNavButton("⌂   PANEL PRINCIPAL", y, () => ShowDashboard(), true); y += 50;
            AddNavButton("◎   EXPLORAR REALIDAD", y, () => ShowExploration()); y += 50;
            AddNavButton("⌁   ANALIZAR ANOMALÍA", y, () => RunAction(game?.AnalyzeAnomaly(), "Análisis")); y += 50;
            AddNavButton("＋   RECUPERAR ENERGÍA", y, () => RunAction(game?.RecoverEnergy(), "Recuperación")); y += 50;
            AddNavButton("▤   INVENTARIO", y, () => ShowInventory()); y += 50;
            AddNavButton("◉   PERFIL", y, () => ShowProfile()); y += 50;
            var divider = new Panel { BackColor = Color.FromArgb(49, 62, 77), Height = 1, Width = 190, Location = new Point(16, y + 10) };
            sidebar.Controls.Add(divider);
            AddNavButton("⏻   DESCONECTAR", y + 28, Disconnect, false, true);

            content = new Panel { Dock = DockStyle.Fill, BackColor = background, Padding = new Padding(28) };
            Controls.Add(content);
            Controls.Add(sidebar);
            Controls.Add(topbar);
        }

        private void AddNavButton(string caption, int y, Action action, bool selected = false, bool danger = false)
        {
            var button = new Button
            {
                Text = caption, Location = new Point(10, y), Size = new Size(195, 42),
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
                BackColor = selected ? Color.FromArgb(58, 30, 38) : Color.Transparent,
                ForeColor = danger ? Color.FromArgb(239, 105, 105) : textMain,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = surfaceLight;
            button.Click += (_, _) => action();
            sidebar.Controls.Add(button);
        }

        private void ShowRegistration()
        {
            content.Controls.Clear();
            var card = CreateCard(650, 570);
            card.Anchor = AnchorStyles.None;
            card.Location = new Point(Math.Max(20, (content.ClientSize.Width - card.Width) / 2), Math.Max(20, (content.ClientSize.Height - card.Height) / 2));
            content.Resize += (_, _) => card.Location = new Point(Math.Max(20, (content.ClientSize.Width - card.Width) / 2), Math.Max(20, (content.ClientSize.Height - card.Height) / 2));

            var heading = MakeLabel("REGISTRO DE EXPLORADOR", 24, true, accent);
            heading.Location = new Point(36, 28);
            var description = MakeLabel("Identifícate antes de ingresar al sistema dimensional.", 10, false, textMuted);
            description.Location = new Point(38, 70);
            card.Controls.Add(heading);
            card.Controls.Add(description);

            int y = 125;
            AddField(card, "NOMBRE DEL CADETE", ref nameInput, y, "Escribe tu nombre"); y += 92;
            AddNumericField(card, "EDAD", ref ageInput, y, 18, 10, 99); y += 92;
            AddNumericField(card, "ENERGÍA INICIAL", ref energyInput, y, 100, 1, 100); y += 92;
            AddNumericField(card, "NIVEL DE CONEXIÓN", ref connectionInput, y, 100, 1, 100);

            var start = MakeButton("INICIAR NEXUS  →", accent, Color.White, 220, 48);
            start.Location = new Point(36, 500);
            start.Click += (_, _) => RegisterExplorer();
            card.Controls.Add(start);
            content.Controls.Add(card);
            titleLabel = heading;
        }

        private void AddField(Panel parent, string caption, ref TextBox box, int y, string placeholder)
        {
            var label = MakeLabel(caption, 8.5F, true, textMuted);
            label.Location = new Point(38, y);
            box = new TextBox { Location = new Point(38, y + 27), Width = 565, Height = 34, Text = "", BackColor = surfaceLight, ForeColor = textMain, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11F) };
            box.PlaceholderText = placeholder;
            parent.Controls.Add(label);
            parent.Controls.Add(box);
        }

        private void AddNumericField(Panel parent, string caption, ref NumericUpDown input, int y, int value, int min, int max)
        {
            var label = MakeLabel(caption, 8.5F, true, textMuted);
            label.Location = new Point(38, y);
            input = new NumericUpDown { Location = new Point(38, y + 27), Width = 565, Height = 34, Minimum = min, Maximum = max, Value = value, BackColor = surfaceLight, ForeColor = textMain, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11F) };
            parent.Controls.Add(label);
            parent.Controls.Add(input);
        }

        private void RegisterExplorer()
        {
            string name = nameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Ingresa el nombre del explorador.", "Registro incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameInput.Focus();
                return;
            }

            var explorer = new Explorer(name, (int)ageInput.Value, (int)energyInput.Value, (int)connectionInput.Value);
            game = new NexusGameEngine(explorer);
            UpdateStatus();
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            if (game == null) { ShowRegistration(); return; }
            content.Controls.Clear();
            AddPageHeading("PANEL PRINCIPAL", "Resumen del explorador y acceso rápido a las operaciones de NEXUS.");
            AddStatusCards(105);

            var mission = CreateCard(0, 170);
            mission.Location = new Point(0, 235);
            mission.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mission.Width = Math.Max(500, content.ClientSize.Width - 56);
            var missionTitle = MakeLabel("MISIÓN ACTUAL", 9, true, accent);
            missionTitle.Location = new Point(24, 20);
            var missionName = MakeLabel(game.MissionActive ? "LA ANOMALÍA PERDIDA" : "SIN MISIÓN ACTIVA", 20, true, textMain);
            missionName.Location = new Point(24, 53);
            var missionText = MakeLabel(game.MissionActive ? "Continúa la exploración y registra el núcleo energético." : "Selecciona una realidad para comenzar una nueva exploración.", 10, false, textMuted);
            missionText.Location = new Point(24, 94);
            mission.Controls.Add(missionTitle);
            mission.Controls.Add(missionName);
            mission.Controls.Add(missionText);
            var explore = MakeButton("SELECCIONAR REALIDAD", accent, Color.White, 230, 44);
            explore.Location = new Point(24, 122);
            explore.Click += (_, _) => SelectDimension();
            mission.Controls.Add(explore);
            content.Controls.Add(mission);

            var quick = MakeLabel("ACCESO RÁPIDO", 9, true, textMuted);
            quick.Location = new Point(0, 430);
            content.Controls.Add(quick);
            AddQuickButton("ANALIZAR ANOMALÍA", 0, 465, () => RunAction(game.AnalyzeAnomaly(), "Análisis"));
            AddQuickButton("RECUPERAR ENERGÍA", 250, 465, () => RunAction(game.RecoverEnergy(), "Recuperación"));
            AddQuickButton("VER INVENTARIO", 500, 465, ShowInventory);
            UpdateStatus();
        }

        private void AddStatusCards(int y)
        {
            if (game == null) return;
            var labels = new[] { "VIDA", "ENERGÍA", "CONEXIÓN" };
            var values = new[] { game.Explorer.Life, game.Explorer.Energy, game.Explorer.Connection };
            for (int i = 0; i < 3; i++)
            {
                var card = CreateCard(0, 105);
                card.Location = new Point(i * 245, y);
                card.Size = new Size(225, 105);
                var label = MakeLabel(labels[i], 8.5F, true, textMuted);
                label.Location = new Point(17, 14);
                var value = MakeLabel(values[i] + " / 100", 21, true, i == 0 ? Color.FromArgb(239, 100, 100) : textMain);
                value.Location = new Point(17, 38);
                var bar = new ProgressBar { Location = new Point(17, 76), Size = new Size(188, 9), Maximum = 100, Value = values[i], Style = ProgressBarStyle.Continuous };
                card.Controls.Add(label); card.Controls.Add(value); card.Controls.Add(bar);
                content.Controls.Add(card);
            }
        }

        private void AddQuickButton(string caption, int x, int y, Action action)
        {
            var button = MakeButton(caption, surfaceLight, textMain, 225, 48);
            button.Location = new Point(x, y);
            button.Click += (_, _) => action();
            content.Controls.Add(button);
        }

        private void SelectDimension()
        {
            if (game == null) return;
            var choice = MessageBox.Show("¿Ingresar a REALIDAD 102?\n\nSí: REALIDAD 102\nNo: REALIDAD 999\nCancelar: regresar", "Selección de realidad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (choice == DialogResult.Cancel) return;
            RunAction(game.StartMission(choice == DialogResult.Yes ? 0 : 1), "Exploración");
            if (game.MissionActive) ShowExploration();
        }

        private void ShowExploration()
        {
            if (game == null) { ShowRegistration(); return; }
            if (!game.MissionActive)
            {
                SelectDimension();
                return;
            }

            content.Controls.Clear();
            AddPageHeading(game.CurrentDimension!.Name, "Explora el mapa con W, A, S, D o con los controles direccionales.");
            var mapCard = CreateCard(730, 650);
            mapCard.Location = new Point(0, 88);
            var mapTitle = MakeLabel("MAPA DIMENSIONAL 10 × 10", 10, true, accent);
            mapTitle.Location = new Point(20, 15);
            mapCard.Controls.Add(mapTitle);

            var grid = new TableLayoutPanel { Location = new Point(20, 52), Size = new Size(540, 540), RowCount = 10, ColumnCount = 10, BackColor = background, Padding = new Padding(2) };
            for (int i = 0; i < 10; i++) { grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10)); grid.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); }
            mapCard.Controls.Add(grid);
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    var cell = new Button { Dock = DockStyle.Fill, Margin = new Padding(2), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Tag = new Point(col, row), Cursor = Cursors.Hand };
                    cell.FlatAppearance.BorderSize = 0;
                    cell.Click += (_, _) =>
                    {
                        var p = (Point)((Button)_.Sender!).Tag!;
                        int dx = Math.Sign(p.X - game.X);
                        int dy = Math.Sign(p.Y - game.Y);
                        if (Math.Abs(p.X - game.X) + Math.Abs(p.Y - game.Y) == 1) ProcessNotice(game.Move(dx, dy));
                        RefreshMap();
                    };
                    mapButtons[col, row] = cell;
                    grid.Controls.Add(cell, col, row);
                }
            }

            var legend = CreateCard(260, 220);
            legend.Location = new Point(755, 88);
            var legendTitle = MakeLabel("LEYENDA", 10, true, accent);
            legendTitle.Location = new Point(20, 18);
            legend.Controls.Add(legendTitle);
            string[] legendLines = { "C   Cadete", "E   Extracción", "*   Célula de energía", "+   Anomalía / entidad", "#   Zona peligrosa", "R   Zona de anomalía", "O   Núcleo energético" };
            for (int i = 0; i < legendLines.Length; i++)
            {
                var line = MakeLabel(legendLines[i], 9, false, textMain);
                line.Location = new Point(22, 52 + i * 22);
                legend.Controls.Add(line);
            }
            var controlsCard = CreateCard(260, 220);
            controlsCard.Location = new Point(755, 325);
            var controlsTitle = MakeLabel("CONTROLES", 10, true, accent);
            controlsTitle.Location = new Point(20, 18);
            controlsCard.Controls.Add(controlsTitle);
            AddMoveButton(controlsCard, "↑", 104, 55, 46, 40, 0, -1);
            AddMoveButton(controlsCard, "←", 55, 101, 46, 40, -1, 0);
            AddMoveButton(controlsCard, "↓", 104, 101, 46, 40, 0, 1);
            AddMoveButton(controlsCard, "→", 153, 101, 46, 40, 1, 0);
            var exit = MakeButton("SALIR DE EXPLORACIÓN", surfaceLight, textMain, 215, 38);
            exit.Location = new Point(20, 164);
            exit.Click += (_, _) => ShowDashboard();
            controlsCard.Controls.Add(exit);
            content.Controls.Add(mapCard);
            content.Controls.Add(legend);
            content.Controls.Add(controlsCard);
            RefreshMap();
            UpdateStatus();
        }

        private void AddMoveButton(Panel parent, string text, int x, int y, int w, int h, int dx, int dy)
        {
            var button = MakeButton(text, surfaceLight, textMain, w, h);
            button.Location = new Point(x, y);
            button.Click += (_, _) => { if (game != null) ProcessNotice(game.Move(dx, dy)); };
            parent.Controls.Add(button);
        }

        private void RefreshMap()
        {
            if (game?.CurrentDimension == null) return;
            var map = game.GetMapSnapshot();
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    string symbol = (col == game.X && row == game.Y) ? "C" : map[row, col];
                    Button b = mapButtons[col, row];
                    b.Text = symbol;
                    b.BackColor = symbol switch
                    {
                        "C" => Color.FromArgb(27, 132, 82),
                        "E" => Color.FromArgb(52, 75, 92),
                        "*" => Color.FromArgb(45, 92, 130),
                        "+" => Color.FromArgb(130, 49, 58),
                        "#" => Color.FromArgb(85, 38, 43),
                        "R" => Color.FromArgb(30, 106, 120),
                        "O" => Color.FromArgb(151, 113, 32),
                        _ => surfaceLight
                    };
                    b.ForeColor = textMain;
                }
            }
            UpdateStatus();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (game == null || !game.MissionActive || game.InCombat) return;
            int dx = 0, dy = 0;
            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: dy = -1; break;
                case Keys.S: case Keys.Down: dy = 1; break;
                case Keys.A: case Keys.Left: dx = -1; break;
                case Keys.D: case Keys.Right: dx = 1; break;
                default: return;
            }
            e.Handled = true;
            ProcessNotice(game.Move(dx, dy));
        }

        private void ProcessNotice(GameNotice? notice)
        {
            if (notice == null || game == null) return;
            UpdateStatus();
            if (notice.CombatStarted) { ShowCombat(); return; }
            if (notice.CoreFound)
            {
                var answer = MessageBox.Show(notice.Message + "\n\n¿Registrar el núcleo energético?", notice.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes) RunAction(game.RegisterCore(), "Registro del núcleo");
                else MessageBox.Show("El núcleo no ha sido registrado. Continúa la exploración.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (game.CoreFound || !game.MissionActive) ShowDashboard(); else RefreshMap();
                return;
            }
            MessageBox.Show(notice.Message, notice.Title, MessageBoxButtons.OK, notice.Type == "error" ? MessageBoxIcon.Error : notice.Type == "success" ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (notice.Defeat) ShowDashboard();
            else if (game.MissionActive) RefreshMap();
            else ShowDashboard();
        }

        private void ShowCombat()
        {
            if (game == null) return;
            while (game.InCombat && game.Explorer.Life > 0)
            {
                var result = MessageBox.Show($"BICHO DIMENSIONAL\nVida del enemigo: {game.EnemyLife}/30\nVida del cadete: {game.Explorer.Life}/100\n\nSí: ATACAR\nNo: ESCAPAR", "COMBATE NEXUS", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                GameNotice outcome = result == DialogResult.Yes ? game.Attack() : game.Escape();
                UpdateStatus();
                if (!game.InCombat || outcome.Defeat)
                {
                    MessageBox.Show(outcome.Message, outcome.Title, MessageBoxButtons.OK, outcome.Defeat ? MessageBoxIcon.Error : MessageBoxIcon.Information);
                    break;
                }
            }
            if (game.Explorer.Life <= 0) ShowDashboard();
            else if (game.MissionActive) { ShowExploration(); }
        }

        private void RunAction(GameNotice? notice, string heading)
        {
            if (notice == null) return;
            MessageBox.Show(notice.Message, heading + " · " + notice.Title, MessageBoxButtons.OK, notice.Type == "error" ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            UpdateStatus();
            if (content.Controls.Count > 0 && game?.MissionActive == true && mapButtons[0, 0] != null) RefreshMap();
            if (notice.MissionCompleted || notice.Defeat) ShowDashboard();
        }

        private void ShowInventory()
        {
            if (game == null) return;
            content.Controls.Clear();
            AddPageHeading("INVENTARIO", "Recursos recolectados durante la exploración dimensional.");
            var card = CreateCard(650, 300);
            card.Location = new Point(0, 100);
            var header = MakeLabel("OBJETOS DISPONIBLES", 10, true, accent);
            header.Location = new Point(24, 20);
            card.Controls.Add(header);
            int y = 65;
            foreach (var item in game.Inventory.Items)
            {
                var name = MakeLabel(item.Key, 11, true, textMain);
                name.Location = new Point(25, y);
                var count = MakeLabel("× " + item.Value, 12, true, Color.FromArgb(91, 195, 142));
                count.Location = new Point(440, y);
                card.Controls.Add(name); card.Controls.Add(count);
                y += 48;
            }
            content.Controls.Add(card);
            UpdateStatus();
        }

        private void ShowProfile()
        {
            if (game == null) return;
            content.Controls.Clear();
            AddPageHeading("PERFIL DEL EXPLORADOR", "Datos de identificación y estado actual del cadete.");
            var card = CreateCard(650, 340);
            card.Location = new Point(0, 100);
            string[] rows = { "NOMBRE: " + game.Explorer.Name, "EDAD: " + game.Explorer.Age, "VIDA: " + game.Explorer.Life + " / 100", "ENERGÍA: " + game.Explorer.Energy + " / 100", "CONEXIÓN: " + game.Explorer.Connection + " / 100", "REALIDAD: " + (game.CurrentDimension?.Name ?? "NAVE NEXUS") };
            for (int i = 0; i < rows.Length; i++)
            {
                var label = MakeLabel(rows[i], 12, true, textMain);
                label.Location = new Point(25, 25 + i * 48);
                card.Controls.Add(label);
            }
            content.Controls.Add(card);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (game == null) return;
            explorerLabel?.GetType();
            if (lifeBar != null)
            {
                lifeBar.Value = game.Explorer.Life;
                energyBar.Value = game.Explorer.Energy;
                connectionBar.Value = game.Explorer.Connection;
                lifeValue.Text = game.Explorer.Life + "%";
                energyValue.Text = game.Explorer.Energy + "%";
                connectionValue.Text = game.Explorer.Connection + "%";
            }
        }

        private void AddPageHeading(string title, string subtitle)
        {
            var heading = MakeLabel(title, 22, true, textMain);
            heading.Location = new Point(0, 0);
            var sub = MakeLabel(subtitle, 9.5F, false, textMuted);
            sub.Location = new Point(2, 42);
            content.Controls.Add(heading);
            content.Controls.Add(sub);
        }

        private Panel CreateCard(int width, int height) =>
            new() { Size = new Size(width, height), BackColor = surface, BorderStyle = BorderStyle.FixedSingle };

        private Label MakeLabel(string text, float size, bool bold, Color color) =>
            new() { Text = text, AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

        private Button MakeButton(string text, Color back, Color fore, int width, int height)
        {
            var button = new Button { Text = text, Size = new Size(width, height), BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(Math.Min(255, back.R + 18), Math.Min(255, back.G + 18), Math.Min(255, back.B + 18));
            return button;
        }

        private void Disconnect()
        {
            if (MessageBox.Show("¿Deseas cerrar la sesión de NEXUS?", "Desconectar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            game = null;
            ShowRegistration();
        }
    }
}
