namespace cineprint.UI
{
    /// <summary>
    /// Champ de saisie : TextBox sans bordure dans un cadre dessine (anneau de focus, texte
    /// indicatif), avec icone et liste de suggestions optionnelles (remplace la ComboBox editable).
    /// </summary>
    public class ChampTexte : ControleAnneau
    {
        readonly BoiteTexte boite;
        readonly Anim survolChevron;
        string? icone;
        string[] suggestions = Array.Empty<string>();

        /// <summary>Touche Entree pressee dans le champ.</summary>
        public event EventHandler? Valide;

        public ChampTexte()
        {
            survolChevron = new Anim(this, 0f, 40f);
            Cursor = Cursors.IBeam;
            boite = new BoiteTexte { BorderStyle = BorderStyle.None, Font = Theme.Corps };
            boite.GotFocus += (s, e) => { focus.Vers(1f); Invalidate(); };
            boite.LostFocus += (s, e) => focus.Vers(0f);
            boite.TextChanged += (s, e) => OnTextChanged(e);
            boite.KeyDown += Boite_KeyDown;
            Controls.Add(boite);
            AppliquerTheme();
        }

        public TextBox Boite => boite;

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text
        {
            get => boite.Text;
            set => boite.Text = value;
        }

        public string Indication { get => boite.Indication; set { boite.Indication = value; boite.Invalidate(); } }
        public string? Icone { get => icone; set { icone = value; PerformLayout(); Invalidate(); } }
        public int LongueurMax { get => boite.MaxLength; set => boite.MaxLength = value; }

        public bool Mono
        {
            get => boite.Font == Theme.Mono;
            set { boite.Font = value ? Theme.Mono : Theme.Corps; PerformLayout(); }
        }

        /// <summary>Valeurs proposees dans le menu deroulant (chevron a droite).</summary>
        public string[] Suggestions
        {
            get => suggestions;
            set { suggestions = value ?? Array.Empty<string>(); PerformLayout(); Invalidate(); }
        }

        bool AvecListe => suggestions.Length > 0;

        public new void Focus() => boite.Focus();
        public void ToutSelectionner() => boite.SelectAll();

        protected override void AppliquerTheme()
        {
            boite.BackColor = Theme.Champ;
            boite.ForeColor = Theme.Texte;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            RectangleF z = Zone;
            int gauche = (int)z.X + S(11) + (icone != null ? S(22) : 0);
            int droite = (int)z.Right - S(10) - (AvecListe ? S(24) : 0);
            int y = (int)Math.Round(z.Y + (z.Height - boite.Height) / 2f) + 1;
            boite.SetBounds(gauche, y, Math.Max(S(20), droite - gauche), boite.Height);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        RectangleF ZoneChevron
        {
            get
            {
                RectangleF z = Zone;
                return new RectangleF(z.Right - S(30), z.Y + S(4), S(26), z.Height - S(8));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool sur = AvecListe && ZoneChevron.Contains(e.Location);
            survolChevron.Vers(sur ? 1f : 0f);
            Cursor = sur ? Cursors.Default : Cursors.IBeam;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survolChevron.Vers(0f);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (AvecListe && ZoneChevron.Contains(e.Location)) OuvrirListe();
            else boite.Focus();
        }

        void Boite_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Valide?.Invoke(this, EventArgs.Empty);
            }
            else if (AvecListe && (e.KeyCode == Keys.F4 || (e.Alt && e.KeyCode == Keys.Down)))
            {
                e.SuppressKeyPress = true;
                OuvrirListe();
            }
        }

        public void OuvrirListe()
        {
            ContextMenuStrip menu = Menus.Creer();
            foreach (string s in suggestions)
            {
                string valeur = s;
                menu.Items.Add(Menus.Element(s, string.Equals(s, Text, StringComparison.OrdinalIgnoreCase), (o, e) =>
                {
                    Text = valeur;
                    boite.Focus();
                    boite.SelectionStart = boite.TextLength;
                }));
            }
            menu.MinimumSize = new Size((int)Zone.Width, 0);
            menu.Closed += (o, e) => menu.BeginInvoke(new Action(menu.Dispose));
            menu.Show(this, new Point(Marge, Height - Marge + S(5)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF z = Zone;
            float rayon = F(8);
            float f = focus.Valeur;

            Color bordure = Dessin.Melange(Theme.ChampBordure, Theme.Accent, f);
            Dessin.RemplirBorde(g, z, rayon, Theme.Champ, bordure, Math.Max(1f, F(0.5f)));
            DessinerAnneau(g, rayon, f);

            if (icone != null)
            {
                RectangleF zi = new(z.X + S(9), z.Y, S(20), z.Height);
                Dessin.Icone(g, icone, zi, F(13), Dessin.Melange(Theme.TexteSecondaire, Theme.Accent, f));
            }

            if (AvecListe)
            {
                RectangleF zc = ZoneChevron;
                float s = survolChevron.Valeur;
                if (s > 0.01f) Dessin.Remplir(g, zc, F(6), Dessin.Voile(Theme.Champ, Theme.Texte, 0.08f * s));
                Dessin.Icone(g, Glyphe.ChevronBas, zc, F(10), Theme.TexteSecondaire);
            }
        }

        protected override bool AfficherFocus => true;
    }

    /// <summary>TextBox sans bordure qui dessine son propre texte indicatif (couleur du theme).</summary>
    public class BoiteTexte : TextBox
    {
        public string Indication { get; set; } = "";

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == Natif.WM_PAINT && TextLength == 0 && Indication.Length > 0 && IsHandleCreated)
            {
                Natif.RECT r = default;
                Natif.SendMessage(Handle, Natif.EM_GETRECT, IntPtr.Zero, ref r);
                Rectangle zone = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
                if (zone.Width <= 0) zone = ClientRectangle;
                TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                    (TextAlign == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);
                using Graphics g = Graphics.FromHwnd(Handle);
                TextRenderer.DrawText(g, Indication, Font, zone, Theme.TexteTertiaire, BackColor, flags);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (TextLength <= 1) Invalidate();
        }
    }
}
