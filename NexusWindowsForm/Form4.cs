using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace NexusWindowsForm;

/// <summary>Arcade sequence where the cadet clears a path through the meteor shower.</summary>
public sealed class Form4 : Form
{
    private const int ObjetivoMeteoritosPequenos = 20;
    private const int ObjetivoMeteoritosGrandes = 20;
    private const int VidaMeteoritoPequeno = 3;
    private const int VidaMeteoritoGrande = 20;

    private readonly BufferedPanel lienzo = new() { Dock = DockStyle.Fill, BackColor = Color.Black };
    private readonly System.Windows.Forms.Timer reloj = new() { Interval = 16 };
    private readonly HashSet<Keys> teclas = new();
    private readonly List<Disparo> disparos = new();
    private readonly List<Meteorito> meteoritos = new();
    private readonly Random azar = new();
    private readonly Bitmap? fondo;
    private readonly Bitmap? nave;
    private readonly Bitmap? disparoImagen;
    private readonly Bitmap? meteoritoPequeno;
    private readonly Bitmap? meteoritoGrande;
    private readonly PuntoEstrella[] estrellas;

    private float naveX;
    private float naveY;
    private int disparoEnfriamiento;
    private int pequenosDestruidos;
    private int pequenosGenerados;
    private int grandesDestruidos;
    private int grandesGenerados;
    private int cuadrosFinal;
    private bool finalizando;
    private string mensajeFinal = "";

    private sealed class Disparo(float x, float y)
    {
        public float X = x;
        public float Y = y;
    }

    private sealed class Meteorito(float x, float y, float velocidad, int vida, bool grande)
    {
        public float X = x;
        public float Y = y;
        public float Velocidad = velocidad;
        public int Vida = vida;
        public int VidaMaxima = vida;
        public bool Grande = grande;
        public float Oscilacion = 0;
    }

    private struct PuntoEstrella(float x, float y, float velocidad, int radio, int brillo)
    {
        public float X = x;
        public float Y = y;
        public float Velocidad = velocidad;
        public int Radio = radio;
        public int Brillo = brillo;
    }

    public Form4()
    {
        Text = "NEXUS // LLUVIA DE METEORITOS";
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        BackColor = Color.FromArgb(4, 7, 17);
        Cursor = Cursors.Cross;

        fondo = CargarImagen("FondoEspacial.jpg");
        nave = CargarImagen("NaveVuelo.png");
        disparoImagen = CargarImagen("DisparoNexus.png");
        meteoritoPequeno = CargarImagen("MeteoritoPequeno.png");
        meteoritoGrande = CargarImagen("MeteoritoGrande.png");

        Controls.Add(lienzo);
        lienzo.Paint += DibujarJuego;
        KeyDown += RegistrarTecla;
        KeyUp += LiberarTecla;
        Shown += (_, _) =>
        {
            ColocarNaveInicial();
            CrearMeteoritosPequenos();
            lienzo.Focus();
            reloj.Start();
        };
        Resize += (_, _) =>
        {
            LimitarNave();
            lienzo.Invalidate();
        };
        reloj.Tick += ActualizarJuego;
        FormClosed += (_, _) =>
        {
            reloj.Stop();
            reloj.Dispose();
            fondo?.Dispose();
            nave?.Dispose();
            disparoImagen?.Dispose();
            meteoritoPequeno?.Dispose();
            meteoritoGrande?.Dispose();
        };

        estrellas = Enumerable.Range(0, 115)
            .Select(_ => new PuntoEstrella(
                azar.Next(0, Math.Max(1, Screen.PrimaryScreen?.Bounds.Width ?? 1600)),
                azar.Next(0, Math.Max(1, Screen.PrimaryScreen?.Bounds.Height ?? 900)),
                (float)(0.35 + azar.NextDouble() * 1.65), azar.Next(1, 4), azar.Next(80, 220)))
            .ToArray();
    }

    private static Bitmap? CargarImagen(string archivo)
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "Resources", archivo);
        if (!File.Exists(ruta)) return null;
        using Image original = Image.FromFile(ruta);
        return new Bitmap(original);
    }

    private void ColocarNaveInicial()
    {
        naveX = ClientSize.Width * 0.11f;
        naveY = ClientSize.Height * 0.5f;
    }

    private void CrearMeteoritosPequenos()
    {
        while (meteoritos.Count < 4 && pequenosGenerados < ObjetivoMeteoritosPequenos)
            CrearMeteoritoPequeno();
    }

    private void CrearMeteoritoPequeno()
    {
        float alto = Math.Max(180, ClientSize.Height - 180);
        meteoritos.Add(new Meteorito(
            ClientSize.Width + azar.Next(40, 480),
            azar.Next(100, (int)alto),
            (float)(2.2 + azar.NextDouble() * 2.1),
            VidaMeteoritoPequeno,
            grande: false));
        pequenosGenerados++;
    }

    private void CrearMeteoritoGrande()
    {
        float alto = Math.Max(180, ClientSize.Height - 180);
        meteoritos.Add(new Meteorito(
            ClientSize.Width + 120,
            azar.Next(120, (int)alto),
            (float)(1.6 + azar.NextDouble() * 0.9),
            VidaMeteoritoGrande,
            grande: true));
        grandesGenerados++;
    }

    private void RegistrarTecla(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D or Keys.T)
        {
            teclas.Add(e.KeyCode);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void LiberarTecla(object? sender, KeyEventArgs e) => teclas.Remove(e.KeyCode);

    private void ActualizarJuego(object? sender, EventArgs e)
    {
        if (finalizando)
        {
            if (--cuadrosFinal <= 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else lienzo.Invalidate();
            return;
        }

        MoverNave();
        MoverEstrellas();
        ActualizarDisparos();
        ActualizarMeteoritos();
        lienzo.Invalidate();
    }

    private void MoverNave()
    {
        float dx = (teclas.Contains(Keys.D) ? 1 : 0) - (teclas.Contains(Keys.A) ? 1 : 0);
        float dy = (teclas.Contains(Keys.S) ? 1 : 0) - (teclas.Contains(Keys.W) ? 1 : 0);
        if (dx != 0 && dy != 0)
        {
            dx *= 0.7071f;
            dy *= 0.7071f;
        }

        float velocidad = Math.Clamp(ClientSize.Width / 190f, 6.5f, 10f);
        naveX += dx * velocidad;
        naveY += dy * velocidad;
        LimitarNave();

        if (teclas.Contains(Keys.T))
        {
            if (disparoEnfriamiento <= 0)
            {
                float ancho = AnchoNave();
                disparos.Add(new Disparo(naveX + ancho * 0.72f, naveY));
                disparoEnfriamiento = 10;
            }
        }
        if (disparoEnfriamiento > 0) disparoEnfriamiento--;
    }

    private void LimitarNave()
    {
        float ancho = AnchoNave();
        float alto = AltoNave();
        naveX = Math.Clamp(naveX, 24, Math.Max(24, ClientSize.Width - ancho - 24));
        naveY = Math.Clamp(naveY, alto / 2 + 78, Math.Max(alto / 2 + 78, ClientSize.Height - alto / 2 - 72));
    }

    private float AnchoNave() => Math.Clamp(ClientSize.Width * 0.105f, 138, 205);
    private float AltoNave() => nave is null ? AnchoNave() * 0.56f : AnchoNave() * nave.Height / nave.Width;

    private void ActualizarDisparos()
    {
        float velocidad = Math.Clamp(ClientSize.Width / 140f, 10f, 16f);
        foreach (Disparo disparo in disparos) disparo.X += velocidad;
        disparos.RemoveAll(d => d.X > ClientSize.Width + 60);
    }

    private void ActualizarMeteoritos()
    {
        float dt = ClientSize.Width / 1700f;
        RectangleF hitNave = new(naveX + AnchoNave() * 0.20f, naveY - AltoNave() * 0.27f,
            AnchoNave() * 0.58f, AltoNave() * 0.54f);

        foreach (Meteorito meteorito in meteoritos.ToArray())
        {
            meteorito.X -= meteorito.Velocidad * dt;
            meteorito.Oscilacion += 0.035f;
            meteorito.Y += (float)Math.Sin(meteorito.Oscilacion) * 0.72f;

            SizeF tamano = TamanoMeteorito(meteorito);
            RectangleF cajaMeteorito = new(meteorito.X, meteorito.Y - tamano.Height / 2,
                tamano.Width, tamano.Height);

            if (hitNave.IntersectsWith(cajaMeteorito))
            {
                Terminar("NAVE IMPACTADA // VIDA PERDIDA");
                return;
            }

            for (int i = disparos.Count - 1; i >= 0; i--)
            {
                Disparo d = disparos[i];
                RectangleF cajaDisparo = new(d.X - 17, d.Y - 17, 34, 34);
                if (!cajaDisparo.IntersectsWith(cajaMeteorito)) continue;

                disparos.RemoveAt(i);
                meteorito.Vida--;
                if (meteorito.Vida > 0) continue;

                meteoritos.Remove(meteorito);
                if (meteorito.Grande)
                {
                    grandesDestruidos++;
                    if (grandesDestruidos >= ObjetivoMeteoritosGrandes)
                    {
                        Terminar("SECTOR DESPEJADO // RUMBO A LA MISIÓN");
                        return;
                    }
                    CrearMeteoritoGrande();
                }
                else
                {
                    pequenosDestruidos++;
                    if (pequenosDestruidos >= ObjetivoMeteoritosPequenos)
                    {
                        meteoritos.Clear();
                        CrearMeteoritoGrande();
                    }
                    else CrearMeteoritosPequenos();
                }
                break;
            }

            if (!meteorito.Grande && meteorito.X < -tamano.Width)
            {
                meteorito.X = ClientSize.Width + azar.Next(120, 700);
                meteorito.Y = azar.Next(110, Math.Max(111, ClientSize.Height - 100));
            }
            else if (meteorito.Grande && meteorito.X < -tamano.Width)
            {
                meteorito.X = ClientSize.Width + 120;
                meteorito.Y = azar.Next(120, Math.Max(121, ClientSize.Height - 120));
            }
        }
    }

    private SizeF TamanoMeteorito(Meteorito m)
    {
        Bitmap? imagen = m.Grande ? meteoritoGrande : meteoritoPequeno;
        float ancho = m.Grande ? Math.Clamp(ClientSize.Width * 0.205f, 235, 350) : Math.Clamp(ClientSize.Width * 0.092f, 112, 160);
        float relacion = imagen is null ? 1.45f : (float)imagen.Width / imagen.Height;
        return new SizeF(ancho, ancho / relacion);
    }

    private void MoverEstrellas()
    {
        for (int i = 0; i < estrellas.Length; i++)
        {
            estrellas[i].X -= estrellas[i].Velocidad;
            if (estrellas[i].X < 0)
            {
                estrellas[i].X = ClientSize.Width + azar.Next(0, 120);
                estrellas[i].Y = azar.Next(0, Math.Max(1, ClientSize.Height));
            }
        }
    }

    private void Terminar(string mensaje)
    {
        finalizando = true;
        mensajeFinal = mensaje;
        cuadrosFinal = 52;
        teclas.Clear();
    }

    private void DibujarJuego(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        Rectangle pantalla = lienzo.ClientRectangle;

        if (fondo is not null) g.DrawImage(fondo, pantalla);
        else g.Clear(Color.FromArgb(4, 7, 17));

        using (var sombra = new LinearGradientBrush(pantalla,
                   Color.FromArgb(95, 1, 4, 14), Color.FromArgb(115, 1, 4, 14), 90f))
            g.FillRectangle(sombra, pantalla);

        foreach (PuntoEstrella estrella in estrellas)
        {
            using var pincel = new SolidBrush(Color.FromArgb(estrella.Brillo, 149, 221, 255));
            g.FillEllipse(pincel, estrella.X, estrella.Y, estrella.Radio, estrella.Radio);
        }

        DibujarPanelHud(g);
        DibujarMeteoritos(g);
        DibujarDisparos(g);
        DibujarNave(g);

        if (finalizando) DibujarPantallaFinal(g, pantalla);
    }

    private void DibujarPanelHud(Graphics g)
    {
        int ancho = ClientSize.Width;
        using var panel = new SolidBrush(Color.FromArgb(210, 4, 9, 20));
        using var borde = new Pen(Color.FromArgb(100, 101, 211, 235), 2);
        g.FillRectangle(panel, 20, 18, ancho - 40, 94);
        g.DrawRectangle(borde, 20, 18, ancho - 40, 94);

        using var fuenteTitulo = new Font("Consolas", Math.Clamp(ancho / 75f, 17, 26), FontStyle.Bold);
        using var fuenteDato = new Font("Consolas", Math.Clamp(ancho / 115f, 11, 16), FontStyle.Bold);
        using var cian = new SolidBrush(Color.FromArgb(117, 225, 245));
        using var rojo = new SolidBrush(Color.FromArgb(255, 82, 92));
        using var blanco = new SolidBrush(Color.FromArgb(235, 240, 248));
        g.DrawString("NEXUS // DEFENSA ESPACIAL", fuenteTitulo, cian, 42, 30);
        g.DrawString("VIDA DE LA NAVE:  1 / 1", fuenteDato, rojo, 44, 74);

        string progreso = pequenosDestruidos < ObjetivoMeteoritosPequenos
            ? $"METEORITOS DE LA LLUVIA: {pequenosDestruidos:00} / {ObjetivoMeteoritosPequenos}   ·   3 IMPACTOS CADA UNO"
            : $"METEORITOS GRANDES: {grandesDestruidos:00} / {ObjetivoMeteoritosGrandes}   ·   20 IMPACTOS CADA UNO";
        SizeF medida = g.MeasureString(progreso, fuenteDato);
        g.DrawString(progreso, fuenteDato, blanco, ancho - medida.Width - 46, 42);
        using var controles = new SolidBrush(Color.FromArgb(202, 197, 212));
        g.DrawString("W A S D  MOVER     T  DISPARAR     EVITA LOS IMPACTOS", fuenteDato,
            controles, ancho - medida.Width - 46, 76);
    }

    private void DibujarNave(Graphics g)
    {
        float w = AnchoNave(), h = AltoNave();
        RectangleF destino = new(naveX, naveY - h / 2, w, h);
        if (nave is not null) g.DrawImage(nave, destino);
        else
        {
            PointF[] forma = { new(naveX, naveY), new(naveX + w * 0.78f, naveY - h * 0.42f),
                new(naveX + w, naveY), new(naveX + w * 0.78f, naveY + h * 0.42f) };
            using var pincel = new SolidBrush(Color.FromArgb(85, 207, 232));
            g.FillPolygon(pincel, forma);
        }
    }

    private void DibujarDisparos(Graphics g)
    {
        foreach (Disparo d in disparos)
        {
            RectangleF rect = new(d.X - 22, d.Y - 22, 44, 44);
            if (disparoImagen is not null) g.DrawImage(disparoImagen, rect);
            else
            {
                using var resplandor = new SolidBrush(Color.FromArgb(110, 88, 210, 255));
                using var centro = new SolidBrush(Color.WhiteSmoke);
                g.FillEllipse(resplandor, rect);
                g.FillEllipse(centro, d.X - 8, d.Y - 8, 16, 16);
            }
        }
    }

    private void DibujarMeteoritos(Graphics g)
    {
        foreach (Meteorito m in meteoritos)
        {
            Bitmap? imagen = m.Grande ? meteoritoGrande : meteoritoPequeno;
            SizeF tamano = TamanoMeteorito(m);
            RectangleF rect = new(m.X, m.Y - tamano.Height / 2, tamano.Width, tamano.Height);
            if (imagen is not null) g.DrawImage(imagen, rect);
            else
            {
                using var pincel = new SolidBrush(Color.FromArgb(183, 106, 89, 90));
                g.FillEllipse(pincel, rect);
            }

            float porcentaje = (float)m.Vida / m.VidaMaxima;
            float anchoBarra = Math.Min(tamano.Width * 0.7f, 150);
            float xBarra = rect.X + (rect.Width - anchoBarra) / 2;
            float yBarra = rect.Y - 13;
            g.FillRectangle(Brushes.DarkSlateGray, xBarra, yBarra, anchoBarra, 7);
            using var vida = new SolidBrush(m.Grande ? Color.FromArgb(255, 142, 89) : Color.FromArgb(255, 193, 102));
            g.FillRectangle(vida, xBarra, yBarra, anchoBarra * porcentaje, 7);
        }
    }

    private void DibujarPantallaFinal(Graphics g, Rectangle pantalla)
    {
        using var velo = new SolidBrush(Color.FromArgb(188, 1, 4, 12));
        g.FillRectangle(velo, pantalla);
        using var borde = new Pen(Color.FromArgb(160, 107, 222, 242), 2);
        using var panel = new SolidBrush(Color.FromArgb(239, 4, 10, 22));
        int w = Math.Min(920, pantalla.Width - 48);
        int h = 190;
        Rectangle cuadro = new((pantalla.Width - w) / 2, (pantalla.Height - h) / 2, w, h);
        g.FillRectangle(panel, cuadro);
        g.DrawRectangle(borde, cuadro);
        using var fuente = new Font("Consolas", Math.Clamp(pantalla.Width / 65f, 20, 31), FontStyle.Bold);
        using var detalle = new Font("Consolas", Math.Clamp(pantalla.Width / 110f, 12, 17), FontStyle.Bold);
        using var cian = new SolidBrush(Color.FromArgb(117, 225, 245));
        using var oro = new SolidBrush(Color.FromArgb(255, 206, 119));
        SizeF texto = g.MeasureString(mensajeFinal, fuente);
        g.DrawString(mensajeFinal, fuente, cian, cuadro.Left + (cuadro.Width - texto.Width) / 2, cuadro.Top + 47);
        string avance = pequenosDestruidos < ObjetivoMeteoritosPequenos
            ? $"METEORITOS DE LA LLUVIA: {pequenosDestruidos} / {ObjetivoMeteoritosPequenos}"
            : $"METEORITOS GRANDES: {grandesDestruidos} / {ObjetivoMeteoritosGrandes}";
        SizeF avanceSize = g.MeasureString(avance, detalle);
        g.DrawString(avance, detalle, oro, cuadro.Left + (cuadro.Width - avanceSize.Width) / 2, cuadro.Top + 104);
    }
}
