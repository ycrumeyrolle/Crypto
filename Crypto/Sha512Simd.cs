#if NETCOREAPP3_0
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Crypto
{
    public class Sha512Simd
    {
        private static Vector256<long> GatherMask = Vector256.Create(0L, 16, 32, 48);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe Vector256<ulong> Gather(ref byte address)
        {
            return Avx2.GatherVector256((ulong*)Unsafe.AsPointer(ref address), GatherMask, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<ulong> Sigma0(Vector256<ulong> W)
        {
            return Avx2.Xor(Avx2.Xor(Avx2.Xor(Avx2.ShiftRightLogical(W, 7), Avx2.ShiftRightLogical(W, 8)), Avx2.Xor(Avx2.ShiftRightLogical(W, 1), Avx2.ShiftLeftLogical(W, 56))), Avx2.ShiftLeftLogical(W, 63));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<ulong> Sigma1(Vector256<ulong> W)
        {
            return Avx2.Xor(Avx2.Xor(Avx2.Xor(Avx2.ShiftRightLogical(W, 6), Avx2.ShiftRightLogical(W, 61)), Avx2.Xor(Avx2.ShiftRightLogical(W, 19), Avx2.ShiftLeftLogical(W, 3))), Avx2.ShiftLeftLogical(W, 45));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<ulong> Schedule(Vector256<ulong> w0, Vector256<ulong> w1, Vector256<ulong> w9, Vector256<ulong> w14, int i, ref Vector256<ulong> schedule)
        {
            //Avx2.BroadcastScalarToVector256()
            Unsafe.Add(ref schedule, i) = Avx2.Add(w0, Unsafe.Add(ref K, i));
            return Avx2.Add(Avx2.Add(w0, w9), Avx2.Add(Sigma0(w1), Sigma1(w14)));
        }

        public void Schedule(ref Vector256<ulong> schedule, ref byte message)
        {
            int i = 0;
            Vector256<ulong> W0, W1, W2, W3, W4, W5, W6, W7, W8, W9, W10, W11, W12, W13, W14, W15;
            W0 = Avx2.Shuffle(Gather(ref message).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W1 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 1)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W2 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 2)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W3 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 3)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W4 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 4)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W5 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 5)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W6 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 6)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W7 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 7)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W8 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 8)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W9 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 9)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W10 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 10)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W11 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 11)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W12 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 12)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W13 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 13)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W14 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 14)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            W15 = Avx2.Shuffle(Gather(ref Unsafe.Add(ref message, 8 * 15)).AsByte(), Sha512Helper.LittleEndianMask256).AsUInt64();
            while (i < 64)
            {
                W0 = Schedule(W0, W1, W9, W14, i++, ref schedule);
                W1 = Schedule(W1, W2, W10, W15, i++, ref schedule);
                W2 = Schedule(W2, W3, W11, W0, i++, ref schedule);
                W3 = Schedule(W3, W4, W12, W1, i++, ref schedule);
                W4 = Schedule(W4, W5, W13, W2, i++, ref schedule);
                W5 = Schedule(W5, W6, W14, W3, i++, ref schedule);
                W6 = Schedule(W6, W7, W15, W4, i++, ref schedule);
                W7 = Schedule(W7, W8, W0, W5, i++, ref schedule);
                W8 = Schedule(W8, W9, W1, W6, i++, ref schedule);
                W9 = Schedule(W9, W10, W2, W7, i++, ref schedule);
                W10 = Schedule(W10, W11, W3, W8, i++, ref schedule);
                W11 = Schedule(W11, W12, W4, W9, i++, ref schedule);
                W12 = Schedule(W12, W13, W5, W10, i++, ref schedule);
                W13 = Schedule(W13, W14, W6, W11, i++, ref schedule);
                W14 = Schedule(W14, W15, W7, W12, i++, ref schedule);
                W15 = Schedule(W15, W0, W8, W13, i++, ref schedule);
            }

            Unsafe.Add(ref schedule, 64) = Avx2.Add(W0, Unsafe.Add(ref K,  64));
            Unsafe.Add(ref schedule, 65) = Avx2.Add(W1, Unsafe.Add(ref K,  65));
            Unsafe.Add(ref schedule, 66) = Avx2.Add(W2, Unsafe.Add(ref K,  66));
            Unsafe.Add(ref schedule, 67) = Avx2.Add(W3, Unsafe.Add(ref K,  67));
            Unsafe.Add(ref schedule, 68) = Avx2.Add(W4, Unsafe.Add(ref K,  68));
            Unsafe.Add(ref schedule, 69) = Avx2.Add(W5, Unsafe.Add(ref K,  69));
            Unsafe.Add(ref schedule, 70) = Avx2.Add(W6, Unsafe.Add(ref K,  70));
            Unsafe.Add(ref schedule, 71) = Avx2.Add(W7, Unsafe.Add(ref K,  71));
            Unsafe.Add(ref schedule, 72) = Avx2.Add(W8, Unsafe.Add(ref K,  72));
            Unsafe.Add(ref schedule, 73) = Avx2.Add(W9, Unsafe.Add(ref K,  73));
            Unsafe.Add(ref schedule, 74) = Avx2.Add(W10, Unsafe.Add(ref K, 74));
            Unsafe.Add(ref schedule, 75) = Avx2.Add(W11, Unsafe.Add(ref K, 75));
            Unsafe.Add(ref schedule, 76) = Avx2.Add(W12, Unsafe.Add(ref K, 76));
            Unsafe.Add(ref schedule, 77) = Avx2.Add(W13, Unsafe.Add(ref K, 77));
            Unsafe.Add(ref schedule, 78) = Avx2.Add(W14, Unsafe.Add(ref K, 78));
            Unsafe.Add(ref schedule, 79) = Avx2.Add(W15, Unsafe.Add(ref K, 79));
        }

        private static ref Vector256<ulong> K => ref k[0];

        private static readonly Vector256<ulong>[] k = {
                Vector256.Create(0x428a2f98d728ae22ul), Vector256.Create(0x7137449123ef65cdul), Vector256.Create(0xb5c0fbcfec4d3b2ful), Vector256.Create(0xe9b5dba58189dbbcul),
                Vector256.Create(0x3956c25bf348b538ul), Vector256.Create(0x59f111f1b605d019ul), Vector256.Create(0x923f82a4af194f9bul), Vector256.Create(0xab1c5ed5da6d8118ul),
                Vector256.Create(0xd807aa98a3030242ul), Vector256.Create(0x12835b0145706fbeul), Vector256.Create(0x243185be4ee4b28cul), Vector256.Create(0x550c7dc3d5ffb4e2ul),
                Vector256.Create(0x72be5d74f27b896ful), Vector256.Create(0x80deb1fe3b1696b1ul), Vector256.Create(0x9bdc06a725c71235ul), Vector256.Create(0xc19bf174cf692694ul),
                Vector256.Create(0xe49b69c19ef14ad2ul), Vector256.Create(0xefbe4786384f25e3ul), Vector256.Create(0x0fc19dc68b8cd5b5ul), Vector256.Create(0x240ca1cc77ac9c65ul),
                Vector256.Create(0x2de92c6f592b0275ul), Vector256.Create(0x4a7484aa6ea6e483ul), Vector256.Create(0x5cb0a9dcbd41fbd4ul), Vector256.Create(0x76f988da831153b5ul),
                Vector256.Create(0x983e5152ee66dfabul), Vector256.Create(0xa831c66d2db43210ul), Vector256.Create(0xb00327c898fb213ful), Vector256.Create(0xbf597fc7beef0ee4ul),
                Vector256.Create(0xc6e00bf33da88fc2ul), Vector256.Create(0xd5a79147930aa725ul), Vector256.Create(0x06ca6351e003826ful), Vector256.Create(0x142929670a0e6e70ul),
                Vector256.Create(0x27b70a8546d22ffcul), Vector256.Create(0x2e1b21385c26c926ul), Vector256.Create(0x4d2c6dfc5ac42aedul), Vector256.Create(0x53380d139d95b3dful),
                Vector256.Create(0x650a73548baf63deul), Vector256.Create(0x766a0abb3c77b2a8ul), Vector256.Create(0x81c2c92e47edaee6ul), Vector256.Create(0x92722c851482353bul),
                Vector256.Create(0xa2bfe8a14cf10364ul), Vector256.Create(0xa81a664bbc423001ul), Vector256.Create(0xc24b8b70d0f89791ul), Vector256.Create(0xc76c51a30654be30ul),
                Vector256.Create(0xd192e819d6ef5218ul), Vector256.Create(0xd69906245565a910ul), Vector256.Create(0xf40e35855771202aul), Vector256.Create(0x106aa07032bbd1b8ul),
                Vector256.Create(0x19a4c116b8d2d0c8ul), Vector256.Create(0x1e376c085141ab53ul), Vector256.Create(0x2748774cdf8eeb99ul), Vector256.Create(0x34b0bcb5e19b48a8ul),
                Vector256.Create(0x391c0cb3c5c95a63ul), Vector256.Create(0x4ed8aa4ae3418acbul), Vector256.Create(0x5b9cca4f7763e373ul), Vector256.Create(0x682e6ff3d6b2b8a3ul),
                Vector256.Create(0x748f82ee5defb2fcul), Vector256.Create(0x78a5636f43172f60ul), Vector256.Create(0x84c87814a1f0ab72ul), Vector256.Create(0x8cc702081a6439ecul),
                Vector256.Create(0x90befffa23631e28ul), Vector256.Create(0xa4506cebde82bde9ul), Vector256.Create(0xbef9a3f7b2c67915ul), Vector256.Create(0xc67178f2e372532bul),
                Vector256.Create(0xca273eceea26619cul), Vector256.Create(0xd186b8c721c0c207ul), Vector256.Create(0xeada7dd6cde0eb1eul), Vector256.Create(0xf57d4f7fee6ed178ul),
                Vector256.Create(0x06f067aa72176fbaul), Vector256.Create(0x0a637dc5a2c898a6ul), Vector256.Create(0x113f9804bef90daeul), Vector256.Create(0x1b710b35131c471bul),
                Vector256.Create(0x28db77f523047d84ul), Vector256.Create(0x32caab7b40c72493ul), Vector256.Create(0x3c9ebe0a15c9bebcul), Vector256.Create(0x431d67c49c100d4cul),
                Vector256.Create(0x4cc5d4becb3e42b6ul), Vector256.Create(0x597f299cfc657e2aul), Vector256.Create(0x5fcb6fab3ad6faecul), Vector256.Create(0x6c44198c4a475817ul)
        };

        // 3, 2, 1, 0, 7, 6, 5, 4,
        // 11, 10, 9, 8, 15, 14, 13, 12,
        // 19, 18, 17, 16, 23, 22, 21, 20,
        // 27, 26, 25, 24, 31, 30, 29, 28
        internal static Vector256<byte> LittleEndianMask256 = Vector256.Create(
                    283686952306183,
                    579005069656919567,
                    1157726452361532951,
                    1736447835066146335
                ).AsByte();

        // 3, 2, 1, 0, 7, 6, 5, 4,
        // 11, 10, 9, 8, 15, 14, 13, 12
        internal static Vector128<byte> LittleEndianMask128 = Vector128.Create(
                    283686952306183,
                    579005069656919567
                ).AsByte();
        private void Transform(ref ulong state, ref byte currentBlock, ref Vector256<ulong> w)
        {
            ulong a, b, c, d, e, f, g, h;
            Schedule(ref w, ref currentBlock);
            for (int j = 0; j < 4; j++)
            {
                a = state;
                b = Unsafe.Add(ref state, 1);
                c = Unsafe.Add(ref state, 2);
                d = Unsafe.Add(ref state, 3);
                e = Unsafe.Add(ref state, 4);
                f = Unsafe.Add(ref state, 5);
                g = Unsafe.Add(ref state, 6);
                h = Unsafe.Add(ref state, 7);
                for (int t = 0; t < 80; t += 8)
                {
                    Round(a, b, c, ref d, e, f, g, ref h, Unsafe.Add(ref w, t).GetElement(j));
                    Round(h, a, b, ref c, d, e, f, ref g, Unsafe.Add(ref w, t + 1).GetElement(j));
                    Round(g, h, a, ref b, c, d, e, ref f, Unsafe.Add(ref w, t + 2).GetElement(j));
                    Round(f, g, h, ref a, b, c, d, ref e, Unsafe.Add(ref w, t + 3).GetElement(j));
                    Round(e, f, g, ref h, a, b, c, ref d, Unsafe.Add(ref w, t + 4).GetElement(j));
                    Round(d, e, f, ref g, h, a, b, ref c, Unsafe.Add(ref w, t + 5).GetElement(j));
                    Round(c, d, e, ref f, g, h, a, ref b, Unsafe.Add(ref w, t + 6).GetElement(j));
                    Round(b, c, d, ref e, f, g, h, ref a, Unsafe.Add(ref w, t + 7).GetElement(j));
                }

                state += a;
                Unsafe.Add(ref state, 1) += b;
                Unsafe.Add(ref state, 2) += c;
                Unsafe.Add(ref state, 3) += d;
                Unsafe.Add(ref state, 4) += e;
                Unsafe.Add(ref state, 5) += f;
                Unsafe.Add(ref state, 6) += g;
                Unsafe.Add(ref state, 7) += h;
            }
        }

        private void Transform(ref ulong state, ref byte currentBlock, ref ulong w)
        {
#if NETCOREAPP3_0
            ref byte wRef = ref Unsafe.As<ulong, byte>(ref w);
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref currentBlock), Sha512Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha512Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 64), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 64)), Sha512Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 96), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 96)), Sha512Helper.LittleEndianMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref currentBlock), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 64), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 64)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 80), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 80)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 96), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 96)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 112), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 112)), Sha512Helper.LittleEndianMask128));
            }
            else
#endif
            {
                for (int i = 0, j = 0; i < 16; ++i, j += 8)
                {
                    Unsafe.Add(ref w, i) = BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref currentBlock, j)));
                }
            }

            ref ulong w0 = ref Unsafe.Add(ref w, 16);
            ref ulong wEnd = ref Unsafe.Add(ref w, 80);
            while (Unsafe.IsAddressLessThan(ref w0, ref wEnd))
            {
                w0 = Unsafe.Subtract(ref w0, 16) + Sha512Helper.Sigma0(Unsafe.Subtract(ref w0, 15)) + Unsafe.Subtract(ref w0, 7) + Sha512Helper.Sigma1(Unsafe.Subtract(ref w0, 2));
                Unsafe.Add(ref w0, 1) = Unsafe.Subtract(ref w0, 15) + Sha512Helper.Sigma0(Unsafe.Subtract(ref w0, 14)) + Unsafe.Subtract(ref w0, 6) + Sha512Helper.Sigma1(Unsafe.Subtract(ref w0, 1));
                w0 = ref Unsafe.Add(ref w0, 2);
            }

            ulong a = state;
            ulong b = Unsafe.Add(ref state, 1);
            ulong c = Unsafe.Add(ref state, 2);
            ulong d = Unsafe.Add(ref state, 3);
            ulong e = Unsafe.Add(ref state, 4);
            ulong f = Unsafe.Add(ref state, 5);
            ulong g = Unsafe.Add(ref state, 6);
            ulong h = Unsafe.Add(ref state, 7);
            for (int i = 0; i < 80; i += 8)
            {
                Round(a, b, c, ref d, e, f, g, ref h, Unsafe.Add(ref w, i), Sha512Helper.K[i]);
                Round(h, a, b, ref c, d, e, f, ref g, Unsafe.Add(ref w, i + 1), Sha512Helper.K[i + 1]);
                Round(g, h, a, ref b, c, d, e, ref f, Unsafe.Add(ref w, i + 2), Sha512Helper.K[i + 2]);
                Round(f, g, h, ref a, b, c, d, ref e, Unsafe.Add(ref w, i + 3), Sha512Helper.K[i + 3]);
                Round(e, f, g, ref h, a, b, c, ref d, Unsafe.Add(ref w, i + 4), Sha512Helper.K[i + 4]);
                Round(d, e, f, ref g, h, a, b, ref c, Unsafe.Add(ref w, i + 5), Sha512Helper.K[i + 5]);
                Round(c, d, e, ref f, g, h, a, ref b, Unsafe.Add(ref w, i + 6), Sha512Helper.K[i + 6]);
                Round(b, c, d, ref e, f, g, h, ref a, Unsafe.Add(ref w, i + 7), Sha512Helper.K[i + 7]);
            }

            state += a;
            Unsafe.Add(ref state, 1) += b;
            Unsafe.Add(ref state, 2) += c;
            Unsafe.Add(ref state, 3) += d;
            Unsafe.Add(ref state, 4) += e;
            Unsafe.Add(ref state, 5) += f;
            Unsafe.Add(ref state, 6) += g;
            Unsafe.Add(ref state, 7) += h;
        }

        public void ComputeHash(ReadOnlySpan<byte> src, Span<byte> destination, ReadOnlySpan<byte> prepend = default)
        {
            const int BlockSize = 128;
            Debug.Assert(destination.Length == 64);
            Debug.Assert(prepend.IsEmpty || prepend.Length == BlockSize);

            Span<ulong> state = stackalloc ulong[] {
                0x6a09e667f3bcc908ul,
                0xbb67ae8584caa73bul,
                0x3c6ef372fe94f82bul,
                0xa54ff53a5f1d36f1ul,
                0x510e527fade682d1ul,
                0x9b05688c2b3e6c1ful,
                0x1f83d9abfb41bd6bul,
                0x5be0cd19137e2179ul
            };
            Span<ulong> w2 = stackalloc ulong[80];
            ref ulong stateRef = ref MemoryMarshal.GetReference(state);
            if (!prepend.IsEmpty)
            {
                Debug.Assert(prepend.Length == BlockSize);
                Transform(ref stateRef, ref MemoryMarshal.GetReference(prepend), ref MemoryMarshal.GetReference(w2));
            }

            ref byte srcRef = ref MemoryMarshal.GetReference(src);
            ref byte srcEndRef = ref Unsafe.Add(ref srcRef, src.Length - BlockSize + 1);
            ref byte srcSimdEndRef = ref Unsafe.Add(ref srcRef, src.Length - 4 * BlockSize + 1);
            if (Unsafe.IsAddressLessThan(ref srcRef, ref srcSimdEndRef))
            {
                //Span<Vector256<ulong>> w = stackalloc Vector256<ulong>[80];
                //ref Vector256<ulong> wRef = ref MemoryMarshal.GetReference(w);
                //do
                //{
                //    Transform(ref stateRef, ref srcRef, ref wRef);
                //    srcRef = ref Unsafe.Add(ref srcRef, BlockSize * 4);
                //} while (Unsafe.IsAddressLessThan(ref srcRef, ref srcSimdEndRef));

                Vector256<ulong>[] returnToPool;
                Span<Vector256<ulong>> w = (returnToPool = ArrayPool<Vector256<ulong>>.Shared.Rent(80));
                try
                {
                    ref Vector256<ulong> wRef = ref MemoryMarshal.GetReference(w);
                    do
                    {
                        Transform(ref stateRef, ref srcRef, ref wRef);
                        srcRef = ref Unsafe.Add(ref srcRef, BlockSize * 4);
                    } while (Unsafe.IsAddressLessThan(ref srcRef, ref srcSimdEndRef));
                }
                finally
                {
                    ArrayPool<Vector256<ulong>>.Shared.Return(returnToPool);
                }
            }

            while (Unsafe.IsAddressLessThan(ref srcRef, ref srcEndRef))
            {
                Transform(ref stateRef, ref srcRef, ref MemoryMarshal.GetReference(w2));
                srcRef = ref Unsafe.Add(ref srcRef, BlockSize);
            }

            int dataLength = src.Length + prepend.Length;
            int remaining = dataLength & (BlockSize - 1);

            Span<byte> lastBlock = stackalloc byte[BlockSize];
            ref byte lastBlockRef = ref MemoryMarshal.GetReference(lastBlock);
            Unsafe.CopyBlockUnaligned(ref lastBlockRef, ref srcRef, (uint)remaining);

            // Pad the last block
            Unsafe.Add(ref lastBlockRef, remaining) = 0x80;
            lastBlock.Slice(remaining + 1).Clear();
            if (remaining >= BlockSize - 2 * sizeof(ulong))
            {
                Transform(ref stateRef, ref lastBlockRef, ref MemoryMarshal.GetReference(w2));
                lastBlock.Slice(0, BlockSize - 2 * sizeof(ulong)).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - 16), 0ul); // Don't support input length > 2^64
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - 8), BinaryPrimitives.ReverseEndianness(bitLength));
            Transform(ref stateRef, ref lastBlockRef, ref MemoryMarshal.GetReference(w2));

            // reverse all the bytes when copying the final state to the output hash.
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
#if NETCOREAPP3_0
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.As<ulong, byte>(ref stateRef)), Sha512Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Avx2.Shuffle(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 32)), Sha512Helper.LittleEndianMask256));
            }
            else
            if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.As<ulong, byte>(ref stateRef)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 16)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 32)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 48), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 48)), Sha512Helper.LittleEndianMask128));
            }
            else
#endif
            {
                Unsafe.WriteUnaligned(ref destinationRef, BinaryPrimitives.ReverseEndianness(stateRef));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 8), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 1)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 2)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 24), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 3)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 4)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 40), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 5)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 48), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 6)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 56), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 7)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Round(ulong a, ulong b, ulong c, ref ulong d, ulong e, ulong f, ulong g, ref ulong h, ulong w)
        {
            h += Sha512Helper.BigSigma1(e) + Sha512Helper.Ch(e, f, g) + w;
            d += h;
            h += Sha512Helper.BigSigma0(a) + Sha512Helper.Maj(a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Round(ulong a, ulong b, ulong c, ref ulong d, ulong e, ulong f, ulong g, ref ulong h, ulong w, ulong k)
        {
            h += Sha512Helper.BigSigma1(e) + Sha512Helper.Ch(e, f, g) + k + w;
            d += h;
            h += Sha512Helper.BigSigma0(a) + Sha512Helper.Maj(a, b, c);
        }
    }
}
#endif