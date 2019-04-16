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

    [IntrinsicMember]
    public static string Stringify(EcmaValue value, EcmaValue replacer = default, EcmaValue space = default) {
      string indentString;
      if (space.Type == EcmaValueType.Object) {
        if (space.IsIntrinsicPrimitiveValue(EcmaValueType.Number)) {
          space = space.ToNumber();
        } else if (space.IsIntrinsicPrimitiveValue(EcmaValueType.String)) {
          space = space.ToString();
        }
      }
      if (space.Type == EcmaValueType.Number) {
        indentString = new String(' ', Math.Max(0, Math.Min(10, space.ToInt32())));
      } else if (space.Type == EcmaValueType.String) {
        indentString = space.ToString();
        if (indentString.Length > 10) {
          indentString = indentString.Substring(0, 10);
        }
      } else {
        indentString = String.Empty;
      }
      if (replacer.IsCallable) {
        return JsonConvert.SerializeObject(value.GetUnderlyingObject(), new EcmaValueJsonConverter(indentString, replacer));
      }
      if (!EcmaArray.IsArray(replacer)) {
        return JsonConvert.SerializeObject(value.GetUnderlyingObject(), new EcmaValueJsonConverter(indentString));
      }
      HashSet<string> propertyList = new HashSet<string>();
      if (EcmaArray.IsArray(replacer)) {
        for (long i = 0, length = replacer[WellKnownPropertyName.Length].ToLength(); i < length; i++) {
          EcmaValue item = replacer[i];
          if (item.IsIntrinsicPrimitiveValue(EcmaValueType.String) ||
              item.IsIntrinsicPrimitiveValue(EcmaValueType.Number)) {
            propertyList.Add(item.ToString());
          }
        }
      }
      return JsonConvert.SerializeObject(value.GetUnderlyingObject(), new EcmaValueJsonConverter(indentString, new List<string>(propertyList)));
    }

    [IntrinsicMember]
    public static EcmaValue Parse(string value, EcmaValue reviver = default) {
      EcmaValue unfiltered = new EcmaValue(JsonConvert.DeserializeObject(value));
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
          foreach (EcmaPropertyKey key in value.EnumerateKeys()) {
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
