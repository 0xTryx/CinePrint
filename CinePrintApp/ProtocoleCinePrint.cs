using System;
using System.Collections.Generic;
using System.Linq;

namespace cineprint
{
    /// <summary>
    /// Construction des trames du protocole CinePrint (voir README du repo).
    ///
    ///   0,ping                         -> Arduino repond "0,pong"
    ///   1,<texte>                      -> Arduino fait socket.println("<texte>")   -> "1,ok"
    ///   2,<len>,<o1;o2;...> (hexa)     -> Arduino fait socket.write({...}, len)    -> "2,<len>,ok"
    ///
    /// Chaque trame est terminee par un '\n' au moment de l'envoi.
    /// </summary>
    public static class Protocole
    {
        // --- Trames de base ---

        public static string Ping() => "0,ping";

        public static string Println(string texte) => "1," + (texte ?? string.Empty);

        public static string Write(params byte[] donnees)
        {
            string hexa = string.Join(";", donnees.Select(b => b.ToString("X2")));
            return "2," + donnees.Length + "," + hexa;
        }

        // --- Raccourcis ESC/POS (envoyes via l'instruction 2 = write) ---

        /// <summary>ESC @ : reinitialise l'imprimante.</summary>
        public static string Init() => Write(0x1B, 0x40);

        /// <summary>ESC a n : alignement (0 = gauche, 1 = centre, 2 = droite).</summary>
        public static string Alignement(byte n) => Write(0x1B, 0x61, n);

        /// <summary>GS ! n : taille des caracteres. Quartet haut = largeur, quartet bas = hauteur.</summary>
        public static string Taille(byte largeur, byte hauteur)
            => Write(0x1D, 0x21, (byte)(((largeur & 0x0F) << 4) | (hauteur & 0x0F)));

        public static string TailleNormale() => Taille(0, 0);
        public static string TailleGrande() => Taille(1, 1);

        /// <summary>ESC E n : gras on/off.</summary>
        public static string Gras(bool actif) => Write(0x1B, 0x45, (byte)(actif ? 1 : 0));

        /// <summary>Avance papier de n lignes.</summary>
        public static string SautLignes(byte n) => Write(0x1B, 0x64, n);

        // --- Ticket complet ---

        /// <summary>
        /// Genere la sequence de trames a envoyer pour imprimer un ticket de cinema.
        /// </summary>
        public static List<string> Ticket(string cinema, string film, int salle, DateTime seance)
        {
            string dateHeure = seance.ToString("dd/MM/yyyy 'a' HH'h'mm");

            return new List<string>
            {
                Init(),
                Alignement(1),          // centre
                TailleGrande(),
                Println(cinema.ToUpper()),
                TailleNormale(),
                Println(""),
                Gras(true),
                Println(film),
                Gras(false),
                Println(""),
                Println("Salle " + salle),
                Println(dateHeure),
                Alignement(0),          // retour a gauche
                SautLignes(4),
            };
        }
    }
}
