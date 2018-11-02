using System;

namespace Codeless.Ecma.Runtime {
  public enum ThisBindingStatus {
    Uninitialized,
    Initialized,
    Lexical
  }

  public class RuntimeFunctionInvocation : IDisposable {
    [ThreadStatic]
    private static RuntimeFunctionInvocation current;

    private RuntimeFunctionInvocation parent;
    private bool disposed;

    internal RuntimeFunctionInvocation(RuntimeFunction method) {
      this.FunctionObject = method;
      this.parent = current;
      current = this;
    }

    internal static RuntimeFunctionInvocation Current {
      get { return current; }
    }

    public RuntimeFunction FunctionObject { get; private set; }
    public ThisBindingStatus ThisBindingStatus { get; private set; }
    public EcmaValue ThisValue { get; set; }
    public RuntimeObject HomeObject { get; set; }
    public RuntimeObject NewTarget { get; set; }

    public void Dispose() {
      if (disposed) {
        throw new ObjectDisposedException("");
      }
      disposed = true;
      current = parent;
    }
  }
}
