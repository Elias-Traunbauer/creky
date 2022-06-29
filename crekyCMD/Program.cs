using System;
using ILGPU;
using ILGPU.Runtime;

namespace creky.server
{
    internal class Program
    {
        static int chunkSize = int.MaxValue - 1000;

        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage");
                Console.WriteLine("Argument 1: results directory path");
                Console.WriteLine("Argument 2: input string file path");
                Console.WriteLine("Argument 3: start index");
                Console.WriteLine("Argument 4: end index");
                Console.WriteLine("Argument 5 (optional): test? [true|false]");
                return;
            }
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
            bool test = args[4] == "true";

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
                        continue;
                    }
                    using var accelerator = device.CreateAccelerator(context);
                    Console.WriteLine($"Test for Device {device.ToString()} Multiprocessors: " + device.NumMultiprocessors);
                    Console.WriteLine($"Testing {chunkSize} iterations");
                    Console.WriteLine($"Input length: {input.Length}");

                    var inputref = accelerator.Allocate1D<byte>(input.Length);
                    var outputInfo = accelerator.Allocate1D<byte>(chunkSize);
                    var foundIdentifier = accelerator.Allocate1D<short>(1);
                    inputref.CopyFromCPU(input);
                    outputInfo.MemSetToZero();
                    foundIdentifier.MemSetToZero();

                    var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, ArrayView<byte>, ArrayView<short>, long>(TryKeyKernel);

                    long chunkStart = startIndex;

                    int currentChunkSize = chunkSize;

                    double startTime = Environment.TickCount;

                    kernel(currentChunkSize, (ArrayView<byte>)outputInfo, (ArrayView<byte>)inputref, (ArrayView<short>)foundIdentifier, chunkStart);

                    foundIdentifier.GetAsArray1D();

                    double duration = Environment.TickCount - startTime;
                    Console.WriteLine($"Start time: {startTime} - End time: {Environment.TickCount}");
                    double seconds = duration / 1000d;
                    long multiplier = (long)(Math.Pow(2, 53) / currentChunkSize);
                    double secondsTotal = seconds * multiplier;
                    double hoursTotal = secondsTotal / 3600d;
                    double daysTotal = hoursTotal / 24;
                    Console.WriteLine($"{currentChunkSize} iterations took {seconds}s -> this has to be done {multiplier}x to try all keys");
                    Console.WriteLine($"Results: Total estimated (worst case) time: " + (int)daysTotal + "d");
                }
            }
            else
            {

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
    }
}