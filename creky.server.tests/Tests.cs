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

            Assert.That(reSSSSS.Replace("\0", ""), Is.EqualTo("Test"));
        }

        [Test]
        public void Encrypt()
        {
            Speck speck = new();
            var array = speck.Encrypt("Test", long.Parse("18238004"));
            string res = "";
            for (int i = 0; i < array.Length; i++)
            {
                res += array[i].ToString() + " ";
            }

            Assert.That(res.Substring(0, res.Length - 1), Is.EqualTo("118 239 150 142 172 247 165 16"));
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
                    }
                }
            }
        }



        [Test]
        public void Encrypt2()
        {
            var res = Speck2.Encrypt("Test", long.Parse("18238004"));

            Assert.That(res, Is.EqualTo("118 239 150 142 172 247 165 16"));
        }

        [Test]
        public void Decrypt2()
        {
            var res = Speck2.Decrypt("118 239 150 142 172 247 165 16", long.Parse("18238004"));

            Assert.That(res, Is.EqualTo("Test"));
        }

        [Test]
        public void Brutforce2()
        {
            Speck2.Brutforce("118 239 150 142 172 247 165 16", 1000);
        }
    }
}