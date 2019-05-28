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
        case EcmaValueType.Number:
          if (value.Equals(EcmaValue.NegativeZero, EcmaValueComparison.SameValue)) {
            return "-0";
          }
          break;
      }
      return value.ToString();
    }

    public static string WriteValue(RuntimeObject value) {
      if (value == null) {
        return WriteValue(EcmaValue.Undefined);
      }
      if (value is PrimitiveObject t) {
        return WriteValue(t.PrimitiveValue);
      }
      if (value is EcmaRegExp || value is EcmaDate) {
        return value.ToString();
      }
      TextWriter writer = new StringWriter();
      Serialize(writer, value, false);
      return writer.ToString();
    }

    private static void Serialize(TextWriter writer, EcmaValue value, bool nested) {
      switch (value.Type) {
        case EcmaValueType.Object:
          if (value.IsCallable) {
            if (nested) {
              writer.Write("function");
            } else {
              writer.Write("function " + value["name"] + "() { [native code] }");
            }
          } else if (EcmaArray.IsArray(value)) {
            SerializeAsArray(writer, value);
          } else if (value.GetUnderlyingObject() is ArgumentList) {
            writer.Write("Arguments ");
            SerializeAsArray(writer, value);
          } else {
            int count = 0;
            EcmaValue ctor = default;
            try {
              ctor = value["constructor"];
            } catch { }
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
                foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
                  EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
                  if (descriptor.IsDataDescriptor && descriptor.Enumerable) {
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
        default:
          writer.Write(WriteValue(value));
          break;
      }
    }

    private static void SerializeAsArray(TextWriter writer, EcmaValue value) {
      TextWriter writer2 = new StringWriter();
      RuntimeObject obj = value.ToObject();
      int count = 0;
      try {
        writer2.Write("[");
        for (long i = 0, len = value["length"].ToLength(); i < len; i++) {
          if (++count > 1) {
            writer2.Write(", ");
          }
          EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(i);
          if (descriptor == null) {
            Serialize(writer2, EcmaValue.Undefined, true);
          } else if (descriptor.IsAccessorDescriptor) {
            writer2.Write("(...)");
          } else {
            Serialize(writer2, descriptor.Value, true);
          }
        }
        foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
          if (!propertyKey.IsArrayIndex) {
            EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
            if (descriptor.IsDataDescriptor && descriptor.Enumerable) {
              if (++count > 1) {
                writer2.Write(", ");
              }
              writer2.Write(propertyKey.ToString());
              writer2.Write(": ");
              Serialize(writer2, descriptor.Value, true);
            }
          }
        }
        writer2.Write("]");
        writer.Write(writer2.ToString());
      } catch {
        writer.Write("[...]");
      }
    }
  }
}
