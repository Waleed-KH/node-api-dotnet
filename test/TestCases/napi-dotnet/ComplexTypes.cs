// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable IDE0060 // Unused parameters
#pragma warning disable IDE0301 // Collection initialization can be simplified

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Microsoft.JavaScript.NodeApi.TestCases;

/// <summary>
/// Tests marshalling of various non-primitive types.
/// </summary>
[JSExport]
public static class ComplexTypes
{
    public static int? NullableInt { get; set; }

    public static string? NullableString { get; set; }

    public static StructObject StructObject { get; set; } = new() { Value = "test" };

    public static ReadonlyStructObject ReadonlyStructObject { get; set; } =
        new() { Value = "test" };

    public static StructObject? NullableStructObject { get; set; }

    public static ClassObject ClassObject { get; set; } = new() { Value = "test" };

    public static ITestInterface InterfaceObject { get; set; } = ClassObject;

    public static ClassObject? NullableClassObject { get; set; }

    public static TestEnum TestEnum { get; set; }

    public static DateTime DateTime { get; set; }
        = new DateTime(2023, 4, 5, 6, 7, 8, DateTimeKind.Unspecified);

    public static DateTime DateTimeLocal { get; }
        = new DateTime(2023, 4, 5, 6, 7, 8, DateTimeKind.Local);

    public static DateTime DateTimeUtc { get; }
        = new DateTime(2023, 4, 5, 6, 7, 8, DateTimeKind.Utc);

    public static TimeSpan TimeSpan { get; set; } = new TimeSpan(1, 12, 30, 45);

    public static DateTimeOffset DateTimeOffset { get; set; }
        = new DateTimeOffset(DateTime, -TimeSpan.FromMinutes(90));

    public static KeyValuePair<string, int> Pair { get; set; }
        = new KeyValuePair<string, int>("pair", 1);

    public static Tuple<string, int> Tuple { get; set; }
        = new Tuple<string, int>("tuple", 2);

    public static (string Key, int Value) ValueTuple { get; set; } = (Key: "valueTuple", Value: 3);

    public static Guid Guid { get; set; } = Guid.Parse("01234567-89AB-CDEF-FEDC-BA9876543210");

    public static BigInteger BigInt { get; set; } = BigInteger.Parse("1234567890123456789012345");
}

/// <summary>
/// Tests marshalling struct objects (passed by value).
/// </summary>
[JSExport]
public struct StructObject
{
    public StructObject() : this(null) {}

    public StructObject(string? value)
    {
        Value = value;
    }

    public string? Value { get; set; }

    public override readonly bool Equals(object? obj)
    {
        return obj is StructObject structObject && Value == structObject.Value;
    }

    public override readonly int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public static string? StaticValue { get; set; }

    public readonly StructObject ThisObject() => this;

    public static bool operator ==(StructObject left, StructObject right) => left.Equals(right);

    public static bool operator !=(StructObject left, StructObject right) => !(left == right);
}

[JSExport]
public readonly struct ReadonlyStructObject
{
    public string? Value { get; init; }
}

/// <summary>
/// Tests marshalling class objects via interfaces.
/// </summary>
[JSExport]
public interface ITestInterface
{
    string? Value { get; set; }

    string AppendValue(string append);

    void AppendAndGetPreviousValue(ref string value, out string? previousValue);

#if !AOT
    string AppendGenericValue<T>(T value);
#endif
}

/// <summary>
/// Tests marshalling class objects (passed by reference).
/// </summary>
[JSExport]
public class ClassObject : ITestInterface
{
    public ClassObject() : this(null) {}

    public ClassObject(string? value)
    {
        Value = value;
    }

    public string? Value { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ClassObject classObject && Value == classObject.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public string AppendValue(string append)
    {
        Value = (Value ?? "") + append;
        return Value!;
    }

    public void AppendAndGetPreviousValue(ref string value, out string? previousValue)
    {
        previousValue = Value;
        Value = (Value ?? "") + value;
        value = Value;
    }

    public bool TryGetValue(out string? value)
    {
        value = Value;
        return value != null;
    }

    public static string? StaticValue { get; set; }

    public ClassObject ThisObject() => this;

    public static ITestInterface Create(string value)
    {
        return new ClassObject { Value = value };
    }

#if !AOT
    public string AppendGenericValue<T>(T value)
    {
        Value = (Value ?? "") + value?.ToString();
        return Value!;
    }

    public static void CallGenericMethod(ITestInterface obj, int value)
    {
        obj.AppendGenericValue<int>(value);
    }

    public static void WithGenericList(IList<StructObject> list)
    {
        // This just ensures the TS generator can handle generic parameter types.
    }
#endif

    public class NestedClass
    {
        public NestedClass(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}

[JSExport]
public enum TestEnum
{
    Zero,
    One,
    Two,
}

// The subclass is declared before the base class to ensure the module generator handles this case.
[JSExport]
public class SubClass2 : SubClass
{
    public SubClass2(int value, int value2) : base(value, value2) {}
}

[JSExport]
public interface IBaseInterface
{
    int Value1 { get; set; }
}

[JSExport]
public class BaseClass : IBaseInterface
{
    public BaseClass(int value) { Value1 = value; }

    public BaseClass(IBaseInterface copy)
    {
        Value1 = copy.Value1;
    }

    // Ensure module generation handles circular references between a base class and subclass.
    public SubClass? BaseProperty { get; set; }

    public int Value1 { get; set; }
}

[JSExport]
public interface ISubInterface : IBaseInterface
{
    int Value2 { get; set; }
}

[JSExport]
public class SubClass : BaseClass, ISubInterface
{
    public SubClass(int value, int value2) : base(value)
    {
        Value2 = value2;
        BaseProperty = this;
    }

    public SubClass(ISubInterface copy) : base(copy)
    {
        Value2 = copy.Value2;
        BaseProperty = this;
    }

    public int Value2 { get; set; }
}

[JSExport]
public class ClassWithPrivateConstructor
{
    private ClassWithPrivateConstructor(string value)
    {
        Value = value;
    }

    public static ClassWithPrivateConstructor CreateInstance(string value)
    {
        return new ClassWithPrivateConstructor(value);
    }

    public string Value { get; }
}

// Ensure module generation handles implementing an interface with a custom type argument.
[JSExport]
public class CollectionOfClassObjects : IEnumerable<ClassObject>
{
    public IEnumerator<ClassObject> GetEnumerator() => throw new NotImplementedException();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => throw new NotImplementedException();
}

// Ensure module generation handles a self-referential interface type argument.
[JSExport]
public class Equatable : IEquatable<Equatable>
{
    public bool Equals(Equatable? other) => other != null;
    public override bool Equals(object? obj) => obj is Equatable;
    public override int GetHashCode() => 1;
}
