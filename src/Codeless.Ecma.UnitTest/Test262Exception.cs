using System;
using System.Runtime.Serialization;

namespace Codeless.Ecma.UnitTest {
  [Serializable]
  internal class Test262Exception : Exception {
    public Test262Exception() { }

    public Test262Exception(string message)
      : base(message) { }

    public Test262Exception(string message, Exception innerException)
      : base(message, innerException) { }

    protected Test262Exception(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
  }
}