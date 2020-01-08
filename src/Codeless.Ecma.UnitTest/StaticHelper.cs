using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Harness;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public static class StaticHelper {
    public static readonly ArrayList Logs = new ArrayList();

    public static readonly Func<EcmaValue> Noop = () => EcmaValue.Undefined;

    public static readonly Action DerivedCtor = () => Global.Super.Construct(EcmaValueUtility.CreateListFromArrayLike(Global.Arguments));

    public static readonly Action UnexpectedFulfill = () => NUnit.Framework.Assert.Fail("Promise should not be fulfilled");

    public static readonly Action UnexpectedReject = () => NUnit.Framework.Assert.Fail("Promise should not be rejected");

    public static readonly Func<EcmaValue> ThrowTest262Exception = () => throw new Test262Exception();

    public static Func<EcmaValue> ThrowTest262WithMessage(string message) {
      return () => throw new Test262Exception(message);
    }

    public static EcmaValue CreateObject(object anonymousObject) {
      EcmaValue obj = new EcmaObject();
      foreach (PropertyInfo property in anonymousObject.GetType().GetProperties()) {
        object value = property.GetValue(anonymousObject);
        obj[property.Name] = new EcmaValue(value);
      }
      return obj;
    }

    public static EcmaValue CreateObject(Func<EcmaValue> toString = null, Func<EcmaValue> valueOf = null, Func<EcmaValue> toPrimitive = null) {
      return CreateObject(toString == null ? null : RuntimeFunction.Create(toString), valueOf == null ? null : RuntimeFunction.Create(valueOf), toPrimitive == null ? null : RuntimeFunction.Create(toPrimitive));
    }

    public static EcmaValue CreateObject(RuntimeFunction toString = null, RuntimeFunction valueOf = null, RuntimeFunction toPrimitive = null) {
      Hashtable properties = new Hashtable();
      if (toString != null) {
        properties["toString"] = toString;
      }
      if (valueOf != null) {
        properties["valueOf"] = valueOf;
      }
      if (toPrimitive != null) {
        properties[Symbol.ToPrimitive] = toPrimitive;
      }
      return new EcmaObject(properties);
    }

    public static EcmaValue CreateObject(params (EcmaValue, EcmaValue)[] properties) {
      EcmaValue obj = new EcmaObject();
      foreach ((EcmaValue propertyKey, EcmaValue value) in properties) {
        obj[propertyKey] = value;
      }
      return obj;
    }

    public static EcmaValue CreateObject(params (EcmaValue, Func<EcmaValue> get, Func<EcmaValue, EcmaValue> set)[] properties) {
      EcmaValue obj = new EcmaObject();
      foreach ((EcmaValue propertyKey, Func<EcmaValue> get, Func<EcmaValue, EcmaValue> set) in properties) {
        DefineProperty(obj, propertyKey, enumerable: true, configurable: true, get: get, set: set);
      }
      return obj;
    }

    public static void DefineProperty(EcmaValue obj, EcmaValue propertyKey, bool? writable = null, bool? enumerable = null, bool? configurable = null, EcmaValue? value = null, Func<EcmaValue> get = null, Func<EcmaValue, EcmaValue> set = null) {
      Hashtable properties = new Hashtable();
      if (writable != null) {
        properties["writable"] = writable.Value;
      }
      if (enumerable != null) {
        properties["enumerable"] = enumerable.Value;
      }
      if (configurable != null) {
        properties["configurable"] = configurable.Value;
      }
      if (value != null) {
        properties["value"] = value.Value;
      }
      if (get != null) {
        properties["get"] = RuntimeFunction.Create(get);
      }
      if (set != null) {
        properties["set"] = RuntimeFunction.Create(set);
      }
      Global.Object.Invoke("defineProperty", obj, propertyKey, new EcmaObject(properties));
    }

    public static EcmaValue CreateProxyCompleteTraps(EcmaValue target, EcmaValue overrides) {
      EcmaValue handler = CreateObject(new {
        getPrototypeOf = overrides["getPrototypeOf"] || ThrowTest262WithMessage("[[GetPrototypeOf]] trap called"),
        setPrototypeOf = overrides["setPrototypeOf"] || ThrowTest262WithMessage("[[SetPrototypeOf]] trap called"),
        isExtensible = overrides["isExtensible"] || ThrowTest262WithMessage("[[IsExtensible]] trap called"),
        preventExtensions = overrides["preventExtensions"] || ThrowTest262WithMessage("[[PreventExtensions]] trap called"),
        getOwnPropertyDescriptor = overrides["getOwnPropertyDescriptor"] || ThrowTest262WithMessage("[[GetOwnProperty]] trap called"),
        has = overrides["has"] || ThrowTest262WithMessage("[[HasProperty]] trap called"),
        get = overrides["get"] || ThrowTest262WithMessage("[[Get]] trap called"),
        set = overrides["set"] || ThrowTest262WithMessage("[[Set]] trap called"),
        deleteProperty = overrides["deleteProperty"] || ThrowTest262WithMessage("[[Delete]] trap called"),
        defineProperty = overrides["defineProperty"] || ThrowTest262WithMessage("[[DefineOwnProperty]] trap called"),
        ownKeys = overrides["ownKeys"] || ThrowTest262WithMessage("[[OwnPropertyKeys]] trap called"),
        apply = overrides["apply"] || ThrowTest262WithMessage("[[Call]] trap called"),
        construct = overrides["construct"] || ThrowTest262WithMessage("[[Construct]] trap called"),
        enumerate = ThrowTest262WithMessage("[[Enumerate]] trap called: this trap has been removed")
      });
      return Global.Proxy.Construct(target, handler);
    }

    public static Action Intercept(Action fn, string message = null) {
      return () => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        fn();
      };
    }

    public static Action<EcmaValue> Intercept(Action<EcmaValue> fn, string message = null) {
      return (a) => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        fn(a);
      };
    }

    public static Action<EcmaValue, EcmaValue> Intercept(Action<EcmaValue, EcmaValue> fn, string message = null) {
      return (a, b) => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        fn(a, b);
      };
    }

    public static Func<EcmaValue> Intercept(Func<EcmaValue> fn, string message = null) {
      return () => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        return fn();
      };
    }

    public static Func<EcmaValue, EcmaValue> Intercept(Func<EcmaValue, EcmaValue> fn, string message = null) {
      return (a) => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        return fn(a);
      };
    }

    public static Func<EcmaValue, EcmaValue, EcmaValue> Intercept(Func<EcmaValue, EcmaValue, EcmaValue> fn, string message = null) {
      return (a, b) => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        return fn(a, b);
      };
    }

    public static Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue> Intercept(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue> fn, string message = null) {
      return (a, b, c) => {
        Logs.Add(message != null ? String.Format(message, Global.Arguments.Select(v => new ValueHolder(v)).ToArray()) : null);
        return fn(a, b, c);
      };
    }

    public static void TestWithTypedArrayConstructors(Action<RuntimeFunction> action, RuntimeFunction[] constructors = null) {
      foreach (RuntimeFunction constructor in constructors ?? new[] { Global.Int8Array, Global.Int16Array, Global.Int32Array, Global.Uint8Array, Global.Uint8ClampedArray, Global.Uint16Array, Global.Uint32Array, Global.Float32Array, Global.Float64Array }) {
        Logs.Clear();
        action(constructor);
      }
    }

    public static void TestTypedArrayConversion(Action<RuntimeFunction, EcmaValue, EcmaValue, EcmaValue> action) {
      TestWithTypedArrayConstructors(TA => {
        EcmaValue values = ByteConversionValues.Values;
        EcmaValue expected = ByteConversionValues.Expected[TA["name"].Invoke("slice", 0, -5)];
        for (long i = 0, len = values["length"].ToLength(); i < len; i++) {
          action(TA, values[i], expected[i], expected[i] == 0 ? 1 : 0);
        }
      });
    }

    public static void DetachBuffer(EcmaValue value) {
      switch (value.ToObject()) {
        case IArrayBufferView view:
          view.Buffer.Detach();
          break;
        case ArrayBuffer buffer:
          buffer.Detach();
          break;
      }
    }

    public static IDisposable TempProperty(EcmaValue obj, EcmaPropertyKey key, EcmaValue value) {
      return new TempPropertyScope(obj.ToObject(), key, new EcmaPropertyDescriptor(value));
    }

    public static IDisposable TempProperty(EcmaValue obj, EcmaPropertyKey key, EcmaPropertyDescriptor descriptor) {
      return new TempPropertyScope(obj.ToObject(), key, descriptor);
    }

    public static string FormatArguments(EcmaValue thisArg, EcmaValue[] args) {
      return String.Format("(this = {0}, arguments = {1})", InspectorUtility.WriteValue(thisArg), String.Join(", ", args.Select(InspectorUtility.WriteValue)));
    }

    private class ValueHolder {
      private EcmaValue value;

      public ValueHolder(EcmaValue value) {
        this.value = value;
      }

      public override string ToString() {
        return value.ToString();
      }
    }

    private class TempPropertyScope : IDisposable {
      private readonly RuntimeObject obj;
      private readonly EcmaPropertyKey key;
      private readonly EcmaPropertyDescriptor previous;

      public TempPropertyScope(RuntimeObject obj, EcmaPropertyKey key, EcmaPropertyDescriptor descriptor) {
        if (descriptor.IsDataDescriptor && !descriptor.HasWritable) {
          descriptor.Writable = true;
        }
        if (!descriptor.HasEnumerable) {
          descriptor.Enumerable = true;
        }
        if (!descriptor.HasConfigurable) {
          descriptor.Configurable = true;
        }
        this.obj = obj;
        this.key = key;
        this.previous = obj.GetOwnProperty(key);
        obj.DefinePropertyOrThrow(key, descriptor);
      }

      public void Dispose() {
        try {
          if (this.previous != null) {
            obj.DefinePropertyOrThrow(key, previous);
          } else {
            obj.DeletePropertyOrThrow(key);
          }
        } catch {
          NUnit.Framework.Assume.That(false, "Unable to remove temporary property {0} on object {1}", key.ToString(), InspectorUtility.WriteValue(obj));
        }
      }
    }
  }
}
