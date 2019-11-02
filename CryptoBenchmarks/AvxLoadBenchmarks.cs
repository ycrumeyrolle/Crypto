using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Sha256Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class AvxLoadBenchmarks
    {
        private static readonly byte[] _src = new byte[64];

        private static Span<byte> Src => _src;

        public AvxLoadBenchmarks()
        {
            Array.Fill<byte>(_src, 1);
        }

        [Benchmark(Baseline = true)]
        public Vector128<byte> AvxLoad()
        {
            ref byte currentBlock = ref MemoryMarshal.GetReference(Src);
            return Avx2.LoadAlignedVector128((byte*)Unsafe.AsPointer(ref currentBlock));
        }

        [Benchmark(Baseline = false)]
        public Vector128<byte> ReadAligned()
        {
            ref byte currentBlock = ref MemoryMarshal.GetReference(Src);
            return Unsafe.ReadUnaligned<Vector128<byte>>(ref currentBlock);
        }

        [Benchmark(Baseline = false)]
        public Vector128<byte> UnsafeAs()
        {
            ref byte currentBlock = ref MemoryMarshal.GetReference(Src);
            return Unsafe.As<byte, Vector128<byte>>(ref currentBlock);
        }
    }
}
