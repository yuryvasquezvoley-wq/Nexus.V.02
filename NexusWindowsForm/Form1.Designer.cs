namespace NexusWindowsForm
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;
        private Panel sidebar = null!;
        private Panel content = null!;
        private Panel topbar = null!;
        private Label brandLabel = null!;
        private Label versionLabel = null!;
        private Label onlineLabel = null!;
        private Label navTitleLabel = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            sidebar = new Panel();
            content = new Panel();
            topbar = new Panel();
            brandLabel = new Label();
            versionLabel = new Label();
            onlineLabel = new Label();
            navTitleLabel = new Label();
            SuspendLayout();

            // Top bar: editable in the Windows Forms Designer.
            topbar.BackColor = Color.FromArgb(15, 22, 31);
            topbar.Dock = DockStyle.Top;
            topbar.Height = 66;
            topbar.Padding = new Padding(22, 0, 22, 0);

            brandLabel.AutoSize = true;
            brandLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            brandLabel.ForeColor = Color.FromArgb(220, 55, 65);
            brandLabel.Location = new Point(22, 14);
            brandLabel.Text = "N E X U S";

            versionLabel.AutoSize = true;
            versionLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            versionLabel.ForeColor = Color.FromArgb(150, 164, 178);
            versionLabel.Location = new Point(190, 25);
            versionLabel.Text = "SYSTEM V2.0  /  DIMENSIONAL EXPLORATION";

            onlineLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            onlineLabel.AutoSize = true;
            onlineLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            onlineLabel.ForeColor = Color.FromArgb(72, 201, 132);
            onlineLabel.Location = new Point(1120, 25);
            onlineLabel.Text = "● SISTEMA LISTO";

            topbar.Controls.Add(brandLabel);
            topbar.Controls.Add(versionLabel);
            topbar.Controls.Add(onlineLabel);

            // Left navigation: editable in the Windows Forms Designer.
            sidebar.BackColor = Color.FromArgb(14, 20, 29);
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 225;
            sidebar.Padding = new Padding(14, 18, 14, 14);

            navTitleLabel.AutoSize = true;
            navTitleLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            navTitleLabel.ForeColor = Color.FromArgb(150, 164, 178);
            navTitleLabel.Location = new Point(18, 20);
            navTitleLabel.Text = "CENTRO DE MANDO";
            sidebar.Controls.Add(navTitleLabel);

            // Main work area: each screen is loaded here.
            content.BackColor = Color.FromArgb(10, 15, 22);
            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(28);

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 22);
            ClientSize = new Size(1360, 850);
            Controls.Add(content);
            Controls.Add(sidebar);
            Controls.Add(topbar);
            MinimumSize = new Size(1100, 720);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NEXUS V2.0 - Dimensional System";
            ResumeLayout(false);
        }
    }
}