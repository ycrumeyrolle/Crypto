using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Sha256Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class StoreBenchmarks
    {
        private static readonly byte[] _buffer = new byte[64];
        private static readonly byte[] _src = new byte[64];

        private static Span<byte> Buffer => _buffer;
        private static Span<byte> Src => _src;

        public StoreBenchmarks()
        {
            Array.Fill<byte>(_src, 1);
        }

        [Benchmark(Baseline = true)]
        public ReadOnlySpan<byte> AvxStore()
        {
            ref byte wRef = ref MemoryMarshal.GetReference(Buffer);
            ref byte currentBlock = ref MemoryMarshal.GetReference(Src);
            Avx2.Store((byte*)Unsafe.AsPointer(ref wRef), Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 0)));
            Avx2.Store((byte*)Unsafe.AsPointer(ref Unsafe.Add(ref wRef, 32)), Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref Unsafe.Add(ref currentBlock, 32), 0)));
            return Src;

        }

        [Benchmark(Baseline = false)]
        public ReadOnlySpan<byte> WriteUnaligned()
        {
            ref byte wRef = ref MemoryMarshal.GetReference(Buffer);
            ref byte currentBlock = ref MemoryMarshal.GetReference(Src);
            Unsafe.WriteUnaligned(ref wRef, Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 0)));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref wRef, 32), Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref currentBlock, 32)));
            return Src;
        }
    }
}
