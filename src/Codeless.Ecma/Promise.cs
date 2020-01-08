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

  public class Promise : RuntimeObject, IInspectorMetaProvider {
    private static int accum;
    private PromiseCallback fulfilledCallback;
    private PromiseCallback rejectedCallback;
    private Action<Promise> hooks;

    public Promise()
      : base(WellKnownObject.PromisePrototype) { }

    public Promise(Task task)
      : this() {
      Guard.ArgumentNotNull(task, "task");
      RuntimeExecution.Enqueue(() => HandleCompletedTask(task), task);
    }

    public Promise(Exception ex)
      : this(PromiseState.Rejected, EcmaValueUtility.GetValueFromException(ex)) { }

    public Promise(PromiseState state, EcmaValue value)
      : this() {
      SetStateFinalize(state, value);
    }

    public Promise(PromiseExecutor callback)
      : this() {
      Guard.ArgumentNotNull(callback, "callback");
      ExecuteCallback(callback);
    }

    public Promise(Promise previous, PromiseCallback fulfilledCallback = null, PromiseCallback rejectedCallback = null)
      : this() {
      InitWithCallback(previous, fulfilledCallback, rejectedCallback);
    }

    public int ID { get; } = Interlocked.Increment(ref accum);

    public PromiseState State { get; private set; }

    public EcmaValue Value { get; private set; }

    public static EcmaValue All(IEnumerable<Promise> promises) {
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
            promise.Reject(other.Value);
          } else if (++count == list.Count) {
            promise.Resolve(new EcmaArray(list.Select(v => v.Value).ToList()));
          }
        });
      }
      return promise;
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
    private void Resolve(EcmaValue value) {
      Resolve(value, false);
    }

    [IntrinsicMember(null)]
    [EcmaSpecification("Promise Reject Functions", EcmaSpecificationKind.AbstractOperations)]
    private void Reject(EcmaValue value) {
      SetState(PromiseState.Rejected, value, false);
    }

    [EcmaSpecification("Promise Resolve Functions", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("PromiseResolveThenableJob", EcmaSpecificationKind.AbstractOperations)]
    private void Resolve(EcmaValue value, bool finalize) {
      if (value.Type == EcmaValueType.Object) {
        RuntimeObject obj = value.ToObject();
        if (obj == this) {
          SetStateFinalize(PromiseState.Rejected, new EcmaTypeErrorException(InternalString.Error.PromiseSelfResolution).ToValue());
          return;
        }
        try {
          RuntimeObject then = obj.GetMethod(WellKnownProperty.Then);
          if (then != null) {
            then.Call(value, (PromiseResolver)Resolve, (PromiseResolver)Reject);
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
            Resolve(value, true);
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
        executor(Resolve, Reject);
      } catch (Exception ex) {
        Reject(EcmaValueUtility.GetValueFromException(ex));
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
        Type type = task.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
          value = new EcmaValue(type.GetProperty("Result").GetValue(type, null));
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
      SetState(PromiseState.Rejected, new EcmaError(reason), false);
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[PromiseState]]", this.State.ToString().ToLower());
      meta.EnumerableProperties.Add("[[PromiseResult]]", this.Value);
    }
  }
}
