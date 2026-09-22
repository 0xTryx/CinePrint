using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace cineprint
{
    /// <summary>
    /// Conversion d'une image en commande ESC/POS "raster bit image" (GS v 0)
    /// et decoupage en petites trames compatibles avec la RAM limitee (2 Ko) de l'Arduino Uno.
    /// </summary>
    public static class EscPosImage
    {
        /// <summary>
        /// Convertit une image en bitmap 1 bit (noir/blanc, dithering Floyd-Steinberg)
        /// a la largeur donnee (en points d'imprimante), puis construit la commande
        /// ESC/POS GS v 0 complete (entete + pixels).
        /// </summary>
        public static byte[] ConvertirEnCommandeEscPos(Bitmap source, int largeurPoints)
        {
            int largeurOctets = (largeurPoints + 7) / 8; // arrondi au multiple de 8 superieur
            int largeurReelle = largeurOctets * 8;
            int hauteurPoints = (int)Math.Round((double)source.Height / source.Width * largeurReelle);
            if (hauteurPoints < 1) hauteurPoints = 1;

            using Bitmap redim = new Bitmap(largeurReelle, hauteurPoints);
            using (Graphics g = Graphics.FromImage(redim))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, largeurReelle, hauteurPoints);
            }

            // Niveaux de gris (en double, pour laisser l'erreur du dithering se propager)
            double[,] gris = new double[largeurReelle, hauteurPoints];
            for (int y = 0; y < hauteurPoints; y++)
                for (int x = 0; x < largeurReelle; x++)
                {
                    Color c = redim.GetPixel(x, y);
                    gris[x, y] = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                }

            byte[] pixels = new byte[largeurOctets * hauteurPoints]; // 1 octet = 8 points horizontaux

            // Dithering Floyd-Steinberg : rendu bien plus lisible qu'un simple seuillage
            for (int y = 0; y < hauteurPoints; y++)
            {
                for (int x = 0; x < largeurReelle; x++)
                {
                    double niveau = gris[x, y];
                    bool noir = niveau < 128;
                    double erreur = niveau - (noir ? 0 : 255);

                    if (noir)
                    {
                        int octetIndex = y * largeurOctets + (x / 8);
                        int bit = 7 - (x % 8);
                        pixels[octetIndex] |= (byte)(1 << bit);
                    }

                    if (x + 1 < largeurReelle) gris[x + 1, y] += erreur * 7.0 / 16.0;
                    if (y + 1 < hauteurPoints)
                    {
                        if (x - 1 >= 0) gris[x - 1, y + 1] += erreur * 3.0 / 16.0;
                        gris[x, y + 1] += erreur * 5.0 / 16.0;
                        if (x + 1 < largeurReelle) gris[x + 1, y + 1] += erreur * 1.0 / 16.0;
                    }
                }
            }

            return ConstruireCommandeRaster(pixels, largeurOctets, hauteurPoints);
        }

        /// <summary>
        /// Construit une commande ESC/POS GS v 0 a partir d'une matrice de bits pure
        /// (vrai/faux = noir/blanc), chaque case etant agrandie a la taille voulue en points.
        /// Utilise pour les QR codes : pas de dithering necessaire, c'est deja du noir/blanc net.
        /// </summary>
        public static byte[] CommandeDepuisMatriceBinaire(bool[,] matrice, int pointsParModule)
        {
            int largeurModules = matrice.GetLength(0);
            int hauteurModules = matrice.GetLength(1);
            int largeurPoints = largeurModules * pointsParModule;
            int hauteurPoints = hauteurModules * pointsParModule;
            int largeurOctets = (largeurPoints + 7) / 8;

            byte[] pixels = new byte[largeurOctets * hauteurPoints];

            for (int my = 0; my < hauteurModules; my++)
            {
                for (int mx = 0; mx < largeurModules; mx++)
                {
                    if (!matrice[mx, my]) continue;

                    for (int dy = 0; dy < pointsParModule; dy++)
                    {
                        int y = my * pointsParModule + dy;
                        for (int dx = 0; dx < pointsParModule; dx++)
                        {
                            int x = mx * pointsParModule + dx;
                            int octetIndex = y * largeurOctets + (x / 8);
                            int bit = 7 - (x % 8);
                            pixels[octetIndex] |= (byte)(1 << bit);
                        }
                    }
                }
            }

            return ConstruireCommandeRaster(pixels, largeurOctets, hauteurPoints);
        }

        /// <summary>En-tete GS v 0 : 1D 76 30 m xL xH yL yH (m = 0 -> mode normal), suivi des pixels.</summary>
        private static byte[] ConstruireCommandeRaster(byte[] pixels, int largeurOctets, int hauteurPoints)
        {
            byte[] entete =
            {
                0x1D, 0x76, 0x30, 0x00,
                (byte)(largeurOctets & 0xFF), (byte)((largeurOctets >> 8) & 0xFF),
                (byte)(hauteurPoints & 0xFF), (byte)((hauteurPoints >> 8) & 0xFF),
            };

            byte[] commande = new byte[entete.Length + pixels.Length];
            Buffer.BlockCopy(entete, 0, commande, 0, entete.Length);
            Buffer.BlockCopy(pixels, 0, commande, entete.Length, pixels.Length);
            return commande;
        }

        /// <summary>
        /// Decoupe une commande ESC/POS en plusieurs trames protocole "2,len,hexa" de taille
        /// bornee, pour ne jamais depasser ce que l'Arduino Uno peut encaisser en une fois.
        /// </summary>
        public static List<string> Decouper(byte[] commande, int octetsParTrame)
        {
            List<string> trames = new();
            for (int i = 0; i < commande.Length; i += octetsParTrame)
            {
                int taille = Math.Min(octetsParTrame, commande.Length - i);
                byte[] morceau = new byte[taille];
                Buffer.BlockCopy(commande, i, morceau, 0, taille);
                trames.Add(Protocole.Write(morceau));
            }
            return trames;
        }
    }
}
