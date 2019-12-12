using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class SuperAccessor {
    private readonly RuntimeFunctionInvocation invocation;
    private readonly RuntimeObject homeObject;

    internal SuperAccessor(RuntimeFunctionInvocation invocation, RuntimeObject homeObject) {
      this.invocation = invocation;
      this.homeObject = homeObject;
    }

    public bool ConstructorInvoked { get; private set; }

    public EcmaValue this[EcmaPropertyKey index] {
      get { return Get(index); }
      set { Set(index, value); }
    }

    public EcmaValue this[string key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[int key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[long key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[Symbol key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[EcmaValue key] {
      get { return this[EcmaPropertyKey.FromValue(key)]; }
      set { this[EcmaPropertyKey.FromValue(key)] = value; }
    }

    public EcmaValue Get(EcmaPropertyKey propertyKey) {
      return homeObject.GetPrototypeOf().Get(propertyKey, invocation.ThisValue.ToObject());
    }

    public EcmaValue Set(EcmaPropertyKey propertyKey, EcmaValue value) {
      return homeObject.GetPrototypeOf().Set(propertyKey, value, invocation.ThisValue.ToObject());
    }

    public EcmaValue Construct(params EcmaValue[] arguments) {
      if (this.ConstructorInvoked) {
        throw new EcmaReferenceErrorException(InternalString.Error.SuperConstructorAlreadyCalled);
      }
      EcmaValue result = homeObject.GetPrototypeOf().Construct(arguments, invocation.NewTarget);
      invocation.ThisValue = result;
      this.ConstructorInvoked = true;
      return result;
    }

    public EcmaValue Invoke(EcmaPropertyKey propertyKey, params EcmaValue[] arguments) {
      RuntimeObject method = homeObject.GetPrototypeOf().GetMethod(propertyKey);
      if (method == null) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      return method.Call(invocation.ThisValue, arguments);
    }
  }
}
