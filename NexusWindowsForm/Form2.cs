using System.Drawing;
using System.Windows.Forms;

namespace NexusWindowsForm
{
    public class Form2 : Form
    {
        private readonly Color fondo = Color.FromArgb(7, 9, 14);
        private readonly Color panel = Color.FromArgb(17, 20, 29);
        private readonly Color rojo = Color.FromArgb(220, 35, 55);
        private readonly Color texto = Color.FromArgb(235, 238, 245);
        private readonly string[,] mapa =
        {
            { "E", ".", ".", ".", ".", ".", ".", ".", ".", "." },
            { ".", ".", "*", ".", ".", ".", ".", "+", ".", "." },
            { ".", "#", ".", ".", "*", ".", ".", ".", ".", "." },
            { ".", ".", ".", "+", ".", ".", "#", ".", ".", "." },
            { ".", "*", ".", ".", ".", ".", ".", ".", "+", "." },
            { ".", ".", "#", ".", ".", "R", ".", ".", ".", "." },
            { ".", "+", ".", ".", "#", ".", ".", "*", ".", "." },
            { ".", ".", ".", "*", ".", ".", ".", ".", "#", "." },
            { ".", "#", ".", ".", "+", ".", "*", ".", ".", "." },
            { ".", ".", ".", ".", ".", ".", ".", ".", "O", "." }
        };

        private readonly Button[,] casillas = new Button[10, 10];
        private readonly Dictionary<string, int> inventario = new()
        {
            ["CELULA DE ENERGIA"] = 0,
            ["RECURSO"] = 0
        };

        private Panel panelRegistro = null!;
        private TextBox txtNombre = null!;
        private NumericUpDown numEdad = null!;
        private NumericUpDown numEnergia = null!;
        private NumericUpDown numConexion = null!;
        private Label lblNombre = null!;
        private Label lblVida = null!;
        private Label lblEnergia = null!;
        private Label lblConexion = null!;
        private Label lblPosicion = null!;
        private Label lblMensaje = null!;
        private Label lblInventario = null!;
        private Label lblMision = null!;
        private Panel panelJuego = null!;
        private FlowLayoutPanel panelMapa = null!;
        private ProgressBar barraVida = null!;
        private ProgressBar barraEnergia = null!;
        private ProgressBar barraConexion = null!;

        private string nombreExplorador = "";
        private int edad;
        private int vida = 100;
        private int energia = 100;
        private int conexion = 100;
        private int x;
        private int y;
        private bool misionActiva;
        private bool nucleoEncontrado;
        private string realidad = "REALIDAD 102";
        private bool iniciado;

        public Form2()
        {
            Text = "NEXUS V2.0 - MATRIX SYSTEM";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(1000, 680);
            BackColor = fondo;
            ForeColor = texto;
            Font = new Font("Segoe UI", 10F);
            KeyPreview = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            ConstruirInterfaz();
            KeyDown += Form2_KeyDown;
        }

        private void ConstruirInterfaz()
        {
            var encabezado = new Label
            {
                Text = "N E X U S     //     SYSTEM V2.0 ONLINE",
                Dock = DockStyle.Top,
                Height = 62,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Black,
                ForeColor = rojo,
                Font = new Font("Consolas", 20F, FontStyle.Bold)
            };
            Controls.Add(encabezado);

            panelJuego = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = fondo,
                Padding = new Padding(18),
                Visible = false
            };
            Controls.Add(panelJuego);
            panelJuego.BringToFront();
            encabezado.BringToFront();

            ConstruirPanelRegistro();
            ConstruirPanelJuego();
        }

        private void ConstruirPanelRegistro()
        {
            panelRegistro = new Panel
            {
                Size = new Size(520, 470),
                BackColor = panel,
                Padding = new Padding(28),
                Location = new Point((ClientSize.Width - 520) / 2, 135),
                Anchor = AnchorStyles.None
            };
            Controls.Add(panelRegistro);

            var titulo = CrearLabel("REGISTRO DEL CADETE", 24, rojo, true);
            titulo.Dock = DockStyle.Top;
            titulo.Height = 55;
            titulo.TextAlign = ContentAlignment.MiddleCenter;
            panelRegistro.Controls.Add(titulo);

            var tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(12, 75, 12, 5)
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            for (int i = 0; i < 5; i++)
                tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            panelRegistro.Controls.Add(tabla);
            titulo.BringToFront();

            txtNombre = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F), BackColor = Color.FromArgb(30, 34, 44), ForeColor = texto, BorderStyle = BorderStyle.FixedSingle };
            numEdad = CrearNumero(18, 80);
            numEnergia = CrearNumero(1, 100);
            numConexion = CrearNumero(1, 100);
            AgregarCampo(tabla, 0, "NOMBRE DEL CADETE:", txtNombre);
            AgregarCampo(tabla, 1, "EDAD:", numEdad);
            AgregarCampo(tabla, 2, "ENERGÍA INICIAL:", numEnergia);
            AgregarCampo(tabla, 3, "NIVEL DE CONEXIÓN:", numConexion);

            var iniciar = CrearBoton("INICIALIZAR NEXUS", rojo);
            iniciar.Dock = DockStyle.Fill;
            iniciar.Click += (_, _) => IniciarSistema();
            tabla.Controls.Add(iniciar, 0, 4);
            tabla.SetColumnSpan(iniciar, 2);
        }

        private NumericUpDown CrearNumero(int min, int max) => new()
        {
            Minimum = min,
            Maximum = max,
            Value = max,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 34, 44),
            ForeColor = texto,
            Font = new Font("Segoe UI", 12F)
        };

        private void AgregarCampo(TableLayoutPanel tabla, int fila, string etiqueta, Control control)
        {
            tabla.Controls.Add(CrearLabel(etiqueta, 10, texto, true), 0, fila);
            tabla.Controls.Add(control, 1, fila);
        }

        private void ConstruirPanelJuego()
        {
            var hud = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                ColumnCount = 4,
                BackColor = panel,
                Padding = new Padding(12)
            };
            for (int i = 0; i < 4; i++)
                hud.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panelJuego.Controls.Add(hud);

            var perfil = new Panel { Dock = DockStyle.Fill };
            lblNombre = CrearLabel("EXPLORADOR", 12, rojo, true);
            lblNombre.Dock = DockStyle.Top;
            lblVida = CrearLabel("VIDA: 100/100", 9, texto);
            lblVida.Dock = DockStyle.Bottom;
            barraVida = CrearBarra();
            barraVida.Dock = DockStyle.Bottom;
            perfil.Controls.Add(lblVida);
            perfil.Controls.Add(barraVida);
            perfil.Controls.Add(lblNombre);
            hud.Controls.Add(perfil, 0, 0);

            lblEnergia = CrearLabel("ENERGÍA: 100/100", 10, texto);
            barraEnergia = CrearBarra();
            barraEnergia.Dock = DockStyle.Bottom;
            lblEnergia.Dock = DockStyle.Top;
            var energiaPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            energiaPanel.Controls.Add(barraEnergia);
            energiaPanel.Controls.Add(lblEnergia);
            hud.Controls.Add(energiaPanel, 1, 0);

            lblConexion = CrearLabel("CONEXIÓN: 100/100", 10, texto);
            barraConexion = CrearBarra();
            barraConexion.Dock = DockStyle.Bottom;
            lblConexion.Dock = DockStyle.Top;
            var conexionPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            conexionPanel.Controls.Add(barraConexion);
            conexionPanel.Controls.Add(lblConexion);
            hud.Controls.Add(conexionPanel, 2, 0);

            lblPosicion = CrearLabel("POSICIÓN: (0,0)", 11, texto, true);
            lblPosicion.Dock = DockStyle.Fill;
            lblPosicion.TextAlign = ContentAlignment.MiddleCenter;
            hud.Controls.Add(lblPosicion, 3, 0);

            var contenido = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 15, 0, 0)
            };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67));
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            panelJuego.Controls.Add(contenido);

            var mapaPanel = new Panel { Dock = DockStyle.Fill, BackColor = panel, Padding = new Padding(12) };
            contenido.Controls.Add(mapaPanel, 0, 0);
            var tituloMapa = CrearLabel("MAPA DIMENSIONAL  //  " + realidad, 13, rojo, true);
            tituloMapa.Dock = DockStyle.Top;
            tituloMapa.Height = 38;
            mapaPanel.Controls.Add(tituloMapa);

            panelMapa = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(10, 13, 20),
                WrapContents = true
            };
            mapaPanel.Controls.Add(panelMapa);
            tituloMapa.BringToFront();
            CrearCasillas();

            var lateral = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = panel,
                Padding = new Padding(14)
            };
            contenido.Controls.Add(lateral, 1, 0);

            lateral.Controls.Add(CrearLabel("MISIÓN ACTUAL", 15, rojo, true));
            lblMision = CrearLabel("LA ANOMALÍA PERDIDA\nExplora la realidad, reúne recursos y registra el núcleo energético.", 10, texto);
            lblMision.Size = new Size(300, 95);
            lateral.Controls.Add(lblMision);

            lateral.Controls.Add(CrearLabel("INVENTARIO", 14, rojo, true));
            lblInventario = CrearLabel("", 10, texto);
            lblInventario.Size = new Size(300, 70);
            lateral.Controls.Add(lblInventario);

            lateral.Controls.Add(CrearLabel("CONTROLES", 14, rojo, true));
            var controles = new TableLayoutPanel { Width = 290, Height = 145, ColumnCount = 3, RowCount = 3 };
            for (int i = 0; i < 3; i++)
            {
                controles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
                controles.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            }
            controles.Controls.Add(CrearBoton("W ▲", rojo, (_, _) => Mover(0, -1)), 1, 0);
            controles.Controls.Add(CrearBoton("A ◀", rojo, (_, _) => Mover(-1, 0)), 0, 1);
            controles.Controls.Add(CrearBoton("S ▼", rojo, (_, _) => Mover(0, 1)), 1, 1);
            controles.Controls.Add(CrearBoton("D ▶", rojo, (_, _) => Mover(1, 0)), 2, 1);
            lateral.Controls.Add(controles);

            lateral.Controls.Add(CrearBoton("ANALIZAR ANOMALÍA  (-20)", rojo, (_, _) => AnalizarAnomalia()));
            lateral.Controls.Add(CrearBoton("RECUPERAR ENERGÍA  (+20)", Color.FromArgb(25, 115, 85), (_, _) => RecuperarEnergia()));
            lateral.Controls.Add(CrearBoton("USAR CÉLULA  (+30)", Color.FromArgb(25, 115, 85), (_, _) => UsarCelula()));
            lateral.Controls.Add(CrearBoton("VER PERFIL", Color.FromArgb(55, 65, 85), (_, _) => MostrarPerfil()));
            lateral.Controls.Add(CrearBoton("CAMBIAR REALIDAD", Color.FromArgb(55, 65, 85), (_, _) => CambiarRealidad()));
            lateral.Controls.Add(CrearBoton("VOLVER AL MENÚ", Color.FromArgb(90, 35, 45), (_, _) => Close()));

            lblMensaje = CrearLabel("SISTEMA LISTO. REGISTRA AL EXPLORADOR PARA COMENZAR.", 10, Color.FromArgb(255, 190, 90), true);
            lblMensaje.Dock = DockStyle.Bottom;
            lblMensaje.Height = 35;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;
            panelJuego.Controls.Add(lblMensaje);
            lblMensaje.BringToFront();
        }

        private void CrearCasillas()
        {
            panelMapa.Controls.Clear();
            int lado = 43;
            for (int fila = 0; fila < 10; fila++)
            {
                for (int col = 0; col < 10; col++)
                {
                    int cx = col, cy = fila;
                    var b = new Button
                    {
                        Size = new Size(lado, lado),
                        Margin = new Padding(2),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Consolas", 12F, FontStyle.Bold),
                        Tag = new Point(cx, cy)
                    };
                    b.FlatAppearance.BorderColor = Color.FromArgb(55, 65, 80);
                    b.Click += (_, _) =>
                    {
                        int dx = cx - x, dy = cy - y;
                        if (Math.Abs(dx) + Math.Abs(dy) == 1)
                            Mover(dx, dy);
                    };
                    casillas[fila, col] = b;
                    panelMapa.Controls.Add(b);
                }
            }
            ActualizarMapa();
        }

        private void IniciarSistema()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre del cadete para iniciar NEXUS.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            nombreExplorador = txtNombre.Text.Trim();
            edad = (int)numEdad.Value;
            energia = (int)numEnergia.Value;
            conexion = (int)numConexion.Value;
            vida = 100;
            iniciado = true;
            misionActiva = true;
            panelRegistro.Visible = false;
            panelJuego.Visible = true;
            panelJuego.BringToFront();
            ActualizarHUD();
            ActualizarInventario();
            Mensaje("NEXUS V2.0 ONLINE. EXPLORADOR REGISTRADO.");
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!iniciado) return;
            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up: Mover(0, -1); e.Handled = true; break;
                case Keys.S:
                case Keys.Down: Mover(0, 1); e.Handled = true; break;
                case Keys.A:
                case Keys.Left: Mover(-1, 0); e.Handled = true; break;
                case Keys.D:
                case Keys.Right: Mover(1, 0); e.Handled = true; break;
                case Keys.Escape: Close(); break;
            }
        }

        private void Mover(int dx, int dy)
        {
            if (!iniciado || !misionActiva) return;
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || nx > 9 || ny < 0 || ny > 9)
            {
                Mensaje("LÍMITE DEL MAPA. NO PUEDES AVANZAR.");
                return;
            }
            if (energia < 2)
            {
                Mensaje("ENERGÍA INSUFICIENTE PARA MOVERTE.");
                return;
            }
            energia -= 2;
            conexion = Math.Max(0, conexion - 1);
            x = nx; y = ny;
            ProcesarCasilla();
            ActualizarTodo();
        }

        private void ProcesarCasilla()
        {
            switch (mapa[y, x])
            {
                case "*":
                    inventario["CELULA DE ENERGIA"]++;
                    mapa[y, x] = ".";
                    Mensaje("CÉLULA DE ENERGÍA RECOLECTADA. REVISA TU INVENTARIO.");
                    break;
                case "+":
                case "#":
                    EnfrentarBicho();
                    break;
                case "R":
                    Mensaje("ZONA DE ANOMALÍA DETECTADA. LOS SENSORES REGISTRAN ACTIVIDAD.");
                    break;
                case "O":
                    if (!nucleoEncontrado && MessageBox.Show("NÚCLEO ENERGÉTICO ENCONTRADO.\n¿Deseas registrarlo?", "NEXUS", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        nucleoEncontrado = true;
                        mapa[y, x] = "E";
                        misionActiva = false;
                        MessageBox.Show("NÚCLEO REGISTRADO.\nDATOS ENVIADOS A LA NAVE.\nMISIÓN COMPLETADA.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Mensaje("EL NÚCLEO SIGUE SIN REGISTRARSE. CONTINÚA LA EXPLORACIÓN.");
                    }
                    break;
            }
        }

        private void EnfrentarBicho()
        {
            int vidaBicho = 30;
            if (MessageBox.Show("BICHO DIMENSIONAL DETECTADO.\nVIDA: 30\n\n¿Quieres combatir?\nSí = Atacar / No = Escapar", "ALERTA NEXUS", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                Mensaje("HAS ESCAPADO DE LA ENTIDAD.");
                return;
            }

            while (vidaBicho > 0 && vida > 0)
            {
                vidaBicho = Math.Max(0, vidaBicho - 20);
                if (vidaBicho > 0) vida = Math.Max(0, vida - 10);
            }

            if (vida > 0)
            {
                mapa[y, x] = ".";
                Mensaje("BICHO DIMENSIONAL ELIMINADO. CAMINO DESPEJADO.");
            }
            else
            {
                misionActiva = false;
                MessageBox.Show("EL CADETE HA PERDIDO TODA SU VIDA.\nMISIÓN FALLIDA.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AnalizarAnomalia()
        {
            if (energia < 20) { Mensaje("ENERGÍA INSUFICIENTE. SE NECESITAN 20 PUNTOS."); return; }
            energia -= 20;
            conexion = Math.Max(0, conexion - 5);
            ActualizarTodo();
            MessageBox.Show("ESCANEO COMPLETADO.\nANOMALÍA DIMENSIONAL DETECTADA.\nCOSTO: 20 ENERGÍA.", "ANÁLISIS NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RecuperarEnergia()
        {
            energia = Math.Min(100, energia + 20);
            conexion = Math.Min(100, conexion + 10);
            ActualizarTodo();
            Mensaje("SE RECUPERARON 20 PUNTOS DE ENERGÍA.");
        }

        private void UsarCelula()
        {
            if (inventario["CELULA DE ENERGIA"] <= 0) { Mensaje("NO TIENES CÉLULAS DE ENERGÍA."); return; }
            inventario["CELULA DE ENERGIA"]--;
            energia = Math.Min(100, energia + 30);
            ActualizarTodo();
            Mensaje("CÉLULA UTILIZADA. +30 ENERGÍA.");
        }

        private void CambiarRealidad()
        {
            var opcion = MessageBox.Show("¿Cambiar a REALIDAD 999?\nSe consumirán 30 puntos de energía.", "SELECCIÓN DE REALIDAD", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (opcion == DialogResult.Cancel) return;
            if (energia < 30) { Mensaje("ENERGÍA INSUFICIENTE PARA VIAJAR."); return; }
            energia -= 30;
            realidad = opcion == DialogResult.Yes ? "REALIDAD 999" : "REALIDAD 102";
            x = 0; y = 0; nucleoEncontrado = false; misionActiva = true;
            ReiniciarMapa();
            ActualizarTodo();
            Mensaje("NUEVA MISIÓN: LA ANOMALÍA PERDIDA. " + realidad);
        }

        private void ReiniciarMapa()
        {
            string[,] original =
            {
                { "E", ".", ".", ".", ".", ".", ".", ".", ".", "." },
                { ".", ".", "*", ".", ".", ".", ".", "+", ".", "." },
                { ".", "#", ".", ".", "*", ".", ".", ".", ".", "." },
                { ".", ".", ".", "+", ".", ".", "#", ".", ".", "." },
                { ".", "*", ".", ".", ".", ".", ".", ".", "+", "." },
                { ".", ".", "#", ".", ".", "R", ".", ".", ".", "." },
                { ".", "+", ".", ".", "#", ".", ".", "*", ".", "." },
                { ".", ".", ".", "*", ".", ".", ".", ".", "#", "." },
                { ".", "#", ".", ".", "+", ".", "*", ".", ".", "." },
                { ".", ".", ".", ".", ".", ".", ".", ".", "O", "." }
            };
            for (int fila = 0; fila < 10; fila++)
                for (int col = 0; col < 10; col++)
                    mapa[fila, col] = original[fila, col];
        }

        private void MostrarPerfil()
        {
            MessageBox.Show($"CADETE: {nombreExplorador}\nEDAD: {edad}\nVIDA: {vida}/100\nENERGÍA: {energia}/100\nCONEXIÓN: {conexion}/100", "PERFIL DEL EXPLORADOR", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ActualizarTodo()
        {
            ActualizarHUD();
            ActualizarMapa();
            ActualizarInventario();
        }

        private void ActualizarHUD()
        {
            lblNombre.Text = "CADETE: " + nombreExplorador;
            lblVida.Text = $"VIDA: {vida}/100";
            lblEnergia.Text = $"ENERGÍA: {energia}/100";
            lblConexion.Text = $"CONEXIÓN: {conexion}/100";
            lblPosicion.Text = $"POSICIÓN: ({x},{y})  //  {realidad}";
            barraVida.Value = Math.Clamp(vida, 0, 100);
            barraEnergia.Value = Math.Clamp(energia, 0, 100);
            barraConexion.Value = Math.Clamp(conexion, 0, 100);
        }

        private void ActualizarMapa()
        {
            if (casillas[0, 0] == null) return;
            for (int fila = 0; fila < 10; fila++)
            {
                for (int col = 0; col < 10; col++)
                {
                    var b = casillas[fila, col];
                    string simbolo = fila == y && col == x ? "C" : mapa[fila, col];
                    b.Text = simbolo;
                    b.ForeColor = simbolo switch
                    {
                        "C" => Color.Lime,
                        "*" => Color.White,
                        "+" => Color.OrangeRed,
                        "#" => Color.Red,
                        "O" => Color.Gold,
                        "R" => Color.Cyan,
                        "E" => Color.DeepSkyBlue,
                        _ => Color.FromArgb(125, 135, 150)
                    };
                    b.BackColor = simbolo switch
                    {
                        "C" => Color.FromArgb(20, 75, 45),
                        "*" => Color.FromArgb(35, 45, 60),
                        "+" or "#" => Color.FromArgb(65, 25, 32),
                        "O" => Color.FromArgb(75, 60, 20),
                        "R" => Color.FromArgb(20, 55, 65),
                        _ => Color.FromArgb(24, 29, 39)
                    };
                    b.Enabled = iniciado;
                }
            }
        }

        private void ActualizarInventario()
        {
            lblInventario.Text = $"CÉLULA DE ENERGÍA: {inventario["CELULA DE ENERGIA"]}\nRECURSOS: {inventario["RECURSO"]}";
        }

        private void Mensaje(string mensaje)
        {
            lblMensaje.Text = ">> " + mensaje;
            ActualizarTodo();
        }

        private Label CrearLabel(string textoLabel, float tamano, Color color, bool negrita = false) => new()
        {
            Text = textoLabel,
            ForeColor = color,
            BackColor = Color.Transparent,
            Font = new Font("Consolas", tamano, negrita ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private ProgressBar CrearBarra() => new()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 100,
            Height = 16,
            Style = ProgressBarStyle.Continuous
        };

        private Button CrearBoton(string textoBoton, Color color, EventHandler? accion = null)
        {
            var boton = new Button
            {
                Text = textoBoton,
                Height = 38,
                Width = 290,
                Margin = new Padding(3, 5, 3, 5),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderColor = Color.FromArgb(90, color);
            boton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15F);
            if (accion != null) boton.Click += accion;
            return boton;
        }
    }
}