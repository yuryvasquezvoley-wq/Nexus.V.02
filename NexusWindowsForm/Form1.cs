using System.IO;
using NexusWindowsForm.Persistencia;

namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        private ReproductorAudioBucle? audioMenu;
        private readonly RepositorioPartidasSql repositorioPartidas = new();
        private EstadoPartida? ultimaPartida;
        private readonly Button guardarPartida = new();
        private readonly Button cargarPartida = new();
        private readonly Button eliminarPartida = new();

        public Form1()
        {
            InitializeComponent();
            string rutaAudioMenu = Path.Combine(AppContext.BaseDirectory, "Resources", "SonidoMenu.mp3");
            if (File.Exists(rutaAudioMenu))
            {
                audioMenu = new ReproductorAudioBucle(rutaAudioMenu, 0.45);
                Shown += (_, _) => audioMenu.Iniciar();
                FormClosing += (_, _) => audioMenu.Dispose();
            }
            PrepararZonaClic(iniciar, "INICIAR JUEGO");
            PrepararZonaClic(salir, "SALIR");
            PrepararZonaClic(guardarPartida, "GUARDAR PARTIDA");
            PrepararZonaClic(cargarPartida, "CARGAR PARTIDA");
            PrepararZonaClic(eliminarPartida, "ELIMINAR PARTIDA");
            guardarPartida.Enabled = false;
            guardarPartida.Click += guardarPartida_Click;
            cargarPartida.Click += cargarPartida_Click;
            eliminarPartida.Click += eliminarPartida_Click;
            Controls.AddRange(new Control[] { guardarPartida, cargarPartida, eliminarPartida });
            Resize += (_, _) => AjustarMenu();
            AjustarMenu();
        }

        private static void PrepararZonaClic(Button boton, string nombre)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = Color.FromArgb(220, 35, 55);
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 25, 38);
            boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(65, 15, 30);
            boton.UseVisualStyleBackColor = false;
            boton.AccessibleName = nombre;
            boton.Cursor = Cursors.Hand;
            boton.Text = nombre;
            boton.BackColor = Color.FromArgb(12, 10, 18);
            boton.ForeColor = Color.FromArgb(235, 238, 245);
            boton.Font = new Font("Consolas", 11F, FontStyle.Bold);
            boton.FlatStyle = FlatStyle.Flat;
        }

        private void AjustarMenu()
        {
            int ancho = Math.Min(380, Math.Max(260, (int)(ClientSize.Width * 0.30)));
            int alto = Math.Clamp((int)(ClientSize.Height * 0.067), 46, 58);
            int espacio = Math.Clamp((int)(ClientSize.Height * 0.012), 7, 12);
            int total = alto * 5 + espacio * 4;
            int x = (ClientSize.Width - ancho) / 2;
            int limiteSuperior = Math.Max(20, ClientSize.Height - total - 20);
            int y = Math.Clamp((ClientSize.Height - total) / 2 + (int)(ClientSize.Height * 0.06), 20, limiteSuperior);
            Button[] opciones = { iniciar, guardarPartida, cargarPartida, eliminarPartida, salir };
            foreach (Button opcion in opciones)
            {
                opcion.SetBounds(x, y, ancho, alto);
                y += alto + espacio;
            }
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click_1(object sender, EventArgs e) { }
        private void pictureBox1_Click_2(object sender, EventArgs e) { }

        private void iniciar_Click(object sender, EventArgs e)
        {
            AbrirJuego(cargarDesdeSql: false);
        }

        private void AbrirJuego(bool cargarDesdeSql)
        {
            audioMenu?.Detener();
            using Form2 juego = new() { CargarPartidaAlIniciar = cargarDesdeSql };
            Hide();
            juego.ShowDialog(this);
            if (juego.EstadoAlCerrar != null)
            {
                ultimaPartida = juego.EstadoAlCerrar;
                guardarPartida.Enabled = true;
            }
            Show();
            Activate();
            audioMenu?.Iniciar();
        }

        private void guardarPartida_Click(object? sender, EventArgs e)
        {
            if (ultimaPartida == null)
            {
                MessageBox.Show(this,
                    "Primero inicia o carga una partida y vuelve al menú. También puedes guardar desde el mapa.",
                    "NEXUS // GUARDADO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                repositorioPartidas.Guardar(ultimaPartida);
                MessageBox.Show(this, "Partida guardada en SQL Server.", "NEXUS // GUARDADO",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"No se pudo guardar. Revisa la base NexusGame y la conexión SQL.\n\nDetalle: {ex.Message}",
                    "NEXUS // ERROR SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cargarPartida_Click(object? sender, EventArgs e) => AbrirJuego(cargarDesdeSql: true);

        private void eliminarPartida_Click(object? sender, EventArgs e)
        {
            DialogResult confirmar = MessageBox.Show(this,
                "¿Quieres eliminar la partida guardada y todos sus datos? Esta acción no se puede deshacer.",
                "NEXUS // ELIMINAR PARTIDA", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes) return;

            try
            {
                bool eliminada = repositorioPartidas.Eliminar();
                ultimaPartida = null;
                guardarPartida.Enabled = false;
                MessageBox.Show(this,
                    eliminada ? "La partida y sus datos fueron eliminados de SQL Server." : "No había una partida guardada para eliminar.",
                    "NEXUS // PARTIDA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"No se pudo eliminar. Revisa la base NexusGame y la conexión SQL.\n\nDetalle: {ex.Message}",
                    "NEXUS // ERROR SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
