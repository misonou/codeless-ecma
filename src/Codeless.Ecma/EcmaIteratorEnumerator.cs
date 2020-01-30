using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaIteratorEnumerator : IGeneratorEnumerable, IEnumerable<EcmaValue>, IEnumerator<EcmaValue> {
    private readonly EcmaValue iterator;
    private readonly EcmaValue nextMethod;
    private bool done;
    private bool disposed;

    public EcmaIteratorEnumerator(EcmaValue iterator) {
      this.iterator = iterator;
      this.nextMethod = iterator[WellKnownProperty.Next];
    }

    public EcmaValue IteratedObject {
      get { return iterator; }
    }

    public EcmaValue Current { get; private set; }

    [EcmaSpecification("IteratorStep", EcmaSpecificationKind.AbstractOperations)]
    public bool MoveNext() {
      return MoveNext(EcmaValue.EmptyArray);
    }

    public bool MoveNext(params EcmaValue[] value) {
      if (this.done) {
        return false;
      }
      EcmaValue result = this.nextMethod.Call(this.iterator, value);
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

    [EcmaSpecification("IteratorClose", EcmaSpecificationKind.AbstractOperations)]
    public void Dispose() {
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

    #region Interface
    object IEnumerator.Current {
      get { return this.Current; }
    }

    GeneratorState IGeneratorEnumerable.State {
      get { return done ? GeneratorState.Closed : GeneratorState.Running; }
    }

    EcmaValue IGeneratorEnumerable.Resume(GeneratorResumeState state, EcmaValue value) {
      switch (state) {
        case GeneratorResumeState.Return:
          throw new GeneratorClosedException(value);
        case GeneratorResumeState.Throw:
          throw EcmaException.FromValue(value);
      }
      if (MoveNext()) {
        return EcmaValueUtility.CreateIterResultObject(this.Current, false);
      }
      return EcmaValueUtility.CreateIterResultObject(EcmaValue.Undefined, true);
    }

    IEnumerator<EcmaValue> IEnumerable<EcmaValue>.GetEnumerator() {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this;
    }
    #endregion
  }
}
