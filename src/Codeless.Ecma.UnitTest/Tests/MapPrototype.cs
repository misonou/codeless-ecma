using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;

namespace Codeless.Ecma.UnitTest.Tests {
  public class MapPrototype {
    [Test]
    public void ForEach() {
      int count = 0;
      EcmaValue map = Map.Construct();
      map.Invoke("set", 1, 2);

      That(() => {
        map.Invoke("forEach", RuntimeFunction.Create(v => {
          if (map["size"] < 2) {
            map.Invoke("set", 2, 3);
          }
          count++;
        }));
      }, Throws.Nothing, "should not throw when map is modified during iteration");
      That(count, Is.EqualTo(2), "should iterate through newly added entry");
    }
  }
}
