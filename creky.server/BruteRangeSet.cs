using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace creky.server
{
    internal class BruteRangeSet
    {
        long keyStart;
        long keyRange;
        byte[] inputBytes;
        public byte[] outputBytes;
        public int width;

        public EventHandler finishedEvent;

        public BruteRangeSet(long keyStart, int keyRange, byte[] inputBytes) 
        { 
            this.keyStart = keyStart;
            this.keyRange = keyRange;
            this.inputBytes = inputBytes;

            // setup gpu

            //long[] keys = new long[keyRange];

            //width = new Speck().Decrypt(inputBytes, keyStart).Length;

            //outputBytes = gpu.Allocate<byte>(keyRange * width);
            //uint[] keyPrepArray = gpu.Allocate<uint>(23 * keyRange);

            //for (long i = keyStart; i < keyStart + keyRange; i++)
            //{
            //    keys[i] = i;
            //}

            //long[] dev_a = gpu.CopyToDevice(keys);
            //byte[] input = gpu.CopyToDevice(inputBytes);

            //gpu.Launch(1, keyRange).add(input, outputBytes, keyPrepArray, keyStart, keyRange, width);

            //// copy the array 'c' back from the GPU to the CPU
            //byte[] output = new byte[width * keyRange];

            //gpu.CopyFromDevice(outputBytes, output);

            //outputBytes = output;

            //gpu.FreeAll();



            finishedEvent.Invoke(this, new EventArgs());
        }

        public byte[] decrypt(byte[] input, long key)
        {
            uint[] keys = new uint[23];
            List<byte> bytes = new List<byte>();
            uint a = (uint)(key / 0x100000000);
            uint b = (uint)(key % 0x100000000);
            keys[0] = b;

            for (uint i = 0; i < 22; i++)
            {
                a = a << 3 | a >> 29;
                a += b;
                a = a ^ i;
                a = a << 8 | a >> 24;
                b = b ^ a;
                keys[i + 1] = b;
            }

            uint x = 0U;
            uint y = 0U;

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
                    x = x ^ keys[i];
                    x = (uint)((x - y) % 0x100000000);
                    x = x >> 3 | x << 29;
                }
                y = y ^ x;
                x = x >> 8 | x << 24;
                x = x ^ keys[0];
                x = (uint)((x - y) % 0x100000000);
                x = x >> 3 | x << 29;
                for (int i = 0; i < 8; i++)
                {
                    bytes.Add((byte)(x % 0x100));
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }
            return bytes.ToArray();
        }

        public static void trykey(Object thread, byte[] input, byte[] output, uint[] keys, long keyStart, int keyRange, int width)
        {
            int tid = 0;
            if (tid + keyStart < keyStart + keyRange)
            {
                long key = tid + keyStart;
                int keyIndex = 23 * tid;
                uint a = (uint)(key / 0x100000000);
                uint b = (uint)(key % 0x100000000);
                keys[keyIndex] = b;

                for (uint i = 0; i < 22; i++)
                {
                    a = a << 3 | a >> 29;
                    a += b;
                    a = a ^ i;
                    a = a << 8 | a >> 24;
                    b = b ^ a;
                    keys[keyIndex + i + 1] = b;
                }

                uint x = 0U;
                uint y = 0U;

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
                        x = x ^ keys[keyIndex + i];
                        x = (uint)((x - y) % 0x100000000);
                        x = x >> 3 | x << 29;
                    }
                    y = y ^ x;
                    x = x >> 8 | x << 24;
                    x = x ^ keys[keyIndex];
                    x = (uint)((x - y) % 0x100000000);
                    x = x >> 3 | x << 29;
                    for (int i = 0; i < 8; i++)
                    {
                        output[tid * width] = (byte)(x % 0x100);
                        x = y << 24 ^ x >> 8;
                        y = y >> 8;
                    }
                }
            }
        }
    }
}
