using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

public static class PulseLauncher
{
    [STAThread] public static void Main()
    {
        string directory=AppDomain.CurrentDomain.BaseDirectory;
        string app=Path.Combine(directory,"PulseAutoSwitch.exe");
        if(!File.Exists(app))
        {
            using(Stream resource=Assembly.GetExecutingAssembly().GetManifestResourceStream("PulseAutoSwitch.Target"))
            {
                if(resource!=null)
                    using(var reader=new StreamReader(resource)) app=reader.ReadToEnd().Trim();
            }
        }
        if(!File.Exists(app))
        {
            using(RegistryKey key=Registry.CurrentUser.OpenSubKey(@"Software\PulseAutoSwitch"))
            {
                string installed=key==null ? null : key.GetValue("ApplicationDirectory") as string;
                if(!String.IsNullOrWhiteSpace(installed)) app=Path.Combine(installed,"PulseAutoSwitch.exe");
            }
        }
        if(!File.Exists(app))
        {
            MessageBox.Show("PULSE AutoSwitch could not be found. Recreate the launcher using CreateDesktopLauncher.ps1.","PULSE AutoSwitch");
            return;
        }
        try { Process.Start(new ProcessStartInfo(app) { UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(app) }); }
        catch(Exception exc) { MessageBox.Show(exc.Message,"PULSE AutoSwitch"); }
    }
}
