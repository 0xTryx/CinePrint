namespace cineprint.UI
{
    /// <summary>
    /// Barre laterale de navigation : en-tete (logo), sections, elements avec icone
    /// et fond de selection anime entre deux elements.
    /// </summary>
    public class BarreLaterale : ControleBase
    {
        public sealed class Element
        {
            public string Texte = "";
            public string? Glyphe;
            public bool Ticket;
            public Func<Color> Couleur = () => Theme.Gris;
            public string? Section;
        }

        readonly List<Element> elements = new();
        readonly List<Anim> survols = new();
        readonly List<Rectangle> zones = new();
        readonly Anim pilule;
        int selection, survol = -1, appui = -1;
        Control? pied;

        public event Action<int>? SelectionChange;

        const float HauteurEntete = 86, HauteurSection = 30, HauteurElement = 34, Ecart = 2;

        public BarreLaterale()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            pilule = new Anim(this, 0f, 50f);
            AccessibleRole = AccessibleRole.PageTabList;
            AppliquerTheme();
        }

        protected override void AppliquerTheme() => BackColor = Theme.Barre;

        public void Ajouter(Element element)
        {
            elements.Add(element);
            survols.Add(new Anim(this, 0f, 40f));
            CalculerZones();
            pilule.Fixer(zones[selection].Y);
        }

        /// <summary>Controle affiche en bas de la barre (etat de l'imprimante).</summary>
        public Control? Pied
        {
            get => pied;
            set
            {
                if (pied != null) Controls.Remove(pied);
                pied = value;
                if (pied != null) Controls.Add(pied);
                PerformLayout();
            }
        }

        public int Selection
        {
            get => selection;
            set
            {
                if (value < 0 || value >= elements.Count || value == selection) return;
                selection = value;
                pilule.Vers(zones[value].Y);
                AccessibilityNotifyClients(AccessibleEvents.Selection, value);
                Invalidate();
                SelectionChange?.Invoke(value);
            }
        }

        void CalculerZones()
        {
            zones.Clear();
            float y = S(HauteurEntete);
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Section != null) y += S(HauteurSection) + (i > 0 ? S(10) : 0);
                zones.Add(new Rectangle(S(10), (int)y, Math.Max(0, Width - S(20)), S(HauteurElement)));
                y += S(HauteurElement) + S(Ecart);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            CalculerZones();
            if (elements.Count > 0 && pilule.Valeur == pilule.Cible) pilule.Fixer(zones[selection].Y);
            if (pied != null)
            {
                int m = S(12);
                pied.SetBounds(m, Height - m - pied.Height, Width - 2 * m, pied.Height);
            }
        }

        int IndexA(Point p)
        {
            for (int i = 0; i < zones.Count; i++) if (zones[i].Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = IndexA(e.Location);
            if (i == survol) return;
            if (survol >= 0) survols[survol].Vers(0f);
            survol = i;
            if (i >= 0) survols[i].Vers(1f);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (survol >= 0) survols[survol].Vers(0f);
            survol = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            appui = IndexA(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int i = IndexA(e.Location);
            if (i >= 0 && i == appui) Selection = i;
            appui = -1;
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Down) Selection = Math.Min(elements.Count - 1, selection + 1);
            else if (e.KeyCode == Keys.Up) Selection = Math.Max(0, selection - 1);
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color fond = Theme.Barre;

            using (SolidBrush trait = new(Theme.BarreBordure))
                g.FillRectangle(trait, Width - 1, 0, 1, Height);

            // En-tete : logo + nom
            LogoApp.Dessiner(g, new RectangleF(S(20), S(30), S(34), S(34)));
            Dessin.Texte(g, "CinePrint", Theme.Marque, new Rectangle(S(64), S(28), Width - S(72), S(22)), Theme.Texte);
            Dessin.Texte(g, "Billetterie cinéma", Theme.Petit, new Rectangle(S(64), S(49), Width - S(72), S(18)), Theme.TexteSecondaire);

            // Fond de selection (position animee)
            RectangleF rp = new(S(10), pilule.Valeur, Width - S(20), S(HauteurElement));
            if (elements.Count > 0) Dessin.Remplir(g, rp, F(8), Theme.Accent);

            for (int i = 0; i < elements.Count; i++)
            {
                Element el = elements[i];
                Rectangle z = zones[i];

                if (el.Section != null)
                    Dessin.Texte(g, el.Section.ToUpperInvariant(), Theme.Section,
                        new Rectangle(S(20), z.Y - S(HauteurSection), z.Width, S(HauteurSection) - S(6)), Theme.TexteTertiaire,
                        TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

                // recouvrement par le fond de selection -> couleur du texte interpolee
                float recouvrement = Math.Clamp(1f - Math.Abs(rp.Y - z.Y) / z.Height, 0f, 1f);

                float s = survols[i].Valeur * (1f - recouvrement);
                float a = appui == i ? 1f : 0f;
                if (s + a > 0.01f && recouvrement < 0.99f)
                    Dessin.Remplir(g, z, F(8), Dessin.Voile(fond, Theme.Texte, 0.055f * s + 0.05f * a));

                RectangleF icone = new(z.X + S(7), z.Y + (z.Height - S(22)) / 2f, S(22), S(22));
                Dessin.Pastille(g, icone, el.Couleur(), el.Glyphe, el.Ticket);

                Color texte = Dessin.Melange(Theme.Texte, Color.White, recouvrement);
                Dessin.Texte(g, el.Texte, Theme.Corps, new Rectangle(z.X + S(38), z.Y, z.Width - S(44), z.Height), texte);
            }

            if (Focused && ShowFocusCues && elements.Count > 0)
            {
                RectangleF anneau = RectangleF.Inflate(zones[selection], F(2), F(2));
                Dessin.Contour(g, anneau, F(10), Dessin.Alpha(Theme.Accent, 0.5f), F(2));
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Natif.WM_NCHITTEST)
            {
                Point p = PointToClient(new Point((short)(m.LParam.ToInt64() & 0xFFFF), (short)((m.LParam.ToInt64() >> 16) & 0xFFFF)));
                if (p.Y < S(HauteurEntete) - S(6))
                {
                    m.Result = (IntPtr)Natif.HTTRANSPARENT;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
