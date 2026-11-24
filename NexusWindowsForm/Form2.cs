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
        private readonly Dictionary<string, List<PointF>> garrafasEnergia = new(StringComparer.Ordinal)
        {
            ["REALIDAD 102"] = new() { new(1700, 1200), new(3900, 1700), new(4200, 3600), new(1100, 4100) },
            ["REALIDAD 999"] = new() { new(900, 1200), new(1900, 2800), new(3300, 3900), new(4500, 1500) }
        };
        private readonly Dictionary<string, List<PointF>> celulasMedicas = new(StringComparer.Ordinal)
        {
            ["REALIDAD 102"] = new() { new(2600, 2850), new(3600, 3000), new(1250, 1650) },
            ["REALIDAD 999"] = new() { new(700, 3200), new(2450, 900), new(4200, 4150) }
        };
        private readonly List<NucleoDimensional> nucleos = new()
        {
            new("102-A", "REALIDAD 102", new PointF(2200, 2600)),
            new("102-B", "REALIDAD 102", new PointF(3400, 3328)),
            new("999-A", "REALIDAD 999", new PointF(1152, 1024)),
            new("999-B", "REALIDAD 999", new PointF(3456, 2496))
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
        private readonly Bitmap[,] spritesCadete = new Bitmap[4, 2];
        private const int nucleosNecesarios = 4;

        private string nombreExplorador = "";
        private int edad;
        private int vida = 100, energia = 100, conexion = 100;
        private float jugadorX = 2500, jugadorY = 2500;
        private float camaraX, camaraY;
        private bool iniciado, encuentroActivo, naveReparada, recuperacionUsada, musicaActiva, misionTerminada;
        private bool portalAvisoMostrado;
        private MediaPlayer? musicaFondo;
        private MediaPlayer? sonidoPasos;
        private MediaPlayer? sonidoAlerta;
        private ReproductorAudioBucle? musicaCinematica;
        private bool alertaEnergiaActiva, alertaVidaActiva;
        private int ticksSonidoPaso;
        private Bitmap? imagenMonstruoHerido, imagenMonstruoCritico;
        private Bitmap? imagenZombieNormal, imagenZombieTransformado;
        private Bitmap? imagenPortal;
        private Bitmap? imagenArenaCombate, imagenCadeteCombate1, imagenCadeteCombate2, imagenMonstruoArena;
        private string realidad = "REALIDAD 102";
        private Entidad? entidadActual;
        private int fotograma;
        private int ticksMovimiento;
        private int celulasVida;
        private int direccionJugador = 4;
        private float resistencia = 100f;
        private int resistenciaMostrada = -1;
        private bool jugadorCaminando;
        private readonly List<ProyectilCombate> proyectilesCombate = new();
        private PointF posicionCadeteCombate, posicionMonstruoCombate;
        private DateTime monstruoAturdidoHasta, siguienteAtaqueMonstruo, finAnticipacionMonstruo;
        private DateTime finGolpeMonstruo, cadeteAturdidoHasta, saltoHasta, siguienteDisparo;
        private bool mirandoDerecha = true, ataqueMonstruoPendiente;

        private sealed class ProyectilCombate(float x, float y, int direccion)
        {
            public float X = x;
            public float Y = y;
            public int Direccion = direccion;
        }

        private sealed record NucleoDimensional(string Id, string Realidad, PointF Posicion);

        private sealed class Entidad
        {
            public string Nombre { get; init; } = "";
            public string Tipo { get; init; } = "";
            public PointF Posicion { get; set; }
            public int Vida { get; set; } = 200;
            public int VidaMaxima { get; init; } = 200;
            public Bitmap Sprite { get; init; } = null!;
            public PointF DestinoMovimiento { get; set; }
            public float Velocidad { get; init; } = 1.1f;
            public bool Derrotada { get; set; }
            public bool EsZombie { get; init; }
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
                    sonidoPasos?.Stop();
                    sonidoAlerta?.Stop();
                    sonidoPasos?.Close();
                    sonidoAlerta?.Close();
                    musicaCinematica?.Dispose();
                    relojMovimiento.Stop();
                    relojEscena.Stop();
                };
                FormClosed += (_, _) =>
                {
                    imgMonstruo.Image = null;
                    imagenMonstruoHerido?.Dispose();
                    imagenMonstruoCritico?.Dispose();
                    imagenZombieNormal?.Dispose();
                    imagenZombieTransformado?.Dispose();
                    imagenPortal?.Dispose();
                    imagenArenaCombate?.Dispose();
                    imagenCadeteCombate1?.Dispose();
                    imagenCadeteCombate2?.Dispose();
                    imagenMonstruoArena?.Dispose();
                    relojMovimiento.Dispose();
                    relojEscena.Dispose();
                };
                CargarImagenesCombate();
                InicializarEfectosSonido();
                CrearEscenasPixelArt();
                CrearEntidades();
                CrearSpritesCadete();
                panelMapa.Paint += DibujarMundo;
                panelEncuentro.Paint += DibujarArenaCombate;
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
            Button[] botones = { btnAnalizar, btnRecuperar, btnUsarCelula, btnVolver };
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

        private void InicializarEfectosSonido()
        {
            try
            {
                string carpeta = Path.Combine(Path.GetTempPath(), "NEXUS_V02_Audio");
                Directory.CreateDirectory(carpeta);
                string pasos = Path.Combine(carpeta, "pasos.wav");
                string alerta = Path.Combine(carpeta, "alerta.wav");

                if (!File.Exists(pasos))
                    CrearWavEfecto(pasos, 0.12, t =>
                    {
                        double envolvente = Math.Exp(-t * 24);
                        double golpe = Math.Sin(2 * Math.PI * (95 - 25 * t) * t);
                        double ruido = Random.Shared.NextDouble() * 2 - 1;
                        return (golpe * 0.62 + ruido * 0.12) * envolvente;
                    });

                if (!File.Exists(alerta))
                    CrearWavEfecto(alerta, 0.62, t =>
                    {
                        bool pulso = (t < 0.16 || (t > 0.29 && t < 0.45));
                        if (!pulso) return 0;
                        double frecuencia = t < 0.16 ? 880 : 660;
                        double local = t < 0.16 ? t : t - 0.29;
                        double envolvente = Math.Min(1, local * 35) * Math.Max(0, 1 - local * 5);
                        return Math.Sin(2 * Math.PI * frecuencia * local) * 0.42 * envolvente;
                    });

                sonidoPasos = new MediaPlayer { Volume = 0.34 };
                sonidoPasos.Open(new Uri(pasos, UriKind.Absolute));
                sonidoAlerta = new MediaPlayer { Volume = 0.68 };
                sonidoAlerta.Open(new Uri(alerta, UriKind.Absolute));
            }
            catch
            {
                sonidoPasos = null;
                sonidoAlerta = null;
            }
        }

        private static void CrearWavEfecto(string ruta, double duracion, Func<double, double> formaOnda)
        {
            const int frecuenciaMuestreo = 22050;
            int cantidadMuestras = (int)(frecuenciaMuestreo * duracion);
            using FileStream archivo = File.Create(ruta);
            using BinaryWriter escritor = new(archivo);
            int bytesDatos = cantidadMuestras * sizeof(short);

            escritor.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            escritor.Write(36 + bytesDatos);
            escritor.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            escritor.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            escritor.Write(16);
            escritor.Write((short)1);
            escritor.Write((short)1);
            escritor.Write(frecuenciaMuestreo);
            escritor.Write(frecuenciaMuestreo * sizeof(short));
            escritor.Write((short)sizeof(short));
            escritor.Write((short)16);
            escritor.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            escritor.Write(bytesDatos);

            for (int i = 0; i < cantidadMuestras; i++)
            {
                double t = (double)i / frecuenciaMuestreo;
                short muestra = (short)(Math.Clamp(formaOnda(t), -1, 1) * short.MaxValue);
                escritor.Write(muestra);
            }
        }

        private static void ReproducirEfecto(MediaPlayer? reproductor)
        {
            if (reproductor == null) return;
            reproductor.Position = TimeSpan.Zero;
            reproductor.Play();
        }

        private void CrearEscenasPixelArt()
        {
            imgPueblo.Image = Properties.Resources.AmbienteHawkins;
        }

        private void CargarImagenesCombate()
        {
            imagenMonstruoHerido = CargarImagen("DemogorgonHerido.jpg");
            imagenMonstruoCritico = CargarImagen("DemogorgonCritico.jpg");
            imagenZombieNormal = CargarImagen("Zombie102_Normal.png");
            imagenZombieTransformado = CargarImagen("Zombie102_Transformado.png");
            imagenPortal = CargarImagen("PortalDimensional.png");
            imagenArenaCombate = CargarImagen("ArenaCombateHawkins.jpg");
            imagenCadeteCombate1 = CargarImagen("CadeteArena1.png");
            imagenCadeteCombate2 = CargarImagen("CadeteArena2.png");
            imagenMonstruoArena = CargarImagen("DemogorgonArena.png");
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
            // El zombie y sus dos estados visuales pertenecen solamente a la Realidad 102.
            if (entidad.EsZombie)
            {
                if ((long)entidad.Vida * 100 < (long)entidad.VidaMaxima * 60 &&
                    imagenZombieTransformado != null)
                    return imagenZombieTransformado;

                return imagenZombieNormal ?? Properties.Resources.MonstruoMapa;
            }

            // La Realidad 999 conserva las imágenes originales del monstruo.
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

        private Bitmap CrearSpriteMonstruo(bool esZombie)
        {
            Image imagen = esZombie && imagenZombieNormal != null
                ? imagenZombieNormal
                : Properties.Resources.MonstruoMapa;
            return new Bitmap(imagen);
        }

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

            // Ocho entidades móviles por realidad. En 102 son zombies;
            // en 999 se mantiene el monstruo dimensional original.
            bool esZombie = realidad == "REALIDAD 102";
            for (int i = 0; i < 8; i++)
            {
                PointF posicion = ObtenerPosicionSegura(520);
                const int vidaMonstruo = 200;
                entidades.Add(new Entidad
                {
                    Nombre = esZombie ? "ZOMBIE ERRANTE" : nombres[i],
                    Tipo = esZombie ? "Infectado" : tipos[i % tipos.Length],
                    Posicion = posicion,
                    DestinoMovimiento = new PointF(posicion.X + Random.Shared.Next(-240, 241),
                                                   posicion.Y + Random.Shared.Next(-240, 241)),
                    Velocidad = 1.0f + (i % 3) * 0.22f,
                    Sprite = CrearSpriteMonstruo(esZombie),
                    EsZombie = esZombie,
                    Vida = vidaMonstruo,
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
            celulasVida = 0;
            alertaEnergiaActiva = false;
            alertaVidaActiva = false;
            ticksSonidoPaso = 0;
            btnRecuperar.Enabled = true;
            btnRecuperar.Text = "RECUPERAR ENERGÍA";
            nucleosRecolectados.Clear();
            realidad = selector.RealidadSeleccionada;
            ColocarPortalesAleatorios();
            jugadorX = 2500; jugadorY = 2500;

            iniciado = true;
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelRegistro.Visible = false; panelJuego.Visible = true; panelJuego.BringToFront(); lblEncabezado.BringToFront();
            AjustarLayout();
            relojMovimiento.Start();
            IniciarMusica();
            ActualizarHUD(); Mensaje("MISIÓN ACTIVA: RECUPERA 4 NÚCLEOS Y REPARA TU NAVE.");
            panelMapa.Focus();
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            // En el registro las letras WASD se escriben normalmente en el nombre.
            if (!iniciado) return;
            if (e.KeyCode is Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey)
            {
                teclas.Add(e.KeyCode);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (encuentroActivo)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    EscaparEntidad();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Space)
                {
                    if (teclas.Add(Keys.Space)) IniciarSaltoCombate();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.R)
                {
                    if (teclas.Add(Keys.R)) DispararProyectilCombate();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Up or Keys.Down or Keys.Left or Keys.Right)
                {
                    teclas.Add(e.KeyCode);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }
            if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; return; }
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
            if (encuentroActivo)
            {
                panelEncuentro.Invalidate();
                return;
            }
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
            if (encuentroActivo)
            {
                bool moviendoEnCombate = teclas.Any(k => k is Keys.W or Keys.A or Keys.S or Keys.D or
                    Keys.Up or Keys.Down or Keys.Left or Keys.Right);
                bool correrEnCombate = moviendoEnCombate && TeclaShiftPresionada() && resistencia > 0 && energia > 0;
                ActualizarResistencia(correrEnCombate);
                ActualizarArenaCombate();
                return;
            }
            bool monstruosEnMovimiento = ActualizarMonstruos();

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
                inventarioCambio |= RecolectarGarrafaCercana();
                if (inventarioCambio) ActualizarHUD();
                DetectarEntidad();
                if (monstruosEnMovimiento) panelMapa.Invalidate();
                return;
            }
            jugadorCaminando = true;
            direccionJugador = DireccionDesdeMovimiento(dx, dy);
            bool correr = TeclaShiftPresionada() && resistencia > 0 && energia > 0;
            ActualizarResistencia(correr);
            float magnitud = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= magnitud; dy /= magnitud;
            float velocidad = energia > 0 ? (correr ? 7.2f : 4.2f) : 1.4f;
            float siguienteX = Math.Clamp(jugadorX + dx * velocidad, 80, 4920);
            if (!EstaDentroDeObstaculo(new PointF(siguienteX, jugadorY), 13)) jugadorX = siguienteX;
            float siguienteY = Math.Clamp(jugadorY + dy * velocidad, 80, 4920);
            if (!EstaDentroDeObstaculo(new PointF(jugadorX, siguienteY), 13)) jugadorY = siguienteY;
            ticksMovimiento++;
            ticksSonidoPaso++;
            if (ticksSonidoPaso >= 20)
            {
                ticksSonidoPaso = 0;
                ReproducirEfecto(sonidoPasos);
            }
            if (ticksMovimiento % 30 == 0)
            {
                energia = Math.Max(0, energia - 1);
                conexion = Math.Max(0, conexion - 1);
            }
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            ActualizarPortalCercano();
            ActualizarNucleosCercanos();
            RecolectarCelulaCercana();
            RecolectarGarrafaCercana();
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

        private void ColocarPortalesAleatorios()
        {
            PointF[] portalesElegidos = new PointF[portales.Count];
            int indicePortal = 0;
            foreach (string realidadPortal in portales.Keys.ToArray())
            {
                PointF candidato = default;
                bool valido = false;
                for (int intento = 0; intento < 500 && !valido; intento++)
                {
                    candidato = new PointF(Random.Shared.Next(320, 4680), Random.Shared.Next(320, 4680));
                    float dxInicio = candidato.X - 2500, dyInicio = candidato.Y - 2500;
                    if (dxInicio * dxInicio + dyInicio * dyInicio < 850 * 850) continue;
                    if (portalesElegidos.Take(indicePortal).Any(p =>
                    {
                        float dx = p.X - candidato.X, dy = p.Y - candidato.Y;
                        return dx * dx + dy * dy < 1500 * 1500;
                    })) continue;
                    if (nucleos.Where(n => n.Realidad == realidadPortal).Any(n =>
                    {
                        float dx = n.Posicion.X - candidato.X, dy = n.Posicion.Y - candidato.Y;
                        return dx * dx + dy * dy < 240 * 240;
                    })) continue;
                    if (EstaDentroDeObstaculo(candidato, 28) || !TieneSalidaLibreDelPortal(candidato)) continue;
                    valido = true;
                }

                if (!valido) candidato = new PointF(4300 - indicePortal * 2100, 4300 - indicePortal * 1300);
                portales[realidadPortal] = candidato;
                portalesElegidos[indicePortal++] = candidato;
            }
        }

        private static bool TieneSalidaLibreDelPortal(PointF portal)
        {
            PointF[] salidas =
            {
                new(portal.X + 180, portal.Y), new(portal.X - 180, portal.Y),
                new(portal.X, portal.Y + 180), new(portal.X, portal.Y - 180)
            };
            return salidas.Any(p => p.X >= 80 && p.X <= 4920 && p.Y >= 80 && p.Y <= 4920 &&
                !EstaDentroDeObstaculo(p, 12));
        }

        private static PointF PosicionEntradaDelPortal(PointF portal)
        {
            PointF[] salidas =
            {
                new(portal.X + 180, portal.Y), new(portal.X - 180, portal.Y),
                new(portal.X, portal.Y + 180), new(portal.X, portal.Y - 180)
            };
            return salidas.FirstOrDefault(p => p.X >= 80 && p.X <= 4920 && p.Y >= 80 && p.Y <= 4920 &&
                !EstaDentroDeObstaculo(p, 12), portal);
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
                Mensaje("PORTAL A LA VISTA. PRESIONA E PARA CRUZAR A LA OTRA REALIDAD.");
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
            if (imagenPortal == null || !portales.TryGetValue(realidad, out PointF portal)) return;

            float x = portal.X - camaraX;
            float y = portal.Y - camaraY;
            float pulso = 1f + MathF.Sin(fotograma * 0.13f) * 0.035f;
            float tamano = 142 * pulso;
            if (x < -tamano || y < -tamano || x > panelMapa.ClientSize.Width + tamano ||
                y > panelMapa.ClientSize.Height + tamano) return;
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
            if (!celulasMedicas.TryGetValue(realidad, out List<PointF>? objetos)) return false;
            int indice = objetos.FindIndex(p => Math.Abs(p.X - jugadorX) < 38 && Math.Abs(p.Y - jugadorY) < 38);
            if (indice < 0) return false;

            objetos.RemoveAt(indice);
            celulasVida++;
            Mensaje("OBJETO RECOGIDO: CÉLULA MÉDICA. USA 'USAR CÉLULA' PARA RECUPERAR VIDA.");
            return true;
        }

        private bool RecolectarGarrafaCercana()
        {
            if (!garrafasEnergia.TryGetValue(realidad, out List<PointF>? objetos)) return false;
            int indice = objetos.FindIndex(p => Math.Abs(p.X - jugadorX) < 42 && Math.Abs(p.Y - jugadorY) < 42);
            if (indice < 0) return false;

            objetos.RemoveAt(indice);
            energia = Math.Min(100, energia + 60);
            ActualizarHUD();
            Mensaje("GARRAFA DE ENERGÍA RECOGIDA: +60 ENERGÍA.");
            return true;
        }

        private void DetectarEntidad()
        {
            Entidad? encontrada = entidades.FirstOrDefault(e => !e.Derrotada &&
                EstaEnRadioDeInteraccion(e.Posicion, 88));
            if (encontrada == null) return;
            encuentroActivo = true; entidadActual = encontrada; teclas.Clear();
            lblMonstruo.Text = $"ENCUENTRO // {encontrada.Nombre}";
            lblDetalleMonstruo.Text = $"{encontrada.Tipo}  //  VIDA: {encontrada.Vida}/{encontrada.VidaMaxima}";
            barraVidaMonstruo.Value = 0;
            barraVidaMonstruo.Maximum = encontrada.VidaMaxima;
            barraVidaMonstruo.Value = encontrada.Vida;
            barraVidaCadeteCombate.Value = Math.Clamp(vida, 0, 100);
            lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100";
            panelEncuentro.BackgroundImage = imagenArenaCombate;
            panelEncuentro.BackgroundImageLayout = ImageLayout.Stretch;
            imgMonstruo.Visible = false;
            btnAtacar.Text = "DISPARAR  [R]";
            btnEscapar.Text = "ESCAPAR  [ESC]";
            btnAtacar.Enabled = true;
            btnEscapar.Enabled = true;
            proyectilesCombate.Clear();
            ataqueMonstruoPendiente = false;
            monstruoAturdidoHasta = DateTime.MinValue;
            finGolpeMonstruo = DateTime.MinValue;
            cadeteAturdidoHasta = DateTime.MinValue;
            saltoHasta = DateTime.MinValue;
            DateTime ahora = DateTime.UtcNow;
            siguienteAtaqueMonstruo = ahora.AddMilliseconds(850);
            siguienteDisparo = DateTime.MinValue;
            mirandoDerecha = true;
            AjustarPanelEncuentro();
            RectangleF arena = LimitesArenaCombate();
            posicionCadeteCombate = new PointF(arena.Left + 115, arena.Bottom - 10);
            posicionMonstruoCombate = new PointF(arena.Right - 145, arena.Bottom - 10);
            panelEncuentro.Visible = true; panelEncuentro.BringToFront();
            panelEncuentro.Focus();
            Mensaje("ENCUENTRO: WASD MOVER  //  R DISPARAR  //  ESPACIO SALTAR  //  SHIFT CORRER.");
            panelEncuentro.Invalidate();
        }

        private void AtacarEntidad()
        {
            DispararProyectilCombate();
        }

        private RectangleF LimitesArenaCombate()
        {
            float ancho = Math.Max(1, panelEncuentro.ClientSize.Width);
            float alto = Math.Max(1, panelEncuentro.ClientSize.Height);
            float margenX = Math.Clamp(ancho * 0.045f, 28f, 76f);
            float superior = Math.Max(132f, alto * 0.19f);
            float inferior = Math.Max(superior + 220f, alto - 185f);
            return RectangleF.FromLTRB(margenX, superior, ancho - margenX, inferior);
        }

        private void IniciarSaltoCombate()
        {
            DateTime ahora = DateTime.UtcNow;
            if (ahora < saltoHasta) return;
            saltoHasta = ahora.AddMilliseconds(620);
            panelEncuentro.Invalidate();
        }

        private void DispararProyectilCombate()
        {
            if (!encuentroActivo || entidadActual == null) return;
            DateTime ahora = DateTime.UtcNow;
            if (ahora < siguienteDisparo) return;
            siguienteDisparo = ahora.AddMilliseconds(260);
            proyectilesCombate.Add(new ProyectilCombate(
                posicionCadeteCombate.X + (mirandoDerecha ? 42 : -42),
                posicionCadeteCombate.Y - 74,
                mirandoDerecha ? 1 : -1));
            panelEncuentro.Invalidate();
        }

        private void ActualizarArenaCombate()
        {
            if (!encuentroActivo || entidadActual == null) return;
            DateTime ahora = DateTime.UtcNow;
            RectangleF arena = LimitesArenaCombate();

            if (teclas.Contains(Keys.R) && ahora >= siguienteDisparo)
                DispararProyectilCombate();

            float dxCadete = 0, dyCadete = 0;
            if (teclas.Contains(Keys.A) || teclas.Contains(Keys.Left)) dxCadete--;
            if (teclas.Contains(Keys.D) || teclas.Contains(Keys.Right)) dxCadete++;
            if (teclas.Contains(Keys.W) || teclas.Contains(Keys.Up)) dyCadete--;
            if (teclas.Contains(Keys.S) || teclas.Contains(Keys.Down)) dyCadete++;
            if (dxCadete != 0 || dyCadete != 0)
            {
                float magnitud = MathF.Sqrt(dxCadete * dxCadete + dyCadete * dyCadete);
                dxCadete /= magnitud;
                dyCadete /= magnitud;
                bool correr = TeclaShiftPresionada() && resistencia > 0 && energia > 0;
                float velocidad = correr ? 7.2f : 4.2f;
                posicionCadeteCombate = new PointF(
                    Math.Clamp(posicionCadeteCombate.X + dxCadete * velocidad, arena.Left + 32, arena.Right - 32),
                    Math.Clamp(posicionCadeteCombate.Y + dyCadete * velocidad, arena.Top + 150, arena.Bottom - 8));
                if (dxCadete < 0) mirandoDerecha = false;
                if (dxCadete > 0) mirandoDerecha = true;
            }

            for (int i = proyectilesCombate.Count - 1; i >= 0; i--)
            {
                ProyectilCombate proyectil = proyectilesCombate[i];
                proyectil.X += proyectil.Direccion * 17f;
                bool impacto = Math.Abs(proyectil.X - posicionMonstruoCombate.X) < 63 &&
                    Math.Abs(proyectil.Y - (posicionMonstruoCombate.Y - 102)) < 88;
                if (impacto)
                {
                    proyectilesCombate.RemoveAt(i);
                    entidadActual.Vida = Math.Max(0, entidadActual.Vida - 10);
                    monstruoAturdidoHasta = ahora.AddMilliseconds(500);
                    siguienteAtaqueMonstruo = monstruoAturdidoHasta.AddMilliseconds(450);
                    ataqueMonstruoPendiente = false;
                    barraVidaMonstruo.Value = Math.Clamp(entidadActual.Vida, 0, barraVidaMonstruo.Maximum);
                    lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}/{entidadActual.VidaMaxima}  //  ATURDIDO";
                    if (entidadActual.Vida <= 0)
                    {
                        CompletarEncuentro();
                        return;
                    }
                }
                else if (proyectil.X < arena.Left - 10 || proyectil.X > arena.Right + 10)
                    proyectilesCombate.RemoveAt(i);
            }

            float dxMonstruo = posicionCadeteCombate.X - posicionMonstruoCombate.X;
            float dyMonstruo = posicionCadeteCombate.Y - posicionMonstruoCombate.Y;
            float distancia = MathF.Sqrt(dxMonstruo * dxMonstruo + dyMonstruo * dyMonstruo);
            bool aturdido = ahora < monstruoAturdidoHasta;
            if (!aturdido && !ataqueMonstruoPendiente && distancia > 128f)
            {
                float paso = Math.Min(2.55f, distancia - 112f);
                posicionMonstruoCombate = new PointF(
                    Math.Clamp(posicionMonstruoCombate.X + dxMonstruo / distancia * paso, arena.Left + 70, arena.Right - 70),
                    Math.Clamp(posicionMonstruoCombate.Y + dyMonstruo / distancia * paso, arena.Top + 160, arena.Bottom - 8));
            }

            if (!aturdido && !ataqueMonstruoPendiente && distancia <= 150f && ahora >= siguienteAtaqueMonstruo)
            {
                ataqueMonstruoPendiente = true;
                finAnticipacionMonstruo = ahora.AddMilliseconds(430);
                siguienteAtaqueMonstruo = ahora.AddMilliseconds(1650);
            }
            if (ataqueMonstruoPendiente && ahora >= finAnticipacionMonstruo)
            {
                ataqueMonstruoPendiente = false;
                finGolpeMonstruo = ahora.AddMilliseconds(260);
                dxMonstruo = posicionCadeteCombate.X - posicionMonstruoCombate.X;
                dyMonstruo = posicionCadeteCombate.Y - posicionMonstruoCombate.Y;
                distancia = MathF.Sqrt(dxMonstruo * dxMonstruo + dyMonstruo * dyMonstruo);
                if (distancia <= 175f && ahora >= saltoHasta)
                {
                    vida = Math.Max(0, vida - 10);
                    cadeteAturdidoHasta = ahora.AddMilliseconds(260);
                    barraVidaCadeteCombate.Value = Math.Clamp(vida, 0, 100);
                    lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100  //  GOLPE RECIBIDO (-10)";
                    ActualizarHUD();
                    if (vida <= 0)
                    {
                        FinalizarMision("EL CADETE HA CAÍDO. MISIÓN FALLIDA.", MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                    lblVidaCadeteCombate.Text = $"VIDA DEL CADETE: {vida}/100  //  GOLPE ESQUIVADO";
            }

            lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}/{entidadActual.VidaMaxima}" +
                (ahora < monstruoAturdidoHasta ? "  //  ATURDIDO" : "");
            panelEncuentro.Invalidate();
        }

        private void CompletarEncuentro()
        {
            if (entidadActual == null) return;
            Entidad derrotada = entidadActual;
            derrotada.Derrotada = true;
            encuentroActivo = false;
            entidadActual = null;
            teclas.Clear();
            jugadorCaminando = false;
            proyectilesCombate.Clear();
            ataqueMonstruoPendiente = false;
            panelEncuentro.Visible = false;
            ReaparecerEntidad(derrotada);
            energia = Math.Max(0, energia - 5);
            ActualizarHUD();
            Mensaje("ENTIDAD DERROTADA. REGRESASTE A HAWKINS.");
            panelMapa.Focus();
            panelMapa.Invalidate();
        }

        private void DibujarArenaCombate(object? sender, PaintEventArgs e)
        {
            if (!encuentroActivo || entidadActual == null) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            RectangleF arena = LimitesArenaCombate();
            using (SolidBrush tinte = new(Color.FromArgb(34, 2, 5, 16)))
                g.FillRectangle(tinte, panelEncuentro.ClientRectangle);
            using (Pen borde = new(Color.FromArgb(150, 93, 211, 231), 2))
                g.DrawRectangle(borde, arena.X, arena.Y, arena.Width, arena.Height);

            DateTime ahora = DateTime.UtcNow;
            float alturaSalto = 0;
            if (ahora < saltoHasta)
            {
                float progreso = 1f - (float)(saltoHasta - ahora).TotalMilliseconds / 620f;
                alturaSalto = MathF.Sin(Math.Clamp(progreso, 0f, 1f) * MathF.PI) * 86f;
            }

            DibujarSombraCombate(g, posicionMonstruoCombate, 74, 18);
            bool monstruoAturdido = ahora < monstruoAturdidoHasta;
            float vaivenMonstruo = monstruoAturdido ? MathF.Sin(Environment.TickCount * 0.06f) * 7f : 0f;
            float avanceGolpe = 0;
            if (ahora < finGolpeMonstruo)
            {
                float progresoGolpe = 1f - (float)(finGolpeMonstruo - ahora).TotalMilliseconds / 260f;
                avanceGolpe = MathF.Sin(Math.Clamp(progresoGolpe, 0f, 1f) * MathF.PI) * 36f;
            }
            bool monstruoMiraIzquierda = posicionMonstruoCombate.X > posicionCadeteCombate.X;
            float desplazamientoGolpe = monstruoMiraIzquierda ? -avanceGolpe : avanceGolpe;
            float monstruoDibujoX = posicionMonstruoCombate.X + vaivenMonstruo + desplazamientoGolpe;
            float monstruoDibujoY = posicionMonstruoCombate.Y - avanceGolpe * 0.15f;
            DibujarSpriteCombate(g, imagenMonstruoArena ?? entidadActual.Sprite,
                new RectangleF(monstruoDibujoX - 86,
                    monstruoDibujoY - 212, 172, 212), monstruoMiraIzquierda);
            if (monstruoAturdido)
            {
                using Pen destello = new(Color.FromArgb(220, 255, 230, 155), 3);
                g.DrawEllipse(destello, posicionMonstruoCombate.X - 42, posicionMonstruoCombate.Y - 185, 84, 68);
            }
            if (ahora < finGolpeMonstruo)
            {
                float direccion = monstruoMiraIzquierda ? -1f : 1f;
                using Pen estelaGolpe = new(Color.FromArgb(210, 255, 89, 92), 7);
                using Pen brilloGolpe = new(Color.FromArgb(235, 255, 222, 195), 2);
                PointF inicio = new(monstruoDibujoX + direccion * 48, monstruoDibujoY - 122);
                PointF final = new(posicionCadeteCombate.X - direccion * 18, posicionCadeteCombate.Y - 94);
                g.DrawLine(estelaGolpe, inicio, final);
                g.DrawLine(brilloGolpe, inicio, final);
            }

            foreach (ProyectilCombate proyectil in proyectilesCombate)
            {
                using Pen resplandor = new(Color.FromArgb(145, 91, 225, 255), 10);
                using Pen rayo = new(Color.FromArgb(255, 255, 233, 151), 4);
                float finX = proyectil.X + proyectil.Direccion * 24;
                g.DrawLine(resplandor, proyectil.X, proyectil.Y, finX, proyectil.Y);
                g.DrawLine(rayo, proyectil.X, proyectil.Y, finX, proyectil.Y);
            }

            DibujarSombraCombate(g, posicionCadeteCombate, 43, 12);
            Image? spriteCadete = teclas.Contains(Keys.R)
                ? imagenCadeteCombate2
                : teclas.Contains(Keys.A) || teclas.Contains(Keys.D) ||
                teclas.Contains(Keys.W) || teclas.Contains(Keys.S) || teclas.Contains(Keys.Left) ||
                teclas.Contains(Keys.Right) || teclas.Contains(Keys.Up) || teclas.Contains(Keys.Down)
                ? (fotograma / 2 % 2 == 0 ? imagenCadeteCombate1 : imagenCadeteCombate2)
                : imagenCadeteCombate1;
            if (ahora < cadeteAturdidoHasta && Environment.TickCount % 100 < 50)
            {
                using SolidBrush brillo = new(Color.FromArgb(110, 255, 255, 255));
                g.FillEllipse(brillo, posicionCadeteCombate.X - 44, posicionCadeteCombate.Y - 150 - alturaSalto, 88, 112);
            }
            DibujarSpriteCombate(g, spriteCadete ?? spritesCadete[2, 0],
                new RectangleF(posicionCadeteCombate.X - 51,
                    posicionCadeteCombate.Y - 136 - alturaSalto, 102, 136), !mirandoDerecha);

            if (ataqueMonstruoPendiente)
            {
                using Font aviso = new("Consolas", Math.Clamp(ClientSize.Height / 44f, 16f, 25f), FontStyle.Bold);
                using SolidBrush colorAviso = new(Color.FromArgb(245, 255, 222, 138));
                using StringFormat centrado = new() { Alignment = StringAlignment.Center };
                g.DrawString("¡EL DEMOGORGON SE PREPARA!  SALTA CON ESPACIO", aviso, colorAviso,
                    new RectangleF(arena.Left, arena.Top + 10, arena.Width, 34), centrado);
            }

            using Font ayuda = new("Consolas", Math.Clamp(ClientSize.Height / 100f, 10f, 15f), FontStyle.Bold);
            using SolidBrush textoAyuda = new(Color.FromArgb(235, 224, 242, 238));
            g.DrawString("W A S D  MOVER     R  DISPARAR     ESPACIO  SALTAR     SHIFT  CORRER",
                ayuda, textoAyuda, arena.Left + 8, arena.Bottom + 9);
        }

        private static void DibujarSombraCombate(Graphics g, PointF posicion, float ancho, float alto)
        {
            using SolidBrush sombra = new(Color.FromArgb(150, 0, 0, 0));
            g.FillEllipse(sombra, posicion.X - ancho / 2, posicion.Y - alto / 2, ancho, alto);
        }

        private static void DibujarSpriteCombate(Graphics g, Image sprite, RectangleF destino, bool invertir)
        {
            GraphicsState estado = g.Save();
            if (invertir)
            {
                float centroX = destino.Left + destino.Width / 2f;
                g.TranslateTransform(centroX, 0);
                g.ScaleTransform(-1, 1);
                g.TranslateTransform(-centroX, 0);
            }
            g.DrawImage(sprite, destino);
            g.Restore(estado);
        }

        private void EscaparEntidad()
        {
            if (entidadActual != null)
                entidadActual.Posicion = new PointF(jugadorX + 220, jugadorY + 180);
            proyectilesCombate.Clear();
            ataqueMonstruoPendiente = false;
            entidadActual = null;
            encuentroActivo = false;
            panelEncuentro.Visible = false;
            teclas.Clear();
            jugadorCaminando = false;
            energia = Math.Max(0, energia - 10);
            ActualizarHUD();
            Mensaje("ESCAPASTE. PERDISTE 10 PUNTOS DE ENERGÍA. LA ENTIDAD SIGUE EN EL SECTOR.");
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

                    // Señalización más clara y visible para las calles de Hawkins (102).
                    if (realidad == "REALIDAD 102")
                    {
                        using Pen lineaVial = new(Color.FromArgb(190, 205, 184, 126), 2);
                        using Pen bordeAcera = new(Color.FromArgb(170, 122, 111, 91), 3);
                        if (calleY <= 1)
                        {
                            g.DrawLine(bordeAcera, sx, sy + 5, sx + tile, sy + 5);
                            g.DrawLine(bordeAcera, sx, sy + 58, sx + tile, sy + 58);
                            if (calleY == 0)
                            {
                                g.DrawLine(lineaVial, sx + 4, sy + 27, sx + 22, sy + 27);
                                g.DrawLine(lineaVial, sx + 40, sy + 27, sx + 58, sy + 27);
                                g.DrawLine(lineaVial, sx + 4, sy + 37, sx + 22, sy + 37);
                                g.DrawLine(lineaVial, sx + 40, sy + 37, sx + 58, sy + 37);
                            }
                        }
                        if (calleX <= 1)
                        {
                            g.DrawLine(bordeAcera, sx + 5, sy, sx + 5, sy + tile);
                            g.DrawLine(bordeAcera, sx + 58, sy, sx + 58, sy + tile);
                            if (calleX == 0)
                            {
                                g.DrawLine(lineaVial, sx + 27, sy + 4, sx + 27, sy + 22);
                                g.DrawLine(lineaVial, sx + 27, sy + 40, sx + 27, sy + 58);
                                g.DrawLine(lineaVial, sx + 37, sy + 4, sx + 37, sy + 22);
                                g.DrawLine(lineaVial, sx + 37, sy + 40, sx + 37, sy + 58);
                            }
                        }
                    }
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

            if (garrafasEnergia.TryGetValue(realidad, out List<PointF>? garrafas))
            foreach (PointF garrafa in garrafas)
            {
                float gx = garrafa.X - camaraX, gy = garrafa.Y - camaraY;
                using SolidBrush sombraGarrafa = new(Color.FromArgb(130, 0, 0, 0));
                using SolidBrush cuerpoGarrafa = new(Color.FromArgb(38, 116, 91));
                using SolidBrush brilloGarrafa = new(Color.FromArgb(115, 225, 174));
                using Pen bordeGarrafa = new(Color.FromArgb(190, 15, 48, 39), 2);
                g.FillEllipse(sombraGarrafa, gx - 17, gy + 14, 34, 8);
                g.FillRectangle(cuerpoGarrafa, gx - 11, gy - 13, 22, 28);
                g.FillRectangle(cuerpoGarrafa, gx - 6, gy - 19, 12, 7);
                g.FillRectangle(brilloGarrafa, gx - 7, gy - 9, 4, 17);
                g.DrawRectangle(bordeGarrafa, gx - 11, gy - 13, 22, 28);
                using Pen simboloEnergia = new(Color.FromArgb(245, 235, 220, 90), 3);
                g.DrawLine(simboloEnergia, gx + 3, gy - 7, gx - 2, gy + 1);
                g.DrawLine(simboloEnergia, gx - 2, gy + 1, gx + 3, gy + 1);
                g.DrawLine(simboloEnergia, gx + 3, gy + 1, gx - 2, gy + 10);
            }

            if (celulasMedicas.TryGetValue(realidad, out List<PointF>? celulas))
            foreach (PointF celula in celulas)
            {
                float cx = celula.X - camaraX, cy = celula.Y - camaraY;
                using SolidBrush baseCelula = new(Color.FromArgb(220, 184, 45, 58));
                using SolidBrush cruzCelula = new(Color.FromArgb(245, 245, 245));
                using Pen bordeCelula = new(Color.FromArgb(245, 75, 15, 25), 2);
                g.FillEllipse(baseCelula, cx - 13, cy - 13, 26, 26);
                g.DrawEllipse(bordeCelula, cx - 13, cy - 13, 26, 26);
                g.FillRectangle(cruzCelula, cx - 3, cy - 9, 6, 18);
                g.FillRectangle(cruzCelula, cx - 9, cy - 3, 18, 6);
            }

            foreach (Entidad entidad in entidades.Where(z => !z.Derrotada))
            {
                float ex = entidad.Posicion.X - camaraX, ey = entidad.Posicion.Y - camaraY;
                if (ex > -70 && ey > -70 && ex < panelMapa.Width + 70 && ey < panelMapa.Height + 70)
                {
                    if (entidad.EsZombie)
                    {
                        // Pequeño vaivén de pasos para dar sensación de caminata.
                        float pasoZombie = MathF.Sin(fotograma * 0.24f + entidad.Posicion.X * 0.01f) * 3f;
                        g.DrawImage(entidad.Sprite, ex - 31, ey - 35 + pasoZombie, 62, 70);
                    }
                    else
                    {
                        g.DrawImage(entidad.Sprite, ex - 35, ey - 33, 70, 66);
                    }
                }
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
            // El portal conserva su brillo y se distingue incluso fuera del radio claro.
            DibujarPortal(g);
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
            // Árboles más notorios: tronco grueso, ramas y una copa amplia
            // para que no parezcan solamente ramas sobre el terreno.
            using SolidBrush sombra = new(Color.FromArgb(125, 8, 15, 10));
            using SolidBrush tronco = new(Color.FromArgb(68, 43, 31));
            using SolidBrush copaOscura = new(Color.FromArgb(25, 53, 34));
            using SolidBrush copaMedia = new(Color.FromArgb(43, 82, 43));
            using SolidBrush copaLuz = new(Color.FromArgb(76, 105, 51));
            using Pen rama = new(Color.FromArgb(55, 37, 28), 4);
            g.FillEllipse(sombra, x + 3, y + 39, 55, 16);
            g.FillRectangle(tronco, x + 24, y + 29, 13, 25);
            g.DrawLine(rama, x + 30, y + 38, x + 13, y + 18);
            g.DrawLine(rama, x + 30, y + 37, x + 48, y + 15);
            g.FillEllipse(copaOscura, x + 2, y + 2, 58, 43);
            g.FillEllipse(copaMedia, x + 7, y - 2, 24, 27);
            g.FillEllipse(copaMedia, x + 29, y + 1, 27, 26);
            g.FillEllipse(copaMedia, x + 13, y + 18, 37, 25);
            g.FillEllipse(copaLuz, x + 12, y + 5, 14, 10);
            g.FillEllipse(copaLuz, x + 36, y + 12, 12, 9);
            using Pen borde = new(Color.FromArgb(150, 16, 37, 25), 2);
            g.DrawEllipse(borde, x + 2, y + 2, 58, 43);
        }

        private void DibujarArbol(Graphics g, int x, int y)
        {
            using SolidBrush sombra = new(Color.FromArgb(125, 0, 0, 0));
            using SolidBrush tronco = new(Color.FromArgb(48, 35, 26));
            using SolidBrush copa = new(realidad == "REALIDAD 102" ? Color.FromArgb(24, 67, 36) : Color.FromArgb(63, 13, 24));
            using SolidBrush copaMedia = new(realidad == "REALIDAD 102" ? Color.FromArgb(42, 91, 43) : Color.FromArgb(115, 25, 36));
            using SolidBrush brillo = new(realidad == "REALIDAD 102" ? Color.FromArgb(91, 119, 54) : Color.FromArgb(165, 35, 43));
            g.FillEllipse(sombra, x + 3, y + 37, 54, 15);
            g.FillRectangle(tronco, x + 23, y + 27, 13, 27);
            g.FillEllipse(copa, x + 2, y + 1, 58, 46);
            g.FillEllipse(copaMedia, x + 8, y - 2, 26, 29);
            g.FillEllipse(copaMedia, x + 30, y + 3, 27, 27);
            g.FillEllipse(brillo, x + 13, y + 6, 13, 10);
            g.FillEllipse(brillo, x + 37, y + 17, 11, 9);
            using Pen contorno = new(Color.FromArgb(170, 12, 30, 20), 2);
            g.DrawEllipse(contorno, x + 2, y + 1, 58, 46);
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
            panelEncuentro.Visible = false;
            DetenerMusica();
            ActualizarHUD();
            Mensaje(mensaje);
            if (vida <= 0)
                MostrarPantallaPerdiste();
            else
                MessageBox.Show(this, mensaje, "NEXUS", MessageBoxButtons.OK, icono);
            Close();
        }

        private void MostrarPantallaPerdiste()
        {
            using Form pantalla = new Form
            {
                Text = "NEXUS // MISIÓN FALLIDA",
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(900, 500),
                BackColor = Color.Black
            };
            using Bitmap imagen = new Bitmap(900, 500);
            using (Graphics g = Graphics.FromImage(imagen))
            {
                g.Clear(Color.FromArgb(5, 0, 5));
                using Pen borde = new Pen(Color.FromArgb(150, 15, 35), 3);
                g.DrawRectangle(borde, 18, 18, 864, 464);
                using Font titulo = new Font("Consolas", 66, FontStyle.Bold);
                using Font subtitulo = new Font("Consolas", 22, FontStyle.Bold);
                using Font detalle = new Font("Consolas", 13, FontStyle.Regular);
                using SolidBrush rojo = new SolidBrush(Color.FromArgb(235, 25, 55));
                using SolidBrush blanco = new SolidBrush(Color.Gainsboro);
                using StringFormat centrado = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("PERDISTE", titulo, rojo, new RectangleF(30, 145, 840, 100), centrado);
                g.DrawString("EL CADETE HA CAÍDO", subtitulo, blanco, new RectangleF(30, 260, 840, 45), centrado);
                g.DrawString("NEXUS SYSTEM  //  CONEXIÓN INTERRUMPIDA", detalle, rojo, new RectangleF(30, 330, 840, 35), centrado);
            }
            PictureBox imagenFinal = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = (Bitmap)imagen.Clone(),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            pantalla.Controls.Add(imagenFinal);
            pantalla.ShowDialog(this);
            imagenFinal.Image?.Dispose();
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
            lblMonstruo.SetBounds(margen, 14, ancho - margen * 2, 42);
            lblMonstruo.Font = new Font("Consolas", Math.Clamp(alto / 40f, 20f, 34f), FontStyle.Bold);
            lblDetalleMonstruo.SetBounds(margen, 57, ancho - margen * 2, 23);

            int anchoBarra = Math.Min(620, ancho - margen * 2);
            int xBarra = (ancho - anchoBarra) / 2;
            barraVidaMonstruo.SetBounds(xBarra, 84, anchoBarra, 20);
            lblVidaCadeteCombate.SetBounds(margen, alto - 155, ancho - margen * 2, 24);
            barraVidaCadeteCombate.SetBounds(xBarra, alto - 127, anchoBarra, 18);
            imgMonstruo.Visible = false;
            imgMonstruo.SetBounds(0, 0, 1, 1);

            int espacioBotones = 26;
            int anchoBoton = Math.Min(260, (ancho - margen * 2 - espacioBotones) / 2);
            int xPrimerBoton = (ancho - (anchoBoton * 2 + espacioBotones)) / 2;
            btnAtacar.SetBounds(xPrimerBoton, alto - 62, anchoBoton, 46);
            btnEscapar.SetBounds(xPrimerBoton + anchoBoton + espacioBotones, alto - 62, anchoBoton, 46);
        }

        private void UsarCelula()
        {
            if (celulasVida <= 0) { Mensaje("NO TIENES CÉLULAS MÉDICAS."); return; }
            if (vida >= 100) { Mensaje("LA VIDA YA ESTÁ AL MÁXIMO."); return; }

            celulasVida--;
            vida = Math.Min(100, vida + 30);
            ActualizarHUD();
            Mensaje("CÉLULA MÉDICA UTILIZADA: +30 PUNTOS DE VIDA.");
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
            PointF puntoEntrada = PosicionEntradaDelPortal(portalDestino);
            jugadorX = puntoEntrada.X;
            jugadorY = puntoEntrada.Y;
            portalAvisoMostrado = false;
            camaraX = jugadorX - panelMapa.Width / 2f; camaraY = jugadorY - panelMapa.Height / 2f;
            CrearEntidades();
            ActualizarHUD(); panelMapa.Invalidate();
            Mensaje("CRUCE POR PORTAL COMPLETADO: " + realidad + ". USA W, A, S Y D PARA EXPLORAR.");
        }

        private void ActualizarHUD()
        {
            lblNombre.Text = "CADETE: " + nombreExplorador;
            lblVida.Text = $"VIDA: {vida}/100";
            lblEnergia.Text = $"ENERGÍA: {energia}/100";
            lblConexion.Text = $"CONEXIÓN: {conexion}/100";
            lblPosicion.Text = $"X: {(int)jugadorX}  Y: {(int)jugadorY}  //  {realidad}";
            lblTituloMapa.Text = "ZONA DE EXPLORACIÓN // " + realidad;
            lblInventario.Text = $"CÉLULAS MÉDICAS: {celulasVida}\r\nNÚCLEOS: {nucleosRecolectados.Count}/{nucleosNecesarios}";
            lblContadorNucleos.Text = naveReparada
                ? $"NAVE LISTA: {nucleosRecolectados.Count}/{nucleosNecesarios}"
                : $"NÚCLEOS: {nucleosRecolectados.Count}/{nucleosNecesarios}";
            lblMision.Text = naveReparada
                ? "Reactor reemplazado. Tu nave está lista para volver a viajar."
                : "Tu nave quedó varada. Reúne 4 núcleos, dos en cada realidad, para reparar el reactor.";
            barraVida.Value = Math.Clamp(vida, 0, 100);
            barraEnergia.Value = Math.Clamp(energia, 0, 100);
            barraConexion.Value = Math.Clamp(conexion, 0, 100);

            // Las alertas se reproducen una sola vez al cruzar el umbral del 20%.
            if (energia < 20 && !alertaEnergiaActiva)
            {
                alertaEnergiaActiva = true;
                ReproducirEfecto(sonidoAlerta);
                Mensaje("ALERTA: ENERGÍA POR DEBAJO DEL 20%.");
            }
            else if (energia >= 25) alertaEnergiaActiva = false;

            if (vida < 20 && !alertaVidaActiva)
            {
                alertaVidaActiva = true;
                ReproducirEfecto(sonidoAlerta);
                Mensaje("ALERTA: VIDA POR DEBAJO DEL 20%.");
            }
            else if (vida >= 25) alertaVidaActiva = false;
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

