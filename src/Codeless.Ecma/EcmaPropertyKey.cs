using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public enum WellKnownPropertyName {
    Length = 1,
    Name,
    Arguments,
    Caller,
    Constructor,
    Prototype,
    ToString,
    ValueOf,
    Configurable,
    Enumerable,
    Writable,
    Value,
    Get,
    Set,
    GetPrototypeOf,
    SetPrototypeOf,
    IsExtensible,
    PreventExtensions,
    GetOwnPropertyDescriptor,
    DefineProperty,
    Has,
    DeleteProperty,
    OwnKeys,
    Apply,
    Construct,
    LastIndex,
    Message,
    ToJson
  }

  [DebuggerStepThrough]
  public struct EcmaPropertyKey : IEquatable<EcmaPropertyKey>, IEcmaValueConvertible {
    private EcmaValue value;

    public EcmaPropertyKey(WellKnownPropertyName value)
      : this(new EcmaValue(new EcmaValueHandle((long)value), WellKnownPropertyNameBinder.Default)) { }

    public EcmaPropertyKey(WellKnownSymbol value)
      : this(new EcmaValue(new EcmaValueHandle((long)value), WellKnownSymbolBinder.Default)) { }

    public EcmaPropertyKey(Symbol value)
      : this(new EcmaValue(value)) { }

    public EcmaPropertyKey(string value)
      : this() {
      uint number;
      EcmaValueHandle handle;
      if (UInt32.TryParse(value, out number) && number < UInt32.MaxValue) {
        this.value = number;
      } else {
        this.value = value;
      }
    }

    public EcmaPropertyKey(int value)
      : this(new EcmaValue(value)) { }

    public EcmaPropertyKey(long value)
      : this(new EcmaValue(value)) { }

    private EcmaPropertyKey(EcmaValue value)
      : this() {
      this.value = value;
    }

    public bool IsSymbol {
      get { return value.Type == EcmaValueType.Symbol; }
    }

    public bool IsArrayIndex {
      get { return value.Type == EcmaValueType.Number; }
    }

    public bool IsCanonicalNumericIndex {
      get {
        if (value.Type == EcmaValueType.Symbol) {
          return false;
        }
        if (value.Type == EcmaValueType.Number) {
          return true;
        }
        double doubleValue;
        if (Double.TryParse(value.ToString(), out doubleValue) && Math.Floor(doubleValue) == doubleValue) {
          value = doubleValue;
          return true;
        }
        return false;
      }
    }

    public Symbol Symbol {
      get {
        if (!IsSymbol) {
          throw new InvalidOperationException();
        }
        return (Symbol)value.GetUnderlyingObject();
      }
    }

    public string Name {
      get { return value.ToString(); }
    }

    public long ArrayIndex {
      get {
        if (!IsArrayIndex) {
          throw new InvalidOperationException();
        }
        return value.ToUInt32();
      }
    }

    public EcmaValue CanonicalNumericIndex {
      get { return value.ToNumber(); }
    }

    public EcmaValue ToValue() {
      return value;
    }

    public static EcmaPropertyKey FromValue(EcmaValue v) {
      if (v.Type == EcmaValueType.Symbol || v.Type == EcmaValueType.Number) {
        return new EcmaPropertyKey(v);
      }
      return new EcmaPropertyKey(v.ToString());
    }

    [EcmaSpecification("IsPropertyKey", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsPropertyKey(EcmaValue v) {
      return v.Type == EcmaValueType.Symbol || v.Type == EcmaValueType.String;
    }

    public bool Equals(EcmaPropertyKey other) {
      return EcmaValue.Equals(other.value, value, EcmaValueComparison.SameValue);
    }

    public override bool Equals(object obj) {
      return obj is EcmaPropertyKey ? Equals((EcmaPropertyKey)obj) : base.Equals(obj);
    }

    public override int GetHashCode() {
      return value.GetHashCode();
    }

    public override string ToString() {
      return value.ToString();
    }

    public static implicit operator EcmaPropertyKey(WellKnownPropertyName value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(WellKnownSymbol value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(Symbol value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(string value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(int index) {
      return new EcmaPropertyKey(index);
    }

    public static implicit operator EcmaPropertyKey(long index) {
      return new EcmaPropertyKey(index);
    }

    private static bool IsWellKnownPropertyName(string name, out EcmaValueHandle value) {
      try {
        value = WellKnownPropertyNameBinder.Default.ToHandle(name);
        return true;
      } catch {
        value = default(EcmaValueHandle);
        return false;
      }
    }
  }
}
