using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class IteratorPrototype : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Iterator(RuntimeFunction iterator) {
      IsUnconstructableFunctionWLength(iterator, "[Symbol.iterator]", 0);

      EcmaValue iterProto = Object.Invoke("getPrototypeOf", Object.Invoke("getPrototypeOf", Array.Construct().Invoke(Symbol.Iterator)));
      That(iterProto, Has.OwnProperty(Symbol.Iterator, iterator, EcmaPropertyAttributes.DefaultMethodProperty));

      EcmaValue thisValue = Object.Construct();
      Case(thisValue, thisValue);
    }
  }
}
