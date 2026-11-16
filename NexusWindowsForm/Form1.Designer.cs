namespace NexusWindowsForm
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1360, 850);
            Name = "Form1";
            Text = "NEXUS V2.0";
            ResumeLayout(false);
        }
    }
}