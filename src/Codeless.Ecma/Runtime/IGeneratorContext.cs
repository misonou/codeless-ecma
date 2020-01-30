using System;

namespace Codeless.Ecma.Runtime {
  public enum GeneratorResumeState {
    Resume,
    Return,
    Throw
  }

  public enum GeneratorState {
    SuspendedStart,
    SuspendedYield,
    Running,
    Closed,
    AwaitingReturn
  }

  public enum GeneratorKind {
    Sync,
    Async
  }

  public interface IGeneratorContext {
    GeneratorKind Kind { get; }
    GeneratorState State { get; }
    GeneratorResumeState ResumeState { get; }
    EcmaValue ResumeValue { get; }
    void Return(EcmaValue value);
  }
}
