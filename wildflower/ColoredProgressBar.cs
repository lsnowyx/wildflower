namespace wildflower
{
    public class ColoredProgressBar : ProgressBar
    {
        public ColoredProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(Color.Black))
            {
                g.FillRectangle(bg, rect);
            }
            int fillWidth = (int)((float)this.Value / this.Maximum * this.Width);
            if (fillWidth > 0)
            {
                Color myColor = ColorTranslator.FromHtml("#00FFFF");
                using (SolidBrush brush = new SolidBrush(myColor))
                {
                    g.FillRectangle(brush, 0, 0, fillWidth, Height);
                }
            }
        }
    }
}
