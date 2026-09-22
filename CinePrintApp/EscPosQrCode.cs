using System.Drawing;
using QRCoder;

namespace cineprint
{
    /// <summary>
    /// Generation d'un QR code (encodage via QRCoder) sous forme de matrice de bits,
    /// convertie ensuite en commande ESC/POS via EscPosImage.CommandeDepuisMatriceBinaire.
    /// Pas besoin de commande "QR code" native de l'imprimante : on l'imprime comme une image,
    /// via le meme chemin GS v 0 deja teste et fonctionnel.
    /// </summary>
    public static class EscPosQrCode
    {
        // Marge de securite (quiet zone) autour du QR code, recommandee par la norme
        // pour que les scanners l'identifient correctement.
        private const int ZoneBlanche = 4;

        /// <summary>Encode le contenu (lien ou texte) en matrice de modules noir/blanc, marge incluse.</summary>
        public static bool[,] GenererMatrice(string contenu)
        {
            using QRCodeGenerator generateur = new();
            // Niveau M (15% de correction) : bon compromis lisibilite/taille pour un lien court.
            // Q ou H generent une matrice plus dense (donc plus d'octets a transmettre) pour le meme contenu.
            QRCodeData data = generateur.CreateQrCode(contenu, QRCodeGenerator.ECCLevel.M);

            int taille = data.ModuleMatrix.Count;
            int tailleAvecMarge = taille + ZoneBlanche * 2;

            bool[,] matrice = new bool[tailleAvecMarge, tailleAvecMarge];
            for (int y = 0; y < taille; y++)
                for (int x = 0; x < taille; x++)
                    matrice[x + ZoneBlanche, y + ZoneBlanche] = data.ModuleMatrix[y][x];

            return matrice;
        }

        /// <summary>Rendu Bitmap pour l'apercu dans l'UI (taille d'affichage, independante de l'impression).</summary>
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
