using System.Runtime.CompilerServices;

namespace Crypto
{
    public static class Sha384Helper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotR64(ulong a, byte b)
               => (a >> b) | (a << (64 - b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BigSigma0(ulong a)
             => RotR64(a, 28) ^ RotR64(a, 34) ^ RotR64(a, 39);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BigSigma1(ulong e)
              => RotR64(e, 14) ^ RotR64(e, 18) ^ RotR64(e, 41);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sigma0(ulong w)
          => RotR64(w, 1) ^ RotR64(w, 8) ^ (w >> 7);
        // => (((((w >> 42) ^ w) >> 13) ^ w) >> 6) ^ ((w << 42) ^ w) << 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sigma1(ulong w)
         => RotR64(w, 19) ^ RotR64(w, 61) ^ (w >> 6);
        //=> (((((w >> 1) ^ w) >> 6) ^ w) >> 1) ^ ((w << 7) ^ w) << 56;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Ch(ulong x, ulong y, ulong z)
            => z ^ (x & (y ^ z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Maj(ulong x, ulong y, ulong z)
            => ((x | y) & z) | (x & y);
        // => (x & y) ^ (x & z) ^ (y & z);
    }
}

