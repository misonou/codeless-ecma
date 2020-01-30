using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class AsyncFromSyncIterator : StatefulIterator {
    private readonly RuntimeObject target;
    private readonly EcmaIteratorEnumerator iterator;

    internal AsyncFromSyncIterator(EcmaValue value)
      : base(WellKnownObject.AsyncFromSyncIteratorPrototype) {
      this.target = value.ToObject();
      this.iterator = new EcmaIteratorEnumerator(value);
    }

    internal AsyncFromSyncIterator(EcmaIteratorEnumerator iterator)
      : base(WellKnownObject.AsyncFromSyncIteratorPrototype) {
      this.target = iterator.IteratedObject.ToObject();
      this.iterator = iterator;
    }

    public override EcmaValue Resume(GeneratorResumeState state, EcmaValue value) {
      if (this.State != GeneratorState.Closed) {
        this.State = GeneratorState.Running;
      }
      try {
        EcmaValue result = default;
        bool done = true;
        switch (state) {
          case GeneratorResumeState.Resume:
            done = !iterator.MoveNext(value);
            result = iterator.Current;
            break;
          case GeneratorResumeState.Return:
            if (!TryGetResult(WellKnownProperty.Return, value, out done, out result)) {
              this.State = GeneratorState.Closed;
              return Promise.Resolve(EcmaValueUtility.CreateIterResultObject(value, true));
            }
            break;
          case GeneratorResumeState.Throw:
            if (!TryGetResult(WellKnownProperty.Throw, value, out done, out result)) {
              this.State = GeneratorState.Closed;
              return Promise.Reject(value);
            }
            break;
        }
        if (done) {
          this.State = GeneratorState.Closed;
        }
        return Promise.Resolve(result, new ResultHandler(done).CreateIterResultObject, null);
      } catch (Exception ex) {
        return Promise.Reject(ex);
      }
    }

    private bool TryGetResult(EcmaPropertyKey propertyKey, EcmaValue value, out bool done, out EcmaValue result) {
      RuntimeObject method = target.GetMethod(propertyKey);
      if (method == null) {
        result = value;
        done = true;
        return false;
      }
      EcmaValue obj = method.Call(target, value);
      Guard.ArgumentIsObject(obj);
      done = obj[WellKnownProperty.Done].ToBoolean();
      result = obj[WellKnownProperty.Value];
      return true;
    }

    private class ResultHandler {
      private readonly bool done;

      public ResultHandler(bool done) {
        this.done = done;
      }

      [IntrinsicMember]
      public EcmaValue CreateIterResultObject(EcmaValue value) {
        return EcmaValueUtility.CreateIterResultObject(value, done);
      }
    }
  }
}
