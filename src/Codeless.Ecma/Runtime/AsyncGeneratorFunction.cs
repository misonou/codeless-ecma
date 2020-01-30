using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  public class AsyncGeneratorFunction : RuntimeFunction {
    private GeneratorDelegate generator;

    public AsyncGeneratorFunction(GeneratorDelegate generator)
      : base(WellKnownObject.AsyncGenerator) {
      Init(generator);
    }

    internal void Init(GeneratorDelegate generator) {
      Guard.ArgumentNotNull(generator, "generator");
      this.generator = generator;
      this.Source = "async function* () { [native code] }";
      this.DefinePropertyOrThrow(WellKnownProperty.Prototype, new EcmaPropertyDescriptor(RuntimeObject.Create(this.Realm.GetRuntimeObject(WellKnownObject.AsyncGeneratorPrototype)), EcmaPropertyAttributes.Writable));
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return new AsyncGenerator(invocation, new GeneratorDelegateEnumerator(generator));
    }
  }
}
