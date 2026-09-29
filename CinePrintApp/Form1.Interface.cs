using cineprint.UI;

namespace cineprint
{
    /// <summary>
    /// Creation et mise en page des controles (dossier UI).
    /// La logique reseau et protocole reste dans Form1.cs.
    /// </summary>
    public partial class Form1
    {
        // --- Structure de la fenetre ---
        BarreLaterale barreLaterale = null!;
        CarteConnexion carteConnexion = null!;
        Page zoneContenu = null!;
        BoutonFenetre[] boutonsFenetre = null!;
        Page[] pages = null!;
        Page pageTicket = null!, pageImage = null!, pageQr = null!, pageConsole = null!, pageReglages = null!;

        // --- Ticket ---
        Carte carte_seance = null!;
        ChampTexte champ_cinema = null!;
        ChampTexte champ_film = null!;
        Compteur compteur_salle = null!;
        SelecteurSeance selecteur_seance = null!;
        Puces puces_horaires = null!;
        ListeTrames liste_trames = null!;
        ApercuTicket apercu_ticket = null!;
        BarreAction barre_ticket = null!;

        // --- Image ---
        ZoneImage zone_image = null!;
        Carte carte_reglagesImage = null!;
        Compteur compteur_largeurImage = null!;
        Compteur compteur_chunkImage = null!;
        Carte carte_detailsImage = null!;
        Carte.Ligne ligne_tailleImage = null!, ligne_octetsImage = null!, ligne_envoiImage = null!;
        BarreAction barre_image = null!;

        // --- QR code ---
        Carte carte_contenuQr = null!;
        ChampTexte champ_lienQr = null!;
        Carte carte_reglagesQr = null!;
        Compteur compteur_pointsModuleQr = null!;
        Compteur compteur_chunkQr = null!;
        Carte carte_detailsQr = null!;
        Carte.Ligne ligne_modulesQr = null!, ligne_tailleQr = null!, ligne_envoiQr = null!;
        ApercuQr apercu_qr = null!;
        BarreAction barre_qr = null!;

        // --- Console ---
        ChampTexte champ_manuel = null!;
        Bouton bouton_ping = null!;
        Bouton bouton_envoyer = null!;
        VueJournal vue_journal = null!;

        // --- Reglages ---
        Carte carte_imprimante = null!;
        ChampTexte champ_ip = null!;
        ChampTexte champ_port = null!;
        Bouton bouton_connexion = null!;
        Carte.Ligne ligne_connexion = null!;
        Carte carte_apparence = null!;
        ControleSegmente segment_apparence = null!;
        Carte carte_aPropos = null!;

        static readonly string[] horaires = { "14:00", "16:30", "18:00", "20:30", "22:45" };

        static int S(float v) => Theme.S(v);

        void ConstruireInterface()
        {
            SuspendLayout();
            Icon = LogoApp.CreerIcone();
            DefinirTaille(new Size(1180, 760), new Size(1060, 690));

            // --- Barre laterale ---
            barreLaterale = new BarreLaterale { Dock = DockStyle.Left, Width = S(250) };
            barreLaterale.Ajouter(new BarreLaterale.Element { Texte = "Ticket", Ticket = true, Couleur = () => Theme.Rouge, Section = "Impression" });
            barreLaterale.Ajouter(new BarreLaterale.Element { Texte = "Image", Glyphe = Glyphe.Image, Couleur = () => Theme.Orange });
            barreLaterale.Ajouter(new BarreLaterale.Element { Texte = "QR Code", Glyphe = Glyphe.Qr, Couleur = () => Theme.Violet });
            barreLaterale.Ajouter(new BarreLaterale.Element { Texte = "Console", Glyphe = Glyphe.Console, Couleur = () => Theme.Graphite, Section = "Outils" });
            barreLaterale.Ajouter(new BarreLaterale.Element { Texte = "Réglages", Glyphe = Glyphe.Reglages, Couleur = () => Theme.Gris });
            barreLaterale.SelectionChange += AfficherPage;

            carteConnexion = new CarteConnexion();
            carteConnexion.Bouton.Click += button_connexion_Click;
            barreLaterale.Pied = carteConnexion;

            // --- Contenu ---
            zoneContenu = new Page { Dock = DockStyle.Fill };
            pageTicket = new Page { Titre = "Ticket", SousTitre = "Compose un billet et envoie-le à l’imprimante thermique.", Visible = false };
            pageImage = new Page { Titre = "Image", SousTitre = "Imprime une photo ou un logo, tramé en noir et blanc.", Visible = false };
            pageQr = new Page { Titre = "QR Code", SousTitre = "Transforme un lien ou un texte en QR code imprimable.", Visible = false };
            pageConsole = new Page { Titre = "Console", SousTitre = "Envoie des trames brutes et suis les échanges avec l’Arduino.", Visible = false };
            pageReglages = new Page { Titre = "Réglages", SousTitre = "Connexion à l’imprimante et apparence de l’application.", Visible = false };
            pages = new[] { pageTicket, pageImage, pageQr, pageConsole, pageReglages };

            ConstruireTicket();
            ConstruireImage();
            ConstruireQr();
            ConstruireConsole();
            ConstruireReglages();

            foreach (Page p in pages) zoneContenu.Controls.Add(p);

            boutonsFenetre = new[]
            {
                new BoutonFenetre(BoutonFenetre.Genre.Reduire),
                new BoutonFenetre(BoutonFenetre.Genre.Agrandir),
                new BoutonFenetre(BoutonFenetre.Genre.Fermer),
            };
            foreach (BoutonFenetre b in boutonsFenetre)
            {
                b.Size = new Size(S(46), S(32));
                zoneContenu.Controls.Add(b);
                b.BringToFront();
            }
            zoneContenu.Layout += (s, e) =>
            {
                for (int i = 0; i < boutonsFenetre.Length; i++)
                    boutonsFenetre[i].Location = new Point(zoneContenu.Width - (boutonsFenetre.Length - i) * S(46), 0);
            };

            Controls.Add(zoneContenu);
            Controls.Add(barreLaterale);
            AfficherPage(0);
            ResumeLayout(true);
        }

        // --- TICKET ---

        void ConstruireTicket()
        {
            champ_cinema = new ChampTexte { Indication = "Nom du cinéma", Height = S(34) + 2 * ControleAnneau.Marge };
            champ_film = new ChampTexte { Indication = "Titre du film", Height = S(34) + 2 * ControleAnneau.Marge };
            compteur_salle = new Compteur { Minimum = 1, Maximum = 20, Valeur = 1, Size = new Size(S(124) + 2 * ControleAnneau.Marge, S(34) + 2 * ControleAnneau.Marge) };
            selecteur_seance = new SelecteurSeance { Height = S(34) + 2 * ControleAnneau.Marge };
            selecteur_seance.Width = selecteur_seance.LargeurIdeale;
            puces_horaires = new Puces { Elements = horaires };
            puces_horaires.Width = puces_horaires.LargeurIdeale;

            carte_seance = new Carte { Titre = "Séance", LargeurLibelle = 128 };
            carte_seance.AjouterLigne("Cinéma", champ_cinema);
            carte_seance.AjouterLigne("Film", champ_film);
            carte_seance.AjouterLigne("Salle", compteur_salle, etirer: false);
            carte_seance.AjouterLigne("Date et heure", selecteur_seance, etirer: false);
            carte_seance.AjouterLigne("Horaires", puces_horaires, etirer: false);

            liste_trames = new ListeTrames();
            apercu_ticket = new ApercuTicket();
            barre_ticket = new BarreAction("Imprimer le ticket", Glyphe.Imprimante);
            barre_ticket.Principal.Click += button_imprimer_Click;
            barre_ticket.Lien.Click += button_connexion_Click;

            champ_cinema.TextChanged += (s, e) => MajApercuTicket();
            champ_film.TextChanged += (s, e) => MajApercuTicket();
            compteur_salle.ValeurChange += (s, e) => MajApercuTicket();
            selecteur_seance.ValeurChange += (s, e) => MajApercuTicket();
            puces_horaires.Clic += i => selecteur_seance.Valeur = selecteur_seance.Valeur.Date + TimeSpan.Parse(horaires[i]);

            pageTicket.Controls.AddRange(new Control[] { carte_seance, liste_trames, apercu_ticket, barre_ticket });
            pageTicket.Layout += (s, e) => DisposerTicket();
        }

        /// <summary>
        /// Barre d'outils alignee sur le grand titre, a droite de l'en-tete
        /// (sans deborder sur la ligne du sous-titre).
        /// </summary>
        static void PlacerBarre(Page p, BarreAction barre)
        {
            int m = ControleAnneau.Marge;
            int x = p.Marge + Dessin.Mesurer(p.Titre, Theme.Titre).Width + S(32);
            barre.SetBounds(x, S(78) - barre.Height, p.Width - p.Marge + m - x, barre.Height);
        }

        void DisposerTicket()
        {
            Page p = pageTicket;
            int m = p.Marge, haut = p.Haut, bas = p.Height - S(28);
            int largeurApercu = S(340), ecart = S(24);
            int largeurGauche = p.Width - 2 * m - largeurApercu - ecart;
            PlacerBarre(p, barre_ticket);
            carte_seance.SetBounds(m, haut, largeurGauche, carte_seance.HauteurIdeale);
            int yTrames = carte_seance.Bottom + S(20);
            liste_trames.SetBounds(m, yTrames, largeurGauche, Math.Max(S(120), bas - yTrames));
            apercu_ticket.SetBounds(p.Width - m - largeurApercu, haut, largeurApercu, Math.Max(S(260), bas - haut));
        }

        // --- IMAGE ---

        void ConstruireImage()
        {
            zone_image = new ZoneImage();
            zone_image.BoutonChoisir.Click += button_choisirImage_Click;
            zone_image.FichierDepose += ChargerImage;
            zone_image.Mode.SelectionChange += (s, e) => zone_image.Invalidate();

            Size tailleCompteur = new(S(132) + 2 * ControleAnneau.Marge, S(34) + 2 * ControleAnneau.Marge);
            compteur_largeurImage = new Compteur { Minimum = 64, Maximum = 576, Increment = 8, Valeur = 384, Size = tailleCompteur };
            compteur_chunkImage = new Compteur { Minimum = 8, Maximum = 512, Increment = 8, Valeur = 32, Size = tailleCompteur };

            carte_reglagesImage = new Carte { Titre = "Réglages d’impression" };
            carte_reglagesImage.AjouterLigne("Largeur", compteur_largeurImage, etirer: false, description: "en points · 384 = 58 mm");
            carte_reglagesImage.AjouterLigne("Octets par trame", compteur_chunkImage, etirer: false, description: "taille des paquets Arduino");

            carte_detailsImage = new Carte { Titre = "Détails" };
            ligne_tailleImage = carte_detailsImage.AjouterInfo("Taille imprimée", "—");
            ligne_octetsImage = carte_detailsImage.AjouterInfo("Données", "—");
            ligne_envoiImage = carte_detailsImage.AjouterInfo("Envoi", "—");

            barre_image = new BarreAction("Imprimer l’image", Glyphe.Imprimante, "Choisir…", Glyphe.Dossier);
            barre_image.Principal.Click += button_imprimerImage_Click;
            barre_image.Secondaire!.Click += button_choisirImage_Click;
            barre_image.Lien.Click += button_connexion_Click;

            compteur_largeurImage.ValeurChange += (s, e) => MajApercuImage();
            compteur_chunkImage.ValeurChange += (s, e) => MajDetailsImage();

            pageImage.Controls.AddRange(new Control[] { zone_image, carte_reglagesImage, carte_detailsImage, barre_image });
            pageImage.Layout += (s, e) => DisposerImage();
        }

        void DisposerImage()
        {
            Page p = pageImage;
            int m = p.Marge, haut = p.Haut, bas = p.Height - S(28);
            int largeurDroite = S(340), ecart = S(24);
            int xDroite = p.Width - m - largeurDroite;
            PlacerBarre(p, barre_image);
            carte_reglagesImage.SetBounds(xDroite, haut, largeurDroite, carte_reglagesImage.HauteurIdeale);
            carte_detailsImage.SetBounds(xDroite, carte_reglagesImage.Bottom + S(20), largeurDroite, carte_detailsImage.HauteurIdeale);
            zone_image.SetBounds(m, haut + S(30), xDroite - ecart - m, Math.Max(S(200), bas - haut - S(30)));
        }

        // --- QR CODE ---

        void ConstruireQr()
        {
            champ_lienQr = new ChampTexte { Indication = "https://…", Icone = Glyphe.Lien, Height = S(36) + 2 * ControleAnneau.Marge };
            carte_contenuQr = new Carte { Titre = "Contenu" };
            carte_contenuQr.AjouterPleineLargeur(champ_lienQr, 58);

            Size tailleCompteur = new(S(132) + 2 * ControleAnneau.Marge, S(34) + 2 * ControleAnneau.Marge);
            compteur_pointsModuleQr = new Compteur { Minimum = 2, Maximum = 20, Valeur = 4, Size = tailleCompteur };
            compteur_chunkQr = new Compteur { Minimum = 8, Maximum = 512, Increment = 8, Valeur = 32, Size = tailleCompteur };
            carte_reglagesQr = new Carte { Titre = "Réglages d’impression" };
            carte_reglagesQr.AjouterLigne("Points par module", compteur_pointsModuleQr, etirer: false, description: "taille d’un carré du code");
            carte_reglagesQr.AjouterLigne("Octets par trame", compteur_chunkQr, etirer: false, description: "taille des paquets Arduino");

            carte_detailsQr = new Carte { Titre = "Détails" };
            ligne_modulesQr = carte_detailsQr.AjouterInfo("Modules", "—");
            ligne_tailleQr = carte_detailsQr.AjouterInfo("Taille imprimée", "—");
            ligne_envoiQr = carte_detailsQr.AjouterInfo("Envoi", "—");

            apercu_qr = new ApercuQr();
            barre_qr = new BarreAction("Imprimer le QR code", Glyphe.Imprimante);
            barre_qr.Principal.Click += button_imprimerQr_Click;
            barre_qr.Lien.Click += button_connexion_Click;

            champ_lienQr.TextChanged += (s, e) => { minuterieQr.Stop(); minuterieQr.Start(); };
            compteur_pointsModuleQr.ValeurChange += (s, e) => MajDetailsQr();
            compteur_chunkQr.ValeurChange += (s, e) => MajDetailsQr();

            pageQr.Controls.AddRange(new Control[] { carte_contenuQr, carte_reglagesQr, carte_detailsQr, apercu_qr, barre_qr });
            pageQr.Layout += (s, e) => DisposerQr();
        }

        void DisposerQr()
        {
            Page p = pageQr;
            int m = p.Marge, haut = p.Haut;
            int largeurApercu = S(380), ecart = S(24);
            int largeurGauche = p.Width - 2 * m - largeurApercu - ecart;
            PlacerBarre(p, barre_qr);
            carte_contenuQr.SetBounds(m, haut, largeurGauche, carte_contenuQr.HauteurIdeale);
            carte_reglagesQr.SetBounds(m, carte_contenuQr.Bottom + S(20), largeurGauche, carte_reglagesQr.HauteurIdeale);
            carte_detailsQr.SetBounds(m, carte_reglagesQr.Bottom + S(20), largeurGauche, carte_detailsQr.HauteurIdeale);
            // apercu aligne sur la colonne de gauche (memes haut et bas de boite)
            apercu_qr.SetBounds(p.Width - m - largeurApercu, haut + S(30), largeurApercu, Math.Max(S(260), carte_detailsQr.Bottom - haut - S(30)));
        }

        // --- CONSOLE ---

        void ConstruireConsole()
        {
            champ_manuel = new ChampTexte { Indication = "Trame brute — ex. 0,ping  ou  1,Bonjour", Icone = Glyphe.Console, Mono = true };
            bouton_ping = new Bouton { Style = StyleBouton.Teinte, Text = "Ping", Icone = Glyphe.Eclair };
            bouton_envoyer = new Bouton { Style = StyleBouton.Primaire, Text = "Envoyer", Icone = Glyphe.Envoyer };
            vue_journal = new VueJournal();

            bouton_ping.Click += button_ping_Click;
            bouton_envoyer.Click += button_envoyer_Click;
            champ_manuel.Valide += button_envoyer_Click;

            pageConsole.Controls.AddRange(new Control[] { champ_manuel, bouton_ping, bouton_envoyer, vue_journal });
            pageConsole.Layout += (s, e) => DisposerConsole();
        }

        void DisposerConsole()
        {
            Page p = pageConsole;
            int m = p.Marge, haut = p.Haut + S(4);
            int h = S(38);
            int lEnvoyer = bouton_envoyer.LargeurIdeale + S(8), lPing = bouton_ping.LargeurIdeale + S(8);
            bouton_envoyer.Placer(p.Width - m - lEnvoyer, haut, lEnvoyer, h);
            bouton_ping.Placer(p.Width - m - lEnvoyer - S(10) - lPing, haut, lPing, h);
            champ_manuel.Placer(m, haut, p.Width - 2 * m - lEnvoyer - lPing - S(20), h);
            int yJournal = haut + h + S(20);
            vue_journal.SetBounds(m, yJournal, p.Width - 2 * m, Math.Max(S(160), p.Height - yJournal - S(28)));
        }

        // --- REGLAGES ---

        void ConstruireReglages()
        {
            champ_ip = new ChampTexte { Indication = "192.168.1.50", Height = S(34) + 2 * ControleAnneau.Marge };
            champ_port = new ChampTexte { Indication = "8080", Size = new Size(S(120) + 2 * ControleAnneau.Marge, S(34) + 2 * ControleAnneau.Marge) };
            bouton_connexion = new Bouton { Style = StyleBouton.Primaire, Text = "Se connecter", Icone = Glyphe.Lien };
            bouton_connexion.Size = new Size(S(170) + 2 * ControleAnneau.Marge, S(32) + 2 * ControleAnneau.Marge);
            bouton_connexion.Click += button_connexion_Click;

            carte_imprimante = new Carte { Titre = "Imprimante", LargeurLibelle = 150 };
            carte_imprimante.AjouterLigne("Adresse IP", champ_ip);
            carte_imprimante.AjouterLigne("Port", champ_port, etirer: false);
            ligne_connexion = carte_imprimante.AjouterLigne("Connexion", bouton_connexion, etirer: false, description: "Hors ligne");

            segment_apparence = new ControleSegmente();
            segment_apparence.Ajouter("Auto", Glyphe.Contraste);
            segment_apparence.Ajouter("Clair", Glyphe.Soleil);
            segment_apparence.Ajouter("Sombre", Glyphe.Lune);
            segment_apparence.FixerSelection((int)Theme.Apparence);
            segment_apparence.Size = new Size(segment_apparence.LargeurIdeale + 2 * ControleAnneau.Marge, S(32) + 2 * ControleAnneau.Marge);
            segment_apparence.SelectionChange += (s, e) => Theme.DefinirApparence((Apparence)segment_apparence.Selection);

            carte_apparence = new Carte { Titre = "Apparence", LargeurLibelle = 150 };
            carte_apparence.AjouterLigne("Thème", segment_apparence, etirer: false, description: "« Auto » suit le réglage de Windows");

            carte_aPropos = new Carte { Titre = "À propos", LargeurLibelle = 150 };
            carte_aPropos.AjouterInfo("Application", "CinePrint 2.0");
            carte_aPropos.AjouterInfo("Protocole", "0 ping · 1 texte · 2 octets bruts");
            carte_aPropos.AjouterInfo("Imprimante", "ESC/POS thermique · 58 mm · 384 points");

            champ_ip.TextChanged += (s, e) => MajAdresse();
            champ_port.TextChanged += (s, e) => MajAdresse();

            pageReglages.Controls.AddRange(new Control[] { carte_imprimante, carte_apparence, carte_aPropos });
            pageReglages.Layout += (s, e) => DisposerReglages();
        }

        void DisposerReglages()
        {
            Page p = pageReglages;
            int m = p.Marge, haut = p.Haut;
            int largeur = Math.Min(p.Width - 2 * m, S(660));
            carte_imprimante.SetBounds(m, haut, largeur, carte_imprimante.HauteurIdeale);
            carte_apparence.SetBounds(m, carte_imprimante.Bottom + S(22), largeur, carte_apparence.HauteurIdeale);
            carte_aPropos.SetBounds(m, carte_apparence.Bottom + S(22), largeur, carte_aPropos.HauteurIdeale);
        }

        // --- NAVIGATION ---

        void AfficherPage(int index)
        {
            for (int i = 0; i < pages.Length; i++) pages[i].Visible = i == index;
            foreach (BoutonFenetre b in boutonsFenetre) b.BringToFront();
            if (pages[index] == pageConsole) champ_manuel.Focus();
        }

        BarreAction? BarreCourante =>
            pageTicket.Visible ? barre_ticket : pageImage.Visible ? barre_image : pageQr.Visible ? barre_qr : null;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys touche = keyData & Keys.KeyCode;
            if ((keyData & Keys.Modifiers) == Keys.Control)
            {
                if (touche >= Keys.D1 && touche <= Keys.D5)
                {
                    barreLaterale.Selection = touche - Keys.D1;
                    return true;
                }
                if (touche == Keys.P && BarreCourante is BarreAction barre)
                {
                    if (barre.Principal.Enabled) barre.Principal.PerformClick();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (boutonsFenetre != null) foreach (BoutonFenetre b in boutonsFenetre) b.FenetreActive = true;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (boutonsFenetre != null) foreach (BoutonFenetre b in boutonsFenetre) b.FenetreActive = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (boutonsFenetre != null) boutonsFenetre[1].Invalidate();
        }
    }
}
