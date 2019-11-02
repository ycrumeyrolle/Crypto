using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using Crypto;

namespace Sha256Benchmarks
{
    [MemoryDiagnoser]
    public class Sha251Benchmarks
    {
        private static readonly SHA256 _clrSha256 = SHA256.Create();
        private static readonly Sha256 _sha256 = new Sha256();
        private static readonly Sha256Simd _sha256Simd = new Sha256Simd();
        //private static readonly Sha256Struct _sha256_6 = new Sha256Struct();
        //private static readonly Sha256StructUnroll _sha256_Struct2 = new Sha256StructUnroll();
        private readonly byte[] _buffer = new byte[32];

        //[Benchmark(Baseline = false)]
        //[ArgumentsSource(nameof(GetData))]
        //public byte[] Sha256_Clr(byte[] value)
        //{
        //    return _clrSha256.ComputeHash(value);
        //}

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Sha256_Optimized(byte[] value)
        {
            _sha256.ComputeHash(value, _buffer);
            return _buffer;
        }

        [Benchmark(Baseline = false)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Sha256_Simd(byte[] value)
        {
            _sha256Simd.ComputeHash(value, _buffer);
            return _buffer;
        }

        //[Benchmark(Baseline = false)]
        //[ArgumentsSource(nameof(GetData))]
        //public byte[] Sha256_Struct(byte[] value)
        //{
        //    _sha256_6.ComputeHash(value, _buffer);
        //    return _buffer;
        //}

        //[Benchmark(Baseline = false)]
        //[ArgumentsSource(nameof(GetData))]
        //public byte[] Sha256_Struct2(byte[] value)
        //{
        //    _sha256_Struct2.ComputeHash(value, _buffer);
        //    return _buffer;
        //}

        public static IEnumerable<byte[]> GetData()
        {
            //yield return Encoding.UTF8.GetBytes("abc");
            //yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456abcdefghijklmnopqrstuvwxyz0123456");
            yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz012345678abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz0123456789012345678901234567890123456790123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz012345678abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz0123456789012345678901234567890123456790123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            //yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            //yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
            //yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16384).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
            //var data = new byte[1000000];
            //Array.Fill<byte>(data, 0);
            //yield return data;
        }
    }
}
