using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Sha256Tests
{
    public class Sha256StructUnroll
    {
        // 3, 2, 1, 0, 7, 6, 5, 4,
        // 11, 10, 9, 8, 15, 14, 13, 12,
        // 19, 18, 17, 16, 23, 22, 21, 20,
        // 27, 26, 25, 24, 31, 30, 29, 28
        private static Vector256<byte> _shuffleMask256 = Vector256.Create(
                289644378169868803,
                868365760874482187,
                1447087143579095571,
                2025808526283708955
                ).AsByte();

        // 3, 2, 1, 0, 7, 6, 5, 4,
        // 11, 10, 9, 8, 15, 14, 13, 12
        private static Vector128<byte> _shuffleMask128 = Vector128.Create(
                289644378169868803,
                868365760874482187
                ).AsByte();

        private static readonly Vector256<uint> _initialState = Vector256.Create(0x6a09e667,
                                      0xbb67ae85,
                                      0x3c6ef372,
                                      0xa54ff53a,
                                      0x510e527f,
                                      0x9b05688c,
                                      0x1f83d9ab,
                                      0x5be0cd19);
        public void ComputeHash(ReadOnlySpan<byte> src, Span<byte> destination, ReadOnlySpan<byte> prepend = default)
        {
            var state = new State
            {
                vector = _initialState
            };

            if (!prepend.IsEmpty)
            {
                Debug.Assert(prepend.Length == 64);
                state.Transform(ref MemoryMarshal.GetReference(prepend));
            }

            ref byte srcRef = ref MemoryMarshal.GetReference(src);
            ref byte srcEndRef = ref Unsafe.Add(ref srcRef, src.Length - 64 + 1);
            while (Unsafe.IsAddressLessThan(ref srcRef, ref srcEndRef))
            {
                state.Transform(ref srcRef);
                srcRef = ref Unsafe.Add(ref srcRef, 64);
            }

            int dataLength = src.Length + prepend.Length;
            int remaining = dataLength & 63;

            Span<byte> lastBlock = stackalloc byte[64];
            ref byte lastBlockRef = ref MemoryMarshal.GetReference(lastBlock);
            Unsafe.CopyBlockUnaligned(ref lastBlockRef, ref srcRef, (uint)remaining);

            // Pad the last block with a "1"
            Unsafe.Add(ref lastBlockRef, remaining) = 0x80;
            lastBlock.Slice(remaining + 1).Clear();
            if (remaining >= 56)
            {
                state.Transform(ref lastBlockRef);
                lastBlock.Slice(0, 56).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, 56), BinaryPrimitives.ReverseEndianness(bitLength));
            state.Transform(ref lastBlockRef);
            
            // Reverse to big endian
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(state.vector.AsByte(), _shuffleMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(state.vector.GetLower().AsByte(), _shuffleMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(state.vector.GetUpper().AsByte(), _shuffleMask128));
            }
            else
            {
                for (int j = 0; j < 4; ++j)
                {
                    destination[j] = (byte)((state.vector.GetElement(0) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 4] = (byte)((state.vector.GetElement(1) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 8] = (byte)((state.vector.GetElement(2) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 12] = (byte)((state.vector.GetElement(3) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 16] = (byte)((state.vector.GetElement(4) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 20] = (byte)((state.vector.GetElement(5) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 24] = (byte)((state.vector.GetElement(6) >> (24 - j * 8)) & 0x000000ff);
                    destination[j + 28] = (byte)((state.vector.GetElement(7) >> (24 - j * 8)) & 0x000000ff);
                }
            }
        }


        [StructLayout(LayoutKind.Explicit, Size = 96)]
        public struct State
        {
            [FieldOffset(0)]
            public uint a;
            [FieldOffset(4)]
            public uint b;
            [FieldOffset(8)]
            public uint c;
            [FieldOffset(12)]
            public uint d;
            [FieldOffset(16)]
            public uint e;
            [FieldOffset(20)]
            public uint f;
            [FieldOffset(24)]
            public uint g;
            [FieldOffset(28)]
            public uint h;

            [FieldOffset(0)]
            public Vector256<uint> vector;

            [FieldOffset(32)]
            public uint w0;
            [FieldOffset(36)]
            public uint w1;
            [FieldOffset(40)]
            public uint w2;
            [FieldOffset(44)]
            public uint w3;
            [FieldOffset(48)]
            public uint w4;
            [FieldOffset(52)]
            public uint w5;
            [FieldOffset(56)]
            public uint w6;
            [FieldOffset(60)]
            public uint w7;
            [FieldOffset(64)]
            public uint w8;
            [FieldOffset(68)]
            public uint w9;
            [FieldOffset(72)]
            public uint w10;
            [FieldOffset(76)]
            public uint w11;
            [FieldOffset(80)]
            public uint w12;
            [FieldOffset(84)]
            public uint w13;
            [FieldOffset(88)]
            public uint w14;
            [FieldOffset(92)]
            public uint w15;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Transform(ref byte currentBlock)
            {
                var state = vector;
                
                ref byte wRef = ref Unsafe.As<uint, byte>(ref Unsafe.AsRef(w0));

                if (Avx2.IsSupported)
                {
                    Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask256));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask256));
                }
                else if (Ssse3.IsSupported)
                {
                    Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask128));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), _shuffleMask128));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask128));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), _shuffleMask128));
                }
                else
                {
                    w0 = ComputeW(ref currentBlock, 0, 0);
                    w1 = ComputeW(ref currentBlock,  1 , 0);
                    w2 = ComputeW(ref currentBlock,  2 , 0);
                    w3 = ComputeW(ref currentBlock,  3 , 0);
                    w4 = ComputeW(ref currentBlock,  4 , 4);
                    w5 = ComputeW(ref currentBlock,  5 , 4);
                    w6 = ComputeW(ref currentBlock,  6 , 4);
                    w7 = ComputeW(ref currentBlock,  7 , 4);
                    w8 = ComputeW(ref currentBlock,  8 , 8);
                    w9 = ComputeW(ref currentBlock,  9 , 8);
                    w10 = ComputeW(ref currentBlock, 10, 8);
                    w11 = ComputeW(ref currentBlock, 11, 8);
                    w12 = ComputeW(ref currentBlock, 12, 12);
                    w13 = ComputeW(ref currentBlock, 13, 12);
                    w14 = ComputeW(ref currentBlock, 14, 12);
                    w15 = ComputeW(ref currentBlock, 15, 12);

                    uint ComputeW(ref byte block, int i, int j) => (uint)((Unsafe.Add(ref block, j) << 24) | (Unsafe.Add(ref block, j + 1) << 16) | (Unsafe.Add(ref block, j + 2) << 8) | (Unsafe.Add(ref block, j + 3)));
                }

                Rounds();

                vector = Avx2.Add(state, vector);
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private void Rounds()
            {
                Round(a, b, c, ref d, e, f, g, ref h, w0, k[0]);
                Round(h, a, b, ref c, d, e, f, ref g, w1, k[1]);
                Round(g, h, a, ref b, c, d, e, ref f, w2, k[2]);
                Round(f, g, h, ref a, b, c, d, ref e, w3, k[3]);
                Round(e, f, g, ref h, a, b, c, ref d, w4, k[4]);
                Round(d, e, f, ref g, h, a, b, ref c, w5, k[5]);
                Round(c, d, e, ref f, g, h, a, ref b, w6, k[6]);
                Round(b, c, d, ref e, f, g, h, ref a, w7, k[7]);

                Round(a, b, c, ref d, e, f, g, ref h, w8, k[8]);
                Round(h, a, b, ref c, d, e, f, ref g, w9, k[9]);
                Round(g, h, a, ref b, c, d, e, ref f, w10, k[10]);
                Round(f, g, h, ref a, b, c, d, ref e, w11, k[11]);
                Round(e, f, g, ref h, a, b, c, ref d, w12, k[12]);
                Round(d, e, f, ref g, h, a, b, ref c, w13, k[13]);
                Round(c, d, e, ref f, g, h, a, ref b, w14, k[14]);
                Round(b, c, d, ref e, f, g, h, ref a, w15, k[15]);

                w0 = Sigma1(w14) + w0 + w9 + Sigma0(w1);
                Round(a, b, c, ref d, e, f, g, ref h, w0, k[16]);
                w1 = Sigma1(w15) + w1 + w10 + Sigma0(w2);
                Round(h, a, b, ref c, d, e, f, ref g, w1, k[17]);
                w2 = Sigma1(w0) + w2 + w11 + Sigma0(w3);
                Round(g, h, a, ref b, c, d, e, ref f, w2, k[18]);
                w3 = Sigma1(w1) + w3 + w12 + Sigma0(w4);
                Round(f, g, h, ref a, b, c, d, ref e, w3, k[19]);
                w4 = Sigma1(w2) + w4 + w13 + Sigma0(w5);
                Round(e, f, g, ref h, a, b, c, ref d, w4, k[20]);
                w5 = Sigma1(w3) + w5 + w14 + Sigma0(w6);
                Round(d, e, f, ref g, h, a, b, ref c, w5, k[21]);
                w6 = Sigma1(w4) + w6 + w15 + Sigma0(w7);
                Round(c, d, e, ref f, g, h, a, ref b, w6, k[22]);
                w7 = Sigma1(w5) + w7 + w0 + Sigma0(w8);
                Round(b, c, d, ref e, f, g, h, ref a, w7, k[23]);

                w8 = Sigma1(w6) + w8 + w1 + Sigma0(w9);
                Round(a, b, c, ref d, e, f, g, ref h, w8, k[24]);
                w9 = Sigma1(w7) + w9 + w2 + Sigma0(w10);
                Round(h, a, b, ref c, d, e, f, ref g, w9, k[25]);
                w10 = Sigma1(w8) + w10 + w3 + Sigma0(w11);
                Round(g, h, a, ref b, c, d, e, ref f, w10, k[26]);
                w11 = Sigma1(w9) + w11 + w4 + Sigma0(w12);
                Round(f, g, h, ref a, b, c, d, ref e, w11, k[27]);
                w12 = Sigma1(w10) + w12 + w5 + Sigma0(w13);
                Round(e, f, g, ref h, a, b, c, ref d, w12, k[28]);
                w13 = Sigma1(w11) + w13 + w6 + Sigma0(w14);
                Round(d, e, f, ref g, h, a, b, ref c, w13, k[29]);
                w14 = Sigma1(w12) + w14 + w7 + Sigma0(w15);
                Round(c, d, e, ref f, g, h, a, ref b, w14, k[30]);
                w15 = Sigma1(w13) + w15 + w8 + Sigma0(w0);
                Round(b, c, d, ref e, f, g, h, ref a, w15, k[31]);

                w0 = Sigma1(w14) + w0 + w9 + Sigma0(w1);
                Round(a, b, c, ref d, e, f, g, ref h, w0, k[32]);
                w1 = Sigma1(w15) + w1 + w10 + Sigma0(w2);
                Round(h, a, b, ref c, d, e, f, ref g, w1, k[33]);
                w2 = Sigma1(w0) + w2 + w11 + Sigma0(w3);
                Round(g, h, a, ref b, c, d, e, ref f, w2, k[34]);
                w3 = Sigma1(w1) + w3 + w12 + Sigma0(w4);
                Round(f, g, h, ref a, b, c, d, ref e, w3, k[35]);
                w4 = Sigma1(w2) + w4 + w13 + Sigma0(w5);
                Round(e, f, g, ref h, a, b, c, ref d, w4, k[36]);
                w5 = Sigma1(w3) + w5 + w14 + Sigma0(w6);
                Round(d, e, f, ref g, h, a, b, ref c, w5, k[37]);
                w6 = Sigma1(w4) + w6 + w15 + Sigma0(w7);
                Round(c, d, e, ref f, g, h, a, ref b, w6, k[38]);
                w7 = Sigma1(w5) + w7 + w0 + Sigma0(w8);
                Round(b, c, d, ref e, f, g, h, ref a, w7, k[39]);

                w8 = Sigma1(w6) + w8 + w1 + Sigma0(w9);
                Round(a, b, c, ref d, e, f, g, ref h, w8, k[40]);
                w9 = Sigma1(w7) + w9 + w2 + Sigma0(w10);
                Round(h, a, b, ref c, d, e, f, ref g, w9, k[41]);
                w10 = Sigma1(w8) + w10 + w3 + Sigma0(w11);
                Round(g, h, a, ref b, c, d, e, ref f, w10, k[42]);
                w11 = Sigma1(w9) + w11 + w4 + Sigma0(w12);
                Round(f, g, h, ref a, b, c, d, ref e, w11, k[43]);
                w12 = Sigma1(w10) + w12 + w5 + Sigma0(w13);
                Round(e, f, g, ref h, a, b, c, ref d, w12, k[44]);
                w13 = Sigma1(w11) + w13 + w6 + Sigma0(w14);
                Round(d, e, f, ref g, h, a, b, ref c, w13, k[45]);
                w14 = Sigma1(w12) + w14 + w7 + Sigma0(w15);
                Round(c, d, e, ref f, g, h, a, ref b, w14, k[46]);
                w15 = Sigma1(w13) + w15 + w8 + Sigma0(w0);
                Round(b, c, d, ref e, f, g, h, ref a, w15, k[47]);

                w0 = Sigma1(w14) + w0 + w9 + Sigma0(w1);
                Round(a, b, c, ref d, e, f, g, ref h, w0, k[48]);
                w1 = Sigma1(w15) + w1 + w10 + Sigma0(w2);
                Round(h, a, b, ref c, d, e, f, ref g, w1, k[49]);
                w2 = Sigma1(w0) + w2 + w11 + Sigma0(w3);
                Round(g, h, a, ref b, c, d, e, ref f, w2, k[50]);
                w3 = Sigma1(w1) + w3 + w12 + Sigma0(w4);
                Round(f, g, h, ref a, b, c, d, ref e, w3, k[51]);
                w4 = Sigma1(w2) + w4 + w13 + Sigma0(w5);
                Round(e, f, g, ref h, a, b, c, ref d, w4, k[52]);
                w5 = Sigma1(w3) + w5 + w14 + Sigma0(w6);
                Round(d, e, f, ref g, h, a, b, ref c, w5, k[53]);
                w6 = Sigma1(w4) + w6 + w15 + Sigma0(w7);
                Round(c, d, e, ref f, g, h, a, ref b, w6, k[54]);
                w7 = Sigma1(w5) + w7 + w0 + Sigma0(w8);
                Round(b, c, d, ref e, f, g, h, ref a, w7, k[55]);

                w8 = Sigma1(w6) + w8 + w1 + Sigma0(w9);
                Round(a, b, c, ref d, e, f, g, ref h, w8, k[56]);
                w9 = Sigma1(w7) + w9 + w2 + Sigma0(w10);
                Round(h, a, b, ref c, d, e, f, ref g, w9, k[57]);
                w10 = Sigma1(w8) + w10 + w3 + Sigma0(w11);
                Round(g, h, a, ref b, c, d, e, ref f, w10, k[58]);
                w11 = Sigma1(w9) + w11 + w4 + Sigma0(w12);
                Round(f, g, h, ref a, b, c, d, ref e, w11, k[59]);
                w12 = Sigma1(w10) + w12 + w5 + Sigma0(w13);
                Round(e, f, g, ref h, a, b, c, ref d, w12, k[60]);
                w13 = Sigma1(w11) + w13 + w6 + Sigma0(w14);
                Round(d, e, f, ref g, h, a, b, ref c, w13, k[61]);
                w14 = Sigma1(w12) + w14 + w7 + Sigma0(w15);
                Round(c, d, e, ref f, g, h, a, ref b, w14, k[62]);
                w15 = Sigma1(w13) + w15 + w8 + Sigma0(w0);
                Round(b, c, d, ref e, f, g, h, ref a, w15, k[63]);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint RotateRight(uint value, byte offset)
                => (value >> offset) | (value << (32 - offset)); // BitOperations.RotateRight?

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint BigSigma0(uint a)
                => RotateRight(RotateRight(RotateRight(a, 9) ^ a, 11) ^ a, 2);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint BigSigma1(uint e)
                  => RotateRight(RotateRight(RotateRight(e, 14) ^ e, 5) ^ e, 6);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint Sigma1(uint x)
                => RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint Sigma0(uint x)
                => RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >> 3);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint Ch(uint x, uint y, uint z)
                => (x & y) ^ (z & ~x);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static uint Maj(uint x, uint y, uint z)
                => (x & y) ^ (x & z) ^ (y & z);

         //   [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            private static void Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint w, uint k)
            {
                uint t1 = h + BigSigma1(e) + Ch(e, f, g) + k + w;
                uint t2 = BigSigma0(a) + Maj(a, b, c);
                d += t1;
                h = t1 + t2;
            }

            // 3, 2, 1, 0, 7, 6, 5, 4,
            // 11, 10, 9, 8, 15, 14, 13, 12,
            // 19, 18, 17, 16, 23, 22, 21, 20,
            // 27, 26, 25, 24, 31, 30, 29, 28
            private static Vector256<byte> _shuffleMask256 = Vector256.Create(
                    289644378169868803,
                    868365760874482187,
                    1447087143579095571,
                    2025808526283708955
                    ).AsByte();

            // 3, 2, 1, 0, 7, 6, 5, 4,
            // 11, 10, 9, 8, 15, 14, 13, 12
            private static Vector128<byte> _shuffleMask128 = Vector128.Create(
                    289644378169868803,
                    868365760874482187
                    ).AsByte();

            private static readonly uint[] k = {
            0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
            0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
            0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
            0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
            0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
            0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
            0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
            0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2
        };
        }
    }

}

