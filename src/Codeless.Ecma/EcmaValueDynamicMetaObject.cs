#if DYNAMIC
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma {
  public partial struct EcmaValue : IDynamicMetaObjectProvider {
    public dynamic ToDynamic() {
      return this;
    }

    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
      return new EcmaValueDynamicMetaObject(parameter, this);
    }
  }

  internal class EcmaValueDynamicMetaObject : DynamicMetaObject {
    public EcmaValueDynamicMetaObject(Expression expression, object value)
      : base(expression, BindingRestrictions.GetTypeRestriction(expression, typeof(EcmaValue)), value) { }

    public override DynamicMetaObject BindConvert(ConvertBinder binder) {
      return ReturnDynamicMetaObject(binder, EcmaValueUtility.ConvertFromEcmaValueExpression(Expression, binder.ReturnType));
    }

    public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
      if (binder.Operation == ExpressionType.Add) {
        ParameterExpression p = Expression.Parameter(typeof(EcmaValue));
        Expression expr = Expression.Block(
          new[] { p },
          Expression.Assign(p, Expression.Convert(Expression, typeof(EcmaValue))),
          Expression.Condition(Expression.Call(p, "ToBoolean", Type.EmptyTypes), arg.Expression, p)
        );
        return ReturnDynamicMetaObject(binder, expr);
      }
      if (binder.Operation == ExpressionType.Or) {
        ParameterExpression p = Expression.Parameter(typeof(EcmaValue));
        Expression expr = Expression.Block(
          new[] { p },
          Expression.Assign(p, Expression.Convert(Expression, typeof(EcmaValue))),
          Expression.Condition(Expression.Call(p, "ToBoolean", Type.EmptyTypes), p, arg.Expression)
        );
        return ReturnDynamicMetaObject(binder, expr);
      }
      if (binder.Operation == ExpressionType.TypeIs) {
        Expression expr = CallEcmaValueMethod("Instanceof", GetConvertToEcmaValueExpression(arg));
        return ReturnDynamicMetaObject(binder, expr);
      }
      if (arg.LimitType != typeof(EcmaValue)) {
        Expression expr = Expression.MakeBinary(binder.Operation, Expression.Convert(Expression, typeof(EcmaValue)), GetConvertToEcmaValueExpression(arg));
        return ReturnDynamicMetaObject(binder, expr);
      }
      return base.BindBinaryOperation(binder, arg);
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
      Expression expr = CallEcmaValueMethod("get_Item", Expression.Constant(new EcmaPropertyKey(binder.Name)));
      return ReturnDynamicMetaObject(binder, expr);
    }

    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
      if (indexes.Length == 1) {
        DynamicMetaObject index = indexes[0];
        if (index.LimitType == typeof(int) || index.LimitType == typeof(string) || index.LimitType == typeof(Symbol)) {
          Expression expr = CallEcmaValueMethod("get_Item", Expression.Convert(index.Expression, typeof(EcmaPropertyKey)));
          return ReturnDynamicMetaObject(binder, expr);
        }
      }
      return base.BindGetIndex(binder, indexes);
    }

    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
      Expression expr = Expression.Assign(Expression.Property(Expression.Convert(Expression, typeof(EcmaValue)), "Item", Expression.Constant(new EcmaPropertyKey(binder.Name))),
         EcmaValueUtility.ConvertToEcmaValueExpression(value.Expression));
      return ReturnDynamicMetaObject(binder, expr);
    }

    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
      if (indexes.Length == 1) {
        DynamicMetaObject index = indexes[0];
        if (index.LimitType == typeof(int) || index.LimitType == typeof(string) || index.LimitType == typeof(Symbol)) {
          Expression expr = Expression.Assign(Expression.Property(Expression.Convert(Expression, typeof(EcmaValue)), "Item", Expression.Convert(index.Expression, typeof(EcmaPropertyKey))),
            GetConvertToEcmaValueExpression(value));
          return ReturnDynamicMetaObject(binder, expr);
        }
      }
      return base.BindSetIndex(binder, indexes, value);
    }

    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
      Expression expr = CallEcmaValueMethod("Call",
        Expression.Constant(EcmaValue.Undefined),
        Expression.NewArrayInit(typeof(EcmaValue), args.Select(GetConvertToEcmaValueExpression)));
      return ReturnDynamicMetaObject(binder, expr);
    }

    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
      Expression expr = CallEcmaValueMethod("Invoke",
        Expression.Constant(new EcmaPropertyKey(binder.Name)),
        Expression.NewArrayInit(typeof(EcmaValue), args.Select(GetConvertToEcmaValueExpression)));
      return ReturnDynamicMetaObject(binder, expr);
    }

    public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
      Expression expr = CallEcmaValueMethod("Construct",
        Expression.NewArrayInit(typeof(EcmaValue), args.Select(GetConvertToEcmaValueExpression)));
      return ReturnDynamicMetaObject(binder, expr);
    }

    private Expression CallEcmaValueMethod(string name, params Expression[] args) {
      return Expression.Call(Expression.Convert(Expression, typeof(EcmaValue)), name, Type.EmptyTypes, args);
    }

    private Expression GetConvertToEcmaValueExpression(DynamicMetaObject arg) {
      return arg.HasValue ? Expression.Constant(new EcmaValue(arg.Value)) : EcmaValueUtility.ConvertToEcmaValueExpression(arg.Expression);
    }

    private DynamicMetaObject ReturnDynamicMetaObject(DynamicMetaObjectBinder binder, Expression value) {
      return new DynamicMetaObject(Expression.Convert(value, binder.ReturnType), this.Restrictions, this.Value);
    }
  }
}
#endif