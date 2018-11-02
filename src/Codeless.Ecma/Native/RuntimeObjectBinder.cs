using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Native {
  internal class RuntimeObjectBinder : ObjectBinder {
    private static readonly WellKnownPropertyName[] convertToPrimitiveMethodString = new[] { WellKnownPropertyName.ToString, WellKnownPropertyName.ValueOf };
    private static readonly WellKnownPropertyName[] convertToPrimitiveMethodNumber = new[] { WellKnownPropertyName.ValueOf, WellKnownPropertyName.ToString };

    public static readonly RuntimeObjectBinder Default = new RuntimeObjectBinder();

    protected RuntimeObjectBinder() { }

    public override string GetToStringTag(EcmaValueHandle handle) {
      RuntimeObject obj = (RuntimeObject)handle.GetTargetAsGCHandle();
      if (obj.IsCallable) {
        return InternalString.ObjectTag.Function;
      }
      IEcmaIntrinsicObject t = obj as IEcmaIntrinsicObject;
      if (t != null) {
        return t.IntrinsicValue.ToStringTag;
      }
      EcmaValue customTag;
      if (TryGet(obj, WellKnownSymbol.ToStringTag, out customTag)) {
        return customTag.ToString();
      }
      return base.GetToStringTag(handle);
    }

    public override bool IsCallable(EcmaValueHandle handle) {
      return ((RuntimeObject)handle.GetTargetAsGCHandle()).IsCallable;
    }

    public override bool IsExtensible(EcmaValueHandle handle) {
      return ((RuntimeObject)handle.GetTargetAsGCHandle()).IsExtensible;
    }

    public override EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      RuntimeObject obj = (RuntimeObject)handle.GetTargetAsGCHandle();
      if (obj.IsCallable) {
        return obj.Call(thisValue, arguments);
      }
      return base.Call(handle, thisValue, arguments);
    }

    protected override RuntimeObject ToRuntimeObject(object target) {
      return (RuntimeObject)target;
    }

    protected override EcmaValue ToPrimitive(object target, EcmaPreferredPrimitiveType kind) {
      EcmaValue result;
      RuntimeObject obj = (RuntimeObject)target;
      if (obj.IsWellKnownSymbolOverriden(WellKnownSymbol.ToPrimitive)) {
        if (TryConvertToPrimitive(obj, Symbol.ToPrimitive, out result)) {
          return result;
        }
        throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
      }
      WellKnownPropertyName[] m = kind == EcmaPreferredPrimitiveType.String ? convertToPrimitiveMethodString : convertToPrimitiveMethodNumber;
      if (TryConvertToPrimitive(obj, m[0], out result) || TryConvertToPrimitive(obj, m[1], out result)) {
        return result;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
    }

    protected override IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target) {
      RuntimeObject obj = (RuntimeObject)target;
      return obj.OwnPropertyKeys.Where(v => obj.GetOwnProperty(v).Enumerable.Value);
    }

    protected override bool HasProperty(object target, EcmaPropertyKey name) {
      return ((RuntimeObject)target).HasProperty(name);
    }

    protected override bool HasOwnProperty(object target, EcmaPropertyKey name) {
      return ((RuntimeObject)target).GetOwnProperty(name) != null;
    }

    protected override bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value) {
      RuntimeObject obj = (RuntimeObject)target;
      if (obj.HasProperty(name)) {
        value = obj.Get(name);
        return true;
      }
      value = default(EcmaValue);
      return false;
    }

    protected override bool TrySet(object target, EcmaPropertyKey name, EcmaValue value) {
      return ((RuntimeObject)target).Set(name, value);
    }

    private bool TryConvertToPrimitive(RuntimeObject obj, EcmaPropertyKey name, out EcmaValue result) {
      EcmaValue method = obj.GetMethod(name);
      if (!method.IsNullOrUndefined) {
        result = method.Call(new EcmaValue(obj));
        return result.Type != EcmaValueType.Object;
      }
      result = default(EcmaValue);
      return false;
    }
  }
}
