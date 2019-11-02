using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Crypto;

namespace Sha256Tests
{
    public class Sha256Struct
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
            State state = new State
            {
                vector = _initialState,
                w = stackalloc uint[64]
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

            // Pad the last block
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

        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public ref struct State
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
            public Span<uint> w;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Transform(ref byte currentBlock)
            {
                Vector256<uint> state = vector;
                ref byte wRef = ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(w));

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
                    for (int i = 0, j = 0; i < 16; ++i, j += 4)
                    {
                        w[i] = (uint)((Unsafe.Add(ref currentBlock, j) << 24) | (Unsafe.Add(ref currentBlock, j + 1) << 16) | (Unsafe.Add(ref currentBlock, j + 2) << 8) | (Unsafe.Add(ref currentBlock, j + 3)));
                    }
                }

                //Rounds();
                for (int i = 16; i < 64; i += 2)
                {
                    w[i] = w[i - 16] + Sha256Helper.Sigma0(w[i - 15]) + w[i - 7] + Sha256Helper.Sigma1(w[i - 2]);
                    w[i + 1] = w[i - 15] + Sha256Helper.Sigma0(w[i - 14]) + w[i - 6] + Sha256Helper.Sigma1(w[i - 1]);
                }

                for (int i = 0; i < 64; i += 8)
                {
                    Round(a, b, c, ref d, e, f, g, ref h, w[i + 0], k[i + 0]);
                    Round(h, a, b, ref c, d, e, f, ref g, w[i + 1], k[i + 1]);
                    Round(g, h, a, ref b, c, d, e, ref f, w[i + 2], k[i + 2]);
                    Round(f, g, h, ref a, b, c, d, ref e, w[i + 3], k[i + 3]);
                    Round(e, f, g, ref h, a, b, c, ref d, w[i + 4], k[i + 4]);
                    Round(d, e, f, ref g, h, a, b, ref c, w[i + 5], k[i + 5]);
                    Round(c, d, e, ref f, g, h, a, ref b, w[i + 6], k[i + 6]);
                    Round(b, c, d, ref e, f, g, h, ref a, w[i + 7], k[i + 7]);
                }

                vector = Avx2.Add(state, vector);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint w, uint k)
            {
                uint t1 = h + Sha256Helper.BigSigma1(e) + Sha256Helper.Ch(e, f, g) + k + w;
                uint t2 = Sha256Helper.BigSigma0(a) + Sha256Helper.Maj(a, b, c);
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

