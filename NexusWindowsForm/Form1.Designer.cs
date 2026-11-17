namespace NexusWindowsForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            nave = new PictureBox();
            iniciar = new Button();
            salir = new Button();
            realidad = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)nave).BeginInit();
            ((System.ComponentModel.ISupportInitialize)realidad).BeginInit();
            SuspendLayout();
            // 
            // nave
            // 
            nave.BackColor = Color.Transparent;
            nave.BackgroundImageLayout = ImageLayout.Zoom;
            nave.Image = (Image)resources.GetObject("nave.Image");
            nave.Location = new Point(616, 169);
            nave.Name = "nave";
            nave.Size = new Size(266, 200);
            nave.SizeMode = PictureBoxSizeMode.Zoom;
            nave.TabIndex = 0;
            nave.TabStop = false;
            nave.Click += pictureBox1_Click_2;
            // 
            // iniciar
            // 
            iniciar.BackColor = Color.Transparent;
            iniciar.AccessibleName = "Iniciar juego";
            iniciar.BackgroundImage = null;
            iniciar.FlatAppearance.BorderSize = 0;
            iniciar.FlatStyle = FlatStyle.Flat;
            iniciar.Location = new Point(500, 327);
            iniciar.Name = "iniciar";
            iniciar.Size = new Size(380, 66);
            iniciar.TabIndex = 1;
            iniciar.UseVisualStyleBackColor = false;
            iniciar.Click += iniciar_Click;
            // 
            // salir
            // 
            salir.BackColor = Color.Transparent;
            salir.AccessibleName = "Salir del juego";
            salir.BackgroundImage = null;
            salir.FlatAppearance.BorderSize = 0;
            salir.FlatStyle = FlatStyle.Flat;
            salir.Location = new Point(535, 410);
            salir.Name = "salir";
            salir.Size = new Size(300, 62);
            salir.TabIndex = 2;
            salir.TabStop = false;
            salir.UseVisualStyleBackColor = false;
            salir.Click += salir_Click;
            // 
            // realidad
            // 
            realidad.BackColor = Color.Transparent;
            realidad.BackgroundImage = Properties.Resources.realidad_desconocida1;
            realidad.BackgroundImageLayout = ImageLayout.Zoom;
            realidad.Location = new Point(104, 111);
            realidad.Name = "realidad";
            realidad.Size = new Size(635, 383);
            realidad.TabIndex = 3;
            realidad.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImage = Properties.Resources.MenuReferencia;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1365, 768);
            Controls.Add(nave);
            Controls.Add(salir);
            Controls.Add(iniciar);
            Controls.Add(realidad);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            WindowState = FormWindowState.Maximized;
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)nave).EndInit();
            ((System.ComponentModel.ISupportInitialize)realidad).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox nave;
        private Button iniciar;
        private Button salir;
        private PictureBox realidad;
    }
}
