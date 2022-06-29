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
    }
}
