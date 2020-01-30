using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public enum PromiseState {
    Pending,
    Fulfilled,
    Rejected
  }

  public delegate EcmaValue PromiseCallback(EcmaValue value);

  public delegate void PromiseResolver(EcmaValue value);

  public delegate void PromiseExecutor(PromiseResolver resolve, PromiseResolver reject);

  public partial class Promise : RuntimeObject, IInspectorMetaProvider {
    private static int accum;
    private PromiseCallback fulfilledCallback;
    private PromiseCallback rejectedCallback;
    private Action<Promise> hooks;

    public Promise()
      : base(WellKnownObject.PromisePrototype) { }

    public Promise(PromiseState state, EcmaValue value)
      : this() {
      SetStateFinalize(state, value);
    }

    public Promise(PromiseExecutor callback)
      : this() {
      Guard.ArgumentNotNull(callback, "callback");
      ExecuteCallback(callback);
    }

    public int ID { get; } = Interlocked.Increment(ref accum);

    public PromiseState State { get; private set; }

    public EcmaValue Value { get; private set; }

    public static Promise All(IEnumerable<EcmaValue> values) {
      return All(values.Select(Resolve));
    }

    public static Promise All(IEnumerable<Promise> promises) {
      Guard.ArgumentNotNull(promises, "promises");
      List<Promise> list = new List<Promise>(promises);
      if (list.Count == 0) {
        return new Promise(PromiseState.Fulfilled, new EcmaArray());
      }
      Promise promise = new Promise();
      int count = 0;
      foreach (Promise p in list) {
        p.ContinueWith(other => {
          if (other.State == PromiseState.Rejected) {
            promise.RejectSelf(other.Value);
          } else if (++count == list.Count) {
            promise.ResolveSelf(new EcmaArray(list.Select(v => v.Value).ToList()));
          }
        });
      }
      return promise;
    }

    public static Promise Race(IEnumerable<EcmaValue> values) {
      return Race(values.Select(Resolve));
    }

    public static Promise Race(IEnumerable<Promise> promises) {
      Guard.ArgumentNotNull(promises, "promises");
      List<Promise> list = new List<Promise>(promises);
      if (list.Count == 0) {
        return new Promise(PromiseState.Fulfilled, new EcmaArray());
      }
      Promise promise = new Promise();
      foreach (Promise p in list) {
        p.ContinueWith(promise.HandleCompletedPromise);
      }
      return promise;
    }

    public static Promise Resolve(EcmaValue value) {
      if (value.GetUnderlyingObject() is Promise p && p[WellKnownProperty.Constructor].ToObject().IsWellknownObject(WellKnownObject.PromiseConstructor)) {
        return p;
      }
      Promise promise = new Promise();
      promise.ResolveSelf(value);
      return promise;
    }

    public static Promise Resolve(EcmaValue value, PromiseCallback fulfilledCallback, PromiseCallback rejectedCallback) {
      Promise promise = new Promise();
      promise.InitWithCallback(Promise.Resolve(value), fulfilledCallback, rejectedCallback);
      return promise;
    }

    public static Promise Reject(Exception ex) {
      return Reject(EcmaValueUtility.GetValueFromException(ex));
    }

    public static Promise Reject(EcmaValue value) {
      Promise promise = new Promise();
      promise.RejectSelf(value);
      return promise;
    }

    public static Promise FromTask(Task task) {
      Guard.ArgumentNotNull(task, "task");
      Promise promise = new Promise();
      RuntimeExecution.Enqueue(() => promise.HandleCompletedTask(task), task);
      return promise;
    }

    public static Promise FromTask<T>(Task<T> task) {
      Guard.ArgumentNotNull(task, "task");
      Promise promise = new Promise();
      RuntimeExecution.Enqueue(() => promise.HandleCompletedTask(task), task);
      return promise;
    }

    [EcmaSpecification("IsPromise", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsPromise(EcmaValue value) {
      return value.GetUnderlyingObject() is Promise;
    }

    internal void InitWithExecutor(RuntimeObject callback) {
      Guard.ArgumentNotNull(callback, "callback");
      ExecuteCallback((a, b) => callback.Call(EcmaValue.Undefined, a, b));
    }

    internal void InitWithCallback(Promise previous, PromiseCallback fulfilledCallback, PromiseCallback rejectedCallback) {
      Guard.ArgumentNotNull(previous, "previous");
      this.fulfilledCallback = fulfilledCallback;
      this.rejectedCallback = rejectedCallback;
      previous.ContinueWith(HandleCompletedPromise);
    }

    internal void ContinueWith(Action<Promise> callback) {
      if (this.State == PromiseState.Pending) {
        hooks += callback;
      } else {
        callback(this);
      }
    }

    [IntrinsicMember(null)]
    private void ResolveSelf(EcmaValue value) {
      ResolveSelf(value, false);
    }

    [IntrinsicMember(null)]
    [EcmaSpecification("Promise Reject Functions", EcmaSpecificationKind.AbstractOperations)]
    private void RejectSelf(EcmaValue value) {
      SetState(PromiseState.Rejected, value, false);
    }

    [EcmaSpecification("Promise Resolve Functions", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("PromiseResolveThenableJob", EcmaSpecificationKind.AbstractOperations)]
    private void ResolveSelf(EcmaValue value, bool finalize) {
      if (value.Type == EcmaValueType.Object) {
        RuntimeObject obj = value.ToObject();
        if (obj == this) {
          SetStateFinalize(PromiseState.Rejected, new EcmaTypeErrorException(InternalString.Error.PromiseSelfResolution).ToValue());
          return;
        }
        try {
          RuntimeObject then = obj.GetMethod(WellKnownProperty.Then);
          if (then != null) {
            then.Call(value, (PromiseResolver)ResolveSelf, (PromiseResolver)RejectSelf);
            return;
          }
        } catch (Exception ex) {
          SetStateFinalize(PromiseState.Rejected, EcmaValueUtility.GetValueFromException(ex));
          return;
        }
      }
      SetState(PromiseState.Fulfilled, value, finalize);
    }

    [EcmaSpecification("PromiseReactionJob", EcmaSpecificationKind.AbstractOperations)]
    private void SetState(PromiseState state, EcmaValue value, bool finalize) {
      PromiseCallback callback = state == PromiseState.Fulfilled ? fulfilledCallback : rejectedCallback;
      fulfilledCallback = null;
      rejectedCallback = null;
      if (finalize || callback == null) {
        SetStateFinalize(state, value);
      } else {
        RuntimeExecution.Enqueue(() => {
          try {
            value = callback(value);
            ResolveSelf(value, true);
          } catch (Exception ex) {
            SetStateFinalize(PromiseState.Rejected, EcmaValueUtility.GetValueFromException(ex));
          }
        });
      }
    }

    [EcmaSpecification("FulfillPromise", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("RejectPromise", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("TriggerPromiseReactions", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("HostPromiseRejectionTracker", EcmaSpecificationKind.AbstractOperations)]
    private void SetStateFinalize(PromiseState state, EcmaValue value) {
      if (this.State == PromiseState.Pending) {
        this.State = state;
        this.Value = value;
        hooks?.Invoke(this);
        if (hooks == null && state == PromiseState.Rejected) {
          RuntimeExecution.SendUnhandledException(value);
        }
        hooks = null;
      }
    }

    [EcmaSpecification("IfAbruptRejectPromise", EcmaSpecificationKind.AbstractOperations)]
    private void ExecuteCallback(PromiseExecutor executor) {
      try {
        executor(ResolveSelf, RejectSelf);
      } catch (Exception ex) {
        RejectSelf(EcmaValueUtility.GetValueFromException(ex));
      }
    }

    private void HandleCompletedPromise(Promise previous) {
      if (previous.State == PromiseState.Pending) {
        throw new InvalidOperationException();
      }
      SetState(previous.State, previous.Value, false);
    }

    private void HandleCompletedTask(Task task) {
      if (task.IsFaulted) {
        RejectFromFaultedTask(task);
      } else {
        EcmaValue value = default;
        for (Type type = task.GetType(); type != typeof(Task); type = type.BaseType) {
          if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
            value = new EcmaValue(type.GetProperty("Result").GetValue(task, null));
            break;
          }
        }
        SetState(PromiseState.Fulfilled, value, false);
      }
    }

    private void HandleCompletedTask<T>(Task<T> task) {
      if (task.IsFaulted) {
        RejectFromFaultedTask(task);
      } else {
        SetState(PromiseState.Fulfilled, new EcmaValue(task.Result), false);
      }
    }

    private void RejectFromFaultedTask(Task task) {
      AggregateException ex = task.Exception.Flatten();
      Exception reason = ex;
      if (ex.InnerExceptions.Count == 1) {
        reason = ex.InnerExceptions[0];
      }
      SetState(PromiseState.Rejected, EcmaValueUtility.GetValueFromException(reason), false);
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[PromiseState]]", this.State.ToString().ToLower());
      meta.EnumerableProperties.Add("[[PromiseResult]]", this.Value);
    }
  }
}
