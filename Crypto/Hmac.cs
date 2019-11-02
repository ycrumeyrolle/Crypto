using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Crypto
{
    public class Hmac : IDisposable
    {
        private readonly Sha256 _sha256;
        private readonly byte[] _innerKey = new byte[64];
        private readonly byte[] _outerKey = new byte[64];
#if NETCOREAPP3_0
        private static readonly Vector256<byte> _innerKeyInit = Vector256.Create((byte)0x36);
        private static readonly Vector256<byte> _outerKeyInit = Vector256.Create((byte)0x5c);
#endif
        public Hmac(Sha256 sha256, ReadOnlySpan<byte> key)
        {
            _sha256 = sha256;
            Span<byte> keyPrime = stackalloc byte[64];
            if (key.Length > 64)
            {
                _sha256.ComputeHash(key, keyPrime);
            }
            else
            {
                key.CopyTo(keyPrime);
            }

#if NETCOREAPP3_0
            if (Avx2.IsSupported)
            {
                ref byte keyRef = ref MemoryMarshal.GetReference(keyPrime);
                ref byte innerKeyRef = ref Unsafe.AsRef(_innerKey[0]);
                ref byte outerKeyRef = ref Unsafe.AsRef(_outerKey[0]);
                var k1 = Unsafe.ReadUnaligned<Vector256<byte>>(ref keyRef);
                var k2 = Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref keyRef, 32));
                Unsafe.WriteUnaligned(ref innerKeyRef, Avx2.Xor(k1, _innerKeyInit));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref innerKeyRef, 32), Avx2.Xor(k2, _innerKeyInit));
                Unsafe.WriteUnaligned(ref outerKeyRef, Avx2.Xor(k1, _outerKeyInit));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref outerKeyRef, 32), Avx2.Xor(k2, _outerKeyInit));
            }
            else
#endif
            {
                int i = 0;
                while (i < key.Length)
                {
                    _innerKey[i] = (byte)(key[i] ^ 0x36);
                    _outerKey[i] = (byte)(key[i] ^ 0x5c);
                    i++;
                }
                for (; i < 64; i++)
                {
                    _innerKey[i] ^= 0x36;
                    _outerKey[i] ^= 0x5c;
                }
            }
        }

        public void ComputeHash(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            // hash(o_key_pad ∥ hash(i_key_pad ∥ message));
            _sha256.ComputeHash(source, destination, _innerKey);
            _sha256.ComputeHash(destination, destination, _outerKey);
        }

        public void Dispose()
        {
            new Span<byte>(_innerKey).Clear();
            new Span<byte>(_outerKey).Clear();
        }
    }
}
