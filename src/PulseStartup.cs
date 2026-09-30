using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

public static class PulseStartup
{
    const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
    public static string ShortcutPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),"PULSE AutoSwitch.lnk"); } }

    public static bool IsEnabled()
    {
        if(File.Exists(ShortcutPath)) return true;
        using(RegistryKey key=Registry.CurrentUser.OpenSubKey(RunKey))
            return key!=null && key.GetValue("PulseAutoSwitch")!=null;
    }

    public static void SetEnabled(bool enabled,string executable)
    {
        if(enabled)
        {
            executable=Path.GetFullPath(executable);
            if(!File.Exists(executable)) throw new FileNotFoundException("The PULSE executable was not found.",executable);
            Directory.CreateDirectory(Path.GetDirectoryName(ShortcutPath));
            object shell=null,shortcut=null;
            try
            {
                shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell",true));
                shortcut=shell.GetType().InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{ShortcutPath});
                Set(shortcut,"TargetPath",executable);
                Set(shortcut,"Arguments","--tray");
                Set(shortcut,"WorkingDirectory",Path.GetDirectoryName(executable));
                Set(shortcut,"IconLocation",executable+",0");
                Set(shortcut,"Description","Start PULSE AutoSwitch at Windows sign-in");
                shortcut.GetType().InvokeMember("Save",BindingFlags.InvokeMethod,null,shortcut,null);
            }
            finally
            {
                if(shortcut!=null) Marshal.FinalReleaseComObject(shortcut);
                if(shell!=null) Marshal.FinalReleaseComObject(shell);
            }
        }
        else if(File.Exists(ShortcutPath)) File.Delete(ShortcutPath);
        using(RegistryKey key=Registry.CurrentUser.OpenSubKey(RunKey,true))
            if(key!=null) key.DeleteValue("PulseAutoSwitch",false);
    }

    static void Set(object shortcut,string name,string value)
    { shortcut.GetType().InvokeMember(name,BindingFlags.SetProperty,null,shortcut,new object[]{value}); }
}
