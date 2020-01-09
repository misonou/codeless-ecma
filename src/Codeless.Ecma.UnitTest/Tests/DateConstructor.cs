using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class DateConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Date", 7, Date.Prototype);

      It("should get [[DateValue]] from Date objects without calling object's method", () => {
        EcmaValue date = Date.Construct(1438560000000);
        date["toString"] = ThrowTest262Exception;
        date["valueOf"] = ThrowTest262Exception;
        date["getTime"] = ThrowTest262Exception;
        That(Date.Construct(date).Invoke("getTime"), Is.EqualTo(1438560000000));
      });

      It("should invoke @@toPrimitive and coerce returned value", () => {
        That(Date.Construct(CreateObject(toPrimitive: () => 8)).Invoke("getTime"), Is.EqualTo(8));
        That(Date.Construct(CreateObject(toPrimitive: () => Undefined)).Invoke("getTime"), Is.EqualTo(NaN));
        That(Date.Construct(CreateObject(toPrimitive: () => true)).Invoke("getTime"), Is.EqualTo(1));
        That(Date.Construct(CreateObject(toPrimitive: () => false)).Invoke("getTime"), Is.EqualTo(0));
        That(Date.Construct(CreateObject(toPrimitive: () => Null)).Invoke("getTime"), Is.EqualTo(0));
        That(Date.Construct(CreateObject(toPrimitive: () => "2016-06-05T18:40:00.000Z")).Invoke("getTime"), Is.EqualTo(1465152000000));

        That(() => Date.Construct(CreateObject(toPrimitive: () => new Symbol())), Throws.TypeError);
        That(() => Date.Construct(CreateObject(toPrimitive: () => new EcmaObject())), Throws.TypeError);
        That(() => Date.Construct(CreateObject(toPrimitive: () => new EcmaArray())), Throws.TypeError);
        That(() => Date.Construct(CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262);
      });

      It("should coerce input value in the correct order", () => {
        Logs.Clear();
        Date.Construct(Undefined,
          CreateObject(toString: Intercept(() => 0, "year")),
          CreateObject(toString: Intercept(() => 0, "month")),
          CreateObject(toString: Intercept(() => 0, "date")),
          CreateObject(toString: Intercept(() => 0, "hours")),
          CreateObject(toString: Intercept(() => 0, "minutes")),
          CreateObject(toString: Intercept(() => 0, "seconds")),
          CreateObject(toString: Intercept(() => 0, "ms")));
        CollectionAssert.AreEqual(new[] { "year", "month", "date", "hours", "minutes", "seconds", "ms" }, Logs);
      });

      It("should return a string representing the current time if Date is called as a function", () => {
        That(Date.Call(Date), Is.TypeOf("string"));
        That(Date.Call(Date, 1), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1, 1), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1, 1, 1), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1, 1, 1, 0), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1, 1, 1, 0, 0), Is.TypeOf("string"));
        That(Date.Call(Date, 1970, 1, 1, 1, 0, 0, 0), Is.TypeOf("string"));
        That(Date.Call(Date, NaN), Is.TypeOf("string"));
        That(Date.Call(Date, Infinity), Is.TypeOf("string"));
        That(Date.Call(Date, -Infinity), Is.TypeOf("string"));
        That(Date.Call(Date, Undefined), Is.TypeOf("string"));
        That(Date.Call(Date, Null), Is.TypeOf("string"));

        That(Date.Construct() - Date.Construct(Date.Call(Date)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1, 1)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1, 1, 1)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1, 1, 1, 0)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1, 1, 1, 0, 0)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, 1970, 1, 1, 1, 0, 0, 0)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, NaN)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, Infinity)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, -Infinity)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, Undefined)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
        That(Date.Construct() - Date.Construct(Date.Call(Date, Null)).Invoke("getTime"), Is.AtLeast(-1000).And.AtMost(1000));
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.DatePrototype)));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Now(RuntimeFunction now) {
      IsUnconstructableFunctionWLength(now, "now", 0);

      That(now.Call(), Is.TypeOf("number"));
    }

    [Test, RuntimeFunctionInjection]
    public void Parse(RuntimeFunction parse) {
      IsUnconstructableFunctionWLength(parse, "parse", 1);

      It("should return value limited to specified time value maximum range", () => {
        EcmaValue minDateStr = "-271821-04-20T00:00:00.000Z";
        EcmaValue minDate = Date.Construct(-8640000000000000);
        That(minDate.Invoke("toISOString"), Is.EqualTo(minDateStr), "minDateStr");
        That(parse.Call(_, minDateStr), Is.EqualTo(minDate.Invoke("valueOf")), "parse minDateStr");

        EcmaValue maxDateStr = "+275760-09-13T00:00:00.000Z";
        EcmaValue maxDate = Date.Construct(8640000000000000);
        That(maxDate.Invoke("toISOString"), Is.EqualTo(maxDateStr), "maxDateStr");
        That(parse.Call(_, maxDateStr), Is.EqualTo(maxDate.Invoke("valueOf")), "parse maxDateStr");

        EcmaValue belowRange = "-271821-04-19T23:59:59.999Z";
        EcmaValue aboveRange = "+275760-09-13T00:00:00.001Z";
        Case((_, belowRange), NaN, "parse below minimum time value");
        Case((_, aboveRange), NaN, "parse above maximum time value");
      });

      It("should return the same initial value if milliseconds ammount is zero", () => {
        EcmaValue zero = Date.Construct(0);
        Case((_, zero.Invoke("toString")), zero.Invoke("valueOf"));
        Case((_, zero.Invoke("toUTCString")), zero.Invoke("valueOf"));
        Case((_, zero.Invoke("toISOString")), zero.Invoke("valueOf"));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void UTC(RuntimeFunction utc) {
      IsUnconstructableFunctionWLength(utc, "UTC", 7);

      It("should return abrupt completions from coercing input values", () => {
        int callCount = 0;
        var thrower = CreateObject(toString: ThrowTest262Exception);
        var counter = CreateObject(toString: () => callCount += 1);

        Case((_, thrower, counter), Throws.Test262);
        Case((_, 0, thrower, counter), Throws.Test262);
        Case((_, 0, 0, thrower, counter), Throws.Test262);
        Case((_, 0, 0, 1, thrower, counter), Throws.Test262);
        Case((_, 0, 0, 1, 0, thrower, counter), Throws.Test262);
        Case((_, 0, 0, 1, 0, 0, thrower, counter), Throws.Test262);
      });

      It("should coerce input value in the correct order", () => {
        Logs.Clear();
        utc.Call(Undefined,
          CreateObject(toString: Intercept(() => 0, "year")),
          CreateObject(toString: Intercept(() => 0, "month")),
          CreateObject(toString: Intercept(() => 0, "date")),
          CreateObject(toString: Intercept(() => 0, "hours")),
          CreateObject(toString: Intercept(() => 0, "minutes")),
          CreateObject(toString: Intercept(() => 0, "seconds")),
          CreateObject(toString: Intercept(() => 0, "ms")));
        CollectionAssert.AreEqual(new[] { "year", "month", "date", "hours", "minutes", "seconds", "ms" }, Logs);
      });

      It("should produce NaN with Inifinite or NaN values", () => {
        Case((_, Infinity), NaN);
        Case((_, -Infinity), NaN);
        Case((_, Infinity, 0), NaN);
        Case((_, -Infinity, 0), NaN);
        Case((_, 0, Infinity), NaN);
        Case((_, 0, -Infinity), NaN);
        Case((_, 0, 0, Infinity), NaN);
        Case((_, 0, 0, -Infinity), NaN);
        Case((_, 0, 0, 1, Infinity), NaN);
        Case((_, 0, 0, 1, -Infinity), NaN);
        Case((_, 0, 0, 1, 0, Infinity), NaN);
        Case((_, 0, 0, 1, 0, -Infinity), NaN);
        Case((_, 0, 0, 1, 0, 0, Infinity), NaN);
        Case((_, 0, 0, 1, 0, 0, -Infinity), NaN);
        Case((_, 0, 0, 1, 0, 0, 0, Infinity), NaN);
        Case((_, 0, 0, 1, 0, 0, 0, -Infinity), NaN);

        Case((_, NaN), NaN, "year");
        Case((_, NaN, 0), NaN, "year");
        Case((_, 1970, NaN), NaN, "month");
        Case((_, 1970, 0, NaN), NaN, "date");
        Case((_, 1970, 0, 1, NaN), NaN, "hours");
        Case((_, 1970, 0, 1, 0, NaN), NaN, "minutes");
        Case((_, 1970, 0, 1, 0, 0, NaN), NaN, "seconds");
        Case((_, 1970, 0, 1, 0, 0, 0, NaN), NaN, "ms");

        Case(_, NaN, "missing non-optional year argument");
      });

      It("should convert non-integer values to integers", () => {
        Case((_, 1970.9, 0.9, 1.9, 0.9, 0.9, 0.9, 0.9), 0);
        Case((_, -1970.9, -0.9, -0.9, -0.9, -0.9, -0.9, -0.9), -124334438400000);
      });

      It("should correct values that exceed their calendar or time boundaries", () => {
        Case((_, 2016, 12), 1483228800000, "month: 12");
        Case((_, 2016, 13), 1485907200000, "month: 13");
        Case((_, 2016, 144), 1830297600000, "month: 144");
        Case((_, 2016, 0, 33), 1454371200000, "day greater than month");
        Case((_, 2016, 2, -27), 1454371200000, "day negative value");

        Case((_, 2016, 6, 5, -1), 1467673200000, "hour: -1");
        Case((_, 2016, 6, 5, 24), 1467763200000, "hour: 24");
        Case((_, 2016, 6, 5, 0, -1), 1467676740000, "minute: -1");
        Case((_, 2016, 6, 5, 0, 60), 1467680400000, "minute: 60");
        Case((_, 2016, 6, 5, 0, 0, -1), 1467676799000, "second: -1");
        Case((_, 2016, 6, 5, 0, 0, 60), 1467676860000, "second: 60");
        Case((_, 2016, 6, 5, 0, 0, 0, -1), 1467676799999, "millisecond: -1");
        Case((_, 2016, 6, 5, 0, 0, 0, 1000), 1467676801000, "millisecond: 1000");
      });

      It("should return values representing the given time", () => {
        Case((_, 1970), 0, "1970");
        Case((_, 1970, 0), 0, "1970, 0");
        Case((_, 2016, 0), 1451606400000, "2016, 0");
        Case((_, 2016, 6), 1467331200000, "2016, 6");
        Case((_, 2016, 6, 1), 1467331200000, "2016, 6, 1");
        Case((_, 2016, 6, 5), 1467676800000, "2016, 6, 5");
        Case((_, 2016, 6, 5, 0), 1467676800000, "2016, 6, 5, 0");
        Case((_, 2016, 6, 5, 15), 1467730800000, "2016, 6, 5, 15");
        Case((_, 2016, 6, 5, 15, 0), 1467730800000, "2016, 6, 5, 15, 0");
        Case((_, 2016, 6, 5, 15, 34), 1467732840000, "2016, 6, 5, 15, 34");
        Case((_, 2016, 6, 5, 15, 34, 0), 1467732840000, "2016, 6, 5, 15, 34, 0");
        Case((_, 2016, 6, 5, 15, 34, 45), 1467732885000, "2016, 6, 5, 15, 34, 45");
        Case((_, 2016, 6, 5, 15, 34, 45, 0), 1467732885000, "2016, 6, 5, 15, 34, 45, 0");
        Case((_, 2016, 6, 5, 15, 34, 45, 876), 1467732885876, "2016, 6, 5, 15, 34, 45, 0");
      });

      It("should return NaN if time is not finite or abs(time) > 8.64E15", () => {
        Case((_, 275760, 8, 13, 0, 0, 0, 0), 8640000000000000);
        Case((_, 275760, 8, 13, 0, 0, 0, 1), NaN);
      });

      It("should offset provided year value conditionally", () => {
        Case((_, -1, 0), -62198755200000, "-1 (no offset)");
        Case((_, 0, 0), -2208988800000, "+0");
        Case((_, -0, 0), -2208988800000, "-0");
        Case((_, -0.999999, 0), -2208988800000, "-0.999999");
        Case((_, 70, 0), 0, "70");
        Case((_, 70.999999, 0), 0, "70.999999");
        Case((_, 99, 0), 915148800000, "99");
        Case((_, 99.999999, 0), 915148800000, "99.999999");
        Case((_, 100, 0), -59011459200000, "100 (no offset)");
      });
    }
  }
}
