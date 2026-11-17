using System.Drawing;
using System.Windows.Forms;

namespace NexusWindowsForm
{
    public partial class Form2 : Form
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
        private bool teclaMovimientoPresionada;

        public Form2()
        {
            InitializeComponent();
            bool enDisenador = System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
            panelJuego.Visible = enDisenador;
            panelRegistro.Visible = true;
            if (!enDisenador)
                CrearCasillas();

            btnInicializar.Click += (_, _) => IniciarSistema();
            btnArriba.Click += (_, _) => Mover(0, -1);
            btnIzquierda.Click += (_, _) => Mover(-1, 0);
            btnAbajo.Click += (_, _) => Mover(0, 1);
            btnDerecha.Click += (_, _) => Mover(1, 0);
            btnAnalizar.Click += (_, _) => AnalizarAnomalia();
            btnRecuperar.Click += (_, _) => RecuperarEnergia();
            btnUsarCelula.Click += (_, _) => UsarCelula();
            btnPerfil.Click += (_, _) => MostrarPerfil();
            btnCambiarRealidad.Click += (_, _) => CambiarRealidad();
            btnVolver.Click += (_, _) => Close();
            KeyDown += Form2_KeyDown;
            KeyUp += Form2_KeyUp;
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
            lblEncabezado.BringToFront();
            ActualizarHUD();
            ActualizarInventario();
            Mensaje("NEXUS V2.0 ONLINE. EXPLORADOR REGISTRADO.");
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!iniciado) return;

            // Windows repite KeyDown mientras la tecla permanece presionada.
            // Bloqueamos esas repeticiones: una pulsación = una casilla.
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Up or Keys.Down or Keys.Left or Keys.Right)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (teclaMovimientoPresionada) return;
                teclaMovimientoPresionada = true;
            }

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

        private void Form2_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Up or Keys.Down or Keys.Left or Keys.Right)
                teclaMovimientoPresionada = false;
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
            lblTituloMapa.Text = "MAPA DIMENSIONAL // " + realidad;
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

    }
}
