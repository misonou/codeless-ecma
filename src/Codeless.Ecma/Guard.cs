using System;
using System.Diagnostics;

namespace Codeless.Ecma {
  [DebuggerStepThrough]
  internal static class Guard {
    public static T ArgumentNotNull<T>(T value, string argumentName) {
      if (Object.ReferenceEquals(value, null)) {
        throw new ArgumentNullException(argumentName);
      }
      return value;
    }

    [EcmaSpecification("RequireObjectCoercible", EcmaSpecificationKind.AbstractOperations)]
    public static void RequireObjectCoercible(EcmaValue value, string errorMessage = null) {
      if (value.IsNullOrUndefined) {
        throw new EcmaTypeErrorException(errorMessage ?? InternalString.Error.NotCoercibleAsObject);
      }
    }

    public static void ArgumentIsObject(EcmaValue value, string errorMessage = null) {
      if (value.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(errorMessage ?? InternalString.Error.NotObject);
      }
    }

    public static void ArgumentIsCallable(EcmaValue value, string errorMessage = null) {
      if (!value.IsCallable) {
        throw new EcmaTypeErrorException(errorMessage ?? InternalString.Error.NotFunction);
      }
    }

    public static void BufferNotDetached(ArrayBuffer buffer) {
      Guard.ArgumentNotNull(buffer, "buffer");
      if (buffer.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
    }

    public static void BufferNotDetached(IArrayBufferView buffer) {
      Guard.ArgumentNotNull(buffer, "buffer");
      if (buffer.Buffer.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
    }
  }
}
