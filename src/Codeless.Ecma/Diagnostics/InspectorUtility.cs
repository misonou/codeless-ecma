using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public static class InspectorUtility {
    public static string WriteValue(EcmaValue value) {
      switch(value.Type) {
        case EcmaValueType.Object:
          return WriteValue(value.ToRuntimeObject());
        case EcmaValueType.String:
          return "\"" + value.ToString().Replace("\"", "\\\"") + "\"";
      }
      return value.ToString();
    }

    public static string WriteValue(RuntimeObject value) {
      IEcmaIntrinsicObject t = value as IEcmaIntrinsicObject;
      if (t != null) {
        EcmaValue d = t.IntrinsicValue;
        if (d.Type != EcmaValueType.Object) {
          return WriteValue(t.IntrinsicValue);
        }
      }
      TextWriter writer = new StringWriter();
      Serialize(writer, new EcmaValue(value), false);
      return writer.ToString();
    }

    public static string GetObjectPrototypeTag(EcmaValue obj) {
      return GetObjectPrototypeTag(obj.ToRuntimeObject());
    }

    public static string GetObjectPrototypeTag(RuntimeObject obj) {
      if (obj.HasProperty(WellKnownSymbol.ToStringTag)) {
        return (string)obj.Get(WellKnownSymbol.ToStringTag);
      }
      for (; obj != null; obj = obj.GetPrototypeOf()) {
        if (obj.GetOwnProperty(WellKnownPropertyName.Constructor) != null) {
          EcmaValue v = obj.Get(WellKnownPropertyName.Constructor);
          if (v.IsCallable && v.HasProperty(WellKnownPropertyName.Name)) {
            return (string)v[WellKnownPropertyName.Name];
          }
        }
      }
      return InternalString.ObjectTag.Object;
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
          } else if (nested) {
            writer.Write(GetObjectPrototypeTag(value));
          } else if (value.IsArrayLike) {
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
            writer.Write(GetObjectPrototypeTag(value));
            writer.Write(" {");
            foreach (EcmaPropertyEntry item in value.EnumerateEntries()) {
              if (++count > 1) {
                writer.Write(", ");
              }
              writer.Write(item.Key.ToString());
              writer.Write(": ");
              Serialize(writer, item.Value, true);
            }
            writer.Write("}");
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
