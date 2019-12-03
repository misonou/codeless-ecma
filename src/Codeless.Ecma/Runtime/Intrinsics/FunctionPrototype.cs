using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionPrototype)]
  internal static class FunctionPrototype {
    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Caller([This] EcmaValue thisValue) {
      RuntimeFunction fn = thisValue.GetUnderlyingObject<RuntimeFunction>();
      if (fn.ContainsUseStrict) {
        throw new EcmaTypeErrorException(InternalString.Error.StrictMode);
      }
      RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
      bool returnCaller = false;
      for (; invocation != null; invocation = invocation.Parent) {
        if (returnCaller) {
          return invocation.FunctionObject;
        }
        if (invocation.FunctionObject == fn) {
          returnCaller = true;
        }
      }
      return EcmaValue.Null;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Arguments([This] EcmaValue thisValue) {
      RuntimeFunction fn = thisValue.GetUnderlyingObject<RuntimeFunction>();
      if (fn.ContainsUseStrict) {
        throw new EcmaTypeErrorException(InternalString.Error.StrictMode);
      }
      RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
      for (; invocation != null; invocation = invocation.Parent) {
        if (invocation.FunctionObject == fn) {
          return invocation.Arguments;
        }
      }
      return EcmaValue.Null;
    }

    [IntrinsicMember]
    public static EcmaValue Apply([This] EcmaValue thisValue, EcmaValue thisArg, EcmaValue args) {
      Guard.ArgumentIsCallable(thisValue);
      return thisValue.Call(thisArg, EcmaValueUtility.CreateListFromArrayLike(args));
    }

    [IntrinsicMember]
    public static EcmaValue Bind([This] EcmaValue thisValue, EcmaValue thisArg, params EcmaValue[] args) {
      RuntimeFunction fn = thisValue.GetUnderlyingObject<RuntimeFunction>();
      return new BoundRuntimeFunction(fn, thisArg, args);
    }

    [IntrinsicMember]
    public static EcmaValue Call([This] EcmaValue thisValue, EcmaValue thisArg, params EcmaValue[] args) {
      Guard.ArgumentIsCallable(thisValue);
      return thisValue.Call(thisArg, args);
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      Guard.ArgumentIsCallable(thisValue);
      RuntimeObject fn = thisValue.ToObject();
      if (fn is BoundRuntimeFunction boundFn) {
        fn = boundFn.TargetFunction;
      }
      EcmaValue name = fn[WellKnownProperty.Name];
      if (name == default) {
        name = "";
      }
      if (fn is ScriptFunction userFn) {
        return "function " + name + "(" + userFn.ParameterList + ") { " + userFn.Source + " }";
      }
      return "function " + name + "() { [native code] }";
    }
  }
}
