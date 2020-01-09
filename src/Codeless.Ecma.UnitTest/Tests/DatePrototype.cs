using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class DatePrototype : TestBase {
    [Test, RuntimeFunctionInjection]
    public void GetDate(RuntimeFunction getDate) {
      IsUnconstructableFunctionWLength(getDate, "getDate", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6), 6);
      Case(Date.Construct(2016, 6, 6, 0, 0, 0, -1), 5);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 999), 6);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 1000), 7);

      Case(Date.Construct(2016, 1, 29), 29);
      Case(Date.Construct(2016, 1, 29, 0, 0, 0, -1), 28);
      Case(Date.Construct(2016, 1, 29, 23, 59, 59, 999), 29);
      Case(Date.Construct(2016, 1, 29, 23, 59, 59, 1000), 1);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetDay(RuntimeFunction getDay) {
      IsUnconstructableFunctionWLength(getDay, "getDay", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6), 3);
      Case(Date.Construct(2016, 6, 6, 0, 0, 0, -1), 2);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 999), 3);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 1000), 4);

      Case(Date.Construct(2016, 6, 9), 6);
      Case(Date.Construct(2016, 6, 9, 0, 0, 0, -1), 5);
      Case(Date.Construct(2016, 6, 9, 23, 59, 59, 999), 6);
      Case(Date.Construct(2016, 6, 9, 23, 59, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetFullYear(RuntimeFunction getFullYear) {
      IsUnconstructableFunctionWLength(getFullYear, "getFullYear", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 0), 2016);
      Case(Date.Construct(2016, 0, 1, 0, 0, 0, -1), 2015);
      Case(Date.Construct(2016, 11, 31, 23, 59, 59, 999), 2016);
      Case(Date.Construct(2016, 11, 31, 23, 59, 59, 1000), 2017);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetHours(RuntimeFunction getHours) {
      IsUnconstructableFunctionWLength(getHours, "getHours", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6, 13), 13);
      Case(Date.Construct(2016, 6, 6, 13, 0, 0, -1), 12);
      Case(Date.Construct(2016, 6, 6, 13, 59, 59, 999), 13);
      Case(Date.Construct(2016, 6, 6, 13, 59, 59, 1000), 14);

      Case(Date.Construct(2016, 6, 6, 23), 23);
      Case(Date.Construct(2016, 6, 6, 23, 0, 0, -1), 22);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 999), 23);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetMilliseconds(RuntimeFunction getMilliseconds) {
      IsUnconstructableFunctionWLength(getMilliseconds, "getMilliseconds", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6), 0);
      Case(Date.Construct(2016, 6, 6, 0, 0, 0, -1), 999);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 999), 999);
      Case(Date.Construct(2016, 6, 6, 23, 59, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetMinutes(RuntimeFunction getMinutes) {
      IsUnconstructableFunctionWLength(getMinutes, "getMinutes", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6, 14, 6), 6);
      Case(Date.Construct(2016, 6, 6, 14, 6, 0, -1), 5);
      Case(Date.Construct(2016, 6, 6, 14, 6, 59, 999), 6);
      Case(Date.Construct(2016, 6, 6, 14, 6, 59, 1000), 7);

      Case(Date.Construct(2016, 6, 6, 14, 59), 59);
      Case(Date.Construct(2016, 6, 6, 14, 59, 0, -1), 58);
      Case(Date.Construct(2016, 6, 6, 14, 59, 59, 999), 59);
      Case(Date.Construct(2016, 6, 6, 14, 59, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetMonth(RuntimeFunction getMonth) {
      IsUnconstructableFunctionWLength(getMonth, "getMonth", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6), 6);
      Case(Date.Construct(2016, 6, 0, 0, 0, 0, -1), 5);
      Case(Date.Construct(2016, 6, 31, 23, 59, 59, 999), 6);
      Case(Date.Construct(2016, 6, 31, 23, 59, 59, 1000), 7);

      Case(Date.Construct(2016, 11, 31), 11);
      Case(Date.Construct(2016, 11, 0, 0, 0, 0, -1), 10);
      Case(Date.Construct(2016, 11, 31, 23, 59, 59, 999), 11);
      Case(Date.Construct(2016, 11, 31, 23, 59, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetSeconds(RuntimeFunction getSeconds) {
      IsUnconstructableFunctionWLength(getSeconds, "getSeconds", 0);
      RequireThisDateObject();

      Case(Date.Construct(2016, 6, 6, 14, 16, 30), 30);
      Case(Date.Construct(2016, 6, 6, 14, 16, 30, -1), 29);
      Case(Date.Construct(2016, 6, 6, 14, 16, 30, 999), 30);
      Case(Date.Construct(2016, 6, 6, 14, 16, 30, 1000), 31);

      Case(Date.Construct(2016, 6, 6, 14, 16, 59), 59);
      Case(Date.Construct(2016, 6, 6, 14, 16, 59, -1), 58);
      Case(Date.Construct(2016, 6, 6, 14, 16, 59, 999), 59);
      Case(Date.Construct(2016, 6, 6, 14, 16, 59, 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetTime(RuntimeFunction getTime) {
      IsUnconstructableFunctionWLength(getTime, "getTime", 0);
      RequireThisDateObject();

      Case(Date.Construct(0), 0);
      Case(Date.Construct(-0d), 0);
      Case(Date.Construct(-1), -1);
      Case(Date.Construct(1), 1);
      Case(Date.Construct(8.64e15), 8.64e15);
      Case(Date.Construct(-8.64e15), -8.64e15);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetTimezoneOffset(RuntimeFunction getTimezoneOffset) {
      IsUnconstructableFunctionWLength(getTimezoneOffset, "getTimezoneOffset", 0);
      RequireThisDateObject();

      That(getTimezoneOffset.Call(Date.Construct(8.64e15)), Is.TypeOf("number"));
      That(getTimezoneOffset.Call(Date.Construct(-8.64e15)), Is.TypeOf("number"));
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCDate(RuntimeFunction getUTCDate) {
      IsUnconstructableFunctionWLength(getUTCDate, "getUTCDate", 0);
      RequireThisDateObject();

      var july6 = 1467763200000;
      var feb29 = 1456704000000;
      var dayMs = 24 * 60 * 60 * 1000;

      Case(Date.Construct(july6), 6);
      Case(Date.Construct(july6 - 1), 5);
      Case(Date.Construct(july6 + dayMs - 1), 6);
      Case(Date.Construct(july6 + dayMs), 7);

      Case(Date.Construct(feb29), 29);
      Case(Date.Construct(feb29 - 1), 28);
      Case(Date.Construct(feb29 + dayMs - 1), 29);
      Case(Date.Construct(feb29 + dayMs), 1);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCDay(RuntimeFunction getUTCDay) {
      IsUnconstructableFunctionWLength(getUTCDay, "getUTCDay", 0);
      RequireThisDateObject();

      var july6 = 1467763200000;
      var july9 = 1468022400000;
      var dayMs = 24 * 60 * 60 * 1000;

      Case(Date.Construct(july6), 3);
      Case(Date.Construct(july6 - 1), 2);
      Case(Date.Construct(july6 + dayMs - 1), 3);
      Case(Date.Construct(july6 + dayMs), 4);

      Case(Date.Construct(july9), 6);
      Case(Date.Construct(july9 - 1), 5);
      Case(Date.Construct(july9 + dayMs - 1), 6);
      Case(Date.Construct(july9 + dayMs), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCFullYear(RuntimeFunction getUTCFullYear) {
      IsUnconstructableFunctionWLength(getUTCFullYear, "getUTCFullYear", 0);
      RequireThisDateObject();

      var dec31 = 1483142400000;
      var dayMs = 24 * 60 * 60 * 1000;

      Case(Date.Construct(dec31), 2016);
      Case(Date.Construct(dec31 - 1), 2016);
      Case(Date.Construct(dec31 + dayMs - 1), 2016);
      Case(Date.Construct(dec31 + dayMs), 2017);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCHours(RuntimeFunction getUTCHours) {
      IsUnconstructableFunctionWLength(getUTCHours, "getUTCHours", 0);
      RequireThisDateObject();

      var hour15 = 1467817200000;
      var hour23 = 1467846000000;
      var hourMs = 60 * 60 * 1000;

      Case(Date.Construct(hour15), 15);
      Case(Date.Construct(hour15 - 1), 14);
      Case(Date.Construct(hour15 + hourMs - 1), 15);
      Case(Date.Construct(hour15 + hourMs), 16);

      Case(Date.Construct(hour23), 23);
      Case(Date.Construct(hour23 - 1), 22);
      Case(Date.Construct(hour23 + hourMs - 1), 23);
      Case(Date.Construct(hour23 + hourMs), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCMilliseconds(RuntimeFunction getUTCMilliseconds) {
      IsUnconstructableFunctionWLength(getUTCMilliseconds, "getUTCMilliseconds", 0);
      RequireThisDateObject();

      var july6 = 1467763200000;

      Case(Date.Construct(july6), 0);
      Case(Date.Construct(july6 - 1), 999);
      Case(Date.Construct(july6 + 999), 999);
      Case(Date.Construct(july6 + 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCMinutes(RuntimeFunction getUTCMinutes) {
      IsUnconstructableFunctionWLength(getUTCMinutes, "getUTCMinutes", 0);
      RequireThisDateObject();

      var threeTwentyTwo = 1467818520000;
      var threeFiftyNine = 1467820740000;
      var minMs = 60 * 1000;

      Case(Date.Construct(threeTwentyTwo), 22);
      Case(Date.Construct(threeTwentyTwo - 1), 21);
      Case(Date.Construct(threeTwentyTwo + minMs - 1), 22);
      Case(Date.Construct(threeTwentyTwo + minMs), 23);

      Case(Date.Construct(threeFiftyNine), 59);
      Case(Date.Construct(threeFiftyNine - 1), 58);
      Case(Date.Construct(threeFiftyNine + minMs - 1), 59);
      Case(Date.Construct(threeFiftyNine + minMs), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCMonth(RuntimeFunction getUTCMonth) {
      IsUnconstructableFunctionWLength(getUTCMonth, "getUTCMonth", 0);
      RequireThisDateObject();

      var july31 = 1469923200000;
      var dec31 = 1483142400000;
      var dayMs = 24 * 60 * 60 * 1000;

      Case(Date.Construct(july31), 6);
      Case(Date.Construct(july31 - 1), 6);
      Case(Date.Construct(july31 + dayMs - 1), 6);
      Case(Date.Construct(july31 + dayMs), 7);

      Case(Date.Construct(dec31), 11);
      Case(Date.Construct(dec31 - 1), 11);
      Case(Date.Construct(dec31 + dayMs - 1), 11);
      Case(Date.Construct(dec31 + dayMs), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void GetUTCSeconds(RuntimeFunction getUTCSeconds) {
      IsUnconstructableFunctionWLength(getUTCSeconds, "getUTCSeconds", 0);
      RequireThisDateObject();

      const long sec34 = 1467819394000;
      const long sec59 = 1467819419000;

      Case(Date.Construct(sec34), 34);
      Case(Date.Construct(sec34 - 1), 33);
      Case(Date.Construct(sec34 + 999), 34);
      Case(Date.Construct(sec34 + 1000), 35);

      Case(Date.Construct(sec59), 59);
      Case(Date.Construct(sec59 - 1), 58);
      Case(Date.Construct(sec59 + 999), 59);
      Case(Date.Construct(sec59 + 1000), 0);

      Case(Date.Construct(NaN), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetDate(RuntimeFunction setDate) {
      IsUnconstructableFunctionWLength(setDate, "setDate", 1);
      IsAbruptedFromToPrimitive(setDate.Bind(Date.Construct()));
      RequireThisDateObject();

      It("should correctly set date", () => {
        Case((Date.Construct(2016, 6), 6), Date.Construct(2016, 6, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0), Date.Construct(2016, 5, 30).Invoke("getTime"));
        Case((Date.Construct(2016, 5), 31), Date.Construct(2016, 6, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(2016, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 6, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 6), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);

        Case((Date.Construct(8.64e15), 28), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetFullYear(RuntimeFunction setFullYear) {
      IsUnconstructableFunctionWLength(setFullYear, "setFullYear", 3);
      IsAbruptedFromToPrimitive(setFullYear.Bind(Date.Construct()));
      IsAbruptedFromToPrimitive(setFullYear.Bind(Date.Construct(), 0));
      IsAbruptedFromToPrimitive(setFullYear.Bind(Date.Construct(), 0, 0));
      RequireThisDateObject();

      It("should correctly set year, month and date", () => {
        Case((Date.Construct(2016, 6), 2015), Date.Construct(2015, 6).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 2016, 3), Date.Construct(2016, 3).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 2016, -1), Date.Construct(2015, 11).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 2016, 12), Date.Construct(2017, 0).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 2016, 6, 6), Date.Construct(2016, 6, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 2016, 6, 0), Date.Construct(2016, 5, 30).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 2016, 5, 31), Date.Construct(2016, 6, 1).Invoke("getTime"));

        Case((Date.Construct(NaN), 2016), Date.Construct(2016, 0).Invoke("getTime"));
        Case((Date.Construct(NaN), 2016, 6), Date.Construct(2016, 6).Invoke("getTime"));
        Case((Date.Construct(NaN), 2016, 6, 7), Date.Construct(2016, 6, 7).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(0, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(1, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(0, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2, 6, 1).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, Null), Date.Construct(0, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, true), Date.Construct(0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, false), Date.Construct(0, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, "   +00200.000E-0002  "), Date.Construct(0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, CreateObject(valueOf: () => 2)), Date.Construct(0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, 6, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, 6, Null), Date.Construct(0, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 6, true), Date.Construct(0, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 6, false), Date.Construct(0, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 6, "   +00200.000E-0002  "), Date.Construct(0, 6, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 6, CreateObject(valueOf: () => 2)), Date.Construct(0, 6, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), 0, NaN), NaN);
        Case((Date.Construct(), 0, 0, NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);
        Case((Date.Construct(), 0, Infinity), NaN);
        Case((Date.Construct(), 0, 0, Infinity), NaN);

        Case((Date.Construct(8.64e15), 275761), NaN);
        Case((Date.Construct(8.64e15), 275760, 9), NaN);
        Case((Date.Construct(8.64e15), 275760, 8, 14), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetHours(RuntimeFunction setHours) {
      IsUnconstructableFunctionWLength(setHours, "setHours", 4);
      IsAbruptedFromToPrimitive(setHours.Bind(Date.Construct()));
      IsAbruptedFromToPrimitive(setHours.Bind(Date.Construct(), 0));
      IsAbruptedFromToPrimitive(setHours.Bind(Date.Construct(), 0, 0));
      IsAbruptedFromToPrimitive(setHours.Bind(Date.Construct(), 0, 0, 0));
      RequireThisDateObject();

      It("should correctly set hours, minutes, seconds and milliseconds", () => {
        Case((Date.Construct(2016, 6, 1), 6), Date.Construct(2016, 6, 1, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), -1), Date.Construct(2016, 5, 30, 23).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 24), Date.Construct(2016, 6, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6, 1), 0, 23), Date.Construct(2016, 6, 1, 0, 23).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, -1), Date.Construct(2016, 5, 30, 23, 59).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 60), Date.Construct(2016, 6, 1, 1, 0).Invoke("getTime"));

        Case((Date.Construct(2016, 6, 1), 0, 0, 45), Date.Construct(2016, 6, 1, 0, 0, 45).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, -1), Date.Construct(2016, 5, 30, 23, 59, 59).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, 60), Date.Construct(2016, 6, 1, 0, 1).Invoke("getTime"));

        Case((Date.Construct(2016, 6, 1), 0, 0, 0, 345), Date.Construct(2016, 6, 1, 0, 0, 0, 345).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, 0, -1), Date.Construct(2016, 5, 30, 23, 59, 59, 999).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, 0, 1000), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 6, 1, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, true), Date.Construct(2016, 6, 1, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, 0, Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, true), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, 0, 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, 0, 0, Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, 0, true), Date.Construct(2016, 6, 1, 0, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 0), NaN);
        Case((Date.Construct(NaN), 0, 0), NaN);
        Case((Date.Construct(NaN), 0, 0, 0), NaN);
        Case((Date.Construct(NaN), 0, 0, 0, 0), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), 0, NaN), NaN);
        Case((Date.Construct(), 0, 0, NaN), NaN);
        Case((Date.Construct(), 0, 0, 0, NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);
        Case((Date.Construct(), 0, Infinity), NaN);
        Case((Date.Construct(), 0, 0, Infinity), NaN);
        Case((Date.Construct(), 0, 0, 0, Infinity), NaN);

        Case((Date.Construct(8.64e15), 24), NaN);
        Case((Date.Construct(8.64e15), 0, 24 * 60), NaN);
        Case((Date.Construct(8.64e15), 0, 0, 24 * 60 * 60), NaN);
        Case((Date.Construct(8.64e15), 0, 0, 0, 24 * 60 * 60 * 1000), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetMilliseconds(RuntimeFunction setMilliseconds) {
      IsUnconstructableFunctionWLength(setMilliseconds, "setMilliseconds", 1);
      IsAbruptedFromToPrimitive(setMilliseconds.Bind(Date.Construct()));
      RequireThisDateObject();

      It("should correctly set milliseconds", () => {
        Case((Date.Construct(2016, 6, 1), 333), Date.Construct(2016, 6, 1, 0, 0, 0, 333).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), -1), Date.Construct(2016, 5, 30, 23, 59, 59, 999).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 1000), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 6, 1, 0, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 0), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);

        Case((Date.Construct(8.64e15), 1), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetMinutes(RuntimeFunction setMinutes) {
      IsUnconstructableFunctionWLength(setMinutes, "setMinutes", 3);
      IsAbruptedFromToPrimitive(setMinutes.Bind(Date.Construct()));
      IsAbruptedFromToPrimitive(setMinutes.Bind(Date.Construct(), 0));
      IsAbruptedFromToPrimitive(setMinutes.Bind(Date.Construct(), 0, 0));
      RequireThisDateObject();

      It("should correctly set minutes, seconds and milliseconds", () => {
        Case((Date.Construct(2016, 6, 1), 23), Date.Construct(2016, 6, 1, 0, 23).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), -1), Date.Construct(2016, 5, 30, 23, 59).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 60), Date.Construct(2016, 6, 1, 1, 0).Invoke("getTime"));

        Case((Date.Construct(2016, 6, 1), 0, 45), Date.Construct(2016, 6, 1, 0, 0, 45).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, -1), Date.Construct(2016, 5, 30, 23, 59, 59).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 60), Date.Construct(2016, 6, 1, 0, 1).Invoke("getTime"));

        Case((Date.Construct(2016, 6, 1), 0, 0, 345), Date.Construct(2016, 6, 1, 0, 0, 0, 345).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, -1), Date.Construct(2016, 5, 30, 23, 59, 59, 999).Invoke("getTime"));
        Case((Date.Construct(2016, 6, 1), 0, 0, 1000), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 6, 1, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, true), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, 0, Null), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, true), Date.Construct(2016, 6, 1, 0, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 0), NaN);
        Case((Date.Construct(NaN), 0, 0), NaN);
        Case((Date.Construct(NaN), 0, 0, 0), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), 0, NaN), NaN);
        Case((Date.Construct(), 0, 0, NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);
        Case((Date.Construct(), 0, Infinity), NaN);
        Case((Date.Construct(), 0, 0, Infinity), NaN);

        Case((Date.Construct(8.64e15), 24 * 60), NaN);
        Case((Date.Construct(8.64e15), 0, 24 * 60 * 60), NaN);
        Case((Date.Construct(8.64e15), 0, 0, 24 * 60 * 60 * 1000), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetMonth(RuntimeFunction setMonth) {
      IsUnconstructableFunctionWLength(setMonth, "setMonth", 2);
      IsAbruptedFromToPrimitive(setMonth.Bind(Date.Construct()));
      IsAbruptedFromToPrimitive(setMonth.Bind(Date.Construct(), 0));
      RequireThisDateObject();

      It("should correctly set month and date", () => {
        Case((Date.Construct(2016, 6), 3), Date.Construct(2016, 3).Invoke("getTime"));
        Case((Date.Construct(2016, 6), -1), Date.Construct(2015, 11).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0), Date.Construct(2016, 0).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 6, 6), Date.Construct(2016, 6, 6).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 6, 0), Date.Construct(2016, 5, 30).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 5, 31), Date.Construct(2016, 6, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), Null), Date.Construct(2016, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 6, Undefined), NaN);
        Case((Date.Construct(2016, 6), 6, Null), Date.Construct(2016, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 6, true), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 6, false), Date.Construct(2016, 6, 0).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 6, "   +00200.000E-0002  "), Date.Construct(2016, 6, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 6, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 0), NaN);
        Case((Date.Construct(NaN), 0, 0), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), 0, NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);
        Case((Date.Construct(), 0, Infinity), NaN);

        Case((Date.Construct(8.64e15), 9), NaN);
        Case((Date.Construct(8.64e15), 8, 14), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetSeconds(RuntimeFunction setSeconds) {
      IsUnconstructableFunctionWLength(setSeconds, "setSeconds", 2);
      IsAbruptedFromToPrimitive(setSeconds.Bind(Date.Construct()));
      IsAbruptedFromToPrimitive(setSeconds.Bind(Date.Construct(), 0));
      RequireThisDateObject();

      It("should correctly set seconds and milliseconds", () => {
        Case((Date.Construct(2016, 6), 45), Date.Construct(2016, 6, 1, 0, 0, 45).Invoke("getTime"));
        Case((Date.Construct(2016, 6), -1), Date.Construct(2016, 5, 30, 23, 59, 59).Invoke("getTime"));
        Case((Date.Construct(2016, 5, 30, 23, 59, 59), 60), Date.Construct(2016, 6).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, 543), Date.Construct(2016, 6, 1, 0, 0, 0, 543).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, -1), Date.Construct(2016, 5, 30, 23, 59, 59, 999).Invoke("getTime"));
        Case((Date.Construct(2016, 5, 30, 23, 59, 59), 0, 1000), Date.Construct(2016, 5, 30, 23, 59, 1).Invoke("getTime"));
      });

      It("should coerce provided argument", () => {
        Case((Date.Construct(2016, 6), Undefined), NaN);
        Case((Date.Construct(2016, 6), true), Date.Construct(2016, 6, 1, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 2).Invoke("getTime"));

        Case((Date.Construct(2016, 6), 0, Undefined), NaN);
        Case((Date.Construct(2016, 6), 0, true), Date.Construct(2016, 6, 1, 0, 0, 0, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, false), Date.Construct(2016, 6, 1).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, "   +00200.000E-0002  "), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
        Case((Date.Construct(2016, 6), 0, CreateObject(valueOf: () => 2)), Date.Construct(2016, 6, 1, 0, 0, 0, 2).Invoke("getTime"));
      });

      It("should set [[DateValue]] to invalid date if arguments is not finite or exceed TimeClip", () => {
        Case((Date.Construct(NaN), 0), NaN);
        Case((Date.Construct(NaN), 0, 0), NaN);

        Case((Date.Construct(), NaN), NaN);
        Case((Date.Construct(), 0, NaN), NaN);
        Case((Date.Construct(), Infinity), NaN);
        Case((Date.Construct(), 0, Infinity), NaN);

        Case((Date.Construct(8.64e15), 24 * 60 * 60), NaN);
        Case((Date.Construct(8.64e15), 0, 24 * 60 * 60 * 1000), NaN);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetTime(RuntimeFunction setTime) {
      IsUnconstructableFunctionWLength(setTime, "setTime", 1);
      IsAbruptedFromToPrimitive(setTime.Bind(Date.Construct()));
      RequireThisDateObject();

      It("should set the [[DateValue]] internal slot of this Date object to v", () => {
        EcmaValue date = Date.Construct(NaN);
        Case((date, 0), 0);
        That(date.Invoke("getTime"), Is.EqualTo(0));

        Case((date, 8.64e15 + 1), NaN);
        That(date.Invoke("getTime"), Is.EqualTo(NaN));
      });

      It("should coerce provided argument", () => {
        EcmaValue date = Date.Construct(2016, 6);
        EcmaValue callCount = 0;
        EcmaValue args = default, thisValue = default;
        EcmaValue arg = CreateObject(valueOf: () => {
          args = Arguments;
          thisValue = This;
          callCount += 1;
          return 2;
        });
        Case((date, arg), 2);
        That(callCount, Is.EqualTo(1));
        That(args["length"], Is.EqualTo(0));
        That(thisValue, Is.EqualTo(arg));

        Case((date, Null), 0);
        Case((date, true), 1);
        Case((date, false), 0);
        Case((date, "   +00200.000E-0002  "), 2);
        Case(date, NaN);
      });

      It("should throw a TypeError if this value does not have [[DateValue]] internal slot", () => {
        EcmaValue callCount = 0;
        EcmaValue arg = CreateObject(valueOf: () => { callCount += 1; return 2; });
        Case((Null, arg), Throws.TypeError);
        Case((_, arg), Throws.TypeError);
        Case((0, arg), Throws.TypeError);
        Case((true, arg), Throws.TypeError);
        Case(("", arg), Throws.TypeError);
        Case((new EcmaArray(), arg), Throws.TypeError);
        Case((new EcmaObject(), arg), Throws.TypeError);
        Case((new Symbol(), arg), Throws.TypeError);

        That(callCount, Is.EqualTo(0), "validation precedes input coercion");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCDate(RuntimeFunction setUTCDate) {
      IsUnconstructableFunctionWLength(setUTCDate, "setUTCDate", 1);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCDate.Call(date, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-11T10:10:10.010Z"));

      Case((Date.Construct(NaN), 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCFullYear(RuntimeFunction setUTCFullYear) {
      IsUnconstructableFunctionWLength(setUTCFullYear, "setUTCFullYear", 3);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCFullYear.Call(date, 2000);
      That(date.Invoke("toISOString"), Is.EqualTo("2000-10-10T10:10:10.010Z"));

      setUTCFullYear.Call(date, 2000, 10);
      That(date.Invoke("toISOString"), Is.EqualTo("2000-11-10T10:10:10.010Z"));

      setUTCFullYear.Call(date, 2000, 11, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("2000-12-11T10:10:10.010Z"));

      Case((Date.Construct(NaN), 0), -62167219200000);
      Case((Date.Construct(NaN), 0, 0), -62167219200000);
      Case((Date.Construct(NaN), 0, 0, 1), -62167219200000);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), 0, NaN), NaN);
      Case((Date.Construct(), 0, 0, NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
      Case((Date.Construct(), 0, Infinity), NaN);
      Case((Date.Construct(), 0, 0, Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCHours(RuntimeFunction setUTCHours) {
      IsUnconstructableFunctionWLength(setUTCHours, "setUTCHours", 4);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCHours.Call(date, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T11:10:10.010Z"));

      setUTCHours.Call(date, 12, 12);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T12:12:10.010Z"));

      setUTCHours.Call(date, 13, 13, 13);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T13:13:13.010Z"));

      setUTCHours.Call(date, 14, 14, 14, 14);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T14:14:14.014Z"));

      Case((Date.Construct(NaN), 0), NaN);
      Case((Date.Construct(NaN), 0, 0), NaN);
      Case((Date.Construct(NaN), 0, 0, 0), NaN);
      Case((Date.Construct(NaN), 0, 0, 0, 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), 0, NaN), NaN);
      Case((Date.Construct(), 0, 0, NaN), NaN);
      Case((Date.Construct(), 0, 0, 0, NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
      Case((Date.Construct(), 0, Infinity), NaN);
      Case((Date.Construct(), 0, 0, Infinity), NaN);
      Case((Date.Construct(), 0, 0, 0, Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCMilliseconds(RuntimeFunction setUTCMilliseconds) {
      IsUnconstructableFunctionWLength(setUTCMilliseconds, "setUTCMilliseconds", 1);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCMilliseconds.Call(date, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.011Z"));

      Case((Date.Construct(NaN), 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCMinutes(RuntimeFunction setUTCMinutes) {
      IsUnconstructableFunctionWLength(setUTCMinutes, "setUTCMinutes", 3);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCMinutes.Call(date, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:11:10.010Z"));

      setUTCMinutes.Call(date, 12, 12);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:12:12.010Z"));

      setUTCMinutes.Call(date, 13, 13, 13);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:13:13.013Z"));

      Case((Date.Construct(NaN), 0), NaN);
      Case((Date.Construct(NaN), 0, 0), NaN);
      Case((Date.Construct(NaN), 0, 0, 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), 0, NaN), NaN);
      Case((Date.Construct(), 0, 0, NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
      Case((Date.Construct(), 0, Infinity), NaN);
      Case((Date.Construct(), 0, 0, Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCMonth(RuntimeFunction setUTCMonth) {
      IsUnconstructableFunctionWLength(setUTCMonth, "setUTCMonth", 2);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCMonth.Call(date, 10);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-11-10T10:10:10.010Z"));

      setUTCMonth.Call(date, 11, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-12-11T10:10:10.010Z"));

      Case((Date.Construct(NaN), 0), NaN);
      Case((Date.Construct(NaN), 0, 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), 0, NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
      Case((Date.Construct(), 0, Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void SetUTCSeconds(RuntimeFunction setUTCSeconds) {
      IsUnconstructableFunctionWLength(setUTCSeconds, "setUTCSeconds", 2);
      RequireThisDateObject();

      EcmaValue date = Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10));
      Assume.That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:10.010Z"));

      setUTCSeconds.Call(date, 11);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:11.010Z"));

      setUTCSeconds.Call(date, 12, 12);
      That(date.Invoke("toISOString"), Is.EqualTo("1999-10-10T10:10:12.012Z"));

      Case((Date.Construct(NaN), 0), NaN);
      Case((Date.Construct(NaN), 0, 0), NaN);

      Case((Date.Construct(), NaN), NaN);
      Case((Date.Construct(), 0, NaN), NaN);
      Case((Date.Construct(), Infinity), NaN);
      Case((Date.Construct(), 0, Infinity), NaN);
    }

    [Test, RuntimeFunctionInjection]
    public void ToDateString(RuntimeFunction toDateString) {
      IsUnconstructableFunctionWLength(toDateString, "toDateString", 0);
      RequireThisDateObject();

      It("should return a string in correct format or Invalid Date", () => {
        EcmaValue re = RegExp.Construct(@"^(Sun|Mon|Tue|Wed|Thu|Fri|Sat) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) [0-9]{2} [0-9]{4}$");
        That((bool)re.Invoke("test", toDateString.Call(Date.Construct())));

        Case(Date.Construct(NaN), "Invalid Date");
      });

      It("should serialize negative years with at least four digits", () => {
        var negative1DigitYearToString = toDateString.Call(Date.Construct("-000001-07-01T00:00Z"));
        var negative2DigitYearToString = toDateString.Call(Date.Construct("-000012-07-01T00:00Z"));
        var negative3DigitYearToString = toDateString.Call(Date.Construct("-000123-07-01T00:00Z"));
        var negative4DigitYearToString = toDateString.Call(Date.Construct("-001234-07-01T00:00Z"));
        var negative5DigitYearToString = toDateString.Call(Date.Construct("-012345-07-01T00:00Z"));
        var negative6DigitYearToString = toDateString.Call(Date.Construct("-123456-07-01T00:00Z"));

        That(negative1DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0001"));
        That(negative2DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0012"));
        That(negative3DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0123"));
        That(negative4DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-1234"));
        That(negative5DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-12345"));
        That(negative6DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-123456"));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToISOString(RuntimeFunction toISOString) {
      IsUnconstructableFunctionWLength(toISOString, "toISOString", 0);
      RequireThisDateObject();

      Case(Date.Construct(Date.Invoke("UTC", 1999, 9, 10, 10, 10, 10, 10)), "1999-10-10T10:10:10.010Z");

      Case(Date.Construct(NaN), Throws.RangeError);

      Case(new EcmaArray(), Throws.TypeError);
      Case(15, Throws.TypeError);
      Case("1970-01-00000:00:00.000Z", Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void ToJSON(RuntimeFunction toJSON) {
      IsUnconstructableFunctionWLength(toJSON, "toJSON", 1);
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleDateString(RuntimeFunction toLocaleDateString) {
      IsUnconstructableFunctionWLength(toLocaleDateString, "toLocaleDateString", 0);
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleString(RuntimeFunction toLocaleString) {
      IsUnconstructableFunctionWLength(toLocaleString, "toLocaleString", 0);
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleTimeString(RuntimeFunction toLocaleTimeString) {
      IsUnconstructableFunctionWLength(toLocaleTimeString, "toLocaleTimeString", 0);
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);
      RequireThisDateObject();

      It("should return a string in correct format or Invalid Date", () => {
        EcmaValue re = RegExp.Construct(@"^(Sun|Mon|Tue|Wed|Thu|Fri|Sat) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) [0-9]{2} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2} GMT[+-][0-9]{4}( \(.+\))?$");
        That((bool)re.Invoke("test", toString.Call(Date.Construct())));

        Case(Date.Construct(NaN), "Invalid Date");
      });

      It("should throw a TypeError on non-Date receivers", () => {
        Case(_, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(Date.Prototype, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case(new EcmaObject(), Throws.TypeError);
        Case("Tue Mar 21 2017 12:16:43 GMT-0400 (EDT)", Throws.TypeError);
        Case(1490113011493, Throws.TypeError);
      });

      It("should serialize negative years with at least four digits", () => {
        var negative1DigitYearToString = toString.Call(Date.Construct("-000001-07-01T00:00Z"));
        var negative2DigitYearToString = toString.Call(Date.Construct("-000012-07-01T00:00Z"));
        var negative3DigitYearToString = toString.Call(Date.Construct("-000123-07-01T00:00Z"));
        var negative4DigitYearToString = toString.Call(Date.Construct("-001234-07-01T00:00Z"));
        var negative5DigitYearToString = toString.Call(Date.Construct("-012345-07-01T00:00Z"));
        var negative6DigitYearToString = toString.Call(Date.Construct("-123456-07-01T00:00Z"));

        That(negative1DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0001"));
        That(negative2DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0012"));
        That(negative3DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-0123"));
        That(negative4DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-1234"));
        That(negative5DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-12345"));
        That(negative6DigitYearToString.Invoke("split", " ")[3], Is.EqualTo("-123456"));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToTimeString(RuntimeFunction toTimeString) {
      IsUnconstructableFunctionWLength(toTimeString, "toTimeString", 0);
      RequireThisDateObject();

      It("should return a string in correct format or Invalid Date", () => {
        EcmaValue re = RegExp.Construct(@"^[0-9]{2}:[0-9]{2}:[0-9]{2} GMT[+-][0-9]{4}( \(.+\))?$");
        That((bool)re.Invoke("test", toTimeString.Call(Date.Construct())));

        Case(Date.Construct(NaN), "Invalid Date");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToUTCString(RuntimeFunction toUTCString) {
      IsUnconstructableFunctionWLength(toUTCString, "toUTCString", 0);
      RequireThisDateObject();

      Case(Date.Construct("2014-03-23T00:00:00Z"), "Sun, 23 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-24T00:00:00Z"), "Mon, 24 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-25T00:00:00Z"), "Tue, 25 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-26T00:00:00Z"), "Wed, 26 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-27T00:00:00Z"), "Thu, 27 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-28T00:00:00Z"), "Fri, 28 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-29T00:00:00Z"), "Sat, 29 Mar 2014 00:00:00 GMT");

      Case(Date.Construct("2014-01-01T00:00:00Z"), "Wed, 01 Jan 2014 00:00:00 GMT");
      Case(Date.Construct("2014-02-01T00:00:00Z"), "Sat, 01 Feb 2014 00:00:00 GMT");
      Case(Date.Construct("2014-03-01T00:00:00Z"), "Sat, 01 Mar 2014 00:00:00 GMT");
      Case(Date.Construct("2014-04-01T00:00:00Z"), "Tue, 01 Apr 2014 00:00:00 GMT");
      Case(Date.Construct("2014-05-01T00:00:00Z"), "Thu, 01 May 2014 00:00:00 GMT");
      Case(Date.Construct("2014-06-01T00:00:00Z"), "Sun, 01 Jun 2014 00:00:00 GMT");
      Case(Date.Construct("2014-07-01T00:00:00Z"), "Tue, 01 Jul 2014 00:00:00 GMT");
      Case(Date.Construct("2014-08-01T00:00:00Z"), "Fri, 01 Aug 2014 00:00:00 GMT");
      Case(Date.Construct("2014-09-01T00:00:00Z"), "Mon, 01 Sep 2014 00:00:00 GMT");
      Case(Date.Construct("2014-10-01T00:00:00Z"), "Wed, 01 Oct 2014 00:00:00 GMT");
      Case(Date.Construct("2014-11-01T00:00:00Z"), "Sat, 01 Nov 2014 00:00:00 GMT");
      Case(Date.Construct("2014-12-01T00:00:00Z"), "Mon, 01 Dec 2014 00:00:00 GMT");

      Case(Date.Construct("-000001-07-01T00:00Z"), "Thu, 01 Jul -001 00:00:00 GMT");
      Case(Date.Construct("-000012-07-01T00:00Z"), "Fri, 01 Jul -012 00:00:00 GMT");
      Case(Date.Construct("-000123-07-01T00:00Z"), "Sun, 01 Jul -123 00:00:00 GMT");
      Case(Date.Construct("-001234-07-01T00:00Z"), "Fri, 01 Jul -1234 00:00:00 GMT");
      Case(Date.Construct("-012345-07-01T00:00Z"), "Thu, 01 Jul -12345 00:00:00 GMT");
      Case(Date.Construct("-123456-07-01T00:00Z"), "Wed, 01 Jul -123456 00:00:00 GMT");

      Case(Date.Construct(NaN), "Invalid Date");
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);

      It("should return [[DateValue]] converted by the Date constructor", () => {
        Case(Date.Construct(6.54321), 6);
        Case(Date.Construct(-6.54321), -6);
        Case(Date.Construct(6.54321e2), 654);
        Case(Date.Construct(-6.54321e2), -654);
        Case(Date.Construct(0.654321e1), 6);
        Case(Date.Construct(-0.654321e1), -6);
        Case(Date.Construct(true), 1);
        Case(Date.Construct(false), 0);
        Case(Date.Construct(1.23e15), 1.23e15);
        Case(Date.Construct(-1.23e15), -1.23e15);
        Case(Date.Construct(1.23e-15), 0);
        Case(Date.Construct(-1.23e-15), -0);

        Case(Date.Construct(NaN), NaN, "NaN");
        Case(Date.Construct(Infinity), NaN, "Infinity");
        Case(Date.Construct(-Infinity), NaN, "Infinity");
        Case(Date.Construct(0), 0, "0");
        Case(Date.Construct(-0), 0, "-0");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToPrimitive(RuntimeFunction toPrimitive) {
      IsUnconstructableFunctionWLength(toPrimitive, "[Symbol.toPrimitive]", 1);
      RequireThisDateObject();

      It("should throw a TypeError if an invalid `hint` argument is specified", () => {
        Case(Date.Construct(), Throws.TypeError);
        Case((Date.Construct(), Undefined), Throws.TypeError);
        Case((Date.Construct(), Null), Throws.TypeError);
        Case((Date.Construct(), ""), Throws.TypeError);
        Case((Date.Construct(), "String"), Throws.TypeError);
        Case((Date.Construct(), "defaultnumber"), Throws.TypeError);
        Case((Date.Construct(), String.Construct("number")), Throws.TypeError);
        Case((Date.Construct(), CreateObject(toString: () => "number")), Throws.TypeError);
      });
    }

    #region Helper
    private static void RequireThisDateObject() {
      It("should throw a TypeError if this value is not a Date object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(Date.Prototype, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new EcmaObject(), Throws.TypeError);
        Case(new EcmaArray(), Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
        Case("Tue Mar 21 2017 12:16:43 GMT-0400 (EDT)", Throws.TypeError);
        Case(1490113011493, Throws.TypeError);
      });
    }
    #endregion
  }
}
