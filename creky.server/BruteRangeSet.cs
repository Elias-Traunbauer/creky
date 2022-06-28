using System;
using System.Collections.Generic;
using OpenCL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using ILGPU;
using ILGPU.Runtime;

namespace creky.server
{
    internal class BruteRangeSet
    {
        public byte[] outputBytes;
        public byte[] outputInfo;
        public int width;

        public BruteRangeSet(long keyStart, int keyRange, byte[] inputBytes)
        {
            Speck speck = new Speck();
            byte[] bytesAsInts = Array.ConvertAll(inputBytes, c => (byte)c);
            width = speck.Decrypt(bytesAsInts, keyStart).Length * 2;

            var context = Context.CreateDefault();
            try
            {
                foreach (Device device in context.Devices)
                {
                    if (device.AcceleratorType == AcceleratorType.Cuda | device.AcceleratorType == AcceleratorType.OpenCL)
                    {
                        var accelerator = device.CreateAccelerator(context);

                        var inputref = accelerator.Allocate1D<byte>(inputBytes.Length);
                        var outputInfo = accelerator.Allocate1D<byte>(keyRange);
                        inputref.CopyFromCPU(inputBytes);
                        outputInfo.MemSetToZero();

                        var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, ArrayView<byte>, long, int>(TryKeyKernel);

                        kernel(keyRange, (ArrayView<byte>)outputInfo, (ArrayView<byte>)inputref, keyStart, width);

                        this.outputInfo = outputInfo.GetAsArray1D();

                        accelerator.Dispose();
                        break;
                    }
                }
                context.Dispose();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                context.Dispose();
            }

            // EasyCL cl = new EasyCL();
            // cl.Accelerator = AcceleratorDevice.GPU;
            // cl.LoadKernel(Trykey);
            // cl.Invoke("trykey", 0, keyRange, input, keyStart, result, width, cache, inputBytes.Length);
            //outputBytes = result;

            //int[] Primes = Enumerable.Range(2, 1000).ToArray();
            //EasyCL cl = new EasyCL();
            //cl.Accelerator = AcceleratorDevice.CPU;        //You can also set the accelerator after loading the kernel
            //cl.LoadKernel(IsPrime);                  //Load kernel string here, (Compiles in the background)
            //cl.Invoke("GetIfPrime", 0, Primes.Length, Primes); //Call Function By Name With Parameters
        }

        static void TryKeyKernel(Index1D index, ArrayView<byte> found, ArrayView<byte> input, long start, int res_width)
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

        public string Trykey
        {
            get
            {
                return @"kernel void trykey(__global int* msg, long start, __global int* res, int width, __global unsigned int* cache, int n)
                    {
                    int index = get_global_id(0);
                    printf(""%d\n"", index);
                    int keyIndex = index * 23;
                    long key = index + start;
                    long a = key / 4294967296;
                    long b = key % 4294967296;
                    cache[keyIndex] = b;

                    for (long i = 0; i < 22; i++)
                    {
                        a = (a << 3) | (a >> 29);
                        a += b;
                        a = a ^ i;
                        a = (a << 8) | (a >> 24);
                        b = b ^ a;

                        
                        cache[i + 1 + keyIndex] = b;
                    }

                    int resStart = index * width + 1;
                    long x = 0;
                    long y = 0;

                    for (int k = 0; k < n / 8; k++)
                    {
                        for (int i = k * 8; i < k * 8 + 8; i++)
                        {
                            x = (y << 24) | (x >> 8);
                            y = (msg[i] << 24) | (y >> 8);
                        }
                        for (int g = 22; g >= 1; g--)
                        {
                            y = y ^ x;
                            x = (x >> 8) | (x << 24);
                            x = x ^ cache[g + keyIndex];
                            x = (x - y) % 4294967296;
                            x = (x >> 3) | (x << 29);
                        }
                        y = y ^ x;
                        x = (x >> 8) | (x << 24);
                        x = x ^ cache[keyIndex];
                        x = (x - y) % 4294967296;
                        x = (x >> 3) | (x << 29);
                        for (int i = 0; i < 8; i++)
                        {

                            res[resStart] = x % 256;
                        printf(""%d\n"", res[resStart]);

                            resStart++;
                            x = (y << 24) ^ (x >> 8);
                            y = y >> 8;
                        }
                    }
                    res[index * width] = resStart - index * width - 1;
                    return;
                }
                ";
            }
        }

        static string dn = @"int resStart = index * width;
                    res[index] = index;
                    int keyIndex = index * 23;
                    long key = index + start;
                    long a = key / 4294967296;
                    long b = key % 4294967296;
                    cache[keyIndex] = b;

                    for (long i = 0; i < 22; i++)
                    {
                        a = (a << 3) | (a >> 29);
                        a += b;
                        a = a ^ i;
                        a = (a << 8) | (a >> 24);
                        b = b ^ a;

                        
                        cache[i + 1 + keyIndex] = b;
                    }

                    
                    long x = 0;
                    long y = 0;

                    for (int k = 0; k < n / 8; k++)
                    {
                        for (int i = k * 8; i < k * 8 + 8; i++)
                        {
                            x = (y << 24) | (x >> 8);
                            y = (msg[i] << 24) | (y >> 8);
                        }
                        for (int g = 22; g >= 1; g--)
                        {
                            y = y ^ x;
                            x = (x >> 8) | (x << 24);
                            x = x ^ cache[g + keyIndex];
                            x = (x - y) % 4294967296;
                            x = (x >> 3) | (x << 29);
                        }
                        y = y ^ x;
                        x = (x >> 8) | (x << 24);
                        x = x ^ cache[keyIndex];
                        x = (x - y) % 4294967296;
                        x = (x >> 3) | (x << 29);
                        for (int i = 0; i < 8; i++)
                        {

                            res[resStart] = x % 256;
                        printf(""%d\n"", res[resStart]);

                            resStart++;
                            x = (y << 24) ^ (x >> 8);
                            y = y >> 8;
                        }
                    }";

        static string IsPrime
        {
            get
            {
                return @"
        kernel void GetIfPrime(__global int* message)
        {
            int index = get_global_id(0);
                printf("" %d \n"",index);
            int upperl=(int)sqrt((float)message[index]);
            for(int i=2;i<=upperl;i++)
            {
                if(message[index]%i==0)
                {
                    //printf("" %d / %d\n"",index,i );
                    message[index]=0;
                    return;
                }
            }
            
        }";
            }
        }

        //static void trykey(byte* msg, long start, byte* res, int width, uint* cache)
        //{
        //    long key = get_global_id(0) + start;
        //    printf(key);
        //    uint a = (uint)(key / 0x100000000);
        //    uint b = (uint)(key % 0x100000000);
        //    cache[0] = b;
        //    for (uint i = 0; i < 22; i++)
        //    {
        //        a = a << 3 | a >> 29;
        //        a += b;
        //        a = a ^ i;
        //        a = a << 8 | a >> 24;
        //        b = b ^ a;

        //        cache[i + 1] = b;
        //    }

        //    int resStart = get_global_id(0) * width;

        //    uint x = 0U;
        //    uint y = 0U;

        //    for (int k = 0; k < msg.Length / 8; k++)
        //    {
        //        for (int i = k * 8; i < k * 8 + 8; i++)
        //        {
        //            x = y << 24 | x >> 8;
        //            y = (uint)msg[i] << 24 | y >> 8;
        //        }
        //        for (int g = 22; g >= 1; g--)
        //        {
        //            y = y ^ x;
        //            x = x >> 8 | x << 24;
        //            x = x ^ keyP[g];
        //            x = (uint)((x - y) % 0x100000000);
        //            x = x >> 3 | x << 29;
        //        }
        //        y = y ^ x;
        //        x = x >> 8 | x << 24;
        //        x = x ^ keyP[0];
        //        x = (uint)((x - y) % 0x100000000);
        //        x = x >> 3 | x << 29;
        //        for (int i = 0; i < 8; i++)
        //        {
        //            res[resStart] = (byte)(x % 0x100);
        //            resStart++;
        //            x = y << 24 ^ x >> 8;
        //            y = y >> 8;
        //        }
        //    }
        //}

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

        //public static void trykey(GThread thread, byte[] input, byte[] output, uint[] keys, long keyStart, int keyRange, int width)
        //{
        //    int tid = thread.threadIdx.x;
        //    if (tid + keyStart < keyStart + keyRange)
        //    {
        //        long key = tid + keyStart;
        //        int keyIndex = 23 * tid;
        //        uint a = (uint)(key / 0x100000000);
        //        uint b = (uint)(key % 0x100000000);
        //        keys[keyIndex] = b;

        //        for (uint i = 0; i < 22; i++)
        //        {
        //            a = a << 3 | a >> 29;
        //            a += b;
        //            a = a ^ i;
        //            a = a << 8 | a >> 24;
        //            b = b ^ a;
        //            keys[keyIndex + i + 1] = b;
        //        }

        //        uint x = 0U;
        //        uint y = 0U;

        //        for (int k = 0; k < input.Length / 8; k++)
        //        {
        //            for (int i = k * 8; i < k * 8 + 8; i++)
        //            {
        //                x = y << 24 | x >> 8;
        //                y = (uint)input[i] << 24 | y >> 8;
        //            }
        //            for (int i = 22; i >= 1; i--)
        //            {
        //                y = y ^ x;
        //                x = x >> 8 | x << 24;
        //                x = x ^ keys[keyIndex + i];
        //                x = (uint)((x - y) % 0x100000000);
        //                x = x >> 3 | x << 29;
        //            }
        //            y = y ^ x;
        //            x = x >> 8 | x << 24;
        //            x = x ^ keys[keyIndex];
        //            x = (uint)((x - y) % 0x100000000);
        //            x = x >> 3 | x << 29;
        //            for (int i = 0; i < 8; i++)
        //            {
        //                output[tid * width] = (byte)(x % 0x100);
        //                x = y << 24 ^ x >> 8;
        //                y = y >> 8;
        //            }
        //        }
        //    }
        //}
    }
}
