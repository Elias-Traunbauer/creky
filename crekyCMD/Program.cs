using System;
using ILGPU;
using ILGPU.Runtime;

namespace creky.server
{
    internal class Program
    {
        static int chunkSize = int.MaxValue;

        static void Main(string[] args)
        {
            // args 0 = resDirPath
            // args 1 = inputFile
            // args 2 = startIndex
            // args 3 = endIndex
            // args 4 = test?

            string resDirPath = args[0];
            string inputText = File.ReadAllText(args[1]);
            var split = inputText.Split(" ");
            byte[] input = new byte[split.Length];
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = byte.Parse(split[i]);
            }
            long startIndex = long.Parse(args[2]);
            long endIndex = long.Parse(args[3]);
            bool test = args[4] == "test";

            long keyCount = endIndex - startIndex;

            int chunkCount = (int)Math.Ceiling((decimal)keyCount / (decimal)chunkSize); // has to be int

            int lastChunkSize = (int)(keyCount % chunkSize);

            if (test)
            {
                using var context = Context.CreateDefault();
                int deviceCount = 0;
                foreach (var device in context.Devices)
                {
                    if (device.AcceleratorType != AcceleratorType.CPU)
                    {
                        deviceCount++;
                    }
                }
                int chunksPerDevice = chunkCount / deviceCount;

                Console.WriteLine($"startIndex: {startIndex}, endIndex: {endIndex}, keyCount: {keyCount}, chunkCount: {chunkCount}, lastChunkSize: {lastChunkSize}, deviceCount: {deviceCount}, chunksPerDevice: {chunksPerDevice}");

                foreach (var device in context.Devices)
                {
                    if (device.AcceleratorType == AcceleratorType.CPU)
                    {
                        return;
                    }
                    using var accelerator = device.CreateAccelerator(context);
                    Console.WriteLine("Test for Device [" + device.Name + "] Multiprocessors: " + device.NumMultiprocessors);
                    Console.WriteLine($"Testing {chunkSize} iterations");

                    var inputref = accelerator.Allocate1D<byte>(input.Length);
                    var outputInfo = accelerator.Allocate1D<byte>(chunkSize);
                    inputref.CopyFromCPU(input);
                    outputInfo.MemSetToZero();

                    var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, ArrayView<byte>, long>(TryKeyKernel);

                    long chunkStart = startIndex;

                    int currentChunkSize = chunkSize;

                    int startTime = Environment.TickCount;

                    kernel(currentChunkSize, (ArrayView<byte>)outputInfo, (ArrayView<byte>)inputref, chunkStart);

                    int duration = Environment.TickCount - startTime;
                    float seconds = duration / 1000f;
                    

                    Console.WriteLine("");
                }
            }
            else
            {

            }
        }
        static void TryKeyKernel(Index1D index, ArrayView<byte> found, ArrayView<byte> input, long start)
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
            }
        }
    }
}