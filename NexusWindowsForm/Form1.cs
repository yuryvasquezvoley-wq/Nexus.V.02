namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        private Rectangle iniciarBounds;
        private Rectangle salirBounds;
        private const int HoverScale = 12;

        public Form1()
        {
            InitializeComponent();

            // Guardar las posiciones y tamaños originales para restaurarlos al salir del botón.
            iniciarBounds = iniciar.Bounds;
            salirBounds = salir.Bounds;

            ConfigurarEfectoHover(iniciar, iniciarBounds);
            ConfigurarEfectoHover(salir, salirBounds);
        }

        private void ConfigurarEfectoHover(Button boton, Rectangle boundsOriginales)
        {
            // Evita el cambio a blanco del estilo visual predeterminado de Windows.
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            boton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            boton.UseVisualStyleBackColor = false;

            boton.MouseEnter += (sender, e) =>
            {
                int nuevoAncho = boundsOriginales.Width + HoverScale;
                int nuevoAlto = boundsOriginales.Height + HoverScale;

                // Agrandar desde el centro para que el botón parezca acercarse.
                boton.Bounds = new Rectangle(
                    boundsOriginales.X - HoverScale / 2,
                    boundsOriginales.Y - HoverScale / 2,
                    nuevoAncho,
                    nuevoAlto);
            };

            boton.MouseLeave += (sender, e) =>
            {
                boton.Bounds = boundsOriginales;
            };
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
            string ruta = @"C:\Games\Programación\NexusClone\Nexus.V.02\Nexus.V.02\bin\Debug\net10.0\Nexus.V.02.exe";

            if (System.IO.File.Exists(ruta))
            {
                this.Hide();
                System.Diagnostics.Process.Start(ruta);
            }
            else
            {
                MessageBox.Show("El archivo no se encontró en la ruta:\n" + ruta, "Error de ruta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
