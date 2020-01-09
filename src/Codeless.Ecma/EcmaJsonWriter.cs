using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal class EcmaJsonWriter {
    private readonly string indentString;
    private readonly EcmaValue replacer;
    private readonly ICollection<string> propertyList;

    public EcmaJsonWriter() {
      this.indentString = String.Empty;
    }

    public EcmaJsonWriter(string indentString) {
      Guard.ArgumentNotNull(indentString, "indentString");
      if (indentString.Length > 10) {
        indentString = indentString.Substring(0, 10);
      }
      this.indentString = indentString;
    }

    public EcmaJsonWriter(string indentString, EcmaValue replacer)
      : this(indentString) {
      this.replacer = replacer;
    }

    public EcmaJsonWriter(string indentString, ICollection<string> propertyList)
      : this(indentString) {
      this.propertyList = propertyList;
    }

    public string Serialize(EcmaValue value) {
      StringBuilder sb = new StringBuilder();
      Serialize(value, new StringWriter(sb));
      return sb.Length == 0 ? null : sb.ToString();
    }

    public void Serialize(EcmaValue value, Stream stream) {
      Serialize(value, new StreamWriter(stream));
    }

    public void Serialize(EcmaValue value, TextWriter writer) {
      RuntimeObject holder = new EcmaObject();
      holder.CreateDataProperty(String.Empty, new EcmaValue(value));
      SerializeJsonProperty(writer, new Stack<RuntimeObject>(), holder, String.Empty);
    }

    private void SerializeJsonProperty(TextWriter writer, Stack<RuntimeObject> stack, RuntimeObject holder, EcmaPropertyKey property) {
      EcmaValue value = TransformValue(holder, property, holder.Get(property));
      if (value.Type != EcmaValueType.Undefined) {
        WritePropertyValue(writer, stack, value);
      }
    }

    private void SerializeJsonObject(TextWriter writer, Stack<RuntimeObject> stack, RuntimeObject value) {
      bool hasEntries = false;
      string prependString = String.Join("", stack.Select(v => indentString).ToArray());

      if (stack.Contains(value)) {
        throw new EcmaTypeErrorException(InternalString.Error.CircularJsonObject);
      }
      stack.Push(value);
      writer.Write("{");

      List<EcmaPropertyEntry> entries = new List<EcmaPropertyEntry>();
      if (propertyList != null) {
        entries.AddRange(propertyList.Select(v => new EcmaPropertyEntry(v, value.Get(v))));
      } else {
        entries.AddRange(value.GetEnumerableOwnProperties(false));
      }
      foreach (EcmaPropertyEntry e in entries) {
        EcmaValue v = TransformValue(value, e.Key, e.Value);
        if (v.Type != EcmaValueType.Undefined) {
          if (hasEntries) {
            writer.Write(",");
          }
          hasEntries = true;
          if (indentString != String.Empty) {
            writer.Write(Environment.NewLine);
            writer.Write(prependString);
            writer.Write(indentString);
          }
          WriteString(writer, e.Key.ToString());
          writer.Write(":");
          if (indentString != String.Empty) {
            writer.Write(" ");
          }
          WritePropertyValue(writer, stack, v);
        }
      }
      if (hasEntries && indentString != String.Empty) {
        writer.Write(Environment.NewLine);
        writer.Write(prependString);
      }
      writer.Write("}");
      stack.Pop();
    }

    private void SerializeJsonArray(TextWriter writer, Stack<RuntimeObject> stack, RuntimeObject value) {
      if (stack.Contains(value)) {
        throw new EcmaTypeErrorException(InternalString.Error.CircularJsonObject);
      }
      string prependString = String.Join("", stack.Select(v => indentString).ToArray());
      stack.Push(value);
      writer.Write("[");

      long length = value.Get(WellKnownProperty.Length).ToLength();
      for (long i = 0; i < length; i++) {
        if (i > 0) {
          writer.Write(",");
        }
        if (indentString != String.Empty) {
          writer.Write(Environment.NewLine);
          writer.Write(prependString);
          writer.Write(indentString);
        }
        EcmaValue item = value.Get(i);
        if (item.Type == EcmaValueType.Undefined) {
          writer.Write("null");
        } else {
          SerializeJsonProperty(writer, stack, value, i);
        }
      }
      if (length > 0 && indentString != String.Empty) {
        writer.Write(Environment.NewLine);
        writer.Write(prependString);
      }
      writer.Write("]");
      stack.Pop();
    }

    private void WritePropertyValue(TextWriter writer, Stack<RuntimeObject> stack, EcmaValue value) {
      switch (value.Type) {
        case EcmaValueType.Null:
          writer.Write("null");
          break;
        case EcmaValueType.Boolean:
          writer.Write(value.ToBoolean() ? "true" : "false");
          break;
        case EcmaValueType.Number:
          writer.Write(value.IsFinite ? value.ToString() : "null");
          break;
        case EcmaValueType.String:
          WriteString(writer, value.ToString());
          break;
        case EcmaValueType.Object:
          if (!value.IsCallable) {
            if (EcmaArray.IsArray(value)) {
              SerializeJsonArray(writer, stack, value.ToObject());
            } else {
              SerializeJsonObject(writer, stack, value.ToObject());
            }
          }
          break;
      }
    }

    [EcmaSpecification("QuoteJSONString", EcmaSpecificationKind.RuntimeSemantics)]
    private void WriteString(TextWriter writer, string str) {
      writer.Write("\"");
      for (int i = 0, len = str.Length; i < len; i++) {
        switch (str[i]) {
          case '\b':
            writer.Write("\\b");
            continue;
          case '\t':
            writer.Write("\\t");
            continue;
          case '\n':
            writer.Write("\\n");
            continue;
          case '\f':
            writer.Write("\\f");
            continue;
          case '\r':
            writer.Write("\\r");
            continue;
          case '"':
            writer.Write("\\\"");
            continue;
          case '\\':
            writer.Write("\\\\");
            continue;
        }
        if (Char.IsSurrogatePair(str, i)) {
          writer.Write(str.Substring(i++, 2));
        } else if (str[i] < 32 || Char.IsSurrogate(str, i)) {
          writer.Write("\\u" + ((int)str[i]).ToString("x4"));
        } else {
          writer.Write(str.Substring(i, 1));
        }
      }
      writer.Write("\"");
    }

    private EcmaValue TransformValue(RuntimeObject holder, EcmaPropertyKey property, EcmaValue value) {
      if (value.Type == EcmaValueType.Object) {
        EcmaValue toJson = value[WellKnownProperty.ToJSON];
        if (toJson.IsCallable) {
          value = toJson.Call(value, property.ToValue());
        }
      }
      if (replacer != EcmaValue.Undefined) {
        value = replacer.Call(holder, property.ToValue(), value);
      }
      if (value.Type == EcmaValueType.Object) {
        EcmaValue primitive = EcmaValueUtility.UnboxPrimitiveObject(value);
        switch (primitive.Type) {
          case EcmaValueType.Number:
          case EcmaValueType.String:
          case EcmaValueType.Boolean:
            return primitive;
        }
      }
      return value;
    }
  }
}
