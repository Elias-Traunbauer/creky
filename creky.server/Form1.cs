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
            BruteRangeSet set = new BruteRangeSet(18000000, 10, bytesAsInts);
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

        }
    }
}
