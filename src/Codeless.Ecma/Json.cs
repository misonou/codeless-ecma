using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Json, Global = true, Name = "JSON")]
  public static class Json {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.Json;

    public static string Stringify(EcmaValue value) {
      return new EcmaJsonWriter().Serialize(value);
    }

    public static string Stringify(EcmaValue value, RuntimeFunction replacer, string indentString) {
      return new EcmaJsonWriter(indentString, replacer).Serialize(value);
    }

    public static string Stringify(EcmaValue value, IList<string> propertyList, string indentString) {
      return new EcmaJsonWriter(indentString, propertyList).Serialize(value);
    }

    [IntrinsicMember]
    public static EcmaValue Stringify(EcmaValue value, EcmaValue replacer, EcmaValue space) {
      string indentString;
      space = EcmaValueUtility.UnboxPrimitiveObject(space);
      if (space.Type == EcmaValueType.Number) {
        indentString = new String(' ', Math.Max(0, Math.Min(10, space.ToInt32())));
      } else if (space.Type == EcmaValueType.String) {
        indentString = space.ToString(true);
      } else {
        indentString = String.Empty;
      }
      string result;
      if (replacer.IsCallable) {
        result = new EcmaJsonWriter(indentString, replacer).Serialize(value);
      } else if (EcmaArray.IsArray(replacer)) {
        HashSet<string> propertyList = new HashSet<string>();
        for (long i = 0, length = replacer[WellKnownPropertyName.Length].ToLength(); i < length; i++) {
          EcmaValue item = EcmaValueUtility.UnboxPrimitiveObject(replacer[i]);
          if (item.Type == EcmaValueType.String || item.Type == EcmaValueType.Number) {
            propertyList.Add(item.ToString(true));
          }
        }
        result = new EcmaJsonWriter(indentString, propertyList).Serialize(value);
      } else {
        result = new EcmaJsonWriter(indentString).Serialize(value);
      }
      return result ?? EcmaValue.Undefined;
    }

    public static EcmaValue Parse(string value) {
      return new EcmaJsonReader(value).Deserialize();
    }

    [IntrinsicMember]
    public static EcmaValue Parse(EcmaValue value, EcmaValue reviver) {
      EcmaValue unfiltered = Parse(value.ToString(true));
      if (!reviver.IsCallable) {
        return unfiltered;
      }
      RuntimeObject root = new EcmaObject();
      root.CreateDataPropertyOrThrow(String.Empty, unfiltered);
      return InternalizeJsonProperty(root, String.Empty, reviver);
    }

    [EcmaSpecification("InternalizeJSONProperty", EcmaSpecificationKind.RuntimeSemantics)]
    private static EcmaValue InternalizeJsonProperty(RuntimeObject holder, EcmaPropertyKey property, EcmaValue reviver) {
      EcmaValue value = holder.Get(property);
      if (value.Type == EcmaValueType.Object) {
        RuntimeObject obj = value.ToObject();
        if (EcmaArray.IsArray(value)) {
          for (long i = 0, length = value[WellKnownPropertyName.Length].ToLength(); i < length; i++) {
            EcmaValue newValue = InternalizeJsonProperty(obj, i, reviver);
            if (newValue.Type == EcmaValueType.Undefined) {
              obj.Delete(i);
            } else {
              obj.CreateDataProperty(i, newValue);
            }
          }
        } else {
          foreach (EcmaPropertyKey key in obj.GetEnumerableOwnPropertyKeys().ToArray()) {
            EcmaValue newValue = InternalizeJsonProperty(obj, key, reviver);
            if (newValue.Type == EcmaValueType.Undefined) {
              obj.Delete(key);
            } else {
              obj.CreateDataProperty(key, newValue);
            }
          }
        }
      }
      return reviver.Call(holder, property.ToValue(), value);
    }
  }
}
