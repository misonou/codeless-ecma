using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ArrayRS : TestBase {
    [Test]
    public void SetLength() {
      EcmaArray arr = new EcmaArray();
      Assume.That(arr.Length, Is.EqualTo(0));
      arr.Length = 3;
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));
      That(!arr.HasProperty(0));
      That(!arr.HasProperty(1));
      That(!arr.HasProperty(2));

      // should delete properties whose name is an array index and is larger than new length
      arr = new EcmaArray(1, 2, 3);
      arr.Length = 2;
      That(arr.ToValue(), Is.EquivalentTo(new[] { 1, 2 }));
      That(!arr.HasProperty(2));

      // should have changed if a property is added whose name is an array index
      arr = new EcmaArray();
      Assume.That(arr.Length, Is.EqualTo(0));
      arr[0] = 1;
      That(arr.ToValue(), Is.EquivalentTo(new[] { 1 }));
      arr[1] = 2;
      That(arr.ToValue(), Is.EquivalentTo(new[] { 1, 2 }));

      arr = new EcmaArray();
      arr[-1] = 1;
      That(arr.Length, Is.EqualTo(0));
      arr[true] = 1;
      That(arr.Length, Is.EqualTo(0));
      arr[CreateObject(valueOf: () => 3)] = 1;
      That(arr.Length, Is.EqualTo(0));

      arr["1"] = 1;
      That(arr.Length, Is.EqualTo(2));
      arr[Number.Construct(2)] = 2;
      That(arr.Length, Is.EqualTo(3));
      arr[String.Construct("3")] = 2;
      That(arr.Length, Is.EqualTo(4));

      arr[4294967294] = 4294967294;
      That(arr.Length, Is.EqualTo(4294967295));
      arr[4294967295] = 4294967295;
      That(arr.Length, Is.EqualTo(4294967295));

      arr = new EcmaArray();
      That(() => arr["length"] = -1, Throws.RangeError);
      That(() => arr["length"] = 4294967295, Throws.Nothing);
      That(() => arr["length"] = 4294967296, Throws.RangeError);
      That(() => arr["length"] = 4294967297, Throws.RangeError);
      That(() => arr["length"] = 1.5, Throws.RangeError);
      That(() => arr["length"] = NaN, Throws.RangeError);
      That(() => arr["length"] = Infinity, Throws.RangeError);
      That(() => arr["length"] = -Infinity, Throws.RangeError);
      That(() => arr["length"] = Undefined, Throws.RangeError);

      arr["length"] = true;
      That(arr.Length, Is.EqualTo(1));
      arr["length"] = Null;
      That(arr.Length, Is.EqualTo(0));
      arr["length"] = Boolean.Construct(false);
      That(arr.Length, Is.EqualTo(0));
      arr["length"] = Number.Construct(1);
      That(arr.Length, Is.EqualTo(1));
      arr["length"] = "1";
      That(arr.Length, Is.EqualTo(1));
      arr["length"] = String.Construct("1");
      That(arr.Length, Is.EqualTo(1));

      arr["length"] = CreateObject(valueOf: () => 2);
      That(arr.Length, Is.EqualTo(2));
      arr["length"] = CreateObject(valueOf: () => 2, toString: () => 1);
      That(arr.Length, Is.EqualTo(2));
      arr["length"] = CreateObject(valueOf: () => 2, toString: () => new EcmaObject());
      That(arr.Length, Is.EqualTo(2));
      arr["length"] = CreateObject(valueOf: () => 2, toString: ThrowTest262Exception);
      That(arr.Length, Is.EqualTo(2));

      arr["length"] = CreateObject(toString: () => 1);
      That(arr.Length, Is.EqualTo(1));
      arr["length"] = CreateObject(toString: () => 1, valueOf: () => new EcmaObject());
      That(arr.Length, Is.EqualTo(1));

      That(() => arr["length"] = CreateObject(valueOf: ThrowTest262Exception, toString: () => 1), Throws.Test262);
      That(() => arr["length"] = CreateObject(valueOf: () => new EcmaObject(), toString: () => new EcmaObject()), Throws.TypeError);
    }

    [Test]
    public void SparseArray() {
      EcmaArray arr = new EcmaArray();

      // create new chunks
      arr[1] = 1;
      arr[4] = 4;
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, 1, Undefined, Undefined, 4 }));
      That(!arr.HasProperty(0));
      That(!arr.HasProperty(2));
      That(!arr.HasProperty(3));

      // add item at the end of a chunk
      arr[5] = 5;
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, 1, Undefined, Undefined, 4, 5 }));
      That(arr.HasProperty(5));

      // add item at the beginning of a chunk
      arr[3] = 3;
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, 1, Undefined, 3, 4, 5 }));
      That(arr.HasProperty(3));

      // merge chunks
      arr[2] = 2;
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, 1, 2, 3, 4, 5 }));
      That(arr.HasProperty(2));

      // become a non-chunked array
      arr[0] = 0;
      That(arr.ToValue(), Is.EquivalentTo(new[] { 0, 1, 2, 3, 4, 5 }));
      That(arr.HasProperty(0));

      // becoma a chunked array
      arr.Delete(1);
      That(arr.ToValue(), Is.EquivalentTo(new[] { 0, Undefined, 2, 3, 4, 5 }));
      That(!arr.HasProperty(1));

      // delete item at the beginning of a chunk
      arr.Delete(2);
      That(arr.ToValue(), Is.EquivalentTo(new[] { 0, Undefined, Undefined, 3, 4, 5 }));
      That(!arr.HasProperty(2));

      // delete item at the end of a chunk
      arr.Delete(5);
      That(arr.ToValue(), Is.EquivalentTo(new[] { 0, Undefined, Undefined, 3, 4, Undefined }));
      That(!arr.HasProperty(5));

      // remove chunk
      arr.Delete(0);
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined, Undefined, Undefined, 3, 4, Undefined }));
      That(!arr.HasProperty(0));

      // no chunk should be created
      arr = new EcmaArray();
      arr[0] = 0;
      That(arr.ToValue(), Is.EquivalentTo(new[] { 0 }));
      arr.Delete(0);
      That(arr.ToValue(), Is.EquivalentTo(new[] { Undefined }));

      arr = new EcmaArray(5);
      arr[0] = 0;
      arr[4] = 4;
    }
  }
}
