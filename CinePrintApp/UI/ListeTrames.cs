namespace cineprint.UI
{
    /// <summary>
    /// Liste des trames envoyees pour le ticket (Protocole.Ticket) : coloration par champ
    /// (instruction, longueur, donnees) et commande ESC/POS correspondante.
    /// </summary>
    public class ListeTrames : ControleBase
    {
        List<string> trames = new();

        public ListeTrames()
        {
            AppliquerTheme();
        }

        protected override void AppliquerTheme() => BackColor = Theme.Carte;

        public void Definir(List<string> nouvelles)
        {
            trames = nouvelles;
            Invalidate();
        }

        int HauteurEntete => S(30);
        int HauteurLigne => S(25);

        /// <summary>Commentaire lisible d'une trame du protocole CinePrint.</summary>
        public static string Decrire(string trame)
        {
            if (trame == "0,ping") return "ping";
            if (trame.StartsWith("1,")) return trame.Length == 2 ? "ligne vide" : "";
            if (!trame.StartsWith("2,")) return "";
            string[] p = trame.Split(',');
            string hexa = p.Length > 2 ? p[2] : "";
            return hexa switch
            {
                "1B;40" => "ESC @ · initialiser",
                "1B;61;00" => "ESC a · à gauche",
                "1B;61;01" => "ESC a · centrer",
                "1B;61;02" => "ESC a · à droite",
                "1D;21;11" => "GS ! · double taille",
                "1D;21;00" => "GS ! · taille normale",
                "1B;45;01" => "ESC E · gras",
                "1B;45;00" => "ESC E · fin du gras",
                _ when hexa.StartsWith("1B;64;") => "ESC d · avance papier",
                _ when hexa.StartsWith("1D;76;30") => "GS v 0 · image",
                _ => "octets bruts",
            };
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Fond);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            int marge = S(16);

            int octets = trames.Sum(t => t.Length + 1);
            TextFormatFlags bas = TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
            Rectangle entete = new(marge, 0, Width - 2 * marge, HauteurEntete - S(7));
            Dessin.Texte(g, "TRAMES DU PROTOCOLE", Theme.Section, entete, Theme.TexteSecondaire, TextFormatFlags.Left | bas);
            Dessin.Texte(g, trames.Count + " trames · " + octets + " octets", Theme.Petit, entete, Theme.TexteTertiaire, TextFormatFlags.Right | bas);

            RectangleF boite = new(0, HauteurEntete, Width, Height - HauteurEntete);
            Dessin.RemplirBorde(g, boite, F(12), Theme.Carte, Theme.CarteBordure, Math.Max(1f, F(0.5f)));
            if (trames.Count == 0) return;

            // deux colonnes, numerotees pour garder l'ordre d'envoi lisible
            int colonnes = Width > S(430) ? 2 : 1;
            int parColonne = (trames.Count + colonnes - 1) / colonnes;
            int dispo = (int)boite.Height - S(20);
            int visibles = Math.Max(1, dispo / HauteurLigne);
            float largeurColonne = (Width - 2 * marge - (colonnes - 1) * S(24)) / (float)colonnes;
            int chasse = Dessin.Mesurer("0000000000", Theme.Mono).Width / 10;

            for (int i = 0; i < trames.Count; i++)
            {
                int col = i / parColonne, ligne = i % parColonne;
                if (ligne >= visibles) continue;
                int x = marge + (int)(col * (largeurColonne + S(24)));
                int y = (int)boite.Y + S(10) + ligne * HauteurLigne;

                if (ligne == visibles - 1 && parColonne > visibles && col == colonnes - 1)
                {
                    Dessin.Texte(g, "+ " + (trames.Count - i) + " autres trames…", Theme.Petit, new Rectangle(x, y, (int)largeurColonne, HauteurLigne), Theme.TexteTertiaire);
                    break;
                }

                Dessin.Texte(g, (i + 1).ToString("00"), Theme.Mono, new Rectangle(x, y, chasse * 2 + S(2), HauteurLigne), Theme.TexteTertiaire);

                // la trame est prioritaire ; le commentaire ne s'affiche que s'il reste la place
                int xt = x + chasse * 3;
                int droite = x + (int)largeurColonne;
                int largeurTrame = trames[i].Length * chasse;
                string description = Decrire(trames[i]);
                int ld = Dessin.Mesurer(description, Theme.Petit).Width;
                if (description.Length > 0 && xt + largeurTrame + S(12) + ld <= droite)
                    Dessin.Texte(g, description, Theme.Petit, new Rectangle(x, y, (int)largeurColonne, HauteurLigne), Theme.TexteTertiaire, Dessin.Droite);
                DessinerTrame(g, trames[i], xt, y, droite, chasse);
            }
        }

        void DessinerTrame(Graphics g, string trame, int x, int y, int limite, int chasse)
        {
            // instruction en accent, longueur en orange, separateurs attenues, donnees en texte
            string[] morceaux = trame.Split(',', 3);
            List<(string texte, Color couleur)> segments = new() { (morceaux[0], Theme.Accent) };
            for (int i = 1; i < morceaux.Length; i++)
            {
                segments.Add((",", Theme.TexteTertiaire));
                bool longueur = morceaux[0] == "2" && i == 1;
                segments.Add((morceaux[i], longueur ? Theme.Orange : Theme.Texte));
            }

            foreach ((string texte, Color couleur) in segments)
            {
                if (x >= limite) break;
                int w = texte.Length * chasse;
                Rectangle r = new(x, y, Math.Min(w + S(2), limite - x), HauteurLigne);
                Dessin.Texte(g, texte, Theme.Mono, r, couleur);
                x += w;
            }
        }
    }
}
