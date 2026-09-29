using System.Globalization;

namespace cineprint.UI
{
    /// <summary>
    /// Selecteur de date et d'heure : deux pastilles qui ouvrent un calendrier et une grille
    /// d'horaires. Molette : +/- 1 jour ou +/- 5 minutes.
    /// </summary>
    public class SelecteurSeance : ControleAnneau
    {
        static readonly CultureInfo fr = new("fr-FR");

        DateTime valeur = DateTime.Now;
        readonly Anim survolDate, survolHeure;
        int ouvert = -1;

        public event EventHandler? ValeurChange;

        public SelecteurSeance()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            survolDate = new Anim(this, 0f, 40f);
            survolHeure = new Anim(this, 0f, 40f);
        }

        public DateTime Valeur
        {
            get => valeur;
            set
            {
                DateTime v = new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
                if (v == valeur) return;
                valeur = v;
                Invalidate();
                ValeurChange?.Invoke(this, EventArgs.Empty);
            }
        }

        string TexteDate
        {
            get
            {
                string t = valeur.ToString("ddd d MMM yyyy", fr);
                return char.ToUpper(t[0]) + t[1..];
            }
        }

        string TexteHeure => valeur.ToString("HH:mm");

        int LargeurPastille(string texte) => S(34) + Dessin.Mesurer(texte, Theme.Corps).Width + S(14);

        public int LargeurIdeale => LargeurPastille("Mer. 30 sept. 2026") + S(8) + LargeurPastille("00:00") + 2 * Marge;

        RectangleF ZoneHeure
        {
            get
            {
                RectangleF z = Zone;
                float w = LargeurPastille(TexteHeure);
                return new RectangleF(z.Right - w, z.Y, w, z.Height);
            }
        }

        RectangleF ZoneDate
        {
            get
            {
                RectangleF h = ZoneHeure;
                float w = LargeurPastille(TexteDate);
                return new RectangleF(h.X - S(8) - w, h.Y, w, h.Height);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            survolDate.Vers(ZoneDate.Contains(e.Location) ? 1f : 0f);
            survolHeure.Vers(ZoneHeure.Contains(e.Location) ? 1f : 0f);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survolDate.Vers(0f);
            survolHeure.Vers(0f);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            if (ZoneDate.Contains(e.Location)) Ouvrir(0);
            else if (ZoneHeure.Contains(e.Location)) Ouvrir(1);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int sens = Math.Sign(e.Delta);
            if (ZoneDate.Contains(e.Location)) Valeur = valeur.AddDays(sens);
            else if (ZoneHeure.Contains(e.Location)) Valeur = valeur.AddMinutes(5 * sens);
            if (e is HandledMouseEventArgs h) h.Handled = true;
        }

        protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) Ouvrir(0);
            else if (e.KeyCode == Keys.Up) Valeur = valeur.AddMinutes(5);
            else if (e.KeyCode == Keys.Down) Valeur = valeur.AddMinutes(-5);
        }

        void Ouvrir(int quoi)
        {
            Control contenu;
            ToolStripDropDown? popup = null;
            if (quoi == 0)
            {
                Calendrier cal = new(valeur.Date);
                cal.DateChoisie += d =>
                {
                    Valeur = d.Date + valeur.TimeOfDay;
                    popup?.Close();
                };
                contenu = cal;
            }
            else
            {
                GrilleHeure grille = new(valeur.Hour, valeur.Minute);
                grille.Change += (h, m, fermer) =>
                {
                    Valeur = valeur.Date + new TimeSpan(h, m, 0);
                    if (fermer) popup?.Close();
                };
                contenu = grille;
            }

            popup = Menus.Popup(contenu);
            ouvert = quoi;
            Invalidate();
            popup.Closed += (s, e) => { ouvert = -1; Invalidate(); };
            RectangleF zp = quoi == 0 ? ZoneDate : ZoneHeure;
            popup.Show(this, new Point((int)Math.Round(zp.Right - contenu.Width), (int)Math.Round(zp.Bottom + S(6))));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Pastille(g, ZoneDate, Glyphe.Calendrier, TexteDate, survolDate.Valeur, ouvert == 0);
            Pastille(g, ZoneHeure, Glyphe.Horloge, TexteHeure, survolHeure.Valeur, ouvert == 1);

            if (focus.Valeur > 0.01f)
            {
                RectangleF r = RectangleF.Union(ZoneDate, ZoneHeure);
                float ep = F(3) * focus.Valeur;
                Dessin.Contour(g, RectangleF.Inflate(r, ep, ep), F(8) + ep, Dessin.Alpha(Theme.Accent, 0.42f * focus.Valeur), ep);
            }
        }

        void Pastille(Graphics g, RectangleF r, string glyphe, string texte, float survol, bool actif)
        {
            Color surface = Surface;
            Color fond = actif ? Dessin.Voile(surface, Theme.Accent, 0.16f) : Dessin.Melange(Theme.Champ, Theme.ChampSurvol, survol);
            Color couleur = actif ? Theme.Accent : Theme.Texte;
            Dessin.Remplir(g, r, F(8), fond);
            Dessin.Icone(g, glyphe, new RectangleF(r.X + S(10), r.Y, S(18), r.Height), F(12.5f), actif ? Theme.Accent : Theme.TexteSecondaire);
            Dessin.Texte(g, texte, Theme.Corps, new Rectangle((int)r.X + S(34), (int)r.Y, (int)r.Width - S(40), (int)r.Height), couleur);
        }
    }

    /// <summary>Calendrier mensuel (lundi en premier) affiche dans un popup.</summary>
    public class Calendrier : ControleBase
    {
        static readonly CultureInfo fr = new("fr-FR");
        DateTime mois;
        readonly DateTime selection;
        int survol = -1;

        public event Action<DateTime>? DateChoisie;

        public Calendrier(DateTime selection)
        {
            this.selection = selection.Date;
            mois = new DateTime(selection.Year, selection.Month, 1);
            Size = new Size(S(304), S(338));
        }

        DateTime Debut
        {
            get
            {
                int decalage = ((int)mois.DayOfWeek + 6) % 7; // lundi = 0
                return mois.AddDays(-decalage);
            }
        }

        const int Lignes = 6;
        int HautGrille => S(80);
        float LargeurCase => (Width - S(24)) / 7f;
        int HauteurCase => S(36);

        RectangleF ZoneCase(int i) => new(S(12) + (i % 7) * LargeurCase, HautGrille + (i / 7) * HauteurCase, LargeurCase, HauteurCase);
        RectangleF ZonePrecedent => new(Width - S(78), S(14), S(30), S(30));
        RectangleF ZoneSuivant => new(Width - S(44), S(14), S(30), S(30));
        RectangleF ZoneAujourdhui => new(S(14), Height - S(40), S(100), S(30));

        int IndexA(Point p)
        {
            if (ZonePrecedent.Contains(p)) return 100;
            if (ZoneSuivant.Contains(p)) return 101;
            if (ZoneAujourdhui.Contains(p)) return 102;
            for (int i = 0; i < 7 * Lignes; i++) if (ZoneCase(i).Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = IndexA(e.Location);
            if (i != survol) { survol = i; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survol = -1;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int i = IndexA(e.Location);
            if (i == 100) mois = mois.AddMonths(-1);
            else if (i == 101) mois = mois.AddMonths(1);
            else if (i == 102) DateChoisie?.Invoke(DateTime.Today);
            else if (i >= 0) DateChoisie?.Invoke(Debut.AddDays(i));
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            mois = mois.AddMonths(-Math.Sign(e.Delta));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color fond = Theme.Menu;

            string titre = mois.ToString("MMMM yyyy", fr);
            titre = char.ToUpper(titre[0]) + titre[1..];
            Dessin.Texte(g, titre, Theme.Grand, new Rectangle(S(18), S(14), Width - S(110), S(30)), Theme.Texte);

            foreach ((RectangleF r, string gl, int id) in new[] { (ZonePrecedent, Glyphe.ChevronGauche, 100), (ZoneSuivant, Glyphe.ChevronDroite, 101) })
            {
                if (survol == id) Dessin.Remplir(g, r, F(8), Dessin.Voile(fond, Theme.Texte, 0.08f));
                Dessin.Icone(g, gl, r, F(11), Theme.Accent);
            }

            string[] jours = { "L", "M", "M", "J", "V", "S", "D" };
            for (int i = 0; i < 7; i++)
                Dessin.Texte(g, jours[i], Theme.PetitGras, Rectangle.Round(new RectangleF(S(12) + i * LargeurCase, S(52), LargeurCase, S(22))),
                    Theme.TexteTertiaire, Dessin.Centre);

            DateTime debut = Debut;
            for (int i = 0; i < 7 * Lignes; i++)
            {
                DateTime jour = debut.AddDays(i);
                RectangleF c = ZoneCase(i);
                float d = Math.Min(c.Width, c.Height) - F(3);
                RectangleF rond = new(c.X + (c.Width - d) / 2f, c.Y + (c.Height - d) / 2f, d, d);
                bool choisi = jour == selection;
                bool aujourdhui = jour == DateTime.Today;
                bool horsMois = jour.Month != mois.Month;

                if (choisi)
                    using (SolidBrush b = new(Theme.Accent)) g.FillEllipse(b, rond);
                else if (survol == i)
                    using (SolidBrush b = new(Dessin.Voile(fond, Theme.Texte, 0.08f))) g.FillEllipse(b, rond);

                Color couleur = choisi ? Color.White : aujourdhui ? Theme.Accent : horsMois ? Theme.TexteTertiaire : Theme.Texte;
                Font police = choisi || aujourdhui ? Theme.CorpsGras : Theme.Corps;
                Dessin.Texte(g, jour.Day.ToString(), police, Rectangle.Round(c), couleur, Dessin.Centre);
            }

            Dessin.Ligne(g, S(12), Height - S(48), Width - S(12), Theme.Separateur);
            RectangleF za = ZoneAujourdhui;
            if (survol == 102) Dessin.Remplir(g, za, F(8), Dessin.Voile(fond, Theme.Texte, 0.08f));
            Dessin.Texte(g, "Aujourd’hui", Theme.CorpsGras, Rectangle.Round(za), Theme.Accent, Dessin.Centre);
        }
    }

    /// <summary>Grille d'horaires : heures (00-23) puis minutes par pas de 5.</summary>
    public class GrilleHeure : ControleBase
    {
        int heure, minute, survol = -1;

        public event Action<int, int, bool>? Change;

        public GrilleHeure(int heure, int minute)
        {
            this.heure = heure;
            this.minute = minute;
            Size = new Size(S(300), S(286));
        }

        const int Colonnes = 6;
        float LargeurCase => (Width - S(24)) / (float)Colonnes;
        int HauteurCase => S(34);
        int HautHeures => S(36);
        int HautMinutes => HautHeures + 4 * HauteurCase + S(36);

        RectangleF Case(int index)
        {
            bool estMinute = index >= 24;
            int i = estMinute ? index - 24 : index;
            float y = (estMinute ? HautMinutes : HautHeures) + (i / Colonnes) * HauteurCase;
            return new RectangleF(S(12) + (i % Colonnes) * LargeurCase, y, LargeurCase, HauteurCase);
        }

        int IndexA(Point p)
        {
            for (int i = 0; i < 36; i++) if (Case(i).Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = IndexA(e.Location);
            if (i != survol) { survol = i; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            survol = -1;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int i = IndexA(e.Location);
            if (i < 0) return;
            if (i < 24) { heure = i; Change?.Invoke(heure, minute, false); }
            else { minute = (i - 24) * 5; Change?.Invoke(heure, minute, true); }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color fond = Theme.Menu;

            Dessin.Texte(g, "HEURE", Theme.Section, new Rectangle(S(18), S(10), Width, S(20)), Theme.TexteTertiaire);
            Dessin.Texte(g, "MINUTES", Theme.Section, new Rectangle(S(18), HautMinutes - S(26), Width, S(20)), Theme.TexteTertiaire);

            for (int i = 0; i < 36; i++)
            {
                RectangleF c = RectangleF.Inflate(Case(i), -F(2), -F(2));
                int v = i < 24 ? i : (i - 24) * 5;
                bool choisi = i < 24 ? v == heure : v == minute;
                if (choisi) Dessin.Remplir(g, c, F(7), Theme.Accent);
                else if (survol == i) Dessin.Remplir(g, c, F(7), Dessin.Voile(fond, Theme.Texte, 0.08f));
                Dessin.Texte(g, v.ToString("00"), choisi ? Theme.CorpsGras : Theme.Corps, Rectangle.Round(c),
                    choisi ? Color.White : Theme.Texte, Dessin.Centre);
            }
        }
    }
}
