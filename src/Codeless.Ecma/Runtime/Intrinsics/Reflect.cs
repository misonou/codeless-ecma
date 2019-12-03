using System;
using System.Linq;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.Reflect, Global = true)]
  internal static class Reflect {
    [IntrinsicMember]
    public static EcmaValue GetPrototypeOf(EcmaValue target) {
      Guard.ArgumentIsObject(target);
      RuntimeObject proto = target.ToObject().GetPrototypeOf();
      return proto == null ? EcmaValue.Null : proto;
    }

    [IntrinsicMember]
    public static EcmaValue SetPrototypeOf(EcmaValue target, EcmaValue proto) {
      Guard.ArgumentIsObject(target);
      if (proto.Type != EcmaValueType.Null && proto.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
      }
      return target.ToObject().SetPrototypeOf(proto == EcmaValue.Null ? null : proto.ToObject());
    }

    [IntrinsicMember]
    public static EcmaValue OwnKeys(EcmaValue target) {
      Guard.ArgumentIsObject(target);
      return new EcmaArray(target.ToObject().GetOwnPropertyKeys().Select(v => v.ToValue()).ToList());
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Get(EcmaValue target, EcmaValue key, EcmaValue? receiver = default) {
      Guard.ArgumentIsObject(target);
      RuntimeObject obj = target.ToObject();
      return obj.Get(EcmaPropertyKey.FromValue(key), receiver.HasValue ? receiver.Value.ToObject() : obj);
    }

    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Set(EcmaValue target, EcmaValue key, EcmaValue value, EcmaValue? receiver = default) {
      Guard.ArgumentIsObject(target);
      RuntimeObject obj = target.ToObject();
      return obj.Set(EcmaPropertyKey.FromValue(key), value, receiver.HasValue ? receiver.Value.ToObject() : obj);
    }

    [IntrinsicMember]
    public static EcmaValue Has(EcmaValue target, EcmaValue key) {
      Guard.ArgumentIsObject(target);
      return target.ToObject().HasProperty(EcmaPropertyKey.FromValue(key));
    }

    [IntrinsicMember]
    public static EcmaValue IsExtensible(EcmaValue target) {
      Guard.ArgumentIsObject(target);
      return target.ToObject().IsExtensible;
    }

    [IntrinsicMember]
    public static EcmaValue PreventExtensions(EcmaValue target) {
      Guard.ArgumentIsObject(target);
      return target.ToObject().PreventExtensions();
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyDescriptor(EcmaValue target, EcmaValue key) {
      Guard.ArgumentIsObject(target);
      EcmaPropertyDescriptor descriptor = target.ToObject().GetOwnProperty(EcmaPropertyKey.FromValue(key));
      return descriptor != null ? descriptor.ToValue() : default;
    }

    [IntrinsicMember]
    public static EcmaValue DefineProperty(EcmaValue target, EcmaValue key, EcmaValue descriptor) {
      Guard.ArgumentIsObject(target);
      return target.ToObject().DefineOwnProperty(EcmaPropertyKey.FromValue(key), EcmaPropertyDescriptor.FromValue(descriptor));
    }

    [IntrinsicMember]
    public static EcmaValue DeleteProperty(EcmaValue target, EcmaValue key) {
      Guard.ArgumentIsObject(target);
      return target.ToObject().Delete(EcmaPropertyKey.FromValue(key));
    }

    [IntrinsicMember]
    public static EcmaValue Apply(EcmaValue target, EcmaValue thisArg, EcmaValue argumentsList) {
      return target.ToObject().Call(thisArg, EcmaValueUtility.CreateListFromArrayLike(argumentsList));
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Construct(EcmaValue target, EcmaValue argumentsList, EcmaValue? newTarget) {
      RuntimeObject obj = target.ToObject();
      return obj.Construct(EcmaValueUtility.CreateListFromArrayLike(argumentsList), newTarget.HasValue ? newTarget.Value.ToObject() : obj);
    }
  }
}
