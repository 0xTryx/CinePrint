using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using cineprint.UI;

namespace cineprint
{
    public partial class Form1 : FenetreModerne
    {
        // --- Socket (meme principe que le TP chatmulti) ---
        TcpClient? client;
        NetworkStream? stream;
        Thread? threadLecture;
        Thread? threadEcriture;

        readonly ConcurrentQueue<string> messagesAEnvoyer = new();

        Bitmap? imageChoisie;
        bool[,]? matriceQr;
        bool connexionEnCours;

        // --- Suivi de l'envoi en cours (barre de progression + notification de fin) ---
        int tramesDuLot;
        string messageFinLot = "";
        BarreAction? barreDuLot;
        readonly System.Windows.Forms.Timer minuterieEnvoi = new() { Interval = 80 };

        // --- Apercus calcules en differe (pas de recalcul a chaque frappe) ---
        readonly System.Windows.Forms.Timer minuterieQr = new() { Interval = 200 };
        readonly System.Windows.Forms.Timer minuterieImage = new() { Interval = 160 };
        int versionApercuImage;
        int octetsImage;

        public Form1()
        {
            InitializeComponent();
            ConstruireInterface();
            minuterieEnvoi.Tick += (s, e) => MajProgressionEnvoi();
            minuterieQr.Tick += (s, e) => { minuterieQr.Stop(); MajQr(); };
            minuterieImage.Tick += (s, e) => { minuterieImage.Stop(); CalculerApercuImage(); };
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            champ_ip.Text = "172.18.197.99";
            champ_port.Text = "8080";
            champ_film.Suggestions = new[]
            {
                "Film de test 1",
                "Film de test 2",
                "Film de test 3",
                "Film de test 4",
                "Film de test 5",
            };
            champ_film.Text = champ_film.Suggestions[0];
            compteur_salle.Valeur = 1;
            selecteur_seance.Valeur = DateTime.Now;
            champ_lienQr.Text = "https://";
            MajEtatConnexion(false);
            MajApercuTicket();
            MajQr();
            MajDetailsImage();
            ActiveControl = barreLaterale;
        }

        // --- CONNEXION ---
        private async void button_connexion_Click(object? sender, EventArgs e)
        {
            if (connexionEnCours) return;

            if (client != null && client.Connected)
            {
                NettoyerConnexion();
                MajEtatConnexion(false);
                Journal("Déconnecté.");
                return;
            }

            string ip = champ_ip.Text.Trim();

            if (string.IsNullOrWhiteSpace(ip))
            {
                Erreur("Adresse IP manquante", "Renseigne-la dans Réglages.");
                barreLaterale.Selection = 4;
                champ_ip.Focus();
                return;
            }

            if (!int.TryParse(champ_port.Text.Trim(), out int port) ||
                port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                Erreur("Port invalide", "Il doit être compris entre 0 et 65535.");
                barreLaterale.Selection = 4;
                champ_port.Focus();
                return;
            }

            // Connexion asynchrone avec delai max de 5 s (le thread UI n'est pas bloque).
            connexionEnCours = true;
            MajEtatConnexion(false);

            try
            {
                NettoyerConnexion();

                TcpClient nouveau = new() { NoDelay = true };
                try
                {
                    using CancellationTokenSource delai = new(TimeSpan.FromSeconds(5));
                    await nouveau.ConnectAsync(ip, port, delai.Token);
                }
                catch
                {
                    nouveau.Dispose();
                    throw;
                }

                client = nouveau;
                stream = client.GetStream();

                threadLecture = new Thread(Lecture) { IsBackground = true };
                threadLecture.Start();

                threadEcriture = new Thread(Ecriture) { IsBackground = true };
                threadEcriture.Start();

                connexionEnCours = false;
                MajEtatConnexion(true);
                Journal("Connecté à " + ip + ":" + port);
                Notification.Afficher(zoneContenu, "Imprimante connectée", ip + ":" + port);
            }
            catch (OperationCanceledException)
            {
                NettoyerConnexion();
                Erreur("Connexion impossible", "Aucune réponse de " + ip + ":" + port + " (délai dépassé).");
            }
            catch (Exception ex)
            {
                NettoyerConnexion();
                Erreur("Connexion impossible", ex.Message);
            }
            finally
            {
                connexionEnCours = false;
                MajEtatConnexion(client != null && client.Connected);
            }
        }

        // --- IMPRIMER LE TICKET ---
        private void button_imprimer_Click(object? sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            string cinema = champ_cinema.Text.Trim();
            string film = champ_film.Text.Trim();
            int salle = compteur_salle.Valeur;
            DateTime seance = selecteur_seance.Valeur;

            if (string.IsNullOrWhiteSpace(cinema) || string.IsNullOrWhiteSpace(film))
            {
                Erreur("Ticket incomplet", "Renseigne le cinéma et le film.");
                return;
            }

            List<string> trames = Protocole.Ticket(cinema, film, salle, seance);
            EnvoyerLot(trames, barre_ticket, "Ticket envoyé", trames.Count + " trames");
            apercu_ticket.AnimerImpression();
        }

        void MajApercuTicket()
        {
            apercu_ticket.Definir(champ_cinema.Text, champ_film.Text, compteur_salle.Valeur, selecteur_seance.Valeur);
            liste_trames.Definir(Protocole.Ticket(champ_cinema.Text.Trim(), champ_film.Text.Trim(), compteur_salle.Valeur, selecteur_seance.Valeur));
            puces_horaires.Selection = Array.IndexOf(horaires, selecteur_seance.Valeur.ToString("HH:mm"));
        }

        // --- IMPRIMER UNE IMAGE ---
        private void button_choisirImage_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Choisir une image à imprimer",
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            ChargerImage(ofd.FileName);
        }

        void ChargerImage(string chemin)
        {
            Bitmap nouvelle;
            try
            {
                // copie en memoire : le fichier n'est pas verrouille
                using FileStream fichier = File.OpenRead(chemin);
                using Image lue = Image.FromStream(fichier);
                nouvelle = new Bitmap(lue);
            }
            catch (Exception ex)
            {
                Erreur("Image illisible", ex.Message);
                return;
            }

            Bitmap? ancienne = imageChoisie;
            imageChoisie = nouvelle;
            zone_image.DefinirImage(imageChoisie, Path.GetFileName(chemin) + " · " + nouvelle.Width + " × " + nouvelle.Height + " px");
            ancienne?.Dispose();
            CalculerApercuImage();
        }

        void MajApercuImage()
        {
            minuterieImage.Stop();
            minuterieImage.Start();
        }

        /// <summary>Tramage calcule en arriere-plan pour l'apercu "rendu thermique".</summary>
        async void CalculerApercuImage()
        {
            if (imageChoisie == null) return;
            int version = ++versionApercuImage;
            int largeur = compteur_largeurImage.Valeur;
            Bitmap copie = new(imageChoisie);
            Color papier = Theme.Papier, encre = Theme.Encre;
            zone_image.DefinirRendu(null, "Calcul du rendu…", true);

            (Bitmap apercu, int octets) = await Task.Run(() =>
            {
                using (copie)
                {
                    byte[] commande = EscPosImage.ConvertirEnCommandeEscPos(copie, largeur);
                    return (EscPosImage.ApercuDepuisCommande(commande, papier, encre), commande.Length);
                }
            });

            if (version != versionApercuImage || IsDisposed)
            {
                apercu.Dispose();
                return;
            }
            octetsImage = octets;
            zone_image.DefinirRendu(apercu, apercu.Width + " × " + apercu.Height + " points", false);
            MajDetailsImage();
        }

        void MajDetailsImage()
        {
            if (imageChoisie == null || octetsImage == 0)
            {
                carte_detailsImage.DefinirValeur(ligne_tailleImage, "Aucune image");
                carte_detailsImage.DefinirValeur(ligne_octetsImage, "—");
                carte_detailsImage.DefinirValeur(ligne_envoiImage, "—");
                return;
            }
            int largeur = (compteur_largeurImage.Valeur + 7) / 8 * 8;
            int hauteur = (octetsImage - 8) / (largeur / 8);
            carte_detailsImage.DefinirValeur(ligne_tailleImage, $"{Mm(largeur)} × {Mm(hauteur)} mm");
            carte_detailsImage.DefinirValeur(ligne_octetsImage, octetsImage.ToString("N0") + " octets");
            carte_detailsImage.DefinirValeur(ligne_envoiImage, EstimationEnvoi(octetsImage, compteur_chunkImage.Valeur));
        }

        private void button_imprimerImage_Click(object? sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            if (imageChoisie == null)
            {
                Erreur("Aucune image", "Choisis ou dépose d’abord une image.");
                return;
            }

            int largeur = compteur_largeurImage.Valeur;
            int octetsParTrame = compteur_chunkImage.Valeur;

            byte[] commande = EscPosImage.ConvertirEnCommandeEscPos(imageChoisie, largeur);
            List<string> trames = new() { Protocole.Init(), Protocole.Alignement(1) };
            trames.AddRange(EscPosImage.Decouper(commande, octetsParTrame));
            trames.Add(Protocole.SautLignes(3));

            EnvoyerLot(trames, barre_image, "Image envoyée", trames.Count + " trames · " + commande.Length.ToString("N0") + " octets");
        }

        // --- IMPRIMER UN QR CODE ---
        void MajQr()
        {
            string contenu = champ_lienQr.Text.Trim();
            if (contenu.Length == 0)
            {
                matriceQr = null;
                apercu_qr.Definir(null, "Saisis un lien ou un texte pour générer le QR code.");
            }
            else
            {
                try
                {
                    matriceQr = EscPosQrCode.GenererMatrice(contenu);
                    apercu_qr.Definir(matriceQr);
                }
                catch (Exception ex)
                {
                    matriceQr = null;
                    apercu_qr.Definir(null, "Impossible de générer ce QR code : " + ex.Message, true);
                }
            }
            MajDetailsQr();
        }

        void MajDetailsQr()
        {
            if (matriceQr == null)
            {
                carte_detailsQr.DefinirValeur(ligne_modulesQr, "—");
                carte_detailsQr.DefinirValeur(ligne_tailleQr, "—");
                carte_detailsQr.DefinirValeur(ligne_envoiQr, "—");
                return;
            }
            int n = matriceQr.GetLength(0);
            int utiles = TailleUtileQr(matriceQr);
            int version = (utiles - 17) / 4;
            int points = n * compteur_pointsModuleQr.Valeur;
            int octets = 8 + (points + 7) / 8 * points;
            carte_detailsQr.DefinirValeur(ligne_modulesQr, $"{utiles} × {utiles} · version {version}");
            carte_detailsQr.DefinirValeur(ligne_tailleQr, $"{Mm(points)} × {Mm(points)} mm");
            carte_detailsQr.DefinirValeur(ligne_envoiQr, EstimationEnvoi(octets, compteur_chunkQr.Valeur));
        }

        private void button_imprimerQr_Click(object? sender, EventArgs e)
        {
            if (!EstConnecte()) return;

            if (matriceQr == null)
            {
                Erreur("Aucun QR code", "Saisis d’abord un lien ou un texte.");
                return;
            }

            int pointsParModule = compteur_pointsModuleQr.Valeur;
            int octetsParTrame = compteur_chunkQr.Valeur;

            byte[] commande = EscPosImage.CommandeDepuisMatriceBinaire(matriceQr, pointsParModule);
            List<string> trames = new() { Protocole.Init(), Protocole.Alignement(1) };
            trames.AddRange(EscPosImage.Decouper(commande, octetsParTrame));
            trames.Add(Protocole.SautLignes(3));

            EnvoyerLot(trames, barre_qr, "QR code envoyé", trames.Count + " trames · " + commande.Length.ToString("N0") + " octets");
        }

        /// <summary>Cote du symbole QR hors marges (etendue des modules noirs).</summary>
        static int TailleUtileQr(bool[,] matrice)
        {
            int n = matrice.GetLength(0), min = n, max = -1;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    if (matrice[x, y])
                    {
                        min = Math.Min(min, x);
                        max = Math.Max(max, x);
                    }
            return max >= min ? max - min + 1 : 0;
        }

        // 203 ppp : 8 points par millimetre
        static string Mm(int points) => (points / 8.0).ToString(points % 8 == 0 ? "0" : "0.0");

        static string EstimationEnvoi(int octets, int octetsParTrame)
        {
            int trames = (octets + octetsParTrame - 1) / octetsParTrame + 3;
            double secondes = trames * 0.03;
            string duree = secondes < 1 ? "< 1 s" : "≈ " + Math.Ceiling(secondes) + " s";
            return trames + " trames · " + duree;
        }

        // --- TESTS MANUELS ---
        private void button_ping_Click(object? sender, EventArgs e)
        {
            if (!EstConnecte()) return;
            Envoyer(Protocole.Ping());
        }

        private void button_envoyer_Click(object? sender, EventArgs e)
        {
            if (!EstConnecte()) return;
            string ligne = champ_manuel.Text.Trim();
            if (ligne.Length == 0) return;
            Envoyer(ligne);
            champ_manuel.Text = "";
            champ_manuel.Focus();
        }

        // --- ENVOI / QUEUE ---
        private void Envoyer(string trame)
        {
            messagesAEnvoyer.Enqueue(trame + "\n");
            Journal(trame, TypeLigne.Envoi);
        }

        /// <summary>Met un lot de trames en file et suit sa progression jusqu'au dernier envoi.</summary>
        private void EnvoyerLot(List<string> trames, BarreAction barre, string titre, string detail)
        {
            foreach (string trame in trames)
                Envoyer(trame);

            tramesDuLot += trames.Count;
            messageFinLot = titre + "|" + detail;
            if (barreDuLot != null && barreDuLot != barre) barreDuLot.Principal.Chargement = false;
            barreDuLot = barre;
            barre.Principal.Chargement = true;
            minuterieEnvoi.Start();
            MajProgressionEnvoi();
        }

        void MajProgressionEnvoi()
        {
            int restantes = messagesAEnvoyer.Count;
            bool connecte = client != null && client.Connected;
            if (restantes > 0 && connecte && tramesDuLot > 0)
            {
                carteConnexion.Progression = Math.Clamp(1f - restantes / (float)tramesDuLot, 0f, 1f);
                return;
            }

            minuterieEnvoi.Stop();
            carteConnexion.Progression = -1f;
            if (barreDuLot != null) barreDuLot.Principal.Chargement = false;
            if (connecte && tramesDuLot > 0)
            {
                string[] message = messageFinLot.Split('|');
                Journal(message[0] + " (" + message[1] + ")", TypeLigne.Succes);
                Notification.Afficher(zoneContenu, message[0], message[1]);
            }
            tramesDuLot = 0;
            barreDuLot = null;
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
            TcpClient? clientLu = client;
            while (client != null && client.Connected && stream != null)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int n = stream.Read(data, 0, data.Length);
                    if (n == 0) break;

                    string recu = Encoding.ASCII.GetString(data, 0, n);
                    foreach (string ligne in recu.Split('\n'))
                        if (ligne.Trim().Length > 0) Journal(ligne.TrimEnd('\r'), TypeLigne.Reception);
                }
                catch { break; }
            }

            // Fin de lecture : si la deconnexion n'est pas volontaire, mise a jour de l'interface.
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (client == null || client != clientLu) return;
                    NettoyerConnexion();
                    MajEtatConnexion(false);
                    Journal("Connexion perdue.", TypeLigne.Erreur);
                    Erreur("Connexion perdue", "L’imprimante ne répond plus.");
                }));
            }
            catch { }
        }

        // --- UI helpers (passerelle Invoke) ---
        private void Journal(string texte, TypeLigne type = TypeLigne.Systeme)
        {
            if (vue_journal.InvokeRequired)
            {
                vue_journal.Invoke(new Action<string, TypeLigne>(Journal), texte, type);
                return;
            }

            vue_journal.Ajouter(texte, type);
        }

        private void Erreur(string titre, string detail)
        {
            Journal(titre + " : " + detail, TypeLigne.Erreur);
            Notification.Afficher(zoneContenu, titre, detail, TypeNotification.Erreur);
        }

        private void MajEtatConnexion(bool connecte)
        {
            EtatConnexion etat = connexionEnCours ? EtatConnexion.Connexion : connecte ? EtatConnexion.Connecte : EtatConnexion.Deconnecte;
            string adresse = Adresse();

            carteConnexion.Etat = etat;
            carteConnexion.Adresse = adresse;
            foreach (BarreAction barre in new[] { barre_ticket, barre_image, barre_qr })
            {
                barre.DefinirEtat(etat);
                barre.Principal.Enabled = connecte;
            }
            bouton_ping.Enabled = connecte;
            bouton_envoyer.Enabled = connecte;

            bouton_connexion.Chargement = etat == EtatConnexion.Connexion;
            bouton_connexion.Text = carteConnexion.Bouton.Text;
            bouton_connexion.Style = connecte ? StyleBouton.Destructif : StyleBouton.Primaire;
            bouton_connexion.Icone = carteConnexion.Bouton.Icone;
            ligne_connexion.Description = etat switch
            {
                EtatConnexion.Connecte => "Connectée à " + adresse,
                EtatConnexion.Connexion => "Connexion en cours…",
                _ => "Hors ligne",
            };
            carte_imprimante.Invalidate();
        }

        string Adresse() => champ_ip.Text.Trim() + ":" + champ_port.Text.Trim();

        void MajAdresse()
        {
            if (client != null && client.Connected) return;
            carteConnexion.Adresse = Adresse();
        }

        private bool EstConnecte()
        {
            if (client != null && client.Connected) return true;
            Erreur("Imprimante hors ligne", "Connecte-toi d’abord à l’Arduino.");
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
        }
    }
}
