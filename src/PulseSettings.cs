using System;
using System.IO;
using System.Xml.Serialization;

public sealed class PulseSettings
{
    public bool Enabled;
    public bool ShowBattery = true;
    public bool MediaButtons = false;
    public string HeadsetId, MonitorId;
    public int LinkByte = 4, LinkMask = 4, OnValue = 4, OffValue = 0;
    public static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
    public static PulseSettings Load()
    {
        try { using (var stream = File.OpenRead(Path)) return (PulseSettings)new XmlSerializer(typeof(PulseSettings)).Deserialize(stream); }
        catch { return new PulseSettings(); }
    }
    public void Save() { using (var stream = File.Create(Path)) new XmlSerializer(typeof(PulseSettings)).Serialize(stream, this); }
    public bool? Connected(byte[] raw)
    {
        if (raw == null || raw.Length < 8 || raw[0] != 0xB0 || LinkByte < 0 || LinkByte >= raw.Length || LinkMask == 0 || OnValue == OffValue) return null;
        int bits = raw[LinkByte] & LinkMask;
        if (bits == OnValue) return true;
        if (bits == OffValue) return false;
        return null;
    }
}
