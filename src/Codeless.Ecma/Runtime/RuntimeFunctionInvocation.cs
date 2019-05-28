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

    private readonly EcmaValue[] arguments;
    private ArgumentList argumentObject;
    private bool disposed;

    internal RuntimeFunctionInvocation(RuntimeFunction method, EcmaValue[] arguments) {
      this.FunctionObject = method;
      this.Parent = current;
      this.arguments = arguments;
      current = this;
    }

    public static RuntimeFunctionInvocation Current {
      get { return current; }
    }

    public RuntimeFunctionInvocation Parent { get; private set; }
    public RuntimeFunction FunctionObject { get; private set; }
    public ThisBindingStatus ThisBindingStatus { get; private set; }
    public EcmaValue ThisValue { get; set; }
    public RuntimeObject HomeObject { get; set; }
    public RuntimeObject NewTarget { get; set; }

    public ArgumentList Arguments {
      get {
        if (argumentObject == null) {
          argumentObject = new ArgumentList(this, arguments);
        }
        return argumentObject;
      }
    }

    public void Dispose() {
      if (disposed) {
        throw new ObjectDisposedException("");
      }
      disposed = true;
      current = Parent;
    }
  }
}
