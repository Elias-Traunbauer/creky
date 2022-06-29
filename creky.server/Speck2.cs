using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace creky.server
{
    public class Speck2
    {
        public static string Encrypt(string input, long key)
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
                input += add;
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
                    y >>= 8;
                }
            }

            string resSSS = "";
            for (int i = 0; i < res.Length; i++)
            {
                resSSS += res[i].ToString() + " ";
            }

            return resSSS.Substring(0, resSSS.Length - 1);
        }

        public static string Decrypt(string input, long key)
        {
            var inputBytes = new List<byte>();

            foreach (var item in input.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }

            var res = trykey(inputBytes.ToArray(), key);
            string reSSSSS = "";

            for (int i = 0; i < res.Length; i++)
            {
                reSSSSS += (char)res[i];
            }

            return reSSSSS.Replace("\0", "");
        }

        public static string Brutforce(string input, int tries)
        {
            string[] text = input.Split(' ');
            var inputBytes = new byte[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                inputBytes[i] = byte.Parse(text[i]);
            }

            BruteRangeSet set = new BruteRangeSet(1007199254740992, tries, inputBytes);

            if (set.outputInfo.Contains(byte.Parse("1")))
            {
                for (int i = 0; i < set.outputInfo.Length; i++)
                {
                    if (set.outputInfo[i] != 0)
                    {
                        string msg = "";
                        var r = trykey(inputBytes, 1007199254740992 + i);
                        for (int j = 0; j < r.Length; j++)
                        {
                            msg += r[j];
                        }

                        return msg;
                    }
                }
            }

            return null;
        }

        private static uint[] PrepareKey(long key)
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

        private static void sp_block (ref uint x, ref uint y, uint[] key)
        {
            rot(key[0], ref x, ref y);
            for (int i = 1; i < 23; i++)
            {
                rot(key[i], ref x, ref y);
            }
        }

        private static void sp_blockrev(ref uint x, ref uint y, uint[] key)
        {
            for (int i = 22; i >= 1; i--)
            {
                rotrev(key[i], ref x, ref y);
            }
            rotrev(key[0], ref x, ref y);
        }

        private static void rot(uint i, ref uint x, ref uint y)
        {
            x = x << 3 | x >> 29;
            x += y;
            x ^= i;
            x = x << 8 | x >> 24;
            y ^= x;
        }

        private static void rotrev (uint i, ref uint x, ref uint y)
        {
            y ^= x;
            x = x >> 8 | x << 24;
            x ^= i;
            x = (uint)((x - y) % 0x100000000);
            x = x >> 3 | x << 29;
        }

        public static char[] trykey(byte[] msg, long key)
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
                    y >>= 8;
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
