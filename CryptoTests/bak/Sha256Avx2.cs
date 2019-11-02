﻿//using System;
//using System.Buffers.Binary;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Runtime.Intrinsics;
//using System.Runtime.Intrinsics.X86;

//namespace Sha256Tests
//{
//    public class Sha256Avx2
//    {
//        static readonly uint[] k = {
//            0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
//            0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
//            0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
//            0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
//            0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
//            0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
//            0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
//            0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2
//        };

//        // 3, 2, 1, 0, 7, 6, 5, 4,
//        // 11, 10, 9, 8, 15, 14, 13, 12,
//        // 19, 18, 17, 16, 23, 22, 21, 20,
//        // 27, 26, 25, 24, 31, 30, 29, 28
//        private static Vector256<byte> _shuffleMask256 = Vector256.Create(
//                289644378169868803,
//                868365760874482187,
//                1447087143579095571,
//                2025808526283708955
//                ).AsByte();

//        // 3, 2, 1, 0, 7, 6, 5, 4,
//        // 11, 10, 9, 8, 15, 14, 13, 12
//        private static Vector128<byte> _shuffleMask128 = Vector128.Create(
//                289644378169868803,
//                868365760874482187
//                ).AsByte();

//        private static readonly Vector256<uint> _initialState = Vector256.Create(0x6a09e667,
//                                      0xbb67ae85,
//                                      0x3c6ef372,
//                                      0xa54ff53a,
//                                      0x510e527f,
//                                      0x9b05688c,
//                                      0x1f83d9ab,
//                                      0x5be0cd19);
//        private Vector256<uint> sha256_transform(Vector256<uint> state, ref byte currentBlock)
//        {
//            Transform_Sha256_p = Transform_Sha256_AVX2_RORX;
//            Transform_Sha256_Len_p = Transform_Sha256_AVX2_RORX_Len;




//            uint a, b, c, d, e, f, g, h;
//            Span<uint> w = stackalloc uint[64];
//            ref byte wRef = ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(w));

//            if (Avx2.IsSupported)
//            {
//                Unsafe.WriteUnaligned(ref wRef, Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask256));
//                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Avx2.Shuffle(Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask256));
//            }
//            else if (Ssse3.IsSupported)
//            {
//                Unsafe.WriteUnaligned(ref wRef, Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 0)), _shuffleMask128));
//                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 16), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 16)), _shuffleMask128));
//                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 32)), _shuffleMask128));
//                Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 48), Ssse3.Shuffle(Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref currentBlock, 48)), _shuffleMask128));
//            }
//            else
//            {
//                for (int i = 0, j = 0; i < 16; ++i, j += 4)
//                {
//                    w[i] = (uint)((Unsafe.Add(ref currentBlock, j) << 24) | (Unsafe.Add(ref currentBlock, j + 1) << 16) | (Unsafe.Add(ref currentBlock, j + 2) << 8) | (Unsafe.Add(ref currentBlock, j + 3)));
//                }
//            }

//            for (int i = 16; i < 64; i += 2)
//            {
//                w[i] = w[i - 16] + ShaHelper.Sigma0(w[i - 15]) + w[i - 7] + ShaHelper.Sigma1(w[i - 2]);
//                w[i + 1] = w[i - 15] + ShaHelper.Sigma0(w[i - 14]) + w[i - 6] + ShaHelper.Sigma1(w[i - 1]);
//            }

//            a = state.GetElement(0);
//            b = state.GetElement(1);
//            c = state.GetElement(2);
//            d = state.GetElement(3);
//            e = state.GetElement(4);
//            f = state.GetElement(5);
//            g = state.GetElement(6);
//            h = state.GetElement(7);

//            for (int i = 0; i < 64; i += 8)
//            {
//                Round(a, b, c, ref d, e, f, g, ref h, w[i], k[i]);
//                Round(h, a, b, ref c, d, e, f, ref g, w[i + 1], k[i + 1]);
//                Round(g, h, a, ref b, c, d, e, ref f, w[i + 2], k[i + 2]);
//                Round(f, g, h, ref a, b, c, d, ref e, w[i + 3], k[i + 3]);
//                Round(e, f, g, ref h, a, b, c, ref d, w[i + 4], k[i + 4]);
//                Round(d, e, f, ref g, h, a, b, ref c, w[i + 5], k[i + 5]);
//                Round(c, d, e, ref f, g, h, a, ref b, w[i + 6], k[i + 6]);
//                Round(b, c, d, ref e, f, g, h, ref a, w[i + 7], k[i + 7]);
//            }

//            if (Avx2.IsSupported)
//            {
//                return Avx2.Add(state, Vector256.Create(a, b, c, d, e, f, g, h));
//            }
//            else if (Sse3.IsSupported)
//            {
//                return Vector256.Create(
//                                 state.GetElement(0) + a,
//                                 state.GetElement(1) + b,
//                                 state.GetElement(2) + c,
//                                 state.GetElement(3) + d,
//                                 state.GetElement(4) + e,
//                                 state.GetElement(5) + f,
//                                 state.GetElement(6) + g,
//                                 state.GetElement(7) + h);
//            }
//            else
//            {
//                return Vector256.Create(
//                    state.GetElement(0) + a,
//                    state.GetElement(1) + b,
//                    state.GetElement(2) + c,
//                    state.GetElement(3) + d,
//                    state.GetElement(4) + e,
//                    state.GetElement(5) + f,
//                    state.GetElement(6) + g,
//                    state.GetElement(7) + h);
//            }
//        }

//        public void ComputeHash(ReadOnlySpan<byte> src, Span<byte> destination, ReadOnlySpan<byte> prepend = default)
//        {
//            int dataLength = src.Length + prepend.Length;
//            Span<byte> data = stackalloc byte[64];
//            var state = _initialState;

//            ref byte dataRef = ref MemoryMarshal.GetReference(data);
//            ref byte srcRef = ref MemoryMarshal.GetReference(src);
//            ref byte srcEndRef = ref Unsafe.Add(ref srcRef, src.Length - 64 + 1);

//            if (!prepend.IsEmpty)
//            {
//                Debug.Assert(prepend.Length == 64);
//                ref byte preprendRef = ref MemoryMarshal.GetReference(prepend);
//                state = sha256_transform(state, ref preprendRef);
//            }

//            while (Unsafe.IsAddressLessThan(ref srcRef, ref srcEndRef))
//            {
//                state = sha256_transform(state, ref srcRef);
//                srcRef = ref Unsafe.Add(ref srcRef, 64);
//            }

//            int remaining = dataLength & 63;
//            Unsafe.CopyBlockUnaligned(ref dataRef, ref srcRef, (uint)remaining);

//            // Pad the last block
//            Unsafe.Add(ref dataRef, remaining) = 0x80;
//            // data.Slice(remaining + 1).Clear();
//            Unsafe.InitBlockUnaligned(ref Unsafe.Add(ref dataRef, remaining + 1), 0, (uint)(64 - remaining - 1));
//            if (remaining >= 56)
//            {
//                state = sha256_transform(state, ref dataRef);
//                // data.Slice(0, 56).Clear();
//                Unsafe.InitBlockUnaligned(ref dataRef, 0, 56u);
//            }

//            // Append to the padding the total message's length in bits and transform.
//            //ulong bitLength = (ulong)dataLength * 8;
//            //data[63] = (byte)bitLength;
//            //data[62] = (byte)(bitLength >> 8);
//            //data[61] = (byte)(bitLength >> 16);
//            //data[60] = (byte)(bitLength >> 24);
//            //data[59] = (byte)(bitLength >> 32);
//            //data[58] = (byte)(bitLength >> 40);
//            //data[57] = (byte)(bitLength >> 48);
//            //data[56] = (byte)(bitLength >> 56);
//            ulong bitLength = (ulong)dataLength << 3;
//            Unsafe.WriteUnaligned(ref Unsafe.Add(ref dataRef, 56), BinaryPrimitives.ReverseEndianness(bitLength));
//            state = sha256_transform(state, ref dataRef);

//            // Since this implementation uses little endian byte ordering and SHA uses big endian,
//            // reverse all the bytes when copying the final state to the output hash.
//            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
//            if (Avx2.IsSupported)
//            {
//                Unsafe.WriteUnaligned(ref destinationRef, Avx2.Shuffle(state.AsByte(), _shuffleMask256));
//            }
//            else if (Ssse3.IsSupported)
//            {
//                Unsafe.WriteUnaligned(ref destinationRef, Ssse3.Shuffle(state.GetLower().AsByte(), _shuffleMask128));
//                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destinationRef, 32), Ssse3.Shuffle(state.GetUpper().AsByte(), _shuffleMask128));
//            }
//            else
//            {
//                for (int j = 0; j < 4; ++j)
//                {
//                    destination[j] = (byte)((state.GetElement(0) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 4] = (byte)((state.GetElement(1) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 8] = (byte)((state.GetElement(2) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 12] = (byte)((state.GetElement(3) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 16] = (byte)((state.GetElement(4) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 20] = (byte)((state.GetElement(5) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 24] = (byte)((state.GetElement(6) >> (24 - j * 8)) & 0x000000ff);
//                    destination[j + 28] = (byte)((state.GetElement(7) >> (24 - j * 8)) & 0x000000ff);
//                }
//            }
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static void Round(uint a, uint b, uint c, ref uint d, uint e, uint f, uint g, ref uint h, uint w, uint k)
//        {
//            uint t1 = h + ShaHelper.BigSigma1(e) + ShaHelper.Ch(e, f, g) + k + w;
//            uint t2 = ShaHelper.BigSigma0(a) + ShaHelper.Maj(a, b, c);
//            d += t1;
//            h = t1 + t2;
//        }



//        static int Transform_Sha256_AVX2_RORX(wc_Sha256* sha256)
//        {

//            "subq	$512, %%rsp\n\t"
//        "leaq	32(%[sha256]), %%rax\n\t"

//    INIT_MASKS_Y(BYTE_FLIP_MASK, SHUF_Y_00BA, SHUF_Y_DC00)
//    LOAD_W_K_LOW(BYTE_FLIP_MASK, rax)

//    LOAD_DIGEST()

//        "movl	%%r9d, " L4 "\n\t"
//        "rorx	$6, %%r12d, " L1 "\n\t"
//        "xorl	%%r10d, " L4 "\n\t"

//    SET_W_Y_4(0)
//    MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 0)
//    MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 8)
//    MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 16)
//    MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 24)

//    SET_W_Y_4(16)
//    MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 32)
//    MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 40)
//    MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 48)
//    MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 56)

//    SET_W_Y_4(32)
//    MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 64)
//    MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 72)
//    MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 80)
//    MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 88)

//    SET_W_Y_4(48)
//        "xorl	" L3 ", " L3 "\n\t"
//        "xorl	" L2 ", " L2 "\n\t"
//    RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 96)
//    RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 104)
//    RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 112)
//    RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 120)
//        /* Prev RND: h += Maj(a,b,c) */
//        "addl	" L3 ", %%r8d\n\t"

//    STORE_ADD_DIGEST()

//        "addq	$512, %%rsp\n\t"

//        :
//        : [FLIP]     "m" (mBYTE_FLIP_MASK[0]),
//          [SHUF00BA] "m" (mSHUF_Y_00BA[0]),
//          [SHUFDC00] "m" (mSHUF_Y_DC00[0]),
//          [sha256]   "r" (sha256),
//          [K]        "m" (K256)
//        : WORK_REGS, STATE_REGS, YMM_REGS, "memory"
//    );

//    return 0;
//}

//    static int Transform_Sha256_AVX2_RORX_Len(wc_Sha256* sha256, word32 len)
//    {
//        if ((len & WC_SHA256_BLOCK_SIZE) != 0)
//        {
//            XMEMCPY(sha256.buffer, sha256.data, WC_SHA256_BLOCK_SIZE);
//            Transform_Sha256_AVX2_RORX(sha256);
//            sha256.data += WC_SHA256_BLOCK_SIZE;
//            len -= WC_SHA256_BLOCK_SIZE;
//            if (len == 0)
//                return 0;
//        }

//        __asm__ __volatile__(


//        "subq	$512, %%rsp\n\t"
//            "movq	120(%[sha256]), %%rax\n\t"


//    INIT_MASKS_Y(BYTE_FLIP_Y_MASK, SHUF_Y_00BA, SHUF_Y_DC00)
//        LOAD_DIGEST()


//        "# Start of loop processing two blocks\n"
//            "1:\n\t"


//    LOAD_W_K(BYTE_FLIP_Y_MASK, rax)


//        "movl	%%r9d, " L4 "\n\t"
//            "rorx	$6, %%r12d, " L1 "\n\t"
//            "xorl	%%r10d, " L4 "\n\t"


//    SET_W_Y_4(0)
//        MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 0)
//        MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 8)
//        MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 16)
//        MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 24)


//    SET_W_Y_4(16)
//        MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 32)
//        MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 40)
//        MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 48)
//        MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 56)


//    SET_W_Y_4(32)
//        MsgSched_Y_RORX(Y0, Y1, Y2, Y3, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 64)
//        MsgSched_Y_RORX(Y1, Y2, Y3, Y0, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 72)
//        MsgSched_Y_RORX(Y2, Y3, Y0, Y1, S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 80)
//        MsgSched_Y_RORX(Y3, Y0, Y1, Y2, S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 88)


//    SET_W_Y_4(48)
//            "xorl	" L3 ", " L3 "\n\t"
//            "xorl	" L2 ", " L2 "\n\t"
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 96)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 104)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 112)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 120)
//            /* Prev RND: h += Maj(a,b,c) */
//        "addl	" L3 ", %%r8d\n\t"
//            "xorl	" L2 ", " L2 "\n\t"


//    ADD_DIGEST()
//        STORE_DIGEST()


//        "movl	%%r9d, " L4 "\n\t"
//            "xorl	" L3 ", " L3 "\n\t"
//            "xorl	%%r10d, " L4 "\n\t"


//    RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 4)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 12)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 20)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 28)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 36)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 44)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 52)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 60)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 68)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 76)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 84)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 92)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 100)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 108)
//        RND_RORX_X4(S_0, S_1, S_2, S_3, S_4, S_5, S_6, S_7, 116)
//        RND_RORX_X4(S_4, S_5, S_6, S_7, S_0, S_1, S_2, S_3, 124)
//            /* Prev RND: h += Maj(a,b,c) */
//        "addl	" L3 ", %%r8d\n\t"
//            "movq	120(%[sha256]), %%rax\n\t"


//    ADD_DIGEST()


//        "addq	$128, %%rax\n\t"
//            "subl	$128, %[len]\n\t"


//    STORE_DIGEST()


//        "movq	%%rax, 120(%[sha256])\n\t"
//            "jnz	1b\n\t"


//        "addq	$512, %%rsp\n\t"
    
//        :
//        : [FLIP]     "m" (mBYTE_FLIP_Y_MASK[0]),
//          [SHUF00BA] "m" (mSHUF_Y_00BA[0]),
//          [SHUFDC00] "m" (mSHUF_Y_DC00[0]),
//          [sha256]   "r" (sha256),
//          [len]      "r" (len),
//          [K]        "m" (K256)
//        : WORK_REGS, STATE_REGS, YMM_REGS, "memory"
//    );

//    return 0;
//}
//    }


   
//}

