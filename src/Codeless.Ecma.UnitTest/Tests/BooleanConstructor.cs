using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class BooleanConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Boolean", 1, Boolean.Prototype);

      It("should convert undefined and null to false", () => {
        Case((_, Undefined), false);
        Case((_, Null), false);
        Case(_, false);
      });

      It("should return boolean value", () => {
        Case((_, true), true);
        Case((_, false), false);
      });

      It("should convert +0, -0 and NaN to false", () => {
        Case((_, +0d), false);
        Case((_, -0d), false);
        Case((_, NaN), false);
      });

      It("should convert non-zero finite number and Infinity to true", () => {
        Case((_, +Infinity), true);
        Case((_, -Infinity), true);
        Case((_, Number.Get("MAX_VALUE")), true);
        Case((_, Number.Get("MIN_VALUE")), true);
        Case((_, 13), true);
        Case((_, -13), true);
        Case((_, 1.3), true);
        Case((_, -1.3), true);
      });

      It("should convert string with length = 0 to false", () => {
        Case((_, ""), false);
      });

      It("should convert string with length > 0 to true", () => {
        Case((_, " "), true);
        Case((_, "true"), true);
        Case((_, "false"), true);
        Case((_, "Nonempty String"), true);
      });

      It("should convert object to true", () => {
        Case((_, Object.Construct()), true);
        Case((_, String.Construct("")), true);
        Case((_, String.Construct()), true);
        Case((_, Boolean.Construct(true)), true);
        Case((_, Boolean.Construct(false)), true);
        Case((_, Boolean.Construct()), true);
        Case((_, Array.Construct()), true);
        Case((_, Number.Construct()), true);
        Case((_, Number.Construct(-0)), true);
        Case((_, Number.Construct(0)), true);
        Case((_, Number.Construct()), true);
        Case((_, Number.Construct(NaN)), true);
        Case((_, Number.Construct(-1)), true);
        Case((_, Number.Construct(1)), true);
        Case((_, Number.Construct(Number.Get("POSITIVE_INFINITY"))), true);
        Case((_, Number.Construct(Number.Get("NEGATIVE_INFINITY"))), true);
        Case((_, Date.Construct()), true);
        Case((_, Date.Construct(0)), true);
        Case((_, new Symbol("1")), true);
      });

      It("should return boolean value not a Boolean object", () => {
        Case((_, Undefined), Is.TypeOf("boolean"));
        Case((_, 0), Is.TypeOf("boolean"));
        Case((_, 1), Is.TypeOf("boolean"));
        Case((_, -1), Is.TypeOf("boolean"));
        Case((_, -Infinity), Is.TypeOf("boolean"));
        Case((_, NaN), Is.TypeOf("boolean"));
        Case((_, "0"), Is.TypeOf("boolean"));
        Case((_, "1"), Is.TypeOf("boolean"));
        Case((_, "-1"), Is.TypeOf("boolean"));
        Case((_, "true"), Is.TypeOf("boolean"));
        Case((_, "false"), Is.TypeOf("boolean"));
        Case((_, String.Construct("1")), Is.TypeOf("boolean"));
        Case((_, Object.Construct(1)), Is.TypeOf("boolean"));
      });

      It("should return a Boolean object when called as part of a new expression", () => {
        That(Boolean.Construct(), Is.TypeOf("object"));
        That(Boolean.Construct(1), Is.TypeOf("object"));
        That(Boolean.Construct(-1), Is.TypeOf("object"));
        That(Boolean.Construct(-Infinity), Is.TypeOf("object"));
        That(Boolean.Construct(NaN), Is.TypeOf("object"));
        That(Boolean.Construct("0"), Is.TypeOf("object"));
        That(Boolean.Construct("1"), Is.TypeOf("object"));
        That(Boolean.Construct("-1"), Is.TypeOf("object"));
        That(Boolean.Construct("true"), Is.TypeOf("object"));
        That(Boolean.Construct("false"), Is.TypeOf("object"));
        That(Boolean.Construct(String.Construct("1")), Is.TypeOf("object"));
        That(Boolean.Construct(Object.Construct(1)), Is.TypeOf("object"));
      });
    }
  }
}
