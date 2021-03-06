﻿using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Constraints;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Codeless.Ecma.UnitTest {
  public class Assert {
    [ThreadStatic]
    private static string message;

    public static void It(string message, Action tests) {
      try {
        StaticHelper.Logs.Clear();
        Assert.message = "It " + message;
        tests();
        RuntimeExecution.ContinueUntilEnd();
      } finally {
        Assert.message = null;
      }
    }

    public static void IsConstructorWLength(EcmaValue fn, string name, int functionLength, RuntimeObject prototype, RuntimeObject fnProto = null) {
      That(fn, Is.TypeOf("function"));
      That(fn, Has.OwnProperty("name", name, EcmaPropertyAttributes.Configurable));
      That(fn, Has.OwnProperty("length", functionLength, EcmaPropertyAttributes.Configurable));
      if (prototype != null) {
        That(fn, Has.OwnProperty("prototype", prototype, EcmaPropertyAttributes.None));
      } else {
        That(Global.Object.Invoke("getOwnPropertyDescriptor", fn, "prototype"), Is.Undefined);
      }
      That(Global.Object.Invoke("getPrototypeOf", fn), Is.EqualTo(fnProto ?? Global.Function.Prototype));
    }

    public static void IsUnconstructableFunctionWLength(EcmaValue fn, string name, int functionLength) {
      That(fn, Is.TypeOf("function"));
      That(fn["prototype"], Is.Undefined);
      That(fn, Has.OwnProperty("name", name ?? String.Empty, EcmaPropertyAttributes.Configurable));
      That(fn, Has.OwnProperty("length", functionLength, EcmaPropertyAttributes.Configurable));
      That(Global.Object.Invoke("getPrototypeOf", fn), Is.EqualTo(Global.Function.Prototype));
      That(Global.Object.Invoke("getOwnPropertyDescriptor", fn, "prototype"), Is.Undefined);
      That(() => fn.Construct(), Throws.TypeError);
    }

    public static void IsAbruptedFromSymbolToNumber(RuntimeFunction fn) {
      EcmaValue sym = new Symbol("1");
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, sym) : fn.Call(sym), Throws.TypeError);
    }

    public static void IsAbruptedFromToPrimitive(RuntimeFunction fn) {
      string message = fn is BoundRuntimeFunction b ?
        String.Format("Function should be returning abrupt record from ToPrimitive(Argument {0})", b.BoundArgs.Length + 1) :
        String.Format("Function should be returning abrupt record from ToPrimitive(this value)");
      EcmaValue obj1 = StaticHelper.CreateObject(valueOf: StaticHelper.ThrowTest262Exception, toString: () => new EcmaObject());
      EcmaValue obj2 = StaticHelper.CreateObject(toString: StaticHelper.ThrowTest262Exception);
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, obj1) : fn.Call(obj1), Throws.Test262, message);
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, obj2) : fn.Call(obj2), Throws.Test262, message);
    }

    public static void IsAbruptedFromToObject(RuntimeFunction fn) {
      string message = fn is BoundRuntimeFunction b ?
        String.Format("Function should be returning abrupt record from ToObject(Argument {0})", b.BoundArgs.Length + 1) :
        String.Format("Function should be returning abrupt record from ToObject(this value)");
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, EcmaValue.Undefined) : fn.Call(EcmaValue.Undefined), Throws.TypeError, message);
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, EcmaValue.Null) : fn.Call(EcmaValue.Null), Throws.TypeError, message);
    }

    public static void VerifyIteratorResult(EcmaValue result, bool done, Action<EcmaValue> action) {
      VerifyIteratorResult(result, done, (object)action);
    }

    public static void VerifyIteratorResult(EcmaValue result, bool done, object value = default) {
      That(result, Is.TypeOf("object"), "iterator return must be an Object");
      That(result["done"], Is.EqualTo(done), "expected iteration done: {0}", done);
      if (value != null) {
        if (value is Delegate d) {
          d.DynamicInvoke(result["value"]);
        } else {
          That(result["value"], value is System.Array arr ? Is.EquivalentTo(arr) : Is.EqualTo(value), "expected iteration result");
        }
      }
    }

    public static void VerifyPromiseSettled(Array sequence = null, int count = 0) {
      RuntimeExecution.ContinueUntilEnd();
      if (sequence != null) {
        That(StaticHelper.Logs, Is.EquivalentTo(sequence));
      } else if (count > 0) {
        That(StaticHelper.Logs.Count, Is.EqualTo(count));
      } else {
        That(StaticHelper.Logs.Count, Is.GreaterThanOrEqualTo(1), "Promise should be fulfilled or rejected");
      }
      StaticHelper.Logs.Clear();
    }

    public static void Case(EcmaValue thisArg, object condition, string message = null) {
      Case((thisArg, new EcmaValue[0]), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3, args.Item4 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3, args.Item4, args.Item5 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3, args.Item4, args.Item5, args.Item6 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3, args.Item4, args.Item5, args.Item6, args.Item7 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue) args, object condition, string message = null) {
      Case((args.Item1, new[] { args.Item2, args.Item3, args.Item4, args.Item5, args.Item6, args.Item7, args.Item8 }), condition, message);
    }

    public static void Case((EcmaValue thisArg, EcmaValue[]) args, object condition, string message = null) {
      RuntimeFunction fn = (RuntimeFunction)TestContext.CurrentContext.Test.Arguments[0];
      Assume.That(fn, Is.Not.Null);

      IResolveConstraint constraint = condition as IResolveConstraint;
      if (constraint == null) {
        constraint = condition is Array arr ? Is.EquivalentTo(arr) : Is.EqualTo(condition);
      }
      message = message + " " + StaticHelper.FormatArguments(args.thisArg, args.Item2);
      if (ShouldRunInDelegate(constraint)) {
        That(() => fn.Call(args.Item1, args.Item2), constraint, message);
      } else {
        That(fn.Call(args.Item1, args.Item2), constraint, message);
      }
    }

    public static void That(EcmaValue value, IResolveConstraint constraint) {
      That(value, constraint, null, new object[0]);
    }

    public static void That(EcmaValue value, IResolveConstraint constraint, string message, params object[] args) {
      message = PrependScopeMessage(message);
      if (constraint is CollectionEquivalentConstraint equivalentConstraint) {
        NUnit.Framework.Assert.That(value.Type != EcmaValueType.Object ? null : EcmaValueUtility.CreateListFromArrayLike(value), equivalentConstraint.Using(new RecursiveArrayEqualityComparer()), message, args);
      } else if (ShouldUnboxResult(constraint)) {
        NUnit.Framework.Assert.That(value.GetUnderlyingObject(), constraint, message, args);
      } else {
        NUnit.Framework.Assert.That(value, constraint, message, args);
      }
    }

    private static bool ShouldUnboxResult(IResolveConstraint constraint) {
      return !(constraint is EqualConstraint equalConstraint && equalConstraint.Arguments[0] is EcmaValue) && !(constraint is DataPropertyConstraint);
    }

    private static bool ShouldRunInDelegate(IResolveConstraint constraint) {
      return (constraint is InstanceOfTypeConstraint typeConstraint && typeof(Exception).IsAssignableFrom((Type)typeConstraint.Arguments[0])) || constraint is ThrowsNothingConstraint;
    }

    private static string PrependScopeMessage(string message) {
      return String.Format("{0}{1}", Assert.message, Assert.message != null && message != null ? ": " + message : "");
    }

    #region Out-out-the-box methods
    public static void That<TActual>(TActual actual, IResolveConstraint expression) {
      NUnit.Framework.Assert.That(actual, expression, message);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(code, constraint, getExceptionMessage);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint, string message, params object[] args) {
      NUnit.Framework.Assert.That(code, constraint, PrependScopeMessage(message), args);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint) {
      NUnit.Framework.Assert.That(code, constraint, message);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(del, expr, getExceptionMessage);
    }

    public static void That<TActual>(TActual actual, IResolveConstraint expression, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(actual, expression, getExceptionMessage);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr) {
      NUnit.Framework.Assert.That(del, expr, message);
    }

    public static void That(Func<bool> condition, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(condition, getExceptionMessage);
    }

    public static void That(Func<bool> condition) {
      NUnit.Framework.Assert.That(condition, PrependScopeMessage(message));
    }

    public static void That(Func<bool> condition, string message, params object[] args) {
      NUnit.Framework.Assert.That(condition, PrependScopeMessage(message), args);
    }

    public static void That(bool condition, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(condition, getExceptionMessage);
    }

    public static void That(bool condition) {
      NUnit.Framework.Assert.That(condition, message);
    }

    public static void That<TActual>(TActual actual, IResolveConstraint expression, string message, params object[] args) {
      NUnit.Framework.Assert.That(actual, expression, PrependScopeMessage(message), args);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, string message, params object[] args) {
      NUnit.Framework.Assert.That(del, expr, PrependScopeMessage(message), args);
    }

    public static void That(bool condition, string message, params object[] args) {
      NUnit.Framework.Assert.That(condition, PrependScopeMessage(message), args);
    }
    #endregion
  }
}
