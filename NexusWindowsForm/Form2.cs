using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexusWindowsForm
{
    public partial class Form2 : Form
    {
        private readonly Color rojo = Color.FromArgb(230, 20, 48);
        private readonly List<Entidad> entidades = new();
        private readonly HashSet<Keys> teclas = new();
        private readonly System.Windows.Forms.Timer relojMovimiento = new();
        private readonly System.Windows.Forms.Timer relojEscena = new();

        private string nombreExplorador = "";
        private int edad;
        private int vida = 100, energia = 100, conexion = 100;
        private float jugadorX = 2500, jugadorY = 2500;
        private float camaraX, camaraY;
        private bool iniciado, encuentroActivo, nucleoEncontrado;
        private string realidad = "REALIDAD 102";
        private Entidad? entidadActual;
        private int fotograma;
        private int ticksMovimiento;
        private int celulasEnergia;
        private readonly PointF nucleo = new(3400, 3400);

        private sealed class Entidad
        {
            public string Nombre { get; init; } = "";
            public string Tipo { get; init; } = "";
            public PointF Posicion { get; set; }
            public int Vida { get; set; } = 60;
            public Bitmap Sprite { get; init; } = null!;
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
            btnCambiarRealidad.Click += (_, _) => CambiarRealidad();
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
            panelEncuentro.Location = new Point(
                Math.Max(20, (panelMapaContenedor.Width - panelEncuentro.Width) / 2),
                Math.Max(50, (panelMapaContenedor.Height - panelEncuentro.Height) / 2));
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            panelMapa.Invalidate();
        }

        private void CrearEscenasPixelArt()
        {
            imgPueblo.Image = CrearEscenaPixelArt(0);
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

        private Bitmap CrearSpriteMonstruo(int tipo)
        {
            Bitmap bmp = new Bitmap(96, 96);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            void P(Color c, int x, int y, int w, int h)
            { using SolidBrush b = new(c); g.FillRectangle(b, x, y, w, h); }
            Color carne = tipo == 0 ? Color.FromArgb(115, 8, 30) : Color.FromArgb(55, 8, 45);
            Color luz = Color.FromArgb(255, 25, 55);
            // Sprite original pixel-art, visto desde arriba.
            P(Color.FromArgb(35, 2, 12), 25, 15, 46, 65);
            P(carne, 30, 18, 36, 58); P(carne, 18, 30, 60, 30);
            P(Color.FromArgb(25, 2, 10), 35, 30, 26, 30);
            P(luz, 38, 34, 7, 7); P(luz, 53, 34, 7, 7);
            P(Color.Black, 40, 36, 3, 3); P(Color.Black, 55, 36, 3, 3);
            if (tipo == 0)
            {
                P(carne, 8, 12, 28, 10); P(carne, 60, 12, 28, 10);
                P(carne, 5, 5, 12, 18); P(carne, 79, 5, 12, 18);
                P(Color.FromArgb(230, 170, 175), 40, 57, 16, 5);
            }
            else
            {
                P(Color.FromArgb(100, 0, 70), 12, 20, 15, 45); P(Color.FromArgb(100, 0, 70), 69, 20, 15, 45);
                P(luz, 42, 48, 12, 5);
            }
            return bmp;
        }

        private void CrearEntidades()
        {
            entidades.Clear();
            entidades.Add(new Entidad { Nombre = "ACECHADOR DEL REVÉS", Tipo = "Entidad orgánica", Posicion = new PointF(2850, 2580), Sprite = CrearSpriteMonstruo(0), Vida = 60 });
            entidades.Add(new Entidad { Nombre = "SOMBRA DEL VACÍO", Tipo = "Entidad de sombra", Posicion = new PointF(2150, 2200), Sprite = CrearSpriteMonstruo(1), Vida = 45 });
        }

        private void IniciarSistema()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre del cadete para iniciar NEXUS.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus(); return;
            }
            nombreExplorador = txtNombre.Text.Trim();
            edad = (int)numEdad.Value;
            energia = (int)numEnergia.Value;
            conexion = (int)numConexion.Value;
            vida = 100; iniciado = true; encuentroActivo = false; nucleoEncontrado = false;
            panelRegistro.Visible = false; panelJuego.Visible = true; panelJuego.BringToFront(); lblEncabezado.BringToFront();
            AjustarLayout();
            relojMovimiento.Start();
            ActualizarHUD(); Mensaje("CADete REGISTRADO. EXPLORA EL MUNDO DESDE UNA VISTA AÉREA.");
            panelMapa.Focus();
        }

        private void Form2_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && encuentroActivo) { EscaparEntidad(); e.Handled = true; return; }
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Up or Keys.Down or Keys.Left or Keys.Right)
            {
                teclas.Add(e.KeyCode); e.Handled = true; e.SuppressKeyPress = true;
            }
        }

        private void Form2_KeyUp(object? sender, KeyEventArgs e)
        {
            teclas.Remove(e.KeyCode);
        }

        private void ActualizarMovimiento()
        {
            if (!iniciado || encuentroActivo || !panelJuego.Visible) return;
            float dx = 0, dy = 0;
            if (teclas.Contains(Keys.W) || teclas.Contains(Keys.Up)) dy--;
            if (teclas.Contains(Keys.S) || teclas.Contains(Keys.Down)) dy++;
            if (teclas.Contains(Keys.A) || teclas.Contains(Keys.Left)) dx--;
            if (teclas.Contains(Keys.D) || teclas.Contains(Keys.Right)) dx++;
            if (dx == 0 && dy == 0) return;
            float magnitud = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= magnitud; dy /= magnitud;
            float velocidad = energia > 0 ? 4.2f : 1.4f;
            jugadorX = Math.Clamp(jugadorX + dx * velocidad, 80, 4920);
            jugadorY = Math.Clamp(jugadorY + dy * velocidad, 80, 4920);
            ticksMovimiento++;
            if (ticksMovimiento % 30 == 0)
            {
                energia = Math.Max(0, energia - 1);
                conexion = Math.Max(0, conexion - 1);
            }
            camaraX = jugadorX - panelMapa.ClientSize.Width / 2f;
            camaraY = jugadorY - panelMapa.ClientSize.Height / 2f;
            DetectarEntidad();
            if (Math.Abs(jugadorX - nucleo.X) < 45 && Math.Abs(jugadorY - nucleo.Y) < 45 && !nucleoEncontrado)
            {
                if (MessageBox.Show("HAS ENCONTRADO EL NÚCLEO ENERGÉTICO. ¿REGISTRARLO?", "MISIÓN NEXUS", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    nucleoEncontrado = true;
                    Mensaje("NÚCLEO REGISTRADO. MISIÓN COMPLETADA.");
                }
            }
            if ((int)jugadorX % 1100 < 8 && (int)jugadorY % 900 < 8)
            {
                celulasEnergia++;
                Mensaje("CÉLULA DE ENERGÍA RECOLECTADA.");
            }
            ActualizarHUD();
            panelMapa.Invalidate();
        }

        private void DetectarEntidad()
        {
            Entidad? encontrada = entidades.FirstOrDefault(e => !e.Derrotada &&
                Math.Abs(e.Posicion.X - jugadorX) < 62 && Math.Abs(e.Posicion.Y - jugadorY) < 62);
            if (encontrada == null) return;
            encuentroActivo = true; entidadActual = encontrada; teclas.Clear();
            lblMonstruo.Text = encontrada.Nombre;
            lblDetalleMonstruo.Text = $"{encontrada.Tipo}  //  VIDA: {encontrada.Vida}";
            imgMonstruo.Image = encontrada.Sprite;
            panelEncuentro.Visible = true; panelEncuentro.BringToFront();
            Mensaje("ALERTA: UNA ENTIDAD TE HA DETECTADO.");
        }

        private void AtacarEntidad()
        {
            if (!encuentroActivo || entidadActual == null) return;
            entidadActual.Vida -= 20;
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
                lblDetalleMonstruo.Text = $"{entidadActual.Tipo}  //  VIDA: {entidadActual.Vida}  //  TE ATACÓ (-12)";
                barraVida.Value = vida;
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
                bool carretera = (Math.Abs(ty % 13) <= 1) || (Math.Abs(tx % 17) <= 1);
                Color suelo = realidad == "REALIDAD 102" ? Color.FromArgb(30, 13, 23) : Color.FromArgb(18, 12, 27);
                using (SolidBrush b = new(carretera ? Color.FromArgb(42, 22, 29) : suelo)) g.FillRectangle(b, sx, sy, tile, tile);
                using (Pen p = new(Color.FromArgb(42, 28, 39))) g.DrawRectangle(p, sx, sy, tile, tile);
                int semilla = Math.Abs((tx * 73856093) ^ (ty * 19349663));
                if (carretera)
                {
                    using SolidBrush marca = new(Color.FromArgb(95, 70, 65));
                    g.FillRectangle(marca, sx + 8, sy + 30, 18, 3);
                }
                else if (semilla % 7 == 0) DibujarArbol(g, sx + 8, sy + 8);
                else if (semilla % 11 == 0) DibujarRuina(g, sx + 5, sy + 8);
                else if (semilla % 5 == 0)
                {
                    using SolidBrush mancha = new(Color.FromArgb(65, 7, 25));
                    g.FillRectangle(mancha, sx + 20, sy + 22, 8, 8);
                    g.FillRectangle(mancha, sx + 37, sy + 39, 5, 5);
                }
            }

            foreach (Entidad entidad in entidades.Where(z => !z.Derrotada))
            {
                float ex = entidad.Posicion.X - camaraX, ey = entidad.Posicion.Y - camaraY;
                if (ex > -70 && ey > -70 && ex < panelMapa.Width + 70 && ey < panelMapa.Height + 70)
                    g.DrawImage(entidad.Sprite, ex - 24, ey - 24, 48, 48);
            }

            // Cadete: sprite cenital pixelado y sombra.
            float px = jugadorX - camaraX, py = jugadorY - camaraY;
            using (SolidBrush sombra = new(Color.FromArgb(150, 0, 0, 0))) g.FillEllipse(sombra, px - 14, py + 8, 28, 10);
            using (SolidBrush cuerpo = new(Color.FromArgb(25, 25, 35))) g.FillRectangle(cuerpo, px - 9, py - 11, 18, 22);
            using (SolidBrush chaqueta = new(Color.FromArgb(40, 120, 85))) g.FillRectangle(chaqueta, px - 8, py - 8, 16, 15);
            using (SolidBrush cabeza = new(Color.FromArgb(220, 175, 145))) g.FillRectangle(cabeza, px - 5, py - 15, 10, 8);
            using (SolidBrush mochila = new(Color.FromArgb(150, 15, 35))) g.FillRectangle(mochila, px - 11, py - 5, 4, 10);
            using (Pen p = new(Color.FromArgb(255, 60, 75), 2)) g.DrawRectangle(p, px - 13, py - 18, 26, 38);

            if (!nucleoEncontrado)
            {
                float nx = nucleo.X - camaraX, ny = nucleo.Y - camaraY;
                using SolidBrush nucleoBrush = new(Color.Gold);
                g.FillRectangle(nucleoBrush, nx - 10, ny - 10, 20, 20);
                using Pen nucleoPen = new(Color.OrangeRed, 3);
                g.DrawRectangle(nucleoPen, nx - 14, ny - 14, 28, 28);
            }
            using SolidBrush coords = new(Color.FromArgb(220, 255, 230, 230));
            g.DrawString($"X: {(int)jugadorX}   Y: {(int)jugadorY}   //   {realidad}", new Font("Consolas", 10, FontStyle.Bold), coords, 12, 12);
        }

        private void DibujarArbol(Graphics g, int x, int y)
        {
            using SolidBrush tronco = new(Color.FromArgb(38, 15, 20));
            using SolidBrush copa = new(Color.FromArgb(15, 35, 24));
            using SolidBrush brillo = new(Color.FromArgb(95, 5, 25));
            g.FillRectangle(tronco, x + 22, y + 28, 12, 22);
            g.FillRectangle(copa, x + 8, y + 5, 40, 35);
            g.FillRectangle(brillo, x + 16, y + 12, 8, 7);
            g.FillRectangle(brillo, x + 34, y + 24, 7, 6);
        }

        private void DibujarRuina(Graphics g, int x, int y)
        {
            using SolidBrush edificio = new(Color.FromArgb(40, 27, 35));
            using SolidBrush techo = new(Color.FromArgb(92, 9, 28));
            g.FillRectangle(edificio, x, y, 52, 45);
            g.FillRectangle(techo, x - 3, y - 4, 58, 10);
            using SolidBrush ventana = new(Color.FromArgb(230, 30, 50));
            g.FillRectangle(ventana, x + 10, y + 15, 7, 8);
            g.FillRectangle(ventana, x + 34, y + 15, 7, 8);
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

        private void CambiarRealidad()
        {
            realidad = realidad == "REALIDAD 102" ? "REALIDAD 999" : "REALIDAD 102";
            jugadorX = 2500; jugadorY = 2500; camaraX = jugadorX - panelMapa.Width / 2f; camaraY = jugadorY - panelMapa.Height / 2f;
            CrearEntidades(); ActualizarHUD(); panelMapa.Invalidate();
            Mensaje("CRUCE DIMENSIONAL COMPLETADO: " + realidad);
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
            lblInventario.Text = $"CÉLULAS DE ENERGÍA: {celulasEnergia}\\nNÚCLEO: {(nucleoEncontrado ? "REGISTRADO" : "PENDIENTE")}";
            barraVida.Value = Math.Clamp(vida, 0, 100);
            barraEnergia.Value = Math.Clamp(energia, 0, 100);
            barraConexion.Value = Math.Clamp(conexion, 0, 100);
        }

        private void Mensaje(string mensaje)
        {
            lblMensaje.Text = ">> " + mensaje;
        }
    }
}
