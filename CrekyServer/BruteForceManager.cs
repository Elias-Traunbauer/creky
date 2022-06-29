using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrekyServer
{
    internal class BruteForceManager
    {
        public static byte[] input = new byte[] { 143, 175, 181, 116, 217, 0, 174, 94, 226, 184, 167, 49, 30, 41, 203, 116, 82, 113, 237, 56 ,254, 212, 62, 90, 64 ,104, 255, 100, 77 ,170 ,125 ,165 ,85 ,248, 158, 169, 178, 179, 187 ,95 ,210, 37, 77, 113 ,22 ,62 ,156 ,142 ,106 ,204 ,62 ,95 ,238, 205, 184, 30 ,144 ,35 ,60 ,27 ,145, 174, 168, 227 }; // test msg
        public const long keyRangeStart = 1007199254740992;
        public const long keyRangeEnd = 9007199254740992;
        public const int workPacketSize = 10;
        public const int keyRange = int.MaxValue - 1000;
        public static string saveFile = "bruteforce";
        public static string matchFile = "results";
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
                    Console.WriteLine(packet.results[i] + " key: " + packet.resultKeys[i]);

                    File.AppendAllLines(matchFile, new string[] { packet.results[i] + " key: " + packet.resultKeys[i] });
                }
            }
            long cnt = 0;
            for (int i = 0; i < status.Length; i++)
            {
                if (status[i])
                {
                    cnt++;
                }
            }
            Console.WriteLine($"Progress: {((decimal)cnt / ((decimal)keyRangeEnd - (decimal)keyRangeStart)) * 100}%");
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
