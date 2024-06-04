using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp_Dessiner
{
    public partial class MainPage : ContentPage
    {
        private List<PointF> points = new List<PointF>();
        private List<string> words = new List<string> { "maison", "chat", "chien", "arbre", "voiture" };
        private Random random = new Random();
        private Color selectedColor = Colors.Black;
        private float brushSize = 2;
        private HubConnection hubConnection;

        // Déclarez une liste pour stocker les réponses
        public List<string> Responses { get; set; } = new List<string>();

        public MainPage()
        {
            InitializeComponent();
            drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);
            brushSizeSlider.ValueChanged += OnBrushSizeChanged;
            responsesListView.ItemsSource = Responses; // Lier la liste de réponses à la source de données de la ListView

            // Configure SignalR
            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://your-server-url/drawingHub")
                .Build();

            hubConnection.On<List<PointF>, string, float>("ReceiveDrawing", (receivedPoints, color, size) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    selectedColor = Color.FromArgb(color);
                    brushSize = size;
                    points.AddRange(receivedPoints);
                    drawingView.Invalidate();
                });
            });

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Handle received message
                });
            });

            ConnectToHub();
        }

        private async Task ConnectToHub()
        {
            await hubConnection.StartAsync();
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            points.Clear();
            drawingView.Invalidate();
        }

        private void OnChooseWordClicked(object sender, EventArgs e)
        {
            string word = words[random.Next(words.Count)];
            wordLabel.Text = $"Mot à dessiner : {word}";
        }

        private void OnColorButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                selectedColor = button.BackgroundColor;
                drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);
                drawingView.Invalidate();
            }
        }

        private void OnBrushSizeChanged(object sender, ValueChangedEventArgs e)
        {
            brushSize = (float)e.NewValue;
            drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);
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
            // Send drawing data to the server
            hubConnection.SendAsync("SendDrawing", points, selectedColor.ToArgbHex(), brushSize);
        }

        private void HandleTouch(TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                points.Add(new PointF((float)touch.X, (float)touch.Y));
            }
            drawingView.Invalidate();
        }

       private void OnSendResponseClicked(object sender, EventArgs e)
        {
            string response = responseEntry.Text;
            // Ajouter la réponse à la liste de réponses
            Responses.Add(response);
            // Effacer le champ de saisie
            responseEntry.Text = string.Empty;
        }
    }

    public class CustomDrawable : IDrawable
    {
        private readonly List<PointF> points;
        private readonly Color color;
        private readonly float brushSize;

        public CustomDrawable(List<PointF> points, Color color, float brushSize)
        {
            this.points = points;
            this.color = color;
            this.brushSize = brushSize;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = color;
            canvas.StrokeSize = brushSize;

            for (int i = 1; i < points.Count; i++)
            {
                canvas.DrawLine(points[i - 1], points[i]);
            }
        }
    }
}
