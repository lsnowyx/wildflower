namespace wildflower
{
    public static class Helper
    {
        public static float CurrentAngle { get; set; } = 0;
        public static bool IsAnimatingButton { get; private set; } = false;
        public static bool IsAnimatingPanel { get; private set; } = false;
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
                picBox.Image = rotated;

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
        public static async Task TrackListAdd(string[] paths, ListBox track_list, bool clearList = true)
        {
            if (paths == null || paths.Length == 0)
            {
                track_list.Items.Clear();
                return;
            }
            if (clearList) track_list.Items.Clear();
            int iteration = 0;
            var chunks = paths
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / 25)
                .Select(g => g.Select(x => x.item).ToArray())
                .ToList();
            var tasks = chunks.Select(chunk =>
            {
                int order = iteration++;
                return Task.Run(() =>
                {
                    var metadata = GetMetadataFromArray(chunk);
                    return (order, metadata);
                });
            }).ToList();
            var results = await Task.WhenAll(tasks);
            foreach (var (_, metadata) in results.OrderBy(r => r.order))
            {
                if (track_list.InvokeRequired)
                {
                    track_list.Invoke(() =>
                    {
                        foreach (var item in metadata)
                            track_list.Items.Add(item.title + item.artist);
                    });
                }
                else
                {
                    foreach (var item in metadata)
                        track_list.Items.Add(item.title + item.artist);
                }
            }
        }
        private static List<(string title, string artist)> GetMetadataFromArray(string[] paths)
        {
            var results = new List<(string title, string artist)>();
            foreach (var path in paths)
                results.Add(GetMetadataFromFile(path));
            return results;
        }
        public static (string title, string artist) GetMetadataFromFile(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                string title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
                string artist = string.Empty;
                if (file.Tag.FirstPerformer != null)
                {
                    artist += $" - {file.Tag.FirstPerformer}";
                }
                return (title, artist);
            }
            catch (Exception)
            {
                return (Path.GetFileNameWithoutExtension(filePath), string.Empty);
            }
        }
        public static bool IsItemClipped(ListBox track_list)
        {
            if (track_list.SelectedItem == null)
                return false;

            string itemText = track_list.SelectedItem.ToString();
            using (Graphics g = track_list.CreateGraphics())
            {
                Size textSize = TextRenderer.MeasureText(g, itemText, track_list.Font);
                int itemWidth = track_list.Width - SystemInformation.VerticalScrollBarWidth / 2;
                return textSize.Width > itemWidth;
            }
        }
    }
}