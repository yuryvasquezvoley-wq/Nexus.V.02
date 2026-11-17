namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

        private void Form1_Load(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click_1(object sender, EventArgs e) { }
        private void pictureBox1_Click_2(object sender, EventArgs e) { }

        private void iniciar_Click(object sender, EventArgs e)
        {
            using Form2 juego = new Form2();
            Hide();
            juego.ShowDialog(this);
            Show();
            Activate();
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

