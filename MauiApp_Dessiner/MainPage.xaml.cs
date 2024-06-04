using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp_Dessiner
{
    public partial class MainPage : ContentPage
    {
        // Liste pour stocker les points du dessin
        private List<PointF> points = new List<PointF>();

        // Liste de mots à dessiner
        private List<string> words = new List<string> { "maison", "chat", "chien", "arbre", "voiture" };
        private Random random = new Random();

        // Couleur et taille du pinceau par défaut
        private Color selectedColor = Colors.Black;
        private float brushSize = 2;

        // Connexion au hub SignalR
        private HubConnection hubConnection;

        // Liste pour stocker les réponses
        public List<string> Responses { get; set; } = new List<string>();

        public MainPage()
        {
            InitializeComponent();
            // Initialiser la vue de dessin avec un objet Drawable personnalisé
            drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);

            // Gérer le changement de taille du pinceau
            brushSizeSlider.ValueChanged += OnBrushSizeChanged;

            // Lier la liste de réponses à la source de données de la ListView
            responsesListView.ItemsSource = Responses;

            // Configurer SignalR
            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://your-server-url/drawingHub")
                .Build();

            // Définir les méthodes pour recevoir des données du hub SignalR
            hubConnection.On<List<PointF>, string, float>("ReceiveDrawing", (receivedPoints, color, size) =>
{
    MainThread.BeginInvokeOnMainThread(() =>
    {
        // Mettre à jour la couleur et la taille du pinceau
        selectedColor = Color.FromArgb(color);
        brushSize = size;
        // Ajouter les points reçus au dessin
        points.AddRange(receivedPoints);
        drawingView.Invalidate();
    });
});

hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
{
    MainThread.BeginInvokeOnMainThread(() =>
    {
        // Gérer les messages reçus
        Responses.Add($"{user}: {message}");
        responsesListView.ItemsSource = null;  // Rafraîchir la ListView
        responsesListView.ItemsSource = Responses;
    });
});

            // Connexion au hub SignalR
            ConnectToHub();
        }

        // Méthode asynchrone pour démarrer la connexion au hub
        private async Task ConnectToHub()
        {
            await hubConnection.StartAsync();
        }

        // Gérer le clic sur le bouton pour effacer le dessin
        private void OnClearClicked(object sender, EventArgs e)
        {
            points.Clear();
            drawingView.Invalidate();
        }

        // Gérer le clic sur le bouton pour choisir un mot à dessiner
        private void OnChooseWordClicked(object sender, EventArgs e)
        {
            string word = words[random.Next(words.Count)];
            wordLabel.Text = $"Mot à dessiner : {word}";
        }

        // Gérer le clic sur un bouton de couleur pour changer la couleur du pinceau
       private void OnColorButtonClicked(object sender, EventArgs e)
{
    if (sender is Button button && button.BackgroundColor != null)
    {
        selectedColor = button.BackgroundColor;
        drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);
        drawingView.Invalidate();
    }
}


        // Gérer le changement de taille du pinceau
        private void OnBrushSizeChanged(object sender, ValueChangedEventArgs e)
        {
            brushSize = (float)e.NewValue;
            drawingView.Drawable = new CustomDrawable(points, selectedColor, brushSize);
            drawingView.Invalidate();
        }

        // Gérer les interactions de début, de glissement et de fin du dessin
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
            // Envoyer les données du dessin au serveur via SignalR
            hubConnection.SendAsync("SendDrawing", points, selectedColor.ToArgbHex(), brushSize);
        }

        // Méthode pour gérer les points de contact sur l'écran
        private void HandleTouch(TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                points.Add(new PointF((float)touch.X, (float)touch.Y));
            }
            drawingView.Invalidate();
        }

        // Gérer le clic sur le bouton pour envoyer une réponse
        private void OnSendResponseClicked(object sender, EventArgs e)
        {
            string response = responseEntry.Text;
            // Ajouter la réponse à la liste de réponses
            Responses.Add(response);
            // Effacer le champ de saisie
            responseEntry.Text = string.Empty;
        }
    }

    // Classe pour dessiner le contenu personnalisé
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

        // Méthode de dessin
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
