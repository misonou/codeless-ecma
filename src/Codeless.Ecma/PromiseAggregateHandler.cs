using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  internal class PromiseAggregateHandler {
    private readonly PromiseAggregator collection;

    public PromiseAggregateHandler(PromiseAggregator collection) {
      Guard.ArgumentNotNull(collection, "collection");
      this.collection = collection;
      collection.Add(this);
    }

    public PromiseState State { get; private set; }
    public EcmaValue Value { get; private set; }

    [IntrinsicMember(null)]
    public void ResolveHandler(EcmaValue value) {
      HandleResult(PromiseState.Fulfilled, value);
    }

    [IntrinsicMember(null)]
    public void RejectHandler(EcmaValue value) {
      HandleResult(PromiseState.Rejected, value);
    }

    public void HandleResult(PromiseState state, EcmaValue value) {
      if (this.State == PromiseState.Pending) {
        this.State = state;
        this.Value = value;
        if (collection.ShouldResolveImmediately) {
          collection.ResolveIfConditionMet();
        }
      }
    }
  }
}
