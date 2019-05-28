using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ObjectConstructor {
    [Test, RuntimeFunctionInjection]
    public void Assign(RuntimeFunction assign) {
      IsUnconstructableFunctionWLength(assign, "assign", 2);
      IsAbruptedFromToObject(assign.Bind(_));
      EcmaValue result, source;

      // first argument is converted to object
      result = assign.Call(_, "a");
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo("a"));

      result = assign.Call(_, 12);
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo(12));

      result = assign.Call(_, true);
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo(true));

      // a later assignment to the same property overrides an earlier assignment
      result = assign.Call(_, CreateObject(("a", 1)), CreateObject(("a", 2)), CreateObject(("a", 3)));
      That(result["a"], Is.EqualTo(3));

      // string have own enumerable properties, so it can be wrapped to objects.
      result = assign.Call(_, 12, "aaa", "bb2b", "1c");
      That(result[0], Is.EqualTo("1"));
      That(result[1], Is.EqualTo("c"));
      That(result[2], Is.EqualTo("2"));
      That(result[3], Is.EqualTo("b"));

      // Errors thrown during retrieval of source object attributes
      source = new EcmaObject();
      DefineProperty(source, "attr", enumerable: true, get: ThrowTest262Exception.Call);
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Does not assign non-enumerable source properties
      source = new EcmaObject();
      DefineProperty(source, "attr", get: ThrowTest262Exception.Call);
      result = assign.Call(_, new EcmaObject(), source);
      That(result.Invoke("hasOwnProperty", "attr"), Is.EqualTo(false));

      // null and undefined source should be ignored
      // Number, Boolean, Symbol cannot have own enumerable properties
      result = assign.Call(_, new EcmaObject(), _, Null, 12, false, new Symbol());
      That(Object.Invoke("keys", result)["length"], Is.EqualTo(0));

      // Invoked with a source which does not have a descriptor for an own property
      ArrayList counter = new ArrayList();
      source = Proxy.Construct(
        new EcmaObject(),
        CreateObject(("ownKeys", CreateFunction(counter, () => EcmaArray.Of("missing")))));
      result = assign.Call(_, new EcmaObject(), source);
      That(counter.Count, Is.EqualTo(1), "Proxy trap was invoked exactly once");
      That(result.Invoke("hasOwnProperty", "missing"), Is.EqualTo(false));

      // Invoked with a source whose own property descriptor cannot be retrieved
      source = Proxy.Construct(
        CreateObject(("attr", Null)),
        CreateObject(("getOwnPropertyDescriptor", ThrowTest262Exception)));
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Invoked with a source whose own property keys cannot be retrieved
      source = Proxy.Construct(
        CreateObject(("attr", Null)),
        CreateObject(("ownKeys", ThrowTest262Exception)));
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Symbol-valued properties are copied after String-valued properties.
      Symbol sym1 = new Symbol();
      Symbol sym2 = new Symbol();
      ArrayList log = new ArrayList();
      source = CreateObject(
        (sym1, get: CreateFunction(log, "get sym1"), set: null),
        ("a", get: CreateFunction(log, "get a"), set: null),
        (sym2, get: CreateFunction(log, "get sym2"), set: null),
        ("b", get: CreateFunction(log, "get b"), set: null)
      );
      assign.Call(_, new EcmaObject(), source);
      That(log, Is.EquivalentTo(new[] { "get a", "get b", "get sym1", "get sym2" }));

      // Errors thrown during definition of target object attributes
      result = new EcmaObject();
      DefineProperty(result, "attr", value: 2);
      Case((_, result, CreateObject(("attr", 2))), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Create(RuntimeFunction create) {
      IsUnconstructableFunctionWLength(create, "create", 2);

      Case((_, true), Throws.TypeError);
      Case((_, ""), Throws.TypeError);
      Case((_, 2), Throws.TypeError);

      Case((_, new EcmaObject(), _), Throws.Nothing);
      Case((_, new EcmaObject(), Null), Throws.TypeError);

      // Object.create creates new Object
      Case((_, RuntimeFunction.Create(() => _).Construct()), Is.TypeOf("object"));
      That(create.Call(_, new EcmaObject()).InstanceOf(Object));

      It("should set the prototype to the passed-in object", () => {
        var proto = RuntimeFunction.Create(() => _).Construct();
        var result = create.Call(_, proto);
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(proto));
        That(proto.Invoke("isPrototypeOf", result), Is.EqualTo(true));
      });

      It("should add new properties", () => {
        var proto = RuntimeFunction.Create(() => _).Construct();
        var result = create.Call(_, proto, CreateObject(
          ("x", CreateObject(("value", true), ("writable", false))),
          ("y", CreateObject(("value", "str"), ("writable", false)))
        ));
        That(result["x"], Is.EqualTo(true));
        That(result["y"], Is.EqualTo("str"));
        That(proto["x"], Is.Undefined);
        That(proto["y"], Is.Undefined);
      });
    }
  }
}
