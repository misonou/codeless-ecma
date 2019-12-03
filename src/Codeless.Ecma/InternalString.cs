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
      public const string Array = "Array";
      public const string ArrayIterator = "Array Iterator";
      public const string WeakSet = "WeakSet";
      public const string Set = "Set";
      public const string WeakMap = "WeakMap";
      public const string Map = "Map";
      public const string MapIterator = "Map Iterator";
      public const string SetIterator = "Set Iterator";
      public const string RegExpStringIterator = "RegExp String Iterator";
      public const string StringIterator = "String Iterator";
      public const string Null = "Null";
    }

    public static class Error {
      public const string NotObject = "#<Object> is not an Object";
      public const string NotConstructor = "#<Object> is not a constructor";
      public const string NotFunction = "#<Object> is not a function";
      public const string NotSymbol = "#<Object> is not a symbol";
      public const string NotExtensible = "#<Object> is not extensible";
      public const string NotIterable = "#<Object> is not iterable";
      public const string NotCoercibleAsObject = "Cannot convert value to an Object";
      public const string NotConvertibleToPrimitive = "Cannot convert object to primitive value";
      public const string SymbolNotConvertibleToNumber = "Cannot convert a Symbol value to a number";
      public const string SymbolNotConvertibleToString = "Cannot convert a Symbol value to a string";
      public const string CreatePropertyThrow = "Cannot create property on #<Object>";
      public const string DeletePropertyThrow = "Cannot delete property on #<Object>";
      public const string PrototypeMustBeObjectOrNull = "Object prototype may only be an Object or null";
      public const string PreventExtensionFailed = "Cannot prevent extensions to #<Object>";
      public const string SetPropertyFailed = "Cannot set property {0} of #<Object>";
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
      public const string InvalidArrayLength = "Invalid array length";
      public const string InvalidIndex = "Invalid index";
      public const string InvalidTimeValue = "Invalid time value";
      public const string InvalidRegex = "Invalid regular expression";
      public const string InvalidRegexFlags = "Invalid regular expression flags";
      public const string StrictMode = "'caller', 'callee', and 'arguments' properties may not be accessed on strict mode functions or the arguments objects for calls to them";
      public const string InvalidHint = "Invalid hint";
      public const string LastIndexNotWritable = "Cannot assign to read only property 'lastIndex' of object";
      public const string RegExpDuplicatedNameGroup = "Duplicate capture group name";
      public const string RegExpInvalidCodePoint = "Unicode codepoint must not be greater than 0x10FFFF in regular expression";
      public const string RegExpExecReturnWrongType = "RegExp exec method returned something other than an Object or null";
      public const string UnexpectedJSONEnd = "Unexpected end of JSON input";
      public const string UnexpectedJSONToken = "Unexpected token {0} in JSON at position {1}";
      public const string MalformedURI = "URI malformed";
      public const string EntryNotObject = "Iterator value is not an entry object";
      public const string ExceedMaximumLength = "Exceed maximum length";
      public const string ReduceEmptyArray = "Reduce of empty array with no initial value";
    }
  }
}
