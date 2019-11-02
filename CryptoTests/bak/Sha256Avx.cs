
//using System;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Runtime.Intrinsics;
//using System.Runtime.Intrinsics.X86;

//namespace Sha256Tests
//{
//    public class Sha256Avx
//    {
//        private static void Transpose(Span<Vector256<byte>> s)
//        {
//            var tmp00 = Avx2.UnpackLow(s[0], s[1]);
//            var tmp01 = Avx2.UnpackHigh(s[0], s[1]);
//            var tmp02 = Avx2.UnpackLow(s[2], s[3]);
//            var tmp03 = Avx2.UnpackHigh(s[2], s[3]);
//            var tmp04 = Avx2.UnpackLow(s[4], s[5]);
//            var tmp05 = Avx2.UnpackHigh(s[4], s[5]);
//            var tmp06 = Avx2.UnpackLow(s[6], s[7]);
//            var tmp07 = Avx2.UnpackHigh(s[6], s[7]);
//            var tmp10 = Avx2.UnpackLow(tmp00, tmp02);
//            var tmp11 = Avx2.UnpackHigh(tmp00, tmp02);
//            var tmp12 = Avx2.UnpackLow(tmp01, tmp03);
//            var tmp13 = Avx2.UnpackHigh(tmp01, tmp03);
//            var tmp14 = Avx2.UnpackLow(tmp04, tmp06);
//            var tmp15 = Avx2.UnpackHigh(tmp04, tmp06);
//            var tmp16 = Avx2.UnpackLow(tmp05, tmp07);
//            var tmp17 = Avx2.UnpackHigh(tmp05, tmp07);
//            s[0] = Avx2.Permute2x128(tmp10, tmp11, 0x20);
//            s[1] = Avx2.Permute2x128(tmp11, tmp15, 0x20);
//            s[2] = Avx2.Permute2x128(tmp12, tmp16, 0x20);
//            s[3] = Avx2.Permute2x128(tmp13, tmp17, 0x20);
//            s[4] = Avx2.Permute2x128(tmp10, tmp14, 0x31);
//            s[5] = Avx2.Permute2x128(tmp11, tmp15, 0x31);
//            s[6] = Avx2.Permute2x128(tmp12, tmp16, 0x31);
//            s[7] = Avx2.Permute2x128(tmp13, tmp17, 0x31);
//        }

//        public void Hash(ReadOnlySpan<byte> source, Span<byte> destination)
//        {
//            Span<Vector256<byte>> s = stackalloc Vector256<byte>[8];
//            Span<Vector256<byte>> w = stackalloc Vector256<byte>[64];

//            ref byte srcRef = ref MemoryMarshal.GetReference(source);
//            ref byte dstRef = ref MemoryMarshal.GetReference(destination);

//            // Load words and transform data correctly
//            for (int i = 0; i < 8; i++)
//            {
//                w[i] = Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref srcRef, 64 * i));
//                w[i + 8] = Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref srcRef, 32 + 64 * i));
//            }

//            Transpose(w);
//            Transpose(w.Slice(8));

//            // Initial State
//            s[0] = Vector256.Create(0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667).AsByte();
//            s[1] = Vector256.Create(0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85).AsByte();
//            s[2] = Vector256.Create(0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372).AsByte();
//            s[3] = Vector256.Create(0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a).AsByte();
//            s[4] = Vector256.Create(0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f).AsByte();
//            s[5] = Vector256.Create(0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c).AsByte();
//            s[6] = Vector256.Create(0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab).AsByte();
//            s[7] = Vector256.Create(0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19).AsByte();

//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 0, w[0]);
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 1, w[1]);
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 2, w[2]);
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 3, w[3]);
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 4, w[4]);
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 5, w[5]);
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 6, w[6]);
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 7, w[7]);
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 8, w[8]);
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 9, w[9]);
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 10, w10);
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 11, w11);
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 12, w12);
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 13, w13);
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 14, w14);
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 15, w15);
//            w16 = Add4(Sigma1(w14), w[0], w[9], Sigma0(w[1]));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 16, w16);
//            w17 = Add4(Sigma1(w15), w[1], w10, Sigma0(w[2]));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 17, w17);
//            w18 = Add4(Sigma1(w16), w[2], w11, Sigma0(w[3]));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 18, w18);
//            w19 = Add4(Sigma1(w17), w[3], w12, Sigma0(w[4]));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 19, w19);
//            w20 = Add4(Sigma1(w18), w[4], w13, Sigma0(w[5]));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 20, w20);
//            w21 = Add4(Sigma1(w19), w[5], w14, Sigma0(w[6]));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 21, w21);
//            w22 = Add4(Sigma1(w20), w[6], w15, Sigma0(w[7]));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 22, w22);
//            w23 = Add4(Sigma1(w21), w[7], w16, Sigma0(w[8]));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 23, w23);
//            w24 = Add4(Sigma1(w22), w[8], w17, Sigma0(w[9]));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 24, w24);
//            w25 = Add4(Sigma1(w23), w[9], w18, Sigma0(w10));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 25, w25);
//            w26 = Add4(Sigma1(w24), w10, w19, Sigma0(w11));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 26, w26);
//            w27 = Add4(Sigma1(w25), w11, w20, Sigma0(w12));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 27, w27);
//            w28 = Add4(Sigma1(w26), w12, w21, Sigma0(w13));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 28, w28);
//            w29 = Add4(Sigma1(w27), w13, w22, Sigma0(w14));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 29, w29);
//            w30 = Add4(Sigma1(w28), w14, w23, Sigma0(w15));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 30, w30);
//            w31 = Add4(Sigma1(w29), w15, w24, Sigma0(w16));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 31, w31);
//            w32 = Add4(Sigma1(w30), w16, w25, Sigma0(w17));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 32, w32);
//            w33 = Add4(Sigma1(w31), w17, w26, Sigma0(w18));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 33, w33);
//            w34 = Add4(Sigma1(w32), w18, w27, Sigma0(w19));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 34, w34);
//            w35 = Add4(Sigma1(w33), w19, w28, Sigma0(w20));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 35, w35);
//            w36 = Add4(Sigma1(w34), w20, w29, Sigma0(w21));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 36, w36);
//            w37 = Add4(Sigma1(w35), w21, w30, Sigma0(w22));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 37, w37);
//            w38 = Add4(Sigma1(w36), w22, w31, Sigma0(w23));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 38, w38);
//            w39 = Add4(Sigma1(w37), w23, w32, Sigma0(w24));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 39, w39);
//            w40 = Add4(Sigma1(w38), w24, w33, Sigma0(w25));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 40, w40);
//            w41 = Add4(Sigma1(w39), w25, w34, Sigma0(w26));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 41, w41);
//            w42 = Add4(Sigma1(w40), w26, w35, Sigma0(w27));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 42, w42);
//            w43 = Add4(Sigma1(w41), w27, w36, Sigma0(w28));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 43, w43);
//            w44 = Add4(Sigma1(w42), w28, w37, Sigma0(w29));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 44, w44);
//            w45 = Add4(Sigma1(w43), w29, w38, Sigma0(w30));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 45, w45);
//            w46 = Add4(Sigma1(w44), w30, w39, Sigma0(w31));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 46, w46);
//            w47 = Add4(Sigma1(w45), w31, w40, Sigma0(w32));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 47, w47);
//            w48 = Add4(Sigma1(w46), w32, w41, Sigma0(w33));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 48, w48);
//            w49 = Add4(Sigma1(w47), w33, w42, Sigma0(w34));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 49, w49);
//            w50 = Add4(Sigma1(w48), w34, w43, Sigma0(w35));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 50, w50);
//            w51 = Add4(Sigma1(w49), w35, w44, Sigma0(w36));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 51, w51);
//            w52 = Add4(Sigma1(w50), w36, w45, Sigma0(w37));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 52, w52);
//            w53 = Add4(Sigma1(w51), w37, w46, Sigma0(w38));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 53, w53);
//            w54 = Add4(Sigma1(w52), w38, w47, Sigma0(w39));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 54, w54);
//            w55 = Add4(Sigma1(w53), w39, w48, Sigma0(w40));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 55, w55);
//            w56 = Add4(Sigma1(w54), w40, w49, Sigma0(w41));
//            SHA256ROUND_AVX(s[0], s[1], s[2], ref s[3], s[4], s[5], s[6], ref s[7], 56, w56);
//            w57 = Add4(Sigma1(w55), w41, w50, Sigma0(w42));
//            SHA256ROUND_AVX(s[7], s[0], s[1], ref s[2], s[3], s[4], s[5], ref s[6], 57, w57);
//            w58 = Add4(Sigma1(w56), w42, w51, Sigma0(w43));
//            SHA256ROUND_AVX(s[6], s[7], s[0], ref s[1], s[2], s[3], s[4], ref s[5], 58, w58);
//            w59 = Add4(Sigma1(w57), w43, w52, Sigma0(w44));
//            SHA256ROUND_AVX(s[5], s[6], s[7], ref s[0], s[1], s[2], s[3], ref s[4], 59, w59);
//            w60 = Add4(Sigma1(w58), w44, w53, Sigma0(w45));
//            SHA256ROUND_AVX(s[4], s[5], s[6], ref s[7], s[0], s[1], s[2], ref s[3], 60, w60);
//            w61 = Add4(Sigma1(w59), w45, w54, Sigma0(w46));
//            SHA256ROUND_AVX(s[3], s[4], s[5], ref s[6], s[7], s[0], s[1], ref s[2], 61, w61);
//            w62 = Add4(Sigma1(w60), w46, w55, Sigma0(w47));
//            SHA256ROUND_AVX(s[2], s[3], s[4], ref s[5], s[6], s[7], s[0], ref s[1], 62, w62);
//            w63 = Add4(Sigma1(w61), w47, w56, Sigma0(w48));
//            SHA256ROUND_AVX(s[1], s[2], s[3], ref s[4], s[5], s[6], s[7], ref s[0], 63, w63);

//            // Feed Forward
//            s[0] = Avx2.Add(s[0], Vector256.Create(0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667, 0x6a09e667).AsByte());
//            s[1] = Avx2.Add(s[1], Vector256.Create(0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85, 0xbb67ae85).AsByte());
//            s[2] = Avx2.Add(s[2], Vector256.Create(0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372, 0x3c6ef372).AsByte());
//            s[3] = Avx2.Add(s[3], Vector256.Create(0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a, 0xa54ff53a).AsByte());
//            s[4] = Avx2.Add(s[4], Vector256.Create(0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f, 0x510e527f).AsByte());
//            s[5] = Avx2.Add(s[5], Vector256.Create(0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c, 0x9b05688c).AsByte());
//            s[6] = Avx2.Add(s[6], Vector256.Create(0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab, 0x1f83d9ab).AsByte());
//            s[7] = Avx2.Add(s[7], Vector256.Create(0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19, 0x5be0cd19).AsByte());

//            // Transpose data again to get correct output
//            Transpose(s);

//            // Store Hash value
//            for (int i = 0; i < 8; i++)
//            {
//                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dstRef, (IntPtr)(32 * i)), s[i]);
//            }
//            return;
//        }

//        private static void SHA256ROUND_AVX(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c, ref Vector256<byte> d, Vector256<byte> e, Vector256<byte> f, Vector256<byte> g, ref Vector256<byte> h, byte rc, Vector256<byte> w)
//        {
//            Vector256<byte> T0 = Add4(Avx2.Add(h, Sigma1(e)), CH_AVX(e, f, g), Vector256.Create(RC[rc], RC[rc], RC[rc], RC[rc]).AsByte(), w);
//            d = Avx2.Add(d, T0);
//            Vector256<byte> T1 = Avx2.Add(Sigma0(a), MAJ_AVX(a, b, c));
//            h = Avx2.Add(T0, T1);
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> RotR32(Vector256<byte> x, byte y)
//            => Avx2.Or(Avx2.ShiftRightLogical(x.AsUInt32(), y), Avx2.ShiftLeftLogical(x.AsUInt32(), (byte)(32 - y))).AsByte();

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> RotL32(Vector256<byte> x, byte y)
//            => Avx2.Or(Avx2.ShiftLeftLogical(x.AsUInt32(), y), Avx2.ShiftRightLogical(x.AsUInt32(), (byte)(32 - y))).AsByte();

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> Sigma1(Vector256<byte> x)
//            => Avx2.Xor(Avx2.Xor(RotR32(x, 17), RotR32(x, 19)), Avx2.ShiftRightLogical(x.AsUInt32(), 10).AsByte());

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> Sigma0(Vector256<byte> x)
//            => Avx2.Xor(Avx2.Xor(RotR32(x, 7), RotR32(x, 18)), Avx2.ShiftRightLogical(x.AsUInt32(), 3).AsByte());

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> Add4(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c, Vector256<byte> d)
//            => Avx2.Add(Avx2.Add(a, b), Avx2.Add(c, d));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> CH_AVX(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c)
//            => Avx2.Xor(Avx2.And(a, b), Avx2.AndNot(c, a));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> MAJ_AVX(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c)
//            => Avx2.Xor(Avx2.Xor(Avx2.And(a, b), Avx2.And(a, c)), Avx2.And(b, c));

//        private static ReadOnlySpan<uint> RC => new uint[]  {
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
//    }
//}
