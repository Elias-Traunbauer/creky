using System.Net;
using System.Net.Sockets;

namespace CrekyServer
{
    internal class Program
    {
        static Socket socket;

        static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 63636));
            socket.Listen(100);
            while (true)
            {
                Socket newClient = socket.Accept();
                Console.WriteLine("New client");
                ClientHandler handler = new ClientHandler(newClient);
                Thread clientThread = new Thread(handler.run);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
    }
}