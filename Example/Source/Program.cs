using WebWind.Core;

public class Program
{
    public static void Main(string[] args)
    {
        WebSocketServer server = new WebSocketServer();
        server.Host(8080);
    }
}