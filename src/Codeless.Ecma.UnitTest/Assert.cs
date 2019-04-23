using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  [DebuggerStepThrough]
  public class Assert {
    [ThreadStatic]
    private static string message;

    public static EcmaValue _ => EcmaValue.Undefined;

    public static IResolveConstraint TypeError => Throws.InstanceOf<EcmaTypeErrorException>();

    public static IResolveConstraint RangeError => Throws.InstanceOf<EcmaRangeErrorException>();

    public static void It(string message, Action tests) {
      NUnit.Framework.Assert.Multiple(() => {
        Assert.message = message;
        tests();
      });
    }

    public static void IsUnconstructableFunctionWLength(EcmaValue fn, string name, int functionLength) {
      That(fn["prototype"], Is.Undefined);
      That(fn["name"], Is.EqualTo(name));
      That(fn, Has.DataProperty("name", EcmaPropertyAttributes.Configurable));
      That(fn["length"], Is.EqualTo(functionLength));
      That(fn, Has.DataProperty("length", EcmaPropertyAttributes.Configurable));
      That(() => fn.Construct(), TypeError);
    }

    public static void IsAbruptedFromSymbolToNumber(RuntimeFunction fn) {
      EcmaValue sym = SymbolConstructor.Symbol("1");
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, sym) : fn.Call(sym), TypeError);
    }

    public static void IsAbruptedFromToPrimitive(RuntimeFunction fn) {
      RuntimeFunction runtimeFunction = RuntimeFunction.FromDelegate(() => throw new Test262Exception());
      EcmaValue obj1 = new EcmaObject(new Hashtable { { "valueOf", runtimeFunction } });
      EcmaValue obj2 = new EcmaObject(new Hashtable { { "toString", runtimeFunction } });
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, obj1) : fn.Call(obj1), Throws.InstanceOf<Test262Exception>());
      That(() => fn is BoundRuntimeFunction ? fn.Call(default, obj2) : fn.Call(obj2), Throws.InstanceOf<Test262Exception>());
    }

    public static void Expect(EcmaValue with, object gives, string because = null) {
      That(Tuple.Create(with), gives, because);
    }

    public static void Expect((EcmaValue, EcmaValue) with, object gives, string because = null) {
      That(with, gives, because);
    }

    public static void Expect((EcmaValue, EcmaValue, EcmaValue) with, object gives, string because = null) {
      That(with, gives, because);
    }

    public static void That(EcmaValue value, IResolveConstraint constraint) {
      if (ShouldUnboxResult(constraint)) {
        NUnit.Framework.Assert.That(value.GetUnderlyingObject(), constraint);
      } else {
        NUnit.Framework.Assert.That(value, constraint);
      }
    }

    public static void That(EcmaValue value, IResolveConstraint constraint, string message, params object[] args) {
      if (ShouldUnboxResult(constraint)) {
        NUnit.Framework.Assert.That(value.GetUnderlyingObject(), constraint, message, args);
      } else {
        NUnit.Framework.Assert.That(value, constraint, message, args);
      }
    }

    public static void That(ITuple callArgs, object condition, string message) {
      RuntimeFunction fn = (RuntimeFunction)TestContext.CurrentContext.Test.Arguments[0];
      EcmaValue thisValue = new EcmaValue(callArgs[0]);
      EcmaValue[] args = new EcmaValue[callArgs.Length - 1];
      for (int i = 1; i < callArgs.Length; i++) {
        args[i - 1] = new EcmaValue(callArgs[i]);
      }
      IResolveConstraint constraint = condition as IResolveConstraint ?? Is.EqualTo(condition);
      if (message == null) {
        message = String.Format("It {0} [Input = {1}]", Assert.message, callArgs);
      }
      if (ShouldRunInDelegate(constraint)) {
        That(() => fn.Call(thisValue, args), constraint, message);
      } else {
        try {
          That(fn.Call(thisValue, args), constraint, message);
        } catch (AssertionException ex) {
          throw;
        } catch (Exception ex) {
          NUnit.Framework.Assert.Fail("{0}: {1}", message, ex.Message);
        }
      }
    }

    private static bool ShouldUnboxResult(IResolveConstraint constraint) {
      return !(constraint is EqualConstraint c && c.Arguments[0] is EcmaValue) && !(constraint is DataPropertyConstraint);
    }

    private static bool ShouldRunInDelegate(IResolveConstraint constraint) {
      return (constraint is InstanceOfTypeConstraint c && ((Type)c.Arguments[0]).IsSubclassOf(typeof(Exception))) || constraint is ThrowsNothingConstraint;
    }

    #region Out-out-the-box methods
    public static void That<TActual>(TActual actual, IResolveConstraint expression) {
      NUnit.Framework.Assert.That(actual, expression);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(code, constraint, getExceptionMessage);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint, string message, params object[] args) {
      NUnit.Framework.Assert.That(code, constraint, message, args);
    }

    public static void That(TestDelegate code, IResolveConstraint constraint) {
      NUnit.Framework.Assert.That(code, constraint);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(del, expr, getExceptionMessage);
    }

    public static void That<TActual>(TActual actual, IResolveConstraint expression, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(actual, expression, getExceptionMessage);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr) {
      NUnit.Framework.Assert.That(del, expr);
    }

    public static void That(Func<bool> condition, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(condition, getExceptionMessage);
    }

    public static void That(Func<bool> condition) {
      NUnit.Framework.Assert.That(condition);
    }

    public static void That(Func<bool> condition, string message, params object[] args) {
      NUnit.Framework.Assert.That(condition, message, args);
    }

    public static void That(bool condition, Func<string> getExceptionMessage) {
      NUnit.Framework.Assert.That(condition, getExceptionMessage);
    }

    public static void That(bool condition) {
      NUnit.Framework.Assert.That(condition);
    }

    public static void That<TActual>(TActual actual, IResolveConstraint expression, string message, params object[] args) {
      NUnit.Framework.Assert.That(actual, expression, message, args);
    }

    public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, string message, params object[] args) {
      NUnit.Framework.Assert.That(del, expr, message, args);
    }

    public static void That(bool condition, string message, params object[] args) {
      NUnit.Framework.Assert.That(condition, message, args);
    }
    #endregion
  }
}
