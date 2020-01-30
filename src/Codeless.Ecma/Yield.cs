using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public static class Yield {
    public static EcmaValue ResumeValue { get; internal set; }

    public static EcmaValue Done(EcmaValue value) {
      RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
      if (invocation == null || invocation.Generator == null) {
        throw new InvalidOperationException();
      }
      invocation.Generator.Return(value);
      throw new GeneratorClosedException(value);
    }

    public static EcmaValue Many(EcmaValue iterable) {
      RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
      if (invocation == null || invocation.Generator == null) {
        throw new InvalidOperationException();
      }
      IGeneratorEnumerator other;
      if (invocation.Generator.Kind == GeneratorKind.Async) {
        other = new RecursiveYieldAsyncEnumerator(iterable.ToObject().GetAsyncIterator());
      } else if (iterable.GetUnderlyingObject() is IGeneratorEnumerable iterator) {
        other = new RecursiveYieldEnumerator(iterator);
      } else {
        other = new RecursiveYieldEnumerator(new EcmaIteratorEnumerator(iterable));
      }
      GeneratorDelegateEnumerator.GetCurrent().PushStack(other);
      return default;
    }

    public static EcmaValue TryFinally(GeneratorDelegate tryBlock, FinallyGeneratorDelegate finallyBlock) {
      TryCatch(tryBlock, null, finallyBlock);
      return default;
    }

    public static EcmaValue TryCatch(GeneratorDelegate tryBlock, CatchGeneratorDelegate catchBlock = null, FinallyGeneratorDelegate finallyBlock = null) {
      GeneratorDelegateEnumerator.GetCurrent().PushStack(new GeneratorDelegateEnumerator(tryBlock, catchBlock, finallyBlock));
      return default;
    }
  }
}
