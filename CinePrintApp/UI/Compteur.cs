namespace cineprint.UI
{
    /// <summary>
    /// Selecteur numerique [ - valeur + ] (remplace NumericUpDown) : clic maintenu = repetition,
    /// molette et fleches haut/bas acceptees, valeur editable au clavier.
    /// </summary>
    public class Compteur : ControleAnneau
    {
        readonly BoiteTexte boite;
        readonly Anim survolMoins, survolPlus, appuiMoins, appuiPlus;
        readonly System.Windows.Forms.Timer repetition = new();
        int valeur, minimum, maximum = 100, increment = 1, sensRepetition;

        public event EventHandler? ValeurChange;

        public Compteur()
        {
            survolMoins = new Anim(this, 0f, 40f);
            survolPlus = new Anim(this, 0f, 40f);
            appuiMoins = new Anim(this, 0f, 30f);
            appuiPlus = new Anim(this, 0f, 30f);

            boite = new BoiteTexte
            {
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center,
                Font = Theme.CorpsGras,
                Text = "0",
            };
            boite.GotFocus += (s, e) => focus.Vers(1f);
            boite.LostFocus += (s, e) => { focus.Vers(0f); Valider(); };
            boite.KeyDown += Boite_KeyDown;
            boite.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            Controls.Add(boite);

            repetition.Tick += (s, e) =>
            {
                repetition.Interval = 55;
                Changer(sensRepetition);
            };
            AppliquerTheme();
        }

        public int Minimum { get => minimum; set { minimum = value; Valeur = valeur; Invalidate(); } }
        public int Maximum { get => maximum; set { maximum = value; Valeur = valeur; Invalidate(); } }
        public int Increment { get => increment; set => increment = Math.Max(1, value); }

        public int Valeur
        {
            get => valeur;
            set
            {
                int v = Math.Clamp(value, minimum, maximum);
                bool change = v != valeur;
                valeur = v;
                if (boite.Text != v.ToString()) boite.Text = v.ToString();
                Invalidate();
                if (change) ValeurChange?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void AppliquerTheme()
        {
            boite.BackColor = Theme.Champ;
            boite.ForeColor = Theme.Texte;
        }

        void Changer(int sens)
        {
            Valider();
            Valeur = valeur + sens * increment;
        }

        void Valider()
        {
            if (int.TryParse(boite.Text, out int v)) Valeur = v;
            else boite.Text = valeur.ToString();
        }

        void Boite_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: Changer(+1); e.Handled = true; break;
                case Keys.Down: Changer(-1); e.Handled = true; break;
                case Keys.Enter: Valider(); boite.SelectAll(); e.SuppressKeyPress = true; break;
            }
        }

        RectangleF ZoneMoins { get { RectangleF z = Zone; return new RectangleF(z.X + S(3), z.Y + S(3), z.Height - S(6), z.Height - S(6)); } }
        RectangleF ZonePlus { get { RectangleF z = Zone; return new RectangleF(z.Right - z.Height + S(3), z.Y + S(3), z.Height - S(6), z.Height - S(6)); } }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            RectangleF z = Zone;
            int gauche = (int)(z.X + z.Height);
            int largeur = (int)(z.Width - 2 * z.Height);
            boite.SetBounds(gauche, (int)Math.Round(z.Y + (z.Height - boite.Height) / 2f) + 1, Math.Max(S(20), largeur), boite.Height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            survolMoins.Vers(ZoneMoins.Contains(e.Location) ? 1f : 0f);
            survolPlus.Vers(ZonePlus.Contains(e.Location) ? 1f : 0f);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survolMoins.Vers(0f);
            survolPlus.Vers(0f);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int sens = ZoneMoins.Contains(e.Location) ? -1 : ZonePlus.Contains(e.Location) ? +1 : 0;
            if (sens == 0) { boite.Focus(); return; }
            (sens < 0 ? appuiMoins : appuiPlus).Vers(1f);
            Changer(sens);
            sensRepetition = sens;
            repetition.Interval = 380;
            repetition.Start();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            repetition.Stop();
            appuiMoins.Vers(0f);
            appuiPlus.Vers(0f);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta != 0) Changer(Math.Sign(e.Delta));
            if (e is HandledMouseEventArgs h) h.Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF z = Zone;
            float rayon = F(8);
            float f = focus.Valeur;
            Dessin.RemplirBorde(g, z, rayon, Theme.Champ, Dessin.Melange(Theme.ChampBordure, Theme.Accent, f), Math.Max(1f, F(0.5f)));
            DessinerAnneau(g, rayon, f);

            DessinerBouton(g, ZoneMoins, Glyphe.Moins, survolMoins.Valeur, appuiMoins.Valeur, valeur > minimum);
            DessinerBouton(g, ZonePlus, Glyphe.Plus, survolPlus.Valeur, appuiPlus.Valeur, valeur < maximum);
        }

        void DessinerBouton(Graphics g, RectangleF r, string glyphe, float survol, float appui, bool actif)
        {
            if (actif && survol + appui > 0.01f)
                Dessin.Remplir(g, r, F(6), Dessin.Voile(Theme.Champ, Theme.Texte, 0.08f * survol + 0.08f * appui));
            Dessin.Icone(g, glyphe, r, F(11), actif ? Theme.Texte : Theme.TexteTertiaire);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) repetition.Dispose();
            base.Dispose(disposing);
        }
    }
}
