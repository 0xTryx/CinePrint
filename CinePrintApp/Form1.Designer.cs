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

        private void InitializeComponent()
        {
            label_ip = new Label();
            textBox_ip = new TextBox();
            label_port = new Label();
            textBox_port = new TextBox();
            button_connexion = new Button();
            label_statut = new Label();
            groupBox_ticket = new GroupBox();
            label_cinema = new Label();
            textBox_cinema = new TextBox();
            label_film = new Label();
            comboBox_film = new ComboBox();
            label_salle = new Label();
            numericUpDown_salle = new NumericUpDown();
            label_seance = new Label();
            dateTimePicker_seance = new DateTimePicker();
            button_imprimer = new Button();
            groupBox_test = new GroupBox();
            textBox_manuel = new TextBox();
            button_ping = new Button();
            button_envoyer = new Button();
            label_log = new Label();
            richTextBox_log = new RichTextBox();
            groupBox_image = new GroupBox();
            button_choisirImage = new Button();
            pictureBox_apercu = new PictureBox();
            label_largeurImage = new Label();
            numericUpDown_largeurImage = new NumericUpDown();
            label_chunkImage = new Label();
            numericUpDown_chunkImage = new NumericUpDown();
            button_imprimerImage = new Button();
            groupBox_qr = new GroupBox();
            label_lienQr = new Label();
            textBox_lienQr = new TextBox();
            button_genererQr = new Button();
            pictureBox_qr = new PictureBox();
            label_pointsModuleQr = new Label();
            numericUpDown_pointsModuleQr = new NumericUpDown();
            label_chunkQr = new Label();
            numericUpDown_chunkQr = new NumericUpDown();
            button_imprimerQr = new Button();
            groupBox_ticket.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_salle).BeginInit();
            groupBox_test.SuspendLayout();
            groupBox_image.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_apercu).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_largeurImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_chunkImage).BeginInit();
            groupBox_qr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_qr).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_pointsModuleQr).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_chunkQr).BeginInit();
            SuspendLayout();
            //
            // label_ip
            //
            label_ip.AutoSize = true;
            label_ip.Location = new Point(12, 18);
            label_ip.Name = "label_ip";
            label_ip.Size = new Size(24, 15);
            label_ip.TabIndex = 0;
            label_ip.Text = "IP :";
            //
            // textBox_ip
            //
            textBox_ip.Location = new Point(42, 15);
            textBox_ip.Name = "textBox_ip";
            textBox_ip.Size = new Size(150, 23);
            textBox_ip.TabIndex = 1;
            //
            // label_port
            //
            label_port.AutoSize = true;
            label_port.Location = new Point(204, 18);
            label_port.Name = "label_port";
            label_port.Size = new Size(36, 15);
            label_port.TabIndex = 2;
            label_port.Text = "Port :";
            //
            // textBox_port
            //
            textBox_port.Location = new Point(244, 15);
            textBox_port.Name = "textBox_port";
            textBox_port.Size = new Size(70, 23);
            textBox_port.TabIndex = 3;
            //
            // button_connexion
            //
            button_connexion.Location = new Point(326, 13);
            button_connexion.Name = "button_connexion";
            button_connexion.Size = new Size(120, 27);
            button_connexion.TabIndex = 4;
            button_connexion.Text = "Connexion";
            button_connexion.UseVisualStyleBackColor = true;
            button_connexion.Click += button_connexion_Click;
            //
            // label_statut
            //
            label_statut.AutoSize = true;
            label_statut.Location = new Point(462, 18);
            label_statut.Name = "label_statut";
            label_statut.Size = new Size(72, 15);
            label_statut.TabIndex = 5;
            label_statut.Text = "Deconnecte";
            //
            // groupBox_ticket
            //
            groupBox_ticket.Controls.Add(label_cinema);
            groupBox_ticket.Controls.Add(textBox_cinema);
            groupBox_ticket.Controls.Add(label_film);
            groupBox_ticket.Controls.Add(comboBox_film);
            groupBox_ticket.Controls.Add(label_salle);
            groupBox_ticket.Controls.Add(numericUpDown_salle);
            groupBox_ticket.Controls.Add(label_seance);
            groupBox_ticket.Controls.Add(dateTimePicker_seance);
            groupBox_ticket.Controls.Add(button_imprimer);
            groupBox_ticket.Location = new Point(12, 52);
            groupBox_ticket.Name = "groupBox_ticket";
            groupBox_ticket.Size = new Size(560, 190);
            groupBox_ticket.TabIndex = 6;
            groupBox_ticket.TabStop = false;
            groupBox_ticket.Text = "Ticket";
            //
            // label_cinema
            //
            label_cinema.AutoSize = true;
            label_cinema.Location = new Point(15, 33);
            label_cinema.Name = "label_cinema";
            label_cinema.Size = new Size(56, 15);
            label_cinema.TabIndex = 0;
            label_cinema.Text = "Cinema :";
            //
            // textBox_cinema
            //
            textBox_cinema.Location = new Point(110, 30);
            textBox_cinema.Name = "textBox_cinema";
            textBox_cinema.Size = new Size(220, 23);
            textBox_cinema.TabIndex = 1;
            //
            // label_film
            //
            label_film.AutoSize = true;
            label_film.Location = new Point(15, 69);
            label_film.Name = "label_film";
            label_film.Size = new Size(36, 15);
            label_film.TabIndex = 2;
            label_film.Text = "Film :";
            //
            // comboBox_film
            //
            comboBox_film.Location = new Point(110, 66);
            comboBox_film.Name = "comboBox_film";
            comboBox_film.Size = new Size(300, 23);
            comboBox_film.TabIndex = 3;
            //
            // label_salle
            //
            label_salle.AutoSize = true;
            label_salle.Location = new Point(15, 105);
            label_salle.Name = "label_salle";
            label_salle.Size = new Size(42, 15);
            label_salle.TabIndex = 4;
            label_salle.Text = "Salle :";
            //
            // numericUpDown_salle
            //
            numericUpDown_salle.Location = new Point(110, 103);
            numericUpDown_salle.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_salle.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDown_salle.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_salle.Name = "numericUpDown_salle";
            numericUpDown_salle.Size = new Size(60, 23);
            numericUpDown_salle.TabIndex = 5;
            //
            // label_seance
            //
            label_seance.AutoSize = true;
            label_seance.Location = new Point(15, 143);
            label_seance.Name = "label_seance";
            label_seance.Size = new Size(53, 15);
            label_seance.TabIndex = 6;
            label_seance.Text = "Seance :";
            //
            // dateTimePicker_seance
            //
            dateTimePicker_seance.CustomFormat = "dd/MM/yyyy  HH:mm";
            dateTimePicker_seance.Format = DateTimePickerFormat.Custom;
            dateTimePicker_seance.Location = new Point(110, 140);
            dateTimePicker_seance.Name = "dateTimePicker_seance";
            dateTimePicker_seance.ShowUpDown = true;
            dateTimePicker_seance.Size = new Size(200, 23);
            dateTimePicker_seance.TabIndex = 7;
            //
            // button_imprimer
            //
            button_imprimer.Location = new Point(360, 138);
            button_imprimer.Name = "button_imprimer";
            button_imprimer.Size = new Size(180, 32);
            button_imprimer.TabIndex = 8;
            button_imprimer.Text = "Imprimer le ticket";
            button_imprimer.UseVisualStyleBackColor = true;
            button_imprimer.Click += button_imprimer_Click;
            //
            // groupBox_test
            //
            groupBox_test.Controls.Add(textBox_manuel);
            groupBox_test.Controls.Add(button_ping);
            groupBox_test.Controls.Add(button_envoyer);
            groupBox_test.Location = new Point(12, 248);
            groupBox_test.Name = "groupBox_test";
            groupBox_test.Size = new Size(560, 70);
            groupBox_test.TabIndex = 7;
            groupBox_test.TabStop = false;
            groupBox_test.Text = "Test manuel (trame brute, ex : 1,hello  ou  0,ping)";
            //
            // textBox_manuel
            //
            textBox_manuel.Location = new Point(15, 30);
            textBox_manuel.Name = "textBox_manuel";
            textBox_manuel.Size = new Size(340, 23);
            textBox_manuel.TabIndex = 0;
            //
            // button_ping
            //
            button_ping.Location = new Point(365, 29);
            button_ping.Name = "button_ping";
            button_ping.Size = new Size(80, 26);
            button_ping.TabIndex = 1;
            button_ping.Text = "Ping";
            button_ping.UseVisualStyleBackColor = true;
            button_ping.Click += button_ping_Click;
            //
            // button_envoyer
            //
            button_envoyer.Location = new Point(455, 29);
            button_envoyer.Name = "button_envoyer";
            button_envoyer.Size = new Size(90, 26);
            button_envoyer.TabIndex = 2;
            button_envoyer.Text = "Envoyer";
            button_envoyer.UseVisualStyleBackColor = true;
            button_envoyer.Click += button_envoyer_Click;
            //
            // label_log
            //
            label_log.AutoSize = true;
            label_log.Location = new Point(12, 326);
            label_log.Name = "label_log";
            label_log.Size = new Size(52, 15);
            label_log.TabIndex = 8;
            label_log.Text = "Journal :";
            //
            // richTextBox_log
            //
            richTextBox_log.Location = new Point(12, 344);
            richTextBox_log.Name = "richTextBox_log";
            richTextBox_log.ReadOnly = true;
            richTextBox_log.Size = new Size(560, 180);
            richTextBox_log.TabIndex = 9;
            richTextBox_log.Text = "";
            //
            // groupBox_image
            //
            groupBox_image.Controls.Add(button_choisirImage);
            groupBox_image.Controls.Add(pictureBox_apercu);
            groupBox_image.Controls.Add(label_largeurImage);
            groupBox_image.Controls.Add(numericUpDown_largeurImage);
            groupBox_image.Controls.Add(label_chunkImage);
            groupBox_image.Controls.Add(numericUpDown_chunkImage);
            groupBox_image.Controls.Add(button_imprimerImage);
            groupBox_image.Location = new Point(584, 52);
            groupBox_image.Name = "groupBox_image";
            groupBox_image.Size = new Size(234, 472);
            groupBox_image.TabIndex = 10;
            groupBox_image.TabStop = false;
            groupBox_image.Text = "Image";
            //
            // button_choisirImage
            //
            button_choisirImage.Location = new Point(12, 25);
            button_choisirImage.Name = "button_choisirImage";
            button_choisirImage.Size = new Size(210, 28);
            button_choisirImage.TabIndex = 0;
            button_choisirImage.Text = "Choisir une image...";
            button_choisirImage.UseVisualStyleBackColor = true;
            button_choisirImage.Click += button_choisirImage_Click;
            //
            // pictureBox_apercu
            //
            pictureBox_apercu.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_apercu.Location = new Point(12, 60);
            pictureBox_apercu.Name = "pictureBox_apercu";
            pictureBox_apercu.Size = new Size(210, 150);
            pictureBox_apercu.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_apercu.TabIndex = 1;
            pictureBox_apercu.TabStop = false;
            //
            // label_largeurImage
            //
            label_largeurImage.AutoSize = true;
            label_largeurImage.Location = new Point(12, 224);
            label_largeurImage.Name = "label_largeurImage";
            label_largeurImage.Size = new Size(92, 15);
            label_largeurImage.TabIndex = 2;
            label_largeurImage.Text = "Largeur (points) :";
            //
            // numericUpDown_largeurImage
            //
            numericUpDown_largeurImage.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown_largeurImage.Location = new Point(12, 242);
            numericUpDown_largeurImage.Minimum = new decimal(new int[] { 64, 0, 0, 0 });
            numericUpDown_largeurImage.Maximum = new decimal(new int[] { 576, 0, 0, 0 });
            numericUpDown_largeurImage.Value = new decimal(new int[] { 384, 0, 0, 0 });
            numericUpDown_largeurImage.Name = "numericUpDown_largeurImage";
            numericUpDown_largeurImage.Size = new Size(90, 23);
            numericUpDown_largeurImage.TabIndex = 3;
            //
            // label_chunkImage
            //
            label_chunkImage.AutoSize = true;
            label_chunkImage.Location = new Point(12, 278);
            label_chunkImage.Name = "label_chunkImage";
            label_chunkImage.Size = new Size(120, 15);
            label_chunkImage.TabIndex = 4;
            label_chunkImage.Text = "Octets par trame :";
            //
            // numericUpDown_chunkImage
            //
            numericUpDown_chunkImage.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown_chunkImage.Location = new Point(12, 296);
            numericUpDown_chunkImage.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown_chunkImage.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numericUpDown_chunkImage.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numericUpDown_chunkImage.Name = "numericUpDown_chunkImage";
            numericUpDown_chunkImage.Size = new Size(90, 23);
            numericUpDown_chunkImage.TabIndex = 5;
            //
            // button_imprimerImage
            //
            button_imprimerImage.Location = new Point(12, 335);
            button_imprimerImage.Name = "button_imprimerImage";
            button_imprimerImage.Size = new Size(210, 32);
            button_imprimerImage.TabIndex = 6;
            button_imprimerImage.Text = "Imprimer l'image";
            button_imprimerImage.UseVisualStyleBackColor = true;
            button_imprimerImage.Click += button_imprimerImage_Click;
            //
            // groupBox_qr
            //
            groupBox_qr.Controls.Add(label_lienQr);
            groupBox_qr.Controls.Add(textBox_lienQr);
            groupBox_qr.Controls.Add(button_genererQr);
            groupBox_qr.Controls.Add(pictureBox_qr);
            groupBox_qr.Controls.Add(label_pointsModuleQr);
            groupBox_qr.Controls.Add(numericUpDown_pointsModuleQr);
            groupBox_qr.Controls.Add(label_chunkQr);
            groupBox_qr.Controls.Add(numericUpDown_chunkQr);
            groupBox_qr.Controls.Add(button_imprimerQr);
            groupBox_qr.Location = new Point(830, 52);
            groupBox_qr.Name = "groupBox_qr";
            groupBox_qr.Size = new Size(234, 472);
            groupBox_qr.TabIndex = 11;
            groupBox_qr.TabStop = false;
            groupBox_qr.Text = "QR Code";
            //
            // label_lienQr
            //
            label_lienQr.AutoSize = true;
            label_lienQr.Location = new Point(12, 25);
            label_lienQr.Name = "label_lienQr";
            label_lienQr.Size = new Size(70, 15);
            label_lienQr.TabIndex = 0;
            label_lienQr.Text = "Lien / texte :";
            //
            // textBox_lienQr
            //
            textBox_lienQr.Location = new Point(12, 43);
            textBox_lienQr.Name = "textBox_lienQr";
            textBox_lienQr.Size = new Size(210, 23);
            textBox_lienQr.TabIndex = 1;
            textBox_lienQr.Text = "https://";
            //
            // button_genererQr
            //
            button_genererQr.Location = new Point(12, 75);
            button_genererQr.Name = "button_genererQr";
            button_genererQr.Size = new Size(210, 28);
            button_genererQr.TabIndex = 2;
            button_genererQr.Text = "Generer l'apercu";
            button_genererQr.UseVisualStyleBackColor = true;
            button_genererQr.Click += button_genererQr_Click;
            //
            // pictureBox_qr
            //
            pictureBox_qr.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_qr.Location = new Point(12, 110);
            pictureBox_qr.Name = "pictureBox_qr";
            pictureBox_qr.Size = new Size(210, 210);
            pictureBox_qr.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_qr.TabIndex = 3;
            pictureBox_qr.TabStop = false;
            //
            // label_pointsModuleQr
            //
            label_pointsModuleQr.AutoSize = true;
            label_pointsModuleQr.Location = new Point(12, 332);
            label_pointsModuleQr.Name = "label_pointsModuleQr";
            label_pointsModuleQr.Size = new Size(94, 15);
            label_pointsModuleQr.TabIndex = 4;
            label_pointsModuleQr.Text = "Points / module :";
            //
            // numericUpDown_pointsModuleQr
            //
            numericUpDown_pointsModuleQr.Location = new Point(12, 350);
            numericUpDown_pointsModuleQr.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numericUpDown_pointsModuleQr.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDown_pointsModuleQr.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numericUpDown_pointsModuleQr.Name = "numericUpDown_pointsModuleQr";
            numericUpDown_pointsModuleQr.Size = new Size(90, 23);
            numericUpDown_pointsModuleQr.TabIndex = 5;
            //
            // label_chunkQr
            //
            label_chunkQr.AutoSize = true;
            label_chunkQr.Location = new Point(120, 332);
            label_chunkQr.Name = "label_chunkQr";
            label_chunkQr.Size = new Size(90, 15);
            label_chunkQr.TabIndex = 6;
            label_chunkQr.Text = "Octets/trame :";
            //
            // numericUpDown_chunkQr
            //
            numericUpDown_chunkQr.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown_chunkQr.Location = new Point(120, 350);
            numericUpDown_chunkQr.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown_chunkQr.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numericUpDown_chunkQr.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numericUpDown_chunkQr.Name = "numericUpDown_chunkQr";
            numericUpDown_chunkQr.Size = new Size(90, 23);
            numericUpDown_chunkQr.TabIndex = 7;
            //
            // button_imprimerQr
            //
            button_imprimerQr.Location = new Point(12, 385);
            button_imprimerQr.Name = "button_imprimerQr";
            button_imprimerQr.Size = new Size(210, 32);
            button_imprimerQr.TabIndex = 8;
            button_imprimerQr.Text = "Imprimer le QR code";
            button_imprimerQr.UseVisualStyleBackColor = true;
            button_imprimerQr.Click += button_imprimerQr_Click;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1076, 536);
            Controls.Add(label_ip);
            Controls.Add(textBox_ip);
            Controls.Add(label_port);
            Controls.Add(textBox_port);
            Controls.Add(button_connexion);
            Controls.Add(label_statut);
            Controls.Add(groupBox_ticket);
            Controls.Add(groupBox_test);
            Controls.Add(label_log);
            Controls.Add(richTextBox_log);
            Controls.Add(groupBox_image);
            Controls.Add(groupBox_qr);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "CinePrint - Application de vente de tickets";
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            groupBox_ticket.ResumeLayout(false);
            groupBox_ticket.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_salle).EndInit();
            groupBox_test.ResumeLayout(false);
            groupBox_test.PerformLayout();
            groupBox_image.ResumeLayout(false);
            groupBox_image.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_apercu).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_largeurImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_chunkImage).EndInit();
            groupBox_qr.ResumeLayout(false);
            groupBox_qr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_qr).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_pointsModuleQr).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_chunkQr).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label_ip;
        private TextBox textBox_ip;
        private Label label_port;
        private TextBox textBox_port;
        private Button button_connexion;
        private Label label_statut;
        private GroupBox groupBox_ticket;
        private Label label_cinema;
        private TextBox textBox_cinema;
        private Label label_film;
        private ComboBox comboBox_film;
        private Label label_salle;
        private NumericUpDown numericUpDown_salle;
        private Label label_seance;
        private DateTimePicker dateTimePicker_seance;
        private Button button_imprimer;
        private GroupBox groupBox_test;
        private TextBox textBox_manuel;
        private Button button_ping;
        private Button button_envoyer;
        private Label label_log;
        private RichTextBox richTextBox_log;
        private GroupBox groupBox_image;
        private Button button_choisirImage;
        private PictureBox pictureBox_apercu;
        private Label label_largeurImage;
        private NumericUpDown numericUpDown_largeurImage;
        private Label label_chunkImage;
        private NumericUpDown numericUpDown_chunkImage;
        private Button button_imprimerImage;
        private GroupBox groupBox_qr;
        private Label label_lienQr;
        private TextBox textBox_lienQr;
        private Button button_genererQr;
        private PictureBox pictureBox_qr;
        private Label label_pointsModuleQr;
        private NumericUpDown numericUpDown_pointsModuleQr;
        private Label label_chunkQr;
        private NumericUpDown numericUpDown_chunkQr;
        private Button button_imprimerQr;
    }
}
