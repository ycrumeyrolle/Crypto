//using System;
//using System.Buffers.Binary;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

//namespace Sha256Tests
//{
//    public class Sha256_2
//    {
//        public void ComputeHash(byte[] blk, Span<byte> ctx)
//        {
//            ref byte ctxRef = ref MemoryMarshal.GetReference(ctx);

//            uint a0;
//            uint b0;
//            uint c0;
//            uint d0;
//            uint e0;
//            uint f0;
//            uint g0;
//            uint h0;
//            uint a = a0 = Unsafe.ReadUnaligned<uint>(ref ctxRef);
//            uint b = b0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, sizeof(uint)));
//            uint c = c0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 2 * sizeof(uint)));
//            uint d = d0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 3 * sizeof(uint)));
//            uint e = e0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 4 * sizeof(uint)));
//            uint f = f0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 5 * sizeof(uint)));
//            uint g = g0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 6 * sizeof(uint)));
//            uint h = h0 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ctxRef, 7 * sizeof(uint)));

//            uint w0, w1, w2, w3, w4, w5, w6, w7;
//            uint w8, w9, w10, w11, w12, w13, w14, w15;
//            uint T1, T2;

//            w0 = LOAD_BIG_32(blk[4 * 0]);
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 0, w0);

//            w1 = LOAD_BIG_32(blk[4 * 1]);
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 1, w1);
//            w2 = LOAD_BIG_32(blk[4 * 2]);
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 2, w2);
//            w3 = LOAD_BIG_32(blk[4 * 3]);
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 3, w3);
//            w4 = LOAD_BIG_32(blk[4 * 4]);
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 4, w4);
//            w5 = LOAD_BIG_32(blk[4 * 5]);
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 5, w5);
//            w6 = LOAD_BIG_32(blk[4 * 6]);
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 6, w6);
//            w7 = LOAD_BIG_32(blk[4 * 7]);
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 7, w7);
//            w8 = LOAD_BIG_32(blk[4 * 8]);
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 8, w8);
//            w9 = LOAD_BIG_32(blk[4 * 9]);
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 9, w9);
//            w10 = LOAD_BIG_32(blk[4 * 10]);
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 10, w10);
//            w11 = LOAD_BIG_32(blk[4 * 11]);
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 11, w11);
//            w12 = LOAD_BIG_32(blk[4 * 12]);
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 12, w12);
//            w13 = LOAD_BIG_32(blk[4 * 13]);
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 13, w13);
//            w14 = LOAD_BIG_32(blk[4 * 14]);
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 14, w14);
//            w15 = LOAD_BIG_32(blk[4 * 15]);
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 15, w15);

//            w0 = Sigma1(w14) + w9 + Sigma0(w1) + w0;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 16, w0);
//            w1 = Sigma1(w15) + w10 + Sigma0(w2) + w1;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 17, w1);
//            w2 = Sigma1(w0) + w11 + Sigma0(w3) + w2;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 18, w2);
//            w3 = Sigma1(w1) + w12 + Sigma0(w4) + w3;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 19, w3);
//            w4 = Sigma1(w2) + w13 + Sigma0(w5) + w4;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 20, w4);
//            w5 = Sigma1(w3) + w14 + Sigma0(w6) + w5;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 21, w5);
//            w6 = Sigma1(w4) + w15 + Sigma0(w7) + w6;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 22, w6);
//            w7 = Sigma1(w5) + w0 + Sigma0(w8) + w7;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 23, w7);
//            w8 = Sigma1(w6) + w1 + Sigma0(w9) + w8;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 24, w8);
//            w9 = Sigma1(w7) + w2 + Sigma0(w10) + w9;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 25, w9);
//            w10 = Sigma1(w8) + w3 + Sigma0(w11) + w10;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 26, w10);
//            w11 = Sigma1(w9) + w4 + Sigma0(w12) + w11;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 27, w11);
//            w12 = Sigma1(w10) + w5 + Sigma0(w13) + w12;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 28, w12);
//            w13 = Sigma1(w11) + w6 + Sigma0(w14) + w13;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 29, w13);
//            w14 = Sigma1(w12) + w7 + Sigma0(w15) + w14;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 30, w14);
//            w15 = Sigma1(w13) + w8 + Sigma0(w0) + w15;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 31, w15);

//            w0 = Sigma1(w14) + w9 + Sigma0(w1) + w0;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 32, w0);
//            w1 = Sigma1(w15) + w10 + Sigma0(w2) + w1;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 33, w1);
//            w2 = Sigma1(w0) + w11 + Sigma0(w3) + w2;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 34, w2);
//            w3 = Sigma1(w1) + w12 + Sigma0(w4) + w3;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 35, w3);
//            w4 = Sigma1(w2) + w13 + Sigma0(w5) + w4;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 36, w4);
//            w5 = Sigma1(w3) + w14 + Sigma0(w6) + w5;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 37, w5);
//            w6 = Sigma1(w4) + w15 + Sigma0(w7) + w6;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 38, w6);
//            w7 = Sigma1(w5) + w0 + Sigma0(w8) + w7;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 39, w7);
//            w8 = Sigma1(w6) + w1 + Sigma0(w9) + w8;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 40, w8);
//            w9 = Sigma1(w7) + w2 + Sigma0(w10) + w9;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 41, w9);
//            w10 = Sigma1(w8) + w3 + Sigma0(w11) + w10;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 42, w10);
//            w11 = Sigma1(w9) + w4 + Sigma0(w12) + w11;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 43, w11);
//            w12 = Sigma1(w10) + w5 + Sigma0(w13) + w12;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 44, w12);
//            w13 = Sigma1(w11) + w6 + Sigma0(w14) + w13;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 45, w13);
//            w14 = Sigma1(w12) + w7 + Sigma0(w15) + w14;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 46, w14);
//            w15 = Sigma1(w13) + w8 + Sigma0(w0) + w15;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 47, w15);

//            w0 = Sigma1(w14) + w9 + Sigma0(w1) + w0;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 48, w0);
//            w1 = Sigma1(w15) + w10 + Sigma0(w2) + w1;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 49, w1);
//            w2 = Sigma1(w0) + w11 + Sigma0(w3) + w2;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 50, w2);
//            w3 = Sigma1(w1) + w12 + Sigma0(w4) + w3;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 51, w3);
//            w4 = Sigma1(w2) + w13 + Sigma0(w5) + w4;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 52, w4);
//            w5 = Sigma1(w3) + w14 + Sigma0(w6) + w5;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 53, w5);
//            w6 = Sigma1(w4) + w15 + Sigma0(w7) + w6;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 54, w6);
//            w7 = Sigma1(w5) + w0 + Sigma0(w8) + w7;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 55, w7);
//            w8 = Sigma1(w6) + w1 + Sigma0(w9) + w8;
//            Sha256Round(a, b, c, ref d, e, f, g, ref h, 56, w8);
//            w9 = Sigma1(w7) + w2 + Sigma0(w10) + w9;
//            Sha256Round(h, a, b, ref c, d, e, f, ref g, 57, w9);
//            w10 = Sigma1(w8) + w3 + Sigma0(w11) + w10;
//            Sha256Round(g, h, a, ref b, c, d, e, ref f, 58, w10);
//            w11 = Sigma1(w9) + w4 + Sigma0(w12) + w11;
//            Sha256Round(f, g, h, ref a, b, c, d, ref e, 59, w11);
//            w12 = Sigma1(w10) + w5 + Sigma0(w13) + w12;
//            Sha256Round(e, f, g, ref h, a, b, c, ref d, 60, w12);
//            w13 = Sigma1(w11) + w6 + Sigma0(w14) + w13;
//            Sha256Round(d, e, f, ref g, h, a, b, ref c, 61, w13);
//            w14 = Sigma1(w12) + w7 + Sigma0(w15) + w14;
//            Sha256Round(c, d, e, ref f, g, h, a, ref b, 62, w14);
//            w15 = Sigma1(w13) + w8 + Sigma0(w0) + w15;
//            Sha256Round(b, c, d, ref e, f, g, h, ref a, 63, w15);

//            Unsafe.WriteUnaligned(ref ctxRef, a + a0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, sizeof(uint)), b + b0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 2 * sizeof(uint)), c + c0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 3 * sizeof(uint)), d + d0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 4 * sizeof(uint)), e + e0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 5 * sizeof(uint)), f + f0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 6 * sizeof(uint)), g + g0);
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref ctxRef, 7 * sizeof(uint)), h + h0);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint RotR32(uint x, byte y)
//             => (x >> y) | (x << (32 - y));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint BigSigma1(uint x)
//            => RotR32(x, 6) ^ RotR32(x, 11) ^ RotR32(x, 25);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint BigSigma0(uint x)
//            => RotR32(x, 2) ^ RotR32(x, 13) ^ RotR32(x, 22);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint Sigma1(uint x)
//            => RotR32(x, 17) ^ RotR32(x, 19) ^ (x >> 10);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint Sigma0(uint x)
//            => RotR32(x, 7) ^ RotR32(x, 18) ^ (x >> 3);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint Ch(uint a, uint b, uint c)
//            => (a & b) ^ (~(a) & c);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint Maj(uint a, uint b, uint c)
//            => (a & b) ^ (a & c) ^ (b & c);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static void Sha256Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, int rc, uint w)
//        {
//            uint T1 = h + BigSigma1(e) + Ch(e, f, g) + SHA256_CONST[rc] + w;
//            d += T1;
//            uint T2 = BigSigma0(a) + Maj(a, b, c);
//            h = T1 + T2;
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint LOAD_BIG_32(uint addr) =>
//            BinaryPrimitives.ReverseEndianness(addr);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static ulong LOAD_BIG_64(ulong addr) => BinaryPrimitives.ReverseEndianness(addr);

//        private static ReadOnlySpan<uint> SHA256_CONST => new uint[] {
//                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
//                0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
//                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
//                0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
//                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
//                0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
//                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
//                0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
//                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
//                0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
//                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
//                0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
//                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
//                0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
//                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
//                0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
//            };

//        static readonly uint[] __sha256_init = new uint[] {
//    0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
//    0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
//};

//    }
//}
