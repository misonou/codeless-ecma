using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class EcmaTimestampBinder : InternalDataBinder<EcmaTimestamp> {
    public static readonly EcmaTimestampBinder Default = new EcmaTimestampBinder();

    private EcmaTimestampBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Date; }
    }
  }
}
