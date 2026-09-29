using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cineprint.UI
{
    /// <summary>
    /// Fonctions de dessin communes : rectangles arrondis a courbure continue, melanges de
    /// couleurs, icones vectorielles, ombres floues, indicateur d'activite.
    /// </summary>
    public static class Dessin
    {
        public static void Lisser(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }

        static float Rad(float degres) => degres * MathF.PI / 180f;

        // --- Formes ---

        /// <summary>
        /// Rectangle arrondi a courbure continue (methode "smooth corners") : chaque coin est une
        /// Bezier + un arc + une Bezier, et commence a (1 + lissage) * rayon du sommet.
        /// Le lissage est reduit automatiquement si le rectangle est trop petit.
        /// </summary>
        public static GraphicsPath Arrondi(RectangleF r, float rayon, float lissage = 0.6f)
        {
            GraphicsPath p = new();
            float budget = Math.Min(r.Width, r.Height) / 2f;
            rayon = Math.Min(rayon, budget);
            if (rayon < 0.5f || r.Width <= 0 || r.Height <= 0)
            {
                p.AddRectangle(r);
                return p;
            }

            float lis = lissage;
            if ((1 + lis) * rayon > budget) lis = Math.Max(0f, budget / rayon - 1f);

            float pp = (1 + lis) * rayon;
            float arc = 90f * (1 - lis);
            float arcLong = MathF.Sin(Rad(arc / 2)) * rayon * MathF.Sqrt(2);
            float alpha = (90f - arc) / 2f;
            float p3p4 = rayon * MathF.Tan(Rad(alpha / 2));
            float beta = 45f * lis;
            float c = p3p4 * MathF.Cos(Rad(beta));
            float d = c * MathF.Tan(Rad(beta));
            float b = (pp - arcLong - c - d) / 3f;
            float a = 2 * b;
            float u = pp - a - b - c;
            float L = r.Left, T = r.Top, R = r.Right, B = r.Bottom, D = 2 * rayon;

            // haut -> coin haut-droit
            p.AddLine(L + pp, T, R - pp, T);
            p.AddBezier(R - pp, T, R - pp + a, T, R - pp + a + b, T, R - u, T + d);
            if (arc > 0.01f) p.AddArc(R - D, T, D, D, 270 + alpha, arc);
            p.AddBezier(R - d, T + u, R, T + pp - a - b, R, T + pp - a, R, T + pp);
            // droite -> coin bas-droit
            p.AddLine(R, T + pp, R, B - pp);
            p.AddBezier(R, B - pp, R, B - pp + a, R, B - pp + a + b, R - d, B - u);
            if (arc > 0.01f) p.AddArc(R - D, B - D, D, D, alpha, arc);
            p.AddBezier(R - u, B - d, R - pp + a + b, B, R - pp + a, B, R - pp, B);
            // bas -> coin bas-gauche
            p.AddLine(R - pp, B, L + pp, B);
            p.AddBezier(L + pp, B, L + pp - a, B, L + pp - a - b, B, L + u, B - d);
            if (arc > 0.01f) p.AddArc(L, B - D, D, D, 90 + alpha, arc);
            p.AddBezier(L + d, B - u, L, B - pp + a + b, L, B - pp + a, L, B - pp);
            // gauche -> coin haut-gauche
            p.AddLine(L, B - pp, L, T + pp);
            p.AddBezier(L, T + pp, L, T + pp - a, L, T + pp - a - b, L + d, T + u);
            if (arc > 0.01f) p.AddArc(L, T, D, D, 180 + alpha, arc);
            p.AddBezier(L + u, T + d, L + pp - a - b, T, L + pp - a, T, L + pp, T);
            p.CloseFigure();
            return p;
        }

        public static void Remplir(Graphics g, RectangleF r, float rayon, Color couleur)
        {
            if (couleur.A == 0) return;
            using GraphicsPath chemin = Arrondi(r, rayon);
            using SolidBrush pinceau = new(couleur);
            g.FillPath(pinceau, chemin);
        }

        /// <summary>Forme remplie avec une bordure nette (deux remplissages, pas de trait flou).</summary>
        public static void RemplirBorde(Graphics g, RectangleF r, float rayon, Color fond, Color bordure, float epaisseur = 1f)
        {
            Remplir(g, r, rayon, bordure);
            RectangleF interieur = RectangleF.Inflate(r, -epaisseur, -epaisseur);
            Remplir(g, interieur, Math.Max(0, rayon - epaisseur), fond);
        }

        /// <summary>Contour seul (anneau de focus...).</summary>
        public static void Contour(Graphics g, RectangleF r, float rayon, Color couleur, float epaisseur)
        {
            if (couleur.A == 0) return;
            using GraphicsPath exterieur = Arrondi(r, rayon);
            using GraphicsPath interieur = Arrondi(RectangleF.Inflate(r, -epaisseur, -epaisseur), Math.Max(0, rayon - epaisseur));
            using Region region = new(exterieur);
            region.Exclude(interieur);
            using SolidBrush pinceau = new(couleur);
            g.FillRegion(pinceau, region);
        }

        public static void Ligne(Graphics g, float x1, float y, float x2, Color couleur)
        {
            using SolidBrush pinceau = new(couleur);
            g.FillRectangle(pinceau, x1, y, x2 - x1, 1);
        }

        // --- Couleurs ---

        public static Color Melange(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)Math.Round(a.A + (b.A - a.A) * t),
                (int)Math.Round(a.R + (b.R - a.R) * t),
                (int)Math.Round(a.G + (b.G - a.G) * t),
                (int)Math.Round(a.B + (b.B - a.B) * t));
        }

        /// <summary>Couleur opaque equivalente a "couleur" posee avec l'opacite donnee sur "fond".</summary>
        public static Color Voile(Color fond, Color couleur, float opacite) => Melange(fond, Color.FromArgb(255, couleur), opacite);

        public static Color Alpha(Color c, float opacite) => Color.FromArgb((int)Math.Round(Math.Clamp(opacite, 0f, 1f) * 255), c);

        public static Color Eclaircir(Color c, float t) => Melange(c, Color.White, t);
        public static Color Assombrir(Color c, float t) => Melange(c, Color.Black, t);

        // --- Icones ---

        /// <summary>Dessine un glyphe Segoe Fluent Icons centre (optiquement) dans la zone.</summary>
        public static void Icone(Graphics g, string glyphe, RectangleF zone, float taille, Color couleur)
        {
            using GraphicsPath chemin = new();
            chemin.AddString(glyphe, Theme.Icones, (int)FontStyle.Regular, taille, PointF.Empty, StringFormat.GenericTypographic);
            RectangleF b = chemin.GetBounds();
            if (b.Width <= 0 || b.Height <= 0) return;
            float dx = MathF.Round(zone.X + (zone.Width - b.Width) / 2f - b.X);
            float dy = MathF.Round(zone.Y + (zone.Height - b.Height) / 2f - b.Y);
            using (Matrix m = new())
            {
                m.Translate(dx, dy);
                chemin.Transform(m);
            }
            using SolidBrush pinceau = new(couleur);
            g.FillPath(pinceau, chemin);
        }

        /// <summary>Silhouette de ticket (encoches laterales + pointilles de detachement).</summary>
        public static GraphicsPath CheminTicket(RectangleF r)
        {
            GraphicsPath p = new() { FillMode = FillMode.Alternate };
            float rc = r.Height * 0.16f;
            float rn = r.Height * 0.17f;
            float yc = r.Top + r.Height / 2f;
            float L = r.Left, T = r.Top, R = r.Right, B = r.Bottom;

            p.AddArc(L, T, 2 * rc, 2 * rc, 180, 90);
            p.AddArc(R - 2 * rc, T, 2 * rc, 2 * rc, 270, 90);
            p.AddArc(R - rn, yc - rn, 2 * rn, 2 * rn, 270, -180);
            p.AddArc(R - 2 * rc, B - 2 * rc, 2 * rc, 2 * rc, 0, 90);
            p.AddArc(L, B - 2 * rc, 2 * rc, 2 * rc, 90, 90);
            p.AddArc(L - rn, yc - rn, 2 * rn, 2 * rn, 90, -180);
            p.CloseFigure();

            // pointilles verticaux (trous, grace au remplissage "alterne")
            float x = L + r.Width * 0.30f;
            float largeur = Math.Max(1f, r.Width * 0.07f);
            int n = 4;
            float marge = r.Height * 0.14f;
            float pas = (r.Height - 2 * marge) / n;
            for (int i = 0; i < n; i++)
            {
                float y = T + marge + i * pas + pas * 0.2f;
                p.AddRectangle(new RectangleF(x - largeur / 2f, y, largeur, pas * 0.6f));
            }
            return p;
        }

        public static void IconeTicket(Graphics g, RectangleF zone, Color couleur, float inclinaison = 0f)
        {
            float w = zone.Width, h = zone.Width * 0.62f;
            RectangleF r = new(zone.X + (zone.Width - w) / 2f, zone.Y + (zone.Height - h) / 2f, w, h);
            using GraphicsPath chemin = CheminTicket(r);
            if (inclinaison != 0f)
            {
                using Matrix m = new();
                m.RotateAt(inclinaison, new PointF(zone.X + zone.Width / 2f, zone.Y + zone.Height / 2f));
                chemin.Transform(m);
            }
            using SolidBrush pinceau = new(couleur);
            g.FillPath(pinceau, chemin);
        }

        /// <summary>
        /// Pastille d'icone : carre arrondi colore (degrade vertical) + glyphe blanc.
        /// </summary>
        public static void Pastille(Graphics g, RectangleF zone, Color couleur, string? glyphe, bool ticket = false)
        {
            using (GraphicsPath chemin = Arrondi(zone, zone.Width * 0.26f))
            using (LinearGradientBrush degrade = new(
                new PointF(zone.X, zone.Y - 1), new PointF(zone.X, zone.Bottom + 1),
                Eclaircir(couleur, 0.14f), Assombrir(couleur, 0.06f)))
            {
                g.FillPath(degrade, chemin);
            }

            RectangleF interieur = RectangleF.Inflate(zone, -zone.Width * 0.2f, -zone.Height * 0.2f);
            if (ticket) IconeTicket(g, interieur, Color.White, -12f);
            else if (glyphe != null) Icone(g, glyphe, zone, zone.Width * 0.58f, Color.White);
        }

        // --- Texte ---

        public const TextFormatFlags Gauche = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        public const TextFormatFlags Centre = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        public const TextFormatFlags Droite = TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

        public static void Texte(Graphics g, string? texte, Font police, Rectangle zone, Color couleur, TextFormatFlags flags = Gauche)
        {
            if (string.IsNullOrEmpty(texte)) return;
            TextRenderer.DrawText(g, texte, police, zone, couleur, flags);
        }

        public static Size Mesurer(string? texte, Font police)
            => string.IsNullOrEmpty(texte) ? Size.Empty
               : TextRenderer.MeasureText(texte, police, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

        // --- Ombres douces (flou mis en cache) ---

        static readonly Dictionary<string, Bitmap> cacheOmbres = new();

        public static void Ombre(Graphics g, RectangleF forme, float rayon, float flou, Color couleur, float decalageY)
        {
            int marge = (int)Math.Ceiling(flou * 1.5f) + 2;
            int w = (int)Math.Ceiling(forme.Width) + marge * 2;
            int h = (int)Math.Ceiling(forme.Height) + marge * 2;
            if (w <= 0 || h <= 0 || w > 4000 || h > 4000) return;

            string cle = $"{w}x{h}/{rayon:0.#}/{flou:0.#}/{couleur.ToArgb()}";
            if (!cacheOmbres.TryGetValue(cle, out Bitmap? bmp))
            {
                if (cacheOmbres.Count > 40)
                {
                    foreach (Bitmap ancien in cacheOmbres.Values) ancien.Dispose();
                    cacheOmbres.Clear();
                }
                bmp = CreerOmbre(w, h, marge, rayon, flou, couleur);
                cacheOmbres[cle] = bmp;
            }
            g.DrawImage(bmp, new Rectangle((int)Math.Round(forme.X) - marge, (int)Math.Round(forme.Y + decalageY) - marge, w, h));
        }

        static Bitmap CreerOmbre(int w, int h, int marge, float rayon, float flou, Color couleur)
        {
            Bitmap bmp = new(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Lisser(g);
                Remplir(g, new RectangleF(marge, marge, w - 2 * marge, h - 2 * marge), rayon, Color.White);
            }

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] px = new int[w * h];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);

            float[] a = new float[w * h];
            for (int i = 0; i < px.Length; i++) a[i] = (px[i] >> 24) & 0xFF;

            int r = Math.Max(1, (int)Math.Round(flou / 3f));
            for (int passe = 0; passe < 3; passe++)
            {
                FlouBoite(a, w, h, r, true);
                FlouBoite(a, w, h, r, false);
            }

            int rgb = couleur.ToArgb() & 0x00FFFFFF;
            float opacite = couleur.A / 255f;
            for (int i = 0; i < px.Length; i++)
            {
                int alpha = (int)Math.Clamp(a[i] * opacite, 0, 255);
                px[i] = (alpha << 24) | rgb;
            }
            System.Runtime.InteropServices.Marshal.Copy(px, 0, data.Scan0, px.Length);
            bmp.UnlockBits(data);
            return bmp;
        }

        static void FlouBoite(float[] a, int w, int h, int r, bool horizontal)
        {
            int longueur = horizontal ? w : h;
            int lignes = horizontal ? h : w;
            float[] tampon = new float[longueur];
            float norme = 1f / (2 * r + 1);
            for (int l = 0; l < lignes; l++)
            {
                int Index(int i) => horizontal ? l * w + i : i * w + l;
                float somme = 0;
                for (int i = -r; i <= r; i++) somme += a[Index(Math.Clamp(i, 0, longueur - 1))];
                for (int i = 0; i < longueur; i++)
                {
                    tampon[i] = somme * norme;
                    somme += a[Index(Math.Min(i + r + 1, longueur - 1))] - a[Index(Math.Max(i - r, 0))];
                }
                for (int i = 0; i < longueur; i++) a[Index(i)] = tampon[i];
            }
        }

        // --- Indicateur d'activite (8 branches) ---

        public static void Spinner(Graphics g, PointF centre, float rayon, int etape, Color couleur)
        {
            const int branches = 8;
            float epaisseur = Math.Max(1.5f, rayon * 0.26f);
            using Pen stylo = new(couleur, epaisseur) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            for (int i = 0; i < branches; i++)
            {
                int age = ((etape - i) % branches + branches) % branches;
                float opacite = 1f - age / (float)branches * 0.85f;
                stylo.Color = Alpha(couleur, opacite * (couleur.A / 255f));
                float angle = Rad(i * 360f / branches - 90f);
                float x1 = centre.X + MathF.Cos(angle) * rayon * 0.45f, y1 = centre.Y + MathF.Sin(angle) * rayon * 0.45f;
                float x2 = centre.X + MathF.Cos(angle) * rayon, y2 = centre.Y + MathF.Sin(angle) * rayon;
                g.DrawLine(stylo, x1, y1, x2, y2);
            }
        }
    }
}
