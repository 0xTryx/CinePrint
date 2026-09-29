namespace cineprint.UI
{
    public enum StyleBouton { Primaire, Secondaire, Teinte, Destructif, Discret }

    /// <summary>
    /// Bouton dessine : styles primaire, secondaire, teinte, destructif ou discret.
    /// Survol / appui animes, icone optionnelle, etat de chargement (indicateur d'activite).
    /// </summary>
    public class Bouton : ControleAnneau, IButtonControl
    {
        StyleBouton style = StyleBouton.Secondaire;
        string? icone;
        bool chargement;
        float rayon = 8f;
        Font? police;
        int etapeSpinner;
        bool enfonce;
        readonly Anim survol, appui;
        readonly System.Windows.Forms.Timer minuterieSpinner = new() { Interval = 95 };

        public Bouton()
        {
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
            survol = new Anim(this, 0f, 40f);
            appui = new Anim(this, 0f, 30f);
            minuterieSpinner.Tick += (s, e) => { etapeSpinner++; Invalidate(); };
        }

        public StyleBouton Style { get => style; set { style = value; Invalidate(); } }
        public string? Icone { get => icone; set { icone = value; Invalidate(); } }
        public float Rayon { get => rayon; set { rayon = value; Invalidate(); } }
        public Font Police { get => police ?? Theme.CorpsGras; set { police = value; Invalidate(); } }

        public bool Chargement
        {
            get => chargement;
            set
            {
                if (chargement == value) return;
                chargement = value;
                minuterieSpinner.Enabled = value;
                Invalidate();
            }
        }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; AccessibleName = value; Invalidate(); }
        }

        /// <summary>Largeur visible ideale pour le contenu (texte + icone + marges).</summary>
        public int LargeurIdeale
        {
            get
            {
                int w = Dessin.Mesurer(Text, Police).Width;
                if (icone != null || chargement) w += S(16) + (Text.Length > 0 ? S(7) : 0);
                return w + S(Text.Length > 0 ? 32 : 16);
            }
        }

        // --- IButtonControl ---
        public DialogResult DialogResult { get; set; }
        public void NotifyDefault(bool value) { }
        public void PerformClick()
        {
            if (Enabled && Visible && !chargement) OnClick(EventArgs.Empty);
        }

        protected override void OnClick(EventArgs e)
        {
            if (chargement) return;
            base.OnClick(e);
        }

        // --- Interactions ---
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); survol.Vers(1f); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); survol.Vers(0f); appui.Vers(0f); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            enfonce = true;
            appui.Vers(1f);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            appui.Vers(0f);
            // clic = relache dans le bouton apres un appui dedans (glisser dehors pour annuler)
            if (enfonce && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)) PerformClick();
            enfonce = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode is Keys.Space or Keys.Enter) { appui.Vers(1f); e.Handled = true; }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode is Keys.Space or Keys.Enter)
            {
                appui.Vers(0f);
                PerformClick();
                e.Handled = true;
            }
        }

        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Enter || base.IsInputKey(keyData);

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled) { survol.Fixer(0f); appui.Fixer(0f); }
            Invalidate();
        }

        // --- Dessin ---
        (Color fond, Color texte) Couleurs(Color surface)
        {
            if (!Enabled)
            {
                Color f = style == StyleBouton.Discret ? Color.Transparent : Dessin.Voile(surface, Theme.Texte, 0.07f);
                return (f, Theme.TexteTertiaire);
            }

            float s = survol.Valeur, a = appui.Valeur;
            switch (style)
            {
                case StyleBouton.Primaire:
                    Color baseA = Theme.Accent;
                    Color fp = Dessin.Melange(Dessin.Eclaircir(baseA, 0.10f * s), Dessin.Assombrir(baseA, 0.14f), a);
                    return (fp, Color.White);
                case StyleBouton.Teinte:
                    return (Dessin.Voile(surface, Theme.Accent, 0.15f + 0.06f * s + 0.08f * a), Theme.Accent);
                case StyleBouton.Destructif:
                    return (Dessin.Voile(surface, Theme.Rouge, 0.15f + 0.06f * s + 0.08f * a), Theme.Rouge);
                case StyleBouton.Discret:
                    Color fd = s + a <= 0.001f ? Color.Transparent : Dessin.Voile(surface, Theme.Texte, 0.06f * s + 0.06f * a);
                    return (fd, Theme.Accent);
                default:
                    return (Dessin.Voile(surface, Theme.Texte, 0.10f + 0.04f * s + 0.06f * a), Theme.Texte);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF z = Zone;
            (Color fond, Color couleurTexte) = Couleurs(Surface);

            if (fond.A > 0) Dessin.Remplir(g, z, F(rayon), fond);

            // Contenu : [icone|spinner] + texte, centres
            Font p = Police;
            Size mesure = Dessin.Mesurer(Text, p);
            bool avecIcone = icone != null || chargement;
            int tailleIcone = S(16);
            int ecart = Text.Length > 0 && avecIcone ? S(7) : 0;
            int largeurContenu = mesure.Width + (avecIcone ? tailleIcone + ecart : 0);
            float x = z.X + (z.Width - largeurContenu) / 2f;

            if (avecIcone)
            {
                RectangleF zi = new(x, z.Y + (z.Height - tailleIcone) / 2f, tailleIcone, tailleIcone);
                if (chargement)
                    Dessin.Spinner(g, new PointF(zi.X + zi.Width / 2f, zi.Y + zi.Height / 2f), tailleIcone * 0.46f, etapeSpinner, couleurTexte);
                else
                    Dessin.Icone(g, icone!, zi, F(14.5f), couleurTexte);
                x += tailleIcone + ecart;
            }

            if (Text.Length > 0)
            {
                Rectangle zt = new((int)Math.Round(x), (int)z.Y, mesure.Width + S(2), (int)z.Height);
                Dessin.Texte(g, Text, p, zt, couleurTexte);
            }

            DessinerAnneau(g, F(rayon), focus.Valeur);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) minuterieSpinner.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>Boutons de la barre de titre personnalisee (reduire / agrandir / fermer).</summary>
    public class BoutonFenetre : ControleBase
    {
        public enum Genre { Reduire, Agrandir, Fermer }

        readonly Genre genre;
        readonly Anim survol, appui;
        bool fenetreActive = true, enfonce;

        public BoutonFenetre(Genre genre)
        {
            this.genre = genre;
            survol = new Anim(this, 0f, 35f);
            appui = new Anim(this, 0f, 25f);
            TabStop = false;
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleName = genre switch { Genre.Reduire => "Réduire", Genre.Agrandir => "Agrandir", _ => "Fermer" };
        }

        public bool FenetreActive { get => fenetreActive; set { fenetreActive = value; Invalidate(); } }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); survol.Vers(1f); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); survol.Vers(0f); appui.Vers(0f); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            enfonce = e.Button == MouseButtons.Left;
            appui.Vers(1f);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            appui.Vers(0f);
            bool clic = enfonce && ClientRectangle.Contains(e.Location);
            enfonce = false;
            if (clic) Executer();
        }

        void Executer()
        {
            Form? f = FindForm();
            if (f == null) return;
            switch (genre)
            {
                case Genre.Reduire: f.WindowState = FormWindowState.Minimized; break;
                case Genre.Agrandir:
                    f.WindowState = f.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    break;
                default: f.Close(); break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color surface = Surface;
            float s = survol.Valeur, a = appui.Valeur;
            Color couleur = fenetreActive ? Theme.Texte : Theme.TexteTertiaire;

            if (genre == Genre.Fermer)
            {
                Color rouge = Color.FromArgb(196, 43, 28);
                Color fond = Dessin.Melange(surface, Dessin.Melange(rouge, Dessin.Eclaircir(rouge, 0.12f), a), s);
                using (SolidBrush b = new(fond)) g.FillRectangle(b, ClientRectangle);
                couleur = Dessin.Melange(couleur, Color.White, s);
            }
            else
            {
                Color fond = Dessin.Voile(surface, Theme.Texte, 0.07f * s + 0.05f * a);
                using SolidBrush b = new(fond);
                g.FillRectangle(b, ClientRectangle);
            }

            string glyphe = genre switch
            {
                Genre.Reduire => Glyphe.Reduire,
                Genre.Agrandir => FindForm()?.WindowState == FormWindowState.Maximized ? Glyphe.Restaurer : Glyphe.Agrandir,
                _ => Glyphe.Fermer,
            };
            Dessin.Icone(g, glyphe, ClientRectangle, F(genre == Genre.Fermer ? 10.5f : 10f), couleur);
        }
    }
}
