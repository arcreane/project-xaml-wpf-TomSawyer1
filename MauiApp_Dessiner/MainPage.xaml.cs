using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace MauiApp_Dessiner
{
    public partial class MainPage : ContentPage
    {
        private List<PointF> points = new List<PointF>();

        public MainPage()
        {
            InitializeComponent();
            drawingView.Drawable = new CustomDrawable(points);
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            points.Clear();
            drawingView.Invalidate();
        }

        private void OnStartInteraction(object sender, TouchEventArgs e)
        {
            HandleTouch(e);
        }

        private void OnDragInteraction(object sender, TouchEventArgs e)
        {
            HandleTouch(e);
        }

        private void OnEndInteraction(object sender, TouchEventArgs e)
        {
            HandleTouch(e);
        }

        private void HandleTouch(TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                points.Add(new PointF((float)touch.X, (float)touch.Y));
            }
            drawingView.Invalidate();
        }
    }

    public class CustomDrawable : IDrawable
    {
        private readonly List<PointF> points;

        public CustomDrawable(List<PointF> points)
        {
            this.points = points;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;

            for (int i = 1; i < points.Count; i++)
            {
                canvas.DrawLine(points[i - 1], points[i]);
            }
        }
    }
}
