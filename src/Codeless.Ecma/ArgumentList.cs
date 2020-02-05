using Codeless.Ecma.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class ArgumentList : IEnumerable {
    private readonly List<EcmaValue> values = new List<EcmaValue>();

    public void Add(EcmaValue value) {
      values.Add(value);
    }

    public void Add(ISpreadLiteral spreadable) {
      Guard.ArgumentNotNull(spreadable, "spreadable");
      foreach (EcmaValue v in spreadable.Value.ForOf()) {
        values.Add(v);
      }
    }

    public EcmaValue[] ToArray() {
      return values.Count == 0 ? EcmaValue.EmptyArray : values.ToArray();
    }

    public static implicit operator EcmaValue[] (ArgumentList builder) {
      return builder == null ? EcmaValue.EmptyArray : builder.ToArray();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return values.GetEnumerator();
    }
  }
}
