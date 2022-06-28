using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace creky.server
{
    public class Speck
    {
        public uint[] Encrypt(string input, long key)
        {
            uint[] keyP = PrepareKey(key);

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

            uint[] res = new uint[input.Length];
            int outputId = 0;

            uint x = 0U;
            uint y = 0U;

            for (int k = 0; k < input.Length / 8; k++)
            {
                for (int i = k * 8; i < k * 8 + 8; i++)
                {
                    x = y << 24 | x >> 8;
                    y = (uint)(byte)input[i] << 24 | y >> 8;
                }
                sp_block(ref x, ref y, keyP);
                for (int i = 0; i < 8; i++)
                {
                    long d = 0x100;
                    res[outputId] = (uint)(x % d);
                    outputId++;
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }

            return res;
        }

        public int[] Decrypt(byte[] input, long key)
        {
            uint[] keyP = PrepareKey(key);

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
                sp_blockrev(ref x, ref y, keyP);
                for (int i = 0; i < 8; i++)
                {
                    ints.Add((int)(x % 0x100));
                    x = y << 24 ^ x >> 8;
                    y = y >> 8;
                }
            }

            return ints.ToArray();
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

        private void sp_block (ref uint x, ref uint y, uint[] key)
        {
            rot(key[0], ref x, ref y);
            for (int i = 1; i < 23; i++)
            {
                rot(key[i], ref x, ref y);
            }
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

        private void rotrev (uint i, ref uint x, ref uint y)
        {
            y = y ^ x;
            x = x >> 8 | x << 24;
            x = x ^ i;
            x = (uint)((x - y) % 0x100000000);
            x = x >> 3 | x << 29;
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

    }
}
