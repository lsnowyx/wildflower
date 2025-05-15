namespace wildflower
{
    public static class Helper
    {
        public static string IconsPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons\\");
        public static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }
        public static async void AnimateRotation(PictureBox picBox, Image originalImage, int totalDegrees, int steps, int delayMs)
        {
            for (int i = 1; i <= steps; i++)
            {
                float angle = totalDegrees * i / (float)steps;
                Image rotated = RotateImage(originalImage, angle);
                picBox.Image = rotated;
                await Task.Delay(delayMs);
            }
        }
        private static Image RotateImage(Image img, float angle)
        {
            Bitmap rotatedBmp = new Bitmap(img.Width, img.Height);
            rotatedBmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, new Point(0, 0));
            }

            return rotatedBmp;
        }
        public static async void AnimateSlideInFromTop(Panel panel, int steps = 10, int delayMs = 15)
        {
            int startY = -panel.Height;
            int endY = 0;
            int stepY = (endY - startY) / steps;
            panel.Location = new Point(0, startY);
            panel.Visible = true;
            for (int i = 0; i <= steps; i++)
            {
                int y = startY + i * stepY;
                panel.Location = new Point(0, y);
                await Task.Delay(delayMs);
            }

            panel.Location = new Point(0, 0);
        }
        public static async void AnimateSlideOutToTop(Panel panel, int steps = 10, int delayMs = 15)
        {
            int startY = panel.Location.Y;
            int endY = -panel.Height;
            int stepY = (endY - startY) / steps;

            for (int i = 0; i <= steps; i++)
            {
                int y = startY + i * stepY;
                panel.Location = new Point(0, y);
                await Task.Delay(delayMs);
            }
            panel.Visible = false;
        }
    }
}