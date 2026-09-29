namespace cineprint.UI
{
    /// <summary>
    /// Carte de formulaire : titre de section, puis lignes (libelle, description optionnelle,
    /// controle ou valeur) separees par un filet.
    /// </summary>
    public class Carte : ControleBase
    {
        public sealed class Ligne
        {
            public string? Libelle;
            public string? Description;
            public Control? Controle;
            public bool Etirer;
            public int Largeur;
            public float Hauteur;
            public string? Valeur;
            public Func<Color>? CouleurValeur;
            public bool Visible = true;
        }

        readonly List<Ligne> lignes = new();
        string? titre;

        public Carte()
        {
            AppliquerTheme();
        }

        public string? Titre { get => titre; set { titre = value; PerformLayout(); Invalidate(); } }
        public float LargeurLibelle { get; set; } = 150;
        public IReadOnlyList<Ligne> Lignes => lignes;

        protected override void AppliquerTheme() => BackColor = Theme.Carte;

        int HauteurEntete => titre != null ? S(30) : 0;

        /// <summary>Hauteur totale necessaire (en pixels reels).</summary>
        public int HauteurIdeale => HauteurEntete + lignes.Where(l => l.Visible).Sum(l => S(l.Hauteur));

        public Ligne AjouterLigne(string libelle, Control controle, bool etirer = true, string? description = null, int largeur = 0)
        {
            Ligne l = new()
            {
                Libelle = libelle,
                Description = description,
                Controle = controle,
                Etirer = etirer,
                Largeur = largeur,
                Hauteur = description != null ? 60 : 50,
            };
            lignes.Add(l);
            Controls.Add(controle);
            PerformLayout();
            return l;
        }

        /// <summary>Ligne d'information : libelle a gauche, valeur (texte secondaire) a droite.</summary>
        public Ligne AjouterInfo(string libelle, string valeur = "")
        {
            Ligne l = new() { Libelle = libelle, Valeur = valeur, Hauteur = 44 };
            lignes.Add(l);
            PerformLayout();
            return l;
        }

        /// <summary>Controle sur toute la largeur de la carte (sans libelle).</summary>
        public Ligne AjouterPleineLargeur(Control controle, float hauteur = 50)
        {
            Ligne l = new() { Controle = controle, Etirer = true, Hauteur = hauteur };
            lignes.Add(l);
            Controls.Add(controle);
            PerformLayout();
            return l;
        }

        public void DefinirValeur(Ligne ligne, string valeur)
        {
            if (ligne.Valeur == valeur) return;
            ligne.Valeur = valeur;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int y = HauteurEntete;
            int marge = S(16);
            foreach (Ligne l in lignes)
            {
                if (l.Controle != null) l.Controle.Visible = l.Visible;
                if (!l.Visible) continue;
                int h = S(l.Hauteur);
                if (l.Controle != null)
                {
                    Control c = l.Controle;
                    int hc = c is ControleAnneau ? c.Height - 2 * ControleAnneau.Marge : c.Height;
                    int yc = y + (h - hc) / 2;
                    if (l.Libelle == null)
                        c.Placer(marge, yc, Width - 2 * marge, hc);
                    else if (l.Etirer)
                    {
                        int x = S(LargeurLibelle);
                        c.Placer(x, yc, Width - x - marge, hc);
                    }
                    else
                    {
                        int w = l.Largeur > 0 ? S(l.Largeur) : (c is ControleAnneau ? c.Width - 2 * ControleAnneau.Marge : c.Width);
                        c.Placer(Width - marge - w, yc, w, hc);
                    }
                }
                y += h;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            int marge = S(16);

            if (titre != null)
                Dessin.Texte(g, titre.ToUpperInvariant(), Theme.Section,
                    new Rectangle(marge, 0, Width - 2 * marge, HauteurEntete - S(7)), Theme.TexteSecondaire,
                    TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            RectangleF boite = new(0, HauteurEntete, Width, Height - HauteurEntete);
            Dessin.RemplirBorde(g, boite, F(12), Theme.Carte, Theme.CarteBordure, Math.Max(1f, F(0.5f)));

            int y = HauteurEntete;
            bool premiere = true;
            foreach (Ligne l in lignes)
            {
                if (!l.Visible) continue;
                int h = S(l.Hauteur);
                if (!premiere) Dessin.Ligne(g, marge, y, Width - marge, Theme.Separateur);
                premiere = false;

                if (l.Libelle != null)
                {
                    int limite = l.Controle == null ? Width : l.Etirer ? S(LargeurLibelle) : l.Controle.Visible().Left;
                    int largeurLibelle = limite - marge - S(8);
                    if (l.Description != null)
                    {
                        Dessin.Texte(g, l.Libelle, Theme.Corps, new Rectangle(marge, y + S(10), largeurLibelle, S(22)), Theme.Texte);
                        Dessin.Texte(g, l.Description, Theme.Petit, new Rectangle(marge, y + S(31), largeurLibelle, S(18)), Theme.TexteSecondaire);
                    }
                    else
                        Dessin.Texte(g, l.Libelle, Theme.Corps, new Rectangle(marge, y, largeurLibelle, h), Theme.Texte);
                }

                if (l.Valeur != null)
                {
                    Color c = l.CouleurValeur?.Invoke() ?? Theme.TexteSecondaire;
                    Dessin.Texte(g, l.Valeur, Theme.Corps, new Rectangle(marge, y, Width - 2 * marge, h), c, Dessin.Droite);
                }
                y += h;
            }
        }
    }
}
