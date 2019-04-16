using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public delegate EcmaValue RuntimeFunctionDelegate(RuntimeFunctionInvocation invocation, EcmaValue[] args);

  public abstract class RuntimeFunction : RuntimeObject {
    public RuntimeFunction()
      : base(WellKnownObject.FunctionPrototype) { }

    public override sealed bool IsCallable => true;

    public RuntimeObject Prototype {
      get { return this.Get(WellKnownPropertyName.Prototype).ToObject(); }
    }

    public static RuntimeFunction GetExecutingFunction() {
      RuntimeFunctionInvocation current = RuntimeFunctionInvocation.Current;
      if (current == null) {
        return null;
      }
      return current.FunctionObject;
    }

    public static RuntimeFunction FromDelegate(Action<EcmaValue> fn) {
      return new DelegateFunction(fn);
    }

    public static RuntimeFunction FromDelegate(Action<EcmaValue, EcmaValue> fn) {
      return new DelegateFunction(fn);
    }

    public static RuntimeFunction FromDelegate(Func<EcmaValue> fn) {
      return new DelegateFunction(fn);
    }

    public static RuntimeFunction FromDelegate(Func<EcmaValue, EcmaValue> fn) {
      return new DelegateFunction(fn);
    }

    public RuntimeFunction Bind(EcmaValue thisArg, params EcmaValue[] arguments) {
      return new BoundRuntimeFunction(this, thisArg, arguments);
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this) { ThisValue = thisValue }) {
        return GetDelegate()(invocation, arguments);
      }
    }

    public override EcmaValue Construct(RuntimeObject newTarget, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this) { ThisValue = ConstructThisValue(newTarget), NewTarget = newTarget }) {
        return GetDelegate()(invocation, arguments);
      }
    }

    protected void InitProperty(string name, int length) {
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Length, new EcmaPropertyDescriptor(length, EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Name, new EcmaPropertyDescriptor(name, EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Arguments, new EcmaPropertyDescriptor(EcmaValue.Undefined, EcmaPropertyAttributes.None));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Caller, new EcmaPropertyDescriptor(EcmaValue.Undefined, EcmaPropertyAttributes.None));
    }

    protected internal virtual RuntimeFunctionDelegate GetDelegate() {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    protected virtual RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    public static explicit operator RuntimeFunction(WellKnownObject type) {
      return (RuntimeFunction)RuntimeRealm.GetRuntimeObject(type);
    }
  }
}
