using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class Generator : GeneratorBase {
    private GeneratorResumeState nextState;
    private EcmaValue resumeValue;

    internal Generator(RuntimeFunctionInvocation invocation, IGeneratorEnumerator iterator)
      : base(invocation, iterator, WellKnownObject.GeneratorPrototype) { }

    public override GeneratorKind Kind => GeneratorKind.Sync;

    public override EcmaValue ResumeValue => resumeValue;

    public override GeneratorResumeState ResumeState => nextState;

    public override EcmaValue Resume(GeneratorResumeState state, EcmaValue value) {
      if (this.State == GeneratorState.Running) {
        throw new EcmaTypeErrorException(InternalString.Error.GeneratorRunning);
      }
      nextState = state;
      if (this.State != GeneratorState.SuspendedYield) {
        switch (state) {
          case GeneratorResumeState.Throw:
            Close();
            throw EcmaException.FromValue(value);
          case GeneratorResumeState.Return:
            Close();
            return EcmaValueUtility.CreateIterResultObject(value, true);
        }
      } else {
        resumeValue = value;
      }
      if (this.State != GeneratorState.Closed) {
        try {
          this.State = GeneratorState.Running;
          if (TryYieldNext(out EcmaValue result)) {
            return EcmaValueUtility.CreateIterResultObject(result, false);
          }
        } catch {
          if (nextState != GeneratorResumeState.Return) {
            Close();
            throw;
          }
        } finally {
          if (this.State != GeneratorState.Closed) {
            this.State = GeneratorState.SuspendedYield;
          }
        }
        Close();
      }
      return EcmaValueUtility.CreateIterResultObject(nextState == GeneratorResumeState.Return ? resumeValue : default, true);
    }

    protected override void ReturnFromGenerator(EcmaValue value) {
      resumeValue = value;
      nextState = GeneratorResumeState.Return;
    }
  }
}
