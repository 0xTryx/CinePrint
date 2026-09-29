namespace cineprint.UI
{
    /// <summary>
    /// Apercu du QR code : chaque module fait un nombre entier de pixels (pas de flou de mise a
    /// l'echelle). Sans matrice, affiche le message courant (vide ou erreur).
    /// </summary>
    public class ApercuQr : ControleBase
    {
        bool[,]? matrice;
        string message = "Saisis un lien ou un texte pour générer le QR code.";
        bool erreur;

        public ApercuQr()
        {
            AppliquerTheme();
        }

        protected override void AppliquerTheme() => BackColor = Theme.Carte;

        public void Definir(bool[,]? m, string? texte = null, bool estErreur = false)
        {
            matrice = m;
            if (texte != null) message = texte;
            erreur = estErreur;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Dessin.RemplirBorde(g, new RectangleF(0, 0, Width, Height), F(14), Theme.Carte, Theme.CarteBordure, Math.Max(1f, F(0.5f)));

            int dispo = Math.Min(Width, Height) - S(48);
            if (dispo <= 0) return;

            if (matrice == null)
            {
                Dessin.Texte(g, message, Theme.Corps, new Rectangle(S(24), Height / 2 - S(22), Width - S(48), S(44)),
                    erreur ? Theme.Rouge : Theme.TexteSecondaire,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                return;
            }

            int n = matrice.GetLength(0);
            int module = Math.Max(1, dispo / n);
            int cote = module * n;
            int x0 = (Width - cote) / 2, y0 = (Height - cote) / 2;

            Dessin.Ombre(g, new RectangleF(x0, y0, cote, cote), F(12), F(10), Color.FromArgb(Theme.Sombre ? 110 : 30, 0, 0, 0), F(3));
            Dessin.Remplir(g, new RectangleF(x0, y0, cote, cote), F(12), Color.White);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            using SolidBrush noir = new(Color.Black);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    if (matrice[x, y]) g.FillRectangle(noir, x0 + x * module, y0 + y * module, module, module);
        }
    }
}
