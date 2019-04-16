using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaIteratorEnumerator : IEnumerable<EcmaValue>, IEnumerator<EcmaValue> {
    private readonly EcmaValue iterator;
    private readonly EcmaValue nextMethod;
    private bool done;

    public EcmaIteratorEnumerator(EcmaValue iterator) {
      this.iterator = iterator;
      this.nextMethod = iterator["next"];
    }

    public EcmaValue Current { get; private set; }

    [EcmaSpecification("IteratorStep", EcmaSpecificationKind.AbstractOperations)]
    public bool MoveNext() {
      if (this.done) {
        return false;
      }
      EcmaValue result = this.nextMethod.Call(this.iterator);
      Guard.ArgumentIsObject(result);
      if (result["done"]) {
        this.done = true;
        return false;
      }
      this.Current = result["value"];
      return true;
    }

    public void Reset() {
      throw new InvalidOperationException("Iterator cannot be reset");
    }

    public IEnumerator<EcmaValue> GetEnumerator() {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this;
    }

    object IEnumerator.Current {
      get { return this.Current; }
    }

    [EcmaSpecification("IteratorClose", EcmaSpecificationKind.AbstractOperations)]
    void IDisposable.Dispose() {
      if (!this.done) {
        EcmaValue returns = iterator["return"];
        if (returns.IsCallable) {
          EcmaValue result = returns.Call(iterator);
          Guard.ArgumentIsObject(result);
        }
      }
    }
  }
}
