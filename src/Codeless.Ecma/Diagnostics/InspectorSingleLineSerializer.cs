using Codeless.Ecma.Runtime;
using System;
using System.IO;

namespace Codeless.Ecma.Diagnostics {
  internal class InspectorSingleLineSerializer : InspectorSerializer {
    public InspectorSingleLineSerializer(TextWriter writer)
      : base(writer) { }

    protected override void WriteObject(EcmaValue value) {
      switch (value.ToObject()) {
        case PrimitiveObject t:
          WriteValue(t.PrimitiveValue);
          break;
        case EcmaRegExp re:
          WriteRegExp(re);
          break;
        case EcmaDate date:
          WriteDate(date);
          break;
        case TypedArray array:
          WriteTypedArray(array);
          break;
        case GeneratorBase generator:
          WriteGenerator(generator);
          break;
        case ArgumentsObject args:
          WriteArgumentsObject(args);
          break;
        case Promise promise:
          WritePromise(promise);
          break;
        case EcmaMap map:
          WriteMap(map);
          break;
        case EcmaSet set:
          WriteSet(set);
          break;
        case RuntimeFunction fn:
          WriteFunction(fn);
          break;
        case EcmaArray array:
          WriteArray(array);
          break;
        case RuntimeObjectProxy proxy:
          WriteObjectTag(proxy);
          WriteToken(InspectorTokenType.Space);
          WriteObjectBody(proxy.ProxyTarget);
          break;
        default:
          base.WriteObject(value);
          break;
      }
    }

    protected override void WriteObjectBody(RuntimeObject obj) {
      foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
        if (descriptor != null && descriptor.IsDataDescriptor && descriptor.Enumerable) {
          WriteProperty(propertyKey, descriptor);
        }
      }
    }

    protected override void WriteProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (this.LastToken != InspectorTokenType.Ellipsis) {
        base.WriteProperty(propertyKey, descriptor);
      }
    }

    protected override void WritePropertyValue(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      WriteValueOrObjectTag(descriptor.Value);
    }

    protected virtual void WriteValueOrObjectTag(EcmaValue value) {
      if (value.IsCallable) {
        WriteToken(InspectorTokenType.Function, "function");
      } else if (value.Type != EcmaValueType.Object || value.IsRegExp || EcmaArray.IsArray(value)) {
        WriteValue(value);
      } else {
        string objectTag = InspectorUtility.GetObjectTag(value.ToObject());
        if (String.IsNullOrEmpty(objectTag)) {
          objectTag = "Object";
        }
        WriteToken(InspectorTokenType.ObjectTag, objectTag);
      }
    }

    protected virtual void WriteArray(RuntimeObject obj) {
      WriteToken(InspectorTokenType.ArrayStart);
      try {
        long length = obj[WellKnownProperty.Length].ToLength();
        long prevIndex = -1;
        foreach (EcmaPropertyKey propertyKey in obj.GetOwnPropertyKeys()) {
          EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(propertyKey);
          if (descriptor == null) {
            continue;
          }
          long index = propertyKey.IsArrayIndex ? propertyKey.ToArrayIndex() : -1;
          if (index >= 0 && index < length) {
            if (index > prevIndex + 1) {
              WriteArrayElision(index - prevIndex - 1);
            }
            WriteArrayItem(propertyKey, descriptor);
            prevIndex = index;
          } else {
            if (length > prevIndex + 1) {
              WriteArrayElision(length - prevIndex - 1);
            }
            prevIndex = length;
            if (descriptor.Enumerable) {
              WriteProperty(propertyKey, descriptor);
            }
          }
          if (this.LastToken == InspectorTokenType.Ellipsis) {
            prevIndex = length;
            break;
          }
        }
        if (length > prevIndex + 1) {
          WriteArrayElision(length - prevIndex - 1);
        }
      } catch {
        WriteToken(InspectorTokenType.Ellipsis);
      }
      WriteToken(InspectorTokenType.ArrayEnd);
    }

    protected virtual void WriteArrayItem(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (this.LastToken != InspectorTokenType.ArrayStart) {
        WriteToken(InspectorTokenType.EntrySeparator);
      }
      if (descriptor.IsAccessorDescriptor) {
        WriteToken(InspectorTokenType.UnexpandedAccessor);
      } else {
        WritePropertyValue(propertyKey, descriptor);
      }
    }

    protected virtual void WriteArrayLength(long count) {
      WriteToken(InspectorTokenType.ArrayLength, "(" + count + ")");
    }

    protected virtual void WriteArrayElision(long count) {
      if (this.LastToken != InspectorTokenType.ArrayStart) {
        WriteToken(InspectorTokenType.EntrySeparator);
      }
      WriteToken(InspectorTokenType.ArrayElision, count > 1 ? "empty × " + count.ToString() : "empty");
    }

    protected virtual void WriteArgumentsObject(ArgumentsObject args) {
      WriteObjectTag(args);
      WriteToken(InspectorTokenType.Space);
      WriteArray(args);
    }

    protected virtual void WriteDate(EcmaDate date) {
      WriteToken(InspectorTokenType.Date, date.ToString());
    }

    protected virtual void WriteFunction(RuntimeFunction fn) {
      WriteToken(InspectorTokenType.Function, fn.Source);
    }

    protected virtual void WriteGenerator(GeneratorBase generator) {
      WriteObjectTag(generator);
      WriteToken(InspectorTokenType.Space);
      WriteToken(InspectorTokenType.ObjectStart);
      switch (generator.State) {
        case GeneratorState.Running:
          WriteObjectState("<running>");
          break;
        case GeneratorState.SuspendedStart:
        case GeneratorState.SuspendedYield:
          WriteObjectState("<suspended>");
          break;
        default:
          WriteObjectState("<closed>");
          break;
      }
      WriteObjectBody(generator);
      WriteToken(InspectorTokenType.ObjectEnd);
    }

    protected virtual void WriteMap(EcmaMap map) {
      WriteObjectTag(map);
      WriteArrayLength(map.Size);
      WriteToken(InspectorTokenType.Space);
      WriteToken(InspectorTokenType.ObjectStart);
      map.ForEach((v, k) => {
        if (this.LastToken != InspectorTokenType.ObjectStart) {
          WriteToken(InspectorTokenType.EntrySeparator);
        }
        WriteValueOrObjectTag(k);
        WriteToken(InspectorTokenType.MapKeyValueSeparator);
        WriteValueOrObjectTag(v);
      });
      WriteToken(InspectorTokenType.ObjectEnd);
    }

    protected virtual void WritePromise(Promise promise) {
      WriteObjectTag(promise);
      WriteToken(InspectorTokenType.Space);
      WriteToken(InspectorTokenType.ObjectStart);
      switch (promise.State) {
        case PromiseState.Pending:
          WriteObjectState("<pending>");
          break;
        case PromiseState.Fulfilled:
          WriteObjectState("<resolved>", promise.Value);
          break;
        case PromiseState.Rejected:
          WriteObjectState("<rejected>", promise.Value);
          break;
      }
      WriteObjectBody(promise);
      WriteToken(InspectorTokenType.ObjectEnd);
    }

    protected virtual void WriteRegExp(EcmaRegExp re) {
      WriteToken(InspectorTokenType.RegExp, re.ToString());
    }

    protected virtual void WriteSet(EcmaSet set) {
      WriteObjectTag(set);
      WriteArrayLength(set.Size);
      WriteToken(InspectorTokenType.Space);
      WriteToken(InspectorTokenType.ObjectStart);
      set.ForEach((v, k) => {
        if (this.LastToken != InspectorTokenType.ObjectStart) {
          WriteToken(InspectorTokenType.EntrySeparator);
        }
        WriteValueOrObjectTag(v);
      });
      WriteToken(InspectorTokenType.ObjectEnd);
    }

    protected virtual void WriteTypedArray(TypedArray array) {
      WriteObjectTag(array);
      WriteArrayLength(array.Length);
      WriteToken(InspectorTokenType.Space);
      WriteArray(array);
    }
  }
}
