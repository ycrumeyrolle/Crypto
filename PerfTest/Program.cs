using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Crypto;

namespace PerfTest
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            //HMACSHA256 _clrSha256 = new HMACSHA256();
            //Hmac _sha256 = new Hmac(new Sha256(), _clrSha256.Key);
            //byte[] _buffer = new byte[32];

            //var value = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz012345678901234567890123456789abcdefghijklmnopqrstuvwxyz012345678901234567890123456789abcdefghijklmnopqrstuvwxyz012345678901234567890123456789");
            //while (true)
            //{
            //    var hash = _clrSha256.ComputeHash(value);
            //    _sha256.ComputeHash(value, _buffer);
            //    if (hash.Length > 32)
            //        break;
            //}

            var _clrSha256 = SHA512.Create();
            var sha256 = new Sha512();
            var sha256Simd = new Sha512Simd();
            //var sha256x = new Sha256X();
            //var sha256Struct = new Sha256Struct();
            //var sha256StructUnroll = new Sha256StructUnroll();
            byte[] _buffer = new byte[64];
            byte[] _buffer_6 = new byte[64];
            byte[] _buffer_12 = new byte[64];

            var value = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567");
            while (true)
            {
                //sha256Struct.ComputeHash(value, _buffer_6);
                //var hash = _clrSha256.ComputeHash(value);
                sha256.ComputeHash(value, _buffer);
                sha256Simd.ComputeHash(value, _buffer);
                //sha256x.ComputeHash(value, _buffer);
                ////sha256StructUnroll.ComputeHash(value, _buffer_12);
                //if (hash.Length > 32)
                //break;
            }
        }
    }
}