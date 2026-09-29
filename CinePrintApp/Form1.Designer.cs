namespace cineprint
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Les controles sont crees par code dans Form1.Interface.cs (controles du dossier UI) :
        // ici uniquement les proprietes de la fenetre.
        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // Form1
            //
            AutoScaleMode = AutoScaleMode.None;
            Name = "Form1";
            Text = "CinePrint";
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
        }

        #endregion
    }
}
