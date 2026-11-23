using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using MediaPlayer = System.Windows.Media.MediaPlayer;

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
            new("999-A", "REALIDAD 999", new PointF(1152, 1024))
        };
        private readonly Dictionary<string, PointF> portales = new(StringComparer.Ordinal)
        {
            ["REALIDAD 102"] = new PointF(4750, 4500),
            ["REALIDAD 999"] = new PointF(600, 4500)
        };
        private readonly HashSet<string> nucleosRecolectados = new(StringComparer.Ordinal);
        private readonly HashSet<Keys> teclas = new();
        private readonly System.Windows.Forms.Timer relojMovimiento = new();
        private readonly System.Windows.Forms.Timer relojEscena = new();
        private readonly System.Windows.Forms.Timer relojAnimacionCombate = new();
        private readonly Bitmap[,] spritesCadete = new Bitmap[4, 2];
        private const int nucleosNecesarios = 3;

        private string nombreExplorador = "";
        private int edad;
        private int vida = 100, energia = 100, conexion = 100;
        private float jugadorX = 2500, jugadorY = 2500;
        private float camaraX, camaraY;
        private bool iniciado, encuentroActivo, naveReparada, recuperacionUsada, musicaActiva, misionTerminada;
        private bool portalAvisoMostrado;
        private MediaPlayer? musicaFondo;
        private ReproductorAudioBucle? musicaCinematica;
        private Bitmap? imagenMonstruoHerido, imagenMonstruoCritico;
        private Bitmap? imagenAtaqueCadete, imagenAtaqueMonstruo, imagenPortal;
        private AnimacionCombate animacionCombate;
        private string realidad = "REALIDAD 102";
        private Entidad? entidadActual;
        private int fotograma;
        private int ticksMovimiento;
        private int celulasEnergia;
        private int direccionJugador = 4;
        private float resistencia = 100f;
        private int resistenciaMostrada = -1;
        private bool jugadorCaminando;

        private sealed record NucleoDimensional(string Id, string Realidad, PointF Posicion);

        private enum AnimacionCombate
        {
            Ninguna,
            AtaqueCadete,
            AtaqueMonstruo
        }

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
                musicaFondo = new MediaPlayer { Volume = 0.42 };
                musicaFondo.MediaOpened += (_, _) =>
                {
                    if (musicaActiva && iniciado) musicaFondo.Play();
                };
                musicaFondo.MediaEnded += (_, _) =>
                {
                    if (!musicaActiva || !iniciado) return;
                    musicaFondo.Position = TimeSpan.Zero;
                    musicaFondo.Play();
                };
                musicaFondo.MediaFailed += (_, _) =>
                {
                    musicaActiva = false;
                    Mensaje("NO SE PUDO ABRIR EL AUDIO DE LA MISIÓN.");
                };
                string rutaAudioCinematica = Path.Combine(AppContext.BaseDirectory, "Resources", "SonidoMenu.mp3");
                if (File.Exists(rutaAudioCinematica))
                    musicaCinematica = new ReproductorAudioBucle(rutaAudioCinematica, 0.45);
                FormClosing += (_, _) =>
                {
                    DetenerMusica();
                    musicaCinematica?.Dispose();
                    relojMovimiento.Stop();
                    relojEscena.Stop();
                    relojAnimacionCombate.Stop();
                };
                FormClosed += (_, _) =>
                {
                    imgMonstruo.Image = null;
                    imagenMonstruoHerido?.Dispose();
                    imagenMonstruoCritico?.Dispose();
                    imagenAtaqueCadete?.Dispose();
                    imagenAtaqueMonstruo?.Dispose();
                    imagenPortal?.Dispose();
                    relojMovimiento.Dispose();
                    relojEscena.Dispose();
                    relojAnimacionCombate.Dispose();
                };
                CargarImagenesCombate();
                relojAnimacionCombate.Interval = 500;
                relojAnimacionCombate.Tick += (_, _) => CompletarAnimacionCombate();
                CrearEscenasPixelArt();
                CrearEntidades();
                CrearSpritesCadete();
                panelMapa.Paint += DibujarMundo;
                panelMapa.Resize += (_, _) => panelMapa.Invalidate();
                relojMovimiento.Interval = 16;
                relojMovimiento.Tick += (_, _) => ActualizarMovimiento();
                relojEscena.Interval = 90;
                relojEscena.Tick += (_, _) => { fotograma++; panelMapa.Invalidate(); };
                relojEscena.Start();
            }

            btnInicializar.Click += (_, _) => IniciarSistema();
            btnAnalizar.Click += (_, _) => AnalizarAnomalia();
            btnRecuperar.Click += (_, _) => RecuperarEnergia();
            btnUsarCelula.Click += (_, _) => UsarCelula();
            btnPerfil.Click += (_, _) => MostrarPerfil();
            btnVolver.Click += (_, _) => Close();
            btnAtacar.Click += (_, _) => AtacarEntidad();
            btnEscapar.Click += (_, _) => EscaparEntidad();
            KeyPreview = true;
            KeyDown += Form2_KeyDown;
            KeyUp += Form2_KeyUp;
            Resize += (_, _) => AjustarLayout();
            Shown += (_, _) => AjustarLayout();
            Shown += (_, _) => musicaCinematica?.Iniciar();
        }


        private void AjustarLayout()
        {
            if (panelJuego == null || panelMapaContenedor == null) return;
            int margen = 18;
            int superior = panelHud.Height + 12;
            int inferior = panelAcciones.Height + lblMensaje.Height + 28;
            int alto = Math.Max(300, panelJuego.ClientSize.Height - superior - inferior);
            int anchoMapa = Math.Max(500, panelJuego.ClientSize.Width - margen * 2);
            panelMapaContenedor.SetBounds(margen, superior, anchoMapa, alto);
            panelLateral.Visible = false;
            imgContadorNucleos.Visible = false;
            lblContadorNucleos.SetBounds(Math.Max(12, anchoMapa - 208), 45, 190, 38);
            lblContadorNucleos.BringToFront();
            panelAcciones.SetBounds(0, panelJuego.ClientSize.Height - panelAcciones.Height - lblMensaje.Height, panelJuego.ClientSize.Width, panelAcciones.Height);
            int[] anchos = { 190, 190, 160, 150, 190 };
            Button[] botones = { btnAnalizar, btnRecuperar, btnUsarCelula, btnPerfil, btnVolver };
            int espacio = 12;
            int total = anchos.Sum() + espacio * (botones.Length - 1);
            int x = Math.Max(12, (panelAcciones.ClientSize.Width - total) / 2);
            for (int i = 0; i < botones.Length; i++)
            {
                botones[i].SetBounds(x, 16, anchos[i], 40);
                x += anchos[i] + espacio;
            }
            panelRegistro.Location = new Point((ClientSize.Width - panelRegistro.Width) / 2, (ClientSize.Height - panelRegistro.Height) / 2);
            AjustarPanelEncuentro();
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelMapa.Invalidate();
        }

        private void CrearEscenasPixelArt()
        {
            imgPueblo.Image = Properties.Resources.AmbienteHawkins;
        }

        private void CargarImagenesCombate()
        {
            imagenMonstruoHerido = CargarImagen("DemogorgonHerido.jpg");
            imagenMonstruoCritico = CargarImagen("DemogorgonCritico.jpg");
            imagenAtaqueCadete = CargarImagen("AtaqueCadete.jpg");
            imagenAtaqueMonstruo = CargarImagen("AtaqueMonstruo.jpg");
            imagenPortal = CargarImagen("PortalDimensional.png");
        }

        private static Bitmap? CargarImagen(string nombre)
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "Resources", nombre);
            if (!File.Exists(ruta)) return null;
            using Bitmap temporal = new(ruta);
            return new Bitmap(temporal);
        }

        private Image ObtenerImagenMonstruo(Entidad entidad)
        {
            if ((long)entidad.Vida * 100 < (long)entidad.VidaMaxima * 20 && imagenMonstruoCritico != null)
                return imagenMonstruoCritico;
            if ((long)entidad.Vida * 100 < (long)entidad.VidaMaxima * 50 && imagenMonstruoHerido != null)
                return imagenMonstruoHerido;
            return Properties.Resources.Demogorgon;
        }

        private void ActualizarImagenMonstruo(Entidad entidad)
        {
            imgMonstruo.Image = ObtenerImagenMonstruo(entidad);
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

            string[] nombres =
            {
                "ACECHADOR DEL REVÉS", "SOMBRA DEL VACÍO",
                "RASTREADOR DE HAWKINS", "DEVORADOR DE CARNE",
                "ACECHANTE NOCTURNO", "ENGENDRO DIMENSIONAL",
                "COSA DEL BOSQUE", "PERSEGUIDOR DEL VACÍO"
            };
            string[] tipos =
            {
                "Entidad orgánica", "Entidad de sombra",
                "Mutación infectada", "Depredador dimensional"
            };

            // Ocho monstruos activos por cada realidad.
            for (int i = 0; i < 8; i++)
            {
                PointF posicion = ObtenerPosicionSegura(520);
                int vidaMonstruo = 42 + (i % 4) * 7;
                entidades.Add(new Entidad
                {
                    Nombre = nombres[i], Tipo = tipos[i % tipos.Length],
                    Posicion = posicion,
                    DestinoMovimiento = new PointF(posicion.X + Random.Shared.Next(-240, 241),
                                                   posicion.Y + Random.Shared.Next(-240, 241)),
                    Velocidad = 1.0f + (i % 3) * 0.22f,
                    Sprite = CrearSpriteMonstruo(), Vida = vidaMonstruo,
                    VidaMaxima = vidaMonstruo
                });
            }
        }

        private PointF ObtenerPosicionSegura(float distanciaMinimaJugador)
        {
            for (int intento = 0; intento < 2500; intento++)
            {
                PointF candidato = new(Random.Shared.Next(100, 4900), Random.Shared.Next(100, 4900));
                float dxJugador = candidato.X - jugadorX, dyJugador = candidato.Y - jugadorY;
                if (dxJugador * dxJugador + dyJugador * dyJugador < distanciaMinimaJugador * distanciaMinimaJugador) continue;
                if (EstaDentroDeObstaculo(candidato, 28)) continue;

                bool demasiadoCerca = entidades.Any(e =>
                {
                    float dx = candidato.X - e.Posicion.X, dy = candidato.Y - e.Posicion.Y;
                    return dx * dx + dy * dy < 230 * 230;
                });
                if (!demasiadoCerca) return candidato;
            }
            return new PointF(Math.Clamp(jugadorX + Random.Shared.Next(-900, 901), 100, 4900),
                              Math.Clamp(jugadorY + Random.Shared.Next(-900, 901), 100, 4900));
        }

        private void ReaparecerEntidad(Entidad entidad)
        {
            entidad.Posicion = ObtenerPosicionSegura(720);
            entidad.DestinoMovimiento = new PointF(entidad.Posicion.X + Random.Shared.Next(-260, 261),
                                                   entidad.Posicion.Y + Random.Shared.Next(-260, 261));
            entidad.Vida = entidad.VidaMaxima;
            entidad.Derrotada = false;
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
            using Form3 selector = new(txtNombre.Text.Trim(), realidad, true);
            if (selector.ShowDialog(this) != DialogResult.OK) return;
            musicaCinematica?.Detener();

            nombreExplorador = txtNombre.Text.Trim();
            edad = (int)numEdad.Value;
            energia = (int)numEnergia.Value;
            conexion = (int)numConexion.Value;
            vida = 100; iniciado = true; encuentroActivo = false; naveReparada = false; recuperacionUsada = false; misionTerminada = false;
            btnRecuperar.Enabled = true;
            btnRecuperar.Text = "RECUPERAR ENERGÍA";
            nucleosRecolectados.Clear();
            realidad = selector.RealidadSeleccionada;
            jugadorX = 2500; jugadorY = 2500;
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelRegistro.Visible = false; panelJuego.Visible = true; panelJuego.BringToFront(); lblEncabezado.BringToFront();
            AjustarLayout();
            if (energia <= 0)
            {
                FinalizarMision("MISIÓN FALLIDA: EL CADETE SE QUEDÓ SIN ENERGÍA.", MessageBoxIcon.Error);
                return;
            }
            relojMovimiento.Start();
            IniciarMusica();
            ActualizarHUD(); Mensaje("MISIÓN ACTIVA: RECUPERA 3 NÚCLEOS Y REPARA TU NAVE.");
            panelMapa.Focus();
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            // En el registro las letras WASD se escriben normalmente en el nombre.
            if (!iniciado) return;
            if (e.KeyCode is Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey)
            {
                teclas.Add(e.KeyCode);
                return;
            }
            if (e.KeyCode == Keys.Escape && encuentroActivo) { EscaparEntidad(); e.Handled = true; return; }
            if (e.KeyCode == Keys.E && !encuentroActivo)
            {
                IntentarCruzarPortal();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
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

        private static int DireccionDesdeMovimiento(float dx, float dy)
        {
            if (dx > 0 && dy < 0) return 1;
            if (dx > 0 && dy > 0) return 3;
            if (dx < 0 && dy > 0) return 5;
            if (dx < 0 && dy < 0) return 7;
            if (dx > 0) return 2;
            if (dy > 0) return 4;
            if (dx < 0) return 6;
            return 0;
        }

        private static int DireccionSprite(int direccion) => direccion switch
        {
            0 or 7 => 0,
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            _ => 2
        };

        private void ActualizarMovimiento()
        {
            if (!iniciado || !panelJuego.Visible) return;
            bool monstruosEnMovimiento = ActualizarMonstruos();
            if (encuentroActivo)
            {
                ActualizarResistencia(false);
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
                ActualizarResistencia(false);
                if (jugadorCaminando) { jugadorCaminando = false; panelMapa.Invalidate(); }
                ActualizarPortalCercano();
                bool inventarioCambio = ActualizarNucleosCercanos();
                inventarioCambio |= RecolectarCelulaCercana();
                if (inventarioCambio) ActualizarHUD();
                DetectarEntidad();
                if (monstruosEnMovimiento) panelMapa.Invalidate();
                return;
            }
            jugadorCaminando = true;
            direccionJugador = DireccionDesdeMovimiento(dx, dy);
            bool correr = TeclaShiftPresionada() && resistencia > 0;
            ActualizarResistencia(correr);
            float magnitud = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= magnitud; dy /= magnitud;
            float velocidad = energia > 0 ? (correr ? 7.2f : 4.2f) : 1.4f;
            float siguienteX = Math.Clamp(jugadorX + dx * velocidad, 80, 4920);
            if (!EstaDentroDeObstaculo(new PointF(siguienteX, jugadorY), 13)) jugadorX = siguienteX;
            float siguienteY = Math.Clamp(jugadorY + dy * velocidad, 80, 4920);
            if (!EstaDentroDeObstaculo(new PointF(jugadorX, siguienteY), 13)) jugadorY = siguienteY;
            ticksMovimiento++;
            if (ticksMovimiento % 30 == 0)
            {
                energia = Math.Max(0, energia - 1);
                conexion = Math.Max(0, conexion - 1);
                if (energia == 0)
                {
                    FinalizarMision("MISIÓN FALLIDA: EL CADETE SE QUEDÓ SIN ENERGÍA.", MessageBoxIcon.Error);
                    return;
                }
            }
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            ActualizarPortalCercano();
            ActualizarNucleosCercanos();
            RecolectarCelulaCercana();
            DetectarEntidad();
            ActualizarHUD();
            panelMapa.Invalidate();
        }

        private bool TeclaShiftPresionada() =>
            teclas.Contains(Keys.ShiftKey) || teclas.Contains(Keys.LShiftKey) || teclas.Contains(Keys.RShiftKey);

        private void ActualizarResistencia(bool corriendo)
        {
            resistencia = corriendo
                ? Math.Max(0, resistencia - 0.38f)
                : Math.Min(100, resistencia + 0.20f);
            int porcentaje = (int)MathF.Round(resistencia);
            if (porcentaje == resistenciaMostrada) return;
            resistenciaMostrada = porcentaje;
            lblVelocidad.Text = $"VELOCIDAD: {porcentaje}%";
            barraVelocidad.Value = porcentaje;
        }

        private bool ActualizarMonstruos()
        {
            bool seMovioAlguno = false;
            foreach (Entidad entidad in entidades.Where(e => !e.Derrotada && e != entidadActual))
            {
                float dxJugador = jugadorX - entidad.Posicion.X;
                float dyJugador = jugadorY - entidad.Posicion.Y;
                float distanciaJugador = MathF.Sqrt(dxJugador * dxJugador + dyJugador * dyJugador);
                if (distanciaJugador < 1650)
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
                if (!EstaDentroDeObstaculo(new PointF(x, anterior.Y), 12)) entidad.Posicion = new PointF(x, anterior.Y);
                float y = Math.Clamp(anterior.Y + dy, 80, 4920);
                if (!EstaDentroDeObstaculo(new PointF(entidad.Posicion.X, y), 12)) entidad.Posicion = new PointF(entidad.Posicion.X, y);
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

        private static bool EstaDentroDeObstaculo(PointF posicion, float radio)
        {
            if (EstaDentroDeUnaCasa(posicion, radio)) return true;
            const int tile = 64;
            int centroX = (int)MathF.Floor(posicion.X / tile);
            int centroY = (int)MathF.Floor(posicion.Y / tile);
            for (int ty = centroY - 1; ty <= centroY + 1; ty++)
            for (int tx = centroX - 1; tx <= centroX + 1; tx++)
            {
                int calleY = ModuloPositivo(ty, 13), calleX = ModuloPositivo(tx, 17);
                bool carretera = calleY <= 1 || calleX <= 1;
                bool acera = !carretera && (calleY == 2 || calleX == 2);
                if (carretera || acera) continue;
                int semilla = unchecked((tx * 73856093) ^ (ty * 19349663)) & int.MaxValue;
                if (semilla % 4 != 0) continue;
                float troncoX = tx * tile + 30, troncoY = ty * tile + 39;
                float dx = posicion.X - troncoX, dy = posicion.Y - troncoY;
                float radioArbol = radio + 23;
                if (dx * dx + dy * dy < radioArbol * radioArbol) return true;
            }
            return false;
        }

        private bool EstaEnRadioDeInteraccion(PointF objetivo, float radio)
        {
            float dx = objetivo.X - jugadorX;
            float dy = objetivo.Y - jugadorY;
            return dx * dx + dy * dy <= radio * radio;
        }

        private void ActualizarPortalCercano()
        {
            if (!portales.TryGetValue(realidad, out PointF portal)) return;
            bool cerca = EstaEnRadioDeInteraccion(portal, 105);
            if (cerca && !portalAvisoMostrado)
            {
                Mensaje("PORTAL OCULTO DETECTADO. PRESIONA E PARA CRUZAR A LA OTRA REALIDAD.");
                portalAvisoMostrado = true;
            }
            else if (!cerca)
            {
                portalAvisoMostrado = false;
            }
        }

        private void IntentarCruzarPortal()
        {
            if (!portales.TryGetValue(realidad, out PointF portal) || !EstaEnRadioDeInteraccion(portal, 105))
            {
                Mensaje("NO HAY UN PORTAL ALCANZABLE. SIGUE EXPLORANDO.");
                return;
            }

            string otraRealidad = realidad == "REALIDAD 102" ? "REALIDAD 999" : "REALIDAD 102";
            CambiarRealidad(otraRealidad);
        }

        private void DibujarPortal(Graphics g)
        {
            if (imagenPortal == null || !portales.TryGetValue(realidad, out PointF portal) ||
                !EstaEnRadioDeInteraccion(portal, 260)) return;

            float x = portal.X - camaraX;
            float y = portal.Y - camaraY;
            float pulso = 1f + MathF.Sin(fotograma * 0.13f) * 0.035f;
            float tamano = 142 * pulso;
            using (SolidBrush aura = new(Color.FromArgb(75, 255, 56, 36)))
                g.FillEllipse(aura, x - tamano * 0.63f, y - tamano * 0.57f, tamano * 1.26f, tamano * 1.14f);

            GraphicsState estado = g.Save();
            using (GraphicsPath recorte = new())
            {
                recorte.AddEllipse(x - tamano / 2, y - tamano / 2, tamano, tamano);
                g.SetClip(recorte);
                Rectangle destino = new((int)(x - tamano / 2), (int)(y - tamano / 2), (int)tamano, (int)tamano);
                Rectangle origen = new(330, 82, 370, 390);
                g.DrawImage(imagenPortal, destino, origen, GraphicsUnit.Pixel);
            }
            g.Restore(estado);
            using Pen borde = new(Color.FromArgb(220, 255, 77, 50), 3);
            g.DrawEllipse(borde, x - tamano / 2, y - tamano / 2, tamano, tamano);
        }

        private bool ActualizarNucleosCercanos()
        {
            if (naveReparada) return false;
            NucleoDimensional? recogido = nucleos.FirstOrDefault(n => n.Realidad == realidad &&
                !nucleosRecolectados.Contains(n.Id) &&
                Math.Abs(n.Posicion.X - jugadorX) < 32 && Math.Abs(n.Posicion.Y - jugadorY) < 32 &&
                EstaEnRadioDeInteraccion(n.Posicion, 48));
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
                EstaEnRadioDeInteraccion(e.Posicion, 88));
            if (encontrada == null) return;
            encuentroActivo = true; entidadActual = encontrada; teclas.Clear();
            lblMonstruo.Text = encontrada.Nombre;
            lblDetalleMonstruo.Text = $"{encontrada.Tipo}  //  VIDA: {encontrada.Vida}/{encontrada.VidaMaxima}";
            ActualizarImagenMonstruo(encontrada);
            barraVidaMonstruo.Value = 0;
            barraVidaMonstruo.Maximum = encontrada.VidaMaxima;
            barraVidaMonstruo.Value = encontrada.Vida;
            barraVidaCadeteCombate.Value = Math.Clamp(vida, 0, 100);
            lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100";
            btnAtacar.Enabled = true;
            btnEscapar.Enabled = true;
            animacionCombate = AnimacionCombate.Ninguna;
            panelEncuentro.Visible = true; panelEncuentro.BringToFront();
            Mensaje("ALERTA: UNA ENTIDAD TE HA DETECTADO.");
        }

        private void AtacarEntidad()
        {
            if (!encuentroActivo || entidadActual == null || animacionCombate != AnimacionCombate.Ninguna) return;
            entidadActual.Vida = Math.Max(0, entidadActual.Vida - 19);
            barraVidaMonstruo.Value = Math.Clamp(entidadActual.Vida, 0, barraVidaMonstruo.Maximum);
            lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}/{entidadActual.VidaMaxima}";
            imgMonstruo.Image = imagenAtaqueCadete ?? ObtenerImagenMonstruo(entidadActual);
            animacionCombate = AnimacionCombate.AtaqueCadete;
            btnAtacar.Enabled = false;
            btnEscapar.Enabled = false;
            relojAnimacionCombate.Stop();
            relojAnimacionCombate.Start();
        }

        private void CompletarAnimacionCombate()
        {
            relojAnimacionCombate.Stop();
            if (!encuentroActivo || entidadActual == null)
            {
                animacionCombate = AnimacionCombate.Ninguna;
                return;
            }

            if (animacionCombate == AnimacionCombate.AtaqueCadete)
            {
                if (entidadActual.Vida <= 0)
                {
                    Entidad monstruoDerrotado = entidadActual;
                    monstruoDerrotado.Derrotada = true;
                    encuentroActivo = false;
                    panelEncuentro.Visible = false;
                    ReaparecerEntidad(monstruoDerrotado);
                    entidadActual = null;
                    animacionCombate = AnimacionCombate.Ninguna;
                    energia = Math.Max(0, energia - 5);
                    if (energia == 0)
                    {
                        FinalizarMision("MISIÓN FALLIDA: EL CADETE SE QUEDÓ SIN ENERGÍA.", MessageBoxIcon.Error);
                        return;
                    }
                    Mensaje("ENTIDAD ELIMINADA. SECTOR DESPEJADO.");
                    ActualizarHUD();
                    panelMapa.Invalidate();
                    return;
                }

                vida = Math.Max(0, vida - 12);
                lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}/{entidadActual.VidaMaxima}";
                barraVidaCadeteCombate.Value = vida;
                lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100  //  {entidadActual.Nombre} TE ATACÓ (-12)";
                imgMonstruo.Image = imagenAtaqueMonstruo ?? ObtenerImagenMonstruo(entidadActual);
                animacionCombate = AnimacionCombate.AtaqueMonstruo;
                ActualizarHUD();
                relojAnimacionCombate.Start();
                return;
            }

            if (animacionCombate == AnimacionCombate.AtaqueMonstruo)
            {
                animacionCombate = AnimacionCombate.Ninguna;
                if (vida <= 0)
                {
                    FinalizarMision("EL CADETE HA CAÍDO. MISIÓN FALLIDA.", MessageBoxIcon.Error);
                    return;
                }
                ActualizarImagenMonstruo(entidadActual);
                btnAtacar.Enabled = true;
                btnEscapar.Enabled = true;
            }
            panelMapa.Invalidate();
        }

        private void EscaparEntidad()
        {
            relojAnimacionCombate.Stop();
            animacionCombate = AnimacionCombate.Ninguna;
            if (entidadActual != null)
                entidadActual.Posicion = new PointF(jugadorX + 220, jugadorY + 180);
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
                Color suelo = realidad == "REALIDAD 102" ? Color.FromArgb(43, 47, 39) : Color.FromArgb(54, 27, 30);
                Color asfalto = realidad == "REALIDAD 102" ? Color.FromArgb(49, 44, 43) : Color.FromArgb(48, 22, 28);
                Color pavimento = realidad == "REALIDAD 102" ? Color.FromArgb(79, 72, 65) : Color.FromArgb(91, 44, 48);
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
                    using Pen bordeAsfalto = new(Color.FromArgb(48, 12, 13, 16), 2);
                    using Pen grieta = new(Color.FromArgb(105, 12, 13, 15), 1);
                    using SolidBrush grava = new(Color.FromArgb(45, 165, 157, 145));
                    using SolidBrush marcaVial = new(Color.FromArgb(115, 181, 157, 111));
                    if (calleY <= 1)
                    {
                        g.DrawLine(bordeAsfalto, sx, sy + 3, sx + tile, sy + 3);
                        if (calleY == 0 && ModuloPositivo(tx, 4) == 0)
                            g.FillRectangle(marcaVial, sx + 12, sy + 31, 23, 3);
                        if (semilla % 5 == 0)
                        {
                            g.DrawLine(grieta, sx + 8, sy + 17, sx + 19, sy + 23);
                            g.DrawLine(grieta, sx + 19, sy + 23, sx + 16, sy + 37);
                        }
                    }
                    if (calleX <= 1)
                    {
                        g.DrawLine(bordeAsfalto, sx + 3, sy, sx + 3, sy + tile);
                        if (calleX == 0 && ModuloPositivo(ty, 4) == 0)
                            g.FillRectangle(marcaVial, sx + 31, sy + 12, 3, 23);
                        if (semilla % 6 == 0)
                        {
                            g.DrawLine(grieta, sx + 18, sy + 9, sx + 26, sy + 20);
                            g.DrawLine(grieta, sx + 26, sy + 20, sx + 39, sy + 17);
                        }
                    }
                    if (semilla % 9 == 0) g.FillRectangle(grava, sx + 17, sy + 43, 3, 2);
                }
                else if (semilla % 4 == 0)
                {
                    if (realidad == "REALIDAD 102") DibujarArbolHawkins(g, sx + 8, sy + 8);
                    else DibujarArbol(g, sx + 8, sy + 8);
                }
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
                if (realidad == "REALIDAD 102" && semilla % 5 == 0)
                    DibujarRaicesHawkins(g, sx, sy, semilla);
                if (realidad == "REALIDAD 102" && !carretera && !acera && semilla % 23 == 0)
                    DibujarCadaver(g, sx, sy, semilla);
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

            DibujarPortal(g);

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
            GraphicsState estadoCadete = g.Save();
            g.TranslateTransform(px, py - 9);
            if (direccionJugador % 2 == 1) g.RotateTransform(-45);
            g.DrawImage(spritesCadete[DireccionSprite(direccionJugador), frameCadete], -22, -29, 44, 58);
            g.Restore(estadoCadete);

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

            if (realidad == "REALIDAD 999")
            {
                using SolidBrush ambienteRojo = new(Color.FromArgb(34, 103, 9, 22));
                g.FillRectangle(ambienteRojo, 0, 0, panelMapa.ClientSize.Width, panelMapa.ClientSize.Height);
            }
            AplicarVisionDelCadete(g, px, py);
            DibujarFaroles(g, inicioX, inicioY, finX, finY);
            DibujarEsporas(g, px, py);
            using SolidBrush coords = new(Color.FromArgb(220, 255, 230, 230));
            g.DrawString($"X: {(int)jugadorX}   Y: {(int)jugadorY}   //   {realidad}", new Font("Consolas", 10, FontStyle.Bold), coords, 12, 12);
        }

        private void AplicarVisionDelCadete(Graphics g, float centroX, float centroY)
        {
            if (!iniciado) return;
            const float radioVision = 190;
            Color sombraAmbiente = realidad == "REALIDAD 999"
                ? Color.FromArgb(188, 10, 0, 7)
                : Color.FromArgb(184, 0, 0, 0);
            using GraphicsPath zonaVision = new();
            zonaVision.AddEllipse(centroX - radioVision, centroY - radioVision, radioVision * 2, radioVision * 2);
            using Region oscuridad = new(new Rectangle(0, 0, panelMapa.ClientSize.Width, panelMapa.ClientSize.Height));
            oscuridad.Exclude(zonaVision);
            using SolidBrush sombra = new(sombraAmbiente);
            g.FillRegion(sombra, oscuridad);

            SmoothingMode suavizado = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using PathGradientBrush degradado = new(zonaVision)
            {
                CenterColor = Color.FromArgb(0, 0, 0, 0),
                SurroundColors = new[] { sombraAmbiente },
                Blend = new Blend
                {
                    Positions = new[] { 0f, 0.54f, 1f },
                    Factors = new[] { 0f, 0f, 1f }
                }
            };
            g.FillPath(degradado, zonaVision);
            g.SmoothingMode = suavizado;
        }

        private void DibujarEsporas(Graphics g, float centroX, float centroY)
        {
            int ancho = Math.Max(1, panelMapa.ClientSize.Width);
            int alto = Math.Max(1, panelMapa.ClientSize.Height);
            bool realidadRojiza = realidad == "REALIDAD 999";

            for (int i = 0; i < 190; i++)
            {
                int semilla = unchecked((i * 7919 + 104729) & int.MaxValue);
                float profundidad = (semilla % 1000) / 999f;
                int baseX = semilla % ancho;
                int baseY = (semilla / 31) % alto;
                float velocidad = 1.3f + profundidad * 2.8f;
                float y = (baseY + fotograma * velocidad) % alto;
                float balanceo = MathF.Sin((fotograma * 0.11f + i) * 0.7f) * (2 + profundidad * 8);
                float x = (baseX + balanceo + ancho) % ancho;
                float dx = x - centroX, dy = y - centroY;
                float distancia = MathF.Sqrt(dx * dx + dy * dy);
                float visibilidad = distancia <= 100
                    ? 1f
                    : 1f - 0.68f * Math.Clamp((distancia - 100) / 260f, 0f, 1f);
                int alpha = Math.Clamp((int)((88 + profundidad * (realidadRojiza ? 165 : 150)) * visibilidad), 28, 238);
                int tamano = 1 + (int)(profundidad * 4.5f);
                int largo = 3 + (int)(profundidad * 9f);
                Color esporaColor = realidadRojiza
                    ? Color.FromArgb(alpha, 255, 38 + (i % 3) * 15, 42)
                    : Color.FromArgb(alpha, 55 + (i % 3) * 12, 205, 82 + (i % 3) * 18);
                using Pen estela = new(esporaColor, tamano);
                g.DrawLine(estela, x, y, x - balanceo * 0.08f, y + largo);
                if (profundidad > 0.66f)
                {
                    using SolidBrush brillo = new(realidadRojiza
                        ? Color.FromArgb(Math.Min(245, alpha + 28), 255, 92, 76)
                        : Color.FromArgb(Math.Min(245, alpha + 28), 116, 255, 135));
                    g.FillEllipse(brillo, x - tamano, y - tamano, tamano * 2, tamano * 2);
                }
            }
        }

        private static void DibujarCadaver(Graphics g, int x, int y, int semilla)
        {
            int giro = semilla % 4;
            using SolidBrush sombra = new(Color.FromArgb(155, 0, 0, 0));
            using SolidBrush ropa = new(Color.FromArgb(220, 28 + giro * 5, 31, 34));
            using SolidBrush piel = new(Color.FromArgb(210, 105, 87, 69));
            using SolidBrush sangre = new(Color.FromArgb(155, 76, 9, 17));
            g.FillEllipse(sombra, x + 12, y + 34, 38, 13);
            g.FillRectangle(ropa, x + 21, y + 17, 23, 25);
            g.FillRectangle(piel, x + 29, y + 8, 12, 11);
            g.FillRectangle(ropa, x + 13, y + 20, 10, 17);
            g.FillRectangle(ropa, x + 41, y + 20, 9, 16);
            g.FillRectangle(ropa, x + 23, y + 39, 8, 12);
            g.FillRectangle(ropa, x + 36, y + 39, 8, 12);
            g.FillRectangle(sangre, x + 15 + giro * 3, y + 29, 8, 5);
            g.FillRectangle(sangre, x + 38, y + 13 + giro, 9, 4);
            using Pen detalle = new(Color.FromArgb(115, 8, 9, 12), 2);
            g.DrawLine(detalle, x + 27, y + 20, x + 39, y + 36);
        }

        private void DibujarFaroles(Graphics g, int inicioX, int inicioY, int finX, int finY)
        {
            bool roja = realidad == "REALIDAD 999";
            for (int ty = inicioY; ty <= finY; ty++)
            for (int tx = inicioX; tx <= finX; tx++)
            {
                int cy = ModuloPositivo(ty, 13), cx = ModuloPositivo(tx, 17);
                bool horizontal = cy == 2 && ModuloPositivo(tx, 6) == 3;
                bool vertical = cx == 2 && ModuloPositivo(ty, 6) == 3;
                if (!horizontal && !vertical) continue;
                float sx = tx * 64 - camaraX + (vertical ? 44 : 18);
                float sy = ty * 64 - camaraY + (horizontal ? 17 : 44);
                float pulso = 0.35f + 0.65f * (0.5f + 0.5f * MathF.Sin(fotograma * 0.31f + tx * 1.7f + ty));
                if ((fotograma + tx * 7 + ty * 11) % 97 < 4) pulso *= 0.16f;
                Color luz = roja ? Color.FromArgb(255, 74, 40) : Color.FromArgb(166, 218, 133);
                int a = (int)(48 + pulso * 92);
                using SolidBrush haloGrande = new(Color.FromArgb(a / 3, luz));
                using SolidBrush haloMedio = new(Color.FromArgb(a / 2, luz));
                using SolidBrush haloCentro = new(Color.FromArgb(a, luz));
                using SolidBrush poste = new(Color.FromArgb(205, 20, 19, 22));
                using SolidBrush foco = new(Color.FromArgb((int)(160 + pulso * 95), 255, 225, 165));
                g.FillEllipse(haloGrande, sx - 43, sy - 36, 86, 72);
                g.FillEllipse(haloMedio, sx - 27, sy - 23, 54, 46);
                g.FillEllipse(haloCentro, sx - 13, sy - 12, 26, 24);
                g.FillRectangle(poste, sx - 2, sy + 3, 4, 17);
                g.FillRectangle(poste, sx - 7, sy - 4, 14, 5);
                g.FillRectangle(foco, sx - 3, sy - 5, 6, 3);
                using Pen reflejo = new(Color.FromArgb((int)(35 + pulso * 70), luz), 2);
                g.DrawLine(reflejo, sx, sy + 18, sx + 10, sy + 35);
            }
        }

        private static void DibujarRaicesHawkins(Graphics g, int x, int y, int semilla)
        {
            int px = 5 + semilla % 27;
            int py = 5 + semilla / 17 % 27;
            Point[] raiz =
            {
                new(x + px, y + py), new(x + px - 7, y + py - 5),
                new(x + px - 12, y + py - 2), new(x + px - 18, y + py - 9),
                new(x + px - 24, y + py - 7)
            };
            using Pen sombra = new(Color.FromArgb(190, 23, 8, 16), 5);
            using Pen carne = new(Color.FromArgb(172, 104, 32, 39), 2);
            using Pen brote = new(Color.FromArgb(165, 139, 44, 47), 1);
            g.DrawLines(sombra, raiz);
            g.DrawLines(carne, raiz);
            g.DrawLine(brote, raiz[2], new Point(raiz[2].X - 3, raiz[2].Y + 6));
            g.DrawLine(brote, raiz[3], new Point(raiz[3].X + 2, raiz[3].Y - 6));
            using SolidBrush espora = new(Color.FromArgb(210, 164, 50, 54));
            g.FillRectangle(espora, x + (semilla / 7 % 52), y + (semilla / 19 % 52), 2, 2);
        }

        private static void DibujarArbolHawkins(Graphics g, int x, int y)
        {
            using Pen ramaOscura = new(Color.FromArgb(35, 16, 20), 5);
            using Pen rama = new(Color.FromArgb(91, 42, 39), 2);
            Point centro = new(x + 27, y + 27);
            Point[] tronco = { new(x + 28, y + 50), centro, new(x + 20, y + 12), new(x + 12, y + 4) };
            g.DrawLines(ramaOscura, tronco);
            g.DrawLines(rama, tronco);
            g.DrawLine(ramaOscura, centro, new Point(x + 42, y + 14));
            g.DrawLine(rama, centro, new Point(x + 42, y + 14));
            g.DrawLine(ramaOscura, new Point(x + 21, y + 20), new Point(x + 7, y + 16));
            g.DrawLine(rama, new Point(x + 21, y + 20), new Point(x + 7, y + 16));
            using SolidBrush hojas = new(Color.FromArgb(95, 48, 25, 32));
            using SolidBrush rojo = new(Color.FromArgb(112, 89, 35, 39));
            g.FillEllipse(hojas, x + 2, y + 1, 16, 13);
            g.FillEllipse(hojas, x + 32, y + 5, 16, 14);
            g.FillEllipse(rojo, x + 10, y + 2, 8, 6);
        }

        private void DibujarArbol(Graphics g, int x, int y)
        {
            using SolidBrush tronco = new(Color.FromArgb(38, 15, 20));
            using SolidBrush copa = new(realidad == "REALIDAD 102" ? Color.FromArgb(15, 43, 29) : Color.FromArgb(63, 13, 24));
            using SolidBrush brillo = new(realidad == "REALIDAD 102" ? Color.FromArgb(67, 81, 44) : Color.FromArgb(165, 35, 43));
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
            Color lawn = realidad == "REALIDAD 102" ? Color.FromArgb(65 + variante * 5, 77, 48) : Color.FromArgb(58 + variante * 5, 30, 31);
            Color roof = realidad == "REALIDAD 102"
                ? new[] { Color.FromArgb(69, 55, 48), Color.FromArgb(54, 61, 60), Color.FromArgb(77, 57, 48) }[variante]
                : new[] { Color.FromArgb(59, 27, 31), Color.FromArgb(72, 28, 34), Color.FromArgb(48, 25, 32) }[variante];
            Color window = realidad == "REALIDAD 102" ? Color.FromArgb(107, 171, 183) : Color.FromArgb(220, 77, 72);

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
            Color tierra = realidad == "REALIDAD 102" ? Color.FromArgb(76 + variante * 4, 76, 45) : Color.FromArgb(72 + variante * 5, 34, 38);
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
            if (energia == 0)
            {
                FinalizarMision("MISIÓN FALLIDA: EL CADETE SE QUEDÓ SIN ENERGÍA.", MessageBoxIcon.Error);
                return;
            }
            Mensaje("ESCANEO COMPLETADO. ACTIVIDAD DIMENSIONAL DETECTADA.");
        }

        private void IniciarMusica()
        {
            if (musicaFondo == null) return;
            string archivoAudio = Path.Combine(AppContext.BaseDirectory, "Resources", "AudioNexus.mp3");
            if (!File.Exists(archivoAudio))
            {
                Mensaje("NO SE ENCONTRÓ EL AUDIO DE LA MISIÓN.");
                return;
            }

            musicaActiva = true;
            musicaFondo.Open(new Uri(archivoAudio, UriKind.Absolute));
        }

        private void DetenerMusica()
        {
            musicaActiva = false;
            if (musicaFondo == null) return;
            musicaFondo.Stop();
            musicaFondo.Close();
        }

        private void FinalizarMision(string mensaje, MessageBoxIcon icono)
        {
            if (misionTerminada) return;
            misionTerminada = true;
            iniciado = false;
            encuentroActivo = false;
            entidadActual = null;
            teclas.Clear();
            relojMovimiento.Stop();
            relojAnimacionCombate.Stop();
            animacionCombate = AnimacionCombate.Ninguna;
            panelEncuentro.Visible = false;
            DetenerMusica();
            ActualizarHUD();
            Mensaje(mensaje);
            MessageBox.Show(this, mensaje, "NEXUS", MessageBoxButtons.OK, icono);
            Close();
        }

        private void RecuperarEnergia()
        {
            if (recuperacionUsada)
            {
                Mensaje("LA RECUPERACIÓN DE ENERGÍA YA SE UTILIZÓ EN ESTA MISIÓN.");
                return;
            }
            recuperacionUsada = true;
            btnRecuperar.Enabled = false;
            btnRecuperar.Text = "RECUPERACIÓN USADA";
            energia = Math.Min(100, energia + 20); conexion = Math.Min(100, conexion + 10);
            ActualizarHUD(); Mensaje("SE RECUPERARON 20 PUNTOS DE ENERGÍA.");
        }

        private void AjustarPanelEncuentro()
        {
            if (panelEncuentro == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            int ancho = ClientSize.Width;
            int alto = ClientSize.Height;
            int margen = Math.Clamp(Math.Min(ancho, alto) / 32, 18, 42);
            panelEncuentro.SetBounds(0, 0, ancho, alto);
            lblMonstruo.SetBounds(margen, 18, ancho - margen * 2, 48);
            lblMonstruo.Font = new Font("Consolas", Math.Clamp(alto / 38f, 20f, 34f), FontStyle.Bold);

            int yBotones = alto - margen - 58;
            int anchoBarra = Math.Min(680, ancho - margen * 2);
            int xBarra = (ancho - anchoBarra) / 2;
            barraVidaCadeteCombate.SetBounds(xBarra, yBotones - 39, anchoBarra, 20);
            lblVidaCadeteCombate.SetBounds(margen, yBotones - 68, ancho - margen * 2, 24);
            barraVidaMonstruo.SetBounds(xBarra, yBotones - 104, anchoBarra, 20);
            lblDetalleMonstruo.SetBounds(margen, yBotones - 133, ancho - margen * 2, 24);

            int yImagen = 76;
            int altoImagen = Math.Max(160, lblDetalleMonstruo.Top - yImagen - 8);
            int anchoImagen = Math.Min(ancho - margen * 2, (int)(altoImagen * 1.83f));
            imgMonstruo.SetBounds((ancho - anchoImagen) / 2, yImagen, anchoImagen, altoImagen);

            int espacioBotones = 26;
            int anchoBoton = Math.Min(260, (ancho - margen * 2 - espacioBotones) / 2);
            int xPrimerBoton = (ancho - (anchoBoton * 2 + espacioBotones)) / 2;
            btnAtacar.SetBounds(xPrimerBoton, yBotones, anchoBoton, 48);
            btnEscapar.SetBounds(xPrimerBoton + anchoBoton + espacioBotones, yBotones, anchoBoton, 48);
        }

        private void UsarCelula()
        {
            if (celulasEnergia <= 0) { Mensaje("NO TIENES CÉLULAS DE ENERGÍA."); return; }
            celulasEnergia--;
            energia = Math.Min(100, energia + 30);
            ActualizarHUD();
            Mensaje("CÉLULA UTILIZADA. +30 ENERGÍA.");
        }

        private void CambiarRealidad(string nuevaRealidad)
        {
            if (nuevaRealidad == realidad || !portales.TryGetValue(nuevaRealidad, out PointF portalDestino)) return;
            encuentroActivo = false;
            entidadActual = null;
            panelEncuentro.Visible = false;
            teclas.Clear();
            jugadorCaminando = false;
            realidad = nuevaRealidad;
            jugadorX = Math.Clamp(portalDestino.X + (nuevaRealidad == "REALIDAD 999" ? 160 : -160), 80, 4920);
            jugadorY = portalDestino.Y;
            portalAvisoMostrado = false;
            camaraX = jugadorX - panelMapa.Width / 2f; camaraY = jugadorY - panelMapa.Height / 2f;
            CrearEntidades();
            ActualizarHUD(); panelMapa.Invalidate();
            Mensaje("CRUCE POR PORTAL COMPLETADO: " + realidad + ". USA W, A, S Y D PARA EXPLORAR.");
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
                ? $"NAVE LISTA: {nucleosRecolectados.Count}/{nucleosNecesarios}"
                : $"NÚCLEOS: {nucleosRecolectados.Count}/{nucleosNecesarios}";
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

