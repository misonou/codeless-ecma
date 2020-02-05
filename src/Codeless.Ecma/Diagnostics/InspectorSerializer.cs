using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma.Diagnostics {
  internal enum InspectorTokenType {
    Undefined,
    Null,
    Boolean,
    BigInt,
    Number,
    String,
    Symbol,
    Date,
    RegExp,
    Function,
    PropertyName,
    NonEnumerablePropertyName,
    ObjectStateName,
    ObjectTag,
    ObjectStart,
    ObjectEnd,
    ArrayStart,
    ArrayEnd,
    ArrayElision,
    ArrayLength,
    EscapedCharacter,
    EntrySeparator,
    PropertyNameValueSeparator,
    MapKeyValueSeparator,
    UnexpandedAccessor,
    Ellipsis,
    Space,
    NewLine
  }

  internal abstract class InspectorSerializer : IDisposable {
    private readonly bool isConsoleOutput;
    private readonly TextWriter writer;
    private ConsoleColor beforeColor;
    private bool disposed;

    public InspectorSerializer(TextWriter writer) {
      Guard.ArgumentNotNull(writer, "writer");
      this.writer = writer;
      this.isConsoleOutput = writer == Console.Out;
    }

    protected InspectorTokenType LastToken { get; private set; }

    public virtual void Serialize(EcmaValue value) {
      try {
        if (isConsoleOutput) {
          beforeColor = Console.ForegroundColor;
        }
        WriteValue(value);
      } catch { }
      if (isConsoleOutput) {
        Console.ForegroundColor = beforeColor;
        writer.Write(Environment.NewLine);
      }
      writer.Flush();
    }

    protected virtual void WriteToken(InspectorTokenType token, string content) {
      if (content.Length > 0) {
        this.LastToken = token;
        if (isConsoleOutput) {
          Console.ForegroundColor = GetColor(token);
        }
        writer.Write(content);
      }
    }

    protected virtual void WriteToken(InspectorTokenType token) {
      switch (token) {
        case InspectorTokenType.ObjectStart:
          WriteToken(token, "{");
          return;
        case InspectorTokenType.ObjectEnd:
          WriteToken(token, "}");
          return;
        case InspectorTokenType.ArrayStart:
          WriteToken(token, "[");
          return;
        case InspectorTokenType.ArrayEnd:
          WriteToken(token, "]");
          return;
        case InspectorTokenType.EntrySeparator:
          WriteToken(token, ", ");
          return;
        case InspectorTokenType.MapKeyValueSeparator:
          WriteToken(token, " => ");
          return;
        case InspectorTokenType.PropertyNameValueSeparator:
          WriteToken(token, ": ");
          return;
        case InspectorTokenType.UnexpandedAccessor:
          WriteToken(token, "(...)");
          return;
        case InspectorTokenType.Undefined:
          WriteToken(token, "undefined");
          return;
        case InspectorTokenType.Null:
          WriteToken(token, "null");
          return;
        case InspectorTokenType.Space:
          WriteToken(token, " ");
          return;
        case InspectorTokenType.Ellipsis:
          WriteToken(token, "\u2026");
          return;
        case InspectorTokenType.NewLine:
          WriteToken(token, Environment.NewLine);
          return;
      }
    }

    protected virtual void WriteValue(EcmaValue value) {
      switch (value.Type) {
        case EcmaValueType.Undefined:
          WriteToken(InspectorTokenType.Undefined);
          return;
        case EcmaValueType.Null:
          WriteToken(InspectorTokenType.Null);
          return;
        case EcmaValueType.BigInt:
          WriteBigInt(value);
          return;
        case EcmaValueType.Boolean:
          WriteBoolean(value);
          return;
        case EcmaValueType.Number:
          WriteNumber(value);
          return;
        case EcmaValueType.Object:
          WriteObject(value);
          return;
        case EcmaValueType.String:
          WriteString(value);
          return;
        case EcmaValueType.Symbol:
          WriteSymbol(value);
          return;
      }
    }

    protected virtual void WriteBoolean(EcmaValue value) {
      WriteToken(InspectorTokenType.Boolean, value.ToString());
    }

    protected virtual void WriteBigInt(EcmaValue value) {
      WriteToken(InspectorTokenType.BigInt, value.ToString() + "n");
    }

    protected virtual void WriteNumber(EcmaValue value) {
      WriteToken(InspectorTokenType.Number, value.Equals(EcmaValue.NegativeZero, EcmaValueComparison.SameValue) ? "-0" : value.ToString());
    }

    protected virtual void WriteString(EcmaValue value) {
      string content = EscapeString(value.ToString());
      if (isConsoleOutput) {
        int lastIndex = 0;
        foreach (Match m in Regex.Matches(content, "\\\\.")) {
          if (m.Index > lastIndex) {
            WriteToken(InspectorTokenType.String, content.Substring(lastIndex, m.Index - lastIndex));
          }
          WriteToken(InspectorTokenType.EscapedCharacter, m.Value);
          lastIndex = m.Index + m.Value.Length;
        }
        if (content.Length > lastIndex) {
          WriteToken(InspectorTokenType.String, content.Substring(lastIndex));
        }
      } else {
        WriteToken(InspectorTokenType.String, content);
      }
    }

    protected virtual void WriteSymbol(EcmaValue value) {
      WriteToken(InspectorTokenType.Symbol, value.ToString());
    }

    protected virtual void WriteObject(EcmaValue value) {
      RuntimeObject obj = value.ToObject();
      InspectorTokenType last = this.LastToken;
      WriteObjectTag(obj);
      if (this.LastToken != last) {
        WriteToken(InspectorTokenType.Space);
      }
      WriteToken(InspectorTokenType.ObjectStart);
      WriteObjectBody(obj);
      WriteToken(InspectorTokenType.ObjectEnd);
    }

    protected virtual void WriteObjectTag(RuntimeObject obj) {
      WriteToken(InspectorTokenType.ObjectTag, InspectorUtility.GetObjectTag(obj));
    }

    protected abstract void WriteObjectBody(RuntimeObject obj);

    protected virtual void WriteObjectState(string state) {
      if (this.LastToken != InspectorTokenType.ObjectStart && this.LastToken != InspectorTokenType.ArrayStart) {
        WriteToken(InspectorTokenType.EntrySeparator);
      }
      WriteToken(InspectorTokenType.ObjectStateName, state);
    }

    protected virtual void WriteObjectState(string state, EcmaValue value) {
      if (this.LastToken != InspectorTokenType.ObjectStart) {
        WriteToken(InspectorTokenType.EntrySeparator);
      }
      WriteToken(InspectorTokenType.ObjectStateName, state);
      WriteToken(InspectorTokenType.PropertyNameValueSeparator);
      WriteValue(value);
    }

    protected virtual void WriteProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (this.LastToken != InspectorTokenType.ObjectStart && this.LastToken != InspectorTokenType.ArrayStart) {
        WriteToken(InspectorTokenType.EntrySeparator);
      }
      WritePropertyName(propertyKey, descriptor);
      WriteToken(InspectorTokenType.PropertyNameValueSeparator);
      if (descriptor.IsAccessorDescriptor) {
        WriteToken(InspectorTokenType.UnexpandedAccessor);
      } else {
        WritePropertyValue(propertyKey, descriptor);
      }
    }

    protected virtual void WritePropertyName(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (descriptor.Enumerable) {
        WriteToken(InspectorTokenType.PropertyName, propertyKey.ToString());
      } else {
        WriteToken(InspectorTokenType.NonEnumerablePropertyName, propertyKey.ToString());
      }
    }

    protected virtual void WritePropertyValue(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      WriteValue(descriptor.Value);
    }

    protected virtual void Dispose() {
      if (!disposed) {
        disposed = true;
        Dispose();
      }
    }

    protected static ConsoleColor GetColor(InspectorTokenType token) {
      switch (token) {
        case InspectorTokenType.ObjectStart:
        case InspectorTokenType.ObjectEnd:
        case InspectorTokenType.ArrayStart:
        case InspectorTokenType.ArrayEnd:
        case InspectorTokenType.PropertyName:
        case InspectorTokenType.PropertyNameValueSeparator:
        case InspectorTokenType.MapKeyValueSeparator:
        case InspectorTokenType.ObjectStateName:
          return ConsoleColor.White;
        case InspectorTokenType.BigInt:
        case InspectorTokenType.Number:
        case InspectorTokenType.String:
          return ConsoleColor.DarkGreen;
        case InspectorTokenType.Symbol:
        case InspectorTokenType.EscapedCharacter:
          return ConsoleColor.Magenta;
        case InspectorTokenType.Boolean:
          return ConsoleColor.Cyan;
        case InspectorTokenType.Function:
        case InspectorTokenType.RegExp:
        case InspectorTokenType.Date:
          return ConsoleColor.DarkCyan;
        case InspectorTokenType.Null:
        case InspectorTokenType.Undefined:
        case InspectorTokenType.ArrayElision:
        case InspectorTokenType.NonEnumerablePropertyName:
          return ConsoleColor.DarkGray;
      }
      return ConsoleColor.Gray;
    }

    protected static string EscapeString(string str) {
      string encoded = Regex.Replace(str, "[\r\n\t\\\"]", m => {
        switch (m.Value) {
          case "\r":
            return "\\r";
          case "\n":
            return "\\n";
          case "\t":
            return "\\t";
          case "\"":
            return "\\\"";
          case "\\":
            return "\\\\";
        }
        return m.Value;
      });
      return "\"" + encoded + "\"";
    }

    void IDisposable.Dispose() {
      Dispose();
    }
  }
}
