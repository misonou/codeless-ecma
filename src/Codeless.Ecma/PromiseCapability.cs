using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal class PromiseCapability {
    private PromiseCapability() { }

    public EcmaValue Promise { get; private set; }
    public EcmaValue ResolveCallback { get; private set; }
    public EcmaValue RejectCallback { get; private set; }
    public PromiseCallback OnFulfill { get; set; }
    public PromiseCallback OnReject { get; set; }
    public bool Handled { get; private set; }

    [EcmaSpecification("NewPromiseCapability", EcmaSpecificationKind.AbstractOperations)]
    public static PromiseCapability CreateFromConstructor(RuntimeObject constructor) {
      Guard.ArgumentNotNull(constructor, "constructor");
      PromiseCapability capability = new PromiseCapability();
      capability.Promise = constructor.Construct((Action<EcmaValue, EcmaValue>)capability.Executor);
      capability.EnsureValidResolver();
      return capability;
    }

    public static PromiseCapability Create() {
      PromiseCapability capability = new PromiseCapability();
      capability.Promise = new Promise((resolve, reject) => {
        capability.ResolveCallback = resolve;
        capability.RejectCallback = reject;
      });
      return capability;
    }

    public void Resolve(EcmaValue value) {
      HandleResult(PromiseState.Fulfilled, value);
    }

    public void Reject(EcmaValue value) {
      HandleResult(PromiseState.Rejected, value);
    }

    [EcmaSpecification("PerformPromiseThen", EcmaSpecificationKind.RuntimeSemantics)]
    public void HandlePromise(Promise promise, PromiseCallback onfulfill, PromiseCallback onreject) {
      Guard.ArgumentNotNull(promise, "promise");
      if (this.Promise.GetUnderlyingObject() is Promise p) {
        p.InitWithCallback(promise, onfulfill, onreject);
      } else {
        this.OnFulfill = onfulfill;
        this.OnReject = onreject;
        promise.ContinueWith(HandleCompletedPromise);
      }
    }

    public void HandleResult(PromiseState state, EcmaValue value) {
      if (!this.Handled) {
        PromiseCallback callback = state == PromiseState.Rejected ? OnReject : OnFulfill;
        EcmaValue resolver = state == PromiseState.Rejected ? RejectCallback : ResolveCallback;
        this.Handled = true;
        if (callback != null) {
          value = callback(value);
        }
        resolver.Call(EcmaValue.Undefined, value);
      }
    }

    [IntrinsicMember(null)]
    [EcmaSpecification("GetCapabilitiesExecutor Functions", EcmaSpecificationKind.AbstractOperations)]
    private void Executor(EcmaValue resolve, EcmaValue reject) {
      if (this.ResolveCallback != default || this.RejectCallback != default) {
        throw new EcmaTypeErrorException(InternalString.Error.ExecutorInitialized);
      }
      this.ResolveCallback = resolve;
      this.RejectCallback = reject;
    }

    private void EnsureValidResolver() {
      if (!ResolveCallback.IsCallable || !RejectCallback.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.ResolverNotCallable);
      }
    }

    private void HandleCompletedPromise(Promise previous) {
      try {
        HandleResult(previous.State, previous.Value);
      } catch (Exception ex) {
        RejectCallback.Call(EcmaValue.Undefined, EcmaValueUtility.GetValueFromException(ex));
      }
    }
  }
}
