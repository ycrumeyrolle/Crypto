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
    public class Sha256StructWolf
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
                digest = new[]
                {
                    0x6A09E667u,
                    0xBB67AE85u,
                    0x3C6EF372u,
                    0xA54FF53Au,
                    0x510E527Fu,
                    0x9B05688Cu,
                    0x1F83D9ABu,
                    0x5BE0CD19u
                },
                w = stackalloc uint[64],
                S = new uint[8]
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
            Unsafe.InitBlockUnaligned(ref Unsafe.Add(ref lastBlockRef, remaining + 1), 0, (uint)(64 - remaining - 1));
            if (remaining >= 56)
            {
                state.Transform(ref lastBlockRef);
                Unsafe.InitBlockUnaligned(ref lastBlockRef, 0, 56u);
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, 56), BinaryPrimitives.ReverseEndianness(bitLength));
            state.Transform(ref lastBlockRef);

            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
            //if (Avx2.IsSupported)
            //{
            //    Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(state.vector.AsByte(), _shuffleMask256));
            //}
            //else if (Ssse3.IsSupported)
            //{
            //    Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(state.vector.GetLower().AsByte(), _shuffleMask128));
            //    Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(state.vector.GetUpper().AsByte(), _shuffleMask128));
            //}
            //else
            //{
            for (int j = 0; j < 4; ++j)
            {
                destination[j] = (byte)((state.digest[0] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 4] = (byte)((state.digest[1] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 8] = (byte)((state.digest[2] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 12] = (byte)((state.digest[3] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 16] = (byte)((state.digest[4] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 20] = (byte)((state.digest[5] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 24] = (byte)((state.digest[6] >> (24 - j * 8)) & 0x000000ff);
                destination[j + 28] = (byte)((state.digest[7] >> (24 - j * 8)) & 0x000000ff);
            }
            //}
        }


        public ref struct State
        {
            public uint[] S;

            public uint[] digest;

            public Span<uint> w;

            private ref uint W => ref MemoryMarshal.GetReference(w);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Transform(ref byte currentBlock)
            {
                S[0] = digest[0];
                S[1] = digest[1];
                S[2] = digest[2];
                S[3] = digest[3];
                S[4] = digest[4];
                S[5] = digest[5];
                S[6] = digest[6];
                S[7] = digest[7];

                //ref byte wRef = ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(w));

                //if (Avx2.IsSupported)
                //{
                //    Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask256));
                //    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask256));
                //}
                //else if (Ssse3.IsSupported)
                //{
                //    Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask128));
                //    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), _shuffleMask128));
                //    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask128));
                //    Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), _shuffleMask128));
                //}
                //else
                //{
                //    for (int i = 0, j = 0; i < 16; ++i, j += 4)
                //    {
                //        w[i] = (uint)((Unsafe.Add(ref currentBlock, j) << 24) | (Unsafe.Add(ref currentBlock, j + 1) << 16) | (Unsafe.Add(ref currentBlock, j + 2) << 8) | (Unsafe.Add(ref currentBlock, j + 3)));
                //    }
                //}

                Round1(0, ref currentBlock); Round1(1, ref currentBlock); Round1(2, ref currentBlock); Round1(3, ref currentBlock);
                Round1(4, ref currentBlock); Round1(5, ref currentBlock); Round1(6, ref currentBlock); Round1(7, ref currentBlock);
                Round1(8, ref currentBlock); Round1(9, ref currentBlock); Round1(10, ref currentBlock); Round1(11, ref currentBlock);
                Round1(12, ref currentBlock); Round1(13, ref currentBlock); Round1(14, ref currentBlock); Round1(15, ref currentBlock);

                //Rounds();
                for (int i = 16; i < 64; i += 16)
                {
                    RoundN(i, 0); RoundN(i, 1); RoundN(i, 2); RoundN(i, 3);
                    RoundN(i, 4); RoundN(i, 5); RoundN(i, 6); RoundN(i, 7);
                    RoundN(i, 8); RoundN(i, 9); RoundN(i, 10); RoundN(i, 11);
                    RoundN(i, 12); RoundN(i, 13); RoundN(i, 14); RoundN(i, 15);
                }

                digest[0] += S[0];
                digest[1] += S[1];
                digest[2] += S[2];
                digest[3] += S[3];
                digest[4] += S[4];
                digest[5] += S[5];
                digest[6] += S[6];
                digest[7] += S[7];
            }

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint SCHED1(int j, ref byte data) => Unsafe.Add(ref W, j) = Unsafe.As<byte, uint>(ref Unsafe.Add(ref data, j * 4));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint SCHED(int j)
                => Unsafe.Add(ref W, j & 15) += Sha256Helper.Sigma1(Unsafe.Add(ref W, (j - 2) & 15)) + Unsafe.Add(ref W, (j - 7) & 15) + Sha256Helper.Sigma0(Unsafe.Add(ref W, (j - 15) & 15));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint a(int i) => S[(0 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint b(int i) => S[(1 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint c(int i) => S[(2 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint d(int i) => S[(3 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint e(int i) => S[(4 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint f(int i) => S[(5 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint g(int i) => S[(6 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint h(int i) => S[(7 - i) & 7];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void d(int i, uint value) => S[(3 - i) & 7] += value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void h(int i, uint value) => S[(7 - i) & 7] = value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Round1(/*int i, */int j, ref byte data)
            {
                uint t0 = h(j) + Sha256Helper.BigSigma1(e(j)) + Sha256Helper.Ch(e(j), f(j), g(j)) + k[/*i +*/ j] + SCHED1(j, ref data);
                uint t1 = Sha256Helper.BigSigma0(a(j)) + Sha256Helper.Maj(a(j), b(j), c(j));
                d(j, t0);
                h(j, t0 + t1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void RoundN(int i, int j)
            {
                uint t0 = h(j) + Sha256Helper.BigSigma1(e(j)) + Sha256Helper.Ch(e(j), f(j), g(j)) + k[i + j] + SCHED(j);
                uint t1 = Sha256Helper.BigSigma0(a(j)) + Sha256Helper.Maj(a(j), b(j), c(j));
                d(j, t0);
                h(j, t0 + t1);
            }
        }
    }
}