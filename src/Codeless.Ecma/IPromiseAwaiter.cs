#if ASYNC
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public interface IPromiseAwaiter : INotifyCompletion {
    bool IsCompleted { get; }
    EcmaValue GetResult();
  }

  #region Implementation
  public partial struct EcmaValue {
    public IPromiseAwaiter GetAwaiter() {
      return PromiseConstructor.Resolve(Global.Promise, this).GetUnderlyingObject<Promise>();
    }
  }

  public partial class Promise : IPromiseAwaiter {
    bool IPromiseAwaiter.IsCompleted {
      get { return this.State != PromiseState.Pending; }
    }

    EcmaValue IPromiseAwaiter.GetResult() {
      if (this.State == PromiseState.Rejected) {
        Keywords.Throw(this.Value);
      }
      return this.Value;
    }

    void INotifyCompletion.OnCompleted(Action continuation) {
      RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
      invocation.SuspendOnDispose = true;
      ContinueWith(_ => {
        invocation.SuspendOnDispose = false;
        using (invocation.Resume()) {
          continuation();
        }
      });
    }
  }
  #endregion
}
#endif
