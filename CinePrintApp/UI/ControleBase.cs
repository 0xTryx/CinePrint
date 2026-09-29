namespace cineprint.UI
{
    /// <summary>
    /// Base des controles dessines (UserPaint) : double tampon, fond = couleur du parent
    /// et redessin quand le theme change.
    /// </summary>
    public class ControleBase : Control
    {
        public ControleBase()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            // Non focalisable par defaut : seuls les controles interactifs le redeviennent.
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            Theme.Change += SurChangementTheme;
        }

        protected static int S(float valeur) => Theme.S(valeur);
        protected static float F(float valeur) => Theme.F(valeur);

        /// <summary>Couleur de la surface sur laquelle le controle est pose.</summary>
        protected Color Surface => Parent?.BackColor ?? Theme.Fond;

        protected virtual void AppliquerTheme() { }

        void SurChangementTheme()
        {
            if (IsDisposed) return;
            AppliquerTheme();
            Invalidate(true);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Surface);

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Change -= SurChangementTheme;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Controle interactif avec une marge reservee a l'anneau de focus.
    /// Zone = partie visible ; Placer() positionne le controle d'apres cette zone.
    /// </summary>
    public class ControleAnneau : ControleBase
    {
        protected readonly Anim focus;

        public ControleAnneau()
        {
            focus = new Anim(this, 0f, 55f);
        }

        public static int Marge => Theme.S(3);

        protected RectangleF Zone => new(Marge, Marge, Width - 2 * Marge, Height - 2 * Marge);

        /// <summary>Anneau translucide autour de la zone, proportionnel a l'animation de focus.</summary>
        protected void DessinerAnneau(Graphics g, float rayon, float progression, Color? couleur = null)
        {
            if (progression <= 0.01f) return;
            float e = F(3f);
            RectangleF r = RectangleF.Inflate(Zone, e * progression, e * progression);
            Dessin.Contour(g, r, rayon + e * progression, Dessin.Alpha(couleur ?? Theme.Accent, 0.42f * progression), e * progression);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            focus.Vers(AfficherFocus ? 1f : 0f);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            focus.Vers(0f);
        }

        /// <summary>Par defaut, l'anneau n'apparait qu'en navigation clavier (Tab).</summary>
        protected virtual bool AfficherFocus => ShowFocusCues;

        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.OnChangeUICues(e);
            if (Focused) focus.Vers(AfficherFocus ? 1f : 0f);
        }
    }

    public static class ExtensionsControle
    {
        /// <summary>Place un controle d'apres sa partie visible (en ajoutant la marge d'anneau si besoin).</summary>
        public static void Placer(this Control c, int x, int y, int largeur, int hauteur)
        {
            int m = c is ControleAnneau ? ControleAnneau.Marge : 0;
            c.SetBounds(x - m, y - m, largeur + 2 * m, hauteur + 2 * m);
        }

        public static Rectangle Visible(this Control c)
        {
            int m = c is ControleAnneau ? ControleAnneau.Marge : 0;
            return Rectangle.Inflate(c.Bounds, -m, -m);
        }
    }
}
