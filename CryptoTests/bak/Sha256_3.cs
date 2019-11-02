//using System;
//using System.Buffers.Binary;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

//namespace Sha256Tests
//{
//    public class Sha256_3
//    {

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint RotR32(uint x, byte y)
//           => (x >> y) | (x << (32 - y));

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

//        //         (first 32 bits of the fractional parts of the cube roots of the first 64 primes 2..311):
//        private static uint[] k = new uint[] {
//                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
//                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
//                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
//                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
//                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
//                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
//                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
//                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
//            };

//        public void ComputeHash(ReadOnlySpan<byte> src, Span<byte> destination)
//        {
//            uint h0 = 0x6a09e667;
//            uint h1 = 0xbb67ae85;
//            uint h2 = 0x3c6ef372;
//            uint h3 = 0xa54ff53a;
//            uint h4 = 0x510e527f;
//            uint h5 = 0x9b05688c;
//            uint h6 = 0x1f83d9ab;
//            uint h7 = 0x5be0cd19;

//            ref byte currentBlock = ref MemoryMarshal.GetReference(src);
//            for (int j = 0; j < src.Length - 64; j += 64)
//            {
//                HashBlock(ref h0, ref h1, ref h2, ref h3, ref h4, ref h5, ref h6, ref h7, ref currentBlock);
//                currentBlock = ref Unsafe.Add(ref currentBlock, 64);
//            }

//            // last block
//            Span<byte> lastBlock = stackalloc byte[64];
//            ref byte lastBlockRef = ref MemoryMarshal.GetReference(lastBlock);
//            int lastByteIndex = src.Length & 0x3F;
//            Unsafe.CopyBlockUnaligned(ref lastBlockRef, ref currentBlock, (uint)lastByteIndex);
//            ref byte lastByteRef = ref Unsafe.Add(ref lastBlockRef, lastByteIndex);
//            lastByteRef = 0x80; // Append the bit '1' to the end of the message 

//            if (lastByteIndex >= 56)
//            {
//                // No place for the last 64 bits, hash the before-last block then clear the block 
//                HashBlock(ref h0, ref h1, ref h2, ref h3, ref h4, ref h5, ref h6, ref h7, ref lastBlockRef);
//                lastBlock.Clear();
//            }

//            long bitlen = src.Length * 8;
//            lastBlock[63] = (byte)(bitlen & 0xFF);
//            lastBlock[62] = (byte)((bitlen >> 8) & 0xFF);
//            lastBlock[61] = (byte)((bitlen >> 16) & 0xFF);
//            lastBlock[60] = (byte)((bitlen >> 24) & 0xFF);
//            lastBlock[59] = (byte)((bitlen >> 32) & 0xFF);
//            lastBlock[58] = (byte)((bitlen >> 40) & 0xFF);
//            lastBlock[57] = (byte)((bitlen >> 48) & 0xFF);
//            lastBlock[56] = (byte)((bitlen >> 56) & 0xFF);

//            HashBlock(ref h0, ref h1, ref h2, ref h3, ref h4, ref h5, ref h6, ref h7, ref lastBlockRef);

//            // Produce the final hash value(big - endian):
//            ref byte dstRef = ref MemoryMarshal.GetReference(destination);
//            Unsafe.WriteUnaligned(ref dstRef, BinaryPrimitives.ReverseEndianness(h0));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 4), BinaryPrimitives.ReverseEndianness(h1));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 8), BinaryPrimitives.ReverseEndianness(h2));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 12), BinaryPrimitives.ReverseEndianness(h3));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 16), BinaryPrimitives.ReverseEndianness(h4));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 20), BinaryPrimitives.ReverseEndianness(h5));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 24), BinaryPrimitives.ReverseEndianness(h6));
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, 28), BinaryPrimitives.ReverseEndianness(h7));
//        }

//        internal static void DWORDToBigEndian(Span<byte> block, uint[] x)
//        {
//            int num = 0;
//            int num2 = 0;
//            while (num < 8)
//            {
//                block[num2] = (byte)((x[num] >> 24) & 0xFF);
//                block[num2 + 1] = (byte)((x[num] >> 16) & 0xFF);
//                block[num2 + 2] = (byte)((x[num] >> 8) & 0xFF);
//                block[num2 + 3] = (byte)(x[num] & 0xFF);
//                num++;
//                num2 += 4;
//            }
//        }


//        private static void HashBlock(ref uint h0, ref uint h1, ref uint h2, ref uint h3, ref uint h4, ref uint h5, ref uint h6, ref uint h7, ref byte currentBLock)
//        {
//            Span<uint> w = stackalloc uint[64];
//            ref byte wRef = ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(w));

//            // Extend the first 16 words into the remaining 48 words w[16..63] of the message schedule array:
//            //   Unsafe.CopyBlockUnaligned(ref wRef, ref currentBLock, 16 * sizeof(uint));


//            for (int i = 0, j = 0; i < 16; ++i, j += 4)
//                w[i] = (uint)((currentBLock << 24) | (Unsafe.Add(ref currentBLock, j + 1) << 16) | (Unsafe.Add(ref currentBLock, j + 2) << 8) | (Unsafe.Add(ref currentBLock, j + 3)));

//            // Extend the first 16 words into the remaining 48 words w[16..63] of the message schedule array:
//            for (int i = 16; i < 64; i++)
//            {
//                uint s0 = Sigma0(w[i - 15]);
//                uint s1 = Sigma1(w[i - 2]);
//                w[i] = w[i - 16] + s0 + w[i - 7] + s1;
//            }

//            // Initialize working variables to current hash value
//            uint a = h0;
//            uint b = h1;
//            uint c = h2;
//            uint d = h3;
//            uint e = h4;
//            uint f = h5;
//            uint g = h6;
//            uint h = h7;

//            // Compression function main loop
//            for (int i = 0; i < 64; i++)
//            {
//                uint S1 = BigSigma1(e);
//                uint ch = Ch(e, f, g);
//                uint temp1 = h + S1 + ch + k[i] + w[i];
//                uint S0 = BigSigma0(a);
//                uint maj = Maj(a, b, c);
//                uint temp2 = S0 + maj;
//                h = g;
//                g = f;
//                f = e;
//                e = d + temp1;
//                d = c;
//                c = b;
//                b = a;
//                a = temp1 + temp2;
//            }


//            // Add the compressed chunk to the current hash value:
//            h0 = h0 + a;
//            h1 = h1 + b;
//            h2 = h2 + c;
//            h3 = h3 + d;
//            h4 = h4 + e;
//            h5 = h5 + f;
//            h6 = h6 + g;
//            h7 = h7 + h;
//        }
//    }
//}