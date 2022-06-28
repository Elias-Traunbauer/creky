namespace creky.server.tests
{
    public class Tests
    {
        [Test]
        public void Decrypt()
        {
            Speck speck = new();
            var inputBytes = new List<byte>();
            var text = "118 239 150 142 172 247 165 16";
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }
            var res = speck.trykey(inputBytes.ToArray(), uint.Parse("18238004"));
            string reSSSSS = "";
            for (int i = 0; i < res.Length; i++)
            {
                reSSSSS += (char)res[i];
            }

            Console.WriteLine(reSSSSS);
            Assert.That(reSSSSS.Replace("\0", ""), Is.EqualTo("Test"));
        }

        [Test]
        public void Encrypt()
        {
            Speck speck = new Speck();
            var array = speck.Encrypt("Test", long.Parse("18238004"));
            string res = "";
            for (int i = 0; i < array.Length; i++)
            {
                res += array[i].ToString() + " ";
            }

            Console.WriteLine(res.Substring(0, res.Length - 1));
            Assert.AreEqual("118 239 150 142 172 247 165 16", res.Substring(0, res.Length - 1));
        }

        [Test]
        public void Brutforce()
        {
            Speck speck = new Speck();
            var inputBytes = new List<byte>();
            var text = "118 239 150 142 172 247 165 16";
            foreach (var item in text.Split(' '))
            {
                inputBytes.Add(byte.Parse(item));
            }
            int[] bytesAsInts = Array.ConvertAll(inputBytes.ToArray(), c => (int)c);

            int time = Environment.TickCount;
            BruteRangeSet set = new(1007199254740992, 2000000000, inputBytes.ToArray());
            int dur = Environment.TickCount - time;
 
            Console.WriteLine("dur: " + dur / 1000 + "s");

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

                        Console.WriteLine(msg);
                        Assert.AreEqual("Test", msg);
                    }
                }
            }
            else
            {
                //MessageBox.Show("None found");
            }
        }
    }
}