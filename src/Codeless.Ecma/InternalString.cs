using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal static class InternalString {
    public static class TypeOf {
      public const string Undefined = "undefined";
      public const string Object = "object";
      public const string Boolean = "boolean";
      public const string Number = "number";
      public const string String = "string";
      public const string Symbol = "symbol";
      public const string Function = "function";
    }

    public static class ObjectTag {
      public const string Undefined = "Undefined";
      public const string Object = "Object";
      public const string Boolean = "Boolean";
      public const string Number = "Number";
      public const string String = "String";
      public const string Symbol = "Symbol";
      public const string Function = "Function";
      public const string Date = "Date";
      public const string RegExp = "RegExp";
      public const string Json = "JSON";
      public const string Arguments = "Arguments";
    }

    public static class Error {
      public const string NotConvertibleToPrimitive = "Cannot convert object to primitive value";
      public const string NotFunction = "{0} is not a function";
      public const string SetProperty = "Unable to set";
      public const string IncompatibleObject = "Incompatible object";
      public const string NotCoercibleAsObject = "";
      public const string CreatePropertyThrow = "";
      public const string DeletePropertyThrow = "";
      public const string PrototypeNotObject = "Object prototype may only be an Object or null";
      public const string SymbolNotConvertibleToNumber = "Cannot convert a Symbol value to a number";
    }
  }
}
