using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  internal static class ArrayHelper {
    public static EcmaValue[] Slice(EcmaValue[] src, int index) {
      Guard.ArgumentNotNull(src, "src");
      int count = src.Length - index;
      if (count <= 0) {
        return EcmaValue.EmptyArray;
      }
      EcmaValue[] dst = new EcmaValue[count];
      Array.Copy(src, index, dst, 0, count);
      return dst;
    }

    public static EcmaValue[] Combine(EcmaValue[] a, EcmaValue[] b) {
      Guard.ArgumentNotNull(a, "a");
      Guard.ArgumentNotNull(b, "b");
      int len1 = a.Length;
      if (len1 == 0) {
        return b;
      }
      int len2 = b.Length;
      if (len2 == 0) {
        return a;
      }
      EcmaValue[] args = new EcmaValue[len1 + len2];
      Array.Copy(a, args, len1);
      Array.Copy(b, 0, args, len1, len2);
      return args;
    }

    public static long GetBoundIndex(EcmaValue index, long len, long defaultValue) {
      if (index == default) {
        return defaultValue;
      }
      index = index.ToInteger();
      return (index < 0 ? EcmaMath.Max(len + index, 0) : EcmaMath.Min(index, len)).ToInt64();
    }

    public static int GetBoundIndex(EcmaValue index, int len, int defaultValue) {
      if (index == default) {
        return defaultValue;
      }
      index = index.ToInteger();
      return (index < 0 ? EcmaMath.Max(len + index, 0) : EcmaMath.Min(index, len)).ToInt32();
    }

    public static int GetBoundIndexClamped(EcmaValue index, int len, int defaultValue) {
      if (index == default) {
        return defaultValue;
      }
      index = index.ToInteger();
      return (index < 0 ? 0 : EcmaMath.Min(index, len)).ToInt32();
    }
  }
}
