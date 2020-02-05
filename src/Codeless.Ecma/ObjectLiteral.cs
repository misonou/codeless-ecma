using Codeless.Ecma.InteropServices;
using System;

namespace Codeless.Ecma {
  public sealed class ObjectLiteral : ObjectLiteralBuilder {
    public EcmaValue ToValue() {
      return CreateObject();
    }

    public static implicit operator EcmaValue(ObjectLiteral source) {
      return source.CreateObject();
    }
  }
}
