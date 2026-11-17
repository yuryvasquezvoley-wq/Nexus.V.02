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
            iniciar = new Button();
            salir = new Button();
            SuspendLayout();
            // 
            // iniciar
            // 
            iniciar.AccessibleName = "Iniciar juego";
            iniciar.BackColor = Color.Transparent;
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
            salir.AccessibleName = "Salir del juego";
            salir.BackColor = Color.Transparent;
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImage = Properties.Resources.MenuReferencia;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1365, 768);
            Controls.Add(salir);
            Controls.Add(iniciar);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
        private Button iniciar;
        private Button salir;
    }
}
