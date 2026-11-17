using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexusWindowsForm
{
    public partial class Form2 : Form
    {
        private readonly Color rojo = Color.FromArgb(230, 20, 48);
        private readonly List<Entidad> entidades = new();
        private readonly List<PointF> celdasEnergia = new() { new(2700, 2500), new(2300, 2400), new(3100, 2850), new(1900, 3000) };
        private readonly List<NucleoDimensional> nucleos = new()
        {
            new("102-A", "REALIDAD 102", new PointF(2200, 2600)),
            new("102-B", "REALIDAD 102", new PointF(3400, 3328)),
            new("999-A", "REALIDAD 999", new PointF(1152, 1024)),
            new("999-B", "REALIDAD 999", new PointF(4352, 1664))
        };
        private readonly HashSet<string> nucleosRecolectados = new(StringComparer.Ordinal);
        private readonly HashSet<Keys> teclas = new();
        private readonly System.Windows.Forms.Timer relojMovimiento = new();
        private readonly System.Windows.Forms.Timer relojEscena = new();
        private readonly Bitmap[,] spritesCadete = new Bitmap[4, 2];
        private const int nucleosNecesarios = 3;

        private string nombreExplorador = "";
        private int edad;
        private int vida = 100, energia = 100, conexion = 100;
        private float jugadorX = 2500, jugadorY = 2500;
        private float camaraX, camaraY;
        private bool iniciado, encuentroActivo, naveReparada;
        private string realidad = "REALIDAD 102";
        private Entidad? entidadActual;
        private int fotograma;
        private int ticksMovimiento;
        private int celulasEnergia;
        private int direccionJugador = 2;
        private bool jugadorCaminando;

        private sealed record NucleoDimensional(string Id, string Realidad, PointF Posicion);

        private sealed class Entidad
        {
            public string Nombre { get; init; } = "";
            public string Tipo { get; init; } = "";
            public PointF Posicion { get; set; }
            public int Vida { get; set; } = 60;
            public int VidaMaxima { get; init; } = 60;
            public Bitmap Sprite { get; init; } = null!;
            public PointF DestinoMovimiento { get; set; }
            public float Velocidad { get; init; } = 1.1f;
            public bool Derrotada { get; set; }
        }

        public Form2()
        {
            InitializeComponent();
            bool enDisenador = System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
            panelJuego.Visible = enDisenador;
            panelRegistro.Visible = true;

            if (!enDisenador)
            {
                CrearEscenasPixelArt();
                CrearEntidades();
                CrearSpritesCadete();
                panelMapa.Paint += DibujarMundo;
                panelMapa.Resize += (_, _) => panelMapa.Invalidate();
                relojMovimiento.Interval = 16;
                relojMovimiento.Tick += (_, _) => ActualizarMovimiento();
                relojEscena.Interval = 180;
                relojEscena.Tick += (_, _) => { fotograma++; panelMapa.Invalidate(); };
                relojEscena.Start();
            }

            btnInicializar.Click += (_, _) => IniciarSistema();
            btnAnalizar.Click += (_, _) => AnalizarAnomalia();
            btnRecuperar.Click += (_, _) => RecuperarEnergia();
            btnUsarCelula.Click += (_, _) => UsarCelula();
            btnPerfil.Click += (_, _) => MostrarPerfil();
            btnCambiarRealidad.Click += (_, _) => SeleccionarRealidad();
            btnVolver.Click += (_, _) => Close();
            btnAtacar.Click += (_, _) => AtacarEntidad();
            btnEscapar.Click += (_, _) => EscaparEntidad();
            KeyPreview = true;
            KeyDown += Form2_KeyDown;
            KeyUp += Form2_KeyUp;
            Resize += (_, _) => AjustarLayout();
            Shown += (_, _) => AjustarLayout();
        }


        private void AjustarLayout()
        {
            if (panelJuego == null || panelMapaContenedor == null || panelLateral == null) return;
            int margen = 18;
            int lateral = Math.Clamp(ClientSize.Width / 5, 320, 390);
            int superior = panelHud.Height + 12;
            int inferior = panelAcciones.Height + lblMensaje.Height + 28;
            int alto = Math.Max(300, panelJuego.ClientSize.Height - superior - inferior);
            int anchoMapa = Math.Max(500, panelJuego.ClientSize.Width - lateral - margen * 3);
            panelMapaContenedor.SetBounds(margen, superior, anchoMapa, alto);
            panelLateral.SetBounds(margen * 2 + anchoMapa, superior, lateral, alto);
            panelAcciones.SetBounds(0, panelJuego.ClientSize.Height - panelAcciones.Height - lblMensaje.Height, panelJuego.ClientSize.Width, panelAcciones.Height);
            int[] anchos = { 190, 190, 160, 150, 190, 190 };
            Button[] botones = { btnAnalizar, btnRecuperar, btnUsarCelula, btnPerfil, btnCambiarRealidad, btnVolver };
            int espacio = 12;
            int total = anchos.Sum() + espacio * (botones.Length - 1);
            int x = Math.Max(12, (panelAcciones.ClientSize.Width - total) / 2);
            for (int i = 0; i < botones.Length; i++)
            {
                botones[i].SetBounds(x, 16, anchos[i], 40);
                x += anchos[i] + espacio;
            }
            panelRegistro.Location = new Point((ClientSize.Width - panelRegistro.Width) / 2, (ClientSize.Height - panelRegistro.Height) / 2);
            panelEncuentro.Location = new Point(
                Math.Max(8, (panelMapa.ClientSize.Width - panelEncuentro.Width) / 2),
                lblTituloMapa.Height + Math.Max(0, (panelMapa.ClientSize.Height - panelEncuentro.Height) / 2));
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelMapa.Invalidate();
        }

        private void CrearEscenasPixelArt()
        {
            imgPueblo.Image = Properties.Resources.MapaHawkins;
            imgBosque.Image = CrearEscenaPixelArt(1);
            imgLaboratorio.Image = CrearEscenaPixelArt(2);
            imgPortal.Image = CrearEscenaPixelArt(3);
        }

        private Bitmap CrearEscenaPixelArt(int escena)
        {
            const int w = 160, h = 90;
            Bitmap bmp = new Bitmap(w, h);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(8, 2, 10));
            void P(Color c, int x, int y, int a, int b)
            { using SolidBrush br = new(c); g.FillRectangle(br, x, y, a, b); }
            Color sombra = Color.FromArgb(20, 5, 18), rojoOscuro = Color.FromArgb(85, 4, 25), rojoFuego = Color.FromArgb(245, 20, 48);
            P(Color.FromArgb(45, 3, 25), 0, 0, w, 45);
            if (escena == 0)
            {
                P(rojoFuego, 112, 8, 22, 22);
                for (int i = 0; i < 5; i++) { int x = i * 34; P(sombra, x, 42, 28, 40); P(rojoOscuro, x + 5, 51, 7, 7); P(rojoFuego, x + 18, 56, 5, 8); }
                P(Color.Black, 0, 78, w, 12);
            }
            else if (escena == 1)
            {
                for (int i = 0; i < 8; i++) { int x = i * 22; P(sombra, x + 5, 8, 9, 82); P(sombra, x - 3, 20, 23, 8); P(rojoOscuro, x - 4, 57, 25, 9); }
                for (int i = 0; i < 18; i++) P(rojoFuego, (i * 37) % 156, (i * 19) % 80, 2, 2);
            }
            else if (escena == 2)
            {
                P(Color.FromArgb(32, 22, 35), 8, 8, 144, 74); P(rojoOscuro, 18, 18, 124, 54);
                for (int i = 0; i < 5; i++) P(Color.FromArgb(100, 20, 38), 22 + i * 25, 12, 4, 64);
                P(rojoFuego, 64, 27, 32, 37); P(Color.FromArgb(255, 90, 110), 70, 33, 20, 24);
            }
            else
            {
                P(rojoOscuro, 42, 10, 78, 70); P(rojoFuego, 52, 17, 58, 58); P(Color.FromArgb(18, 4, 18), 70, 31, 22, 31);
                for (int i = 0; i < 10; i++) P(rojoFuego, (i * 31) % 155, (i * 13) % 85, 3, 3);
            }
            return bmp;
        }

        private static Bitmap CrearSpriteMonstruo() => (Bitmap)Properties.Resources.MonstruoMapa.Clone();

        private void CrearEntidades()
        {
            foreach (Entidad anterior in entidades) anterior.Sprite.Dispose();
            entidades.Clear();
            PointF primero = new(2850, 2580);
            PointF segundo = new(2150, 2200);
            entidades.Add(new Entidad
            {
                Nombre = "ACECHADOR DEL REVÉS", Tipo = "Entidad orgánica", Posicion = primero,
                DestinoMovimiento = new PointF(primero.X + 170, primero.Y - 90), Velocidad = 1.15f,
                Sprite = CrearSpriteMonstruo(), Vida = 60
            });
            entidades.Add(new Entidad
            {
                Nombre = "SOMBRA DEL VACÍO", Tipo = "Entidad de sombra", Posicion = segundo,
                DestinoMovimiento = new PointF(segundo.X - 140, segundo.Y + 120), Velocidad = 0.95f,
                Sprite = CrearSpriteMonstruo(), Vida = 45, VidaMaxima = 45
            });
        }

        private void CrearSpritesCadete()
        {
            for (int direccion = 0; direccion < 4; direccion++)
            for (int paso = 0; paso < 2; paso++)
                spritesCadete[direccion, paso] = CrearSpriteCadete(direccion, paso);
        }

        private static Bitmap CrearSpriteCadete(int direccion, int paso)
        {
            Bitmap bmp = new(28, 38);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.None;
            void P(Color c, int x, int y, int w, int h)
            {
                using SolidBrush b = new(c);
                g.FillRectangle(b, x, y, w, h);
            }

            Color borde = Color.FromArgb(14, 20, 31);
            Color armadura = Color.FromArgb(74, 91, 111);
            Color luz = Color.FromArgb(155, 178, 190);
            Color cyan = Color.FromArgb(36, 222, 239);
            int zancada = paso == 0 ? 0 : 2;

            // Botas y piernas: cambian de posición en cada fotograma.
            P(borde, 5 - zancada, 27, 8, 9); P(armadura, 6 - zancada, 28, 6, 6); P(luz, 6 - zancada, 29, 3, 2);
            P(borde, 15 + zancada, 27, 8, 9); P(armadura, 16 + zancada, 28, 6, 6); P(luz, 18 + zancada, 29, 3, 2);
            P(Color.FromArgb(21, 29, 43), 8 - zancada, 23, 6, 7); P(armadura, 9 - zancada, 24, 4, 5);
            P(Color.FromArgb(21, 29, 43), 15 + zancada, 23, 6, 7); P(armadura, 16 + zancada, 24, 4, 5);

            // Mochila técnica, torso blindado y brazos articulados.
            P(borde, 5, 14, 18, 13); P(Color.FromArgb(39, 51, 68), 6, 15, 16, 11);
            P(borde, 3, 16, 6, 10); P(armadura, 4, 17, 4, 7); P(luz, 4, 18, 2, 2);
            P(borde, 19, 16, 6, 10); P(armadura, 20, 17, 4, 7); P(luz, 21, 18, 2, 2);
            P(borde, 7, 15, 14, 11); P(armadura, 8, 16, 12, 9); P(luz, 9, 16, 8, 2);
            P(Color.FromArgb(30, 42, 57), 10, 20, 8, 4); P(cyan, 11, 20, 2, 2); P(cyan, 16, 20, 2, 2);
            P(Color.FromArgb(29, 40, 55), 5, 16, 3, 9); P(Color.FromArgb(29, 40, 55), 20, 16, 3, 9);

            // Casco de exploración con visor luminoso orientado según WASD.
            P(borde, 6, 1, 16, 15); P(armadura, 7, 2, 14, 12); P(luz, 9, 3, 8, 2);
            P(Color.FromArgb(54, 70, 89), 8, 5, 11, 7);
            if (direccion == 2)
            {
                P(borde, 7, 6, 14, 7); P(Color.FromArgb(10, 80, 106), 8, 7, 12, 5);
                P(cyan, 9, 7, 8, 3); P(Color.FromArgb(156, 250, 255), 10, 7, 3, 1);
            }
            else if (direccion == 1)
            {
                P(borde, 15, 5, 8, 8); P(Color.FromArgb(10, 80, 106), 17, 6, 5, 6); P(cyan, 18, 6, 3, 4);
            }
            else if (direccion == 3)
            {
                P(borde, 5, 5, 8, 8); P(Color.FromArgb(10, 80, 106), 6, 6, 5, 6); P(cyan, 7, 6, 3, 4);
            }
            else
            {
                P(Color.FromArgb(39, 51, 68), 9, 5, 10, 7);
                P(cyan, 10, 8, 3, 3); P(cyan, 15, 8, 3, 3);
            }
            P(luz, 6, 7, 2, 4); P(borde, 7, 13, 14, 3);

            // Paneles de la mochila y baliza cian al mirar hacia arriba.
            if (direccion == 0)
            {
                P(borde, 8, 16, 12, 8); P(Color.FromArgb(63, 77, 93), 9, 17, 10, 6);
                P(cyan, 10, 18, 2, 3); P(cyan, 16, 18, 2, 3); P(luz, 12, 17, 2, 2);
            }
            P(borde, 9, 25, 10, 2); P(Color.FromArgb(28, 38, 53), 11, 26, 6, 1);
            return bmp;
        }

        private void IniciarSistema()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre del cadete para iniciar NEXUS.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus(); return;
            }
            using Form3 selector = new(realidad);
            if (selector.ShowDialog(this) != DialogResult.OK) return;

            MessageBox.Show(this,
                "Eres un viajero de realidades. Durante un salto dimensional, el reactor de tu nave se rompió y te dejó varado en Hawkins. " +
                "Recupera tres núcleos estables en las realidades 102 y 999 para reemplazar el núcleo dañado y reparar la nave. " +
                "La linterna es tu única guía: los acechadores se mueven entre las casas y solo verás lo que ilumine su haz.",
                "NEXUS // DIRECTIVA DE MISIÓN", MessageBoxButtons.OK, MessageBoxIcon.Information);

            nombreExplorador = txtNombre.Text.Trim();
            edad = (int)numEdad.Value;
            energia = (int)numEnergia.Value;
            conexion = (int)numConexion.Value;
            vida = 100; iniciado = true; encuentroActivo = false; naveReparada = false;
            nucleosRecolectados.Clear();
            realidad = selector.RealidadSeleccionada;
            jugadorX = 2500; jugadorY = 2500;
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelRegistro.Visible = false; panelJuego.Visible = true; panelJuego.BringToFront(); lblEncabezado.BringToFront();
            AjustarLayout();
            relojMovimiento.Start();
            ActualizarHUD(); Mensaje("MISIÓN ACTIVA: RECUPERA 3 NÚCLEOS Y REPARA TU NAVE.");
            panelMapa.Focus();
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            // En el registro las letras WASD se escriben normalmente en el nombre.
            if (!iniciado) return;
            if (e.KeyCode == Keys.Escape && encuentroActivo) { EscaparEntidad(); e.Handled = true; return; }
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Up or Keys.Down or Keys.Left or Keys.Right)
            {
                teclas.Add(e.KeyCode); e.Handled = true; e.SuppressKeyPress = true;
            }
        }

        private void Form2_KeyUp(object? sender, KeyEventArgs e)
        {
            teclas.Remove(e.KeyCode);
            if (teclas.Count == 0 && jugadorCaminando)
            {
                jugadorCaminando = false;
                panelMapa.Invalidate();
            }
        }

        private void ActualizarMovimiento()
        {
            if (!iniciado || !panelJuego.Visible) return;
            bool monstruosEnMovimiento = ActualizarMonstruos();
            if (encuentroActivo)
            {
                if (monstruosEnMovimiento) panelMapa.Invalidate();
                return;
            }

            float dx = 0, dy = 0;
            if (teclas.Contains(Keys.W) || teclas.Contains(Keys.Up)) dy--;
            if (teclas.Contains(Keys.S) || teclas.Contains(Keys.Down)) dy++;
            if (teclas.Contains(Keys.A) || teclas.Contains(Keys.Left)) dx--;
            if (teclas.Contains(Keys.D) || teclas.Contains(Keys.Right)) dx++;
            if (dx == 0 && dy == 0)
            {
                if (jugadorCaminando) { jugadorCaminando = false; panelMapa.Invalidate(); }
                bool inventarioCambio = ActualizarNucleosCercanos();
                inventarioCambio |= RecolectarCelulaCercana();
                if (inventarioCambio) ActualizarHUD();
                DetectarEntidad();
                if (monstruosEnMovimiento) panelMapa.Invalidate();
                return;
            }
            jugadorCaminando = true;
            direccionJugador = Math.Abs(dx) > Math.Abs(dy)
                ? (dx > 0 ? 1 : 3)
                : (dy > 0 ? 2 : 0);
            float magnitud = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= magnitud; dy /= magnitud;
            float velocidad = energia > 0 ? 4.2f : 1.4f;
            float siguienteX = Math.Clamp(jugadorX + dx * velocidad, 80, 4920);
            if (!EstaDentroDeUnaCasa(new PointF(siguienteX, jugadorY), 11)) jugadorX = siguienteX;
            float siguienteY = Math.Clamp(jugadorY + dy * velocidad, 80, 4920);
            if (!EstaDentroDeUnaCasa(new PointF(jugadorX, siguienteY), 11)) jugadorY = siguienteY;
            ticksMovimiento++;
            if (ticksMovimiento % 30 == 0)
            {
                energia = Math.Max(0, energia - 1);
                conexion = Math.Max(0, conexion - 1);
            }
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            ActualizarNucleosCercanos();
            RecolectarCelulaCercana();
            DetectarEntidad();
            ActualizarHUD();
            panelMapa.Invalidate();
        }

        private bool ActualizarMonstruos()
        {
            bool seMovioAlguno = false;
            foreach (Entidad entidad in entidades.Where(e => !e.Derrotada && e != entidadActual))
            {
                float dxJugador = jugadorX - entidad.Posicion.X;
                float dyJugador = jugadorY - entidad.Posicion.Y;
                float distanciaJugador = MathF.Sqrt(dxJugador * dxJugador + dyJugador * dyJugador);
                if (distanciaJugador < 420)
                    entidad.DestinoMovimiento = new PointF(jugadorX, jugadorY);
                else
                {
                    float dxDestino = entidad.DestinoMovimiento.X - entidad.Posicion.X;
                    float dyDestino = entidad.DestinoMovimiento.Y - entidad.Posicion.Y;
                    if (dxDestino * dxDestino + dyDestino * dyDestino < 24 * 24)
                    {
                        entidad.DestinoMovimiento = new PointF(
                            Math.Clamp(entidad.Posicion.X + Random.Shared.Next(-220, 221), 90, 4910),
                            Math.Clamp(entidad.Posicion.Y + Random.Shared.Next(-220, 221), 90, 4910));
                    }
                }

                float dx = entidad.DestinoMovimiento.X - entidad.Posicion.X;
                float dy = entidad.DestinoMovimiento.Y - entidad.Posicion.Y;
                float distancia = MathF.Sqrt(dx * dx + dy * dy);
                if (distancia < 1) continue;
                float paso = Math.Min(entidad.Velocidad, distancia);
                dx = dx / distancia * paso;
                dy = dy / distancia * paso;
                PointF anterior = entidad.Posicion;
                float x = Math.Clamp(anterior.X + dx, 80, 4920);
                if (!EstaDentroDeUnaCasa(new PointF(x, anterior.Y), 10)) entidad.Posicion = new PointF(x, anterior.Y);
                float y = Math.Clamp(anterior.Y + dy, 80, 4920);
                if (!EstaDentroDeUnaCasa(new PointF(entidad.Posicion.X, y), 10)) entidad.Posicion = new PointF(entidad.Posicion.X, y);
                if (Math.Abs(entidad.Posicion.X - anterior.X) > 0.01f || Math.Abs(entidad.Posicion.Y - anterior.Y) > 0.01f)
                    seMovioAlguno = true;
                else
                    entidad.DestinoMovimiento = new PointF(
                        Math.Clamp(anterior.X + Random.Shared.Next(-180, 181), 90, 4910),
                        Math.Clamp(anterior.Y + Random.Shared.Next(-180, 181), 90, 4910));
            }
            return seMovioAlguno;
        }

        private static bool EstaDentroDeUnaCasa(PointF posicion, float radio)
        {
            const int tile = 64;
            int inicioX = (int)MathF.Floor((posicion.X - radio - 166) / tile);
            int finX = (int)MathF.Ceiling((posicion.X + radio + 166) / tile);
            int inicioY = (int)MathF.Floor((posicion.Y - radio - 134) / tile);
            int finY = (int)MathF.Ceiling((posicion.Y + radio + 134) / tile);
            RectangleF cuerpo = new(posicion.X - radio, posicion.Y - radio, radio * 2, radio * 2);

            for (int ty = inicioY; ty <= finY; ty++)
            for (int tx = inicioX; tx <= finX; tx++)
            {
                int loteX = ModuloPositivo(tx, 17), loteY = ModuloPositivo(ty, 13);
                if (!((loteX == 4 || loteX == 12) && (loteY == 4 || loteY == 9))) continue;
                RectangleF paredes = new(tx * tile - 84, ty * tile - 66, 164, 116);
                if (paredes.IntersectsWith(cuerpo)) return true;
            }
            return false;
        }

        private bool EstaEnHazDeLinterna(PointF objetivo, float alcance)
        {
            float dx = objetivo.X - jugadorX;
            float dy = objetivo.Y - jugadorY;
            float distancia = MathF.Sqrt(dx * dx + dy * dy);
            if (distancia < 18) return true;
            if (distancia > alcance) return false;
            (float dirX, float dirY) = direccionJugador switch
            {
                0 => (0, -1),
                1 => (1, 0),
                2 => (0, 1),
                _ => (-1, 0)
            };
            float producto = (dx * dirX + dy * dirY) / distancia;
            return producto >= 0.76f;
        }

        private bool ActualizarNucleosCercanos()
        {
            if (naveReparada) return false;
            NucleoDimensional? recogido = nucleos.FirstOrDefault(n => n.Realidad == realidad &&
                !nucleosRecolectados.Contains(n.Id) &&
                Math.Abs(n.Posicion.X - jugadorX) < 32 && Math.Abs(n.Posicion.Y - jugadorY) < 32 &&
                EstaEnHazDeLinterna(n.Posicion, 360));
            if (recogido == null || !nucleosRecolectados.Add(recogido.Id)) return false;

            naveReparada = nucleosRecolectados.Count >= nucleosNecesarios;
            string progreso = $"{nucleosRecolectados.Count}/{nucleosNecesarios}";
            Mensaje(naveReparada
                ? $"OBJETO RECOGIDO: NÚCLEO {progreso}. REACTOR REEMPLAZADO; NAVE REPARADA."
                : $"OBJETO RECOGIDO: NÚCLEO {progreso}. BUSCA OTROS EN LAS DOS REALIDADES.");
            return true;
        }

        private bool RecolectarCelulaCercana()
        {
            int indice = celdasEnergia.FindIndex(p => Math.Abs(p.X - jugadorX) < 34 && Math.Abs(p.Y - jugadorY) < 34);
            if (indice < 0) return false;
            celdasEnergia.RemoveAt(indice);
            celulasEnergia++;
            Mensaje("OBJETO RECOGIDO: CÉLULA DE ENERGÍA.");
            return true;
        }

        private void DetectarEntidad()
        {
            Entidad? encontrada = entidades.FirstOrDefault(e => !e.Derrotada &&
                Math.Abs(e.Posicion.X - jugadorX) < 62 && Math.Abs(e.Posicion.Y - jugadorY) < 62 &&
                EstaEnHazDeLinterna(e.Posicion, 360));
            if (encontrada == null) return;
            encuentroActivo = true; entidadActual = encontrada; teclas.Clear();
            lblMonstruo.Text = encontrada.Nombre;
            lblDetalleMonstruo.Text = $"{encontrada.Tipo}  //  VIDA: {encontrada.Vida}/{encontrada.VidaMaxima}";
            imgMonstruo.Image = Properties.Resources.Demogorgon;
            barraVidaMonstruo.Value = 0;
            barraVidaMonstruo.Maximum = encontrada.VidaMaxima;
            barraVidaMonstruo.Value = encontrada.Vida;
            barraVidaCadeteCombate.Value = Math.Clamp(vida, 0, 100);
            lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100";
            panelEncuentro.Visible = true; panelEncuentro.BringToFront();
            Mensaje("ALERTA: UNA ENTIDAD TE HA DETECTADO.");
        }

        private void AtacarEntidad()
        {
            if (!encuentroActivo || entidadActual == null) return;
            entidadActual.Vida -= 20;
            barraVidaMonstruo.Value = Math.Clamp(entidadActual.Vida, 0, barraVidaMonstruo.Maximum);
            if (entidadActual.Vida <= 0)
            {
                entidadActual.Derrotada = true;
                encuentroActivo = false; panelEncuentro.Visible = false; entidadActual = null;
                energia = Math.Max(0, energia - 5);
                Mensaje("ENTIDAD ELIMINADA. SECTOR DESPEJADO.");
            }
            else
            {
                vida = Math.Max(0, vida - 12);
                lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}/{entidadActual.VidaMaxima}";
                barraVida.Value = vida;
                barraVidaCadeteCombate.Value = vida;
                lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100  //  {entidadActual.Nombre} TE ATACÓ (-12)";
                if (vida <= 0)
                {
                    encuentroActivo = false; panelEncuentro.Visible = false;
                    MessageBox.Show("EL CADETE HA CAÍDO. MISIÓN FALLIDA.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            ActualizarHUD(); panelMapa.Invalidate();
        }

        private void EscaparEntidad()
        {
            if (entidadActual != null)
            {
                entidadActual.Posicion = new PointF(jugadorX + 220, jugadorY + 180);
            }
            entidadActual = null; encuentroActivo = false; panelEncuentro.Visible = false;
            Mensaje("ESCAPASTE. LA ENTIDAD SIGUE EN EL SECTOR.");
            panelMapa.Focus();
        }

        private void DibujarMundo(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.Clear(Color.FromArgb(12, 5, 13));
            int tile = 64;
            int inicioX = (int)(camaraX / tile) - 1, inicioY = (int)(camaraY / tile) - 1;
            int finX = inicioX + panelMapa.ClientSize.Width / tile + 3;
            int finY = inicioY + panelMapa.ClientSize.Height / tile + 3;
            for (int ty = inicioY; ty <= finY; ty++)
            for (int tx = inicioX; tx <= finX; tx++)
            {
                int sx = tx * tile - (int)camaraX, sy = ty * tile - (int)camaraY;
                int calleY = Math.Abs(ty % 13), calleX = Math.Abs(tx % 17);
                bool carretera = calleY <= 1 || calleX <= 1;
                bool acera = !carretera && (calleY == 2 || calleX == 2);
                Color suelo = realidad == "REALIDAD 102" ? Color.FromArgb(50, 59, 42) : Color.FromArgb(31, 24, 39);
                Color asfalto = realidad == "REALIDAD 102" ? Color.FromArgb(48, 47, 45) : Color.FromArgb(40, 27, 39);
                Color pavimento = realidad == "REALIDAD 102" ? Color.FromArgb(86, 80, 66) : Color.FromArgb(61, 48, 62);
                using (SolidBrush b = new(carretera ? asfalto : acera ? pavimento : suelo)) g.FillRectangle(b, sx, sy, tile, tile);
                if (acera)
                {
                    using Pen junta = new(Color.FromArgb(38, 22, 20), 1);
                    g.DrawLine(junta, sx + 31, sy, sx + 31, sy + tile);
                    g.DrawLine(junta, sx, sy + 31, sx + tile, sy + 31);
                }
                int semilla = unchecked((tx * 73856093) ^ (ty * 19349663)) & int.MaxValue;
                if (carretera)
                {
                    using SolidBrush marca = new(Color.FromArgb(140, 174, 151, 104));
                    if (calleY == 0 && tx % 3 == 0) g.FillRectangle(marca, sx + 25, sy + 30, 19, 3);
                    if (calleX == 0 && ty % 3 == 0) g.FillRectangle(marca, sx + 30, sy + 24, 3, 19);
                }
                else if (semilla % 7 == 0) DibujarArbol(g, sx + 8, sy + 8);
                else if (semilla % 11 == 0)
                {
                    using SolidBrush escombro = new(Color.FromArgb(104, 67, 57, 54));
                    g.FillRectangle(escombro, sx + 15, sy + 17, 12, 7);
                    g.FillRectangle(escombro, sx + 34, sy + 31, 8, 12);
                    g.FillRectangle(escombro, sx + 20, sy + 42, 6, 4);
                }
                else if (semilla % 5 == 0)
                {
                    using SolidBrush mancha = new(Color.FromArgb(65, 7, 25));
                    g.FillRectangle(mancha, sx + 20, sy + 22, 8, 8);
                    g.FillRectangle(mancha, sx + 37, sy + 39, 5, 5);
                }
            }

            // Las casas ocupan varias casillas, así que se dibujan después del suelo
            // para que las baldosas vecinas no tapen sus paredes ni el tejado.
            for (int ty = inicioY - 2; ty <= finY + 2; ty++)
            for (int tx = inicioX - 2; tx <= finX + 2; tx++)
            {
                int loteX = ModuloPositivo(tx, 17), loteY = ModuloPositivo(ty, 13);
                bool anclaCasa = (loteX == 4 || loteX == 12) && (loteY == 4 || loteY == 9);
                bool juntoCarretera = Math.Abs(ty % 13) <= 2 || Math.Abs(tx % 17) <= 2;
                if (!anclaCasa || juntoCarretera) continue;

                int semillaCasa = unchecked((tx * 73856093) ^ (ty * 19349663)) & int.MaxValue;
                int casaX = tx * tile - (int)camaraX - 126;
                int casaY = ty * tile - (int)camaraY - 84;
                DibujarCasaBarrio(g, casaX, casaY, semillaCasa);
            }

            foreach (PointF celda in celdasEnergia)
            {
                float cx = celda.X - camaraX, cy = celda.Y - camaraY;
                using SolidBrush energiaBrush = new(Color.Gold);
                g.FillRectangle(energiaBrush, cx - 8, cy - 8, 16, 16);
                using Pen energiaPen = new(Color.OrangeRed, 2);
                g.DrawRectangle(energiaPen, cx - 11, cy - 11, 22, 22);
            }

            foreach (Entidad entidad in entidades.Where(z => !z.Derrotada))
            {
                float ex = entidad.Posicion.X - camaraX, ey = entidad.Posicion.Y - camaraY;
                if (ex > -70 && ey > -70 && ex < panelMapa.Width + 70 && ey < panelMapa.Height + 70)
                    g.DrawImage(entidad.Sprite, ex - 35, ey - 33, 70, 66);
            }

            // Cadete astronauta, con dirección y paso animado en pixel art.
            float px = jugadorX - camaraX, py = jugadorY - camaraY;
            using (SolidBrush sombra = new(Color.FromArgb(175, 0, 0, 0))) g.FillEllipse(sombra, px - 20, py + 15, 40, 12);
            int frameCadete = jugadorCaminando ? fotograma % 2 : 0;
            g.DrawImage(spritesCadete[direccionJugador, frameCadete], px - 22, py - 38, 44, 58);

            foreach (NucleoDimensional nucleo in nucleos.Where(n => n.Realidad == realidad &&
                !nucleosRecolectados.Contains(n.Id)).Take(Math.Max(0, nucleosNecesarios - nucleosRecolectados.Count)))
            {
                float nx = nucleo.Posicion.X - camaraX, ny = nucleo.Posicion.Y - camaraY;
                using SolidBrush resplandor = new(Color.FromArgb(95, 82, 222, 255));
                g.FillEllipse(resplandor, nx - 21, ny - 21, 42, 42);
                PointF[] cristal = { new(nx, ny - 15), new(nx + 11, ny), new(nx, ny + 15), new(nx - 11, ny) };
                using SolidBrush cristalBrush = new(Color.FromArgb(95, 224, 245, 255));
                using Pen bordeCristal = new(Color.FromArgb(90, 240, 250, 255), 2);
                g.FillPolygon(cristalBrush, cristal);
                g.DrawPolygon(bordeCristal, cristal);
            }

            DibujarLinterna(g, px, py);
            using SolidBrush coords = new(Color.FromArgb(220, 255, 230, 230));
            g.DrawString($"X: {(int)jugadorX}   Y: {(int)jugadorY}   //   {realidad}", new Font("Consolas", 10, FontStyle.Bold), coords, 12, 12);
        }

        private void DibujarLinterna(Graphics g, float centroX, float centroY)
        {
            if (!iniciado) return;
            float anguloCentro = direccionJugador switch { 0 => 270, 1 => 0, 2 => 90, _ => 180 };
            const float alcance = 360;
            const float apertura = 78;
            using GraphicsPath haz = new();
            haz.AddPie(centroX - alcance, centroY - alcance, alcance * 2, alcance * 2,
                anguloCentro - apertura / 2, apertura);
            using Region oscuridad = new(new Rectangle(0, 0, panelMapa.ClientSize.Width, panelMapa.ClientSize.Height));
            oscuridad.Exclude(haz);
            using SolidBrush sombra = new(Color.FromArgb(246, 2, 4, 10));
            g.FillRegion(sombra, oscuridad);
            using SolidBrush luz = new(Color.FromArgb(24, 255, 235, 170));
            g.FillPath(luz, haz);

            float radianes = anguloCentro * MathF.PI / 180;
            using Pen rayo = new(Color.FromArgb(72, 255, 239, 184), 2);
            g.DrawLine(rayo, centroX, centroY - 5,
                centroX + MathF.Cos(radianes) * alcance, centroY + MathF.Sin(radianes) * alcance);
        }

        private void DibujarArbol(Graphics g, int x, int y)
        {
            using SolidBrush tronco = new(Color.FromArgb(38, 15, 20));
            using SolidBrush copa = new(realidad == "REALIDAD 102" ? Color.FromArgb(15, 43, 29) : Color.FromArgb(45, 13, 37));
            using SolidBrush brillo = new(realidad == "REALIDAD 102" ? Color.FromArgb(67, 81, 44) : Color.FromArgb(132, 21, 54));
            g.FillRectangle(tronco, x + 22, y + 28, 12, 22);
            g.FillRectangle(copa, x + 8, y + 5, 40, 35);
            g.FillRectangle(brillo, x + 16, y + 12, 8, 7);
            g.FillRectangle(brillo, x + 34, y + 24, 7, 6);
        }

        private static int ModuloPositivo(int valor, int modulo) => (valor % modulo + modulo) % modulo;
        private void DibujarCasaBarrio(Graphics g, int x, int y, int semilla)
        {
            void R(Color color, int rx, int ry, int rw, int rh)
            {
                using SolidBrush b = new(color);
                g.FillRectangle(b, x + rx, y + ry, rw, rh);
            }
            void L(Color color, int width, Point a, Point b)
            {
                using Pen p = new(color, width);
                g.DrawLine(p, x + a.X, y + a.Y, x + b.X, y + b.Y);
            }

            int variante = ModuloPositivo(semilla, 3);
            Color lawn = realidad == "REALIDAD 102" ? Color.FromArgb(65 + variante * 5, 77, 48) : Color.FromArgb(42, 35, 48 + variante * 5);
            Color roof = realidad == "REALIDAD 102"
                ? new[] { Color.FromArgb(69, 55, 48), Color.FromArgb(54, 61, 60), Color.FromArgb(77, 57, 48) }[variante]
                : new[] { Color.FromArgb(43, 29, 46), Color.FromArgb(56, 27, 43), Color.FromArgb(37, 35, 52) }[variante];
            Color window = realidad == "REALIDAD 102" ? Color.FromArgb(107, 171, 183) : Color.FromArgb(201, 67, 94);

            // Patio rectangular, cercas, césped y acceso desde la calle.
            R(Color.FromArgb(100, 0, 0, 0), 3, 7, 246, 158);
            R(lawn, 0, 0, 244, 154);
            using (Pen cerca = new(Color.FromArgb(74, 62, 48), 2))
            {
                g.DrawLine(cerca, x + 4, y + 5, x + 4, y + 147);
                g.DrawLine(cerca, x + 240, y + 5, x + 240, y + 147);
                g.DrawLine(cerca, x + 4, y + 5, x + 92, y + 5);
                g.DrawLine(cerca, x + 152, y + 5, x + 240, y + 5);
                g.DrawLine(cerca, x + 4, y + 147, x + 92, y + 147);
                g.DrawLine(cerca, x + 152, y + 147, x + 240, y + 147);
            }
            for (int i = 0; i < 18; i++)
            {
                int gx = 12 + ModuloPositivo(semilla + i * 29, 216);
                int gy = 10 + ModuloPositivo(semilla / 11 + i * 17, 130);
                R(Color.FromArgb(105, 99 + i % 3 * 8, 102, 61), gx, gy, 3, 2);
            }

            // Cimientos y paredes bajo el tejado, con fachada de madera y ladrillo.
            R(Color.FromArgb(23, 20, 21), 41, 22, 165, 112);
            R(Color.FromArgb(89, 77, 62), 46, 28, 155, 99);
            R(Color.FromArgb(107, 88, 67), 49, 31, 149, 94);
            for (int i = 0; i < 7; i++)
                L(Color.FromArgb(74, 59, 48), 1, new Point(49, 43 + i * 11), new Point(198, 43 + i * 11));

            // Tejado visto desde arriba: faldones, tejas, respiradero y una zona dañada.
            R(Color.FromArgb(22, 18, 22), 42, 18, 164, 96);
            R(roof, 47, 21, 154, 89);
            for (int row = 0; row < 9; row++)
            {
                int yy = 27 + row * 9;
                L(Color.FromArgb(103, 96, 79), 1, new Point(50, yy), new Point(198, yy));
                int salto = row % 2 == 0 ? 0 : 10;
                for (int col = 0; col < 8; col++)
                {
                    int xx = 52 + salto + col * 18;
                    if (xx < 195) L(Color.FromArgb(48, 35, 34), 1, new Point(xx, yy), new Point(xx, yy + 8));
                }
            }
            R(Color.FromArgb(44, 40, 38), 109, 25, 27, 6);
            R(Color.FromArgb(91, 84, 70), 112, 26, 20, 3);
            R(Color.FromArgb(19, 15, 19), 162, 31, 23, 18);
            R(Color.FromArgb(117, 39, 48), 166, 34, 15, 11);
            R(Color.FromArgb(53, 39, 40), 172, 20, 14, 7);

            // Ventanas, puerta, porche y garaje con detalles luminosos.
            R(Color.FromArgb(24, 25, 30), 53, 54, 25, 19); R(window, 56, 57, 19, 13);
            R(Color.FromArgb(24, 25, 30), 174, 54, 22, 19); R(window, 177, 57, 16, 13);
            R(Color.FromArgb(35, 25, 24), 105, 83, 28, 27); R(Color.FromArgb(129, 73, 50), 109, 86, 20, 23);
            R(Color.FromArgb(191, 175, 124), 124, 96, 2, 2);
            R(Color.FromArgb(86, 74, 59), 95, 108, 50, 8);
            R(Color.FromArgb(111, 98, 76), 88, 116, 64, 7);
            R(Color.FromArgb(81, 78, 69), 167, 78, 29, 45);
            for (int i = 0; i < 4; i++) R(Color.FromArgb(122, 111, 91), 169, 81 + i * 9, 25, 2);
            R(Color.FromArgb(77, 71, 60), 171, 124, 24, 28);

            // Arbustos, árboles pequeños y piedras en cada parcela.
            DibujarArbol(g, x + 3, y + 15);
            DibujarArbol(g, x + 194, y + 101);
            R(Color.FromArgb(26, 52, 34), 79, 11, 32, 8);
            R(Color.FromArgb(38, 70, 42), 82, 9, 25, 7);
            R(Color.FromArgb(102, 93, 74), 22, 120, 8, 5);
            R(Color.FromArgb(102, 93, 74), 31, 127, 6, 4);
        }

        private void DibujarCasaAbandonada(Graphics g, int x, int y, int semilla)
        {
            void Poligono(Color color, params Point[] puntos)
            {
                using SolidBrush b = new(color);
                g.FillPolygon(b, puntos);
            }

            void Linea(Color color, int ancho, params Point[] puntos)
            {
                using Pen p = new(color, ancho);
                g.DrawLines(p, puntos);
            }

            Point Pt(int px, int py) => new(x + px, y + py);
            int variante = ModuloPositivo(semilla, 3);
            Color tierra = realidad == "REALIDAD 102" ? Color.FromArgb(76 + variante * 4, 76, 45) : Color.FromArgb(48, 43, 61 + variante * 5);
            Color tejadoA = variante switch
            {
                0 => Color.FromArgb(24, 18, 24),
                1 => Color.FromArgb(23, 20, 29),
                _ => Color.FromArgb(31, 17, 25)
            };
            Color tejadoB = variante switch
            {
                0 => Color.FromArgb(31, 19, 28),
                1 => Color.FromArgb(29, 22, 34),
                _ => Color.FromArgb(39, 18, 29)
            };

            // Jardín en rombo, ruinas, raíces y sendero de piedra.
            Poligono(Color.FromArgb(90, 0, 0, 0), Pt(125, 18), Pt(246, 88), Pt(125, 166), Pt(4, 91));
            Poligono(tierra, Pt(125, 8), Pt(246, 78), Pt(125, 158), Pt(4, 83));
            Linea(Color.FromArgb(115, 104, 76), 2, Pt(7, 83), Pt(125, 158), Pt(242, 78));
            for (int i = 0; i < 15; i++)
            {
                int gx = 18 + ModuloPositivo(semilla + i * 37, 205);
                int gy = 45 + ModuloPositivo(semilla / 7 + i * 19, 88);
                using SolidBrush hierba = new(Color.FromArgb(95, 91 + i % 3 * 8, 100, 55));
                g.FillRectangle(hierba, x + gx, y + gy, 3, 2);
            }
            Linea(Color.FromArgb(42, 18, 17), 4, Pt(12, 111), Pt(36, 104), Pt(51, 112), Pt(63, 104));
            Linea(Color.FromArgb(42, 18, 17), 3, Pt(212, 108), Pt(193, 98), Pt(177, 104), Pt(163, 96));
            for (int i = 0; i < 4; i++)
            {
                int py = 130 + i * 6;
                Poligono(Color.FromArgb(104, 92, 75), Pt(96 - i * 4, py), Pt(130 + i * 3, py - 2), Pt(139 + i * 2, py + 5), Pt(104 - i * 3, py + 7));
                Linea(Color.FromArgb(48, 36, 32), 1, Pt(110 - i * 2, py + 1), Pt(119 + i * 2, py + 4));
            }

            // Paredes de madera y cimientos vistos en perspectiva isométrica.
            Poligono(Color.FromArgb(29, 25, 28), Pt(31, 64), Pt(125, 106), Pt(125, 145), Pt(31, 101));
            Poligono(Color.FromArgb(46, 38, 37), Pt(125, 106), Pt(219, 61), Pt(219, 105), Pt(125, 145));
            Poligono(Color.FromArgb(63, 62, 52), Pt(34, 62), Pt(125, 103), Pt(125, 111), Pt(34, 70));
            Poligono(Color.FromArgb(82, 72, 58), Pt(125, 103), Pt(216, 59), Pt(216, 67), Pt(125, 111));
            for (int i = 0; i < 5; i++)
            {
                int wy = 77 + i * 11;
                Linea(Color.FromArgb(82, 80, 65), 1, Pt(39, wy), Pt(119, wy + 36));
                Linea(Color.FromArgb(102, 83, 67), 1, Pt(131, wy + 34), Pt(211, wy - 5));
            }

            // Tejado roto, con capas de teja, corte abierto e interior iluminado.
            Poligono(tejadoA, Pt(21, 62), Pt(109, 11), Pt(132, 24), Pt(44, 76));
            Poligono(tejadoB, Pt(109, 11), Pt(237, 59), Pt(145, 108), Pt(132, 24));
            Linea(Color.FromArgb(101, 79, 70), 2, Pt(21, 62), Pt(109, 11), Pt(237, 59));
            Linea(Color.FromArgb(101, 79, 70), 2, Pt(21, 62), Pt(44, 76), Pt(132, 37));
            Linea(Color.FromArgb(101, 79, 70), 2, Pt(132, 37), Pt(145, 108), Pt(237, 59));
            for (int i = 0; i < 7; i++)
            {
                int offset = i * 8;
                Linea(Color.FromArgb(65, 58, 56), 1, Pt(31 + offset, 60 - offset / 2), Pt(112 + offset / 2, 17 + offset / 4));
                Linea(Color.FromArgb(73, 55, 56), 1, Pt(117 + offset, 16 + offset / 3), Pt(225 - offset / 2, 60 + offset / 3));
            }
            Poligono(Color.FromArgb(34, 22, 29), Pt(76, 48), Pt(111, 29), Pt(155, 48), Pt(119, 69));
            Poligono(Color.FromArgb(63, 43, 39), Pt(80, 48), Pt(111, 32), Pt(151, 49), Pt(119, 66));
            for (int i = 0; i < 5; i++)
            {
                int dx = i * 7;
                Linea(Color.FromArgb(95, 65, 48), 1, Pt(86 + dx, 49 - dx / 3), Pt(124 + dx / 2, 65 - dx / 3));
            }
            // Guirnalda de luces cálidas dentro del corte del tejado.
            Color[] luces = { Color.FromArgb(255, 205, 105), Color.FromArgb(111, 192, 210), Color.FromArgb(231, 101, 91), Color.FromArgb(201, 183, 111) };
            Linea(Color.FromArgb(24, 17, 22), 1, Pt(88, 44), Pt(111, 37), Pt(143, 49));
            for (int i = 0; i < 9; i++)
            {
                int lx = 89 + i * 6;
                int ly = i < 4 ? 44 - i * 2 : 37 + (i - 4) * 3;
                using SolidBrush luz = new(luces[(i + Math.Abs(semilla % luces.Length)) % luces.Length]);
                g.FillRectangle(luz, x + lx, y + ly, 3, 4);
            }
            using (Font letras = new("Consolas", 5, FontStyle.Bold))
            using (SolidBrush brilloLetras = new(Color.FromArgb(242, 210, 151)))
                g.DrawString("A B C D E", letras, brilloLetras, x + 98, y + 47);

            // Ventanas encendidas, puerta y escalones.
            Poligono(Color.FromArgb(69, 10, 28), Pt(50, 77), Pt(67, 85), Pt(67, 101), Pt(50, 93));
            Poligono(Color.FromArgb(207, 105, 64), Pt(53, 80), Pt(64, 85), Pt(64, 96), Pt(53, 91));
            Linea(Color.FromArgb(34, 19, 22), 2, Pt(58, 82), Pt(58, 94));
            Poligono(Color.FromArgb(27, 17, 22), Pt(158, 93), Pt(179, 83), Pt(179, 100), Pt(158, 111));
            Poligono(Color.FromArgb(194, 76, 51), Pt(161, 94), Pt(176, 87), Pt(176, 98), Pt(161, 106));
            Linea(Color.FromArgb(32, 17, 19), 2, Pt(169, 90), Pt(169, 102));
            Poligono(Color.FromArgb(18, 13, 18), Pt(105, 111), Pt(126, 120), Pt(126, 145), Pt(105, 135));
            Poligono(Color.FromArgb(90, 34, 37), Pt(109, 115), Pt(122, 121), Pt(122, 139), Pt(109, 133));
            using SolidBrush perilla = new(Color.FromArgb(231, 183, 101));
            g.FillRectangle(perilla, x + 119, y + 127, 2, 2);
            Poligono(Color.FromArgb(73, 63, 56), Pt(98, 137), Pt(128, 148), Pt(138, 143), Pt(108, 132));
            Poligono(Color.FromArgb(92, 78, 65), Pt(91, 144), Pt(131, 158), Pt(143, 151), Pt(103, 137));

            // Lianas colgantes y valla rota alrededor de la casa.
            Linea(Color.FromArgb(20, 35, 25), 3, Pt(29, 60), Pt(25, 76), Pt(34, 84));
            Linea(Color.FromArgb(26, 44, 28), 2, Pt(205, 73), Pt(198, 89), Pt(205, 96));
            Linea(Color.FromArgb(53, 47, 39), 2, Pt(15, 94), Pt(34, 105), Pt(49, 99));
            Linea(Color.FromArgb(53, 47, 39), 2, Pt(188, 117), Pt(208, 127), Pt(225, 116));
            for (int i = 0; i < 4; i++)
            {
                using SolidBrush hoja = new(Color.FromArgb(55, 61 + i * 3, 48, 35));
                g.FillRectangle(hoja, x + 22 + i * 9, y + 100 - i * 3, 5, 3);
            }
        }

        private void AnalizarAnomalia()
        {
            if (energia < 20) { Mensaje("ENERGÍA INSUFICIENTE. SE NECESITAN 20 PUNTOS."); return; }
            energia -= 20; conexion = Math.Max(0, conexion - 5); ActualizarHUD();
            Mensaje("ESCANEO COMPLETADO. ACTIVIDAD DIMENSIONAL DETECTADA.");
        }

        private void RecuperarEnergia()
        {
            energia = Math.Min(100, energia + 20); conexion = Math.Min(100, conexion + 10);
            ActualizarHUD(); Mensaje("SE RECUPERARON 20 PUNTOS DE ENERGÍA.");
        }

        private void UsarCelula()
        {
            if (celulasEnergia <= 0) { Mensaje("NO TIENES CÉLULAS DE ENERGÍA."); return; }
            celulasEnergia--;
            energia = Math.Min(100, energia + 30);
            ActualizarHUD();
            Mensaje("CÉLULA UTILIZADA. +30 ENERGÍA.");
        }

        private void SeleccionarRealidad()
        {
            using Form3 selector = new(realidad);
            if (selector.ShowDialog(this) == DialogResult.OK)
                CambiarRealidad(selector.RealidadSeleccionada);
        }

        private void CambiarRealidad(string nuevaRealidad)
        {
            if (nuevaRealidad == realidad) return;
            encuentroActivo = false;
            entidadActual = null;
            panelEncuentro.Visible = false;
            teclas.Clear();
            jugadorCaminando = false;
            realidad = nuevaRealidad;
            jugadorX = 2500; jugadorY = 2500; camaraX = jugadorX - panelMapa.Width / 2f; camaraY = jugadorY - panelMapa.Height / 2f;
            CrearEntidades();
            celdasEnergia.Clear();
            celdasEnergia.AddRange(new[] { new PointF(2700, 2500), new PointF(2300, 2400), new PointF(3100, 2850), new PointF(1900, 3000) });
            ActualizarHUD(); panelMapa.Invalidate();
            Mensaje("CRUCE DIMENSIONAL COMPLETADO: " + realidad + ". USA W, A, S Y D PARA EXPLORAR.");
        }

        private void MostrarPerfil()
        {
            MessageBox.Show($"CADETE: {nombreExplorador}\nEDAD: {edad}\nVIDA: {vida}/100\nENERGÍA: {energia}/100\nCONEXIÓN: {conexion}/100\nCOORDENADAS: X {jugadorX:0}, Y {jugadorY:0}",
                "PERFIL DEL CADETE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ActualizarHUD()
        {
            lblNombre.Text = "CADETE: " + nombreExplorador;
            lblVida.Text = $"VIDA: {vida}/100";
            lblEnergia.Text = $"ENERGÍA: {energia}/100";
            lblConexion.Text = $"CONEXIÓN: {conexion}/100";
            lblPosicion.Text = $"X: {(int)jugadorX}  Y: {(int)jugadorY}  //  {realidad}";
            lblTituloMapa.Text = "ZONA DE EXPLORACIÓN // " + realidad;
            lblInventario.Text = $"CÉLULAS: {celulasEnergia}\r\nNÚCLEOS: {nucleosRecolectados.Count}/{nucleosNecesarios}";
            lblContadorNucleos.Text = naveReparada
                ? $"NAVE\r\nREPARADA\r\n{nucleosRecolectados.Count}/{nucleosNecesarios}"
                : $"NÚCLEOS\r\n{nucleosRecolectados.Count} / {nucleosNecesarios}";
            lblMision.Text = naveReparada
                ? "Reactor reemplazado. Tu nave está lista para volver a viajar."
                : "Tu nave quedó varada. Reúne 3 núcleos en las realidades 102 y 999 para reparar el reactor.";
            barraVida.Value = Math.Clamp(vida, 0, 100);
            barraEnergia.Value = Math.Clamp(energia, 0, 100);
            barraConexion.Value = Math.Clamp(conexion, 0, 100);
        }

        private void Mensaje(string mensaje)
        {
            lblMensaje.Text = ">> " + mensaje;
        }

        private void ConfigurarBotonAccion(Button boton, string texto, int x, int y, int ancho)
        {
            boton.BackColor = Color.FromArgb(190, 12, 35);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = Color.FromArgb(255, 50, 70);
            boton.FlatAppearance.BorderSize = 1;
            boton.Font = new Font("Consolas", 9F, FontStyle.Bold);
            boton.ForeColor = Color.White;
            boton.Location = new Point(x, y);
            boton.Size = new Size(ancho, 40);
            boton.Text = texto;
            boton.UseVisualStyleBackColor = false;
            boton.Cursor = Cursors.Hand;
        }
    }
}
