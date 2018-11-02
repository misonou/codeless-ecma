using Codeless.Ecma.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal class EcmaValueJsonConverter : JsonConverter {
    private readonly string indentString;
    private readonly EcmaValue replacer;
    private readonly IList<string> propertyList;

    public EcmaValueJsonConverter(string indentString, EcmaValue replacer) {
      this.indentString = indentString;
      this.replacer = replacer;
    }

    public EcmaValueJsonConverter(string indentString, IList<string> propertyList) {
      this.indentString = indentString;
      this.propertyList = propertyList;
    }

    public override bool CanConvert(Type objectType) {
      return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      RuntimeObject o = new EcmaObject();
      o.CreateDataProperty(String.Empty, new EcmaValue(value));
      SerializeJsonProperty(writer, new Stack<RuntimeObject>(), o, String.Empty);
    }

    private void SerializeJsonProperty(JsonWriter writer, Stack<RuntimeObject> stack, RuntimeObject holder, EcmaPropertyKey property) {
      EcmaValue value = TransformValue(holder, property, holder.Get(property));
      WritePropertyValue(writer, stack, value);
    }

    private void SerializeJsonObject(JsonWriter writer, Stack<RuntimeObject> stack, RuntimeObject value) {
      bool hasEntries = false;
      string prependString = String.Join("", stack.Select(v => indentString));

      if (stack.Contains(value)) {
        throw new EcmaTypeErrorException("Cyclical");
      }
      stack.Push(value);
      writer.WriteStartObject();

      List<EcmaPropertyEntry> entries = new List<EcmaPropertyEntry>();
      if (propertyList != null) {
        entries.AddRange(propertyList.Select(v => new EcmaPropertyEntry(v, value.Get(v))));
      } else {
        entries.AddRange(value.GetEnumerableOwnProperties());
      }


      foreach (EcmaPropertyEntry e in entries) {
        EcmaValue v = TransformValue(value, e.Key, e.Value);
        if (v.Type != EcmaValueType.Undefined) {
          hasEntries = true;
          if (indentString != String.Empty) {
            writer.WriteRaw(Environment.NewLine);
            writer.WriteRaw(prependString);
            writer.WriteRaw(indentString);
          }
          writer.WritePropertyName(e.Key.ToString());
          writer.WriteRaw(":");
          if (indentString != String.Empty) {
            writer.WriteRaw(" ");
          }
          WritePropertyValue(writer, stack, v);
        }
      }
      if (hasEntries && indentString != String.Empty) {
        writer.WriteRaw(Environment.NewLine);
        writer.WriteRaw(prependString);
      }
      writer.WriteEndObject();
      stack.Pop();
    }

    private void SerializeJsonArray(JsonWriter writer, Stack<RuntimeObject> stack, RuntimeObject value) {
      if (stack.Contains(value)) {
        throw new EcmaTypeErrorException("Cyclical");
      }
      string prependString = String.Join("", stack.Select(v => indentString));
      stack.Push(value);
      writer.WriteStartArray();

      long length = value.Get(WellKnownPropertyName.Length).ToLength();
      for (long i = 0; i < length; i++) {
        if (i > 0) {
          writer.WriteRaw(",");
        }
        if (indentString != String.Empty) {
          writer.WriteRaw(Environment.NewLine);
          writer.WriteRaw(prependString);
          writer.WriteRaw(indentString);
        }
        EcmaValue item = value.Get(i);
        if (item.Type == EcmaValueType.Undefined) {
          writer.WriteNull();
        } else {
          SerializeJsonProperty(writer, stack, value, i);
        }
      }
      if (length > 0 && indentString != String.Empty) {
        writer.WriteRaw(Environment.NewLine);
        writer.WriteRaw(prependString);
      }
      writer.WriteEndArray();
      stack.Pop();
    }

    private void WritePropertyValue(JsonWriter writer, Stack<RuntimeObject> stack, EcmaValue value) {
      switch (value.Type) {
        case EcmaValueType.Null:
          writer.WriteNull();
          break;
        case EcmaValueType.Boolean:
          writer.WriteValue(value.ToBoolean());
          break;
        case EcmaValueType.Number:
          if (value.IsFinite) {
            writer.WriteRawValue(value.ToString());
          } else {
            writer.WriteNull();
          }
          break;
        case EcmaValueType.String:
          writer.WriteValue((string)value.GetUnderlyingObject());
          break;
        case EcmaValueType.Object:
          if (!value.IsCallable) {
            if (EcmaArray.IsArray(value)) {
              SerializeJsonArray(writer, stack, value.ToRuntimeObject());
            } else {
              SerializeJsonObject(writer, stack, value.ToRuntimeObject());
            }
          }
          break;
      }
    }

    private EcmaValue TransformValue(RuntimeObject holder, EcmaPropertyKey property, EcmaValue value) {
      if (value.Type == EcmaValueType.Object) {
        EcmaValue toJson = value[WellKnownPropertyName.ToJson];
        if (toJson.IsCallable) {
          value = toJson.Call(value, property.ToValue());
        }
      }
      if (replacer != EcmaValue.Undefined) {
        value = replacer.Call(holder, property.ToValue(), value);
      }
      if (value.Type == EcmaValueType.Object) {
        if (EcmaValueUtility.IsIntrinsicPrimitiveValue(value, EcmaValueType.Number)) {
          value = value.ToNumber();
        } else if (EcmaValueUtility.IsIntrinsicPrimitiveValue(value, EcmaValueType.String)) {
          value = value.ToString();
        } else if (EcmaValueUtility.IsIntrinsicPrimitiveValue(value, EcmaValueType.Boolean)) {
          value = value.ToBoolean();
        }
      }
      return value;
    }
  }
}
