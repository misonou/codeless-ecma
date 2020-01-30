using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AsyncIteratorPrototype : TestBase {
    [Test, RuntimeFunctionInjection]
    public void AsyncIterator(RuntimeFunction asyncIterator) {
      IsUnconstructableFunctionWLength(asyncIterator, "[Symbol.asyncIterator]", 0);

      AsyncGeneratorFunction generator = new AsyncGeneratorFunction(EmptyGenerator);
      EcmaValue iterProto = Object.Invoke("getPrototypeOf", Object.Invoke("getPrototypeOf", generator.Prototype));
      That(iterProto, Has.OwnProperty(Symbol.AsyncIterator, asyncIterator, EcmaPropertyAttributes.DefaultMethodProperty));

      EcmaValue thisValue = Object.Construct();
      Case(thisValue, thisValue);
    }
  }
}
