namespace cineprint.UI
{
    /// <summary>
    /// Page de contenu : titre + sous-titre. L'en-tete renvoie HTTRANSPARENT pour servir
    /// de zone de deplacement de la fenetre.
    /// </summary>
    public class Page : ControleBase
    {
        string titre = "", sousTitre = "";

        public Page()
        {
            Dock = DockStyle.Fill;
            AppliquerTheme();
        }

        protected override void AppliquerTheme() => BackColor = Theme.Fond;

        public string Titre { get => titre; set { titre = value; Invalidate(); } }
        public string SousTitre { get => sousTitre; set { sousTitre = value; Invalidate(); } }

        /// <summary>Marge laterale du contenu.</summary>
        public int Marge => S(36);

        /// <summary>Ordonnee du debut du contenu (sous le titre).</summary>
        public int Haut => S(124);

        /// <summary>Largeur utile du contenu.</summary>
        public int LargeurUtile => Width - 2 * Marge;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Texte(g, titre, Theme.Titre, new Rectangle(Marge - S(1), S(40), Width - 2 * Marge, S(38)), Theme.Texte);
            Dessin.Texte(g, sousTitre, Theme.Corps, new Rectangle(Marge, S(78), Width - 2 * Marge, S(22)), Theme.TexteSecondaire);
        }

        protected override void WndProc(ref Message m)
        {
            // L'en-tete (sans controle dessus) est "transparent" : la fenetre le traite comme sa barre de titre.
            if (m.Msg == Natif.WM_NCHITTEST)
            {
                Point p = PointToClient(new Point((short)(m.LParam.ToInt64() & 0xFFFF), (short)((m.LParam.ToInt64() >> 16) & 0xFFFF)));
                if (p.Y < Haut - S(14))
                {
                    m.Result = (IntPtr)Natif.HTTRANSPARENT;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
