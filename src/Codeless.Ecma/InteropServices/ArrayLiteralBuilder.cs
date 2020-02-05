using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma.InteropServices {
  public interface IElisionLiteral { }

  public abstract class ArrayLiteralBuilder : ObjectLiteralBuilderBase, IEnumerable {
    public static readonly IElisionLiteral Empty = new ElisionLiteral();

    private readonly Dictionary<int, object> emptyOrSpread = new Dictionary<int, object>();
    private readonly List<EcmaValue> values = new List<EcmaValue>();

    public void Add(EcmaValue value) {
      values.Add(value);
    }

    public void Add(IElisionLiteral elision) {
      emptyOrSpread[values.Count] = elision;
      values.Add(EcmaValue.Undefined);
    }

    public void Add(ISpreadLiteral spreadable) {
      Guard.ArgumentNotNull(spreadable, "spreadable");
      emptyOrSpread[values.Count] = spreadable;
      values.Add(spreadable.Value);
    }

    protected override RuntimeObject CreateObject() {
      if (emptyOrSpread.Count == 0) {
        return new EcmaArray(new List<EcmaValue>(values));
      }
      EcmaArray target = new EcmaArray();
      for (int i = 0, j = 0, len = values.Count; i < len; i++) {
        if (!emptyOrSpread.TryGetValue(i, out object modifier)) {
          target[j++] = values[i];
          continue;
        }
        if (modifier is ISpreadLiteral) {
          foreach (EcmaValue w in values[i].ForOf()) {
            target[j++] = w;
          }
        } else {
          j++;
        }
      }
      return target;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      throw new InvalidOperationException("This class is not intended to be iterated");
    }

    private class ElisionLiteral : IElisionLiteral { }
  }
}
