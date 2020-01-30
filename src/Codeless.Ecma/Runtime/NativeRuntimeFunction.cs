using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.Runtime {
  internal enum NativeRuntimeFunctionConstraint {
    None,
    AlwaysConstruct,
    DenyCall,
    DenyConstruct
  }

  internal class NativeRuntimeFunction : RuntimeFunction {
    private static readonly ConcurrentDictionary<MethodInfo, RuntimeFunctionDelegate> dictionary = new ConcurrentDictionary<MethodInfo, RuntimeFunctionDelegate>();
    private static readonly MethodInfo createFromConstructor = typeof(RuntimeObject).GetMethod("CreateFromConstructor");
    private static readonly MethodInfo createFromConstructorDefault = createFromConstructor.MakeGenericMethod(typeof(EcmaObject));

    private readonly NativeRuntimeFunctionConstraint constraint;
    private readonly WellKnownObject defaultProto = WellKnownObject.ObjectPrototype;
    private readonly MethodInfo constructThisValue;
    private readonly MethodInfo method;
    private RuntimeFunctionDelegate fn;

    public NativeRuntimeFunction(string name, MethodInfo method)
      : this(name, method, WellKnownObject.FunctionPrototype) { }

    public NativeRuntimeFunction(string name, MethodInfo method, WellKnownObject proto)
      : base(proto) {
      Guard.ArgumentNotNull(method, "method");
      this.method = method;

      Type runtimeObjectType = null;
      if (method.HasAttribute(out IntrinsicConstructorAttribute attribute)) {
        constraint = attribute.Constraint;
        runtimeObjectType = attribute.ObjectType;
        if (attribute.Prototype != 0) {
          defaultProto = attribute.Prototype;
        }
      } else if (method.HasAttribute(out IntrinsicMemberAttribute a2)) {
        constraint = NativeRuntimeFunctionConstraint.DenyConstruct;
        if (name == null) {
          name = a2.Name;
        }
      } else {
        SetPrototypeInternal(new EcmaObject());
        if (name == null) {
          name = "";
        }
      }
      this.Source = "function " + name + "() { [native code] }";
      InitProperty(name, GetFuncLength(method));
      constructThisValue = runtimeObjectType == null || runtimeObjectType == typeof(EcmaObject) ? createFromConstructorDefault : createFromConstructor.MakeGenericMethod(runtimeObjectType);
    }

    public override bool IsConstructor {
      get { return constraint != NativeRuntimeFunctionConstraint.DenyConstruct; }
    }

    [DebuggerStepThrough]
    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.AlwaysConstruct) {
        return base.Construct(arguments, this);
      }
      if (constraint == NativeRuntimeFunctionConstraint.DenyCall) {
        throw new EcmaTypeErrorException(InternalString.Error.MustCallAsConstructor);
      }
      return base.Call(thisValue, arguments);
    }

    [DebuggerStepThrough]
    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return GetDelegate()(invocation, arguments, method.IsStatic ? null : invocation.ThisValue.GetUnderlyingObject());
    }

    [DebuggerStepThrough]
    protected RuntimeFunctionDelegate GetDelegate() {
      if (fn == null) {
        fn = dictionary.GetOrAdd(method, NativeRuntimeFunctionCompiler.Compile);
      }
      return fn;
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      try {
        return (RuntimeObject)constructThisValue.Invoke(null, new object[] { newTarget, defaultProto });
      } catch (TargetInvocationException ex) {
        throw ex.InnerException;
      }
    }

    private static int GetFuncLength(MethodInfo m) {
      IntrinsicMemberAttribute attribute;
      if (m.HasAttribute(out attribute) && attribute.FunctionLength >= 0) {
        return attribute.FunctionLength;
      }
      return m.GetParameters().Count(pi => !Attribute.IsDefined(pi, typeof(ThisAttribute)) && !Attribute.IsDefined(pi, typeof(NewTargetAttribute)));
    }
  }
}
