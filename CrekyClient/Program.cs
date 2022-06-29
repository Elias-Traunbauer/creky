using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using CrekyServer;

namespace CrekyClient
{
    internal class Program
    {
        static Socket socket;
        static bool waiting = false;
        static RequestPacket packetR;

        static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Loopback, 63636));

            NetworkStream stream = new NetworkStream(socket);
            BinaryFormatter objectStream = new BinaryFormatter();

            while (true)
            {
                try
                {
                    TaskPacket packet = (TaskPacket)objectStream.Deserialize(stream);

                    BruteRangeSet set = new BruteRangeSet(packet, (packet) =>
                    {
                        waiting = false;
                        packetR = packet;
                    });
                    waiting = true;

                    Console.WriteLine("Starting execution");

                    set.run();

                    while (waiting)
                    {
                        Thread.Sleep(100);
                    }

                    objectStream.Serialize(stream, packetR);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Exception occured in client thread: " + ex.StackTrace);
                    return;
                }
            }
        }
    }
}