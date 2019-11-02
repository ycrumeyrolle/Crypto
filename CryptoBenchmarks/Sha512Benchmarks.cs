using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using Crypto;

namespace Sha256Benchmarks
{
    [MemoryDiagnoser]
    public class Sha512Benchmarks
    {
        private static readonly SHA512 _clrSha512 = SHA512.Create();
        private static readonly Sha512 _sha512 = new Sha512();
        private static readonly Sha512Simd _sha512Simd = new Sha512Simd();
        private readonly byte[] _buffer = new byte[64];

        //[Benchmark(Baseline = true)]
        //[ArgumentsSource(nameof(GetData))]
        //public byte[] Sha512_Clr(byte[] value)
        //{
        //    return _clrSha512.ComputeHash(value);
        //}

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Sha512_Optimized(byte[] value)
        {
            _sha512.ComputeHash(value, _buffer);
            return _buffer;
        }

        [Benchmark(Baseline = false)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Sha512_Simd(byte[] value)
        {
            _sha512Simd.ComputeHash(value, _buffer);
            return _buffer;
        }

        public static IEnumerable<byte[]> GetData()
        {
            //yield return Encoding.UTF8.GetBytes("abc");
            yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            //yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            //yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
            //yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16384).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
            //var data = new byte[1000000];
            //Array.Fill<byte>(data, 0);
            //yield return data;
        }
    }
}
