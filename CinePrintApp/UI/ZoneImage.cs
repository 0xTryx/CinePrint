using System.Drawing.Drawing2D;

namespace cineprint.UI
{
    /// <summary>
    /// Zone de depot d'image (glisser-deposer ou bouton) avec apercu de l'image d'origine
    /// ou du rendu thermique (image tramee reconstruite depuis la commande GS v 0).
    /// </summary>
    public class ZoneImage : ControleBase
    {
        static readonly string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp" };

        Image? original;
        Bitmap? rendu;
        string? nomFichier, details;
        readonly Anim depot;
        readonly System.Windows.Forms.Timer minuterieSpinner = new() { Interval = 95 };
        int etapeSpinner;

        public ControleSegmente Mode { get; }
        public Bouton BoutonChoisir { get; }

        public event Action<string>? FichierDepose;

        public ZoneImage()
        {
            AllowDrop = true;
            depot = new Anim(this, 0f, 45f);
            minuterieSpinner.Tick += (s, e) => { etapeSpinner++; Invalidate(); };
            AppliquerTheme();

            Mode = new ControleSegmente();
            Mode.Ajouter("Original");
            Mode.Ajouter("Rendu thermique");
            Mode.FixerSelection(1);
            Mode.SelectionChange += (s, e) => Invalidate();
            Mode.Visible = false;

            BoutonChoisir = new Bouton { Style = StyleBouton.Teinte, Text = "Choisir une image…", Icone = Glyphe.Dossier };

            Controls.Add(Mode);
            Controls.Add(BoutonChoisir);
        }

        protected override void AppliquerTheme() => BackColor = Theme.Carte;

        public bool AfficheRendu => Mode.Selection == 1;

        public void DefinirImage(Image? image, string? nom)
        {
            original = image;
            nomFichier = nom;
            Mode.Visible = image != null;
            BoutonChoisir.Visible = image == null;
            PerformLayout();
            Invalidate();
        }

        /// <summary>Image tramee (null pendant le calcul).</summary>
        public void DefinirRendu(Bitmap? image, string? texteDetails, bool enCalcul)
        {
            rendu?.Dispose();
            rendu = image;
            details = texteDetails;
            minuterieSpinner.Enabled = enCalcul && image == null;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                minuterieSpinner.Dispose();
                rendu?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int lm = Math.Min(Mode.LargeurIdeale, Width - S(40));
            Mode.Placer((Width - lm) / 2, S(16), lm, S(30));
            int lb = BoutonChoisir.LargeurIdeale + S(8);
            BoutonChoisir.Placer((Width - lb) / 2, Height / 2 + S(22), lb, S(34));
        }

        // --- Glisser-deposer ---
        static string? CheminValide(DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] fichiers && fichiers.Length == 1 &&
                extensions.Contains(Path.GetExtension(fichiers[0]).ToLowerInvariant()))
                return fichiers[0];
            return null;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            bool ok = CheminValide(e) != null;
            e.Effect = ok ? DragDropEffects.Copy : DragDropEffects.None;
            depot.Vers(ok ? 1f : 0f);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            depot.Vers(0f);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            depot.Vers(0f);
            string? chemin = CheminValide(e);
            if (chemin != null) FichierDepose?.Invoke(chemin);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF boite = new(0, 0, Width, Height);
            float d = depot.Valeur;
            Color fond = Dessin.Voile(Theme.Carte, Theme.Accent, 0.07f * d);
            Dessin.RemplirBorde(g, boite, F(14), fond, Dessin.Melange(Theme.CarteBordure, Theme.Accent, d), Math.Max(1f, F(0.5f)) + F(1.5f) * d);

            if (original == null) DessinerVide(g, fond);
            else DessinerImage(g);
        }

        void DessinerVide(Graphics g, Color fond)
        {
            // cadre en pointilles
            RectangleF cadre = RectangleF.Inflate(new RectangleF(0, 0, Width, Height), -F(14), -F(14));
            using (GraphicsPath chemin = Dessin.Arrondi(cadre, F(10)))
            using (Pen tirets = new(Dessin.Melange(Dessin.Voile(fond, Theme.Texte, 0.22f), Theme.Accent, depot.Valeur), Math.Max(1f, F(1.2f))))
            {
                tirets.DashPattern = new[] { 4f, 3f };
                g.DrawPath(tirets, chemin);
            }

            // titre + legende au-dessus du bouton, bloc centre verticalement
            int cy = Height / 2;
            Dessin.Texte(g, "Dépose une image ici", Theme.Grand, new Rectangle(0, cy - S(56), Width, S(26)), Theme.Texte, Dessin.Centre);
            Dessin.Texte(g, "PNG, JPG ou BMP · tramée automatiquement en noir et blanc", Theme.Petit,
                new Rectangle(S(20), cy - S(26), Width - S(40), S(20)), Theme.TexteSecondaire, Dessin.Centre);
        }

        void DessinerImage(Graphics g)
        {
            Rectangle zone = new(S(20), S(62), Width - S(40), Height - S(62) - S(46));
            if (zone.Width <= 0 || zone.Height <= 0) return;

            if (AfficheRendu)
            {
                if (rendu == null)
                    Dessin.Spinner(g, new PointF(zone.X + zone.Width / 2f, zone.Y + zone.Height / 2f), F(12), etapeSpinner, Theme.TexteSecondaire);
                else DessinerRendu(g, zone, rendu);
            }
            else DessinerOriginal(g, zone, original!);

            // legende
            Rectangle legende = new(S(20), Height - S(40), Width - S(40), S(28));
            Dessin.Texte(g, nomFichier, Theme.Petit, new Rectangle(legende.X, legende.Y, legende.Width / 2, legende.Height), Theme.TexteSecondaire);
            Dessin.Texte(g, details, Theme.Petit, new Rectangle(legende.X + legende.Width / 2, legende.Y, legende.Width / 2, legende.Height), Theme.TexteTertiaire, Dessin.Droite);
        }

        static RectangleF Ajuster(Rectangle zone, float w, float h)
        {
            float echelle = Math.Min(zone.Width / w, zone.Height / h);
            float lw = w * echelle, lh = h * echelle;
            return new RectangleF(zone.X + (zone.Width - lw) / 2f, zone.Y + (zone.Height - lh) / 2f, lw, lh);
        }

        void DessinerOriginal(Graphics g, Rectangle zone, Image image)
        {
            RectangleF r = Ajuster(zone, image.Width, image.Height);
            GraphicsState etat = g.Save();
            using (GraphicsPath forme = Dessin.Arrondi(r, F(8)))
            {
                g.SetClip(forme);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, r);
            }
            g.Restore(etat);
        }

        void DessinerRendu(Graphics g, Rectangle zone, Bitmap image)
        {
            // Papier thermique + points a une echelle entiere (pixels nets, sans moire)
            float marge = F(14);
            Rectangle interieur = Rectangle.Inflate(zone, -(int)marge, -(int)marge);
            float echelle = Math.Min(interieur.Width / (float)image.Width, interieur.Height / (float)image.Height);
            float entier = echelle >= 1f ? MathF.Floor(echelle) : echelle;
            float lw = image.Width * entier, lh = image.Height * entier;
            RectangleF r = new(MathF.Round(zone.X + (zone.Width - lw) / 2f), MathF.Round(zone.Y + (zone.Height - lh) / 2f), lw, lh);
            RectangleF papier = RectangleF.Inflate(r, marge, marge);

            Dessin.Ombre(g, papier, F(6), F(12), Color.FromArgb(Theme.Sombre ? 120 : 36, 0, 0, 0), F(4));
            Dessin.Remplir(g, papier, F(6), Theme.Papier);

            GraphicsState etat = g.Save();
            g.InterpolationMode = entier >= 1f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(image, r);
            g.Restore(etat);
        }
    }
}
