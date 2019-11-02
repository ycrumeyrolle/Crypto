using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sha256Tests
{
    public class Sha256_4
    {
        public class SHA256_CTX
        {
            public byte[] data = new byte[64];
            public uint datalen;
            public long bitlen;
            public uint[] state = new uint[8];
        }


        static readonly uint[] k = {
    0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
    0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
    0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
    0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
    0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
    0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
    0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
    0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2
};

        /*********************** FUNCTION DEFINITIONS ***********************/
        public void sha256_transform(SHA256_CTX ctx, ReadOnlySpan<byte> data)
        {

            uint a, b, c, d, e, f, g, h, t1, t2;
            Span<uint> w = stackalloc uint[64];
            ref byte wRef = ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(w));
            ref byte currentBlock = ref MemoryMarshal.GetReference(data);

            for (int i = 0, j = 0; i < 16; ++i, j += 4)
                w[i] = (uint)((Unsafe.Add(ref currentBlock, j) << 24) | (Unsafe.Add(ref currentBlock, j + 1) << 16) | (Unsafe.Add(ref currentBlock, j + 2) << 8) | (Unsafe.Add(ref currentBlock, j + 3)));
            for (int i = 16; i < 64; ++i)
            {
                uint s0 = Sigma0(w[i - 15]);
                uint s1 = Sigma1(w[i - 2]);
                w[i] = w[i - 16] + s0 + w[i - 7] + s1;
            }

            a = ctx.state[0];
            b = ctx.state[1];
            c = ctx.state[2];
            d = ctx.state[3];
            e = ctx.state[4];
            f = ctx.state[5];
            g = ctx.state[6];
            h = ctx.state[7];

            for (int i = 0; i < 64; ++i)
            {
                t1 = h + BigSigma1(e) + Ch(e, f, g) + k[i] + w[i];
                t2 = BigSigma0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + t1;
                d = c;
                c = b;
                b = a;
                a = t1 + t2;
            }

            ctx.state[0] += a;
            ctx.state[1] += b;
            ctx.state[2] += c;
            ctx.state[3] += d;
            ctx.state[4] += e;
            ctx.state[5] += f;
            ctx.state[6] += g;
            ctx.state[7] += h;
        }

        public void ComputeHash(ReadOnlySpan<byte> src, Span<byte> destination)
        {
            var ctx = new SHA256_CTX();
            sha256_init(ctx);
            sha256_update(ctx, src, src.Length);
            sha256_final(ctx, destination);
        }

        void sha256_init(SHA256_CTX ctx)
        {
            ctx.datalen = 0;
            ctx.bitlen = 0;
            ctx.state[0] = 0x6a09e667;
            ctx.state[1] = 0xbb67ae85;
            ctx.state[2] = 0x3c6ef372;
            ctx.state[3] = 0xa54ff53a;
            ctx.state[4] = 0x510e527f;
            ctx.state[5] = 0x9b05688c;
            ctx.state[6] = 0x1f83d9ab;
            ctx.state[7] = 0x5be0cd19;
        }

        void sha256_update(SHA256_CTX ctx, ReadOnlySpan<byte> data, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                ctx.data[ctx.datalen] = data[i];
                ctx.datalen++;
                if (ctx.datalen == 64)
                {
                    sha256_transform(ctx, ctx.data);
                    ctx.bitlen += 512;
                    ctx.datalen = 0;
                }
            }
        }

        void sha256_final(SHA256_CTX ctx, Span<byte> hash)
        {
            uint i;

            i = ctx.datalen;

            // Pad whatever data is left in the buffer.
            if (ctx.datalen < 56)
            {
                ctx.data[i++] = 0x80;
                while (i < 56)
                    ctx.data[i++] = 0x00;
            }
            else
            {
                ctx.data[i++] = 0x80;
                while (i < 64)
                    ctx.data[i++] = 0x00;
                sha256_transform(ctx, ctx.data);
                ctx.data.AsSpan(0, 56).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ctx.bitlen += ctx.datalen * 8;
            ctx.data[63] = (byte)ctx.bitlen;
            ctx.data[62] = (byte)(ctx.bitlen >> 8);
            ctx.data[61] = (byte)(ctx.bitlen >> 16);
            ctx.data[60] = (byte)(ctx.bitlen >> 24);
            ctx.data[59] = (byte)(ctx.bitlen >> 32);
            ctx.data[58] = (byte)(ctx.bitlen >> 40);
            ctx.data[57] = (byte)(ctx.bitlen >> 48);
            ctx.data[56] = (byte)(ctx.bitlen >> 56);
            sha256_transform(ctx, ctx.data);

            // Since this implementation uses little endian byte ordering and SHA uses big endian,
            // reverse all the bytes when copying the final state to the output hash.
            for (int j = 0; j < 4; ++j)
            {
                hash[j] = (byte)((ctx.state[0] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 4] = (byte)((ctx.state[1] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 8] = (byte)((ctx.state[2] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 12] = (byte)((ctx.state[3] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 16] = (byte)((ctx.state[4] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 20] = (byte)((ctx.state[5] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 24] = (byte)((ctx.state[6] >> (24 - j * 8)) & 0x000000ff);
                hash[j + 28] = (byte)((ctx.state[7] >> (24 - j * 8)) & 0x000000ff);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotR32(uint a, byte b)
            => (((a) >> (b)) | ((a) << (32 - (b))));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint BigSigma1(uint x)
            => RotR32(x, 6) ^ RotR32(x, 11) ^ RotR32(x, 25);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint BigSigma0(uint x)
            => RotR32(x, 2) ^ RotR32(x, 13) ^ RotR32(x, 22);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Sigma1(uint x)
            => RotR32(x, 17) ^ RotR32(x, 19) ^ (x >> 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Sigma0(uint x)
            => RotR32(x, 7) ^ RotR32(x, 18) ^ (x >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Ch(uint x, uint y, uint z)
            => (((x) & (y)) ^ (~(x) & (z)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Maj(uint x, uint y, uint z)
            => (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)));
    }
}

