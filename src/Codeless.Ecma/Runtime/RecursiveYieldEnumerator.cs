using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  internal class RecursiveYieldEnumerator : IGeneratorEnumerator {
    public RecursiveYieldEnumerator(IGeneratorEnumerable iterator) {
      Guard.ArgumentNotNull(iterator, "iterator");
      this.Iterator = iterator;
    }

    public IGeneratorEnumerable Iterator { get; }
    public IGeneratorContext Context { get; private set; }
    public EcmaValue Current { get; private set; }

    public void Init(IGeneratorContext context) {
      Guard.ArgumentNotNull(context, "context");
      this.Context = context;
    }

    public bool MoveNext() {
      GeneratorState currentState = this.Iterator.State;
      switch (currentState) {
        case GeneratorState.Closed:
        case GeneratorState.AwaitingReturn:
          this.Current = EcmaValue.Undefined;
          return false;
      }
      GeneratorResumeState resumeState = this.Context.ResumeState;
      try {
        this.Current = GetNextValue(resumeState, this.Context.ResumeValue);
      } catch (GeneratorClosedException ex) {
        this.Current = ex.ReturnValue;
        if (resumeState != GeneratorResumeState.Return) {
          return false;
        }
        throw;
      }
      return resumeState != GeneratorResumeState.Resume || this.Iterator.State != GeneratorState.Closed;
    }

    protected virtual EcmaValue GetNextValue(GeneratorResumeState state, EcmaValue value) {
      return UnwrapResult(this.Iterator.Resume(state, value));
    }

    [IntrinsicMember]
    protected static EcmaValue UnwrapResult(EcmaValue result) {
      Guard.ArgumentIsObject(result);
      bool done = result[WellKnownProperty.Done].ToBoolean();
      EcmaValue returnValue = result[WellKnownProperty.Value];
      if (done) {
        throw new GeneratorClosedException(returnValue);
      }
      return returnValue;
    }

    #region Interface
    object IEnumerator.Current {
      get { return this.Current; }
    }

    void IGeneratorEnumerator.PushStack(IGeneratorEnumerator other) { }

    bool IGeneratorEnumerator.SetException(Exception ex) {
      return false;
    }

    bool IGeneratorEnumerator.SetFinally() {
      return false;
    }

    void IEnumerator.Reset() { }

    void IDisposable.Dispose() { }

    IEnumerator<EcmaValue> IEnumerable<EcmaValue>.GetEnumerator() {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this;
    }
    #endregion
  }
}
