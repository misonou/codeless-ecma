using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class GeneratorClosedException : Exception {
    public GeneratorClosedException(EcmaValue returnValue) {
      this.ReturnValue = returnValue;
    }

    public EcmaValue ReturnValue { get; }
  }

  public abstract class GeneratorBase : StatefulIterator, IGeneratorContext, IInspectorMetaProvider {
    private static readonly string[] stateString = { "suspended", "suspended", "running", "closed", "suspended" };
    private readonly IGeneratorEnumerator iterator;
    private readonly RuntimeFunctionInvocation invocation;

    internal GeneratorBase(RuntimeFunctionInvocation invocation, IGeneratorEnumerator iterator, WellKnownObject proto)
      : base(proto) {
      this.invocation = invocation;
      this.iterator = iterator;
      invocation.Generator = this;
      invocation.SuspendOnDispose = true;
      SetPrototypeOf(invocation.FunctionObject.Get(WellKnownProperty.Prototype).ToObject());
      iterator.Init(this);
    }

    public abstract GeneratorKind Kind { get; }

    public abstract EcmaValue ResumeValue { get; }

    public abstract GeneratorResumeState ResumeState { get; }

    protected abstract void ReturnFromGenerator(EcmaValue value);

    protected bool TryYieldNext(out EcmaValue result) {
      using (invocation.Resume()) { 
        if (iterator.MoveNext()) {
          result = iterator.Current;
          return true;
        }
        result = default;
        return false;
      }
    }

    protected virtual void Close() {
      invocation.SuspendOnDispose = false;
      invocation.Dispose();
      this.State = GeneratorState.Closed;
      try {
        iterator.Dispose();
      } catch { }
    }

    void IGeneratorContext.Return(EcmaValue value) {
      ReturnFromGenerator(value);
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[GeneratorFunction]]", invocation.FunctionObject);
      meta.EnumerableProperties.Add("[[GeneratorStatus]]", stateString[(int)this.State]);
    }
  }
}
