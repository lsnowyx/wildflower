namespace wildflower
{
    public static class Helper
    {
        public static float CurrentAngle { get; set; } = 0;
        public static bool IsAnimatingButton { get; private set; } = false;
        public static bool IsAnimatingPanel { get; private set; } = false;
        public static string IconsPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons\\");
        public static Image LoadIconImage(string fileName, int width, int height)
        {
            using Image source = Image.FromFile(Path.Combine(IconsPath, fileName));
            return ResizeImage(source, width, height);
        }

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
        public static async void AnimateRotation(PictureBox picBox, Image baseImage, float deltaAngle, int steps, int delayMs)
        {
            if (IsAnimatingButton) return;
            IsAnimatingButton = true;

            float startAngle = CurrentAngle;
            float targetAngle = CurrentAngle + deltaAngle;

            for (int i = 1; i <= steps; i++)
            {
                float angle = startAngle + (deltaAngle * i / steps);
                Image rotated = RotateImage(baseImage, angle);
                Image? previous = picBox.Image;
                picBox.Image = rotated;
                if (previous != null && !ReferenceEquals(previous, baseImage))
                    previous.Dispose();

                await Task.Delay(delayMs);
            }
            CurrentAngle = targetAngle % 360;
            IsAnimatingButton = false;
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
            if (IsAnimatingPanel) return;
            IsAnimatingPanel = true;

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
            IsAnimatingPanel = false;
        }
        public static async void AnimateSlideOutToTop(Panel panel, int steps = 10, int delayMs = 15)
        {
            if (IsAnimatingPanel) return;
            IsAnimatingPanel = true;
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
            IsAnimatingPanel = false;
        }
        public static bool IsItemClipped(ListBox track_list)
        {
            if (track_list.SelectedItem == null)
                return false;

            string itemText = track_list.SelectedItem?.ToString() ?? string.Empty;
            using (Graphics g = track_list.CreateGraphics())
            {
                Size textSize = TextRenderer.MeasureText(g, itemText, track_list.Font);
                int itemWidth = track_list.Width - SystemInformation.VerticalScrollBarWidth / 2;
                return textSize.Width > itemWidth;
            }
        }
    }
}
