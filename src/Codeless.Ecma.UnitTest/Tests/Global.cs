using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;

namespace Codeless.Ecma.UnitTest.Tests {
  public class Global {
    [Test, RuntimeFunctionInjection]
    public static void ParseInt(RuntimeFunction parseInt) {
      Expect((_, " 0xF", 16), gives: 15);
      Expect((_, " F", 16), gives: 15);
      Expect((_, "17", 8), gives: 15);
      Expect((_, "015", 10), gives: 15);
      Expect((_, "15.99", 10), gives: 15);
      Expect((_, "15,123", 10), gives: 15);
      Expect((_, "15e2", 10), gives: 15);
      Expect((_, "15px", 10), gives: 15);
      Expect((_, "0e0", 16), gives: 224);
      Expect((_, "+015", 10), gives: 15);
      Expect((_, "-015", 10), gives: -15);
      Expect((_, 4.7, 10), gives: 4);
      Expect((_, 4.7 * 1e22, 10), gives: 4);
      Expect((_, 0.00000000000434, 10), gives: 4);
      Expect((_, "+", 10), gives: NaN);
      Expect((_, "-", 10), gives: NaN);
      Expect((_, "Hello", 8), gives: NaN);
      Expect((_, "546", 2), gives: NaN);
    }

    [Test, RuntimeFunctionInjection]
    public static void ParseFloat(RuntimeFunction parseFloat) {
      Expect((_, 3.14), gives: 3.14);

      Expect((_, "3.14"), gives: 3.14);
      Expect((_, "314e-2"), gives: 3.14);
      Expect((_, "0.0314E+2"), gives: 3.14);
      Expect((_, "3.14more non-digit characters"), gives: 3.14);
      Expect((_, "FF2"), gives: NaN);

      RuntimeObject foo = new EcmaObject(new Hashtable { { "toString", RuntimeFunction.FromDelegate(() => "3.14") } });
      Expect((_, foo), gives: 3.14);
    }

    [Test, RuntimeFunctionInjection]
    public static void IsNaN(RuntimeFunction isNaN) {
      Expect((_, NaN), gives: true);
      Expect((_, Undefined), gives: true);
      Expect((_, new EcmaObject()), gives: true);
      Expect((_, true), gives: false);
      Expect((_, Null), gives: false);
      Expect((_, 37), gives: false);

      Expect((_, "37"), gives: false);      // false: "37" is converted to the number 37 which is not NaN
      Expect((_, "37.37"), gives: false);   // false: "37.37" is converted to the number 37.37 which is not NaN
      Expect((_, "123ABC"), gives: true);   // true:  parseInt("123ABC") is 123 but Number("123ABC") is NaN
      Expect((_, ""), gives: false);        // false: the empty string is converted to 0 which is not NaN 
      Expect((_, " "), gives: false);       // false: a string with spaces is converted to 0 which is not NaN 
      Expect((_, "blabla"), true);

      Expect((_, new EcmaDate()), gives: false);
      Expect((_, new EcmaDate().ToString()), gives: true);
    }

    [Test, RuntimeFunctionInjection]
    public static void IsFinite(RuntimeFunction isFinite) {
      IsUnconstructableFunctionWLength(isFinite, "isFinite", 1);
      IsAbruptedFromToPrimitive(isFinite.Bind(default));
      IsAbruptedFromSymbolToNumber(isFinite.Bind(default));
    }

    [Test, RuntimeFunctionInjection]
    public static void EncodeURI(RuntimeFunction encodeUri) {
      Expect((_, "https://mozilla.org/?x=шеллы"), "https://mozilla.org/?x=%D1%88%D0%B5%D0%BB%D0%BB%D1%8B");
    }

    [Test, RuntimeFunctionInjection]
    public static void EncodeURIComponent(RuntimeFunction encodeURIComponent) {
      Expect((_, "?x=шеллы"), gives: "%3Fx%3D%D1%88%D0%B5%D0%BB%D0%BB%D1%8B");
      Expect((_, "?x=test"), gives: "%3Fx%3Dtest");
    }

    [Test, RuntimeFunctionInjection]
    public static void Escape(RuntimeFunction escape) {
      Expect((_, "abc123"), gives: "abc123");
      Expect((_, "äöü"), gives: "%E4%F6%FC");
      Expect((_, "ć"), gives: "%u0107");
      Expect((_, "@*_+-./"), gives: "@*_+-./");
    }
  }
}
