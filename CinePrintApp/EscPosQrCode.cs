using System.Drawing;
using QRCoder;

namespace cineprint
{
    /// <summary>
    /// Generation d'un QR code (encodage via QRCoder) sous forme de matrice de bits.
    /// Il est imprime comme une image raster GS v 0 (EscPosImage.CommandeDepuisMatriceBinaire),
    /// sans utiliser la commande QR native de l'imprimante.
    /// </summary>
    public static class EscPosQrCode
    {
        // Marge blanche ajoutee autour de la matrice QRCoder (qui contient deja une zone de silence de 4 modules).
        private const int ZoneBlanche = 4;

        /// <summary>Encode le contenu (lien ou texte) en matrice de modules noir/blanc, marge incluse.</summary>
        public static bool[,] GenererMatrice(string contenu)
        {
            using QRCodeGenerator generateur = new();
            // Correction d'erreur niveau M (~15 %). Q ou H donnent une matrice plus dense,
            // donc plus d'octets a envoyer pour le meme contenu.
            QRCodeData data = generateur.CreateQrCode(contenu, QRCodeGenerator.ECCLevel.M);

            int taille = data.ModuleMatrix.Count;
            int tailleAvecMarge = taille + ZoneBlanche * 2;

            bool[,] matrice = new bool[tailleAvecMarge, tailleAvecMarge];
            for (int y = 0; y < taille; y++)
                for (int x = 0; x < taille; x++)
                    matrice[x + ZoneBlanche, y + ZoneBlanche] = data.ModuleMatrix[y][x];

            return matrice;
        }

        /// <summary>Rendu Bitmap de la matrice (pixelsParModule pixels par module), independant de l'impression.</summary>
        public static Bitmap GenererApercu(bool[,] matrice, int pixelsParModule = 6)
        {
            int taille = matrice.GetLength(0);
            Bitmap bmp = new(taille * pixelsParModule, taille * pixelsParModule);

            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            for (int y = 0; y < taille; y++)
                for (int x = 0; x < taille; x++)
                    if (matrice[x, y])
                        g.FillRectangle(Brushes.Black, x * pixelsParModule, y * pixelsParModule, pixelsParModule, pixelsParModule);

            return bmp;
        }
    }
}
