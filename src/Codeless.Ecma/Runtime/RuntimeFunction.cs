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

    public RuntimeObject HomeObject { get; private set; }

    public override sealed bool IsCallable => true;

    protected override string ToStringTag => InternalString.ObjectTag.Function;

    public RuntimeObject Prototype {
      get { return this.Get(WellKnownProperty.Prototype).ToObject(); }
    }

    public static RuntimeFunction GetExecutingFunction() {
      RuntimeFunctionInvocation current = RuntimeFunctionInvocation.Current;
      if (current == null) {
        return null;
      }
      return current.FunctionObject;
    }

    public static RuntimeFunction Create(Action fn) {
      return new DelegateRuntimeFunction(fn);
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

    public static RuntimeFunction Create(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue> fn) {
      return new DelegateRuntimeFunction(fn);
    }

    public RuntimeFunction AsDerivedClassConstructorOf(RuntimeObject super) {
      if (this is IntrinsicFunction) {
        throw new InvalidOperationException("Cannot modified HomeObject internal slot for intrinsic function");
      }
      RuntimeObject proto = Create(GetPrototypeFromConstructor(super, WellKnownObject.ObjectPrototype));
      SetPrototypeOf(super);
      SetPrototypeInternal(proto);
      this.HomeObject = this;
      return this;
    }

    public RuntimeFunction AsHomedMethodOf(RuntimeObject homeObject) {
      if (this is IntrinsicFunction) {
        throw new InvalidOperationException("Cannot modified HomeObject internal slot for intrinsic function");
      }
      Guard.ArgumentNotNull(homeObject, "homeObject");
      this.HomeObject = homeObject;
      return this;
    }

    public RuntimeFunction Bind(EcmaValue thisArg, params EcmaValue[] arguments) {
      return new BoundRuntimeFunction(this, thisArg, arguments);
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this, thisValue, arguments)) {
        return Invoke(invocation, arguments);
      }
    }

    public override EcmaValue Construct(EcmaValue[] arguments, RuntimeObject newTarget) {
      Guard.ArgumentNotNull(arguments, "arguments");
      Guard.ArgumentNotNull(newTarget, "newTarget");
      if (!this.IsConstructor) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      if (!newTarget.IsConstructor) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      RuntimeObject thisValue = default;
      if (this.HomeObject == null) {
        thisValue = ConstructThisValue(newTarget);
      }
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this, thisValue, arguments, newTarget)) {
        EcmaValue returnValue = Invoke(invocation, arguments);
        if (returnValue.Type == EcmaValueType.Object) {
          return returnValue;
        }
        if (invocation.Super != null && !invocation.Super.ConstructorInvoked) {
          throw new EcmaReferenceErrorException(InternalString.Error.SuperConstructorNotCalled);
        }
        return invocation.ThisValue;
      }
    }

    protected virtual EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    protected virtual RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      throw new EcmaTypeErrorException(InternalString.Error.IllegalInvocation);
    }

    protected void InitProperty(string name, int length) {
      Guard.ArgumentNotNull(name, "name");
      DefineOwnPropertyNoChecked(WellKnownProperty.Length, new EcmaPropertyDescriptor(length, EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownProperty.Name, new EcmaPropertyDescriptor(name, EcmaPropertyAttributes.Configurable));
    }

    protected void SetPrototypeInternal(RuntimeObject proto) {
      proto.DefineOwnProperty(WellKnownProperty.Constructor, new EcmaPropertyDescriptor(this, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
      DefineOwnProperty(WellKnownProperty.Prototype, new EcmaPropertyDescriptor(proto, EcmaPropertyAttributes.Writable));
    }

    public static implicit operator RuntimeFunction(Delegate del) {
      return del != null ? new DelegateRuntimeFunction(del) : null;
    }

    public static explicit operator RuntimeFunction(WellKnownObject type) {
      return (RuntimeFunction)RuntimeRealm.Current.GetRuntimeObject(type);
    }
  }
}
