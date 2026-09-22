using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace cineprint
{
    public partial class Form1 : Form
    {
        // --- Socket (meme principe que le TP chatmulti) ---
        TcpClient? client;
        NetworkStream? stream;
        Thread? threadLecture;
        Thread? threadEcriture;

        readonly ConcurrentQueue<string> messagesAEnvoyer = new();

        Bitmap? imageChoisie;
        bool[,]? matriceQr;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            textBox_ip.Text = "172.18.197.99";
            textBox_port.Text = "8080";
            textBox_cinema.Text = "Le Royal";
            comboBox_film.Items.AddRange(new object[]
            {
                "Dune - Deuxieme partie",
                "Inception",
                "Interstellar",
                "Le Voyage de Chihiro",
                "Oppenheimer",
            });
            comboBox_film.SelectedIndex = 0;
            dateTimePicker_seance.Value = DateTime.Now.Date.AddHours(20).AddMinutes(30);
            MajEtatConnexion(false);
        }

        // --- CONNEXION ---
        private void button_connexion_Click(object sender, EventArgs e)
        {
            if (client != null && client.Connected)
            {
                NettoyerConnexion();
                MajEtatConnexion(false);
                Journal("Systeme : deconnecte.");
                return;
            }

            string ip = textBox_ip.Text.Trim();

            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show("IP manquante.");
                return;
            }

            if (!int.TryParse(textBox_port.Text.Trim(), out int port) ||
                port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                MessageBox.Show("Port invalide (0 - 65535).");
                return;
            }

            button_connexion.Enabled = false;

            try
            {
                NettoyerConnexion();

                client = new TcpClient(ip, port);
                client.NoDelay = true;
                stream = client.GetStream();

                threadLecture = new Thread(Lecture) { IsBackground = true };
                threadLecture.Start();

                threadEcriture = new Thread(Ecriture) { IsBackground = true };
                threadEcriture.Start();

                MajEtatConnexion(true);
                Journal("Systeme : connecte a " + ip + ":" + port);
            }
            catch (Exception ex)
            {
                NettoyerConnexion();
                MajEtatConnexion(false);
                MessageBox.Show("Erreur de connexion : " + ex.Message);
            }

            button_connexion.Enabled = true;
        }

        // --- IMPRIMER LE TICKET ---
        private void button_imprimer_Click(object sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            string cinema = textBox_cinema.Text.Trim();
            string film = comboBox_film.Text.Trim();
            int salle = (int)numericUpDown_salle.Value;
            DateTime seance = dateTimePicker_seance.Value;

            if (string.IsNullOrWhiteSpace(cinema) || string.IsNullOrWhiteSpace(film))
            {
                MessageBox.Show("Renseigne le cinema et le film.");
                return;
            }

            List<string> trames = Protocole.Ticket(cinema, film, salle, seance);
            foreach (string trame in trames)
                Envoyer(trame);

            Journal("--> Ticket envoye (" + trames.Count + " trames)");
        }

        // --- IMPRIMER UNE IMAGE ---
        private void button_choisirImage_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Choisir une image a imprimer",
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            imageChoisie?.Dispose();
            imageChoisie = new Bitmap(ofd.FileName);
            pictureBox_apercu.Image = imageChoisie;
        }

        private void button_imprimerImage_Click(object sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            if (imageChoisie == null)
            {
                MessageBox.Show("Choisis d'abord une image.");
                return;
            }

            int largeur = (int)numericUpDown_largeurImage.Value;
            int octetsParTrame = (int)numericUpDown_chunkImage.Value;

            byte[] commande = EscPosImage.ConvertirEnCommandeEscPos(imageChoisie, largeur);
            List<string> trames = new() { Protocole.Init(), Protocole.Alignement(1) };
            trames.AddRange(EscPosImage.Decouper(commande, octetsParTrame));
            trames.Add(Protocole.SautLignes(3));

            foreach (string trame in trames)
                Envoyer(trame);

            Journal("--> Image envoyee (" + trames.Count + " trames, " + commande.Length + " octets)");
        }

        // --- IMPRIMER UN QR CODE ---
        private void button_genererQr_Click(object sender, EventArgs e)
        {
            string contenu = textBox_lienQr.Text.Trim();
            if (contenu.Length == 0)
            {
                MessageBox.Show("Rentre un lien ou un texte.");
                return;
            }

            try
            {
                matriceQr = EscPosQrCode.GenererMatrice(contenu);
                pictureBox_qr.Image?.Dispose();
                pictureBox_qr.Image = EscPosQrCode.GenererApercu(matriceQr);
            }
            catch (Exception ex)
            {
                matriceQr = null;
                MessageBox.Show("Impossible de generer ce QR code : " + ex.Message);
            }
        }

        private void button_imprimerQr_Click(object sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            if (matriceQr == null)
            {
                MessageBox.Show("Genere d'abord l'apercu du QR code.");
                return;
            }

            int pointsParModule = (int)numericUpDown_pointsModuleQr.Value;
            int octetsParTrame = (int)numericUpDown_chunkQr.Value;

            byte[] commande = EscPosImage.CommandeDepuisMatriceBinaire(matriceQr, pointsParModule);
            List<string> trames = new() { Protocole.Init(), Protocole.Alignement(1) };
            trames.AddRange(EscPosImage.Decouper(commande, octetsParTrame));
            trames.Add(Protocole.SautLignes(3));

            foreach (string trame in trames)
                Envoyer(trame);

            Journal("--> QR code envoye (" + trames.Count + " trames, " + commande.Length + " octets)");
        }

        // --- TESTS MANUELS ---
        private void button_ping_Click(object sender, EventArgs e)
        {
            if (!EstConnecte()) return;
            Envoyer(Protocole.Ping());
        }

        private void button_envoyer_Click(object sender, EventArgs e)
        {
            if (!EstConnecte()) return;
            string ligne = textBox_manuel.Text.Trim();
            if (ligne.Length == 0) return;
            Envoyer(ligne);
            textBox_manuel.Clear();
        }

        // --- ENVOI / QUEUE ---
        private void Envoyer(string trame)
        {
            messagesAEnvoyer.Enqueue(trame + "\n");
            Journal(">> " + trame);
        }

        public void Ecriture()
        {
            while (client != null && client.Connected && stream != null)
            {
                try
                {
                    if (messagesAEnvoyer.TryDequeue(out string? trame))
                    {
                        byte[] data = Encoding.ASCII.GetBytes(trame);
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch { break; }

                Thread.Sleep(30);
            }
        }

        // --- LECTURE ---
        public void Lecture()
        {
            while (client != null && client.Connected && stream != null)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int n = stream.Read(data, 0, data.Length);
                    if (n == 0) break;

                    string recu = Encoding.ASCII.GetString(data, 0, n).TrimEnd('\r', '\n');
                    Journal("<< " + recu);
                }
                catch { break; }
            }
        }

        // --- UI helpers (passerelle Invoke) ---
        private void Journal(string texte)
        {
            if (richTextBox_log.InvokeRequired)
            {
                richTextBox_log.Invoke(new Action<string>(Journal), texte);
                return;
            }

            richTextBox_log.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + texte + Environment.NewLine);
            richTextBox_log.SelectionStart = richTextBox_log.Text.Length;
            richTextBox_log.ScrollToCaret();
        }

        private void MajEtatConnexion(bool connecte)
        {
            label_statut.Text = connecte ? "Connecte" : "Deconnecte";
            label_statut.ForeColor = connecte ? System.Drawing.Color.Green : System.Drawing.Color.Firebrick;
            button_connexion.Text = connecte ? "Deconnexion" : "Connexion";
            groupBox_ticket.Enabled = connecte;
            groupBox_test.Enabled = connecte;
            groupBox_image.Enabled = connecte;
            button_imprimerQr.Enabled = connecte;
        }

        private bool EstConnecte()
        {
            if (client != null && client.Connected) return true;
            MessageBox.Show("Pas connecte a l'Arduino.");
            return false;
        }

        private void NettoyerConnexion()
        {
            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }
            stream = null;
            client = null;
            while (messagesAEnvoyer.TryDequeue(out _)) { }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            NettoyerConnexion();
            imageChoisie?.Dispose();
            pictureBox_qr.Image?.Dispose();
        }
    }
}
