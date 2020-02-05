using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class WorkerMessageEventArgs : EventArgs {
    public WorkerMessageEventArgs(EcmaValue message) {
      this.Message = message;
    }

    public EcmaValue Message { get; }
  }

  public class Worker : RuntimeObject {
    private RuntimeExecution execution;
    private CrossRealmMessageHandler workerReceiver;
    private CrossRealmMessageHandler hostReceiver;

    public Worker(Action workerStart)
      : base(WellKnownObject.ObjectPrototype) {
      Init(workerStart);
    }

    public event EventHandler<WorkerMessageEventArgs> MessageReceived {
      add => hostReceiver.MessageReceived += value;
      remove => hostReceiver.MessageReceived -= value;
    }

    internal void Init(Action workerStart) {
      Guard.ArgumentNotNull(workerStart, "workerStart");
      RuntimeObject workerGlobal = null;
      this.hostReceiver = new CrossRealmMessageHandler(this);
      this.execution = RuntimeExecution.CreateWorkerThread(hostRealm => {
        workerGlobal = RuntimeRealm.Current.GetRuntimeObject(WellKnownObject.Global);
        workerGlobal["postMessage"] = (Action<EcmaValue, EcmaValue>)hostReceiver.PostMessage;
        workerStart();
      }, false);
      this.workerReceiver = new CrossRealmMessageHandler(workerGlobal);

      DefineOwnPropertyNoChecked("postMessage", new EcmaPropertyDescriptor((Action<EcmaValue, EcmaValue>)workerReceiver.PostMessage, EcmaPropertyAttributes.DefaultMethodProperty));
      DefineOwnPropertyNoChecked("terminate", new EcmaPropertyDescriptor((Action)this.Terminate, EcmaPropertyAttributes.DefaultMethodProperty));
      this.Realm.BeforeDisposed += Terminate;
    }

    public void PostMessage(EcmaValue value) {
      workerReceiver.PostMessage(value, EcmaValue.EmptyArray);
    }

    public void PostMessage(EcmaValue value, EcmaValue[] transfer) {
      workerReceiver.PostMessage(value, transfer);
    }

    [IntrinsicMember("terminate")]
    public void Terminate() {
      Terminate(false);
    }

    private void Terminate(bool isRealmDisposed) {
      execution.AutoExit = true;
      execution.WakeImmediately();
      if (!isRealmDisposed) {
        this.Realm.BeforeDisposed -= Terminate;
      }
    }

    private void Terminate(object sender, EventArgs e) {
      Terminate(true);
    }

    private class CrossRealmMessageHandler {
      private readonly RuntimeObject receiver;

      public CrossRealmMessageHandler(RuntimeObject receiver) {
        Guard.ArgumentNotNull(receiver, "receiver");
        this.receiver = receiver;
      }

      public event EventHandler<WorkerMessageEventArgs> MessageReceived;

      [IntrinsicMember("postMessage", FunctionLength = 1)]
      public void PostMessage(EcmaValue value, EcmaValue transfer) {
        PostMessage(value, EcmaValueUtility.CreateListFromArrayLikeOrEmpty(transfer));
      }

      public void PostMessage(EcmaValue value, EcmaValue[] transfer) {
        value = EcmaValueUtility.CloneValue(value, transfer, receiver.Realm);
        receiver.Realm.ExecutionContext.Enqueue(receiver.Realm, () => {
          MessageReceived?.Invoke(receiver, new WorkerMessageEventArgs(value));
          EcmaValue onmessage = receiver["onmessage"];
          if (onmessage.IsCallable) {
            onmessage.Call(receiver, value);
          }
        });
      }
    }
  }
}
