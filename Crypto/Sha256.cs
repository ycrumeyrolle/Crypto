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
    public class Sha256
    {
        private void Transform(ref uint state, ref byte currentBlock, ref uint w)
        {
#if NETCOREAPP3_0
            ref byte wRef = ref Unsafe.As<uint, byte>(ref w);
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref currentBlock), Sha256Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha256Helper.LittleEndianMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref currentBlock), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), Sha256Helper.LittleEndianMask128));
            }
            else
#endif
            {
                for (int i = 0, j = 0; i < 16; ++i, j += 4)
                {
                    Unsafe.Add(ref w, i) = BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref currentBlock, j)));
                }
            }

            ref uint w0 = ref Unsafe.Add(ref w, 16);
            ref uint wEnd = ref Unsafe.Add(ref w, 64);
            while (Unsafe.IsAddressLessThan(ref w0, ref wEnd))
            {
                w0 = Unsafe.Subtract(ref w0, 16) + Sha256Helper.Sigma0(Unsafe.Subtract(ref w0, 15)) + Unsafe.Subtract(ref w0, 7) + Sha256Helper.Sigma1(Unsafe.Subtract(ref w0, 2));
                Unsafe.Add(ref w0, 1) = Unsafe.Subtract(ref w0, 15) + Sha256Helper.Sigma0(Unsafe.Subtract(ref w0, 14)) + Unsafe.Subtract(ref w0, 6) + Sha256Helper.Sigma1(Unsafe.Subtract(ref w0, 1));
                w0 = ref Unsafe.Add(ref w0, 2);
            }

            //Unsafe.Add(ref w, i) = Unsafe.Add(ref w, i - 16) + Sha256Helper.Sigma0(Unsafe.Add(ref w, i - 15)) + Unsafe.Add(ref w, i - 7) + Sha256Helper.Sigma1(Unsafe.Add(ref w, i - 2));
            //Unsafe.Add(ref w, i + 1) = Unsafe.Add(ref w, i - 15) + Sha256Helper.Sigma0(Unsafe.Add(ref w, i - 14)) + Unsafe.Add(ref w, i - 6) + Sha256Helper.Sigma1(Unsafe.Add(ref w, i - 1));

            uint a = state;
            uint b = Unsafe.Add(ref state, 1);
            uint c = Unsafe.Add(ref state, 2);
            uint d = Unsafe.Add(ref state, 3);
            uint e = Unsafe.Add(ref state, 4);
            uint f = Unsafe.Add(ref state, 5);
            uint g = Unsafe.Add(ref state, 6);
            uint h = Unsafe.Add(ref state, 7);

            for (int i = 0; i < 64; i += 8)
            {
                Round(a, b, c, ref d, e, f, g, ref h, Unsafe.Add(ref w, i), Sha256Helper.K[i]);
                Round(h, a, b, ref c, d, e, f, ref g, Unsafe.Add(ref w, i + 1), Sha256Helper.K[i + 1]);
                Round(g, h, a, ref b, c, d, e, ref f, Unsafe.Add(ref w, i + 2), Sha256Helper.K[i + 2]);
                Round(f, g, h, ref a, b, c, d, ref e, Unsafe.Add(ref w, i + 3), Sha256Helper.K[i + 3]);
                Round(e, f, g, ref h, a, b, c, ref d, Unsafe.Add(ref w, i + 4), Sha256Helper.K[i + 4]);
                Round(d, e, f, ref g, h, a, b, ref c, Unsafe.Add(ref w, i + 5), Sha256Helper.K[i + 5]);
                Round(c, d, e, ref f, g, h, a, ref b, Unsafe.Add(ref w, i + 6), Sha256Helper.K[i + 6]);
                Round(b, c, d, ref e, f, g, h, ref a, Unsafe.Add(ref w, i + 7), Sha256Helper.K[i + 7]);
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
            const int BlockSize = 64;
            Debug.Assert(destination.Length == 32);
            Debug.Assert(prepend.IsEmpty || prepend.Length == BlockSize);

            Span<uint> state = stackalloc uint[] {
                0x6a09e667,
                0xbb67ae85,
                0x3c6ef372,
                0xa54ff53a,
                0x510e527f,
                0x9b05688c,
                0x1f83d9ab,
                0x5be0cd19
            };
            Span<uint> W = stackalloc uint[64];
            ref uint w = ref MemoryMarshal.GetReference(W);
            ref uint stateRef = ref MemoryMarshal.GetReference(state);
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
            if (remaining >= BlockSize - sizeof(ulong))
            {
                Transform(ref stateRef, ref lastBlockRef, ref w);
                lastBlock.Slice(0, BlockSize - sizeof(ulong)).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - sizeof(ulong)), BinaryPrimitives.ReverseEndianness(bitLength));
            Transform(ref stateRef, ref lastBlockRef, ref w);

            // reverse all the bytes when copying the final state to the output hash.
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
#if NETCOREAPP3_0
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(state))), Sha256Helper.LittleEndianMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.As<uint, byte>(ref stateRef)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<uint, byte>(ref stateRef), 16)), Sha256Helper.LittleEndianMask128));
            }
            else
#endif
            {
                Unsafe.WriteUnaligned(ref destinationRef, BinaryPrimitives.ReverseEndianness(stateRef));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 4), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 1)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 8), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 2)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 12), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 3)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 4)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 20), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 5)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 24), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 6)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 28), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 7)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint w, uint k)
        {
            h += Sha256Helper.BigSigma1(e) + Sha256Helper.Ch(e, f, g) + k + w;
            d += h;
            h += Sha256Helper.BigSigma0(a) + Sha256Helper.Maj(a, b, c);
        }
    }

    public class Sha256X
    {
        private void Transform(ref uint state, ref byte currentBlock, ref uint w)
        {
#if NETCOREAPP3_0
            ref byte wRef = ref Unsafe.As<uint, byte>(ref w);
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref currentBlock), Sha256Helper.LittleEndianMask256));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha256Helper.LittleEndianMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref currentBlock), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), Sha256Helper.LittleEndianMask128));
            }
            else
#endif
            {
                for (int i = 0, j = 0; i < 16; ++i, j += 4)
                {
                    Unsafe.Add(ref w, i) = BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref currentBlock, j)));
                }
            }

            ref uint w0 = ref Unsafe.Add(ref w, 16);
            ref uint wEnd = ref Unsafe.Add(ref w, 64);
            while (Unsafe.IsAddressLessThan(ref w0, ref wEnd))
            {
                w0 = Unsafe.Subtract(ref w0, 16) + Sha256Helper.Sigma0(Unsafe.Subtract(ref w0, 15)) + Unsafe.Subtract(ref w0, 7) + Sha256Helper.Sigma1(Unsafe.Subtract(ref w0, 2));
                Unsafe.Add(ref w0, 1) = Unsafe.Subtract(ref w0, 15) + Sha256Helper.Sigma0(Unsafe.Subtract(ref w0, 14)) + Unsafe.Subtract(ref w0, 6) + Sha256Helper.Sigma1(Unsafe.Subtract(ref w0, 1));
                w0 = ref Unsafe.Add(ref w0, 2);
            }

            //Unsafe.Add(ref w, i) = Unsafe.Add(ref w, i - 16) + Sha256Helper.Sigma0(Unsafe.Add(ref w, i - 15)) + Unsafe.Add(ref w, i - 7) + Sha256Helper.Sigma1(Unsafe.Add(ref w, i - 2));
            //Unsafe.Add(ref w, i + 1) = Unsafe.Add(ref w, i - 15) + Sha256Helper.Sigma0(Unsafe.Add(ref w, i - 14)) + Unsafe.Add(ref w, i - 6) + Sha256Helper.Sigma1(Unsafe.Add(ref w, i - 1));

            uint a = state;
            uint b = Unsafe.Add(ref state, 1);
            uint c = Unsafe.Add(ref state, 2);
            uint d = Unsafe.Add(ref state, 3);
            uint e = Unsafe.Add(ref state, 4);
            uint f = Unsafe.Add(ref state, 5);
            uint g = Unsafe.Add(ref state, 6);
            uint h = Unsafe.Add(ref state, 7);

            for (int i = 0; i < 64; i += 8)
            {
                Round(a, b, c, ref d, e, f, g, ref h, Unsafe.Add(ref w, i), Sha256Helper.K[i]);
                Round(h, a, b, ref c, d, e, f, ref g, Unsafe.Add(ref w, i + 1), Sha256Helper.K[i + 1]);
                Round(g, h, a, ref b, c, d, e, ref f, Unsafe.Add(ref w, i + 2), Sha256Helper.K[i + 2]);
                Round(f, g, h, ref a, b, c, d, ref e, Unsafe.Add(ref w, i + 3), Sha256Helper.K[i + 3]);
                Round(e, f, g, ref h, a, b, c, ref d, Unsafe.Add(ref w, i + 4), Sha256Helper.K[i + 4]);
                Round(d, e, f, ref g, h, a, b, ref c, Unsafe.Add(ref w, i + 5), Sha256Helper.K[i + 5]);
                Round(c, d, e, ref f, g, h, a, ref b, Unsafe.Add(ref w, i + 6), Sha256Helper.K[i + 6]);
                Round(b, c, d, ref e, f, g, h, ref a, Unsafe.Add(ref w, i + 7), Sha256Helper.K[i + 7]);
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
            const int BlockSize = 64;
            Debug.Assert(destination.Length == 32);
            Debug.Assert(prepend.IsEmpty || prepend.Length == BlockSize);

            Span<uint> state = stackalloc uint[] {
                0x6a09e667,
                0xbb67ae85,
                0x3c6ef372,
                0xa54ff53a,
                0x510e527f,
                0x9b05688c,
                0x1f83d9ab,
                0x5be0cd19
            };
            Span<uint> W = stackalloc uint[64];
            ref uint w = ref MemoryMarshal.GetReference(W);
            ref uint stateRef = ref MemoryMarshal.GetReference(state);
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
            if (remaining >= BlockSize - sizeof(ulong))
            {
                Transform(ref stateRef, ref lastBlockRef, ref w);
                lastBlock.Slice(0, BlockSize - sizeof(ulong)).Clear();
            }

            // Append to the padding the total message's length in bits and transform.
            ulong bitLength = (ulong)dataLength << 3;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref lastBlockRef, BlockSize - sizeof(ulong)), BinaryPrimitives.ReverseEndianness(bitLength));
            Transform(ref stateRef, ref lastBlockRef, ref w);

            // reverse all the bytes when copying the final state to the output hash.
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
#if NETCOREAPP3_0
            if (Avx2.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(state))), Sha256Helper.LittleEndianMask256));
            }
            else if (Ssse3.IsSupported)
            {
                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.As<uint, byte>(ref stateRef)), Sha256Helper.LittleEndianMask128));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), Ssse3.Shuffle(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref Unsafe.As<uint, byte>(ref stateRef), 16)), Sha256Helper.LittleEndianMask128));
            }
            else
#endif
            {
                Unsafe.WriteUnaligned(ref destinationRef, BinaryPrimitives.ReverseEndianness(stateRef));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 4), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 1)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 8), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 2)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 12), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 3)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 16), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 4)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 20), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 5)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 24), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 6)));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 28), BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref stateRef, 7)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Round0(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint k)
        {
            h += Sha256Helper.BigSigma1(e) + Sha256Helper.Ch(e, f, g) + k;
            d += h;
            h += Sha256Helper.BigSigma0(a) + Sha256Helper.Maj(a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint w, uint k)
        {
            h += Sha256Helper.BigSigma1(e) + Sha256Helper.Ch(e, f, g) + k + w;
            d += h;
            h += Sha256Helper.BigSigma0(a) + Sha256Helper.Maj(a, b, c);
        }
    }
}

