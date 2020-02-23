using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;

namespace Codeless.Ecma.UnitTest.Harness {
  public class Agent : IDisposable {
    // Agents call Atomics.wait on this location to sleep.
    public const int SLEEP_LOC = 0;
    // 1 if the started worker is ready, 0 otherwise.
    public const int START_LOC = 1;
    // The number of workers that have received the broadcast.
    public const int BROADCAST_LOC = 2;
    // Each worker has a count of outstanding reports; worker N uses memory location [WORKER_REPORT_LOC + N].
    public const int WORKER_REPORT_LOC = 3;

    public class Timeout {
      public const int Yield = 100;
      public const int Small = 200;
      public const int Long = 1000;
      public const int Huge = 10000;
    }

    public delegate void WorkerStart(WorkerAgent agent);

    private readonly Queue<EcmaValue> pendingReports = new Queue<EcmaValue>();
    private readonly List<EcmaValue> workers = new List<EcmaValue>();
    private EcmaValue i32a = Null;

    public void Start(WorkerStart script) {
      if (i32a == Null) {
        i32a = new Int32Array(new SharedArrayBuffer(256));
      }
      List<EcmaValue> messages = new List<EcmaValue>();
      EcmaValue worker = StartWorker(script, messages);
      worker["index"] = workers.Count;
      worker["getMessage"] = FunctionLiteral(() => {
        if (messages.Count == 0) {
          return default;
        }
        EcmaValue msg = messages[0];
        messages.RemoveAt(0);
        return msg;
      });
      worker.Invoke("postMessage", StaticHelper.CreateObject(new { kind = "start", index = worker["index"], i32a }));
      workers.Add(worker);
    }

    public void Broadcast(EcmaValue sab, EcmaValue id = default) {
      if (!sab.InstanceOf(Global.SharedArrayBuffer)) {
        throw new EcmaTypeErrorException("sab must be a SharedArrayBuffer.");
      }
      Global.Atomics.Invoke("store", i32a, BROADCAST_LOC, 0);
      foreach (EcmaValue worker in workers) {
        worker.Invoke("postMessage", StaticHelper.CreateObject(new { kind = "broadcast", id = id | 0, sab }));
      }
      while (Global.Atomics.Invoke("load", i32a, BROADCAST_LOC) != workers.Count) ;
    }

    public void SafeBroadcast(EcmaValue typedArray) {
      EcmaValue Constructor = Global.Object.Invoke("getPrototypeOf", typedArray)["constructor"];
      EcmaValue temp = Constructor.Construct(Global.SharedArrayBuffer.Construct(Constructor["BYTES_PER_ELEMENT"]));
      try {
        // This will never actually wait, but that's fine because we only
        // want to ensure that this typedArray CAN be waited on and is shareable.
        Global.Atomics.Invoke("wait", temp, 0, 1);
        // TODO: when BigInt is implemented changes to the following line to include BigInt typed array
        // Atomics.wait(temp, 0, Constructor == Global.Int32Array ? 1 : BigInt(1));
      } catch (Exception error) {
        NUnit.Framework.Assert.Fail($"{Constructor["name"]} cannot be used as a shared typed array. ({error})");
      }
      Broadcast(typedArray["buffer"]);
    }

    public void WaitUntil(EcmaValue typedArray, EcmaValue index, EcmaValue expected) {
      EcmaValue agents;
      while ((agents = Global.Atomics.Invoke("load", typedArray, index)) != expected) ;
      Assert.That(agents, Is.EqualTo(expected), "Reporting number of 'agents' equals the value of 'expected'");
    }

    public void Sleep(int ms) {
      Global.Atomics.Invoke("wait", i32a, SLEEP_LOC, 0, ms);
    }

    public void TryYield() {
      Sleep(Timeout.Yield);
    }

    public void TrySleep(int ms) {
      Sleep(ms);
    }

    public EcmaValue GetReport() {
      EcmaValue result;
      do {
        Sleep(1);
        foreach (EcmaValue worker in workers) {
          while (Global.Atomics.Invoke("load", i32a, WORKER_REPORT_LOC + worker["index"]) > 0) {
            pendingReports.Enqueue(worker.Invoke("getMessage"));
            Global.Atomics.Invoke("sub", i32a, WORKER_REPORT_LOC + worker["index"], 1);
          }
        }
        result = pendingReports.Count > 0 ? pendingReports.Dequeue() : Null;
      } while (result == Null);
      return result;
    }

    public EcmaValue MonotonicNow() {
      return RuntimeExecution.GetPerformaceTimestamp(0.005);
    }

    public void Dispose() {
      foreach (EcmaValue worker in workers) {
        worker.Invoke("close");
      }
    }

    private EcmaValue StartWorker(WorkerStart script, List<EcmaValue> captures = null) {
      Worker workerObj = new Worker();
      RuntimeObject worker = RuntimeRealm.Current.ResolveRuntimeObject(workerObj);
      EcmaValue postMessage = GetPostMessageCallback(worker, captures);

      RuntimeExecution execution = RuntimeExecution.CreateWorkerThread(hostRealm => {
        WorkerAgent agent = new WorkerAgent(postMessage);
        This.ToObject()["postMessage"] = postMessage;
        This.ToObject()["onmessage"] = FunctionLiteral(msg => {
          switch (msg["kind"].ToStringOrThrow()) {
            case "start":
              agent.Init(msg["i32a"], msg["index"]);
              script(agent);
              break;
            case "broadcast":
              Global.Atomics.Invoke("add", i32a, BROADCAST_LOC, 1);
              agent.Broadcast(EcmaArray.Of(msg["sab"], msg["id"]));
              break;
          }
        });
      }, false);
      workerObj.Init(execution.DefaultRealm);
      return worker;
    }

    private static EcmaValue GetPostMessageCallback(RuntimeObject receiver, List<EcmaValue> captures = null) {
      RuntimeRealm realm = receiver.Realm;
      return FunctionLiteral(msg => {
        if (captures != null) {
          captures.Add(msg);
        }
        realm.ExecutionContext.Enqueue(realm, () => {
          EcmaValue onmessage = receiver["onmessage"];
          if (onmessage.IsCallable) {
            onmessage.Call(Undefined, msg);
          }
        });
      });
    }

    public class Worker {
      private RuntimeRealm workerRealm;
      private EcmaValue postMessage;

      internal void Init(RuntimeRealm workerRealm) {
        this.workerRealm = workerRealm;
        this.postMessage = GetPostMessageCallback(workerRealm.GetRuntimeObject(WellKnownObject.Global));
      }

      [IntrinsicMember("postMessage")]
      public void PostMessage(EcmaValue message) {
        postMessage.Call(Undefined, message);
      }

      [IntrinsicMember("close")]
      public void Close() {
        workerRealm.ExecutionContext.AutoExit = true;
        workerRealm.ExecutionContext.WakeImmediately();
      }
    }

    public class WorkerAgent {
      private readonly Queue<EcmaValue> broadcasts = new Queue<EcmaValue>();
      private readonly EcmaValue postMessage;
      private EcmaValue pendingReceiver;

      public WorkerAgent(EcmaValue postMessage) {
        this.postMessage = postMessage;
      }

      public EcmaValue Int32Array { get; private set; } = Null;
      public EcmaValue Index { get; private set; }

      public void Init(EcmaValue i32a, EcmaValue index) {
        this.Int32Array = i32a;
        this.Index = index;
      }

      public void Broadcast(EcmaValue message) {
        broadcasts.Enqueue(message);
        HandleBroadcast();
      }

      public void HandleBroadcast() {
        if (pendingReceiver && broadcasts.Count > 0) {
          pendingReceiver.Call(Null, EcmaValueUtility.CreateListFromArrayLike(broadcasts.Dequeue()));
          pendingReceiver = default;
        }
      }

      public void ReceiveBroadcast(Action<EcmaValue> action) {
        ReceiveBroadcast((EcmaValue)action);
      }

      public void ReceiveBroadcast(EcmaValue receiver) {
        pendingReceiver = receiver;
        HandleBroadcast();
      }

      public void Report(EcmaValue msg) {
        postMessage.Call(Undefined, Global.String.Call(Undefined, msg));
        Global.Atomics.Invoke("add", Int32Array, WORKER_REPORT_LOC + Index, 1);
      }

      public void Sleep(int ms) {
        Global.Atomics.Invoke("wait", Int32Array, SLEEP_LOC, 0, ms);
      }

      public void Leaving() { }

      public EcmaValue MonotonicNow() {
        return RuntimeExecution.GetPerformaceTimestamp(0.005);
      }
    }
  }
}
