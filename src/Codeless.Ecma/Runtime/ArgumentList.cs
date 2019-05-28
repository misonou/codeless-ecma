using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Runtime {
  public class ArgumentList : RuntimeObject, IList<EcmaValue> {
    private static readonly RuntimeFunction throwStrictMode = RuntimeFunction.Create(ThrowStrictMode);
    private readonly RuntimeFunctionInvocation invocation;
    private readonly EcmaValue[] arguments;

    public ArgumentList(RuntimeFunctionInvocation invocation, EcmaValue[] arguments)
      : base(WellKnownObject.ObjectPrototype) {
      Guard.ArgumentNotNull(invocation, "invocation");
      Guard.ArgumentNotNull(arguments, "arguments");
      this.invocation = invocation;
      this.arguments = arguments;
      if (invocation.FunctionObject.ContainsUseStrict) {
        DefineOwnPropertyNoChecked(WellKnownPropertyName.Callee, new EcmaPropertyDescriptor(throwStrictMode, throwStrictMode, EcmaPropertyAttributes.None));
      } else {
        DefineOwnPropertyNoChecked(WellKnownPropertyName.Callee, new EcmaPropertyDescriptor(invocation.Parent?.FunctionObject, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
      DefineOwnPropertyNoChecked(WellKnownSymbol.Iterator, new EcmaPropertyDescriptor(RuntimeRealm.GetRuntimeObject(WellKnownObject.ArrayPrototype).Get(WellKnownSymbol.Iterator), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
    }

    public EcmaValue this[int index] {
      get { return arguments[index]; }
      set { arguments[index] = value; }
    }

    public int Count {
      get { return arguments.Length; }
    }

    public RuntimeFunction Callee {
      get { return invocation.Parent?.FunctionObject; }
    }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.Arguments; }
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      IEnumerable<EcmaPropertyKey> ownKeys = base.GetOwnPropertyKeys();
      IEnumerable<EcmaPropertyKey> indexes = Enumerable.Range(0, arguments.Length).Select(v => new EcmaPropertyKey(v));
      if (ownKeys.Any()) {
        return indexes.Concat(new[] { new EcmaPropertyKey("length") });
      }
      return indexes.Concat(ownKeys);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (EcmaValueUtility.TryIndexByPropertyKey(arguments, propertyKey, out EcmaValue ch)) {
        return new EcmaPropertyDescriptor(ch, EcmaPropertyAttributes.Enumerable);
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return new EcmaPropertyDescriptor(arguments.Length, EcmaPropertyAttributes.None);
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex && propertyKey.ToArrayIndex() < arguments.Length) {
        return true;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return true;
      }
      return base.HasProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (this.IsExtensible) {
        if (base.GetOwnPropertyKeys().Any()) {
          DefineOwnPropertyNoChecked("length", GetOwnProperty("length"));
        }
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    private static EcmaValue ThrowStrictMode() {
      throw new EcmaTypeErrorException(InternalString.Error.StrictMode);
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
      throw new NotImplementedException();
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
