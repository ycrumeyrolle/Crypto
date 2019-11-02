using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using Crypto;

namespace Sha256Benchmarks
{
    [MemoryDiagnoser]
    public class HmacSha256Benchmarks
    {
        private static HMACSHA256 _clrHmacSha256 = new HMACSHA256();
        private static Hmac _hmacSha256 = new Hmac(new Sha256(), _clrHmacSha256.Key);
        private readonly byte[] _buffer = new byte[32];

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] HmacSha256_Clr(byte[] value)
        {
            return _clrHmacSha256.ComputeHash(value);
        }

        [Benchmark(Baseline = false)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] HmacSha256_Simd(byte[] value)
        {
            _hmacSha256.ComputeHash(value, _buffer);
            return _buffer;
        }

        public static IEnumerable<byte[]> GetData()
        {
            yield return Encoding.UTF8.GetBytes("abc");
            yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz012345678901234567890123456789");
            //yield return Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
            //yield return Encoding.UTF8.GetBytes(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567", 16384).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x), sb => sb.ToString()));
        }
    }
}
