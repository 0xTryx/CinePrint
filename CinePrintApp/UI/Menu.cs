namespace cineprint.UI
{
    /// <summary>
    /// Rendu des menus et popups : fond et texte du theme, surbrillance arrondie,
    /// coins arrondis DWM.
    /// </summary>
    public class RenduMenu : ToolStripRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using SolidBrush b = new(Theme.Menu);
            e.Graphics.FillRectangle(b, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected || !e.Item.Enabled) return;
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            RectangleF r = new(Theme.F(5), Theme.F(1), e.Item.Width - Theme.F(10), e.Item.Height - Theme.F(2));
            Dessin.Remplir(g, r, Theme.F(6), Theme.Accent);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = !e.Item.Enabled ? Theme.TexteTertiaire : e.Item.Selected ? Color.White : Theme.Texte;
            e.TextFormat |= TextFormatFlags.NoPrefix;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            Dessin.Lisser(g);
            Color c = e.Item.Selected ? Color.White : Theme.Accent;
            // colonne des coches reservee par le menu, centree verticalement sur l'element
            Rectangle zone = e.ImageRectangle;
            RectangleF r = new(zone.X + Theme.F(2), 0, zone.Width, e.Item.Height);
            Dessin.Icone(g, Glyphe.Coche, r, Theme.F(12), c);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.TexteSecondaire;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            float y = e.Item.Height / 2f;
            Dessin.Ligne(e.Graphics, Theme.F(10), y, e.Item.Width - Theme.F(10), Theme.Separateur);
        }
    }

    public static class Menus
    {
        /// <summary>Menu contextuel au style de l'application.</summary>
        public static ContextMenuStrip Creer()
        {
            ContextMenuStrip menu = new()
            {
                Renderer = new RenduMenu(),
                ShowImageMargin = false,
                ShowCheckMargin = true,
                Font = Theme.Corps,
                Padding = new Padding(0, Theme.S(5), 0, Theme.S(5)),
                DropShadowEnabled = false,
                BackColor = Theme.Menu,
            };
            menu.HandleCreated += (s, e) => Natif.ArrondirFenetre(menu.Handle, false, Theme.MenuBordure);
            return menu;
        }

        public static ToolStripMenuItem Element(string texte, bool coche, EventHandler clic)
        {
            ToolStripMenuItem item = new(texte)
            {
                Checked = coche,
                Padding = new Padding(0, Theme.S(4), Theme.S(10), Theme.S(4)),
                ForeColor = Theme.Texte,
            };
            item.Click += clic;
            return item;
        }

        /// <summary>
        /// Popup libre (calendrier, grille d'heures...) : un ToolStripDropDown qui heberge un controle,
        /// se ferme tout seul au clic exterieur et ne vole pas l'activation de la fenetre.
        /// </summary>
        public static ToolStripDropDown Popup(Control contenu)
        {
            ToolStripControlHost hote = new(contenu)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = contenu.Size,
            };
            ToolStripDropDown popup = new()
            {
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                DropShadowEnabled = false,
                Renderer = new RenduMenu(),
                BackColor = Theme.Menu,
                AutoSize = true,
            };
            popup.Items.Add(hote);
            popup.HandleCreated += (s, e) => Natif.ArrondirFenetre(popup.Handle, false, Theme.MenuBordure);
            popup.Closed += (s, e) => popup.BeginInvoke(new Action(() => popup.Dispose()));
            return popup;
        }
    }
}
