using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
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
            BruteRangeSet set = new BruteRangeSet(0, 0, new byte[0]);
            Speck speck = new Speck();
            var inputBytes = new List<byte>();
            var text = tbBytesInDe.Text;
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }
            var res = set.decrypt(inputBytes.ToArray(), uint.Parse(tbKeyInDe.Text));
            string reSSSSS = "";
            for (int i = 0; i < res.Length; i++)
            {
                reSSSSS += (char)res[i];
            }
            tbTextOutDe.Text = reSSSSS;
        }

        private void btnBruteForce_Click(object sender, EventArgs e)
        {
            var inputBytes = new List<byte>();
            var text = tbBytesIn.Text;
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }

            BruteRangeSet set = new BruteRangeSet(1000000000000000, 10000000, inputBytes.ToArray());

            List<string> results = new List<string>();

            for (int i = 0; i < set.outputBytes.Length; i+= set.width)
            {
                string res = "";

                for (int j = i; j < set.width; j++)
                {
                    res += set.outputBytes[i + j];
                }
                results.Add(res);
            }
            MessageBox.Show("Finished");
        }
    }
}
