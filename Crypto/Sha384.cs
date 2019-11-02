using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Crypto
{
    public class Sha384
    {
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
                w0 = ref Unsafe.Add(ref w0, 1);
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
            Debug.Assert(destination.Length == 48);

            Span<ulong> W = stackalloc ulong[80];
            ref ulong w = ref MemoryMarshal.GetReference(W);
            Span<ulong> state = stackalloc ulong[] {
                0xcbbb9d5dc1059ed8ul,
                0x629a292a367cd507ul,
                0x9159015a3070dd17ul,
                0x152fecd8f70e5939ul,
                0x67332667ffc00b31ul,
                0x8eb44a8768581511ul,
                0xdb0c2e0d64f98fa7ul,
                0x47b5481dbefa4fa4ul,
            };
            ref ulong stateRef = ref MemoryMarshal.GetReference(state);
            if (!prepend.IsEmpty)
            {
                Debug.Assert(prepend.Length == BlockSize);
                Transform(ref stateRef, ref MemoryMarshal.GetReference(prepend), ref w);
            }

            ref byte srcRef = ref MemoryMarshal.GetReference(src);
            ref byte srcEndRef = ref Unsafe.Add(ref srcRef, src.Length - BlockSize + 1);
            while (Unsafe.IsAddressLessThan(ref srcRef, ref srcEndRef))
            {
                Transform(ref stateRef, ref srcRef, ref w);
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
            if (remaining >= BlockSize - 16)
            {
                Transform(ref stateRef, ref lastBlockRef, ref w);
                lastBlock.Slice(0, BlockSize - 16).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - 16), 0ul); // Don't support input length > 2^64
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - 8), BinaryPrimitives.ReverseEndianness(bitLength));
            Transform(ref stateRef, ref lastBlockRef, ref w);

            // reverse all the bytes when copying the final state to the output hash.
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
#if NETCOREAPP3_0
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.As<ulong, byte>(ref stateRef)), Sha512Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 32)), Sha512Helper.LittleEndianMask128));
            }
            else
            if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.As<ulong, byte>(ref stateRef)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 16)), Sha512Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<ulong, byte>(ref stateRef), 32)), Sha512Helper.LittleEndianMask128));
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
            }
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

