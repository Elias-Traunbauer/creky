using CrekyServer;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrekyClient
{
    internal class BruteRangeSet
    {
        TaskPacket task;
        int chunkSize = int.MaxValue - 1000;
        int chunkIndexNext = 0;
        Context context;
        int chunksPerDevice;
        int chunkCount;
        int lastChunkSize;
        int finished = 0;
        int deviceCount;
        bool found = false;
        List<string> results;
        Action<RequestPacket> request;

        public BruteRangeSet(TaskPacket newTask, Action<RequestPacket> result)
        {
            results = new List<string>();
            request = result;
            task = newTask;
        }

        public void run()
        {
            long keyCount = task.keyEnd - task.keyStart;

            chunkCount = (int)Math.Ceiling((decimal)keyCount / (decimal)chunkSize); // has to be int

            lastChunkSize = (int)(keyCount % chunkSize);

            context = Context.CreateDefault();
            deviceCount = 0;
            foreach (var device in context.Devices)
            {
                if (device.AcceleratorType != AcceleratorType.CPU)
                {
                    deviceCount++;
                }
            }
            chunksPerDevice = chunkCount / deviceCount;

            Console.WriteLine($"startIndex: {task.keyStart}, endIndex: {task.keyEnd}, keyCount: {keyCount}, chunkCount: {chunkCount}, lastChunkSize: {lastChunkSize}, deviceCount: {deviceCount}, chunksPerDevice: {chunksPerDevice}");

            foreach (var device in context.Devices)
            {
                if (device.AcceleratorType == AcceleratorType.CPU)
                {
                    continue;
                }

                Thread thread = new Thread(new ParameterizedThreadStart(ExecuteOnDevice));
                thread.IsBackground = true;
                thread.Start(device);
            }
        }

        void ExecuteOnDevice(Object device)
        {
            using var accelerator = ((Device)device).CreateAccelerator(context);
            Console.WriteLine($"Execution on device {device}");
            Console.WriteLine($"Input length: {task.input.Length}");

            var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, ArrayView<byte>, ArrayView<short>, long>(TryKeyKernel);

            var inputref = accelerator.Allocate1D<byte>(task.input.Length);

            for (int i = 0; i < chunksPerDevice; i++)
            {
                int chunkIndex = chunkIndexNext;
                chunkIndexNext++;

                int currentChunkSize = chunkSize;

                if (chunkIndex == chunkCount - 1)
                {
                    currentChunkSize = lastChunkSize != 0 ? lastChunkSize : chunkSize;
                }

                long chunkStart = task.keyStart + chunkSize * chunkIndex;

                var outputInfo = accelerator.Allocate1D<byte>(currentChunkSize);
                var foundIdentifier = accelerator.Allocate1D<short>(1);
                inputref.CopyFromCPU(task.input);
                outputInfo.MemSetToZero();
                foundIdentifier.MemSetToZero();

                Console.WriteLine($"[Chunk{chunkIndex}] Trying {currentChunkSize} keys from index {chunkStart}");

                double startTime = Environment.TickCount;

                kernel(currentChunkSize, (ArrayView<byte>)outputInfo, (ArrayView<byte>)inputref, (ArrayView<short>)foundIdentifier, chunkStart);

                if (foundIdentifier.GetAsArray1D()[0] == 1)
                {
                    Console.WriteLine("Found match");
                    found = true;
                    var info = outputInfo.GetAsArray1D();
                    for (int j = 0; j < info.Length; j++)
                    {
                        if (info[j] == 1)
                        {
                            long key = chunkStart + j;
                            char[] msg = trykey(task.input, key);
                            string text = "";
                            for (int k = 0; k < msg.Length; k++)
                            {
                                text += msg[k];
                            }
                            Console.WriteLine("Match string: " + text);
                            results.Add(text);
                        }
                    }
                }

                double duration = Environment.TickCount - startTime;
                double seconds = duration / 1000d;
                Console.WriteLine($"Chunk{chunkIndex} finished in {seconds}s");

                outputInfo.Dispose();
                foundIdentifier.Dispose();
            }
            finished++;
            if (finished == deviceCount)
            {
                RequestPacket packet = new RequestPacket();
                packet.foundMatch = found;
                packet.Id = task.Id;
                if (found)
                {
                    packet.results = results.ToArray();
                }
                request(packet);
            }
        }

        static void TryKeyKernel(Index1D index, ArrayView<byte> found, ArrayView<byte> input, ArrayView<short> foundIdentifier, long start)
        {
            long keyN = index + start;
            uint a = (uint)(keyN / 0x100000000);
            uint b = (uint)(keyN % 0x100000000);
            uint[] key = new uint[23];
            key[0] = b;

            for (uint i = 0; i < 22; i++)
            {
                a = a << 3 | a >> 29;
                a += b;
                a = a ^ i;
                a = a << 8 | a >> 24;
                b = b ^ a;
                key[i + 1] = b;
            }

            uint x = 0U;
            uint y = 0U;

            byte[] result = new byte[128];

            int resId = 0;
            for (int k = 0; k < input.Length / 8; k++)
            {
                for (int i = k * 8; i < k * 8 + 8; i++)
                {
                    x = y << 24 | x >> 8;
                    y = (uint)input[i] << 24 | y >> 8;
                }
                for (int i = 22; i >= 1; i--)
                {
                    y = y ^ x;
                    x = x >> 8 | x << 24;
                    x = x ^ key[i];
                    x = (uint)((x - y) % 0x100000000);
                    x = x >> 3 | x << 29;
                }
                y = y ^ x;
                x = x >> 8 | x << 24;
                x = x ^ key[0];
                x = (uint)((x - y) % 0x100000000);
                x = x >> 3 | x << 29;
                for (int i = 0; i < 8; i++)
                {
                    result[resId] = (byte)(x % 0x100);
                    resId++;
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }
            int val = 0;
            for (int i = 0; i < resId; i++)
            {
                if (result[i] >= 97 && result[i] <= 122)
                {
                    val++;
                }
                else if (result[i] == 32)
                {
                    val += 3;
                }
            }
            if (val > resId)
            {
                found[index] = 1;
                foundIdentifier[0] = 1;
            }
        }

        public char[] trykey(byte[] msg, long key)
        {
            uint[] keyP = PrepareKey(key);

            char[] res = new char[msg.Length * 10];

            int resId = 0;

            uint x = 0U;
            uint y = 0U;

            for (int k = 0; k < msg.Length / 8; k++)
            {
                for (int i = k * 8; i < k * 8 + 8; i++)
                {
                    x = y << 24 | x >> 8;
                    y = (uint)msg[i] << 24 | y >> 8;
                }
                sp_blockrev(ref x, ref y, keyP);
                for (int i = 0; i < 8; i++)
                {
                    res[resId] = (char)(x % 0x100);
                    resId++;
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }

            char[] realRes = new char[resId];
            for (int i = 0; i < resId; i++)
            {
                realRes[i] = res[i];
            }

            return realRes;
        }

        private uint[] PrepareKey(long key)
        {
            uint[] keyRes = new uint[23];
            uint a = (uint)(key / 0x100000000);
            uint b = (uint)(key % 0x100000000);
            keyRes[0] = b;
            for (uint i = 0; i < 22; i++)
            {
                rot(i, ref a, ref b);
                keyRes[i + 1] = b;
            }
            return keyRes;
        }

        private void sp_blockrev(ref uint x, ref uint y, uint[] key)
        {
            for (int i = 22; i >= 1; i--)
            {
                rotrev(key[i], ref x, ref y);
            }
            rotrev(key[0], ref x, ref y);
        }

        private void rot(uint i, ref uint x, ref uint y)
        {
            x = x << 3 | x >> 29;
            x += y;
            x = x ^ i;
            x = x << 8 | x >> 24;
            y = y ^ x;
        }

        private void rotrev(uint i, ref uint x, ref uint y)
        {
            y = y ^ x;
            x = x >> 8 | x << 24;
            x = x ^ i;
            x = (uint)((x - y) % 0x100000000);
            x = x >> 3 | x << 29;
        }
    }
}
