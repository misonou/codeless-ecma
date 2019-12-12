using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  public class RuntimeUnhandledExceptionEventArgs : EventArgs {
    public RuntimeUnhandledExceptionEventArgs(Exception ex, EcmaValue value) {
      this.Exception = ex;
      this.Value = value;
    }

    public EcmaValue Value { get; }
    public Exception Exception { get; }
  }

  public struct RuntimeExecutionHandle {
    public RuntimeExecutionHandle(int id) {
      this.Id = id;
    }

    public int Id { get; }
  }

  [Flags]
  public enum RuntimeExecutionFlags {
    None,
    Recurring = 1,
    Cancellable = 2
  }

  public class RuntimeExecution {
    [ThreadStatic]
    private static RuntimeExecution current;

    private readonly List<Job> queue = new List<Job>();
    private readonly List<WaitHandle> handles = new List<WaitHandle>();
    private int nextId;

    private RuntimeExecution() {
      this.Thread = Thread.CurrentThread;
    }

    public EventHandler<RuntimeUnhandledExceptionEventArgs> UnhandledException;

    public CancellationTokenSource CancellationToken { get; private set; }

    public Thread Thread { get; }

    public static RuntimeExecution Current {
      get { return EnsureInstance(); }
    }

    [EcmaSpecification("EnqueueJob", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeExecutionHandle Enqueue(Action action) {
      return Enqueue(action, 0, RuntimeExecutionFlags.None);
    }

    public static RuntimeExecutionHandle Enqueue(Action action, int milliseconds, RuntimeExecutionFlags flags) {
      RuntimeExecution current = EnsureInstance();
      return current.Enqueue(RuntimeRealm.Current, action, milliseconds, flags);
    }

    public RuntimeExecutionHandle Enqueue(RuntimeRealm realm, Action action, int milliseconds, RuntimeExecutionFlags flags) {
      Guard.ArgumentNotNull(realm, "realm");
      Guard.ArgumentNotNull(action, "action");
      if (realm.ExecutionContext != this) {
        throw new InvalidOperationException("Realm must be belong to the same execution context");
      }
      if (milliseconds < 0) {
        throw new ArgumentOutOfRangeException("milliseconds", "Delay or interval in milliseconds cannot be negative");
      }
      Job job = new Job {
        Id = (flags & RuntimeExecutionFlags.Cancellable) != 0 ? ++nextId : 0,
        Realm = realm,
        Action = action,
        TriggerTime = milliseconds > 0 ? DateTime.Now.AddMilliseconds(milliseconds) : DateTime.MinValue,
        RecurringMilliseconds = (flags & RuntimeExecutionFlags.Recurring) != 0 ? milliseconds : -1
      };
      queue.Add(job);
      return new RuntimeExecutionHandle(job.Id);
    }

    public static void SetInterrupt(Task task, Action action) {
      RuntimeExecution current = EnsureInstance();
      current.SetInterrupt(RuntimeRealm.Current, task, action);
    }

    public void SetInterrupt(RuntimeRealm realm, Task task, Action action) {
      Guard.ArgumentNotNull(realm, "realm");
      Guard.ArgumentNotNull(task, "task");
      Guard.ArgumentNotNull(action, "action");
      if (realm.ExecutionContext != this) {
        throw new InvalidOperationException("Realm must be belong to the same execution context");
      }
      if (!task.IsCompleted) {
        AutoResetEvent handle = new AutoResetEvent(false);
        lock (this) {
          handles.Add(handle);
        }
        task.ContinueWith(_ => {
          Enqueue(realm, action, 0, RuntimeExecutionFlags.None);
          handle.Set();
        });
      } else {
        Enqueue(realm, action, 0, RuntimeExecutionFlags.None);
      }
    }

    public static bool Cancel(RuntimeExecutionHandle handle) {
      RuntimeExecution current = EnsureInstance();
      if (handle.Id <= 0) {
        throw new ArgumentOutOfRangeException("handle", "Invalid handle");
      }
      int index = current.queue.FindIndex(v => v.Id == handle.Id);
      if (index >= 0) {
        current.queue.RemoveAt(index);
        return true;
      }
      return false;
    }

    public static bool ContinueUntilEnd() {
      return ContinueUntilEnd(new CancellationTokenSource());
    }

    public static bool ContinueUntilEnd(CancellationTokenSource ct) {
      bool result = false;
      while (Continue(ct)) {
        result = true;
      }
      return result;
    }

    [EcmaSpecification("RunJobs", EcmaSpecificationKind.AbstractOperations)]
    public static bool Continue(CancellationTokenSource ct) {
      RuntimeExecution current = EnsureInstance();
      bool executed = false;
      DateTime? waitUntil = null;
      List<Job> actions = new List<Job>(current.queue);
      current.queue.Clear();

      foreach (Job job in actions) {
        if (job.Realm.Disposed) {
          continue;
        }
        if (!waitUntil.HasValue || waitUntil > job.TriggerTime) {
          waitUntil = job.TriggerTime;
        }
        if (DateTime.Now < job.TriggerTime) {
          current.queue.Add(job);
          continue;
        }
        executed = true;
        if (job.RecurringMilliseconds > 0) {
          current.queue.Add(new Job {
            Id = job.Id,
            Realm = job.Realm,
            Action = job.Action,
            TriggerTime = DateTime.Now.AddMilliseconds(job.RecurringMilliseconds),
            RecurringMilliseconds = job.RecurringMilliseconds
          });
        }
        try {
          job.Realm.Execute(job.Action);
        } catch (Exception ex) {
          SendUnhandledException(ex);
        }
      }
      bool hasPending = current.queue.Count > 0 || current.handles.Count > 0;
      if (!executed && hasPending) {
        if (waitUntil >= DateTime.Now) {
          SuspendThread(ct, waitUntil.Value - DateTime.Now);
        } else if (waitUntil == null) {
          SuspendThread(ct, null);
        }
      }
      return hasPending;
    }

    public static void SendUnhandledException(EcmaValue value) {
      Exception ex = null;
      if (value.GetUnderlyingObject() is EcmaError e) {
        ex = e.Exception;
      }
      RuntimeExecution current = EnsureInstance();
      current.UnhandledException?.Invoke(null, new RuntimeUnhandledExceptionEventArgs(ex ?? new EcmaRuntimeException(value), value));
    }

    public static void SendUnhandledException(Exception ex) {
      Guard.ArgumentNotNull(ex, "ex");
      RuntimeExecution current = EnsureInstance();
      current.UnhandledException?.Invoke(null, new RuntimeUnhandledExceptionEventArgs(ex, EcmaValueUtility.GetValueFromException(ex)));
    }

    private static void SuspendThread(CancellationTokenSource ct, TimeSpan? timeSpan) {
      List<WaitHandle> handles;
      current.CancellationToken = ct;
      lock (current) {
        handles = new List<WaitHandle>(current.handles);
      }
      if (handles.Count > 0) {
        handles.Add(ct.Token.WaitHandle);
        int index = timeSpan.HasValue ? WaitHandle.WaitAny(handles.ToArray(), timeSpan.Value) : WaitHandle.WaitAny(handles.ToArray());
        if (index >= 0 && index != handles.Count - 1) {
          lock (current) {
            current.handles.RemoveAt(index);
          }
        }
      }
    }

    private static RuntimeExecution EnsureInstance() {
      if (current == null) {
        current = new RuntimeExecution();
      }
      return current;
    }

    private class Job {
      public int Id { get; set; }
      public RuntimeRealm Realm { get; set; }
      public Action Action { get; set; }
      public DateTime TriggerTime { get; set; }
      public int RecurringMilliseconds { get; set; }
    }
  }
}
