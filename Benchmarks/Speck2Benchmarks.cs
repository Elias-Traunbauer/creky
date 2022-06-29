using BenchmarkDotNet.Attributes;
using creky.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class Speck2Benchmarks
{
    [Benchmark]
    public void Trykey()
    {
        string[] text = "118 239 150 142 172 247 165 16".Split(' ');
        var inputBytes = new byte[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            inputBytes[i] = byte.Parse(text[i]);
        }

        Speck2.trykey(inputBytes, 1007199254740992);
    }

    [Benchmark]
    public void BruteRangeSet()
    {
        string[] text = "118 239 150 142 172 247 165 16".Split(' ');
        var inputBytes = new byte[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            inputBytes[i] = byte.Parse(text[i]);
        }

        BruteRangeSet set = new(1007199254740992, 1000, inputBytes);
    }
}
