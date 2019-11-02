//using System;
//using System.Security.Cryptography;
//using System.Text;
//using Xunit;

//namespace Sha256Tests
//{
//    public class Sha256Tests_Struct2
//    {
//        [Theory]
//        [InlineData(0, "")]
//        [InlineData(3, "abc")]
//        [InlineData(55, "abcdefghijklmnopqrstuvwxyz01234567890123456789012345678")]
//        [InlineData(56, "abcdefghijklmnopqrstuvwxyz012345678901234567890123456789")]
//        [InlineData(64, "abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567")]
//        [InlineData(65, "abcdefghijklmnopqrstuvwxyz012345678901234567890123456789012345678")]
//        [InlineData(128, "abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567abcdefghijklmnopqrstuvwxyz01234567890123456789012345678901234567")]
//        public void Hash(int count, string value)
//        {
//            Assert.Equal(count, value.Length);
//            var shaClr = SHA256.Create();
//            var sha = new Sha256StructUnroll();
//            var data = Encoding.UTF8.GetBytes(value);
//            var expectedHash = shaClr.ComputeHash(data);

//            var result = new byte[32];
//            sha.ComputeHash(data, result);
//            Assert.Equal(expectedHash, result);
//        }

//        [Theory]
//        [InlineData(0xbd, 1, "68325720aabd7c82f30f554b313d0570c95accbb7dc4b5aae11204c08ffe732b")]
//        [InlineData(0x00, 55, "02779466cdec163811d078815c633f21901413081449002f24aa3e80f0b88ef7")]
//        [InlineData(0x00, 56, "d4817aa5497628e7c77e6b606107042bbba3130888c5f47a375e6179be789fbb")]
//        [InlineData(0x00, 57, "65a16cb7861335d5ace3c60718b5052e44660726da4cd13bb745381b235a1785")]
//        [InlineData(0x00, 64, "f5a5fd42d16a20302798ef6ed309979b43003d2320d9f0e8ea9831a92759fb4b")]
//        [InlineData(0x00, 1000, "541b3e9daa09b20bf85fa273e5cbd3e80185aa4ec298e765db87742b70138a53")]
//        [InlineData(0x41, 1000, "c2e686823489ced2017f6059b8b239318b6364f6dcd835d0a519105a1eadd6e4")]
//        [InlineData(0x55, 1005, "f4d62ddec0f3dd90ea1380fa16a5ff8dc4c54b21740650f24afc4120903552b0")]
//        [InlineData(0x00, 1000000, "d29751f2649b32ff572b5e0a9f541ea660a50f94ff0beedfb0b692b924cc8025")]
//        [InlineData(0x5a, 0x20000000, "15a1868c12cc53951e182344277447cd0979536badcc512ad24c67e9b2d4f3dd")]
//        [InlineData(0x00, 0x41000000, "461c19a93bd4344f9215f5ec64357090342bc66b15a148317d276e31cbc20b53")]
//        //[InlineData(0x42, 0x6000003e, "c23ce8a7895f4b21ec0daf37920ac0a262a220045a03eb2dfed48ef9b05aabea")]
//        public void HashNist(byte value, int byteCount, string expectedHex)
//        {
//            Assert.Equal(64, expectedHex.Length);
//            var sha = new Sha256StructUnroll();
//            //var sha = SHA256.Create();
//            var expectedHash = ToBin(expectedHex);
//            var data = new byte[byteCount];
//            Array.Fill(data, value);
//            var result = new byte[32];
//            sha.ComputeHash(data, result);
//            Assert.Equal(expectedHash, result);
//        }

//        public static byte[] ToBin(string hex)
//        {
//            byte[] bytes = new byte[hex.Length / 2];
//            for (int i = 0; i < hex.Length; i += 2)
//            {
//                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
//            }
//            return bytes;
//        }
//    }
//}
