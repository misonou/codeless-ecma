using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ErrorConstructor)]
  internal static class ErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError))]
    public static EcmaValue Error([This] EcmaValue thisValue, EcmaValue message) {
      EcmaError error = thisValue.GetUnderlyingObject<EcmaError>();
      error.Init(message != default ? message.ToString(true) : null);
      return thisValue;
    }
  }

  #region NativeError
  [IntrinsicObject(WellKnownObject.EvalError)]
  internal static class EvalErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue EvalError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }

  [IntrinsicObject(WellKnownObject.RangeError)]
  internal static class RangeErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue RangeError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }

  [IntrinsicObject(WellKnownObject.ReferenceError)]
  internal static class ReferenceErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue ReferenceError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }

  [IntrinsicObject(WellKnownObject.SyntaxError)]
  internal static class SyntaxErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue SyntaxError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }

  [IntrinsicObject(WellKnownObject.TypeError)]
  internal static class TypeErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue TypeError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }

  [IntrinsicObject(WellKnownObject.UriError)]
  internal static class UriErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Name = "URIError", ObjectType = typeof(EcmaError), SuperClass = WellKnownObject.ErrorConstructor)]
    public static EcmaValue UriError([This] EcmaValue thisValue, EcmaValue message) {
      return ErrorConstructor.Error(thisValue, message);
    }
  }
  #endregion
}
