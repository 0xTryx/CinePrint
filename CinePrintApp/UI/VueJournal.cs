namespace cineprint.UI
{
    public enum TypeLigne { Systeme, Envoi, Reception, Succes, Erreur }

    /// <summary>
    /// Journal des echanges (RichTextBox) : horodatage, sens colore (envoi / reception),
    /// retrait suspendu pour les lignes longues. Les ajouts sont mis en tampon et ecrits
    /// toutes les 40 ms (un envoi d'image genere plusieurs centaines de lignes).
    /// </summary>
    public class VueJournal : ControleBase
    {
        readonly RichTextBox texte;
        readonly List<(string heure, string message, TypeLigne type)> entrees = new();
        readonly List<(string heure, string message, TypeLigne type)> enAttente = new();
        readonly System.Windows.Forms.Timer vidage = new() { Interval = 40 };
        int envois, receptions;

        public Bouton BoutonEffacer { get; }

        public VueJournal()
        {
            texte = new RichTextBox
            {
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = Theme.Mono,
                WordWrap = true,
                TabStop = false,
                Visible = false,
            };
            texte.HandleCreated += (s, e) => ThemeNatif();
            BoutonEffacer = new Bouton { Style = StyleBouton.Discret, Text = "Effacer", Icone = Glyphe.Corbeille };
            BoutonEffacer.Click += (s, e) => Effacer();
            Controls.Add(texte);
            Controls.Add(BoutonEffacer);
            vidage.Tick += (s, e) => Vider();
            AppliquerTheme();
        }

        int HauteurEntete => S(52);

        protected override void AppliquerTheme()
        {
            BackColor = Theme.Carte;
            texte.BackColor = Theme.Carte;
            texte.ForeColor = Theme.Texte;
            ThemeNatif();
            Reconstruire();
        }

        void ThemeNatif()
        {
            if (texte.IsHandleCreated) Natif.SetWindowTheme(texte.Handle, Theme.Sombre ? "DarkMode_Explorer" : "Explorer", null);
        }

        public void Ajouter(string message, TypeLigne type)
        {
            enAttente.Add((DateTime.Now.ToString("HH:mm:ss"), message, type));
            if (type == TypeLigne.Envoi) envois++;
            if (type == TypeLigne.Reception) receptions++;
            if (!vidage.Enabled) vidage.Start();
        }

        public void Effacer()
        {
            entrees.Clear();
            enAttente.Clear();
            envois = receptions = 0;
            texte.Clear();
            texte.Visible = false;
            Invalidate();
        }

        void Vider()
        {
            vidage.Stop();
            if (enAttente.Count == 0) return;
            texte.Visible = true;
            Redessin(false);
            foreach (var e in enAttente) Ecrire(e.heure, e.message, e.type);
            entrees.AddRange(enAttente);
            enAttente.Clear();
            Redessin(true);
            AllerEnBas();
            Invalidate();
        }

        void Reconstruire()
        {
            if (entrees.Count == 0) return;
            Redessin(false);
            texte.Clear();
            foreach (var e in entrees) Ecrire(e.heure, e.message, e.type);
            Redessin(true);
            AllerEnBas();
        }

        /// <summary>Defile jusqu'a la derniere ligne (SB_BOTTOM, valable aussi apres des ajouts page masquee).</summary>
        void AllerEnBas()
        {
            if (!texte.IsHandleCreated) return;
            const int WM_VSCROLL = 0x0115, SB_BOTTOM = 7;
            texte.SelectionStart = texte.TextLength;
            Natif.SendMessage(texte.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated) BeginInvoke(new Action(AllerEnBas));
        }

        void Redessin(bool actif)
        {
            if (!texte.IsHandleCreated) return;
            const int WM_SETREDRAW = 0x000B;
            Natif.SendMessage(texte.Handle, WM_SETREDRAW, (IntPtr)(actif ? 1 : 0), IntPtr.Zero);
            if (actif) texte.Invalidate();
        }

        void Ecrire(string heure, string message, TypeLigne type)
        {
            (string marque, Color couleur) = type switch
            {
                TypeLigne.Envoi => ("→", Theme.Accent),
                TypeLigne.Reception => ("←", Theme.Vert),
                TypeLigne.Succes => ("✓", Theme.Vert),
                TypeLigne.Erreur => ("!", Theme.Rouge),
                _ => ("•", Theme.TexteTertiaire),
            };
            Color couleurMessage = type switch
            {
                TypeLigne.Systeme => Theme.TexteSecondaire,
                TypeLigne.Erreur => Theme.Rouge,
                _ => Theme.Texte,
            };

            // Insertion de la ligne complete, puis mise en forme par plages (saut de ligne AVANT
            // chaque entree sauf la premiere : pas de ligne vide en fin de journal).
            string heureEtMarque = heure + "  " + marque + "  ";
            int debut = texte.TextLength + (texte.TextLength > 0 ? 1 : 0);
            texte.Select(texte.TextLength, 0);
            texte.SelectedText = (texte.TextLength > 0 ? "\n" : "") + heureEtMarque + message;

            texte.Select(debut, heure.Length + 2);
            texte.SelectionColor = Theme.TexteTertiaire;
            texte.Select(debut + heure.Length + 2, marque.Length + 2);
            texte.SelectionColor = couleur;
            texte.Select(debut + heureEtMarque.Length, message.Length);
            texte.SelectionColor = couleurMessage;

            texte.Select(debut, 0);
            texte.SelectionIndent = 0;
            texte.SelectionHangingIndent = TextRenderer.MeasureText(heureEtMarque, Theme.Mono, Size.Empty, TextFormatFlags.NoPadding).Width;
            texte.Select(texte.TextLength, 0);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int lb = BoutonEffacer.LargeurIdeale;
            BoutonEffacer.Placer(Width - S(10) - lb, (HauteurEntete - S(30)) / 2, lb, S(30));
            texte.SetBounds(S(18), HauteurEntete + S(10), Width - S(22), Height - HauteurEntete - S(18));
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Dessin.RemplirBorde(g, new RectangleF(0, 0, Width, Height), F(14), Theme.Carte, Theme.CarteBordure, Math.Max(1f, F(0.5f)));

            Dessin.Texte(g, "Journal", Theme.CorpsGras, new Rectangle(S(18), 0, S(120), HauteurEntete), Theme.Texte);
            int x = S(18) + Dessin.Mesurer("Journal", Theme.CorpsGras).Width + S(14);
            x = Badge(g, x, "→ " + envois, Theme.Accent);
            Badge(g, x + S(6), "← " + receptions, Theme.Vert);

            Dessin.Ligne(g, 0, HauteurEntete, Width, Theme.Separateur);

            if (entrees.Count == 0 && enAttente.Count == 0)
                Dessin.Texte(g, "Aucun échange pour le moment. Connecte l’imprimante puis envoie un ping.", Theme.Corps,
                    new Rectangle(S(18), HauteurEntete + S(14), Width - S(36), S(24)), Theme.TexteTertiaire);
        }

        int Badge(Graphics g, int x, string texteBadge, Color couleur)
        {
            Size m = Dessin.Mesurer(texteBadge, Theme.PetitGras);
            RectangleF r = new(x, (HauteurEntete - S(22)) / 2f, m.Width + S(16), S(22));
            Dessin.Remplir(g, r, r.Height / 2f, Dessin.Voile(Theme.Carte, couleur, 0.16f));
            Dessin.Texte(g, texteBadge, Theme.PetitGras, Rectangle.Round(r), couleur, Dessin.Centre);
            return (int)r.Right;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) vidage.Dispose();
            base.Dispose(disposing);
        }
    }
}
