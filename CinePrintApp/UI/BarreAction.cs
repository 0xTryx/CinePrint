namespace cineprint.UI
{
    /// <summary>
    /// Barre d'outils de l'en-tete d'une page, alignee a droite : etat de l'imprimante
    /// (+ lien "Se connecter"), bouton secondaire optionnel et bouton principal.
    /// </summary>
    public class BarreAction : ControleBase
    {
        EtatConnexion etat;
        Rectangle zoneEtat;

        public Bouton Principal { get; }
        public Bouton? Secondaire { get; }
        public Bouton Lien { get; }

        public BarreAction(string textePrincipal, string icone, string? texteSecondaire = null, string? iconeSecondaire = null)
        {
            Principal = new Bouton { Style = StyleBouton.Primaire, Text = textePrincipal, Icone = icone, Police = Theme.Grand, Rayon = 10 };
            Lien = new Bouton { Style = StyleBouton.Discret, Text = "Se connecter" };
            Controls.Add(Principal);
            Controls.Add(Lien);
            if (texteSecondaire != null)
            {
                Secondaire = new Bouton { Style = StyleBouton.Secondaire, Text = texteSecondaire, Icone = iconeSecondaire, Police = Theme.Grand, Rayon = 10 };
                Controls.Add(Secondaire);
            }
            Height = S(38) + 2 * ControleAnneau.Marge;
        }

        public void DefinirEtat(EtatConnexion nouvelEtat)
        {
            etat = nouvelEtat;
            Lien.Visible = etat == EtatConnexion.Deconnecte;
            PerformLayout();
            Invalidate();
        }

        string TexteEtat => etat switch
        {
            EtatConnexion.Connecte => "Prête à imprimer",
            EtatConnexion.Connexion => "Connexion…",
            _ => "Hors ligne",
        };

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int m = ControleAnneau.Marge;
            int h = S(38);
            int y = (Height - h) / 2;
            int x = Width - m;

            int lp = Principal.LargeurIdeale + S(8);
            x -= lp;
            Principal.Placer(x, y, lp, h);
            if (Secondaire != null)
            {
                int ls = Secondaire.LargeurIdeale + S(4);
                x -= S(10) + ls;
                Secondaire.Placer(x, y, ls, h);
            }

            // etat (texte + lien "Se connecter") a gauche des boutons
            x -= S(16);
            if (Lien.Visible)
            {
                int ll = Lien.LargeurIdeale - S(10);
                x -= ll;
                Lien.Placer(x, (Height - S(30)) / 2, ll, S(30));
                x -= S(2);
            }
            int lt = Dessin.Mesurer(TexteEtat, Theme.Corps).Width;
            zoneEtat = new Rectangle(x - lt, 0, lt + S(2), Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Dessin.Texte(e.Graphics, TexteEtat, Theme.Corps, zoneEtat, Theme.TexteSecondaire);
        }

        protected override void WndProc(ref Message m)
        {
            // HTTRANSPARENT : le fond de la barre sert aussi a deplacer la fenetre
            if (m.Msg == Natif.WM_NCHITTEST)
            {
                m.Result = (IntPtr)Natif.HTTRANSPARENT;
                return;
            }
            base.WndProc(ref m);
        }
    }
}
