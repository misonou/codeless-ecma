using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class AsyncGenerator : GeneratorBase {
    private readonly Queue<ResumeRecord> queue = new Queue<ResumeRecord>();
    private EcmaValue returnValue;

    internal AsyncGenerator(RuntimeFunctionInvocation invocation, IGeneratorEnumerator iterator)
      : base(invocation, iterator, WellKnownObject.AsyncGeneratorPrototype) { }

    public override GeneratorKind Kind => GeneratorKind.Async;

    public override EcmaValue ResumeValue {
      get { return queue.Peek().Value; }
    }

    public override GeneratorResumeState ResumeState {
      get {
        if (this.State == GeneratorState.AwaitingReturn) {
          return GeneratorResumeState.Return;
        }
        return queue.Peek().State;
      }
    }

    public override EcmaValue Resume(GeneratorResumeState state, EcmaValue value) {
      PromiseCapability capability = PromiseCapability.Create();
      ResumeRecord record = new ResumeRecord(state, value, capability);
      queue.Enqueue(record);
      if (this.State != GeneratorState.Running) {
        ResumeNext();
      }
      return capability.Promise;
    }

    protected override void ReturnFromGenerator(EcmaValue value) {
      AwaitReturn(value, false);
    }

    protected override void Close() {
      base.Close();
      if (queue.Count > 0) {
        Resolve(returnValue, true);
        returnValue = EcmaValue.Undefined;
      }
      while (queue.Count > 0) {
        Resolve(EcmaValue.Undefined, true);
      }
    }

    [EcmaSpecification("AsyncGeneratorResolve", EcmaSpecificationKind.AbstractOperations)]
    private void Resolve(EcmaValue value, bool done) {
      queue.Dequeue().Capability.Resolve(EcmaValueUtility.CreateIterResultObject(value, done));
    }

    [EcmaSpecification("AsyncGeneratorReject", EcmaSpecificationKind.AbstractOperations)]
    private void Reject(EcmaValue ex) {
      queue.Dequeue().Capability.Reject(ex);
    }

    [EcmaSpecification("AsyncGeneratorResumeNext", EcmaSpecificationKind.AbstractOperations)]
    private void ResumeNext() {
      if (this.State == GeneratorState.AwaitingReturn || queue.Count == 0) {
        return;
      }
      ResumeRecord record = queue.Peek();
      if (this.State == GeneratorState.Closed) {
        switch (record.State) {
          case GeneratorResumeState.Resume:
            Close();
            return;
          case GeneratorResumeState.Return:
            AwaitReturn(record.Value, false);
            return;
          case GeneratorResumeState.Throw:
            Reject(record.Value);
            return;
        }
        return;
      }
      if (this.State == GeneratorState.SuspendedStart && record.State != GeneratorResumeState.Resume) {
        if (record.State == GeneratorResumeState.Throw) {
          Reject(record.Value);
          Close();
        } else {
          AwaitReturn(record.Value, false);
        }
        return;
      }
      if (record.State == GeneratorResumeState.Return) {
        AwaitReturn(record.Value, true);
      } else {
        YieldNext();
      }
    }

    private void YieldNext() {
      if (this.State != GeneratorState.Closed) {
        try {
          this.State = GeneratorState.Running;
          if (TryYieldNext(out EcmaValue result)) {
            Promise.Resolve(result, ResolveFromYield, RejectFromYield);
            return;
          }
        } catch (GeneratorClosedException ex) {
          Resolve(ex.ReturnValue, true);
        } catch (Exception ex) {
          Reject(EcmaValueUtility.GetValueFromException(ex));
        }
        if (this.State != GeneratorState.AwaitingReturn) {
          Close();
        }
      }
    }

    private void AwaitReturn(EcmaValue value, bool resume) {
      this.State = GeneratorState.AwaitingReturn;
      Promise.Resolve(value, resume ? (PromiseCallback)ResumeFromReturn : ResolveFromReturn, RejectFromYield);
    }

    [IntrinsicMember]
    private EcmaValue ResumeFromReturn(EcmaValue value) {
      returnValue = value;
      YieldNext();
      return default;
    }

    [IntrinsicMember]
    private EcmaValue ResolveFromReturn(EcmaValue value) {
      Resolve(value, true);
      Close();
      return default;
    }

    [IntrinsicMember]
    private EcmaValue ResolveFromYield(EcmaValue value) {
      this.State = GeneratorState.SuspendedYield;
      Resolve(value, false);
      ResumeNext();
      return default;
    }

    [IntrinsicMember]
    private EcmaValue RejectFromYield(EcmaValue value) {
      if (value.GetUnderlyingObject() is EcmaError error && error.Exception is GeneratorClosedException closedException) {
        returnValue = closedException.ReturnValue;
      } else {
        Reject(value);
      }
      Close();
      return default;
    }

    private class ResumeRecord {
      public ResumeRecord(GeneratorResumeState state, EcmaValue value, PromiseCapability capability) {
        this.State = state;
        this.Value = value;
        this.Capability = capability;
      }

      public GeneratorResumeState State { get; }
      public EcmaValue Value { get; }
      public PromiseCapability Capability { get; }
    }
  }
}
