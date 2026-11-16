namespace NexusWindowsForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
