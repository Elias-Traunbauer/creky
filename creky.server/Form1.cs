using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace creky.server
{
    public partial class creky : Form
    {
        public creky()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            Speck speck = new Speck();
            var array = speck.Encrypt(tbInput.Text, long.Parse(tbKeyIn.Text));
            string res = "";
            for (int i = 0; i < array.Length; i++)
            {
                res += array[i].ToString() + " ";
            }
            tbOutput.Text = res.Substring(0, res.Length - 1);
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            Speck speck = new Speck();
            var inputBytes = new List<byte>();
            var text = tbBytesInDe.Text;
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }
            var res = speck.trykey(inputBytes.ToArray(), uint.Parse(tbKeyInDe.Text));
            string reSSSSS = "";
            for (int i = 0; i < res.Length; i++)
            {
                reSSSSS += (char)res[i];
            }
            tbTextOutDe.Text = reSSSSS;
        }

        private void btnBruteForce_Click(object sender, EventArgs e)
        {
            Speck speck = new Speck();
            var inputBytes = new List<byte>();
            var text = tbBytesIn.Text;
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }
            int[] bytesAsInts = Array.ConvertAll(inputBytes.ToArray(), c => (int)c);

            int time = Environment.TickCount;
            BruteRangeSet set = new BruteRangeSet(1007199254740992, Convert.ToInt32(amount.Text), inputBytes.ToArray());
            int dur = Environment.TickCount - time;
            MessageBox.Show("dur: " + dur / 1000 + "s");
            if (set.outputInfo.Contains(byte.Parse("1")))
            {
                for (int i = 0; i < set.outputInfo.Length; i++)
                {
                    if (set.outputInfo[i] != 0)
                    {
                        string msg = "";
                        var r = speck.trykey(inputBytes.ToArray(), 1007199254740992 + i);
                        for (int j = 0; j < r.Length; j++)
                        {
                            msg += r[j];
                        }
                        MessageBox.Show(msg);
                    }
                }
            }
            else
            {
                //MessageBox.Show("None found");
            }


            //int tt = Environment.TickCount;
            //Parallel.For(18000000, 18000000 + 10000000, (long key) =>
            //{
            //    var res = speck.trykey(inputBytes.ToArray(), key);
            //    int val = 0;
            //    string msg = "";
            //    for (int i = 0; i < res.Length; i++)
            //    {
            //        msg += res[i];
            //        if (res[i] >= 97 && res[i] <= 122)
            //        {
            //            val++;
            //        }
            //        if (res[i] == 32)
            //        {
            //            val += 3;
            //        }
            //    }
            //    if (val > res.Length)
            //    {
            //        MessageBox.Show(msg);
            //    }
            //});
            //int td = Environment.TickCount - tt;
            //MessageBox.Show("dur: " + td);



            //List<char[]> ress = new List<char[]>();
            //int time = Environment.TickCount;
            //for (long i = 18000000 * 100; i < 18000000 * 100+ 1000000; i++)
            //{
            //    char[] res = speck.trykey(inputBytes.ToArray(), i);
            //    ress.Add(res);

            //    int m = 0;
            //    string txt = "";
            //    for (int g = 0; g < res.Length; g++)
            //    {
            //        txt += res[g];
            //        if (res[g] >= 97 && res[g] <= 122)
            //        {
            //            m++;
            //        }
            //        else if (res[g] == 32)
            //        {
            //            m += 3;
            //        }

            //    }

            //    if (m > res.Length)
            //    {
            //        MessageBox.Show("Found: " + txt);
            //    }
            //}
            //int dir = Environment.TickCount - time;
            //MessageBox.Show(dir / 1000 + "s");
            //foreach (var item in ress)
            //{
            //    int m = 0;
            //    string txt = "";
            //    for (int i = 0; i < item.Length; i++)
            //    {
            //        txt += item[i];
            //        if (item[i] >= 97 && item[i] <= 122)
            //        {
            //            m++;
            //        }
            //        else if (item[i] == 32)
            //        {
            //            m += 3;
            //        }

            //    }

            //    if (m > item.Length)
            //    {
            //        MessageBox.Show("Found: " + txt);
            //    }
            //}

            //List<string> results = new List<string>();

            //for (int i = 0; i < set.outputBytes.Length; i+= set.width)
            //{
            //    string res = "";

            //    for (int j = i; j < set.width; j++)
            //    {
            //        res += set.outputBytes[i + j];
            //    }
            //    results.Add(res);
            //}
        }

        private void creky_Load(object sender, EventArgs e)
        {
            tbBytesIn.Text = "143 175 181 116 217 0 174 94 226 184 167 49 30 41 203 116 82 113 237 56 254 212 62 90 64 104 255 100 77 170 125 165 85 248 158 169 178 179 187 95 210 37 77 113 22 62 156 142 106 204 62 95 238 205 184 30 144 35 60 27 145 174 168 227";
            amount.Text = "200000000";
        }
    }
}
