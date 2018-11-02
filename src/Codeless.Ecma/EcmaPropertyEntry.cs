using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public struct EcmaPropertyEntry : IEcmaValueConvertible {
    public EcmaPropertyEntry(EcmaPropertyKey key, EcmaValue value) {
      Key = key;
      Value = value;
    }

    public EcmaPropertyKey Key { get; private set; }
    public EcmaValue Value { get; private set; }

    public EcmaValue ToValue() {
      return EcmaArray.Of(this.Key.ToValue(), this.Value);
    }
  }
}
