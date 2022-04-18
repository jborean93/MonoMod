﻿using MonoMod.Backports;
using MonoMod.Core;
using MonoMod.Core.Platforms;
using System;
using System.Runtime.CompilerServices;

var platTriple = PlatformTriple.Current;

{
    var alloc = platTriple.System.MemoryAllocator;

    if (alloc.TryAllocateInRange(new((nint)0x10000, (nint) 0x8000, (nint) (1 << 50) - 1, 20), out var allocated)) {
        using var a = allocated;
        Console.WriteLine(a.BaseAddress);
    }
}

GC.GetTotalMemory(true);

Console.WriteLine(AbiSelftest.DetectAbi(platTriple).ToString());

Console.WriteLine(platTriple.Runtime.Abi.ToString());

var method = typeof(TestClass).GetMethod(nameof(TestClass.TestDetourMethod))!;
var method2 = typeof(TestClass).GetMethod(nameof(TestClass.Target))!;

var from = platTriple.GetNativeMethodBody(method);
Console.WriteLine($"0x{from:X16}");
var to = platTriple.GetNativeMethodBody(method2);
Console.WriteLine($"0x{to:X16}");

using var detour = platTriple.CreateNativeDetour(from, to);

TestClass.TestDetourMethod();

static class TestClass {
    [MethodImpl(MethodImplOptionsEx.NoInlining)]
    public static void TestDetourMethod() {
        var factory = DetourFactory.Current;

        Console.WriteLine(factory.SupportedFeatures);
    }

    [MethodImpl(MethodImplOptionsEx.NoInlining)]
    public static void Target() {
        Console.WriteLine("Method successfully detoured");
    }
}