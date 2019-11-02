
//using System;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

//namespace Sha256
//{
//    public class Sha256
//    {   
//        public void ComputeHash(ReadOnlySpan<byte> source, Span<byte> destination)
//        {
//            Span<uint> state = stackalloc uint[8];
//            Span<uint> w = stackalloc uint[64];

//            ref byte sourceRef = ref MemoryMarshal.GetReference((source));
//            ref byte destinationRef = ref MemoryMarshal.GetReference((destination));
//            ref byte wRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(w));
//            ref byte sRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(state));

//            // Load words
//            Unsafe.CopyBlockUnaligned(ref wRef, ref sourceRef, 64);

//            // Initial State
//            state[0] = 0x6a09e667u;
//            state[1] = 0xbb67ae85u;
//            state[2] = 0x3c6ef372u;
//            state[3] = 0xa54ff53au;
//            state[4] = 0x510e527fu;
//            state[5] = 0x9b05688cu;
//            state[6] = 0x1f83d9abu;
//            state[7] = 0x5be0cd19u;

//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 0, w[0]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 1, w[1]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 2, w[2]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 3, w[3]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 4, w[4]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 5, w[5]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 6, w[6]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 7, w[7]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 8, w[8]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 9, w[9]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 10, w[10]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 11, w[11]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 12, w[12]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 13, w[13]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 14, w[14]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 15, w[15]);
//            w[16] = Sigma1(w[14]) + w[0] + w[9] + Sigma0(w[1]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 16, w[16]);
//            w[17] = Sigma1(w[15]) + w[1] + w[10] + Sigma0(w[2]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 17, w[17]);
//            w[18] = Sigma1(w[16]) + w[2] + w[11] + Sigma0(w[3]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 18, w[18]);
//            w[19] = Sigma1(w[17]) + w[3] + w[12] + Sigma0(w[4]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 19, w[19]);
//            w[20] = Sigma1(w[18]) + w[4] + w[13] + Sigma0(w[5]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 20, w[20]);
//            w[21] = Sigma1(w[19]) + w[5] + w[14] + Sigma0(w[6]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 21, w[21]);
//            w[22] = Sigma1(w[20]) + w[6] + w[15] + Sigma0(w[7]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 22, w[22]);
//            w[23] = Sigma1(w[21]) + w[7] + w[16] + Sigma0(w[8]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 23, w[23]);
//            w[24] = Sigma1(w[22]) + w[8] + w[17] + Sigma0(w[9]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 24, w[24]);
//            w[25] = Sigma1(w[23]) + w[9] + w[18] + Sigma0(w[10]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 25, w[25]);
//            w[26] = Sigma1(w[24]) + w[10] + w[19] + Sigma0(w[11]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 26, w[26]);
//            w[27] = Sigma1(w[25]) + w[11] + w[20] + Sigma0(w[12]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 27, w[27]);
//            w[28] = Sigma1(w[26]) + w[12] + w[21] + Sigma0(w[13]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 28, w[28]);
//            w[29] = Sigma1(w[27]) + w[13] + w[22] + Sigma0(w[14]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 29, w[29]);
//            w[30] = Sigma1(w[28]) + w[14] + w[23] + Sigma0(w[15]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 30, w[30]);
//            w[31] = Sigma1(w[29]) + w[15] + w[24] + Sigma0(w[16]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 31, w[31]);
//            w[32] = Sigma1(w[30]) + w[16] + w[25] + Sigma0(w[17]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 32, w[32]);
//            w[33] = Sigma1(w[31]) + w[17] + w[26] + Sigma0(w[18]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 33, w[33]);
//            w[34] = Sigma1(w[32]) + w[18] + w[27] + Sigma0(w[19]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 34, w[34]);
//            w[35] = Sigma1(w[33]) + w[19] + w[28] + Sigma0(w[20]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 35, w[35]);
//            w[36] = Sigma1(w[34]) + w[20] + w[29] + Sigma0(w[21]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 36, w[36]);
//            w[37] = Sigma1(w[35]) + w[21] + w[30] + Sigma0(w[22]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 37, w[37]);
//            w[38] = Sigma1(w[36]) + w[22] + w[31] + Sigma0(w[23]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 38, w[38]);
//            w[39] = Sigma1(w[37]) + w[23] + w[32] + Sigma0(w[24]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 39, w[39]);
//            w[40] = Sigma1(w[38]) + w[24] + w[33] + Sigma0(w[25]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 40, w[40]);
//            w[41] = Sigma1(w[39]) + w[25] + w[34] + Sigma0(w[26]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 41, w[41]);
//            w[42] = Sigma1(w[40]) + w[26] + w[35] + Sigma0(w[27]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 42, w[42]);
//            w[43] = Sigma1(w[41]) + w[27] + w[36] + Sigma0(w[28]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 43, w[43]);
//            w[44] = Sigma1(w[42]) + w[28] + w[37] + Sigma0(w[29]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 44, w[44]);
//            w[45] = Sigma1(w[43]) + w[29] + w[38] + Sigma0(w[30]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 45, w[45]);
//            w[46] = Sigma1(w[44]) + w[30] + w[39] + Sigma0(w[31]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 46, w[46]);
//            w[47] = Sigma1(w[45]) + w[31] + w[40] + Sigma0(w[32]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 47, w[47]);
//            w[48] = Sigma1(w[46]) + w[32] + w[41] + Sigma0(w[33]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 48, w[48]);
//            w[49] = Sigma1(w[47]) + w[33] + w[42] + Sigma0(w[34]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 49, w[49]);
//            w[50] = Sigma1(w[48]) + w[34] + w[43] + Sigma0(w[35]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 50, w[50]);
//            w[51] = Sigma1(w[49]) + w[35] + w[44] + Sigma0(w[36]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 51, w[51]);
//            w[52] = Sigma1(w[50]) + w[36] + w[45] + Sigma0(w[37]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 52, w[52]);
//            w[53] = Sigma1(w[51]) + w[37] + w[46] + Sigma0(w[38]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 53, w[53]);
//            w[54] = Sigma1(w[52]) + w[38] + w[47] + Sigma0(w[39]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 54, w[54]);
//            w[55] = Sigma1(w[53]) + w[39] + w[48] + Sigma0(w[40]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 55, w[55]);
//            w[56] = Sigma1(w[54]) + w[40] + w[49] + Sigma0(w[41]);
//            Sha256Round(state[0], state[1], state[2], ref state[3], state[4], state[5], state[6], ref state[7], 56, w[56]);
//            w[57] = Sigma1(w[55]) + w[41] + w[50] + Sigma0(w[42]);
//            Sha256Round(state[7], state[0], state[1], ref state[2], state[3], state[4], state[5], ref state[6], 57, w[57]);
//            w[58] = Sigma1(w[56]) + w[42] + w[51] + Sigma0(w[43]);
//            Sha256Round(state[6], state[7], state[0], ref state[1], state[2], state[3], state[4], ref state[5], 58, w[58]);
//            w[59] = Sigma1(w[57]) + w[43] + w[52] + Sigma0(w[44]);
//            Sha256Round(state[5], state[6], state[7], ref state[0], state[1], state[2], state[3], ref state[4], 59, w[59]);
//            w[60] = Sigma1(w[58]) + w[44] + w[53] + Sigma0(w[45]);
//            Sha256Round(state[4], state[5], state[6], ref state[7], state[0], state[1], state[2], ref state[3], 60, w[60]);
//            w[61] = Sigma1(w[59]) + w[45] + w[54] + Sigma0(w[46]);
//            Sha256Round(state[3], state[4], state[5], ref state[6], state[7], state[0], state[1], ref state[2], 61, w[61]);
//            w[62] = Sigma1(w[60]) + w[46] + w[55] + Sigma0(w[47]);
//            Sha256Round(state[2], state[3], state[4], ref state[5], state[6], state[7], state[0], ref state[1], 62, w[62]);
//            w[63] = Sigma1(w[61]) + w[47] + w[56] + Sigma0(w[48]);
//            Sha256Round(state[1], state[2], state[3], ref state[4], state[5], state[6], state[7], ref state[0], 63, w[63]);

//            // Feed Forward
//            state[0] += 0x6a09e667u;
//            state[1] += 0xbb67ae85u;
//            state[2] += 0x3c6ef372u;
//            state[3] += 0xa54ff53au;
//            state[4] += 0x510e527fu;
//            state[5] += 0x9b05688cu;
//            state[6] += 0x1f83d9abu;
//            state[7] += 0x5be0cd19u;

//            // Store Hash value
//            Unsafe.CopyBlockUnaligned(ref destinationRef, ref sRef, 32);
//        }

//        private static uint[] RC = {
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

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static uint RotR32(uint x, byte y)
//            => (x >> y) | (x << (32 - y));

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
//        private static void Sha256Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint rc, uint w)
//        {
//            uint T1 = h + BigSigma1(e) + Ch(e, f, g) + RC[rc] + w;
//            d += T1;
//            uint T2 = BigSigma0(a) + Maj(a, b, c);
//            h = T1 + T2;
//        }
//    }
//}