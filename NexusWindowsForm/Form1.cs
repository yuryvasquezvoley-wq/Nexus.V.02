namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            nave.Visible = false;
            realidad.Visible = false;
            PrepararZonaClic(iniciar, "Iniciar juego");
            PrepararZonaClic(salir, "Salir");
            Resize += (_, _) => AjustarMenu();
            AjustarMenu();
        }

        private static void PrepararZonaClic(Button boton, string nombre)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 230, 235, 190);
            boton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            boton.UseVisualStyleBackColor = false;
            boton.Text = string.Empty;
            boton.AccessibleName = nombre;
            boton.Cursor = Cursors.Hand;
        }

        private void AjustarMenu()
        {
            iniciar.Bounds = new Rectangle(
                (int)(ClientSize.Width * 0.36), (int)(ClientSize.Height * 0.425),
                (int)(ClientSize.Width * 0.28), (int)(ClientSize.Height * 0.09));
            salir.Bounds = new Rectangle(
                (int)(ClientSize.Width * 0.39), (int)(ClientSize.Height * 0.535),
                (int)(ClientSize.Width * 0.22), (int)(ClientSize.Height * 0.09));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click_2(object sender, EventArgs e)
        {
        }

        private void iniciar_Click(object sender, EventArgs e)
        {
            string? directorio = AppContext.BaseDirectory;
            string? proyecto = null;

            // Encuentra el proyecto junto a la solución para que el botón funcione
            // también en otros equipos y configuraciones de compilación.
            for (int nivel = 0; nivel < 8 && directorio is not null; nivel++)
            {
                string candidato = System.IO.Path.Combine(directorio, "Nexus.V.02", "Nexus.V.02.csproj");
                if (System.IO.File.Exists(candidato))
                {
                    proyecto = candidato;
                    break;
                }

                directorio = System.IO.Directory.GetParent(directorio)?.FullName;
            }

            if (proyecto is null)
            {
                MessageBox.Show("No se encontró el proyecto del juego junto a la solución.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string directorioProyecto = System.IO.Path.GetDirectoryName(proyecto)!;
                string configuracion = System.IO.Path.GetFileName(
                    System.IO.Directory.GetParent(AppContext.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar))!.FullName);
                string framework = System.IO.Path.GetFileName(AppContext.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                string ejecutable = System.IO.Path.Combine(
                    directorioProyecto, "bin", configuracion, framework, "Nexus.V.02.exe");

                System.Diagnostics.ProcessStartInfo inicio;
                if (System.IO.File.Exists(ejecutable))
                {
                    // Ejecuta la compilación existente para no reconstruir la aplicación
                    // mientras este menú mantiene cargado NexusWindowsForm.exe.
                    inicio = new System.Diagnostics.ProcessStartInfo(ejecutable)
                    {
                        WorkingDirectory = directorioProyecto,
                        UseShellExecute = true
                    };
                }
                else
                {
                    inicio = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{proyecto}\" --no-launch-profile",
                        WorkingDirectory = directorioProyecto,
                        UseShellExecute = true
                    };
                }

                System.Diagnostics.Process.Start(inicio);
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("No se pudo iniciar .NET. Instala el SDK de .NET 11 para ejecutar NEXUS.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
