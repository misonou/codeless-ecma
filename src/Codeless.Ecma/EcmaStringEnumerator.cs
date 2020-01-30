using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public class EcmaStringEnumerator : EcmaIterator, IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
    private readonly string target;
    private int nextIndex = -1;

    public EcmaStringEnumerator(string target)
      : base(target, EcmaIteratorResultKind.Value, WellKnownObject.StringIteratorPrototype) {
      this.target = target;
    }

    public KeyValuePair<EcmaValue, EcmaValue> Current { get; private set; }

    public bool MoveNext() {
      if (++nextIndex < target.Length) {
        if (Char.IsSurrogatePair(target, nextIndex)) {
          this.Current = new KeyValuePair<EcmaValue, EcmaValue>(nextIndex, target.Substring(nextIndex, 2));
          nextIndex++;
        } else {
          this.Current = new KeyValuePair<EcmaValue, EcmaValue>(nextIndex, target.Substring(nextIndex, 1));
        }
        return true;
      }
      return false;
    }

    protected override IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject) {
      return this;
    }

    #region Interface
    object IEnumerator.Current => this.Current;

    void IEnumerator.Reset() {
      throw new InvalidOperationException();
    }

    void IDisposable.Dispose() { }
    #endregion
  }
}
