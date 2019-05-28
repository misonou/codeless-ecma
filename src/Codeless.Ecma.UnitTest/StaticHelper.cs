using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public static class StaticHelper {
    public static EcmaValue _ => EcmaValue.Undefined;

    public static readonly RuntimeFunction ThrowTest262Exception = RuntimeFunction.Create(() => throw new Test262Exception());

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

    public static Func<EcmaValue> CreateFunction(IList count, string message) {
      return () => {
        count.Add(message);
        return _;
      };
    }

    public static RuntimeFunction CreateFunction(IList count, Func<EcmaValue> fn) {
      return RuntimeFunction.Create(() => {
        count.Add(null);
        return fn();
      });
    }

    public static RuntimeFunction CreateFunction(IList count, string message, Func<EcmaValue> fn) {
      return RuntimeFunction.Create(() => {
        count.Add(message);
        return fn();
      });
    }
  }
}
