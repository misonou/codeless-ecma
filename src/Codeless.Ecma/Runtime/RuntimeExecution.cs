using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  public delegate void RuntimeExecutionStart(RuntimeRealm parentRealm);

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
    private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private RuntimeRealm defaultRealm;
    private int nextId;

    private RuntimeExecution() {
      current = this;
      defaultRealm = new RuntimeRealm();
      this.Thread = Thread.CurrentThread;
      this.CanSuspend = true;
      this.AutoExit = true;
    }

    public EventHandler<RuntimeUnhandledExceptionEventArgs> UnhandledException;

    public Thread Thread { get; }

    public bool CanSuspend { get; set; }

    public bool AutoExit { get; set; }

    public bool ShouldExit {
      get { return queue.Count == 0 && handles.Count == 0 && this.AutoExit; }
    }

    public RuntimeRealm DefaultRealm {
      get {
        if (defaultRealm.Disposed) {
          defaultRealm = new RuntimeRealm();
        }
        return defaultRealm;
      }
    }

    public static RuntimeExecution Current {
      get { return EnsureInstance(); }
    }

    public static RuntimeExecution CreateWorkerThread(RuntimeExecutionStart action, bool autoExit) {
      ManualResetEvent waitHandle = new ManualResetEvent(false);
      RuntimeRealm parentRealm = RuntimeRealm.Current;
      RuntimeExecution execution = null;
      new Thread(() => {
        execution = EnsureInstance();
        execution.AutoExit = autoExit;
        try {
          action(parentRealm);
          waitHandle.Set();
          ContinueUntilEnd();
        } catch (Exception ex) {
          SendUnhandledException(ex);
          waitHandle.Set();
        }
      }).Start();
      waitHandle.WaitOne();
      return execution;
    }

    public static double GetPerformaceTimestamp(double precision) {
      Stopwatch stopwatch = EnsureInstance().stopwatch;
      stopwatch.Stop();
      double milliseconds = Math.Round(stopwatch.ElapsedTicks * (1000 / precision) / Stopwatch.Frequency) * precision;
      stopwatch.Start();
      return milliseconds;
    }

    [EcmaSpecification("EnqueueJob", EcmaSpecificationKind.AbstractOperations)]
    public static void Enqueue(Action action) {
      Enqueue(action, 0, RuntimeExecutionFlags.None);
    }

    public static void Enqueue(Action action, Task task) {
      RuntimeExecution current = EnsureInstance();
      current.Enqueue(RuntimeRealm.Current, action, task);
    }

    public static RuntimeExecutionHandle Enqueue(Action action, int milliseconds, RuntimeExecutionFlags flags) {
      RuntimeExecution current = EnsureInstance();
      return current.Enqueue(RuntimeRealm.Current, action, milliseconds, flags);
    }

    public void Enqueue(RuntimeRealm realm, Action action) {
      Guard.ArgumentNotNull(realm, "realm");
      Guard.ArgumentNotNull(action, "action");
      if (realm.ExecutionContext != this) {
        throw new InvalidOperationException("Realm must be belong to the same execution context");
      }
      Enqueue(realm, action, 0, RuntimeExecutionFlags.None);
      WakeImmediately();
    }

    public void Enqueue(RuntimeRealm realm, Action action, Task task) {
      Guard.ArgumentNotNull(realm, "realm");
      Guard.ArgumentNotNull(task, "task");
      Guard.ArgumentNotNull(action, "action");
      if (realm.ExecutionContext != this) {
        throw new InvalidOperationException("Realm must be belong to the same execution context");
      }
      if (!task.IsCompleted) {
        ManualResetEvent handle = new ManualResetEvent(false);
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
      WakeImmediately();
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

    public void WakeImmediately() {
      if (this.Thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin) {
        resetEvent.Set();
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
      bool result = false;
      while (Continue()) {
        result = true;
      }
      return result;
    }

    [EcmaSpecification("RunJobs", EcmaSpecificationKind.AbstractOperations)]
    public static bool Continue() {
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
      if (!executed && !current.ShouldExit) {
        if (waitUntil >= DateTime.Now) {
          SuspendThread(waitUntil.Value - DateTime.Now);
        } else if (waitUntil == null) {
          SuspendThread(null);
        }
      }
      return !current.ShouldExit;
    }

    public static void SendUnhandledException(EcmaValue value) {
      Exception ex = null;
      if (value.GetUnderlyingObject() is EcmaError e) {
        ex = e.Exception;
      }
      RuntimeExecution current = EnsureInstance();
      current.UnhandledException?.Invoke(null, new RuntimeUnhandledExceptionEventArgs(ex ?? EcmaException.FromValue(value), value));
    }

    public static void SendUnhandledException(Exception ex) {
      Guard.ArgumentNotNull(ex, "ex");
      RuntimeExecution current = EnsureInstance();
      current.UnhandledException?.Invoke(null, new RuntimeUnhandledExceptionEventArgs(ex, EcmaValueUtility.GetValueFromException(ex)));
    }

    private static void SuspendThread(TimeSpan? timeSpan) {
      AutoResetEvent resetEvent = current.resetEvent;
      List<WaitHandle> handles;
      lock (current) {
        handles = new List<WaitHandle>(current.handles);
      }
      resetEvent.Reset();
      handles.Add(resetEvent);
      int index = timeSpan.HasValue ? WaitHandle.WaitAny(handles.ToArray(), timeSpan.Value) : WaitHandle.WaitAny(handles.ToArray());
      if (index >= 0 && handles[index] != resetEvent) {
        lock (current) {
          current.handles.RemoveAt(index);
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
