using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private PromiseCallback fulfilledCallback;
    private PromiseCallback rejectedCallback;
    private PromiseCallback finallyCallback;

    public Promise()
      : base(WellKnownObject.PromisePrototype) { }

    public Promise(Task task)
      : this() {
      Guard.ArgumentNotNull(task, "task");
      task.ContinueWith(HandleCompletedTask);
    }

    public Promise(Exception ex)
      : this(PromiseState.Rejected, new EcmaValue(ex)) { }

    public Promise(PromiseState state, EcmaValue value)
      : this() {
      SetStateFinalize(state, value);
    }

    public Promise(RuntimeFunction callback)
      : this() {
      Guard.ArgumentNotNull(callback, "callback");
      InitWithCallback(callback);
    }

    public Promise(PromiseExecutor callback)
      : this() {
      Guard.ArgumentNotNull(callback, "callback");
      callback(Resolve, Reject);
    }

    public Promise(Promise previous, PromiseCallback fulfilledCallback = null, PromiseCallback rejectedCallback = null, PromiseCallback finallyCallback = null)
      : this() {
      Guard.ArgumentNotNull(previous, "previous");
      this.fulfilledCallback = fulfilledCallback;
      this.rejectedCallback = rejectedCallback;
      this.finallyCallback = finallyCallback;
      // add hook to itself so that this promise can react to the transformed result
      // from the specified [onFulfill] or [onReject] handlers
      // before other chained promises
      AddHook(this);
      previous.AddHook(this);
    }

    public PromiseState State { get; private set; }

    public EcmaValue Value { get; private set; }

    public static EcmaValue All(IEnumerable<Promise> promises) {
      Guard.ArgumentNotNull(promises, "promises");
      List<Promise> list = new List<Promise>(promises);
      if (list.Count == 0) {
        return new Promise(PromiseState.Fulfilled, new EcmaArray());
      }
      Promise promise = new Promise();
      EcmaValue[] values = new EcmaValue[list.Count];
      bool[] called = new bool[list.Count];
      int count = 0;
      list.ForEach(p => {
        p.AddHook(promise, v => {
          int index = list.IndexOf(p);
          if (!called[index]) {
            called[index] = true;
            values[index] = v;
            if (++count == values.Length) {
              promise.Resolve(new EcmaArray(values));
            }
          }
          return v;
        });
      });
      return promise;
    }

    public static Promise Any(IEnumerable<Promise> promises) {
      Guard.ArgumentNotNull(promises, "promises");
      List<Promise> list = new List<Promise>(promises);
      if (list.Count == 0) {
        return new Promise(PromiseState.Fulfilled, new EcmaArray());
      }
      Promise promise = new Promise();
      foreach (Promise p in list) {
        p.AddHook(promise);
      }
      return promise;
    }

    internal void InitWithCallback(RuntimeObject callback) {
      Guard.ArgumentNotNull(callback, "callback");
      callback.Call(EcmaValue.Undefined,
        RuntimeFunction.Create((Action<EcmaValue>)this.Resolve),
        RuntimeFunction.Create((Action<EcmaValue>)this.Reject));
    }

    private void AddHook(Promise other, PromiseCallback onfulfill = null) {
      switch (this.State) {
        case PromiseState.Pending:
          fulfilledCallback += onfulfill ?? other.ResolveCallback;
          rejectedCallback += other.RejectCallback;
          break;
        case PromiseState.Fulfilled:
          fulfilledCallback += onfulfill ?? other.ResolveCallback;
          if (fulfilledCallback.GetInvocationList().Length == 1) {
            SetState(PromiseState.Fulfilled, this.Value);
          }
          break;
        case PromiseState.Rejected:
          rejectedCallback += other.RejectCallback;
          if (rejectedCallback.GetInvocationList().Length == 1) {
            SetState(PromiseState.Fulfilled, this.Value);
          }
          break;
      }
    }

    private void Resolve(EcmaValue value) {
      SetState(PromiseState.Fulfilled, value);
    }

    private void Reject(EcmaValue value) {
      SetState(PromiseState.Rejected, value);
    }

    private EcmaValue ResolveCallback(EcmaValue value) {
      if (value.GetUnderlyingObject() is Promise other) {
        other.AddHook(this);
      } else {
        SetState(PromiseState.Fulfilled, value);
      }
      return value;
    }

    private EcmaValue RejectCallback(EcmaValue value) {
      if (value.GetUnderlyingObject() is Promise other) {
        other.AddHook(this);
      } else {
        SetState(PromiseState.Rejected, value);
      }
      return value;
    }

    private void SetState(PromiseState state, EcmaValue value) {
      PromiseCallback callback = state == PromiseState.Fulfilled ? fulfilledCallback : rejectedCallback;
      fulfilledCallback = null;
      rejectedCallback = null;
      if (callback == null) {
        SetStateFinalize(state, value);
      } else {
        this.Realm.Enqueue(() => {
          try {
            EcmaValue result = callback(value);
            if (!(result.GetUnderlyingObject() is Promise)) {
              SetStateFinalize(PromiseState.Fulfilled, result);
            }
          } catch (Exception ex) {
            SetStateFinalize(PromiseState.Rejected, new EcmaValue(ex));
          }
        });
      }
    }

    private void SetStateFinalize(PromiseState state, EcmaValue value) {
      if (this.State != PromiseState.Pending) {
        this.State = state;
        this.Value = value;
        if (finallyCallback != null) {
          PromiseCallback callback = finallyCallback;
          finallyCallback = null;
          callback(EcmaValue.Undefined);
        }
      }
    }

    private void HandleCompletedTask(Task task) {
      if (task.IsFaulted) {
        SetState(PromiseState.Rejected, new EcmaValue(task.Exception));
      } else {
        SetState(PromiseState.Fulfilled, GetValueFromTask(task));
      }
    }

    private EcmaValue GetValueFromTask(Task task) {
      Type type = task.GetType();
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
        return new EcmaValue(type.GetProperty("Result").GetValue(type, null));
      }
      return default;
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[PromiseState]]", this.State.ToString().ToLower());
      meta.EnumerableProperties.Add("[[PromiseResult]]", this.Value);
    }
  }
}
