using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ProxyConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Proxy", 2, null);
      That(GlobalThis, Has.OwnProperty("Proxy", ctor, EcmaPropertyAttributes.DefaultMethodProperty));

      That(TypeOf(ctor.Construct(Object.Construct(), Object.Construct())), Is.EqualTo("object"));
      That(() => ctor.Call(Object.Construct(), Object.Construct()), Throws.TypeError);

      EcmaValue revocable = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
      revocable.Invoke("revoke");

      That(() => ctor.Construct(Object.Construct(), revocable["proxy"]), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), Undefined), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), Null), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), 0), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), false), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), ""), Throws.TypeError);
      That(() => ctor.Construct(Object.Construct(), new Symbol()), Throws.TypeError);

      That(() => ctor.Construct(revocable["proxy"], Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct(Undefined, Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct(Null, Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct(0, Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct(false, Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct("", Object.Construct()), Throws.TypeError);
      That(() => ctor.Construct(new Symbol(), Object.Construct()), Throws.TypeError);

      // A Proxy exotic object is only callable if the given target is callable.
      EcmaValue p1 = Proxy.Construct(Object.Construct(), Object.Construct());
      That(() => p1.Call(_), Throws.TypeError);

      // A Proxy exotic object only accepts a constructor call if target is constructor.
      EcmaValue p2 = Proxy.Construct(GlobalThis["parseInt"], Object.Construct());
      That(() => p2.Call(_), Throws.Nothing);
      That(() => p2.Construct(_), Throws.TypeError);

      // The realm of a proxy exotic object is the realm of its target function
      RuntimeRealm realm = new RuntimeRealm();
      EcmaValue C = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
      C["prototype"] = Null;
      EcmaValue P = Proxy.Construct(C, Object.Construct());
      That(Object.Invoke("getPrototypeOf", P.Construct()), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ObjectPrototype)), "The realm of a proxy exotic object is the realm of its target function");

      P = Proxy.Construct(P, Object.Construct());
      That(Object.Invoke("getPrototypeOf", P.Construct()), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ObjectPrototype)), "GetFunctionRealm is called recursively");
    }

    [Test, RuntimeFunctionInjection]
    public void Revocable(RuntimeFunction revocable) {
      IsUnconstructableFunctionWLength(revocable, "revocable", 2);
      That(Proxy, Has.OwnProperty("revocable", revocable, EcmaPropertyAttributes.DefaultMethodProperty));

      It("The returned object has a proxy property which is the created Proxy object built with the given target and handler given parameters", () => {
        EcmaValue target = CreateObject(new { attr = "foo" });
        EcmaValue r = revocable.Call(_, target, CreateObject(new { get = RuntimeFunction.Create((t, prop) => t[prop] + "!") }));
        That(r["proxy"]["attr"], Is.EqualTo("foo!"));
      });

      It("The property of Proxy Revocation functions", () => {
        EcmaValue revocationFunction = revocable.Call(_, Object.Construct(), Object.Construct())["revoke"];
        IsUnconstructableFunctionWLength(revocationFunction, null, 0);
        That(Object.Invoke("isExtensible", revocationFunction), Is.EqualTo(true));

        EcmaValue r = revocable.Call(_, Object.Construct(), Object.Construct());
        That(r.Invoke("revoke"), Is.Undefined);
        That(r.Invoke("revoke"), Is.Undefined);
      });
    }
  }
}
