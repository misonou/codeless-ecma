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
    private readonly MethodInfo method;
    private RuntimeFunctionDelegate fn;

    public NativeRuntimeFunction(string name, MethodInfo method) {
      this.method = method;
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Length, new EcmaPropertyDescriptor(GetFuncLength(method), EcmaPropertyAttributes.None));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Name, new EcmaPropertyDescriptor(name, EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Arguments, new EcmaPropertyDescriptor(EcmaValue.Undefined, EcmaPropertyAttributes.None));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Caller, new EcmaPropertyDescriptor(EcmaValue.Undefined, EcmaPropertyAttributes.None));

      IntrinsicConstructorAttribute attribute;
      if (method.HasAttribute(out attribute)) {
        constraint = attribute.Constraint;
      }
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.AlwaysConstruct) {
        return base.Construct(this, arguments);
      }
      if (constraint == NativeRuntimeFunctionConstraint.DenyCall) {
        throw new EcmaTypeErrorException("");
      }
      return base.Call(thisValue, arguments);
    }

    public override EcmaValue Construct(EcmaValue newTarget, params EcmaValue[] arguments) {
      if (constraint == NativeRuntimeFunctionConstraint.DenyConstruct) {
        throw new EcmaTypeErrorException("");
      }
      return base.Construct(newTarget, arguments);
    }

    public override RuntimeFunctionDelegate GetDelegate() {
      if (fn == null) {
        fn = NativeRuntimeFunctionCompiler.Compile(method);
      }
      return fn;
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
