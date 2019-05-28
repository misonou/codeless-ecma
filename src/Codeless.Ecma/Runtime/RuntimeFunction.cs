using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public delegate EcmaValue RuntimeFunctionDelegate(RuntimeFunctionInvocation invocation, EcmaValue[] args, object target);

  public abstract class RuntimeFunction : RuntimeObject {
    public RuntimeFunction()
      : base(WellKnownObject.FunctionPrototype) { }

    public bool ContainsUseStrict { get; protected set; }

    public override sealed bool IsCallable => true;

    protected override string ToStringTag => InternalString.ObjectTag.Function;

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

    public static RuntimeFunction Create(Action<EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Action<EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Action<EcmaValue, EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Func<EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Func<EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Func<EcmaValue, EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public static RuntimeFunction Create(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public RuntimeFunction Bind(EcmaValue thisArg, params EcmaValue[] arguments) {
      return new BoundRuntimeFunction(this, thisArg, arguments);
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this, arguments) { ThisValue = thisValue }) {
        return Invoke(invocation, arguments);
      }
    }

    public override EcmaValue Construct(EcmaValue[] arguments, RuntimeObject newTarget) {
      Guard.ArgumentNotNull(arguments, "arguments");
      RuntimeObject thisValue = ConstructThisValue(newTarget);
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this, arguments) { ThisValue = thisValue, NewTarget = newTarget }) {
        EcmaValue returnValue = Invoke(invocation, arguments);
        return returnValue.Type == EcmaValueType.Object ? returnValue : thisValue;
      }
    }

    protected virtual EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    protected virtual RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    protected void InitProperty(string name, int length) {
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Length, new EcmaPropertyDescriptor(length, EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Name, new EcmaPropertyDescriptor(name, EcmaPropertyAttributes.Configurable));
    }

    internal void SetPrototypeInternal(RuntimeObject proto, EcmaPropertyAttributes protoAttributes) {
      proto.DefineOwnProperty("constructor", new EcmaPropertyDescriptor(this, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
      DefineOwnProperty("prototype", new EcmaPropertyDescriptor(proto, protoAttributes));
    }

    public static explicit operator RuntimeFunction(WellKnownObject type) {
      return (RuntimeFunction)RuntimeRealm.GetRuntimeObject(type);
    }
  }
}
