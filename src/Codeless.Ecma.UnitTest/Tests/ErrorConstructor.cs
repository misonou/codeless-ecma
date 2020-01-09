using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ErrorConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Error", 1, Error.Prototype);
      IsAbruptedFromToPrimitive(ctor.Bind(_));

      That(ctor.Call(), Is.InstanceOf(ctor));
      That(Object.Prototype["toString"].Call(ctor.Construct()), Is.EqualTo("[object Error]"));

      It("should coerce first argument to string", () => {
        That(ctor.Construct(Null)["message"], Is.EqualTo("null"));
        That(ctor.Construct(0)["message"], Is.EqualTo("0"));
        That(ctor.Construct(true)["message"], Is.EqualTo("true"));
        That(ctor.Construct(Object.Construct())["message"], Is.EqualTo("[object Object]"));
        That(ctor.Construct(CreateObject(toString: () => "foo"))["message"], Is.EqualTo("foo"));
        That(ctor.Construct(CreateObject(toString: () => Object.Construct(), valueOf: () => 1))["message"], Is.EqualTo("1"));
        That(ctor.Construct(CreateObject(toPrimitive: () => "foo"))["message"], Is.EqualTo("foo"));

        That(() => ctor.Construct(new Symbol()), Throws.TypeError);
        That(() => ctor.Construct(CreateObject(toString: () => Object.Construct(), valueOf: () => Object.Construct())), Throws.TypeError);
      });

      It("should define own message property if first argument is not undefined", () => {
        That(ctor.Call(_, "msg1"), Has.OwnProperty("message", "msg1", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
        That(ctor.Construct("msg1"), Has.OwnProperty("message", "msg1", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

        That(ctor.Construct().HasOwnProperty("message"), Is.EqualTo(false));
        That(ctor.Construct(Undefined).HasOwnProperty("message"), Is.EqualTo(false));
        That(ctor.Construct(Null).HasOwnProperty("message"), Is.EqualTo(true));
        That(ctor.Construct("").HasOwnProperty("message"), Is.EqualTo(true));
      });

      It("should define own stack property", () => {
        That(ctor.Construct(), Has.OwnProperty("stack", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
        That(ctor.Construct()["stack"], Is.Not.EqualTo(Undefined));
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ErrorPrototype)));
      });
    }

    [Test]
    public void DerivedConstructor() {
      foreach (string derived in new[] { "EvalError", "RangeError", "ReferenceError", "SyntaxError", "TypeError", "URIError" }) {
        That(GlobalThis[derived], Is.TypeOf("function"));

        RuntimeFunction ctor = (RuntimeFunction)GlobalThis[derived].ToObject();
        WellKnownObject protoType = (WellKnownObject)System.Enum.Parse(typeof(WellKnownObject), derived + "Prototype", true);
        IsConstructorWLength(ctor, derived, 1, ctor.Realm.GetRuntimeObject(protoType), Error);
        IsAbruptedFromToPrimitive(ctor.Bind(_));

        That(ctor.Call(), Is.InstanceOf(ctor));
        That(Object.Prototype["toString"].Call(ctor.Construct()), Is.EqualTo("[object Error]"));

        It("should coerce first argument to string", () => {
          That(ctor.Construct(Null)["message"], Is.EqualTo("null"));
          That(ctor.Construct(0)["message"], Is.EqualTo("0"));
          That(ctor.Construct(true)["message"], Is.EqualTo("true"));
          That(ctor.Construct(Object.Construct())["message"], Is.EqualTo("[object Object]"));
          That(ctor.Construct(CreateObject(toString: () => "foo"))["message"], Is.EqualTo("foo"));
          That(ctor.Construct(CreateObject(toString: () => Object.Construct(), valueOf: () => 1))["message"], Is.EqualTo("1"));
          That(ctor.Construct(CreateObject(toPrimitive: () => "foo"))["message"], Is.EqualTo("foo"));

          That(() => ctor.Construct(new Symbol()), Throws.TypeError);
          That(() => ctor.Construct(CreateObject(toString: () => Object.Construct(), valueOf: () => Object.Construct())), Throws.TypeError);
        });

        It("should define own message property if first argument is not undefined", () => {
          That(ctor.Call(_, "msg1")["message"], Is.EqualTo("msg1"));
          That(ctor.Construct("msg1")["message"], Is.EqualTo("msg1"));

          That(ctor.Construct().HasOwnProperty("message"), Is.EqualTo(false));
          That(ctor.Construct(Undefined).HasOwnProperty("message"), Is.EqualTo(false));
          That(ctor.Construct(Null).HasOwnProperty("message"), Is.EqualTo(true));
          That(ctor.Construct("").HasOwnProperty("message"), Is.EqualTo(true));
        });

        It("should define own stack property", () => {
          That(ctor.Construct(), Has.OwnProperty("stack", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
          That(ctor.Construct()["stack"], Is.Not.EqualTo(Undefined));
        });

        It("should derive [[Prototype]] value from realm of newTarget", () => {
          RuntimeRealm realm = new RuntimeRealm();
          EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
          fn["prototype"] = Null;
          EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(protoType)));
        });
      }
    }
  }
}
