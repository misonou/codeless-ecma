using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [Cloneable(false)]
  public class EcmaDate : RuntimeObject {
    private EcmaTimestamp timestamp;

    public EcmaDate()
      : this(DateTime.UtcNow) { }

    public EcmaDate(DateTime dt)
      : this(EcmaTimestamp.FromNativeDateTime(dt).Value) { }

    public EcmaDate(string str)
      : this(DateConstructor.ParseInternal(str).Value) { }

    public EcmaDate(long timestamp)
      : base(WellKnownObject.DatePrototype) {
      this.timestamp = new EcmaTimestamp(timestamp);
    }

    public EcmaDate(long year, long month)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month)) { }

    public EcmaDate(long year, long month, long date)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month, date)) { }

    public EcmaDate(long year, long month, long date, long hours)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month, date, hours)) { }

    public EcmaDate(long year, long month, long date, long hours, long minutes)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month, date, hours, minutes)) { }

    public EcmaDate(long year, long month, long date, long hours, long minutes, long seconds)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month, date, hours, minutes, seconds)) { }

    public EcmaDate(long year, long month, long date, long hours, long minutes, long seconds, long milliseconds)
      : this(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, year, month, date, hours, minutes, seconds, milliseconds)) { }

    public DateTime Value {
      get { return timestamp.ToNativeDateTime(DateTimeKind.Local); }
      set { timestamp = new EcmaTimestamp(value); }
    }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.Date; }
    }

    internal EcmaTimestamp Timestamp {
      get { return timestamp; }
      set { timestamp = value; }
    }

    public EcmaDate Parse(string str) {
      return new EcmaDate(str);
    }

    public long GetTime() {
      return timestamp.Value;
    }

    public int GetFullYear() {
      return timestamp.GetComponent(EcmaDateComponent.Year);
    }

    public int GetMonth() {
      return timestamp.GetComponent(EcmaDateComponent.Month);
    }

    public int GetDate() {
      return timestamp.GetComponent(EcmaDateComponent.Date);
    }

    public int GetHours() {
      return timestamp.GetComponent(EcmaDateComponent.Hours);
    }

    public int GetMinutes() {
      return timestamp.GetComponent(EcmaDateComponent.Minutes);
    }

    public int GetSeconds() {
      return timestamp.GetComponent(EcmaDateComponent.Seconds);
    }

    public int GetMilliseconds() {
      return timestamp.GetComponent(EcmaDateComponent.Milliseconds);
    }

    public int GetDay() {
      return timestamp.GetComponent(EcmaDateComponent.WeekDay);
    }

    public int GetTimezoneOffset() {
      return EcmaTimestamp.TimezoneOffset;
    }

    public int GetUtcFullYear() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Year);
    }

    public int GetUtcMonth() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Month);
    }

    public int GetUtcDate() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Date);
    }

    public int GetUtcHours() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Hours);
    }

    public int GetUtcMinutes() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Minutes);
    }

    public int GetUtcSeconds() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Seconds);
    }

    public int GetUtcMilliseconds() {
      return timestamp.GetComponentUtc(EcmaDateComponent.Milliseconds);
    }

    public int GetUtcDay() {
      return timestamp.GetComponentUtc(EcmaDateComponent.WeekDay);
    }

    public long SetTime(long time) {
      timestamp = new EcmaTimestamp(time);
      return timestamp.Value;
    }

    public long SetFullYear(long year) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponents(EcmaDateComponent.Year, year);
    }

    public long SetFullYear(long year, long month) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponents(EcmaDateComponent.Year, year, month);
    }

    public long SetFullYear(long year, long month, long date) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponents(EcmaDateComponent.Year, year, month, date);
    }

    public long SetMonth(long month) {
      return SetComponents(EcmaDateComponent.Month, month);
    }

    public long SetMonth(long month, long date) {
      return SetComponents(EcmaDateComponent.Month, month, date);
    }

    public long SetDate(long date) {
      return SetComponents(EcmaDateComponent.Date, date);
    }

    public long SetHours(long hours) {
      return SetComponents(EcmaDateComponent.Hours, hours);
    }

    public long SetHours(long hours, long minutes) {
      return SetComponents(EcmaDateComponent.Hours, hours, minutes);
    }

    public long SetHours(long hours, long minutes, long seconds) {
      return SetComponents(EcmaDateComponent.Hours, hours, minutes, seconds);
    }

    public long SetHours(long hours, long minutes, long seconds, long milliseconds) {
      return SetComponents(EcmaDateComponent.Hours, hours, minutes, seconds, milliseconds);
    }

    public long SetMinutes(long minutes) {
      return SetComponents(EcmaDateComponent.Minutes, minutes);
    }

    public long SetMinutes(long minutes, long seconds) {
      return SetComponents(EcmaDateComponent.Minutes, minutes, seconds);
    }

    public long SetMinutes(long minutes, long seconds, long milliseconds) {
      return SetComponents(EcmaDateComponent.Minutes, minutes, seconds, milliseconds);
    }

    public long SetSeconds(long seconds) {
      return SetComponents(EcmaDateComponent.Seconds, seconds);
    }

    public long SetSeconds(long seconds, long milliseconds) {
      return SetComponents(EcmaDateComponent.Seconds, seconds, milliseconds);
    }

    public long SetMilliseconds(long milliseconds) {
      return SetComponents(EcmaDateComponent.Milliseconds, milliseconds);
    }

    public long SetUtcFullYear(long year) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponentsUtc(EcmaDateComponent.Year, year);
    }

    public long SetUtcFullYear(long year, long month) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponentsUtc(EcmaDateComponent.Year, year, month);
    }

    public long SetUtcFullYear(long year, long month, long date) {
      if (!timestamp.IsValid) {
        timestamp = EcmaTimestamp.LocalEpoch;
      }
      return SetComponentsUtc(EcmaDateComponent.Year, year, month, date);
    }

    public long SetUtcMonth(long month) {
      return SetComponentsUtc(EcmaDateComponent.Month, month);
    }

    public long SetUtcMonth(long month, long date) {
      return SetComponentsUtc(EcmaDateComponent.Month, month, date);
    }

    public long SetUtcDate(long date) {
      return SetComponentsUtc(EcmaDateComponent.Date, date);
    }

    public long SetUtcHours(long hours) {
      return SetComponentsUtc(EcmaDateComponent.Hours, hours);
    }

    public long SetUtcHours(long hours, long minutes) {
      return SetComponentsUtc(EcmaDateComponent.Hours, hours, minutes);
    }

    public long SetUtcHours(long hours, long minutes, long seconds) {
      return SetComponentsUtc(EcmaDateComponent.Hours, hours, minutes, seconds);
    }

    public long SetUtcHours(long hours, long minutes, long seconds, long milliseconds) {
      return SetComponentsUtc(EcmaDateComponent.Hours, hours, minutes, seconds, milliseconds);
    }

    public long SetUtcMinutes(long minutes) {
      return SetComponentsUtc(EcmaDateComponent.Minutes, minutes);
    }

    public long SetUtcMinutes(long minutes, long seconds) {
      return SetComponentsUtc(EcmaDateComponent.Minutes, minutes, seconds);
    }

    public long SetUtcMinutes(long minutes, long seconds, long milliseconds) {
      return SetComponentsUtc(EcmaDateComponent.Minutes, minutes, seconds, milliseconds);
    }

    public long SetUtcSeconds(long seconds) {
      return SetComponentsUtc(EcmaDateComponent.Seconds, seconds);
    }

    public long SetUtcSeconds(long seconds, long milliseconds) {
      return SetComponentsUtc(EcmaDateComponent.Seconds, seconds, milliseconds);
    }

    public long SetUtcMilliseconds(long milliseconds) {
      return SetComponentsUtc(EcmaDateComponent.Milliseconds, milliseconds);
    }

    public override string ToString() {
      return timestamp.ToString(DateTimeFormatInfo.InvariantInfo);
    }

    public string ToISOString() {
      return timestamp.ToISOString();
    }

    public EcmaValue ToUTCString() {
      return timestamp.ToUTCString(DateTimeFormatInfo.InvariantInfo);
    }

    public EcmaValue ToDateString() {
      return timestamp.ToDateString(DateTimeFormatInfo.InvariantInfo);
    }

    public EcmaValue ToTimeString() {
      return timestamp.ToTimeString(DateTimeFormatInfo.InvariantInfo);
    }

    public string ToLocaleString() {
      return timestamp.ToString(DateTimeFormatInfo.CurrentInfo);
    }

    public EcmaValue ToLocaleDateString() {
      return timestamp.ToDateString(DateTimeFormatInfo.CurrentInfo);
    }

    public EcmaValue ToLocaleTimeString() {
      return timestamp.ToTimeString(DateTimeFormatInfo.CurrentInfo);
    }

    private long SetComponents(EcmaDateComponent start, params long[] args) {
      timestamp = (EcmaTimestamp)EcmaTimestamp.GetTimestamp(timestamp.Value, (int)start, args);
      return timestamp.Value;
    }

    private long SetComponentsUtc(EcmaDateComponent start, params long[] args) {
      timestamp = (EcmaTimestamp)EcmaTimestamp.GetTimestampUtc(timestamp.Value, (int)start, args);
      return timestamp.Value;
    }
  }
}
