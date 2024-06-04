using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRServer.Hubs
{
    public class DrawingHub : Hub
    {
        public async Task SendDrawing(List<PointF> points, string color, float brushSize)
        {
            await Clients.Others.SendAsync("ReceiveDrawing", points, color, brushSize);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }

    public class PointF
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}
