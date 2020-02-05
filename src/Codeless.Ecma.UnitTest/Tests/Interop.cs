using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class Interop : TestBase {
    public class TestObject {
      public int Value { get; set; }
      public int ReadOnlyValue { get; } = 1;

      [IntrinsicMember]
      public void MyMethod() { }
    }

    [Test]
    public void Type() {
      It("should coerce native values to correct types", () => {
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
      });
    }

    [Test]
    public void StructuredSerialize() {
      RuntimeRealm other = new RuntimeRealm();

      It("should clone object with own non-symbol properties", () => {
        EcmaValue obj = new EcmaObject();
        obj["boolean"] = true;
        obj["number"] = 1;
        obj["string"] = "";
        obj["circular"] = obj;
        obj["booleanObj"] = Boolean.Construct(false);
        obj["numberObj"] = Number.Construct(0);
        obj["stringObj"] = String.Construct("");
        obj[new Symbol()] = "foo";
        Object.Invoke("defineProperty", obj, "foo", CreateObject(new { enumerable = true, value = 1 }));
        Object.Invoke("defineProperty", obj, "bar", CreateObject(new { enumerable = true, get = FunctionLiteral(() => 0) }));
        Object.Invoke("defineProperty", obj, "baz", CreateObject(new { enumerable = false, value = 1 }));

        other.Execute(() => {
          EcmaValue clone = obj.ToObject().Clone(other);
          That(clone != obj);
          That(Object.Invoke("getPrototypeOf", clone), Is.EqualTo(Object.Prototype), "prototype is derived from target realm");

          That(clone["boolean"], Is.EqualTo(true));
          That(clone["number"], Is.EqualTo(1));
          That(clone["string"], Is.EqualTo(""));

          That(clone["booleanObj"], Is.InstanceOf(Boolean), "Boolean object is copied");
          That(clone["numberObj"], Is.InstanceOf(Number), "Number object is copied");
          That(clone["stringObj"], Is.InstanceOf(String), "String object is copied");

          That(clone["circular"], Is.EqualTo(clone), "circular reference is preserved");
          That(clone, Has.OwnProperty("foo", 1, EcmaPropertyAttributes.DefaultDataProperty), "non-writable and non-configurable flag is not copied");
          That(clone, Has.OwnProperty("bar", 0, EcmaPropertyAttributes.DefaultDataProperty), "accessor is copied as data");
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "baz"), Is.Undefined, "non-enumerable property is not copied");
          That(Object.Invoke("getOwnPropertySymbols", clone)["length"], Is.Zero, "symbol property is not copied");
        });
      });

      It("should not clone prototype chain", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue customProto = Object.Invoke("create", obj);
        obj["foo"] = "bar";

        Assume.That(Object.Invoke("getPrototypeOf", customProto), Is.EqualTo(obj));
        Assume.That(customProto["foo"], Is.EqualTo("bar"));

        other.Execute(() => {
          EcmaValue clone = customProto.ToObject().Clone(other);
          That(clone != customProto);
          That(Object.Invoke("getPrototypeOf", clone), Is.EqualTo(Object.Prototype), "prototype chain is ignored");
          That(Object.Invoke("getOwnPropertyNames", clone)["length"], Is.Zero, "inherited properties are not copied");
          That(clone["foo"], Is.Undefined);
        });
      });

      It("should throw if property values are not cloneable", () => {
        EcmaValue poisoned = new EcmaObject();
        poisoned["foo"] = new Symbol();
        other.Execute(() => {
          That(() => poisoned.ToObject().Clone(other), Throws.Exception, "symbol value causes exception");
        });
      });

      It("should shallow clone primitive objects", () => {
        EcmaValue boolObj = Boolean.Construct(false);
        EcmaValue numObj = Number.Construct(0);
        EcmaValue strObj = String.Construct("");

        boolObj["foo"] = 1;
        numObj["foo"] = 1;
        strObj["foo"] = 1;

        Assume.That(Object.Invoke("getOwnPropertyNames", boolObj)["length"], Is.EqualTo(1));
        Assume.That(Object.Invoke("getOwnPropertyNames", numObj)["length"], Is.EqualTo(1));
        Assume.That(Object.Invoke("getOwnPropertyNames", strObj)["length"], Is.EqualTo(1));

        other.Execute(() => {
          EcmaValue cboolObj = boolObj.ToObject().Clone(other);
          That(cboolObj, Is.TypeOf("object"));
          That(Object.Invoke("getOwnPropertyNames", cboolObj)["length"], Is.Zero);

          EcmaValue cnumObj = numObj.ToObject().Clone(other);
          That(cnumObj, Is.TypeOf("object"));
          That(Object.Invoke("getOwnPropertyNames", cnumObj)["length"], Is.Zero);

          EcmaValue cstrObj = strObj.ToObject().Clone(other);
          That(cstrObj, Is.TypeOf("object"));
          That(Object.Invoke("getOwnPropertyNames", cstrObj)["length"], Is.Zero);
        });
      });

      It("should shallow clone Date object", () => {
        EcmaValue date = Date.Construct(2020, 0, 1);
        EcmaValue ts = +date;
        date["foo"] = 1;

        other.Execute(() => {
          EcmaValue clone = date.ToObject().Clone(other);
          That(clone, Is.InstanceOf(Date));
          That(+clone, Is.EqualTo(ts));
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "foo"), Is.Undefined);
        });
      });

      It("should deeply clone [[MapData]]", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", map, map);
        map.Invoke("set", "bar", "baz");
        map["foo"] = 1;

        other.Execute(() => {
          EcmaValue clone = map.ToObject().Clone(other);
          That(clone, Is.InstanceOf(Map));
          That(clone.Invoke("get", clone), Is.EqualTo(clone));
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "foo"), Is.Undefined, "own property is not copied");

          clone.Invoke("set", map, 1);
          clone.Invoke("delete", "bar");
          clone.Invoke("set", "baz", true);
        });
        That(map.Invoke("get", map), Is.EqualTo(map), "original value is unchanged");
        That(map.Invoke("get", "bar"), Is.EqualTo("baz"), "original entry is not deleted");
        That(map.Invoke("has", "baz"), Is.False, "key added in clone is not added in original map");
      });

      It("should deeply clone [[SetData]]", () => {
        EcmaValue set = Set.Construct();
        set.Invoke("add", set);
        set.Invoke("add", true);
        set["foo"] = 1;

        other.Execute(() => {
          EcmaValue clone = set.ToObject().Clone(other);
          That(clone, Is.InstanceOf(Set));
          That(clone.Invoke("has", clone), Is.True);
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "foo"), Is.Undefined, "own property is not copied");

          clone.Invoke("delete", true);
          clone.Invoke("add", false);
        });
        That(set.Invoke("has", true), Is.True);
        That(set.Invoke("has", false), Is.False);
      });

      It("should clone [[ArrayBufferData]] with same byte contents", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(4);
        EcmaValue view = Global.Uint8Array.Construct(buffer);
        view[1] = 1;
        view[2] = 2;
        view[3] = 3;
        buffer["foo"] = 1;

        other.Execute(() => {
          EcmaValue clone = buffer.ToObject().Clone(other);
          EcmaValue cloneView = Global.Uint8Array.Construct(clone);
          That(cloneView, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "foo"), Is.Undefined, "own property is not copied");
          cloneView[0] = 42;
        });
        That(view, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
      });

      It("should clone [[ArrayBufferData]] with same data block if it is shared", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(4);
        EcmaValue view = Global.Uint8Array.Construct(buffer);
        view[1] = 1;
        view[2] = 2;
        view[3] = 3;
        buffer["foo"] = 1;

        other.Execute(() => {
          EcmaValue clone = buffer.ToObject().Clone(other);
          EcmaValue cloneView = Global.Uint8Array.Construct(clone);
          That(cloneView, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
          That(Object.Invoke("getOwnPropertyDescriptor", clone, "foo"), Is.Undefined, "own property is not copied");
          cloneView[0] = 42;
        });
        That(view, Is.EquivalentTo(new[] { 42, 1, 2, 3 }));
      });

      It("should clone [[ViewedArrayBuffer]] with same byte contents", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(4);
        EcmaValue view = Global.Uint8Array.Construct(buffer);
        view[1] = 1;
        view[2] = 2;
        view[3] = 3;
        view["foo"] = 1;

        other.Execute(() => {
          EcmaValue cloneView = view.ToObject().Clone(other);
          That(cloneView, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
          That(Object.Invoke("getOwnPropertyDescriptor", cloneView, "foo"), Is.Undefined, "own property is not copied");
          cloneView[0] = 42;
        });
        That(view, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
      });

      It("should clone [[ViewedArrayBuffer]] with same data block if it is shared", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(4);
        EcmaValue view = Global.Uint8Array.Construct(buffer);
        view[1] = 1;
        view[2] = 2;
        view[3] = 3;
        view["foo"] = 1;

        other.Execute(() => {
          EcmaValue cloneView = view.ToObject().Clone(other);
          That(cloneView, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
          That(Object.Invoke("getOwnPropertyDescriptor", cloneView, "foo"), Is.Undefined, "own property is not copied");
          cloneView[0] = 42;
        });
        That(view, Is.EquivalentTo(new[] { 42, 1, 2, 3 }));
      });

      It("should throw if object is callable", () => {
        EcmaValue fn = FunctionLiteral(Noop);
        That(() => fn.ToObject().Clone(other), Throws.Exception);
      });

      It("should throw if object has other internal slots", () => {
        EcmaValue symObj = Object.Construct(new Symbol());
        That(() => symObj.ToObject().Clone(other), Throws.Exception);
      });

      It("should detach array buffer if it is in the transfer list", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(4);
        EcmaValue view = Global.Uint8Array.Construct(buffer);
        view[1] = 1;
        view[2] = 2;
        view[3] = 3;

        other.Execute(() => {
          EcmaValue clone = buffer.ToObject().Clone(other, buffer.ToObject());
          EcmaValue cloneView = Global.Uint8Array.Construct(clone);
          That(cloneView, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
        });
        That(() => view[1], Throws.TypeError);
      });

      It("should throw if object in transfer list is not transferable", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(4);
        That(() => buffer.ToObject().Clone(other, new EcmaObject()), Throws.Exception);
      });

      It("should throw if array buffer in transfer list is detached", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(4);
        EcmaValue detached = Global.ArrayBuffer.Construct(4);
        DetachBuffer(detached);
        That(() => buffer.ToObject().Clone(other, detached.ToObject()), Throws.Exception);
      });
    }

    [Test]
    public void ObjectHosting() {
      It("should return different instances on different execution thread", () => {
        TestObject obj = new TestObject();
        RuntimeObject v0 = RuntimeRealm.Current.GetRuntimeObject(obj);

        RuntimeExecution.CreateWorkerThread(parentRealm => {
          Assume.That(parentRealm, Is.Not.EqualTo(RuntimeRealm.Current), "realm should be different");

          RuntimeObject v1 = RuntimeRealm.Current.GetRuntimeObject(obj);
          That(v0 != v1, "returned object should be different");
          That(v0.Realm, Is.EqualTo(parentRealm));
          That(v1.Realm, Is.EqualTo(RuntimeRealm.Current));

          That(v0.GetPrototypeOf() != v1.GetPrototypeOf(), "the prototype of returned object should be different");
          That(v0.GetPrototypeOf().Realm, Is.EqualTo(parentRealm));
          That(v1.GetPrototypeOf().Realm, Is.EqualTo(RuntimeRealm.Current));

          That(v0.GetPrototypeOf().GetPrototypeOf() != v1.GetPrototypeOf().GetPrototypeOf(), "all prototype ancestors of returned object should be different");

          That(v0["MyMethod"], Is.TypeOf("function"));
          That(v0["MyMethod"], Is.Not.EqualTo(v1["MyMethod"]), "property value of object type should be different");
        }, true).Thread.Join();
      });

      It("should return different instances on different realm of the same execution thread", () => {
        TestObject obj = new TestObject();
        RuntimeObject v0 = RuntimeRealm.Current.GetRuntimeObject(obj);
        RuntimeRealm parentRealm = RuntimeRealm.Current;
        RuntimeRealm other = new RuntimeRealm();

        other.Enqueue(() => {
          Assume.That(RuntimeRealm.Current, Is.Not.EqualTo(parentRealm), "realm should be different");
          Assume.That(RuntimeRealm.Current, Is.EqualTo(other));

          RuntimeObject v1 = RuntimeRealm.Current.GetRuntimeObject(obj);
          That(v0 != v1, "returned object should be different");
          That(v0.Realm, Is.EqualTo(parentRealm));
          That(v1.Realm, Is.EqualTo(RuntimeRealm.Current));

          That(v0.GetPrototypeOf() != v1.GetPrototypeOf(), "the prototype of returned object should be different");
          That(v0.GetPrototypeOf().Realm, Is.EqualTo(parentRealm));
          That(v1.GetPrototypeOf().Realm, Is.EqualTo(RuntimeRealm.Current));

          That(v0.GetPrototypeOf().GetPrototypeOf() != v1.GetPrototypeOf().GetPrototypeOf(), "all prototype ancestors of returned object should be different");

          That(v0["MyMethod"], Is.TypeOf("function"));
          That(v0["MyMethod"], Is.Not.EqualTo(v1["MyMethod"]), "property value of object type should be different");
        });
        RuntimeExecution.ContinueUntilEnd();
      });

      It("should expose native properties as getters and setters on protoype", () => {
        TestObject obj = new TestObject();
        EcmaValue value = new EcmaValue(obj);
        value["Value"] = 1;

        That(value["Value"], Is.EqualTo(1));
        That(obj.Value, Is.EqualTo(1), "Value should be reflected on native object");
        That(() => value["ReadOnlyValue"] = 2, Throws.Nothing, "Setting a readonly property does not throw exception");
        That(value["ReadOnlyValue"], Is.EqualTo(1), "Value should not be changed");

        RuntimeObject testObjectPrototype = value.ToObject().GetPrototypeOf();
        That(testObjectPrototype.GetOwnProperty("Value"), Is.Not.EqualTo(EcmaValue.Undefined), "Property should be defined on the reflected prototype object");
        That(testObjectPrototype.GetOwnProperty("Value").Get, Is.Not.EqualTo(EcmaValue.Undefined), "Property should be defined as getter/setter");
        That(testObjectPrototype.GetOwnProperty("ReadOnlyValue").Set, Is.Undefined, "Readonly property should have no setter");
      });

      It("should expose IList as an Array exotic object", () => {
        EcmaValue list = new EcmaValue(new List<object> { 1, 2, 3, new EcmaObject() });
        That(list["length"], Is.EqualTo(4));
        That(list.ToString(), Is.EqualTo("1,2,3,[object Object]"));
        That(Global.Json.Invoke("stringify", list), Is.EqualTo("[1,2,3,{}]"));
      });

      It("should expose IDictionary as an ordinary object for valid EcmaPropertyKey", () => {
        Hashtable ht = new Hashtable();
        EcmaValue obj = new EcmaValue(ht);
        obj["prop"] = 1;
        That(obj["prop"], Is.EqualTo(1));
        That(Global.Json.Invoke("stringify", obj), Is.EqualTo("{\"prop\":1}"));
      });
    }
  }
}
