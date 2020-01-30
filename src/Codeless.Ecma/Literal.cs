using Codeless.Ecma.Runtime;
using System;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public static class Literal {
    public static EcmaValue BigIntLiteral(long value) {
      return BigIntHelper.ToBigInt(value);
    }

    public static EcmaValue BigIntLiteral(string str) {
      Guard.ArgumentNotNull(str, "str");
      if (str.Length == 0) {
        throw new ArgumentException("Literal cannot be empty", "str");
      }
      if (str[str.Length - 1] == 'n') {
        str = str.Substring(0, str.Length - 1);
      }
      // negative non-decimal is not valid BigInt string (error thrown when passed to BigInt constructor)
      // but included here as text like -0xfn can produce intended value
      if (str[0] == '-') {
        return -BigIntHelper.ToBigInt(str.Substring(1));
      }
      return BigIntHelper.ToBigInt(str);
    }

    public static EcmaValue RegExpLiteral(string str) {
      Guard.ArgumentNotNull(str, "str");
      if (str.Length == 0) {
        throw new ArgumentException("Literal cannot be empty", "str");
      }
      int lastPos = str.LastIndexOf('/');
      if (str[0] != '/' || lastPos <= 1) {
        throw new ArgumentException("Invalid RegExp literal", "str");
      }
      return EcmaRegExp.Parse(str.Substring(1, lastPos - 1), str.Substring(lastPos + 1));
    }

    public static EcmaValue FunctionLiteral(Action del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Action<EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Action<EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Action<EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Action<EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(del);
    }

    public static EcmaValue FunctionLiteral(string name, Action del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Action<EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Action<EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Action<EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Action<EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue> del) {
      return new DelegateRuntimeFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(Func<Task> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, Task> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<Task<EcmaValue>> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<Task> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, Task> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, Task> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<Task<EcmaValue>> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(name, del);
    }

    public static EcmaValue FunctionLiteral(string name, Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, Task<EcmaValue>> del) {
      return new AsyncFunction(name, del);
    }
  }
}
