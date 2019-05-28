using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Codeless.Ecma {
  internal class EcmaJsonReader : IDisposable {
    private readonly TextReader reader;
    private int pos;
    private int ch;

    public EcmaJsonReader(string str) {
      this.reader = new StringReader(str);
    }

    public EcmaJsonReader(Stream stream) {
      this.reader = new StreamReader(stream);
    }

    public EcmaValue Deserialize() {
      if (ReadToken() == -1) {
        throw GetException();
      }
      EcmaValue value = ReadValue();
      if (ch != -1 && ReadToken() != -1) {
        throw GetException();
      }
      return value;
    }

    private int ReadChar() {
      ch = reader.Read();
      pos++;
      return ch;
    }

    private bool ReadChar(char ch) {
      return ReadChar() == ch;
    }

    private int ReadToken() {
      int ch;
      while ((ch = ReadChar()) != -1 && (ch == '\n' || ch == '\r' || ch == '\t' || ch == ' ')) ;
      return ch;
    }

    private int ReadWhitespace() {
      int ch = this.ch;
      while ((ch == '\n' || ch == '\r' || ch == '\t' || ch == ' ') && (ch = ReadChar()) != -1) ;
      return ch;
    }

    private EcmaValue ReadValue() {
      switch (ch) {
        case '{':
          return ReadObject();
        case '[':
          return ReadArray();
        case '"':
          return ReadString();
        case 'f':
        case 't':
          return ReadBoolean();
        case 'n':
          return ReadNull();
        case '-':
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          return ReadNumber();
      }
      throw GetException();
    }

    private EcmaObject ReadObject() {
      EcmaObject obj = new EcmaObject();
      if (ReadToken() != '}') {
        do {
          if (ch != '"') {
            throw GetException();
          }
          string key = ReadString();
          if (ch != ':' || ReadToken() == -1) {
            throw GetException();
          }
          obj.CreateDataPropertyOrThrow(key, ReadValue());
          switch (ch) {
            case '}':
              goto end;
            case ',':
              continue;
            default:
              throw GetException();
          }
        } while (ReadToken() != -1);
      }
      end:
      ReadToken();
      return obj;
    }

    private EcmaArray ReadArray() {
      List<EcmaValue> list = new List<EcmaValue>();
      if (ReadToken() != ']') {
        do {
          list.Add(ReadValue());
          switch (ch) {
            case ']':
              goto end;
            case ',':
              continue;
            default:
              throw GetException();
          }
        } while (ReadToken() != -1);
      }
      end:
      ReadToken();
      return new EcmaArray(list);
    }

    private EcmaValue ReadNull() {
      if (ReadChar('u') && ReadChar('l') && ReadChar('l')) {
        ReadToken();
        return EcmaValue.Null;
      }
      throw GetException();
    }

    private bool ReadBoolean() {
      switch (ch) {
        case 'f':
          if (ReadChar('a') && ReadChar('l') && ReadChar('s') && ReadChar('e')) {
            ReadToken();
            return false;
          }
          break;
        case 't':
          if (ReadChar('r') && ReadChar('u') && ReadChar('e')) {
            ReadToken();
            return true;
          }
          break;
      }
      throw GetException();
    }

    private double ReadNumber() {
      int sign = ch == '-' ? -1 : 1;
      if (sign < 0 && ReadChar() == -1) {
        throw GetException();
      }
      double num = DecDigitToInt(ch);
      if (num != 0) {
        while (IsDigit(ReadChar())) {
          num = num * 10 + (ch - '0');
        }
      } else {
        ReadChar();
      }
      if (ch == '.') {
        int cur = pos;
        double dp = DecDigitToInt(ReadChar());
        while (IsDigit(ReadChar())) {
          dp = dp * 10 + (ch - '0');
        }
        num += dp * Math.Pow(10, cur - pos + 1);
      }
      num *= sign;
      if (ch != 'e' && ch != 'E') {
        ReadWhitespace();
        return num;
      }

      sign = ReadChar() == '-' ? -1 : 1;
      if (sign < 0 && ReadChar() == -1) {
        throw GetException();
      }
      int exp = DecDigitToInt(ch);
      if (exp != 0) {
        while (IsDigit(ReadChar())) {
          exp = exp * 10 + (ch - '0');
        }
      }
      ReadWhitespace();
      return num * Math.Pow(10, sign * exp);
    }

    private string ReadString() {
      StringBuilder sb = new StringBuilder();
      int ch;
      while ((ch = ReadChar()) != -1) {
        if (ch < 0x20) {
          throw GetException();
        }
        if (ch == '\\') {
          switch (ReadChar()) {
            case 'u':
              sb.Append((char)((HexDigitToInt(ReadChar()) << 12) + (HexDigitToInt(ReadChar()) << 8) + (HexDigitToInt(ReadChar()) << 4) + HexDigitToInt(ReadChar())));
              continue;
            case '/':
              sb.Append('/');
              continue;
            case '\\':
              sb.Append('\\');
              continue;
            case '"':
              sb.Append('"');
              continue;
            case 't':
              sb.Append('\t');
              continue;
            case 'r':
              sb.Append('\r');
              continue;
            case 'n':
              sb.Append('\n');
              continue;
            case 'f':
              sb.Append('\f');
              continue;
            case 'b':
              sb.Append('\b');
              continue;
          }
          throw GetException();
        }
        if (ch == '"') {
          ReadToken();
          return sb.ToString();
        }
        sb.Append((char)ch);
      }
      throw GetException();
    }

    private bool IsDigit(int ch) {
      return ch >= '0' && ch <= '9';
    }

    private int DecDigitToInt(int ch) {
      if (ch >= '0' && ch <= '9') {
        return ch - '0';
      }
      throw GetException();
    }

    private int HexDigitToInt(int ch) {
      if (ch >= '0' && ch <= '9') {
        return ch - '0';
      }
      if (ch >= 'A' && ch <= 'F') {
        return ch - 'A' + 10;
      }
      if (ch >= 'a' && ch <= 'f') {
        return ch - 'a' + 10;
      }
      throw GetException();
    }

    private Exception GetException() {
      if (ch == -1) {
        return new EcmaSyntaxErrorException(InternalString.Error.UnexpectedJSONEnd);
      }
      return new EcmaSyntaxErrorException(InternalString.Error.UnexpectedJSONToken, (char)ch, pos);
    }

    public void Dispose() {
      reader.Dispose();
    }
  }
}
