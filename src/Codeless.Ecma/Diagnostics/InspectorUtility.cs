using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
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
          if (value.GetUnderlyingObject() is RuntimeObjectProxy) {
            writer.Write("Proxy {}");
          } else if (value.IsCallable) {
            if (nested) {
              writer.Write("function");
            } else {
              writer.Write(FunctionPrototype.ToString(value));
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
              ctor = value[WellKnownProperty.Constructor];
            } catch { }
            if (!nested) {
              try {
                TextWriter writer2 = new StringWriter();
                RuntimeObject obj = value.ToObject();
                if (!ctor.IsNullOrUndefined) {
                  writer2.Write(ctor[WellKnownProperty.Name]);
                  if (obj is EcmaMapBase collection) {
                    writer2.Write("(");
                    writer2.Write(collection.Size);
                    writer2.Write(")");
                  }
                  writer2.Write(" ");
                }
                writer2.Write("{");
                if (obj is EcmaSet set) {
                  set.ForEach((v, k) => {
                    if (++count > 1) {
                      writer2.Write(", ");
                    }
                    Serialize(writer2, v, true);
                  });
                } else if (obj is EcmaMap map) {
                  map.ForEach((v, k) => {
                    if (++count > 1) {
                      writer2.Write(", ");
                    }
                    Serialize(writer2, k, true);
                    writer2.Write(" => ");
                    Serialize(writer2, v, true);
                  });
                } else if (obj is Promise promise) {
                  switch (promise.State) {
                    case PromiseState.Pending:
                      writer2.Write("<pending>");
                      break;
                    case PromiseState.Fulfilled:
                      writer2.Write("<resolved>: ");
                      Serialize(writer2, promise.Value, true);
                      break;
                    case PromiseState.Rejected:
                      writer2.Write("<rejected>: ");
                      Serialize(writer2, promise.Value, true);
                      break;
                  }
                } else {
                  foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
                    EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
                    if (descriptor != null && descriptor.IsDataDescriptor && descriptor.Enumerable) {
                      if (++count > 1) {
                        writer2.Write(", ");
                      }
                      writer2.Write(propertyKey.ToString());
                      writer2.Write(": ");
                      Serialize(writer2, descriptor.Value, true);
                    }
                  }
                }
                writer2.Write("}");
                writer.Write(writer2.ToString());
                break;
              } catch { }
            }
            writer.Write(ctor.IsNullOrUndefined ? "Object" : ctor[WellKnownProperty.Name]);
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
        long length = value[WellKnownProperty.Length].ToLength();
        long prevIndex = -1;
        writer2.Write("[");
        foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
          EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
          if (descriptor == null) {
            continue;
          }
          long index = propertyKey.IsArrayIndex ? propertyKey.ToArrayIndex() : -1;
          if (index >= 0 && index < length) {
            WriteEmpty(writer2, index, prevIndex, ref count);
            if (++count > 1) {
              writer2.Write(", ");
            }
            if (descriptor.IsAccessorDescriptor) {
              writer2.Write("(...)");
            } else {
              Serialize(writer2, descriptor.Value, true);
            }
            prevIndex = index;
          } else {
            WriteEmpty(writer2, length, prevIndex, ref count);
            prevIndex = length;
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
        WriteEmpty(writer2, length, prevIndex, ref count);
        writer2.Write("]");
        writer.Write(writer2.ToString());
      } catch {
        writer.Write("[...]");
      }
    }

    private static void WriteEmpty(TextWriter writer, long cur, long prev, ref int count) {
      long diff = cur - prev;
      if (diff > 1) {
        if (++count > 1) {
          writer.Write(", ");
        }
        if (diff == 2) {
          writer.Write("empty");
        } else {
          writer.Write("empty × ");
          writer.Write(diff - 1);
        }
      }
    }
  }
}
