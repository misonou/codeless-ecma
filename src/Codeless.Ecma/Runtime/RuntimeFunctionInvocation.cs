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
    private RuntimeFunctionInvocation previous;
    private ArgumentsObject argumentObject;
    private int childCount;
    private bool disposed;

    internal RuntimeFunctionInvocation(RuntimeFunction method, EcmaValue thisValue, EcmaValue[] arguments, RuntimeObject newTarget = null) {
      this.FunctionObject = method;
      this.Parent = current;
      this.ThisValue = thisValue;
      this.NewTarget = newTarget;
      this.arguments = arguments;
      this.previous = current;
      if (method.IsHomedMethod || method.IsDerivedConstructor) { 
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
    public IGeneratorContext Generator { get; internal set; }
    public bool SuspendOnDispose { get; internal set; }

    public ArgumentsObject Arguments {
      get {
        if (argumentObject == null) {
          argumentObject = new ArgumentsObject(this, arguments);
        }
        return argumentObject;
      }
    }

    public IDisposable Resume() {
      if (disposed) {
        throw new ObjectDisposedException("");
      }
      if (this.Parent != null) {
        this.Parent.childCount--;
      }
      previous = current;
      current = this;
      return this;
    }

    public void Dispose() {
      if (!disposed) {
        RuntimeFunctionInvocation parent = this.Parent;
        bool isSuspendedDispose = parent != null && previous != parent;
        if (current == this) {
          current = previous;
        }
        if (this.SuspendOnDispose) {
          if (parent != null) {
            parent.childCount++;
          }
        } else if (childCount == 0) {
          disposed = true;
          if (isSuspendedDispose && parent.childCount == 0) {
            parent.Dispose();
          }
        }
        previous = null;
      }
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
