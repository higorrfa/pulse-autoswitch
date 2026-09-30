using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
public sealed class PulseUsb : IDisposable
{
    const string DeviceMatch = "VID_054C&PID_0D5E&MI_03";
    SafeFileHandle file;
    IntPtr usb;
    byte inputPipe;
    public string InputInfo { get; private set; }
    public string Error { get; private set; }
    public bool IsOpen { get { return usb != IntPtr.Zero; } }

    public bool Open()
    {
        Dispose();
        foreach (Guid discoveredGuid in FindGuids())
        {
            Guid guid = discoveredGuid;
            IntPtr set = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, 0x12);
            if (set == new IntPtr(-1)) continue;
            try
            {
                for (uint index = 0; ; index++)
                {
                    InterfaceData data = new InterfaceData();
                    data.Size = Marshal.SizeOf(typeof(InterfaceData));
                    if (!SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, index, ref data)) break;
                    uint needed;
                    SetupDiGetDeviceInterfaceDetail(set, ref data, IntPtr.Zero, 0, out needed, IntPtr.Zero);
                    if (needed == 0) continue;
                    IntPtr detail = Marshal.AllocHGlobal((int)needed);
                    try
                    {
                        Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
                        if (!SetupDiGetDeviceInterfaceDetail(set, ref data, detail, needed, out needed, IntPtr.Zero)) continue;
                        string path = Marshal.PtrToStringUni(IntPtr.Add(detail, 4));
                        if (path == null || path.IndexOf(DeviceMatch, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        SafeFileHandle candidate = CreateFile(path, 0xC0000000, 3, IntPtr.Zero, 3, 0x40000080, IntPtr.Zero);
                        if (candidate.IsInvalid) { candidate.Dispose(); continue; }
                        IntPtr handle;
                        if (!WinUsb_Initialize(candidate, out handle)) { candidate.Dispose(); continue; }
                        UsbInterfaceDescriptor desc;
                        if (!WinUsb_QueryInterfaceSettings(handle, 0, out desc) || desc.Number != 3)
                        { WinUsb_Free(handle); candidate.Dispose(); continue; }
                        file = candidate; usb = handle; Error = null;
                        InputInfo="no input endpoint";
                        for(byte p=0;p<desc.Endpoints;p++)
                        {
                            PipeInformation pipe;
                            if(!WinUsb_QueryPipe(usb,0,p,out pipe) || pipe.Type!=3 || (pipe.Id&128)==0) continue;
                            uint timeout=10;
                            if(WinUsb_SetPipePolicy(usb,pipe.Id,3,4,ref timeout))
                            { inputPipe=pipe.Id; InputInfo="interrupt IN "+pipe.Id.ToString("X2"); break; }
                        }
                        return true;
                    }
                    finally { Marshal.FreeHGlobal(detail); }
                }
            }
            finally { SetupDiDestroyDeviceInfoList(set); }
        }
        Error = "PULSE 3D interface 3 is not available through WinUSB.";
        return false;
    }

    public byte[] Read()
    { return ReadReport(0xB0); }

    public byte[] ReadInput()
    {
        if(!IsOpen || inputPipe==0) return null;
        byte[] buffer=new byte[64]; uint count;
        if(!WinUsb_ReadPipe(usb,inputPipe,buffer,(uint)buffer.Length,out count,IntPtr.Zero) || count==0) return null;
        Array.Resize(ref buffer,(int)count); return buffer;
    }

    public byte[] ReadReport(byte reportId)
    {
        if (!IsOpen && !Open()) return null;
        byte[] buffer = new byte[65];
        SetupPacket packet = new SetupPacket { RequestType = 0xA1, Request = 1, Value = (ushort)(0x0300|reportId), Index = 3, Length = 65 };
        uint count;
        if (!WinUsb_ControlTransfer(usb, packet, buffer, (uint)buffer.Length, out count, IntPtr.Zero))
        {
            int code = Marshal.GetLastWin32Error();
            Error = "GET_REPORT "+reportId.ToString("X2")+": " + new Win32Exception(code).Message + " (" + code + ")";
            return null;
        }
        if (count < 8 || buffer[0] != reportId)
        { Error = "Response "+reportId.ToString("X2")+" is invalid: " + count + " bytes / " + BitConverter.ToString(buffer); return null; }
        Array.Resize(ref buffer, (int)count);
        Error = null;
        return buffer;
    }

    static IEnumerable<Guid> FindGuids()
    {
        HashSet<Guid> result = new HashSet<Guid>();
        using (RegistryKey root = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
        {
            if (root != null) foreach (string hardware in root.GetSubKeyNames())
            {
                if (!hardware.Equals(DeviceMatch, StringComparison.OrdinalIgnoreCase)) continue;
                using (RegistryKey devices = root.OpenSubKey(hardware))
                foreach (string instance in devices.GetSubKeyNames())
                using (RegistryKey parameters = devices.OpenSubKey(instance + @"\Device Parameters"))
                {
                    if (parameters == null) continue;
                    object value = parameters.GetValue("DeviceInterfaceGUIDs") ?? parameters.GetValue("DeviceInterfaceGUID");
                    string[] values = value as string[];
                    if (values == null && value is string) values = new string[] { (string)value };
                    if (values == null) continue;
                    foreach (string item in values) { Guid guid; if (Guid.TryParse(item, out guid)) result.Add(guid); }
                }
            }
        }
        return result;
    }

    public void Dispose()
    {
        inputPipe=0;
        if (usb != IntPtr.Zero) { WinUsb_Free(usb); usb = IntPtr.Zero; }
        if (file != null) { file.Dispose(); file = null; }
    }

    [StructLayout(LayoutKind.Sequential)] struct InterfaceData { public int Size; public Guid Guid; public uint Flags; public IntPtr Reserved; }
    [StructLayout(LayoutKind.Sequential, Pack=1)] struct SetupPacket { public byte RequestType, Request; public ushort Value, Index, Length; }
    [StructLayout(LayoutKind.Sequential, Pack=1)] struct UsbInterfaceDescriptor { public byte Length, Type, Number, Alternate, Endpoints, Class, SubClass, Protocol, StringIndex; }
    [StructLayout(LayoutKind.Sequential)] struct PipeInformation { public int Type; public byte Id; public ushort MaximumPacketSize; public byte Interval; }
    [DllImport("setupapi.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern IntPtr SetupDiGetClassDevs(ref Guid guid, string enumerator, IntPtr parent, uint flags);
    [DllImport("setupapi.dll", SetLastError=true)] static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr device, ref Guid guid, uint index, ref InterfaceData data);
    [DllImport("setupapi.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set, ref InterfaceData data, IntPtr detail, uint size, out uint needed, IntPtr device);
    [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
    [DllImport("winusb.dll", SetLastError=true)] static extern bool WinUsb_Initialize(SafeFileHandle file, out IntPtr handle);
    [DllImport("winusb.dll", SetLastError=true)] static extern bool WinUsb_QueryInterfaceSettings(IntPtr handle, byte alternate, out UsbInterfaceDescriptor desc);
    [DllImport("winusb.dll", SetLastError=true)] static extern bool WinUsb_ControlTransfer(IntPtr handle, SetupPacket packet, byte[] buffer, uint length, out uint transferred, IntPtr overlapped);
    [DllImport("winusb.dll",SetLastError=true)] static extern bool WinUsb_QueryPipe(IntPtr handle,byte alternate,byte index,out PipeInformation pipe);
    [DllImport("winusb.dll",SetLastError=true)] static extern bool WinUsb_SetPipePolicy(IntPtr handle,byte id,uint policy,uint length,ref uint value);
    [DllImport("winusb.dll",SetLastError=true)] static extern bool WinUsb_ReadPipe(IntPtr handle,byte id,byte[] buffer,uint length,out uint count,IntPtr overlapped);
    [DllImport("winusb.dll")] static extern bool WinUsb_Free(IntPtr handle);
}

public static class UsbProbe
{
    public static void Main(string[] args)
    {
        using (PulseUsb device = new PulseUsb())
        {
            DateTime end = DateTime.Now.AddSeconds(args.Length == 0 ? 1 : 90);
            string previous = null;
            do
            {
                byte[] report = device.Read();
                string line = report == null ? device.Error : BitConverter.ToString(report);
                if (line != previous) { Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + line); previous = line; }
                if (report == null) device.Dispose();
                System.Threading.Thread.Sleep(200);
            } while (DateTime.Now < end);
        }
    }
}
