using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;

namespace Codeless.Ecma.UnitTest.Tests {
  public class Interop : TestBase {
    public class TestObject {
      public int Value { get; set; }
      public int ReadOnlyValue { get; } = 1;
    }

    [Test]
    public void Type() {
      EcmaValue undefined = default;
      EcmaValue nullValue = EcmaValue.Null;
      EcmaValue stringValue = "abs";
      EcmaValue intValue = 1;
      EcmaValue doubleValue = 1d;
      EcmaValue obj = new EcmaObject();
      EcmaValue symbol = new EcmaValue(Symbol.Species);
      EcmaValue numberObject = intValue.ToObject();

      That(undefined.Type, Is.EqualTo(EcmaValueType.Undefined));
      That(nullValue.Type, Is.EqualTo(EcmaValueType.Null));
      That(stringValue.Type, Is.EqualTo(EcmaValueType.String));
      That(intValue.Type, Is.EqualTo(EcmaValueType.Number));
      That(doubleValue.Type, Is.EqualTo(EcmaValueType.Number));
      That(obj.Type, Is.EqualTo(EcmaValueType.Object));
      That(symbol.Type, Is.EqualTo(EcmaValueType.Symbol));
      That(numberObject.Type, Is.EqualTo(EcmaValueType.Object));
    }

    [Test]
    public void ReflectedNativeObject() {
      TestObject obj = new TestObject();
      EcmaValue value = new EcmaValue(obj);
      value["Value"] = 1;

      That(value["Value"], Is.EqualTo(1));
      That(obj.Value, Is.EqualTo(1), "Value should be reflected on native object");
      That(() => { value["ReadOnlyValue"] = 2; }, Throws.Nothing, "Setting a readonly property does not throw exception");
      That(value["ReadOnlyValue"], Is.EqualTo(1), "Value should not be changed");

      RuntimeObject testObjectPrototype = value.ToObject().GetPrototypeOf();
      That(testObjectPrototype.GetOwnProperty("Value"), Is.Not.EqualTo(EcmaValue.Undefined), "Property should be defined on the reflected prototype object");
      That(testObjectPrototype.GetOwnProperty("Value").Get, Is.Not.EqualTo(EcmaValue.Undefined), "Property should be defined as getter/setter");
      That(testObjectPrototype.GetOwnProperty("ReadOnlyValue").Set, Is.Undefined, "Readonly property should have no setter");
    }

    [Test]
    public void NativeArrayObject() {
      EcmaValue list = new EcmaValue(new List<object> { 1, 2, 3, new EcmaObject() });

      That(list["length"], Is.EqualTo(4));
      That(list.ToString(), Is.EqualTo("1,2,3,[object Object]"));
      That(Global.Json.Invoke("stringify", list), Is.EqualTo("[1,2,3,{}]"));
    }

    [Test]
    public void NativeDictionaryObject() {
      EcmaValue ht = new EcmaValue(new Hashtable());
      ht["prop"] = 1;

      That(ht["prop"], Is.EqualTo(1));
      That(Global.Json.Invoke("stringify", ht), Is.EqualTo("{\"prop\":1}"));
    }
  }
}
