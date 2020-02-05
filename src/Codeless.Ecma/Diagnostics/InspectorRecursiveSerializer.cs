using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Diagnostics {
  internal class InspectorRecursiveSerializer : InspectorSerializer {
    private readonly List<RuntimeObject> stack = new List<RuntimeObject>();
    private readonly InspectorSerializer singleLine;
    private readonly int maxDepth;

    public InspectorRecursiveSerializer(TextWriter writer, int maxDepth)
      : base(writer) {
      this.maxDepth = maxDepth;
      this.singleLine = new InspectorSingleLineSerializer(writer);
    }

    public override void Serialize(EcmaValue value) {
      stack.Clear();
      base.Serialize(value);
    }

    protected override void WriteObject(EcmaValue value) {
      if (value.IsCallable) {
        WriteToken(InspectorTokenType.Function, value.ToString());
      } else {
        base.WriteObject(value);
      }
    }

    protected override void WriteObjectBody(RuntimeObject obj) {
      InspectorMetaObject meta = new InspectorMetaObject(obj);
      stack.Add(obj);
      foreach (InspectorMetaObjectEntry e in meta.EnumerableProperties) {
        WriteProperty(e.Key, new EcmaPropertyDescriptor(e.Value, EcmaPropertyAttributes.Enumerable));
      }
      foreach (InspectorMetaObjectEntry e in meta.NonEnumerableProperties) {
        WriteProperty(e.Key, new EcmaPropertyDescriptor(e.Value, EcmaPropertyAttributes.None));
      }
      stack.Remove(obj);
      if (this.LastToken != InspectorTokenType.ObjectStart) {
        WriteToken(InspectorTokenType.NewLine);
        WriteToken(InspectorTokenType.Space, new String('\t', stack.Count));
      }
    }

    protected override void WritePropertyName(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      WriteToken(InspectorTokenType.NewLine);
      WriteToken(InspectorTokenType.Space, new String('\t', stack.Count));
      base.WritePropertyName(propertyKey, descriptor);
    }

    protected override void WritePropertyValue(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (descriptor.Value.Type != EcmaValueType.Object) {
        base.WritePropertyValue(propertyKey, descriptor);
      } else {
        RuntimeObject obj = descriptor.Value.ToObject();
        if (stack.Count == maxDepth || stack.Contains(obj)) {
          WriteToken(InspectorTokenType.UnexpandedAccessor);
        } else if (propertyKey == "__proto__") {
          singleLine.Serialize(descriptor.Value);
        } else {
          base.WritePropertyValue(propertyKey, descriptor);
        }
      }
    }
  }
}
