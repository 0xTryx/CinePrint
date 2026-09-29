using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace cineprint.UI
{
    public enum TypeNotification { Succes, Info, Erreur }

    /// <summary>
    /// Notification non bloquante affichee en bas du contenu : fenetre calque (UpdateLayeredWindow)
    /// avec transparence par pixel, fondu d'entree / sortie et fermeture automatique.
    /// </summary>
    public sealed class Notification : Form
    {
        static Notification? courante;

        readonly Control ancre;
        readonly Bitmap rendu;
        readonly Stopwatch chrono = Stopwatch.StartNew();
        readonly System.Windows.Forms.Timer minuterie = new() { Interval = 15 };
        readonly int duree;
        long debutSortie = -1;

        const int Entree = 260, Sortie = 240;

        public static void Afficher(Control ancre, string titre, string? detail = null, TypeNotification type = TypeNotification.Succes)
        {
            Form? proprietaire = ancre.FindForm();
            if (proprietaire == null || proprietaire.WindowState == FormWindowState.Minimized) return;
            courante?.Fermer();
            courante = new Notification(ancre, titre, detail, type);
            courante.Show(proprietaire);
        }

        Notification(Control ancre, string titre, string? detail, TypeNotification type)
        {
            this.ancre = ancre;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            duree = type == TypeNotification.Erreur ? 4200 : 2800;
            rendu = Rendre(titre, detail, type);
            Size = rendu.Size;
            minuterie.Tick += (s, e) => Animer();
            Form? f = ancre.FindForm();
            if (f != null)
            {
                f.Move += Proprietaire_Change;
                f.Resize += Proprietaire_Change;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= Natif.WS_EX_LAYERED | Natif.WS_EX_TRANSPARENT | Natif.WS_EX_TOOLWINDOW | Natif.WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Animer();
            minuterie.Start();
        }

        void Proprietaire_Change(object? sender, EventArgs e)
        {
            if (sender is Form f && f.WindowState == FormWindowState.Minimized) Fermer();
            else Animer();
        }

        public void Fermer()
        {
            if (debutSortie < 0) debutSortie = chrono.ElapsedMilliseconds;
        }

        static float Adoucir(float t) => 1f - MathF.Pow(1f - Math.Clamp(t, 0f, 1f), 3f);

        void Animer()
        {
            if (IsDisposed) return;
            long t = chrono.ElapsedMilliseconds;
            if (debutSortie < 0 && t > Entree + duree) debutSortie = t;

            float opacite, glissement;
            if (debutSortie >= 0)
            {
                float p = (t - debutSortie) / (float)Sortie;
                if (p >= 1f)
                {
                    minuterie.Stop();
                    Close();
                    return;
                }
                opacite = 1f - Adoucir(p);
                glissement = Adoucir(p) * Theme.F(6);
            }
            else
            {
                float p = Adoucir(t / (float)Entree);
                opacite = p;
                glissement = (1f - p) * Theme.F(14);
            }

            Rectangle zone = ancre.RectangleToScreen(ancre.ClientRectangle);
            Point position = new(
                zone.X + (zone.Width - rendu.Width) / 2,
                zone.Bottom - rendu.Height - Theme.S(8) + (int)glissement);
            Appliquer(position, (byte)Math.Round(opacite * 255));
        }

        void Appliquer(Point position, byte opacite)
        {
            IntPtr ecran = Natif.GetDC(IntPtr.Zero);
            IntPtr memoire = Natif.CreateCompatibleDC(ecran);
            IntPtr hBitmap = rendu.GetHbitmap(Color.FromArgb(0));
            IntPtr ancien = Natif.SelectObject(memoire, hBitmap);
            try
            {
                Natif.POINT dest = new() { X = position.X, Y = position.Y };
                Natif.SIZE taille = new() { Cx = rendu.Width, Cy = rendu.Height };
                Natif.POINT source = new();
                Natif.BLENDFUNCTION melange = new() { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = opacite, AlphaFormat = 1 };
                Natif.UpdateLayeredWindow(Handle, ecran, ref dest, ref taille, memoire, ref source, 0, ref melange, 2);
            }
            finally
            {
                Natif.SelectObject(memoire, ancien);
                Natif.DeleteObject(hBitmap);
                Natif.DeleteDC(memoire);
                Natif.ReleaseDC(IntPtr.Zero, ecran);
            }
        }

        static Bitmap Rendre(string titre, string? detail, TypeNotification type)
        {
            float e = Theme.Echelle;
            int marge = Theme.S(26);
            Font fTitre = Theme.CorpsGras, fDetail = Theme.Petit;

            Size mt = TextRenderer.MeasureText(titre, fTitre, Size.Empty, TextFormatFlags.NoPadding);
            Size md = detail == null ? Size.Empty : TextRenderer.MeasureText(detail, fDetail, Size.Empty, TextFormatFlags.NoPadding);
            int largeurTexte = Math.Max(mt.Width, md.Width);
            int hauteur = Theme.S(detail == null ? 48 : 60);
            int largeur = Math.Min(Theme.S(560), Theme.S(16) + Theme.S(28) + Theme.S(12) + largeurTexte + Theme.S(22));

            Bitmap bmp = new(largeur + 2 * marge, hauteur + 2 * marge, PixelFormat.Format32bppArgb);
            bmp.SetResolution(96f * e, 96f * e);
            using Graphics g = Graphics.FromImage(bmp);
            Dessin.Lisser(g);
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            RectangleF forme = new(marge, marge, largeur, hauteur);
            Dessin.Ombre(g, forme, hauteur / 2f, Theme.F(16), Color.FromArgb(Theme.Sombre ? 150 : 60, 0, 0, 0), Theme.F(6));

            Color fond = Theme.Sombre ? Color.FromArgb(252, 0x2A, 0x2A, 0x2E) : Color.FromArgb(252, 255, 255, 255);
            Color bordure = Theme.Sombre ? Color.FromArgb(255, 0x40, 0x40, 0x45) : Color.FromArgb(255, 0xDE, 0xDE, 0xE3);
            Dessin.Remplir(g, forme, hauteur / 2f, bordure);
            Dessin.Remplir(g, RectangleF.Inflate(forme, -Math.Max(1f, Theme.F(0.5f)), -Math.Max(1f, Theme.F(0.5f))), hauteur / 2f, fond);

            Color couleur = type switch
            {
                TypeNotification.Erreur => Theme.Rouge,
                TypeNotification.Info => Theme.Accent,
                _ => Theme.Vert,
            };
            string glyphe = type switch
            {
                TypeNotification.Erreur => Glyphe.Alerte,
                TypeNotification.Info => Glyphe.Info,
                _ => Glyphe.Coche,
            };
            float d = Theme.F(28);
            RectangleF rond = new(forme.X + Theme.F(12), forme.Y + (hauteur - d) / 2f, d, d);
            using (SolidBrush b = new(couleur)) g.FillEllipse(b, rond);
            Dessin.Icone(g, glyphe, rond, Theme.F(type == TypeNotification.Succes ? 13 : 14), Color.White);

            float xTexte = rond.Right + Theme.F(12);
            using SolidBrush bt = new(Theme.Texte);
            using SolidBrush bd = new(Theme.TexteSecondaire);
            StringFormat sf = new(StringFormat.GenericTypographic) { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            float largeurDispo = forme.Right - Theme.F(20) - xTexte;
            if (detail == null)
            {
                float yt = forme.Y + (hauteur - fTitre.GetHeight(g)) / 2f;
                g.DrawString(titre, fTitre, bt, new RectangleF(xTexte, yt, largeurDispo, fTitre.GetHeight(g) + 2), sf);
            }
            else
            {
                float ht = fTitre.GetHeight(g), hd = fDetail.GetHeight(g);
                float y0 = forme.Y + (hauteur - ht - hd) / 2f;
                g.DrawString(titre, fTitre, bt, new RectangleF(xTexte, y0, largeurDispo, ht + 2), sf);
                g.DrawString(detail, fDetail, bd, new RectangleF(xTexte, y0 + ht, largeurDispo, hd + 2), sf);
            }
            return bmp;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Form? f = ancre.FindForm();
            if (f != null)
            {
                f.Move -= Proprietaire_Change;
                f.Resize -= Proprietaire_Change;
            }
            if (courante == this) courante = null;
            minuterie.Dispose();
            rendu.Dispose();
        }
    }
}
