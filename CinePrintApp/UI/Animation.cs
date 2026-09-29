using System.Diagnostics;

namespace cineprint.UI
{
    /// <summary>
    /// Valeur animee (survol, appui, focus...) qui rejoint sa cible par decroissance exponentielle.
    /// </summary>
    public sealed class Anim
    {
        readonly Control proprietaire;
        readonly float constanteTemps;

        public float Valeur { get; private set; }
        public float Cible { get; private set; }
        public event Action? Avance;

        /// <param name="constanteTemps">en ms : ~63 % du chemin parcouru apres ce delai.</param>
        public Anim(Control proprietaire, float valeur = 0f, float constanteTemps = 45f)
        {
            this.proprietaire = proprietaire;
            this.constanteTemps = constanteTemps;
            Valeur = Cible = valeur;
        }

        public void Vers(float cible)
        {
            Cible = cible;
            if (Valeur != cible) Animateur.Inscrire(this);
        }

        public void Fixer(float valeur)
        {
            Valeur = Cible = valeur;
            proprietaire.Invalidate();
        }

        internal bool Avancer(float dt)
        {
            float k = 1f - MathF.Exp(-dt / constanteTemps);
            Valeur += (Cible - Valeur) * k;
            if (MathF.Abs(Cible - Valeur) < 0.0025f * Math.Max(1f, MathF.Abs(Cible))) Valeur = Cible;
            if (proprietaire.IsDisposed) return true;
            Avance?.Invoke();
            proprietaire.Invalidate();
            return Valeur == Cible;
        }
    }

    /// <summary>Une seule minuterie (~60 images/s) pour toutes les animations en cours.</summary>
    internal static class Animateur
    {
        static readonly System.Windows.Forms.Timer minuterie = new() { Interval = 15 };
        static readonly List<Anim> actives = new();
        static readonly Stopwatch chrono = Stopwatch.StartNew();
        static long dernier;

        static Animateur() => minuterie.Tick += Tick;

        public static void Inscrire(Anim anim)
        {
            if (!actives.Contains(anim)) actives.Add(anim);
            if (!minuterie.Enabled)
            {
                dernier = chrono.ElapsedMilliseconds;
                minuterie.Start();
            }
        }

        static void Tick(object? sender, EventArgs e)
        {
            long maintenant = chrono.ElapsedMilliseconds;
            float dt = Math.Clamp(maintenant - dernier, 1, 50);
            dernier = maintenant;
            for (int i = actives.Count - 1; i >= 0; i--)
                if (actives[i].Avancer(dt)) actives.RemoveAt(i);
            if (actives.Count == 0) minuterie.Stop();
        }
    }
}
