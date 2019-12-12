using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal abstract class PromiseAggregator : Collection<PromiseAggregateHandler> {
    private readonly PromiseCapability capability;

    public PromiseAggregator(PromiseCapability capability) {
      Guard.ArgumentNotNull(capability, "capability");
      this.capability = capability;
    }

    public bool ShouldResolveImmediately { get; private set; }

    public PromiseAggregateHandler CreateHandler() {
      return new PromiseAggregateHandler(this);
    }

    public void ResolveIfConditionMet() {
      if (!capability.Handled) {
        PromiseState state = GetState(out EcmaValue value);
        if (state != PromiseState.Pending) {
          capability.HandleResult(state, value);
        } else {
          this.ShouldResolveImmediately = true;
        }
      }
    }

    protected virtual PromiseState GetState(out EcmaValue value) {
      value = default;
      return PromiseState.Pending;
    }
  }
}
