using Codeless.Ecma.InteropServices;
using System;

namespace Codeless.Ecma {
  public sealed class ArrayLiteral : ArrayLiteralBuilder {
    public EcmaValue ToValue() {
      return CreateObject();
    }

    public static implicit operator EcmaValue(ArrayLiteral source) {
      return source.CreateObject();
    }
  }
}
