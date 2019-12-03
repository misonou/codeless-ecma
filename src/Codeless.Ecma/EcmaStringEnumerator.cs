using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public class EcmaStringEnumerator : IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
    private readonly string target;
    private int nextIndex = -1;

    public EcmaStringEnumerator(string target) {
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

    public void Reset() {
      nextIndex = -1;
    }

    object IEnumerator.Current => this.Current;

    void IDisposable.Dispose() { }
  }
}
