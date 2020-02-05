using Codeless.Ecma.InteropServices;
using System;

namespace Codeless.Ecma {
  public sealed class ClassLiteral : ClassLiteralBuilder {
    public ClassLiteral()
      : base(null, null) { }

    public ClassLiteral(IExtendsModifier super)
      : base(null, super) { }

    public ClassLiteral(string name)
      : base(name, null) { }

    public ClassLiteral(string name, IExtendsModifier super)
      : base(name, super) { }

    public EcmaValue ToValue() {
      return CreateObject();
    }

    public static implicit operator EcmaValue(ClassLiteral source) {
      return source.CreateObject();
    }
  }
}
