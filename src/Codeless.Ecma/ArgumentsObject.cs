using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma {
  public class ArgumentsObject : RuntimeObject, IList<EcmaValue> {
    private enum TaintLevel { None, AttributeTainted, BindingTainted }

    private readonly RuntimeFunctionInvocation invocation;
    private readonly EcmaValue[] arguments;
    private TaintLevel[] taintedLevel;

    internal ArgumentsObject(RuntimeFunctionInvocation invocation, EcmaValue[] arguments)
      : base(WellKnownObject.ObjectPrototype) {
      Guard.ArgumentNotNull(invocation, "invocation");
      Guard.ArgumentNotNull(arguments, "arguments");
      this.invocation = invocation;
      this.arguments = arguments;
      if (invocation.FunctionObject.ContainsUseStrict) {
        EcmaValue throwTypeError = this.Realm.GetRuntimeObject(WellKnownObject.ThrowTypeError);
        DefineOwnPropertyNoChecked(WellKnownProperty.Callee, new EcmaPropertyDescriptor(throwTypeError, throwTypeError, EcmaPropertyAttributes.None));
      } else {
        DefineOwnPropertyNoChecked(WellKnownProperty.Callee, new EcmaPropertyDescriptor(invocation.FunctionObject, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
      DefineOwnPropertyNoChecked(WellKnownProperty.Length, new EcmaPropertyDescriptor(arguments.Length, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      DefineOwnPropertyNoChecked(WellKnownSymbol.Iterator, new EcmaPropertyDescriptor(RuntimeRealm.Current.GetRuntimeObject(WellKnownObject.ArrayPrototype).Get(WellKnownSymbol.Iterator), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
    }

    public int Count {
      get { return arguments.Length; }
    }

    public RuntimeFunction Callee {
      get { return invocation.FunctionObject; }
    }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.Arguments; }
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      IEnumerable<EcmaPropertyKey> ownKeys = base.GetOwnPropertyKeys();
      if (taintedLevel != null) {
        return ownKeys;
      }
      IEnumerable<EcmaPropertyKey> indexes = Enumerable.Range(0, arguments.Length).Select(v => new EcmaPropertyKey(v));
      return indexes.Concat(ownKeys);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      EcmaPropertyDescriptor current = base.GetOwnProperty(propertyKey);
      if (EcmaValueUtility.TryIndexByPropertyKey(arguments, propertyKey, out EcmaValue value)) {
        if (taintedLevel == null) {
          return new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.DefaultDataProperty);
        }
        if (taintedLevel[propertyKey.ToArrayIndex()] == TaintLevel.AttributeTainted) {
          current.Value = value;
        }
      }
      return current;
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      return (taintedLevel == null && IsBoundedVariable(propertyKey)) || base.HasProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (IsBoundedVariable(propertyKey)) {
        int index = (int)propertyKey.ToArrayIndex();
        EnsureTainted();
        if (taintedLevel[index] != TaintLevel.BindingTainted) {
          if (descriptor.HasValue) {
            arguments[index] = descriptor.Value;
          }
          taintedLevel[index] = !descriptor.IsAccessorDescriptor && (!descriptor.HasWritable || descriptor.Writable) ? TaintLevel.AttributeTainted : TaintLevel.BindingTainted;
        }
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (IsBoundedVariable(propertyKey)) {
        EnsureTainted();
        taintedLevel[propertyKey.ToArrayIndex()] = TaintLevel.BindingTainted;
      }
      return base.Delete(propertyKey);
    }

    private void EnsureTainted() {
      if (taintedLevel == null) {
        for (int i = 0, len = arguments.Length; i < len; i++) {
          DefineOwnPropertyNoChecked(i, new EcmaPropertyDescriptor(arguments[i], EcmaPropertyAttributes.DefaultDataProperty));
        }
        taintedLevel = new TaintLevel[arguments.Length];
      }
    }

    private bool IsBoundedVariable(EcmaPropertyKey propertyKey) {
      return propertyKey.IsArrayIndex && propertyKey.ToArrayIndex() < arguments.Length;
    }

    #region Interfaces
    bool ICollection<EcmaValue>.IsReadOnly {
      get { return true; }
    }

    int IList<EcmaValue>.IndexOf(EcmaValue item) {
      for (int i = 0, len = arguments.Length; i < len; i++) {
        if (arguments[i].Equals(item, EcmaValueComparison.SameValue)) {
          return i;
        }
      }
      return -1;
    }

    void IList<EcmaValue>.Insert(int index, EcmaValue item) {
      throw new InvalidOperationException();
    }

    void IList<EcmaValue>.RemoveAt(int index) {
      throw new InvalidOperationException();
    }

    void ICollection<EcmaValue>.Add(EcmaValue item) {
      throw new InvalidOperationException();
    }

    void ICollection<EcmaValue>.Clear() {
      throw new InvalidOperationException();
    }

    bool ICollection<EcmaValue>.Contains(EcmaValue item) {
      return arguments.Contains(item, EcmaValueEqualityComparer.SameValue);
    }

    void ICollection<EcmaValue>.CopyTo(EcmaValue[] array, int arrayIndex) {
      arguments.CopyTo(array, arrayIndex);
    }

    bool ICollection<EcmaValue>.Remove(EcmaValue item) {
      throw new InvalidOperationException();
    }

    IEnumerator<EcmaValue> IEnumerable<EcmaValue>.GetEnumerator() {
      return arguments.OfType<EcmaValue>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return arguments.GetEnumerator();
    }
    #endregion
  }
}
