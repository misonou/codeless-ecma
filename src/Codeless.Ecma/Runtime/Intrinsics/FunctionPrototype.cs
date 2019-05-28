using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionPrototype)]
  internal static class FunctionPrototype {
    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Caller([This] RuntimeFunction fn) {
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
    public static EcmaValue Arguments([This] RuntimeFunction fn) {
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
    public static EcmaValue Apply([This] RuntimeFunction fn, EcmaValue thisValue, EcmaValue args) {
      return fn.Call(thisValue, EcmaValueUtility.CreateListFromArrayLike(args));
    }

    [IntrinsicMember]
    public static EcmaValue Bind([This] RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      return new BoundRuntimeFunction(fn, thisValue, args);
    }

    [IntrinsicMember]
    public static EcmaValue Call([This] RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      return fn.Call(thisValue, args);
    }
  }
}