using System.Drawing.Drawing2D;
using System.Text;

namespace cineprint.UI
{
    /// <summary>
    /// Apercu du ticket thermique (58 mm, 32 caracteres par ligne) : reprend les lignes de
    /// Protocole.Ticket() (centrage, double taille, gras, conversion ASCII).
    /// AnimerImpression() fait defiler le papier hors de la fente.
    /// </summary>
    public class ApercuTicket : ControleBase
    {
        const int CaracteresParLigne = 32;

        string cinema = "", film = "";
        int salle = 1;
        DateTime seance = DateTime.Now;
        readonly Anim sortie;
        readonly Dictionary<(float, bool), Font> polices = new();

        public ApercuTicket()
        {
            sortie = new Anim(this, 1f, 260f);
        }

        public void Definir(string cinema, string film, int salle, DateTime seance)
        {
            this.cinema = cinema;
            this.film = film;
            this.salle = salle;
            this.seance = seance;
            Invalidate();
        }

        /// <summary>Lance l'animation de sortie du papier.</summary>
        public void AnimerImpression()
        {
            sortie.Fixer(0f);
            sortie.Vers(1f);
        }

        record LigneTicket(string Texte, bool Double, bool Gras);

        /// <summary>Meme conversion qu'a l'envoi (Encoding.ASCII) : les caracteres non ASCII deviennent '?'.</summary>
        static string Ascii(string s) => Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(s));

        static IEnumerable<string> Couper(string texte, int largeur)
        {
            if (texte.Length == 0) { yield return ""; yield break; }
            for (int i = 0; i < texte.Length; i += largeur)
                yield return texte.Substring(i, Math.Min(largeur, texte.Length - i));
        }

        List<LigneTicket> Lignes()
        {
            // Meme sequence que Protocole.Ticket()
            List<LigneTicket> l = new();
            foreach (string m in Couper(Ascii(cinema.Trim().ToUpper()), CaracteresParLigne / 2)) l.Add(new(m, true, false));
            l.Add(new("", false, false));
            foreach (string m in Couper(Ascii(film.Trim()), CaracteresParLigne)) l.Add(new(m, false, true));
            l.Add(new("", false, false));
            l.Add(new("Salle " + salle, false, false));
            l.Add(new(Ascii(Protocole.FormatSeance(seance)), false, false));
            return l;
        }

        Font Police(float taillePx, bool gras)
        {
            float cle = MathF.Round(taillePx, 1);
            if (!polices.TryGetValue((cle, gras), out Font? f))
            {
                f = new Font(gras ? Theme.FamilleMonoGras : Theme.FamilleMono, cle, FontStyle.Regular, GraphicsUnit.Pixel);
                polices[(cle, gras)] = f;
            }
            return f;
        }

        public int HauteurEntete => S(30);

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);

            Dessin.Texte(g, "APERÇU DU TICKET", Theme.Section, new Rectangle(S(16), 0, Width, HauteurEntete - S(7)), Theme.TexteSecondaire,
                TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            // fond : carte (gris clair en theme clair, pour detacher le papier) + degrade radial centre sur le ticket
            RectangleF scene = new(0, HauteurEntete, Width, Height - HauteurEntete);
            Color fondScene = Theme.Sombre ? Theme.Carte : Color.FromArgb(0xEC, 0xEC, 0xEF);
            Dessin.RemplirBorde(g, scene, F(14), fondScene, Theme.CarteBordure, Math.Max(1f, F(0.5f)));

            int largeurPapier = Math.Min(Width - S(70), S(290));
            if (largeurPapier <= S(60)) return;
            float x0 = MathF.Round((Width - largeurPapier) / 2f);

            // Metriques "thermiques" : 32 caracteres sur la largeur imprimable, interligne 30/12 points
            float interieur = largeurPapier - 2 * F(22);
            float chasse = interieur / CaracteresParLigne;
            Font reference = Police(20f, false);
            float largeurRef = TextRenderer.MeasureText(g, new string('0', CaracteresParLigne), reference, Size.Empty, TextFormatFlags.NoPadding).Width;
            float taille = 20f * interieur / largeurRef;
            float hLigne = chasse * 2.5f;

            List<LigneTicket> lignes = Lignes();
            float hTexte = lignes.Sum(l => l.Double ? 2 * hLigne : hLigne);
            float dent = F(5);
            float hauteurPapier = hLigne * 1.2f + hTexte + hLigne * 3.2f + dent;

            // ticket centre verticalement dans la scene (legende comprise)
            float hauteurTotale = hauteurPapier + S(46);
            float yFente = MathF.Round(Math.Max(scene.Y + S(34), scene.Y + (scene.Height - hauteurTotale) / 2f));

            GraphicsState etatHalo = g.Save();
            using (GraphicsPath forme = Dessin.Arrondi(RectangleF.Inflate(scene, -1, -1), F(13)))
                g.SetClip(forme);
            float rayonHalo = Math.Max(scene.Width, hauteurPapier) * 0.75f;
            RectangleF halo = new(Width / 2f - rayonHalo, yFente + hauteurPapier / 2f - rayonHalo, 2 * rayonHalo, 2 * rayonHalo);
            using (GraphicsPath cercle = new())
            {
                cercle.AddEllipse(halo);
                using PathGradientBrush degrade = new(cercle)
                {
                    CenterColor = Dessin.Voile(fondScene, Color.White, Theme.Sombre ? 0.06f : 0.55f),
                    SurroundColors = new[] { fondScene },
                };
                g.FillEllipse(degrade, halo);
            }
            g.Restore(etatHalo);

            float decalage = -(1f - sortie.Valeur) * (hauteurPapier + F(8));
            RectangleF papier = new(x0, yFente + decalage, largeurPapier, hauteurPapier);

            GraphicsState etat = g.Save();
            g.SetClip(new RectangleF(0, yFente, Width, scene.Bottom - F(2) - yFente));

            Dessin.Ombre(g, new RectangleF(papier.X + F(4), papier.Y + F(10), papier.Width - F(8), papier.Height - F(10)), 0,
                F(18), Color.FromArgb(Theme.Sombre ? 150 : 64, 0, 0, 0), F(8));

            using (GraphicsPath chemin = CheminPapier(papier, dent))
            {
                using SolidBrush fond = new(Theme.Papier);
                g.FillPath(fond, chemin);
            }

            // ombre portee de la fente sur le haut du papier
            RectangleF ombreFente = new(papier.X, yFente, papier.Width, F(16));
            using (LinearGradientBrush lg = new(ombreFente, Color.FromArgb(34, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), LinearGradientMode.Vertical))
                g.FillRectangle(lg, ombreFente);

            float y = papier.Y + hLigne * 1.2f;
            foreach (LigneTicket l in lignes)
            {
                float h = l.Double ? 2 * hLigne : hLigne;
                if (l.Texte.Length > 0)
                {
                    Font f = Police(l.Double ? 2 * taille : taille, l.Gras);
                    Rectangle zone = Rectangle.Round(new RectangleF(papier.X, y, papier.Width, h));
                    TextRenderer.DrawText(g, l.Texte, f, zone, Theme.Encre, Theme.Papier,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping);
                }
                y += h;
            }
            g.Restore(etat);

            // fente de l'imprimante (dessinee par-dessus le papier)
            RectangleF fente = new(x0 - S(14), yFente - F(5), largeurPapier + S(28), F(10));
            Color couleurFente = Theme.Sombre ? Color.FromArgb(0x06, 0x06, 0x07) : Color.FromArgb(0xC7, 0xC7, 0xCC);
            Dessin.Remplir(g, fente, fente.Height / 2f, couleurFente);
            Dessin.Remplir(g, new RectangleF(fente.X + F(5), fente.Bottom - F(1.5f), fente.Width - F(10), F(1)), 0,
                Theme.Sombre ? Color.FromArgb(0x33, 0x33, 0x38) : Color.FromArgb(0xE4, 0xE4, 0xE8));

            float bas = yFente + hauteurPapier + S(24);
            if (bas + S(10) < scene.Bottom)
                Dessin.Texte(g, "58 mm · 32 caractères par ligne", Theme.Petit, new Rectangle(0, (int)bas - S(10), Width, S(20)),
                    Dessin.Melange(fondScene, Theme.TexteSecondaire, sortie.Valeur), Dessin.Centre);
        }

        static GraphicsPath CheminPapier(RectangleF r, float dent)
        {
            GraphicsPath p = new();
            p.AddLine(r.Left, r.Top, r.Right, r.Top);
            p.AddLine(r.Right, r.Top, r.Right, r.Bottom - dent);
            int n = Math.Max(4, (int)Math.Round(r.Width / (dent * 2.4f)));
            float w = r.Width / n;
            for (int i = 0; i < n; i++)
            {
                float xa = r.Right - i * w;
                p.AddLine(xa, r.Bottom - dent, xa - w / 2f, r.Bottom);
                p.AddLine(xa - w / 2f, r.Bottom, xa - w, r.Bottom - dent);
            }
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Font f in polices.Values) f.Dispose();
                polices.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
