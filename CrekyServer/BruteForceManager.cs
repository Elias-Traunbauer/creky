using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrekyServer
{
    internal class BruteForceManager
    {
        public static byte[] input = new byte[] { 192, 253, 254, 121, 108, 155, 118, 170, 155, 152, 106, 191, 207, 145, 206, 162, 65, 231, 185, 62, 171, 54, 146, 147, 249, 45, 24, 91, 114, 114, 175, 5, 17, 202, 184, 36, 86, 78, 210, 131, 71, 28, 71, 74, 8, 147, 221, 148, 39, 117, 92, 147, 180, 203, 178, 147, 22, 84, 137, 109, 136, 209, 111, 137 }; // test msg
        public const long keyRangeStart = 1007199254740992;
        public const long keyRangeEnd = 9007199254740992;
        public const int workPacketSize = 10;
        public const int keyRange = int.MaxValue - 1000;
        public static string saveFile = "bruteforce";
        public bool[] reserved;
        public bool[] status;

        public BruteForceManager()
        {
            if (!File.Exists(saveFile))
            {
                CreateSaveFile();
            }
            else
            {
                LoadSaveFile();
            }
        }

        public void LoadSaveFile()
        {
            byte[] inputBytes = File.ReadAllBytes(saveFile);
            status = new bool[inputBytes.Length];
            reserved = new bool[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++)
            {
                status[i] = inputBytes[i] == 1;
                reserved[i] = inputBytes[i] == 1;
            }
        }

        public void ApplySaveFile()
        {
            status = new bool[(keyRangeEnd - keyRangeStart) / ((long)keyRange * workPacketSize)];
            reserved = new bool[(keyRangeEnd - keyRangeStart) / ((long)keyRange * workPacketSize)];
            byte[] outputBytes = new byte[status.Length];
            for (int i = 0; i < outputBytes.Length; i++)
            {
                outputBytes[i] = status[i] ? (byte)1 : (byte)0;
            }
            File.WriteAllBytes(saveFile, outputBytes);
        }

        public void CreateSaveFile()
        {
            status = new bool[(keyRangeEnd - keyRangeStart) / ((long)keyRange * workPacketSize)];
            reserved = new bool[(keyRangeEnd - keyRangeStart) / ((long)keyRange * workPacketSize)];
            byte[] outputBytes = new byte[status.Length];
            File.WriteAllBytes(saveFile, outputBytes);
        }

        private static BruteForceManager? _instance;

        public static BruteForceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BruteForceManager();
                }
                return _instance;
            }
        }

        public void FinishedPacket(RequestPacket packet)
        {
            status[packet.Id] = true;
            Console.WriteLine($"Finished packet [{packet.Id}]");
            ApplySaveFile();
            if (packet.foundMatch)
            {
                Console.WriteLine("Found match");
                for (int i = 0; i < packet.results.Length; i++)
                {
                    Console.WriteLine(packet.results[i]);
                }
            }
        }

        public TaskPacket? GetWorkPacket()
        {
            Random rnd = new Random();
            int index;
            if (reserved.Contains(false))
            {
                while (reserved[index = rnd.Next(0, status.Length)])
                {
                }
                reserved[index] = true;
                TaskPacket packet = new TaskPacket();
                packet.input = input;
                packet.Id = index;
                packet.keyStart = index * (long)keyRange * workPacketSize;
                packet.keyEnd = packet.keyStart + (long)keyRange * workPacketSize;
                return packet;
            }

            return null;
        }
    }
}
