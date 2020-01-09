using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class SymbolBinder : PrimitiveBinder<Symbol> {
    public static readonly SymbolBinder Default = new SymbolBinder();

    protected SymbolBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Symbol; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Symbol; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Invalid; }
    }

    public override Symbol FromHandle(EcmaValueHandle handle) {
      return (Symbol)handle.GetTargetAsGCHandle();
    }

    public override EcmaValueHandle ToHandle(Symbol value) {
      throw new InvalidOperationException();
    }

    public override RuntimeObject ToRuntimeObject(Symbol value) {
      return new TransientPrimitiveObject(new EcmaValue(value), WellKnownObject.SymbolPrototype);
    }

    public override bool ToBoolean(Symbol value) {
      return true;
    }

    public override int ToInt32(Symbol value) {
      throw new EcmaTypeErrorException(InternalString.Error.SymbolNotConvertibleToNumber);
    }

    public override long ToInt64(Symbol value) {
      throw new EcmaTypeErrorException(InternalString.Error.SymbolNotConvertibleToNumber);
    }

    public override double ToDouble(Symbol value) {
      throw new EcmaTypeErrorException(InternalString.Error.SymbolNotConvertibleToNumber);
    }

    public override string ToString(Symbol value) {
      throw new EcmaTypeErrorException(InternalString.Error.SymbolNotConvertibleToString);
    }

    public override bool TryGet(Symbol target, EcmaPropertyKey name, out EcmaValue value) {
      value = ToRuntimeObject(target).Get(name);
      return true;
    }
  }
}
