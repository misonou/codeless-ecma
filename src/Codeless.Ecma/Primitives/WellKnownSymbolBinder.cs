using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class WellKnownSymbolBinder : SymbolBinder {
    public new static readonly WellKnownSymbolBinder Default = new WellKnownSymbolBinder();

    protected WellKnownSymbolBinder() { }

    public override Symbol FromHandle(EcmaValueHandle handle) {
      return Symbol.GetSymbol((WellKnownSymbol)handle.Value);
    }

    public override EcmaValueHandle ToHandle(Symbol value) {
      if (value.SymbolType == 0) {
        throw new InvalidOperationException();
      }
      return new EcmaValueHandle((long)value.SymbolType);
    }
  }
}
