using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace CrekyServer
{
    internal class ClientHandler
    {
        Socket socket;

        public ClientHandler(Socket clientSocket)
        {
            socket = clientSocket;
        }

        public void run()
        {
            NetworkStream stream = new NetworkStream(socket);
            BinaryFormatter objectStream = new BinaryFormatter();
            while (true)
            {
                try
                {
                    var taskPacket = BruteForceManager.Instance.GetWorkPacket();
                    if (taskPacket != null)
                    {
                        Console.WriteLine("Sending work-packet to client");
                        objectStream.Serialize(stream, taskPacket);
                    }
                    else
                    {
                        Console.WriteLine("Couldnt get new Task packet");
                        return;
                    }

                    RequestPacket packet = (RequestPacket)objectStream.Deserialize(stream);
                    BruteForceManager.Instance.FinishedPacket(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured in client thread: " + ex.Message + " " + ex.StackTrace);
                    return;
                }
            }
        }
    }
}
