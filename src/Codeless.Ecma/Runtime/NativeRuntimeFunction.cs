using Codeless.Ecma.Runtime.Intrinsics;
using System;
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
    private readonly NativeRuntimeFunctionConstraint constraint;
    private readonly WellKnownObject defaultProto = WellKnownObject.ObjectPrototype;
    private readonly Type runtimeObjectType;
    private readonly MethodInfo method;
    private RuntimeFunctionDelegate fn;

    public NativeRuntimeFunction(string name, MethodInfo method) {
      Guard.ArgumentNotNull(method, "method");
      this.method = method;
      InitProperty(name, GetFuncLength(method));

      if (method.HasAttribute(out IntrinsicConstructorAttribute attribute)) {
        constraint = attribute.Constraint;
        runtimeObjectType = attribute.ObjectType;
        if (method.DeclaringType.HasAttribute(out IntrinsicObjectAttribute a1)) {
          defaultProto = RuntimeRealm.GetPrototypeOf(a1.ObjectType);
        }
      } else {
        constraint = NativeRuntimeFunctionConstraint.DenyConstruct;
      }
      runtimeObjectType = runtimeObjectType ?? typeof(EcmaObject);
    }

    public override bool IsConstructor {
      get { return constraint != NativeRuntimeFunctionConstraint.DenyConstruct; }
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.AlwaysConstruct) {
        return base.Construct(this, arguments);
      }
      if (constraint == NativeRuntimeFunctionConstraint.DenyCall) {
        throw new EcmaTypeErrorException(InternalString.Error.MustCallAsConstructor);
      }
      return base.Call(thisValue, arguments);
    }

    public override EcmaValue Construct(RuntimeObject newTarget, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.DenyConstruct) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      return base.Construct(newTarget, arguments);
    }

    protected internal override RuntimeFunctionDelegate GetDelegate() {
      if (fn == null) {
        fn = NativeRuntimeFunctionCompiler.Compile(method);
      }
      return fn;
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      return (RuntimeObject)typeof(RuntimeObject).GetMethod("CreateFromConstructor").MakeGenericMethod(runtimeObjectType).Invoke(null, new object[] { defaultProto, newTarget });
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
