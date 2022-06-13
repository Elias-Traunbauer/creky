using Cudafy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace creky.server
{
    internal class Speck
    {
        List<uint> publicKey = new List<uint>();

        public uint[] Encrypt(string input, long key)
        {
            PrepareKey(key);

            if (input.Length % 8 != 0)
            {
                string add = "";
                int to = 8 - input.Length % 8;
                for (int i = 0; i < to; i++)
                {
                    add += (char)0;
                }
                input = input + add;
            }

            List<uint> output = new List<uint>();

            uint x = 0U;
            uint y = 0U;

            for (int k = 0; k < input.Length / 8; k++)
            {
                for (int i = k * 8; i < k * 8 + 8; i++)
                {
                    x = y << 24 | x >> 8;
                    y = (uint)((uint)(byte)input[i] << 24 | y >> 8);
                }
                sp_block(ref x, ref y);
                for (int i = 0; i < 8; i++)
                {
                    long d = 0x100;
                    output.Add((uint)(x % d));
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }

            return output.ToArray();
        }

        public int[] Decrypt(byte[] input, long key)
        {
            PrepareKey(key);

            List<int> ints = new List<int>();

            uint x = 0U;
            uint y = 0U;

            for (int k = 0; k < input.Length / 8; k++)
            {
                for (int i = k * 8; i < k * 8 + 8; i++)
                {
                    x = y << 24 | x >> 8;
                    y = (uint)input[i] << 24 | y >> 8;
                }
                sp_blockrev(ref x, ref y);
                for (int i = 0; i < 8; i++)
                {
                    ints.Add((int)(x % 0x100));
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }

            return ints.ToArray();
        }

        private void PrepareKey(long key)
        {
            publicKey.Clear();
            uint a = (uint)(key / 0x100000000);
            uint b = (uint)(key % 0x100000000);
            publicKey.Add(b);
            for (uint i = 0; i < 22; i++)
            {
                rot(i, ref a, ref b);
                publicKey.Add(b);
            }
        }

        private void sp_block (ref uint x, ref uint y)
        {
            rot(publicKey[0], ref x, ref y);
            for (int i = 1; i < 23; i++)
            {
                rot(publicKey[i], ref x, ref y);
            }
        }

        private void sp_blockrev(ref uint x, ref uint y)
        {
            for (int i = 22; i >= 1; i--)
            {
                rotrev(publicKey[i], ref x, ref y);
            }
            rotrev(publicKey[0], ref x, ref y);
        }

        private void rot(uint i, ref uint x, ref uint y)
        {
            x = x << 3 | x >> 29;
            x += y;
            x = x ^ i;
            x = x << 8 | x >> 24;
            y = y ^ x;
        }

        private void rotrev (uint i, ref uint x, ref uint y)
        {
            y = y ^ x;
            x = x >> 8 | x << 24;
            x = x ^ i;
            x = (uint)((x - y) % 0x100000000);
            x = x >> 3 | x << 29;
        }

    }
}
