namespace wildflower
{
    public class ColoredProgressBar : ProgressBar
    {
        public ColoredProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;
            rect.Width = (int)((float)Value / Maximum * Width);
            if (rect.Width > 0)
            {
                Color myColor = ColorTranslator.FromHtml("#00FFFF");
                using (SolidBrush brush = new SolidBrush(myColor))
                {
                    g.FillRectangle(brush, 0, 0, rect.Width, Height);
                }
            }
        }
    }
}