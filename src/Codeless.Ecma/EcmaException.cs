using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public enum EcmaNativeErrorType {
    Invalid,
    EvalError,
    RangeError,
    ReferenceError,
    SyntaxError,
    TypeError,
    UriError
  }

  public abstract class EcmaException : Exception, IEcmaValueConvertible {
    internal EcmaException(string message)
      : base(message) { }

    public virtual EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.Invalid; }
    }

    public EcmaValue ToValue() {
      return ErrorConstructor.CreateError(this);
    }
  }

  public class EcmaTypeErrorException : EcmaException {
    internal EcmaTypeErrorException(string message)
      : base(message) { }

    internal EcmaTypeErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }

    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.TypeError; }
    }
  }

  public class EcmaRangeErrorException : EcmaException {
    internal EcmaRangeErrorException(string message)
      : base(message) { }

    internal EcmaRangeErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }

    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.RangeError; }
    }
  }

  public class EcmaSyntaxErrorException : EcmaException {
    internal EcmaSyntaxErrorException(string message)
      : base(message) { }

    internal EcmaSyntaxErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }

    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.SyntaxError; }
    }
  }

  public class EcmaReferenceErrorException : EcmaException {
    internal EcmaReferenceErrorException(string message)
      : base(message) { }

    internal EcmaReferenceErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }
    
    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.ReferenceError; }
    }
  }

  public class EcmaEvalErrorException : EcmaException {
    internal EcmaEvalErrorException(string message)
      : base(message) { }

    internal EcmaEvalErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }

    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.EvalError; }
    }
  }
  
  public class EcmaUriErrorException : EcmaException {
    internal EcmaUriErrorException(string message)
      : base(message) { }

    internal EcmaUriErrorException(string message, params object[] p)
      : base(String.Format(message, p)) { }

    public override EcmaNativeErrorType ErrorType {
      get { return EcmaNativeErrorType.UriError; }
    }
  }
}
