using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal class EcmaPropertyKeyCollection : Collection<EcmaPropertyKey> {
    private int stringStartKey;
    private int symbolStartKey;

    protected override void InsertItem(int index, EcmaPropertyKey item) {
      if (index != this.Count || Contains(item)) {
        throw new InvalidOperationException();
      }
      if (item.IsSymbol) {
        base.InsertItem(index, item);
      } else if (item.IsArrayIndex) {
        int i = 0;
        long v = item.ArrayIndex;
        for (; i < v && i < stringStartKey; i++) ;
        base.InsertItem(i, item);
        stringStartKey++;
        symbolStartKey++;
      } else {
        base.InsertItem(symbolStartKey, item);
        symbolStartKey++;
      }
    }

    protected override void RemoveItem(int index) {
      base.RemoveItem(index);
      if (index < stringStartKey) {
        stringStartKey--;
      }
      if (index < symbolStartKey) {
        symbolStartKey--;
      }
    }

    protected override void SetItem(int index, EcmaPropertyKey item) {
      throw new InvalidOperationException();
    }

    protected override void ClearItems() {
      base.ClearItems();
      stringStartKey = 0;
      symbolStartKey = 0;
    }
  }
}
