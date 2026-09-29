namespace cineprint.UI
{
    /// <summary>Rangee de pastilles cliquables (horaires de seance rapides).</summary>
    public class Puces : ControleBase
    {
        string[] elements = Array.Empty<string>();
        readonly List<RectangleF> zones = new();
        int selection = -1, survol = -1, appui = -1;

        public event Action<int>? Clic;

        public Puces()
        {
            Height = S(30);
        }

        public string[] Elements
        {
            get => elements;
            set { elements = value; Calculer(); Invalidate(); }
        }

        public int Selection { get => selection; set { if (selection != value) { selection = value; Invalidate(); } } }

        public int LargeurIdeale
        {
            get
            {
                int total = 0;
                foreach (string e in elements) total += Dessin.Mesurer(e, Theme.CorpsGras).Width + S(24);
                return total + Math.Max(0, elements.Length - 1) * S(6);
            }
        }

        void Calculer()
        {
            zones.Clear();
            float x = Width - LargeurIdeale;
            foreach (string e in elements)
            {
                float w = Dessin.Mesurer(e, Theme.CorpsGras).Width + S(24);
                zones.Add(new RectangleF(x, (Height - S(28)) / 2f, w, S(28)));
                x += w + S(6);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Calculer();
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
            if (i != survol) { survol = i; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survol = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            appui = IndexA(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int i = IndexA(e.Location);
            if (i >= 0 && i == appui) Clic?.Invoke(i);
            appui = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color surface = Surface;
            for (int i = 0; i < elements.Length && i < zones.Count; i++)
            {
                RectangleF r = zones[i];
                bool choisi = i == selection;
                float t = (survol == i ? 0.04f : 0f) + (appui == i ? 0.05f : 0f);
                Color fond = choisi ? Dessin.Voile(surface, Theme.Accent, 0.18f + t) : Dessin.Voile(surface, Theme.Texte, 0.07f + t);
                Dessin.Remplir(g, r, r.Height / 2f, fond);
                Dessin.Texte(g, elements[i], Theme.CorpsGras, Rectangle.Round(r), choisi ? Theme.Accent : Theme.Texte, Dessin.Centre);
            }
        }
    }
}
