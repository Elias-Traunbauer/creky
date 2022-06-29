using BenchmarkDotNet.Attributes;

namespace creky.server;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BrutforceBenchmark
{
    [Benchmark]
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
        BruteRangeSet set = new BruteRangeSet(1007199254740992, 1000, inputBytes.ToArray());
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

    [Benchmark]
    public void Brutforce2()
    {
        Speck speck = new Speck();
        var text = "118 239 150 142 172 247 165 16".Split(' ');
        var inputBytes = new byte[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            inputBytes[i] = byte.Parse(text[i]);
        }

        BruteRangeSet set = new(1007199254740992, 1000, inputBytes);

        if (set.outputInfo.Contains(byte.Parse("1")))
        {
            for (int i = 0; i < set.outputInfo.Length; i++)
            {
                if (set.outputInfo[i] != 0)
                {
                    string msg = "";
                    var r = speck.trykey(inputBytes, 1007199254740992 + i);
                    for (int j = 0; j < r.Length; j++)
                    {
                        msg += r[j];
                    }
                }
            }
        }
    }

    [Benchmark]
    public void Brutforce3()
    {
        Speck2.Brutforce("118 239 150 142 172 247 165 16", 1000);
    }
}
