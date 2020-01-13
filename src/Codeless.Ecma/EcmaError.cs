using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [Cloneable(true)]
  public class EcmaError : RuntimeObject {
    public EcmaError() :
      base(WellKnownObject.ErrorPrototype) {
      Init(null);
    }

    public EcmaError(string message)
      : base(WellKnownObject.ErrorPrototype) {
      Init(message);
    }

    public EcmaError(Exception ex)
      : base(GetPrototype(ex)) {
      Guard.ArgumentNotNull(ex, "ex");
      Init(ex.Message);
      this.Exception = ex;
      if (ex is EcmaException e1) {
        this.CallSite = e1.CallSite;
      }
    }

    public RuntimeFunctionInvocation CallSite { get; } = RuntimeFunctionInvocation.Current;

    public Exception Exception { get; }

    protected override string ToStringTag => InternalString.ObjectTag.Error;

    internal void Init(string message) {
      if (message != null) {
        this.DefinePropertyOrThrow(WellKnownProperty.Message, new EcmaPropertyDescriptor(message, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
      if (this.GetOwnProperty(WellKnownProperty.Stack) == null) {
        this.DefinePropertyOrThrow(WellKnownProperty.Stack, new EcmaPropertyDescriptor(new DelegateRuntimeFunction((Func<string>)this.GetStack), EcmaPropertyAttributes.LazyInitialize | EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
    }

    private string GetStack() {
      StringBuilder sb = new StringBuilder(ErrorPrototype.ToString(this));
      for (RuntimeFunctionInvocation invocation = CallSite; invocation != null; invocation = invocation.Parent) {
        sb.AppendLine();
        sb.Append("\tat " + invocation.GetDebuggerDisplay());
      }
      return sb.ToString();
    }

    private static WellKnownObject GetPrototype(Exception ex) {
      if (ex is EcmaException e1) {
        switch (e1.ErrorType) {
          case EcmaNativeErrorType.EvalError:
            return WellKnownObject.EvalErrorPrototype;
          case EcmaNativeErrorType.RangeError:
            return WellKnownObject.RangeErrorPrototype;
          case EcmaNativeErrorType.ReferenceError:
            return WellKnownObject.ReferenceErrorPrototype;
          case EcmaNativeErrorType.SyntaxError:
            return WellKnownObject.SyntaxErrorPrototype;
          case EcmaNativeErrorType.TypeError:
            return WellKnownObject.TypeErrorPrototype;
          case EcmaNativeErrorType.UriError:
            return WellKnownObject.UriErrorPrototype;
        }
      }
      return WellKnownObject.ErrorPrototype;
    }
  }
}
