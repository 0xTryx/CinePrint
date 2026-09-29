namespace cineprint.UI
{
    /// <summary>Controle segmente : options exclusives, curseur de selection anime.</summary>
    public class ControleSegmente : ControleAnneau
    {
        readonly List<(string texte, string? glyphe)> options = new();
        readonly Anim curseur;
        int selection, survol = -1;

        public event EventHandler? SelectionChange;

        public ControleSegmente()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            curseur = new Anim(this, 0f, 50f);
            AccessibleRole = AccessibleRole.PageTabList;
        }

        public void Ajouter(string texte, string? glyphe = null)
        {
            options.Add((texte, glyphe));
            Invalidate();
        }

        public int Selection
        {
            get => selection;
            set
            {
                if (value < 0 || value >= options.Count || value == selection) return;
                selection = value;
                curseur.Vers(value);
                SelectionChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public void FixerSelection(int valeur)
        {
            selection = Math.Clamp(valeur, 0, Math.Max(0, options.Count - 1));
            curseur.Fixer(selection);
        }

        public int LargeurIdeale
        {
            get
            {
                int max = options.Count == 0 ? 0 : options.Max(o => Dessin.Mesurer(o.texte, Theme.CorpsGras).Width + (o.glyphe != null ? S(22) : 0));
                return options.Count * (max + S(28)) + S(4);
            }
        }

        float LargeurSegment => options.Count == 0 ? 0 : (Zone.Width - F(4)) / options.Count;

        int IndexA(Point p)
        {
            if (options.Count == 0) return -1;
            int i = (int)((p.X - Zone.X - F(2)) / LargeurSegment);
            return i >= 0 && i < options.Count && Zone.Contains(p) ? i : -1;
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
            int i = IndexA(e.Location);
            if (i >= 0) Selection = i;
        }

        protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left) Selection = Math.Max(0, selection - 1);
            else if (e.KeyCode == Keys.Right) Selection = Math.Min(options.Count - 1, selection + 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF z = Zone;
            Color surface = Surface;
            Color piste = Dessin.Voile(surface, Theme.Texte, Theme.Sombre ? 0.09f : 0.075f);
            Dessin.Remplir(g, z, F(9), piste);
            if (options.Count == 0) return;

            float w = LargeurSegment;
            RectangleF pouce = new(z.X + F(2) + curseur.Valeur * w, z.Y + F(2), w, z.Height - F(4));

            // separateurs uniquement entre options non adjacentes au curseur
            for (int i = 1; i < options.Count; i++)
            {
                float x = z.X + F(2) + i * w;
                bool pres = Math.Abs(curseur.Valeur - i) < 1.05f || Math.Abs(curseur.Valeur - (i - 1)) < 1.05f;
                if (!pres)
                    Dessin.Ligne(g, x, z.Y + z.Height * 0.3f, x + 1, Dessin.Voile(piste, Theme.Texte, 0.12f));
            }

            if (Theme.Sombre) Dessin.Remplir(g, pouce, F(7), Dessin.Voile(surface, Theme.Texte, 0.30f));
            else
            {
                Dessin.Ombre(g, pouce, F(7), F(4), Color.FromArgb(40, 0, 0, 0), F(1));
                Dessin.Remplir(g, pouce, F(7), Color.White);
            }

            for (int i = 0; i < options.Count; i++)
            {
                (string texte, string? glyphe) = options[i];
                RectangleF seg = new(z.X + F(2) + i * w, z.Y, w, z.Height);
                float proche = Math.Clamp(1f - Math.Abs(curseur.Valeur - i), 0f, 1f);
                Color c = Dessin.Melange(survol == i ? Theme.Texte : Theme.TexteSecondaire, Theme.Texte, proche);
                Font p = proche > 0.5f ? Theme.CorpsGras : Theme.Corps;

                Size mt = Dessin.Mesurer(texte, p);
                int largeur = mt.Width + (glyphe != null ? S(22) : 0);
                float x = seg.X + (seg.Width - largeur) / 2f;
                if (glyphe != null)
                {
                    Dessin.Icone(g, glyphe, new RectangleF(x, seg.Y, S(16), seg.Height), F(12.5f), c);
                    x += S(22);
                }
                Dessin.Texte(g, texte, p, new Rectangle((int)Math.Round(x), (int)seg.Y, mt.Width + S(2), (int)seg.Height), c);
            }

            DessinerAnneau(g, F(9), focus.Valeur);
        }
    }
}
