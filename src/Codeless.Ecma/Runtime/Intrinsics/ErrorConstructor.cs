using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ErrorConstructor)]
  internal static class ErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue Error([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return CreateError(constructor, message, WellKnownObject.ErrorPrototype, new Exception());
    }

    public static RuntimeObject CreateError(RuntimeObject constructor, EcmaValue message, WellKnownObject defaultProto, Exception ex) {
      RuntimeObject err = new PrimitiveObject(new EcmaValue(ex), defaultProto, constructor);
      if (message != default) {
        err.DefinePropertyOrThrow(WellKnownProperty.Message, new EcmaPropertyDescriptor(message.ToString(), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
      return err;
    }

    public static RuntimeObject CreateError(EcmaException exception) {
      Guard.ArgumentNotNull(exception, "exception");
      RuntimeObject err = new PrimitiveObject(new EcmaValue(exception), GetPrototype(exception));
      if (!String.IsNullOrEmpty(exception.Message)) {
        err.DefinePropertyOrThrow(WellKnownProperty.Message, new EcmaPropertyDescriptor(exception.Message.ToString(), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
      return err;
    }

    private static WellKnownObject GetPrototype(EcmaException ex) {
      switch (ex.ErrorType) {
        case EcmaNativeErrorType.EvalError:
          return WellKnownObject.EvalErrorPrototype;
        case EcmaNativeErrorType.RangeError:
          return WellKnownObject.RangeErrorPrototype;
        case EcmaNativeErrorType.ReferenceError:
          return WellKnownObject.ReferenceErrorPrototype;
        case EcmaNativeErrorType.SyntaxError:
          return WellKnownObject.SyntaxErrorPrototype;
        case EcmaNativeErrorType.TypeError:
          return WellKnownObject.TypeErrorPrototype;
        case EcmaNativeErrorType.UriError:
          return WellKnownObject.UriErrorPrototype;
        default:
          return WellKnownObject.ErrorPrototype;
      }
    }
  }

  #region NativeError
  [IntrinsicObject(WellKnownObject.EvalError)]
  internal static class EvalErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue EvalError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.EvalErrorPrototype, new EcmaEvalErrorException(message.ToString()));
    }
  }

  [IntrinsicObject(WellKnownObject.RangeError)]
  internal static class RangeErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue RangeError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.RangeErrorPrototype, new EcmaRangeErrorException(message.ToString()));
    }
  }

  [IntrinsicObject(WellKnownObject.ReferenceError)]
  internal static class ReferenceErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue ReferenceError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.ReferenceErrorPrototype, new EcmaReferenceErrorException(message.ToString()));
    }
  }

  [IntrinsicObject(WellKnownObject.SyntaxError)]
  internal static class SyntaxErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue SyntaxError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.SyntaxErrorPrototype, new EcmaSyntaxErrorException(message.ToString()));
    }
  }

  [IntrinsicObject(WellKnownObject.TypeError)]
  internal static class TypeErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue TypeError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.TypeErrorPrototype, new EcmaTypeErrorException(message.ToString()));
    }
  }

  [IntrinsicObject(WellKnownObject.UriError)]
  internal static class UriErrorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    [IntrinsicMember("URIError")]
    public static EcmaValue UriError([NewTarget] RuntimeObject constructor, EcmaValue message) {
      return ErrorConstructor.CreateError(constructor, message, WellKnownObject.UriErrorPrototype, new EcmaUriErrorException(message.ToString()));
    }
  }
  #endregion
}
