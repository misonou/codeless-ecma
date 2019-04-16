using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public static class InspectorUtility {
    public static string WriteValue(EcmaValue value) {
      switch (value.Type) {
        case EcmaValueType.Object:
          return WriteValue(value.ToObject());
        case EcmaValueType.String:
          return "\"" + value.ToString().Replace("\"", "\\\"") + "\"";
      }
      return value.ToString();
    }

    public static string WriteValue(RuntimeObject value) {
      if (value == null) {
        return WriteValue(EcmaValue.Undefined);
      }
      if (value is IntrinsicObject t) {
        return WriteValue(t.IntrinsicValue);
      }
      TextWriter writer = new StringWriter();
      Serialize(writer, value, false);
      return writer.ToString();
    }

    private static void Serialize(TextWriter writer, EcmaValue value, bool nested) {
      switch (value.Type) {
        case EcmaValueType.Object:
          int count = 0;
          if (value.IsCallable) {
            if (nested) {
              writer.Write("function");
            } else {
              writer.Write("function " + value["name"] + "() { [native code] }");
            }
          } else if (EcmaArray.IsArray(value)) {
            writer.Write("[");
            foreach (EcmaPropertyEntry item in value.EnumerateEntries()) {
              if (++count > 1) {
                writer.Write(", ");
              }
              if (!item.Key.IsArrayIndex) {
                writer.Write(item.Key.ToString());
                writer.Write(": ");
              }
              Serialize(writer, item.Value, true);
            }
            writer.Write("]");
          } else {
            EcmaValue ctor = value["constructor"];
            if (!nested) {
              try {
                TextWriter writer2 = new StringWriter();
                RuntimeObject obj = value.ToObject();
                if (!ctor.IsNullOrUndefined) {
                  writer2.Write(ctor["name"]);
                  if (obj is EcmaMapBase collection) {
                    writer2.Write("(");
                    writer2.Write(collection.Size);
                    writer2.Write(")");
                  }
                  writer2.Write(" ");
                }
                writer2.Write("{");
                foreach (EcmaPropertyKey propertyKey in obj.OwnPropertyKeys) {
                  EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
                  if (descriptor.IsDataDescriptor && descriptor.Enumerable.Value) {
                    if (++count > 1) {
                      writer2.Write(", ");
                    }
                    writer2.Write(propertyKey.ToString());
                    writer2.Write(": ");
                    Serialize(writer2, descriptor.Value, true);
                  }
                }
                writer2.Write("}");
                writer.Write(writer2.ToString());
                break;
              } catch { }
            }
            writer.Write(ctor.IsNullOrUndefined ? "Object" : ctor["name"]);
          }
          break;
        case EcmaValueType.String:
          writer.Write("\"" + value.ToString().Replace("\"", "\\\"") + "\"");
          break;
        default:
          writer.Write(value.ToString());
          break;
      }
    }
  }
}
