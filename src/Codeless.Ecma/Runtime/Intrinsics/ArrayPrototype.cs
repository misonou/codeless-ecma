using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayPrototype)]
  internal static class ArrayPrototype {
    [IntrinsicMember(EcmaPropertyAttributes.Writable)]
    public const int Length = 0;

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Join([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Pop([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Push([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Reverse([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Shift([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Unshift([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Splice([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Sort([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Filter([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Some([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Every([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Map([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue IndexOf([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue LastIndexOf([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Reduce([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue ReduceRight([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue CopyWithin([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Find([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue FindIndex([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Fill([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Includes([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Entries([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue ForEach([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisArg) { throw new NotImplementedException(); }
    [IntrinsicMember]
    public static EcmaValue Concat([This] EcmaValue thisArg) { throw new NotImplementedException(); }
  }
}
