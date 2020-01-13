using Codeless.Ecma.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Codeless.Ecma {
  [Cloneable(false)]
  public class SharedArrayBuffer : ArrayBuffer {
    private Dictionary<long, List<WaiterHandle>> waiterLists;

    public SharedArrayBuffer()
      : base(WellKnownObject.SharedArrayBufferPrototype) { }

    public SharedArrayBuffer(RuntimeObject constructor)
      : base(WellKnownObject.SharedArrayBufferPrototype, constructor) { }

    public SharedArrayBuffer(long size)
      : base(WellKnownObject.SharedArrayBufferPrototype, size) { }

    public SharedArrayBuffer(byte[] buffer)
      : base(WellKnownObject.SharedArrayBufferPrototype, buffer) { }

    public SharedArrayBuffer(SharedArrayBuffer buffer)
      : base(WellKnownObject.SharedArrayBufferPrototype, (byte[])buffer?.SyncRoot) {
      Dictionary<long, List<WaiterHandle>> waiterLists = new Dictionary<long, List<WaiterHandle>>();
      Interlocked.CompareExchange(ref buffer.waiterLists, waiterLists, null);
      this.waiterLists = buffer.waiterLists;
    }

    public object SyncRoot {
      get { return this.Int32Array; }
    }

    public int Notify(long position, int count) {
      lock (this.SyncRoot) {
        int remaining = count;
        if (waiterLists != null && waiterLists.TryGetValue(position, out List<WaiterHandle> list)) {
          while (remaining > 0 && list.Count > 0) {
            WaiterHandle handle = list[0];
            if (handle.Set()) {
              remaining--;
            }
            list.RemoveAt(0);
          }
        }
        return count - remaining;
      }
    }

    public bool Wait(long position, int comparand, int ms, out bool comparandEquals) {
      object syncRoot = this.SyncRoot;
      bool exitAtFinally = true;
      Monitor.Enter(syncRoot);
      try {
        // compare value with bytes in position as a signed int
        // because only Int32Array is waitable through the Atomics object
        comparandEquals = GetInt32(position) == comparand;
        if (!comparandEquals) {
          return false;
        }
        WaiterHandle handle = new WaiterHandle();
        if (waiterLists == null) {
          waiterLists = new Dictionary<long, List<WaiterHandle>>();
        }
        List<WaiterHandle> list;
        if (!waiterLists.TryGetValue(position, out list)) {
          list = new List<WaiterHandle>();
          waiterLists.Add(position, list);
        }
        list.Add(handle);
        exitAtFinally = false;
        Monitor.Exit(syncRoot);
        // add extra millisecond to ensure that
        // thread does not wake up within a milliseconds before desired amount of time
        if (!handle.WaitOne(ms < 0 ? ms : ms + 1)) {
          lock (syncRoot) {
            list.Remove(handle);
          }
          return false;
        }
        return true;
      } finally {
        if (exitAtFinally) {
          Monitor.Exit(syncRoot);
        }
      }
    }

    private class WaiterHandle {
      private readonly ManualResetEvent handle = new ManualResetEvent(false);
      private int counter = 1;

      public bool WaitOne(int milliseconds) {
        return handle.WaitOne(milliseconds) || Interlocked.Decrement(ref counter) != 0;
      }

      public bool Set() {
        handle.Set();
        return Interlocked.Decrement(ref counter) == 0;
      }
    }
  }
}
