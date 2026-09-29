namespace cineprint.UI
{
    public enum EtatConnexion { Deconnecte, Connexion, Connecte }

    /// <summary>
    /// Carte d'etat de l'imprimante (bas de la barre laterale) : etat, adresse,
    /// progression de l'envoi en cours et bouton connexion / deconnexion.
    /// </summary>
    public class CarteConnexion : ControleBase
    {
        EtatConnexion etat;
        string adresse = "";
        float progression = -1f;
        readonly Anim barre;

        public Bouton Bouton { get; }

        public CarteConnexion()
        {
            barre = new Anim(this, 0f, 60f);
            Bouton = new Bouton { Style = StyleBouton.Primaire, Text = "Se connecter", Icone = Glyphe.Lien };
            Controls.Add(Bouton);
            Height = HauteurIdeale;
            AppliquerTheme();
        }

        protected override void AppliquerTheme() => BackColor = Theme.Carte;

        public EtatConnexion Etat
        {
            get => etat;
            set
            {
                etat = value;
                Bouton.Chargement = value == EtatConnexion.Connexion;
                Bouton.Text = value switch
                {
                    EtatConnexion.Connecte => "Se déconnecter",
                    EtatConnexion.Connexion => "Connexion…",
                    _ => "Se connecter",
                };
                Bouton.Style = value == EtatConnexion.Connecte ? StyleBouton.Secondaire : StyleBouton.Primaire;
                Bouton.Icone = value == EtatConnexion.Connecte ? Glyphe.Deconnecter : Glyphe.Lien;
                Invalidate();
            }
        }

        public string Adresse { get => adresse; set { adresse = value; Invalidate(); } }

        /// <summary>Progression de l'envoi (0..1), ou -1 pour masquer la barre.</summary>
        public float Progression
        {
            get => progression;
            set
            {
                bool visibleAvant = progression >= 0, visibleApres = value >= 0;
                progression = value;
                if (visibleApres) barre.Vers(value);
                else barre.Fixer(0f);
                if (visibleAvant != visibleApres)
                {
                    Height = HauteurIdeale;
                    Parent?.PerformLayout();
                }
                Invalidate();
            }
        }

        public int HauteurIdeale => S(progression >= 0 ? 132 : 108);

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int m = S(12);
            Bouton.Placer(m, Height - m - S(32), Width - 2 * m, S(32));
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Barre);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Dessin.RemplirBorde(g, new RectangleF(0, 0, Width, Height), F(12), Theme.Carte, Theme.CarteBordure, Math.Max(1f, F(0.5f)));

            int m = S(14);
            string titre = etat switch
            {
                EtatConnexion.Connecte => "Imprimante connectée",
                EtatConnexion.Connexion => "Connexion en cours…",
                _ => "Imprimante hors ligne",
            };
            Dessin.Texte(g, titre, Theme.CorpsGras, new Rectangle(m, S(13), Width - 2 * m, S(22)), Theme.Texte);
            Dessin.Texte(g, adresse, Theme.Petit, new Rectangle(m, S(35), Width - 2 * m, S(18)), Theme.TexteSecondaire);

            if (progression >= 0)
            {
                RectangleF piste = new(m, S(66), Width - 2 * m - S(44), F(5));
                Dessin.Remplir(g, piste, piste.Height / 2f, Dessin.Voile(Theme.Carte, Theme.Texte, 0.12f));
                RectangleF plein = piste;
                plein.Width = Math.Max(piste.Height, piste.Width * Math.Clamp(barre.Valeur, 0f, 1f));
                Dessin.Remplir(g, plein, piste.Height / 2f, Theme.Accent);
                Dessin.Texte(g, $"{(int)Math.Round(progression * 100)} %", Theme.PetitGras,
                    new Rectangle((int)piste.Right, S(58), Width - m - (int)piste.Right, S(20)), Theme.TexteSecondaire, Dessin.Droite);
            }
        }
    }
}
