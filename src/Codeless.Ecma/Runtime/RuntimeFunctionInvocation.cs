using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Codeless.Ecma.Runtime {
  public enum ThisBindingStatus {
    Uninitialized,
    Initialized,
    Lexical
  }

  [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
  public class RuntimeFunctionInvocation : IDisposable {
    [ThreadStatic]
    private static RuntimeFunctionInvocation current;

    private readonly EcmaValue[] arguments;
    private ArgumentList argumentObject;
    private bool disposed;

    internal RuntimeFunctionInvocation(RuntimeFunction method, EcmaValue thisValue, EcmaValue[] arguments, RuntimeObject newTarget = null) {
      this.FunctionObject = method;
      this.Parent = current;
      this.ThisValue = thisValue;
      this.NewTarget = newTarget;
      this.arguments = arguments;
      if (method.HomeObject != null) {
        this.Super = new SuperAccessor(this, method.HomeObject);
      }
      current = this;
    }

    public static RuntimeFunctionInvocation Current {
      get { return current; }
    }

    public RuntimeFunctionInvocation Parent { get; private set; }
    public RuntimeFunction FunctionObject { get; private set; }
    public ThisBindingStatus ThisBindingStatus { get; private set; }
    public EcmaValue ThisValue { get; internal set; }
    public SuperAccessor Super { get; private set; }
    public RuntimeObject NewTarget { get; private set; }

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

    internal string GetDebuggerDisplay() {
      EcmaValue name = this.FunctionObject[WellKnownProperty.Name];
      string location = "<anonymous>";
      if (name == default) {
        return location;
      }
      return name.ToString() + " (" + location + ")";
    }
  }
}
