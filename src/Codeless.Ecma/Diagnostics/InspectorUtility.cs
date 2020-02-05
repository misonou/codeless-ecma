using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public static class InspectorUtility {
    [ThreadStatic]
    private static InspectorSerializer defaultConsoleWriter;

    public static string GetObjectTag(RuntimeObject value) {
      if (value is RuntimeObjectProxy) {
        return InternalString.ObjectTag.Proxy;
      }
      if (value is TypedArray arr) {
        return TypedArrayInfo.GetTypedArrayName(arr.ArrayKind);
      }
      EcmaValue name = default;
      try {
        name = value[WellKnownSymbol.ToStringTag];
      } catch { }
      try {
        if (name == default) {
          EcmaValue ctor = value[WellKnownProperty.Constructor];
          if (!ctor.IsNullOrUndefined && !ctor.ToObject().IsWellknownObject(WellKnownObject.ObjectConstructor)) {
            name = ctor[WellKnownProperty.Name];
          }
        }
      } catch { }
      return name == default ? "" : name.ToString();
    }

    public static string WriteValue(EcmaValue value) {
      StringBuilder sb = new StringBuilder();
      using (InspectorSerializer instance = new InspectorSingleLineSerializer(new StringWriter(sb))) {
        instance.Serialize(value);
      }
      return sb.ToString();
    }

    public static void WriteValue(TextWriter writer, EcmaValue value) {
      using (InspectorSerializer instance = new InspectorSingleLineSerializer(writer)) {
        instance.Serialize(value);
      }
    }

    public static void WriteConsole(EcmaValue value) {
      if (defaultConsoleWriter == null) {
        defaultConsoleWriter = new InspectorSingleLineSerializer(Console.Out);
      }
      defaultConsoleWriter.Serialize(value);
    }

    public static void WriteConsole(EcmaValue value, int maxDepth) {
      using (InspectorSerializer instance = new InspectorRecursiveSerializer(Console.Out, maxDepth)) {
        instance.Serialize(value);
      }
    }
  }
}
