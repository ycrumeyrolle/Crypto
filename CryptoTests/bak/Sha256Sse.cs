//using System;
//using System.Buffers.Binary;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Runtime.Intrinsics;
//using System.Runtime.Intrinsics.X86;
//using System.Text;

//namespace Sha256Tests
//{
//    public class Sha256_4
//    {
//        static readonly uint[] sha256_consts = {
//    0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, /*  0 */
//    0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
//    0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, /*  8 */
//    0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
//    0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, /* 16 */
//    0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
//    0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, /* 24 */
//    0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
//    0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, /* 32 */
//    0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
//    0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, /* 40 */
//    0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
//    0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, /* 48 */
//    0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
//    0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, /* 56 */
//    0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
//};

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

//        private static Vector256<byte> CH_AVX(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c)
//            => Avx2.Xor(Avx2.And(a, b), Avx2.AndNot(c, a));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> MAJ_AVX(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c)
//            => Avx2.Xor(Avx2.Xor(Avx2.And(a, b), Avx2.And(a, c)), Avx2.And(b, c));



//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static Vector128<byte> Ch(Vector128<byte> b, Vector128<byte> c, Vector128<byte> d)
//        {
//            return Sse2.Xor(Sse2.And(b, c), Sse2.AndNot(d, b));
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static Vector128<byte> Maj(Vector128<byte> b, Vector128<byte> c, Vector128<byte> d)
//        {
//            return Sse2.Xor(Sse2.Xor(Sse2.And(b, c), Sse2.And(b, d)), Sse2.And(c, d));
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static Vector128<byte> ROTR(Vector128<byte> x, byte n)
//        {
//            return Sse2.Or(Sse2.ShiftRightLogical(x.AsInt16(), n), Sse2.ShiftLeftLogical(x.AsInt16(), (byte)(32 - n))).AsByte();
//        }

//        static Vector128<byte> SHR(Vector128<byte> x, byte n)
//        {
//            return Sse2.ShiftLeftLogical(x.AsInt16(), n).AsByte();
//        }

//        /* SHA256 Functions */
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> BIGSIGMA0_256(Vector128<byte> x) => Sse2.Xor(Sse2.Xor(ROTR((x), 2), ROTR((x), 13)), ROTR((x), 22));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> BIGSIGMA1_256(Vector128<byte> x) => Sse2.Xor(Sse2.Xor(ROTR((x), 6), ROTR((x), 11)), ROTR((x), 25));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> SIGMA0_256(Vector128<byte> x) => Sse2.Xor(Sse2.Xor(ROTR((x), 7), ROTR((x), 18)), SHR((x), 3));

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> SIGMA1_256(Vector128<byte> x) => Sse2.Xor(Sse2.Xor(ROTR((x), 17), ROTR((x), 19)), SHR((x), 10));

//        static Vector128<byte> load_epi32(uint x0, uint x1, uint x2, uint x3)
//        {
//            return Vector128.Create(x0, x1, x2, x3).AsByte();
//        }

//        //static uint store32(Vector128<byte> x)
//        //{
//        //    union { uint ret[0]; Vector128<byte> x; }
//        //    box;
//        //    box.x = x;
//        //    return box.ret[0];
//        //}

//        //static void store_epi32(Vector128<byte> x, uint* x0, uint* x1, uint* x2, uint* x3)
//        //{
//        //    union { uint ret[4]; Vector128<byte> x; }
//        //    box = { .x = x };
//        //    *x0 = box.ret[3]; *x1 = box.ret[2]; *x2 = box.ret[1]; *x3 = box.ret[0];
//        //}

//        static Vector128<byte> SHA256_CONST(int i)
//        {
//            return Vector128.Create(sha256_consts[i]).AsByte();
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> add4(Vector128<byte> x0, Vector128<byte> x1, Vector128<byte> x2, Vector128<byte> x3) => Sse2.Add(Sse2.Add(Sse2.Add(x0, x1), x2), x3);
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector128<byte> add5(Vector128<byte> x0, Vector128<byte> x1, Vector128<byte> x2, Vector128<byte> x3, Vector128<byte> x4) => Sse2.Add(add4(x0, x1, x2, x3), x4);


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static void SHA256ROUND(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c, ref Vector128<byte> d, Vector128<byte> e, Vector128<byte> f, Vector128<byte> g, ref Vector128<byte> h, int i, Vector128<byte> w)
//        {
//            var T1 = add5(h, BIGSIGMA1_256(e), Ch(e, f, g), SHA256_CONST(i), w);
//            d = Sse2.Add(d, T1);
//            var T2 = Sse2.Add(BIGSIGMA0_256(a), Maj(a, b, c));
//            h
//                = Sse2.Add(T1, T2);
//        }
//        static uint SWAP32(uint addr)
//        {
//            return BinaryPrimitives.ReverseEndianness(addr);
//        }

//        static Vector128<byte> LOAD(byte[][] blk, int i)
//        {
//            return Vector128.Create(SWAP32(blk[0][i * 4]), SWAP32(blk[1] [ i * 4]), SWAP32(blk[2] [ i * 4]), SWAP32(blk[3] [ i * 4])).AsByte();
//        }

//        //static void dumpreg(Vector128<byte> x, char* msg)
//        //{
//        //    union { uint ret[4]; Vector128<byte> x; }
//        //    box = { .x = x };
//        //    printf("%s %08x %08x %08x %08x\n", msg, box.ret[0], box.ret[1], box.ret[2], box.ret[3]);
//        //}

//        //#if 0
//        //#define dumpstate() printf("%s: %08x %08x %08x %08x %08x %08x %08x %08x %08x\n", \
//        //__func__, store32(w0), store32(a), store32(b), store32(c), store32(d), store32(e), store32(f), store32(g), store32(h));
//        //#else
//        //#define dumpstate()
//        //#endif



//        //blk : byte[64][4]
//        // hash : uint[8][4]
//        void __sha256_int(byte[][] blk, uint[][] hash)
//        {
//            uint[] h0 = hash[0], h1 = hash[1], h2 = hash[2], h3 = hash[3];

//            var a = Vector128.Create(h0[0], h1[0], h2[0], h3[0]).AsByte();
//            var b = Vector128.Create(h0[1], h1[1], h2[1], h3[1]).AsByte();
//            var c = Vector128.Create(h0[2], h1[2], h2[2], h3[2]).AsByte();
//            var d = Vector128.Create(h0[3], h1[3], h2[3], h3[3]).AsByte();
//            var e = Vector128.Create(h0[4], h1[4], h2[4], h3[4]).AsByte();
//            var f = Vector128.Create(h0[5], h1[5], h2[5], h3[5]).AsByte();
//            var g = Vector128.Create(h0[6], h1[6], h2[6], h3[6]).AsByte();
//            var h = Vector128.Create(h0[7], h1[7], h2[7], h3[7]).AsByte();

//            Vector128<byte> w0, w1, w2, w3, w4, w5, w6, w7;
//            Vector128<byte> w8, w9, w10, w11, w12, w13, w14, w15;
//            Vector128<byte> T1, T2;

//            w0 = LOAD(blk, 0);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 0, w0);
//            w1 = LOAD(blk, 1);    
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 1, w1);
//            w2 = LOAD(blk, 2);    
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 2, w2);
//            w3 = LOAD(blk, 3);    
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 3, w3);
//            w4 = LOAD(blk, 4);    
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 4, w4);
//            w5 = LOAD(blk, 5);    
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 5, w5);
//            w6 = LOAD(blk, 6);    
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 6, w6);
//            w7 = LOAD(blk, 7);    
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 7, w7);
//            w8 = LOAD(blk, 8);    
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 8, w8);
//            w9 = LOAD(blk, 9);    
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 9, w9);
//            w10 = LOAD(blk, 10);  
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 10, w10);
//            w11 = LOAD(blk, 11);  
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 11, w11);
//            w12 = LOAD(blk, 12);  
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 12, w12);
//            w13 = LOAD(blk, 13);  
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 13, w13);
//            w14 = LOAD(blk, 14);  
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 14, w14);
//            w15 = LOAD(blk, 15);  
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 15, w15);

//            w0 = add4(SIGMA1_256(w14), w9, SIGMA0_256(w1), w0);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 16, w0);
//            w1 = add4(SIGMA1_256(w15), w10, SIGMA0_256(w2), w1);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 17, w1);
//            w2 = add4(SIGMA1_256(w0), w11, SIGMA0_256(w3), w2);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 18, w2);
//            w3 = add4(SIGMA1_256(w1), w12, SIGMA0_256(w4), w3);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 19, w3);
//            w4 = add4(SIGMA1_256(w2), w13, SIGMA0_256(w5), w4);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 20, w4);
//            w5 = add4(SIGMA1_256(w3), w14, SIGMA0_256(w6), w5);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 21, w5);
//            w6 = add4(SIGMA1_256(w4), w15, SIGMA0_256(w7), w6);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 22, w6);
//            w7 = add4(SIGMA1_256(w5), w0, SIGMA0_256(w8), w7);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 23, w7);
//            w8 = add4(SIGMA1_256(w6), w1, SIGMA0_256(w9), w8);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 24, w8);
//            w9 = add4(SIGMA1_256(w7), w2, SIGMA0_256(w10), w9);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 25, w9);
//            w10 = add4(SIGMA1_256(w8), w3, SIGMA0_256(w11), w10);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 26, w10);
//            w11 = add4(SIGMA1_256(w9), w4, SIGMA0_256(w12), w11);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 27, w11);
//            w12 = add4(SIGMA1_256(w10), w5, SIGMA0_256(w13), w12);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 28, w12);
//            w13 = add4(SIGMA1_256(w11), w6, SIGMA0_256(w14), w13);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 29, w13);
//            w14 = add4(SIGMA1_256(w12), w7, SIGMA0_256(w15), w14);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 30, w14);
//            w15 = add4(SIGMA1_256(w13), w8, SIGMA0_256(w0), w15);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 31, w15);
                                  
//            w0 = add4(SIGMA1_256(w14), w9, SIGMA0_256(w1), w0);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 32, w0);
//            w1 = add4(SIGMA1_256(w15), w10, SIGMA0_256(w2), w1);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 33, w1);
//            w2 = add4(SIGMA1_256(w0), w11, SIGMA0_256(w3), w2);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 34, w2);
//            w3 = add4(SIGMA1_256(w1), w12, SIGMA0_256(w4), w3);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 35, w3);
//            w4 = add4(SIGMA1_256(w2), w13, SIGMA0_256(w5), w4);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 36, w4);
//            w5 = add4(SIGMA1_256(w3), w14, SIGMA0_256(w6), w5);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 37, w5);
//            w6 = add4(SIGMA1_256(w4), w15, SIGMA0_256(w7), w6);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 38, w6);
//            w7 = add4(SIGMA1_256(w5), w0, SIGMA0_256(w8), w7);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 39, w7);
//            w8 = add4(SIGMA1_256(w6), w1, SIGMA0_256(w9), w8);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 40, w8);
//            w9 = add4(SIGMA1_256(w7), w2, SIGMA0_256(w10), w9);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 41, w9);
//            w10 = add4(SIGMA1_256(w8), w3, SIGMA0_256(w11), w10);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 42, w10);
//            w11 = add4(SIGMA1_256(w9), w4, SIGMA0_256(w12), w11);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 43, w11);
//            w12 = add4(SIGMA1_256(w10), w5, SIGMA0_256(w13), w12);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 44, w12);
//            w13 = add4(SIGMA1_256(w11), w6, SIGMA0_256(w14), w13);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 45, w13);
//            w14 = add4(SIGMA1_256(w12), w7, SIGMA0_256(w15), w14);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 46, w14);
//            w15 = add4(SIGMA1_256(w13), w8, SIGMA0_256(w0), w15);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 47, w15);
                                 
//            w0 = add4(SIGMA1_256(w14), w9, SIGMA0_256(w1), w0);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 48, w0);
//            w1 = add4(SIGMA1_256(w15), w10, SIGMA0_256(w2), w1);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 49, w1);
//            w2 = add4(SIGMA1_256(w0), w11, SIGMA0_256(w3), w2);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 50, w2);
//            w3 = add4(SIGMA1_256(w1), w12, SIGMA0_256(w4), w3);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 51, w3);
//            w4 = add4(SIGMA1_256(w2), w13, SIGMA0_256(w5), w4);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 52, w4);
//            w5 = add4(SIGMA1_256(w3), w14, SIGMA0_256(w6), w5);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 53, w5);
//            w6 = add4(SIGMA1_256(w4), w15, SIGMA0_256(w7), w6);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 54, w6);
//            w7 = add4(SIGMA1_256(w5), w0, SIGMA0_256(w8), w7);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 55, w7);
//            w8 = add4(SIGMA1_256(w6), w1, SIGMA0_256(w9), w8);
//            SHA256ROUND(a, b, c, ref d, e, f, g, ref h, 56, w8);
//            w9 = add4(SIGMA1_256(w7), w2, SIGMA0_256(w10), w9);
//            SHA256ROUND(h, a, b, ref c, d, e, f, ref g, 57, w9);
//            w10 = add4(SIGMA1_256(w8), w3, SIGMA0_256(w11), w10);
//            SHA256ROUND(g, h, a, ref b, c, d, e, ref f, 58, w10);
//            w11 = add4(SIGMA1_256(w9), w4, SIGMA0_256(w12), w11);
//            SHA256ROUND(f, g, h, ref a, b, c, d, ref e, 59, w11);
//            w12 = add4(SIGMA1_256(w10), w5, SIGMA0_256(w13), w12);
//            SHA256ROUND(e, f, g, ref h, a, b, c, ref d, 60, w12);
//            w13 = add4(SIGMA1_256(w11), w6, SIGMA0_256(w14), w13);
//            SHA256ROUND(d, e, f, ref g, h, a, b, ref c, 61, w13);
//            w14 = add4(SIGMA1_256(w12), w7, SIGMA0_256(w15), w14);
//            SHA256ROUND(c, d, e, ref f, g, h, a, ref b, 62, w14);
//            w15 = add4(SIGMA1_256(w13), w8, SIGMA0_256(w0), w15);
//            SHA256ROUND(b, c, d, ref e, f, g, h, ref a, 63, w15);

//            //dumpreg(d, "last d");


//#define store(x,i)  \
//            w0 = load_epi32((*h0)[i], (*h1)[i], (*h2)[i], (*h3)[i]); \
//    w1 = _mm_add_epi32(w0, x); \
//    store_epi32(w1, &(*h0)[i], &(*h1)[i], &(*h2)[i], &(*h3)[i]);

//            //printf("%s a: %08x %08x\n", __func__, store32(a), (*h0)[0]);
//            store(a, 0);
//            //printf("%s a: %08x\n", __func__, (*h0)[0]);
//            store(b, 1);
//            store(c, 2);
//            store(d, 3);
//            store(e, 4);
//            store(f, 5);
//            store(g, 6);
//            store(h, 7);
//        }
//    }
//}
