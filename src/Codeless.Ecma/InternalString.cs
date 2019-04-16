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
      public const string ArrayIterator = "Array Iterator";
      public const string WeakSet = "WeakSet";
      public const string Set = "Set";
      public const string WeakMap = "WeakMap";
      public const string Map = "Map";
      public const string MapIterator = "Map Iterator";
      public const string SetIterator = "Set Iterator";
    }

    public static class Error {
      public const string NotObject = "#<Object> is not an Object";
      public const string NotConstructor = "#<Object> is not a constructor";
      public const string NotFunction = "#<Object> is not a function";
      public const string NotExtensible = "#<Object> is not extensible";
      public const string NotCoercibleAsObject = "Cannot convert value to an Object";
      public const string NotConvertibleToPrimitive = "Cannot convert object to primitive value";
      public const string SymbolNotConvertibleToNumber = "Cannot convert a Symbol value to a number";
      public const string CreatePropertyThrow = "Cannot create property on #<Object>";
      public const string DeletePropertyThrow = "Cannot delete property on #<Object>";
      public const string PrototypeMustBeObjectOrNull = "Object prototype may only be an Object or null";
      public const string PreventExtensionFailed = "Cannot prevent extensions to #<Object>";
      public const string SetPrototypeFailed = "Cannot set prototype of #<Object>";
      public const string SetPropertyNullOrUndefined = "Cannot set property '#<Property>' on null or undefined";
      public const string MustCallAsConstructor = "Constructor #<Object> requires 'new'";
      public const string IncompatibleObject = "Incompatible object";
      public const string IllegalInvocation = "Illegal invocation";
      public const string InvalidTrapResult = "Trap function returned result that is inconsistent with the target object";
      public const string ProxyRevoked = "Cannot perform operation on a proxy that has been revoked";
      public const string TargetOrHandlerRevoked = "Cannot create proxy with a revoked proxy as target or handler";
      public const string TargetOrHandlerNotObject = "Cannot create proxy with a non-object as target or handler";
      public const string CircularJsonObject = "Converting circular structure to JSON";
      public const string InvalidDescriptor = "Invalid property descriptor. Cannot both specify accessors and a value or writable attribute";
      public const string GetterMustBeFunction = "Getter must be a function";
      public const string SetterMustBeFunction = "Setter must be a function";
      public const string InfinityToInteger = "Infinity out of range";
      public const string InvalidArrayLength = "Invalid array length";
    }
  }
}
