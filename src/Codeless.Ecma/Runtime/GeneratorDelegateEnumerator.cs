using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  public delegate IEnumerable<EcmaValue> GeneratorDelegate();

  public delegate IEnumerable<EcmaValue> CatchGeneratorDelegate(EcmaValue ex);

  public delegate IEnumerable<EcmaValue> FinallyGeneratorDelegate();

  internal class GeneratorDelegateEnumerator : IGeneratorEnumerator {
    private enum State { Try, Catch, Finally, Closed }

    private readonly GeneratorDelegate tryBlock;
    private readonly CatchGeneratorDelegate catchBlock;
    private readonly FinallyGeneratorDelegate finallyBlock;
    private List<IGeneratorEnumerator> stack;
    private IGeneratorContext context;
    private IEnumerator<EcmaValue> iterator;
    private Exception exception;
    private State state;
    private bool afterYield;

    [ThreadStatic]
    private static GeneratorDelegateEnumerator current;

    public GeneratorDelegateEnumerator(GeneratorDelegate tryBlock)
      : this(tryBlock, null, null) { }

    public GeneratorDelegateEnumerator(GeneratorDelegate tryBlock, CatchGeneratorDelegate catchBlock, FinallyGeneratorDelegate finallyBlock) {
      Guard.ArgumentNotNull(tryBlock, "tryBlock");
      this.tryBlock = tryBlock;
      this.catchBlock = catchBlock;
      this.finallyBlock = finallyBlock;
    }

    public EcmaValue Current {
      get {
        IGeneratorEnumerator iterator = stack[0];
        return iterator == this ? this.iterator.Current : iterator.Current;
      }
    }

    public static GeneratorDelegateEnumerator GetCurrent() {
      if (current == null) {
        throw new InvalidOperationException();
      }
      return current;
    }

    public void PushStack(IGeneratorEnumerator other) {
      other.Init(context);
      if (other is GeneratorDelegateEnumerator e) {
        e.stack = stack;
      }
      stack.Insert(0, other);
    }

    public bool MoveNext() {
      begin:
      IGeneratorEnumerator top;
      Yield.ResumeValue = context.ResumeValue;
      while ((top = stack[0]) != this) {
        // iterate through inner generators
        // remove it from the stack if inner generator is completed
        if (top.MoveNext()) {
          return true;
        }
        Yield.ResumeValue = stack[0].Current;
        stack.RemoveAt(0);
      }
      if (state == State.Closed) {
        return false;
      }
      bool moveNext = true;
      if (afterYield) {
        switch (context.ResumeState) {
          case GeneratorResumeState.Return:
            moveNext = SetFinally();
            break;
          case GeneratorResumeState.Throw:
            moveNext = SetException(EcmaException.FromValue(context.ResumeValue));
            break;
        }
      }
      while (moveNext) {
        GeneratorDelegateEnumerator previous = current;
        iterator = iterator ?? tryBlock().GetEnumerator();
        afterYield = false;
        current = this;
        try {
          if (iterator.MoveNext()) {
            afterYield = true;
            if (stack[0] != this) {
              // new generator scope is pushed to the stack by either a yield* or try-catch block
              // ignore the yielded dummy value and begin iterate through the delegated enumerator
              goto begin;
            }
            return true;
          }
        } catch (GeneratorClosedException) {
        } catch (Exception ex) {
          moveNext = SetException(ex);
          continue;
        } finally {
          current = previous;
        }
        moveNext = SetFinally();
      }
      return false;
    }

    public bool SetException(Exception ex) {
      if (state == State.Finally) {
        throw ex;
      }
      if (state == State.Try && catchBlock != null) {
        afterYield = false;
        state = State.Catch;
        iterator.Dispose();
        iterator = catchBlock(EcmaValueUtility.GetValueFromException(ex)).GetEnumerator();
        return true;
      }
      exception = ex;
      return SetFinally();
    }

    public bool SetFinally() {
      if (finallyBlock != null && state != State.Finally) {
        afterYield = false;
        state = State.Finally;
        iterator.Dispose();
        iterator = finallyBlock().GetEnumerator();
        return true;
      }
      state = State.Closed;
      iterator.Dispose();

      if (exception != null) {
        int i = stack.IndexOf(this);
        if (i != stack.Count - 1) {
          stack[i + 1].SetException(exception);
        } else if (context.State == GeneratorState.Running) {
          throw exception;
        }
      }
      return false;
    }

    #region interface
    object IEnumerator.Current {
      get { return this.Current; }
    }

    void IGeneratorEnumerator.Init(IGeneratorContext context) {
      Guard.ArgumentNotNull(context, "context");
      this.context = context;
      this.stack = new List<IGeneratorEnumerator>() { this };
    }

    void IEnumerator.Reset() { }

    void IDisposable.Dispose() {
      if (iterator != null) {
        iterator.Dispose();
      }
    }

    IEnumerator<EcmaValue> IEnumerable<EcmaValue>.GetEnumerator() {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this;
    }
    #endregion
  }
}
