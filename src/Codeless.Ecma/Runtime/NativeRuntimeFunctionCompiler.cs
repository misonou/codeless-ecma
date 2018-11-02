using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.Runtime {
  internal class NativeRuntimeFunctionCompiler {
    private static readonly ParameterExpression pRecord = Expression.Parameter(typeof(RuntimeFunctionInvocation), "record");
    private static readonly ParameterExpression pArgs = Expression.Parameter(typeof(EcmaValue[]), "args");
    private static readonly Expression pThisArg = Expression.Property(pRecord, "ThisValue");
    private static readonly Expression pNewTarget = Expression.Property(pRecord, "NewTarget");
    private static readonly MethodInfo mSliceArguments = ((Func<EcmaValue[], int, EcmaValue[]>)SliceArguments).Method;
    private static readonly MethodInfo mGetUnderlyingObject = ((Func<EcmaValue, Type, object>)GetUnderlyingObject).Method;

    private readonly MethodInfo method;
    private readonly ParameterInfo[] parameters;
    private readonly int posThis = -1;
    private readonly int posNew = -1;
    private readonly bool tailArrayOnly;
    private readonly bool tailArray;

    private NativeRuntimeFunctionCompiler(MethodInfo method) {
      this.method = method;
      this.parameters = method.GetParameters();
      if (parameters.Length > 0) {
        for (int i = 0, length = parameters.Length; i < length; i++) {
          if (Attribute.IsDefined(parameters[i], typeof(ThisAttribute))) {
            posThis = i;
            continue;
          }
          if (Attribute.IsDefined(parameters[i], typeof(NewTargetAttribute))) {
            posNew = i;
            continue;
          }
        }
      }
      if (Attribute.IsDefined(parameters[parameters.Length - 1], typeof(ParamArrayAttribute))) {
        this.tailArray = true;
        if (parameters.Length <= 2 && posNew + posThis == parameters.Length - 3) {
          this.tailArrayOnly = true;
        }
      }
    }

    public static RuntimeFunctionDelegate Compile(MethodInfo method) {
      NativeRuntimeFunctionCompiler compiler = new NativeRuntimeFunctionCompiler(method);
      Expression<RuntimeFunctionDelegate> lambda = Expression.Lambda<RuntimeFunctionDelegate>(
        EcmaValueUtility.ConvertToEcmaValueExpression(compiler.GetExpression()), pRecord, pArgs);
      return lambda.Compile();
    }

    private Expression GetExpression() {
      if (parameters.Length == 0) {
        return GetCallExpression(new Expression[0]);
      }
      if (parameters.Length == 1) {
        if (posThis == 0) {
          return GetCallExpression(new[] { pThisArg });
        }
        if (posNew == 0) {
          return GetCallExpression(new[] { pNewTarget });
        }
      }
      if (tailArrayOnly) {
        Expression[] args = new Expression[parameters.Length];
        if (posThis >= 0) {
          args[posThis] = pThisArg;
        }
        if (posNew >= 0) {
          args[posNew] = pNewTarget;
        }
        args[args.Length - 1] = pArgs;
        return GetCallExpression(args);
      }
      return GetSwitchExression();
    }

    private Expression GetCallExpression(IEnumerable<Expression> args) {
      if (method.IsStatic) {
        return Expression.Call(method, args.ToArray());
      }
      return Expression.Call(Expression.Convert(Expression.Call(mGetUnderlyingObject, pThisArg, Expression.Constant(method.DeclaringType)), method.DeclaringType), method, args);
    }

    private Expression GetArgumentFromArray(int argsIndex, int nativeParameterIndex) {
      return EcmaValueUtility.ConvertFromEcmaValueExpression(Expression.ArrayIndex(pArgs, Expression.Constant(argsIndex)), parameters[nativeParameterIndex].ParameterType);
    }

    private Expression GetSwitchExression() {
      Expression len = Expression.Property(pArgs, "Length");
      Expression cur = GetCallExpression(GetParametersForSwitchDefault());
      int max = Math.Min(4, parameters.Length - (posNew >= 0 ? 1 : 0) - (posThis >= 0 ? 1 : 0));
#if NET35
      for (int i = 0; i <= max; i++) {
        cur = Expression.Condition(Expression.Equal(len, Expression.Constant(i)), GetCallExpression(GetParametersForSwitchCase(i)), cur);
      }
      return cur;
#else
      SwitchCase[] cases = new SwitchCase[max + 1];
      for (int i = 0; i <= max; i++) {
        cases[i] = Expression.SwitchCase(GetCallExpression(GetParametersForSwitchCase(i)), Expression.Constant(i));
      }
      return Expression.Switch(len, cur, cases);
#endif
    }

    private IEnumerable<Expression> GetParametersForSwitchCase(int pArgsLength) {
      int k = 0;
      for (int i = 0, length = parameters.Length - (tailArray ? 1 : 0); i < length; i++) {
        if (i == posNew) {
          yield return EcmaValueUtility.ConvertFromEcmaValueExpression(pNewTarget, parameters[i].ParameterType);
        } else if (i == posThis) {
          yield return EcmaValueUtility.ConvertFromEcmaValueExpression(pThisArg, parameters[i].ParameterType);
        } else if (k >= pArgsLength) {
          yield return Expression.Constant(EcmaValue.Undefined);
        } else {
          yield return GetArgumentFromArray(k++, i);
        }
      }
      if (tailArray) {
        if (k >= pArgsLength) {
          yield return Expression.MakeMemberAccess(null, typeof(EcmaValue).GetField("EmptyArray", BindingFlags.Static | BindingFlags.Public));
        } else {
          yield return GetTailArray(k);
        }
      }
    }

    private IEnumerable<Expression> GetParametersForSwitchDefault() {
      int k = 0;
      for (int i = 0, length = parameters.Length - (tailArray ? 1 : 0); i < length; i++) {
        if (i == posNew) {
          yield return EcmaValueUtility.ConvertFromEcmaValueExpression(pNewTarget, parameters[i].ParameterType);
        } else if (i == posThis) {
          yield return EcmaValueUtility.ConvertFromEcmaValueExpression(pThisArg, parameters[i].ParameterType);
        } else {
          yield return Expression.Condition(
            Expression.GreaterThan(Expression.Property(pArgs, "Length"), Expression.Constant(i)),
            EcmaValueUtility.ConvertFromEcmaValueExpression(Expression.ArrayIndex(pArgs, Expression.Constant(k++)), parameters[i].ParameterType),
            Expression.Constant(EcmaValue.Undefined));
        }
      }
      if (tailArray) {
        yield return GetTailArray(k);
      }
    }

    private Expression GetTailArray(int argStartIndex) {
      return Expression.Call(mSliceArguments, pArgs, Expression.Constant(argStartIndex));
    }

    private static EcmaValue[] SliceArguments(EcmaValue[] src, int index) {
      if (src.Length == index) {
        return EcmaValue.EmptyArray;
      }
      EcmaValue[] dst = new EcmaValue[src.Length - index];
      Array.Copy(src, index, dst, 0, dst.Length);
      return dst;
    }

    private static object GetUnderlyingObject(EcmaValue value, Type requiredType) {
      object obj = value.GetUnderlyingObject();
      if (obj == null || (obj.GetType() != requiredType && !obj.GetType().IsSubclassOf(requiredType))) {
        throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
      }
      return obj;
    }
  }
}
