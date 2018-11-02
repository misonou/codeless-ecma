using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class RuntimeObjectProxy : RuntimeObject {
    private readonly RuntimeObject handler;
    private readonly RuntimeObject target;

    [EcmaSpecification("ProxyCreate", EcmaSpecificationKind.AbstractOperations)]
    public RuntimeObjectProxy(RuntimeObject target, RuntimeObject handler)
      : base(WellKnownObject.ObjectPrototype) {
      // TODO
      this.target = target;
      this.handler = handler;
    }

    public RuntimeObject ProxyTarget {
      get { return target; }
    }

    public override bool IsExtensible {
      get {
        // TODO
        return base.IsExtensible;
      }
    }

    public override IList<EcmaPropertyKey> OwnPropertyKeys {
      get {
        // TODO
        return base.OwnPropertyKeys;
      }
    }

    public override bool PreventExtensions() {
      // TODO
      return base.PreventExtensions();
    }

    public override RuntimeObject GetPrototypeOf() {
      // TODO
      return base.GetPrototypeOf();
    }

    public override bool SetPrototypeOf(RuntimeObject proto) {
      // TODO
      return base.SetPrototypeOf(proto);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      EcmaValue method = handler.GetMethod(WellKnownPropertyName.GetOwnPropertyDescriptor);
      EcmaPropertyDescriptor targetDesc = base.GetOwnProperty(propertyKey);

      if (!method.IsNullOrUndefined) {
        EcmaValue result = method.Call(handler, target, propertyKey.ToValue());
        if (result.Type == EcmaValueType.Undefined) {
          if (targetDesc != null && (!targetDesc.Configurable.Value || !this.IsExtensible)) {
            throw new EcmaTypeErrorException("");
          }
          return null;
        }
        EcmaPropertyDescriptor resultDesc = EcmaPropertyDescriptor.FromValue(result);
        if (!EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref resultDesc, targetDesc, !this.IsExtensible) ||
            resultDesc.Configurable == false && (targetDesc == null || targetDesc.Configurable == true)) {
          throw new EcmaTypeErrorException("");
        }
        return resultDesc;
      }
      return targetDesc;
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      // TODO
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      // TODO
      return base.HasProperty(propertyKey);
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      // TODO
      return base.Get(propertyKey, receiver);
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      // TODO
      return base.Set(propertyKey, value, receiver);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      // TODO
      return base.Delete(propertyKey);
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      // TODO
      return base.Call(thisValue, arguments);
    }

    public override EcmaValue Construct(EcmaValue newTarget, params EcmaValue[] arguments) {
      // TODO
      return base.Construct(newTarget, arguments);
    }
  }
}
