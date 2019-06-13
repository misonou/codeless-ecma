using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaArrayEnumerator : IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
    private readonly RuntimeObject target;
    private long nextIndex = -1;

    public EcmaArrayEnumerator(RuntimeObject target) {
      this.target = target;
    }

    public KeyValuePair<EcmaValue, EcmaValue> Current { get; private set; }

    public bool MoveNext() {
      if (++nextIndex < target.Get(WellKnownProperty.Length).ToLength()) {
        this.Current = new KeyValuePair<EcmaValue, EcmaValue>(nextIndex, target.Get(nextIndex));
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
