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
    public static string Stringify(EcmaValue value, EcmaValue replacer, EcmaValue space) {
      string indentString;
      if (space.Type == EcmaValueType.Object) {
        if (EcmaValueUtility.IsIntrinsicPrimitiveValue(space, EcmaValueType.Number)) {
          space = space.ToNumber();
        } else if (EcmaValueUtility.IsIntrinsicPrimitiveValue(space, EcmaValueType.String)) {
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
      HashSet<string> propertyList = new HashSet<string>();
      if (EcmaArray.IsArray(replacer)) {
        for (long i = 0, length = replacer[WellKnownPropertyName.Length].ToLength(); i < length; i++) {
          EcmaValue item = replacer[i];
          switch (item.Type) {
            case EcmaValueType.String:
            case EcmaValueType.Number:
              propertyList.Add(item.ToString());
              break;
            case EcmaValueType.Object:
              if (EcmaValueUtility.IsIntrinsicPrimitiveValue(value, EcmaValueType.String) ||
                  EcmaValueUtility.IsIntrinsicPrimitiveValue(value, EcmaValueType.Number)) {
                propertyList.Add(item.ToString());
              }
              break;
          }
        }
      }
      return JsonConvert.SerializeObject(value.GetUnderlyingObject(), new EcmaValueJsonConverter(indentString, new List<string>(propertyList)));
    }

    public static EcmaValue Parse(string value) {
      return Parse(value, EcmaValue.Undefined);
    }

    [IntrinsicMember]
    public static EcmaValue Parse(string value, EcmaValue reviver) {
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
        RuntimeObject obj = value.ToRuntimeObject();
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
