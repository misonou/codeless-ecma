using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaIteratorEnumerator : IEnumerable<EcmaValue>, IEnumerator<EcmaValue> {
    private readonly EcmaValue iterator;
    private readonly EcmaValue nextMethod;
    private bool done;
    private bool disposed;

    public EcmaIteratorEnumerator(EcmaValue iterator) {
      this.iterator = iterator;
      this.nextMethod = iterator[WellKnownProperty.Next];
    }

    public EcmaValue Current { get; private set; }

    [EcmaSpecification("IteratorStep", EcmaSpecificationKind.AbstractOperations)]
    public bool MoveNext() {
      if (this.done) {
        return false;
      }
      EcmaValue result = this.nextMethod.Call(this.iterator);
      Guard.ArgumentIsObject(result);
      if (result[WellKnownProperty.Done]) {
        this.done = true;
        return false;
      }
      this.Current = result[WellKnownProperty.Value];
      return true;
    }

    public void Reset() {
      throw new InvalidOperationException("Iterator cannot be reset");
    }

    public void Done() {
      this.done = true;
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
      if (!disposed) {
        disposed = true;
        if (!this.done) {
          EcmaValue returns = iterator[WellKnownProperty.Return];
          if (returns.IsCallable) {
            returns.Call(iterator);
          }
        }
      }
    }
  }
}
