using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public sealed class IntlModule : RuntimeModule<IntlObjectKind> {
    public static readonly SharedObjectHandle Intl;
    public static readonly SharedObjectHandle Locale;
    public static readonly SharedObjectHandle LocalePrototype;
    public static readonly SharedObjectHandle Collator;
    public static readonly SharedObjectHandle CollatorPrototype;
    public static readonly SharedObjectHandle NumberFormat;
    public static readonly SharedObjectHandle NumberFormatPrototype;
    public static readonly SharedObjectHandle DateTimeFormat;
    public static readonly SharedObjectHandle DateTimeFormatPrototype;
    public static readonly SharedObjectHandle PluralRules;
    public static readonly SharedObjectHandle PluralRulesPrototype;
    public static readonly SharedObjectHandle ListFormat;
    public static readonly SharedObjectHandle ListFormatPrototype;
    public static readonly SharedObjectHandle RelativeTimeFormat;
    public static readonly SharedObjectHandle RelativeTimeFormatPrototype;

    static IntlModule() {
      IntlModule module = RuntimeRealm.RegisterModule<IntlModule>();
      Intl = module.GetSharedObjectHandle(IntlObjectKind.Intl);
      Locale = module.GetSharedObjectHandle(IntlObjectKind.Locale);
      LocalePrototype = module.GetSharedObjectHandle(IntlObjectKind.LocalePrototype);
      Collator = module.GetSharedObjectHandle(IntlObjectKind.Collator);
      CollatorPrototype = module.GetSharedObjectHandle(IntlObjectKind.CollatorPrototype);
      NumberFormat = module.GetSharedObjectHandle(IntlObjectKind.NumberFormat);
      NumberFormatPrototype = module.GetSharedObjectHandle(IntlObjectKind.NumberFormatPrototype);
      DateTimeFormat = module.GetSharedObjectHandle(IntlObjectKind.DateTimeFormat);
      DateTimeFormatPrototype = module.GetSharedObjectHandle(IntlObjectKind.DateTimeFormatPrototype);
      PluralRules = module.GetSharedObjectHandle(IntlObjectKind.PluralRules);
      PluralRulesPrototype = module.GetSharedObjectHandle(IntlObjectKind.PluralRulesPrototype);
      ListFormat = module.GetSharedObjectHandle(IntlObjectKind.ListFormat);
      ListFormatPrototype = module.GetSharedObjectHandle(IntlObjectKind.ListFormatPrototype);
      RelativeTimeFormat = module.GetSharedObjectHandle(IntlObjectKind.RelativeTimeFormat);
      RelativeTimeFormatPrototype = module.GetSharedObjectHandle(IntlObjectKind.RelativeTimeFormatPrototype);
    }

    protected override void OnAfterInitializing(IList<RuntimeObject> runtimeObject) {
      base.OnAfterInitializing(runtimeObject);

      RuntimeModule module = RuntimeRealm.IntrinsicModule;
      module.OverrideProperty(WellKnownObject.ArrayPrototype, "toLocaleString", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.ArrayPrototypeToLocaleString));
      module.OverrideProperty(WellKnownObject.DatePrototype, "toLocaleString", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.DatePrototypeToLocaleString));
      module.OverrideProperty(WellKnownObject.DatePrototype, "toLocaleDateString", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.DatePrototypeToLocaleDateString));
      module.OverrideProperty(WellKnownObject.DatePrototype, "toLocaleTimeString", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.DatePrototypeToLocaleTimeString));
      module.OverrideProperty(WellKnownObject.NumberPrototype, "toLocaleString", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.NumberPrototypeToLocaleString));
      module.OverrideProperty(WellKnownObject.StringPrototype, "localeCompare", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.StringPrototypeLocaleCompare));
      module.OverrideProperty(WellKnownObject.StringPrototype, "toLocaleLowerCase", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.StringPrototypeToLocaleLowerCase));
      module.OverrideProperty(WellKnownObject.StringPrototype, "toLocaleUpperCase", DelegateRuntimeFunction.FromDelegate((Func<EcmaValue, EcmaValue, EcmaValue>)BuiltInFunctionReplacement.StringPrototypeToLocaleUpperCase));
    }
  }
}
