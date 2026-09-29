using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cineprint.UI
{
    /// <summary>Logo de l'application (dessine en vectoriel) et icone de fenetre multi-resolution.</summary>
    public static class LogoApp
    {
        static readonly Color haut = Color.FromArgb(0xFF, 0x5E, 0x3A);
        static readonly Color bas = Color.FromArgb(0xFF, 0x2A, 0x68);

        public static void Dessiner(Graphics g, RectangleF zone)
        {
            using (GraphicsPath forme = Dessin.Arrondi(zone, zone.Width * 0.235f))
            {
                using (LinearGradientBrush degrade = new(new PointF(zone.X, zone.Y), new PointF(zone.Right, zone.Bottom), haut, bas))
                    g.FillPath(degrade, forme);

                // reflet sur la moitie haute
                RectangleF reflet = new(zone.X, zone.Y, zone.Width, zone.Height * 0.55f);
                using LinearGradientBrush brillance = new(reflet, Color.FromArgb(46, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical);
                GraphicsState etat = g.Save();
                g.SetClip(forme, CombineMode.Intersect);
                g.FillRectangle(brillance, reflet);
                g.Restore(etat);
            }

            RectangleF ticket = RectangleF.Inflate(zone, -zone.Width * 0.17f, -zone.Height * 0.17f);
            RectangleF ombre = ticket;
            ombre.Offset(0, zone.Height * 0.035f);
            Dessin.IconeTicket(g, ombre, Color.FromArgb(55, 90, 0, 20), -14f);
            Dessin.IconeTicket(g, ticket, Color.White, -14f);
        }

        /// <summary>Icone .ico multi-tailles (PNG) generee a la volee pour la barre des taches.</summary>
        public static Icon CreerIcone()
        {
            int[] tailles = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            List<byte[]> images = tailles.Select(Png).ToList();

            using MemoryStream ms = new();
            using BinaryWriter bw = new(ms);
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)tailles.Length);
            int decalage = 6 + 16 * tailles.Length;
            for (int i = 0; i < tailles.Length; i++)
            {
                byte t = (byte)(tailles[i] >= 256 ? 0 : tailles[i]);
                bw.Write(t);
                bw.Write(t);
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(images[i].Length);
                bw.Write(decalage);
                decalage += images[i].Length;
            }
            foreach (byte[] image in images) bw.Write(image);
            bw.Flush();
            ms.Position = 0;
            return new Icon(ms);
        }

        static byte[] Png(int taille)
        {
            using Bitmap bmp = new(taille, taille, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Dessin.Lisser(g);
                float marge = taille <= 24 ? 0.5f : taille * 0.04f;
                Dessiner(g, new RectangleF(marge, marge, taille - 2 * marge, taille - 2 * marge));
            }
            using MemoryStream ms = new();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
