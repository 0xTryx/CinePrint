using System.Drawing.Text;
using Microsoft.Win32;

namespace cineprint.UI
{
    public enum Apparence { Automatique, Clair, Sombre }

    /// <summary>
    /// Couleurs et polices de l'application (themes clair et sombre). Les controles lisent ces
    /// valeurs a chaque dessin ; un changement d'apparence declenche l'evenement Change.
    /// </summary>
    public static class Theme
    {
        // --- Apparence ---

        public static Apparence Apparence { get; private set; } = Apparence.Automatique;
        public static bool Sombre { get; private set; } = SystemeEnSombre();
        public static event Action? Change;

        public static void DefinirApparence(Apparence apparence)
        {
            Apparence = apparence;
            Actualiser();
        }

        /// <summary>A appeler quand Windows signale un changement de theme.</summary>
        public static void Actualiser()
        {
            bool sombre = Apparence switch
            {
                Apparence.Clair => false,
                Apparence.Sombre => true,
                _ => SystemeEnSombre(),
            };
            if (sombre == Sombre) return;
            Sombre = sombre;
            Change?.Invoke();
        }

        static bool SystemeEnSombre()
        {
            try
            {
                using RegistryKey? cle = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return cle?.GetValue("AppsUseLightTheme") is int valeur && valeur == 0;
            }
            catch { return false; }
        }

        // --- Mise a l'echelle (ecrans haute densite) ---

        /// <summary>1 = 96 ppp (100 %), 2 = 192 ppp (200 %)...</summary>
        public static float Echelle { get; set; } = 1f;

        /// <summary>Convertit une mesure "logique" (a 100 %) en pixels reels.</summary>
        public static int S(float valeur) => (int)Math.Round(valeur * Echelle);
        public static float F(float valeur) => valeur * Echelle;

        // --- Couleurs ---

        static Color Hex(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        static Color Choix(int sombre, int clair) => Hex(Sombre ? sombre : clair);

        // Surfaces
        public static Color Fond => Choix(0x111113, 0xF5F5F7);
        public static Color Barre => Choix(0x18181B, 0xEBEBEF);
        public static Color BarreBordure => Choix(0x242428, 0xDADADF);
        public static Color Carte => Choix(0x1C1C1F, 0xFFFFFF);
        public static Color CarteBordure => Choix(0x2A2A2E, 0xE2E2E7);
        public static Color Separateur => Choix(0x2A2A2E, 0xEBEBEF);
        public static Color Champ => Choix(0x29292D, 0xF4F4F6);
        public static Color ChampSurvol => Choix(0x313135, 0xEDEDF0);
        public static Color ChampBordure => Choix(0x37373C, 0xDCDCE1);
        public static Color Menu => Choix(0x242427, 0xFFFFFF);
        public static Color MenuBordure => Choix(0x3A3A3F, 0xD6D6DB);

        // Papier thermique (identique dans les deux themes)
        public static Color Papier => Hex(0xFCFCF9);
        public static Color Encre => Hex(0x1F1F1F);

        // Texte
        public static Color Texte => Choix(0xF5F5F7, 0x1D1D1F);
        public static Color TexteSecondaire => Choix(0x98989F, 0x6E6E73);
        public static Color TexteTertiaire => Choix(0x636368, 0xA1A1A6);

        // Couleurs d'accent
        public static Color Accent => Choix(0x0A84FF, 0x007AFF);
        public static Color Vert => Choix(0x30D158, 0x34C759);
        public static Color Rouge => Choix(0xFF453A, 0xFF3B30);
        public static Color Orange => Choix(0xFF9F0A, 0xFF9500);
        public static Color Violet => Choix(0xBF5AF2, 0xAF52DE);
        public static Color Graphite => Choix(0x636366, 0x8E8E93);
        public static Color Gris => Hex(0x8E8E93);

        // --- Polices (repli sur Segoe UI / Consolas si les variantes sont absentes) ---

        static readonly HashSet<string> familles = new(
            new InstalledFontCollection().Families.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);

        static string Famille(params string[] noms) => noms.FirstOrDefault(familles.Contains) ?? "Segoe UI";

        static readonly string display = Famille("Segoe UI Variable Display", "Segoe UI");
        static readonly string displayDemiGras = Famille("Segoe UI Variable Display Semib", "Segoe UI Semibold");
        static readonly string texte = Famille("Segoe UI Variable Text", "Segoe UI");
        static readonly string texteDemiGras = Famille("Segoe UI Variable Text Semibold", "Segoe UI Semibold");
        static readonly string petit = Famille("Segoe UI Variable Small", "Segoe UI");
        static readonly string petitDemiGras = Famille("Segoe UI Variable Small Semibol", "Segoe UI Semibold");
        static readonly string mono = Famille("Cascadia Mono", "Consolas");
        static readonly string monoGras = Famille("Cascadia Mono SemiBold", "Consolas");

        public static readonly Font Titre = new(display, 19f, FontStyle.Bold);
        public static readonly Font Marque = new(displayDemiGras, 12.5f);
        public static readonly Font Corps = new(texte, 10f);
        public static readonly Font CorpsGras = new(texteDemiGras, 10f);
        public static readonly Font Grand = new(texteDemiGras, 10.5f);
        public static readonly Font Petit = new(petit, 8.5f);
        public static readonly Font PetitGras = new(petitDemiGras, 8.5f);
        public static readonly Font Section = new(petitDemiGras, 7.75f);
        public static readonly Font Mono = new(mono, 9.25f);

        public static string FamilleMono => mono;
        public static string FamilleMonoGras => monoGras;

        public static readonly FontFamily Icones = new(Famille("Segoe Fluent Icons", "Segoe MDL2 Assets"));
    }

    /// <summary>Codes des glyphes Segoe Fluent Icons (repli : Segoe MDL2 Assets).</summary>
    public static class Glyphe
    {
        public const string Imprimante = "\uE749";
        public const string Image = "\uEB9F";
        public const string Qr = "\uED14";
        public const string Console = "\uE756";
        public const string Reglages = "\uE713";
        public const string Calendrier = "\uE787";
        public const string Horloge = "\uE823";
        public const string ChevronBas = "\uE70D";
        public const string ChevronGauche = "\uE76B";
        public const string ChevronDroite = "\uE76C";
        public const string Plus = "\uE710";
        public const string Moins = "\uE738";
        public const string Coche = "\uE73E";
        public const string Lien = "\uE71B";
        public const string Envoyer = "\uE724";
        public const string Eclair = "\uE945";
        public const string Corbeille = "\uE74D";
        public const string Dossier = "\uE838";
        public const string Info = "\uE946";
        public const string Alerte = "\uE7BA";
        public const string Soleil = "\uE706";
        public const string Lune = "\uE708";
        public const string Contraste = "\uE7A1";
        public const string Reduire = "\uE921";
        public const string Agrandir = "\uE922";
        public const string Restaurer = "\uE923";
        public const string Fermer = "\uE8BB";
        public const string Deconnecter = "\uE711";
    }
}
