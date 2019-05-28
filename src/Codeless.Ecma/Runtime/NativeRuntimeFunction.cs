using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    private readonly NativeRuntimeFunctionConstraint constraint;
    private readonly WellKnownObject defaultProto = WellKnownObject.ObjectPrototype;
    private readonly MethodInfo constructThisValue;
    private readonly MethodInfo method;
    private RuntimeFunctionDelegate fn;

    public NativeRuntimeFunction(string name, MethodInfo method) {
      Guard.ArgumentNotNull(method, "method");
      this.method = method;
      InitProperty(name, GetFuncLength(method));

      Type runtimeObjectType = null;
      if (method.HasAttribute(out IntrinsicConstructorAttribute attribute)) {
        constraint = attribute.Constraint;
        runtimeObjectType = attribute.ObjectType;
        if (method.DeclaringType.HasAttribute(out IntrinsicObjectAttribute a1)) {
          defaultProto = RuntimeRealm.GetPrototypeOf(a1.ObjectType);
        }
      } else if (method.HasAttribute(out IntrinsicMemberAttribute _)) {
        constraint = NativeRuntimeFunctionConstraint.DenyConstruct;
      } else {
        SetPrototypeInternal(new EcmaObject(), EcmaPropertyAttributes.Writable);
      }
      constructThisValue = createFromConstructor.MakeGenericMethod(runtimeObjectType ?? typeof(EcmaObject));
    }

    public override bool IsConstructor {
      get { return constraint != NativeRuntimeFunctionConstraint.DenyConstruct; }
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.AlwaysConstruct) {
        return base.Construct(arguments, this);
      }
      if (constraint == NativeRuntimeFunctionConstraint.DenyCall) {
        throw new EcmaTypeErrorException(InternalString.Error.MustCallAsConstructor);
      }
      return base.Call(thisValue, arguments);
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return GetDelegate()(invocation, arguments, method.IsStatic ? null : invocation.ThisValue.GetUnderlyingObject());
    }

    protected RuntimeFunctionDelegate GetDelegate() {
      if (fn == null) {
        fn = dictionary.GetOrAdd(method, NativeRuntimeFunctionCompiler.Compile);
      }
      return fn;
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      if (constraint == NativeRuntimeFunctionConstraint.DenyConstruct) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      try {
        return (RuntimeObject)constructThisValue.Invoke(null, new object[] { defaultProto, newTarget });
      } catch (TargetInvocationException ex) {
        throw ex.InnerException;
      }
    }

    private static int GetFuncLength(MethodInfo m) {
      IntrinsicMemberAttribute attribute;
      if (m.HasAttribute(out attribute) && attribute.FunctionLength >= 0) {
        return attribute.FunctionLength;
      }
      return m.GetParameters().Count(pi => !Attribute.IsDefined(pi, typeof(ThisAttribute)) && !Attribute.IsDefined(pi, typeof(NewTargetAttribute)) && !Attribute.IsDefined(pi, typeof(ParamArrayAttribute)));
    }
  }
}
