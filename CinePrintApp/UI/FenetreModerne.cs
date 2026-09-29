using System.Runtime.InteropServices;

namespace cineprint.UI
{
    /// <summary>
    /// Fenetre sans barre de titre native : WM_NCCALCSIZE etend la zone cliente sur la barre de
    /// titre en conservant le cadre (ombre, redimensionnement, Snap). Les points pour lesquels les
    /// controles enfants renvoient HTTRANSPARENT sont traites comme barre de titre (HTCAPTION).
    /// </summary>
    public class FenetreModerne : Form
    {
        Size tailleClient = new(1100, 720);
        Size tailleMin = new(900, 600);

        public FenetreModerne()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;
            Theme.Change += AppliquerTheme;
        }

        /// <summary>Taille de la zone cliente voulue (en pixels logiques, a 100 %).</summary>
        public void DefinirTaille(Size client, Size minimum)
        {
            tailleClient = client;
            tailleMin = minimum;
        }

        protected virtual void AppliquerTheme()
        {
            BackColor = Theme.Fond;
            if (IsHandleCreated)
            {
                Natif.ModeSombre(Handle, Theme.Sombre);
                Natif.ArrondirFenetre(Handle, false, Theme.Sombre ? Color.FromArgb(0x3A, 0x3A, 0x3F) : Color.FromArgb(0xC8, 0xC8, 0xCE));
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AppliquerTheme();
            // Force le recalcul du cadre : WM_NCCALCSIZE retire la barre de titre native.
            Natif.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                Natif.SWP_FRAMECHANGED | Natif.SWP_NOMOVE | Natif.SWP_NOSIZE | Natif.SWP_NOZORDER | Natif.SWP_NOACTIVATE);

            int dx = Width - ClientSize.Width, dy = Height - ClientSize.Height;
            MinimumSize = new Size(Theme.S(tailleMin.Width) + dx, Theme.S(tailleMin.Height) + dy);
            Size = new Size(Theme.S(tailleClient.Width) + dx, Theme.S(tailleClient.Height) + dy);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (StartPosition == FormStartPosition.CenterScreen) CenterToScreen();
        }

        int EpaisseurCadre
        {
            get
            {
                uint dpi = (uint)DeviceDpi;
                return Natif.GetSystemMetricsForDpi(Natif.SM_CYFRAME, dpi) + Natif.GetSystemMetricsForDpi(Natif.SM_CXPADDEDBORDER, dpi);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Natif.WM_NCCALCSIZE when m.WParam != IntPtr.Zero:
                {
                    // Calcul standard (bordures invisibles gauche/droite/bas conservees pour le
                    // redimensionnement), puis on remonte le haut de la zone cliente sur la barre de titre.
                    Natif.NCCALCSIZE_PARAMS p = Marshal.PtrToStructure<Natif.NCCALCSIZE_PARAMS>(m.LParam);
                    int hautOrigine = p.Rgrc0.Top;
                    base.WndProc(ref m);
                    p = Marshal.PtrToStructure<Natif.NCCALCSIZE_PARAMS>(m.LParam);
                    p.Rgrc0.Top = hautOrigine + (Natif.IsZoomed(Handle) ? EpaisseurCadre : 0);
                    Marshal.StructureToPtr(p, m.LParam, false);
                    m.Result = IntPtr.Zero;
                    return;
                }

                case Natif.WM_NCHITTEST:
                {
                    base.WndProc(ref m);
                    if ((int)m.Result != Natif.HTCLIENT) return;
                    // Seuls les points laisses "transparents" par les enfants arrivent ici.
                    Point p = PointToClient(new Point((short)(m.LParam.ToInt64() & 0xFFFF), (short)((m.LParam.ToInt64() >> 16) & 0xFFFF)));
                    int bord = Theme.S(5);
                    if (WindowState == FormWindowState.Normal && p.Y < bord)
                        m.Result = (IntPtr)(p.X < bord * 2 ? Natif.HTTOPLEFT : p.X > ClientSize.Width - bord * 2 ? Natif.HTTOPRIGHT : Natif.HTTOP);
                    else
                        m.Result = (IntPtr)Natif.HTCAPTION;
                    return;
                }

                case Natif.WM_SETTINGCHANGE:
                    base.WndProc(ref m);
                    if (m.LParam != IntPtr.Zero && Marshal.PtrToStringUni(m.LParam) == "ImmersiveColorSet")
                        Theme.Actualiser();
                    return;
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Change -= AppliquerTheme;
            base.Dispose(disposing);
        }
    }
}
