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
            BruteRangeSet set = new BruteRangeSet(1007199254740992, 2000000000, inputBytes.ToArray());
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

        }

        private void creky_Load(object sender, EventArgs e)
        {
            tbBytesIn.Text = "143 175 181 116 217 0 174 94 226 184 167 49 30 41 203 116 82 113 237 56 254 212 62 90 64 104 255 100 77 170 125 165 85 248 158 169 178 179 187 95 210 37 77 113 22 62 156 142 106 204 62 95 238 205 184 30 144 35 60 27 145 174 168 227";
        }

        private void tbBytesIn_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
