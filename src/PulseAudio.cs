using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

public sealed class AudioOutput
{
    public string Id, Name;
    public override string ToString() { return Name; }
}

public static class PulseAudio
{
    public static List<AudioOutput> List(int flow=0)
    {
        IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        IMMDeviceCollection devices = null;
        List<AudioOutput> result = new List<AudioOutput>();
        try
        {
            enumerator.EnumAudioEndpoints(flow, 1, out devices);
            uint count; devices.GetCount(out count);
            for (uint i = 0; i < count; i++)
            {
                IMMDevice device; devices.Item(i, out device);
                try
                {
                    string id; device.GetId(out id);
                    IPropertyStore store; device.OpenPropertyStore(0, out store);
                    string name = id;
                    try
                    {
                        PropertyKey key = new PropertyKey { Format = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), Id = 14 };
                        PropVariant value; store.GetValue(ref key, out value);
                        try { if (value.Type == 31) name = Marshal.PtrToStringUni(value.Pointer); }
                        finally { PropVariantClear(ref value); }
                    }
                    finally { Marshal.ReleaseComObject(store); }
                    result.Add(new AudioOutput { Id = id, Name = name ?? id });
                }
                finally { Marshal.ReleaseComObject(device); }
            }
        }
        finally
        {
            if (devices != null) Marshal.ReleaseComObject(devices);
            Marshal.ReleaseComObject(enumerator);
        }
        return result;
    }

    public static string DefaultId(int role,int flow=0)
    {
        IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        IMMDevice device = null;
        try
        {
            enumerator.GetDefaultAudioEndpoint(flow, role, out device);
            string id; device.GetId(out id); return id;
        }
        catch (COMException) { return null; }
        finally
        {
            if (device != null) Marshal.ReleaseComObject(device);
            Marshal.ReleaseComObject(enumerator);
        }
    }

    public static void Select(string id)
    {
        IPolicyConfig policy = (IPolicyConfig)new PolicyConfigClient();
        try
        {
            for (int role = 0; role < 3; role++)
                if (!String.Equals(DefaultId(role), id, StringComparison.OrdinalIgnoreCase))
                    Marshal.ThrowExceptionForHR(policy.SetDefaultEndpoint(id, role));
        }
        finally { Marshal.ReleaseComObject(policy); }
    }

    public static void SelectInputRole(string id,int role)
    {
        IPolicyConfig policy=(IPolicyConfig)new PolicyConfigClient();
        try { if(!String.Equals(DefaultId(role,1),id,StringComparison.OrdinalIgnoreCase)) Marshal.ThrowExceptionForHR(policy.SetDefaultEndpoint(id,role)); }
        finally { Marshal.ReleaseComObject(policy); }
    }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(int flow, int states, out IMMDeviceCollection collection);
        void GetDefaultAudioEndpoint(int flow, int role, out IMMDevice device);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
    }
    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection { void GetCount(out uint count); void Item(uint index, out IMMDevice device); }
    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        void Activate(ref Guid iid, int context, IntPtr parameters, out IntPtr result);
        void OpenPropertyStore(int mode, out IPropertyStore store);
        void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        void GetState(out int state);
    }
    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumerator { }
    [StructLayout(LayoutKind.Sequential)] struct PropertyKey { public Guid Format; public uint Id; }
    [StructLayout(LayoutKind.Explicit, Size=24)] struct PropVariant { [FieldOffset(0)] public ushort Type; [FieldOffset(8)] public IntPtr Pointer; }
    [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        void GetCount(out uint count); void GetAt(uint index, out PropertyKey key);
        void GetValue(ref PropertyKey key, out PropVariant value);
    }
    [DllImport("ole32.dll")] static extern int PropVariantClear(ref PropVariant value);
    [ComImport, Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        void GetMixFormat(); void GetDeviceFormat(); void ResetDeviceFormat(); void SetDeviceFormat();
        void GetProcessingPeriod(); void SetProcessingPeriod(); void GetShareMode(); void SetShareMode();
        void GetPropertyValue(); void SetPropertyValue();
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, int role);
    }
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")] class PolicyConfigClient { }
}
