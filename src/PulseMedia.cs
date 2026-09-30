using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public static class PulseMedia
{
    public const ushort Next=0xB0, Previous=0xB1, PlayPause=0xB3;
    [StructLayout(LayoutKind.Sequential)] struct Keyboard { public ushort Key, Scan; public uint Flags, Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Sequential)] struct Mouse { public int X,Y; public uint Data,Flags,Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Explicit)] struct Data { [FieldOffset(0)] public Keyboard Keyboard; [FieldOffset(0)] public Mouse Mouse; }
    [StructLayout(LayoutKind.Sequential)] struct Input { public uint Type; public Data Data; }
    [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,Input[] inputs,int size);
    public static void Send(ushort key)
    {
        var inputs=new Input[] {
            new Input { Type=1, Data=new Data { Keyboard=new Keyboard { Key=key } } },
            new Input { Type=1, Data=new Data { Keyboard=new Keyboard { Key=key,Flags=2 } } }
        };
        if(SendInput(2,inputs,Marshal.SizeOf(typeof(Input)))!=2)
            throw new Win32Exception(Marshal.GetLastWin32Error(),"Windows did not accept the media key.");
    }
}
