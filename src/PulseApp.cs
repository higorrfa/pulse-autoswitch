using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

public sealed class PulseWindow : Form
{
    readonly PulseSettings settings;
    readonly Label status = new Label(), route = new Label();
    readonly TextBox report = new TextBox();
    readonly ComboBox headset = new ComboBox(), monitor = new ComboBox();
    readonly CheckBox enabled = new CheckBox(), startup = new CheckBox();
    readonly CheckBox showBattery = new CheckBox();
    readonly CheckBox mediaButtons = new CheckBox();
    readonly Label batteryLabel = new Label();
    readonly Label volumeLabel = new Label();
    PulseVolumePopup volumePopup;
    readonly PulseMeter batteryBar = new PulseMeter();
    readonly PulseMeter volumeBar = new PulseMeter();
    readonly Label batteryNote = new Label();
    readonly NotifyIcon tray = new NotifyIcon();
    readonly NotifyIcon batteryTray = new NotifyIcon();
    Icon batteryIcon;
    int? batteryLevel;
    bool? batteryConnected;
    string batteryIconKey;
    EventWaitHandle openEvent;
    readonly Icon connectedIcon=PulseIcons.Create(Color.FromArgb(41,120,224),Color.FromArgb(30,180,110));
    readonly Icon disconnectedIcon=PulseIcons.Create(Color.FromArgb(110,118,130),Color.FromArgb(110,118,130));
    readonly Icon unknownIcon=PulseIcons.Create(Color.FromArgb(41,120,224),Color.FromArgb(236,158,30));
    readonly object sync = new object();
    readonly List<AudioOutput> outputs = new List<AudioOutput>();
    Thread worker;
    volatile bool running = true;
    bool exiting, loading;
    string lastLog;
    readonly string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pulse.log");

    public PulseWindow(PulseSettings value, bool startHidden, EventWaitHandle openSignal)
    {
        openEvent=openSignal;
        settings = value;
        Text = "PULSE AutoSwitch"; Size = new Size(640, 720);
        MinimumSize = new Size(610,720); MaximumSize = new Size(1000,1100);
        Font = new Font("Segoe UI",10); StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245,246,250); ForeColor=Color.FromArgb(33,37,50); Icon=connectedIcon;
        var shell=new Panel { Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(24) };
        var grid=new TableLayoutPanel { Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,RowCount=9,Padding=Padding.Empty };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        var header=new Panel { Dock=DockStyle.Fill,Height=65,Margin=new Padding(0,0,0,12) };
        header.Controls.Add(new Label { Text="PULSE AutoSwitch",AutoSize=true,Font=new Font("Segoe UI",22,FontStyle.Bold),Location=new Point(0,0) });
        header.Controls.Add(new Label { Text="Your headset. The right output. Automatically.",AutoSize=true,ForeColor=Color.FromArgb(111,116,132),Location=new Point(1,43) });
        grid.Controls.Add(header);
        var connection=new Panel { Dock=DockStyle.Fill,Height=54,Margin=new Padding(0,0,0,12) };
        status.Text="Reading receiver..."; status.Font=new Font("Segoe UI",11,FontStyle.Bold); status.AutoSize=true; status.Location=new Point(0,0);
        route.Text="Waiting for headset status"; route.ForeColor=Color.FromArgb(111,116,132); route.AutoSize=false; route.Dock=DockStyle.Bottom; route.Height=24; route.AutoEllipsis=true;
        connection.Controls.Add(status); connection.Controls.Add(route); grid.Controls.Add(connection);
        var metrics=new TableLayoutPanel { Dock=DockStyle.Fill,Height=118,ColumnCount=2,RowCount=1,Margin=new Padding(0,0,0,14) };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50)); metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var batteryCard=new PulseCard { Dock=DockStyle.Fill,Margin=new Padding(0,0,7,0) };
        var volumeCard=new PulseCard { Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0) };
        batteryCard.Controls.Add(new Label { Text="BATTERY",AutoSize=true,Location=new Point(18,14),Font=new Font("Segoe UI",8,FontStyle.Bold),ForeColor=Color.FromArgb(111,116,132) });
        batteryLabel.Text="—"; batteryLabel.AutoSize=true; batteryLabel.Font=new Font("Segoe UI",24,FontStyle.Bold); batteryLabel.Location=new Point(14,30); batteryCard.Controls.Add(batteryLabel);
        batteryNote.Text="Waiting for receiver"; batteryNote.AutoSize=true; batteryNote.Font=new Font("Segoe UI",8); batteryNote.ForeColor=Color.FromArgb(111,116,132); batteryNote.Location=new Point(18,77); batteryCard.Controls.Add(batteryNote);
        batteryBar.Dock=DockStyle.Bottom; batteryCard.Controls.Add(batteryBar);
        volumeCard.Controls.Add(new Label { Text="VOLUME",AutoSize=true,Location=new Point(18,14),Font=new Font("Segoe UI",8,FontStyle.Bold),ForeColor=Color.FromArgb(111,116,132) });
        volumeLabel.Text="—"; volumeLabel.AutoSize=true; volumeLabel.Font=new Font("Segoe UI",24,FontStyle.Bold); volumeLabel.Location=new Point(14,30); volumeCard.Controls.Add(volumeLabel);
        volumeCard.Controls.Add(new Label { Text="Headset level",AutoSize=true,Location=new Point(18,77),Font=new Font("Segoe UI",8),ForeColor=Color.FromArgb(111,116,132) });
        volumeBar.Dock=DockStyle.Bottom; volumeCard.Controls.Add(volumeBar);
        metrics.Controls.Add(batteryCard,0,0); metrics.Controls.Add(volumeCard,1,0); grid.Controls.Add(metrics);
        var outputsCard=new PulseCard { Dock=DockStyle.Fill,Height=170,Margin=new Padding(0,0,0,14) };
        var outputGrid=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=4 };
        outputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        outputGrid.Controls.Add(new Label { Text="Headset output",AutoSize=true,ForeColor=Color.FromArgb(111,116,132) });
        headset.DropDownStyle=ComboBoxStyle.DropDownList; headset.Dock=DockStyle.Fill; headset.Margin=new Padding(0,3,0,12); outputGrid.Controls.Add(headset);
        outputGrid.Controls.Add(new Label { Text="Fallback output",AutoSize=true,ForeColor=Color.FromArgb(111,116,132) });
        monitor.DropDownStyle=ComboBoxStyle.DropDownList; monitor.Dock=DockStyle.Fill; monitor.Margin=new Padding(0,3,0,0); outputGrid.Controls.Add(monitor);
        outputsCard.Controls.Add(outputGrid); grid.Controls.Add(outputsCard);
        enabled.Appearance=Appearance.Button; enabled.FlatStyle=FlatStyle.Flat; enabled.FlatAppearance.BorderSize=0;
        enabled.Checked=settings.Enabled; enabled.Dock=DockStyle.Fill; enabled.Height=43; enabled.Margin=new Padding(0,0,0,14); enabled.TextAlign=ContentAlignment.MiddleCenter;
        Action updateRoutingButton=delegate { enabled.Text=enabled.Checked ? "Automatic routing  ·  ON" : "Enable automatic routing"; enabled.BackColor=enabled.Checked ? Color.FromArgb(117,82,215) : Color.White; enabled.ForeColor=enabled.Checked ? Color.White : ForeColor; };
        updateRoutingButton(); enabled.CheckedChanged+=delegate { updateRoutingButton(); }; grid.Controls.Add(enabled);
        var preferences=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,10) };
        startup.Text="Start with Windows"; startup.AutoSize=true; startup.Checked=PulseStartup.IsEnabled(); startup.Margin=new Padding(0,0,0,8); preferences.Controls.Add(startup);
        showBattery.Text="Show battery in the system tray"; showBattery.AutoSize=true; showBattery.Checked=settings.ShowBattery; showBattery.Margin=new Padding(0,0,0,6); preferences.Controls.Add(showBattery); grid.Controls.Add(preferences);
        var advanced=new PulseCard { Dock=DockStyle.Fill,AutoSize=true,Visible=false,Margin=new Padding(0,0,0,12) };
        var advancedGrid=new TableLayoutPanel { Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,RowCount=3 };
        advancedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        mediaButtons.Text="Chat/Game track shortcuts (experimental)"; mediaButtons.AutoSize=true; mediaButtons.Checked=settings.MediaButtons; advancedGrid.Controls.Add(mediaButtons);
        advancedGrid.Controls.Add(new Label { Text="Stops at balance limits. OFF/MONITOR is not mapped.",AutoSize=true,Font=new Font("Segoe UI",8),ForeColor=Color.FromArgb(111,116,132),Margin=new Padding(0,4,0,10) });
        report.ReadOnly=true; report.Multiline=true; report.Dock=DockStyle.Fill; report.Height=65; report.BorderStyle=BorderStyle.None; report.BackColor=Color.FromArgb(248,249,252); report.Font=new Font("Consolas",8); report.ScrollBars=ScrollBars.Vertical; advancedGrid.Controls.Add(report);
        advanced.Controls.Add(advancedGrid); grid.Controls.Add(advanced);
        var buttons=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoSize=true,WrapContents=true,Margin=Padding.Empty };
        var refresh=new Button { Text="Refresh outputs",AutoSize=true,Height=34 };
        refresh.Click+=delegate { RefreshOutputs(); };
        var advancedButton=new Button { Text="Advanced",AutoSize=true,Height=34 };
        advancedButton.Click+=delegate { advanced.Visible=!advanced.Visible; advancedButton.Text=advanced.Visible ? "Hide advanced" : "Advanced"; Height=advanced.Visible ? 870 : 720; };
        var minimize=new Button { Text="Minimize",AutoSize=true,Height=34 }; minimize.Click+=delegate { Hide(); };
        var quit=new Button { Text="Quit",AutoSize=true,Height=34 }; quit.Click+=delegate { exiting=true; Close(); };
        foreach(Button button in new[]{refresh,advancedButton,minimize,quit}) { button.FlatStyle=FlatStyle.Flat; button.FlatAppearance.BorderColor=Color.FromArgb(219,222,231); button.BackColor=Color.White; button.Padding=new Padding(8,4,8,4); button.Margin=new Padding(0,0,8,0); buttons.Controls.Add(button); }
        grid.Controls.Add(buttons);
        shell.Controls.Add(grid); Controls.Add(shell);
        RefreshOutputs();
        headset.SelectedIndexChanged += SaveSelections; monitor.SelectedIndexChanged += SaveSelections;
        enabled.CheckedChanged += delegate { lock(sync) { settings.Enabled = enabled.Checked; settings.Save(); } };
        showBattery.CheckedChanged += delegate { batteryTray.Visible=showBattery.Checked; lock(sync) { settings.ShowBattery=showBattery.Checked; settings.Save(); } };
        mediaButtons.CheckedChanged += delegate { lock(sync) { settings.MediaButtons=mediaButtons.Checked; settings.Save(); } };
        startup.CheckedChanged += delegate
        {
            try
            {
                PulseStartup.SetEnabled(startup.Checked,Application.ExecutablePath);
            }
            catch (Exception exc) { MessageBox.Show(this, exc.Message, "Start with Windows"); }
        };
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, delegate { Show(); WindowState = FormWindowState.Normal; Activate(); });
        var pauseItem=new ToolStripMenuItem("Pause automation") { CheckOnClick=true, Checked=!enabled.Checked };
        pauseItem.CheckedChanged += delegate { enabled.Checked=!pauseItem.Checked; };
        enabled.CheckedChanged += delegate { pauseItem.Checked=!enabled.Checked; };
        menu.Items.Add(pauseItem);
        menu.Items.Add("Quit", null, delegate { exiting = true; Close(); });
        tray.Icon = unknownIcon; tray.Text = "PULSE AutoSwitch"; tray.ContextMenuStrip = menu; tray.Visible = true;
        batteryIcon=PulseIcons.Battery(null,null); batteryTray.Icon=batteryIcon;
        batteryTray.Text="PULSE 3D: waiting for battery status"; batteryTray.ContextMenuStrip=menu; batteryTray.Visible=settings.ShowBattery;
        batteryTray.MouseClick += delegate(object sender,MouseEventArgs e) { if(e.Button==MouseButtons.Left) new PulseBatteryPopup(batteryLevel,batteryConnected).Show(); };
        tray.MouseClick += delegate(object sender,MouseEventArgs e) { if(e.Button==MouseButtons.Left) { Show(); WindowState=FormWindowState.Normal; Activate(); } };
        Resize += delegate { if(WindowState==FormWindowState.Minimized) Hide(); };
        FormClosing += delegate(object sender, FormClosingEventArgs e) { if(!exiting) { e.Cancel=true; Hide(); } };
        Shown += delegate { if(startHidden) Hide(); };
        worker = new Thread(Poll) { IsBackground = true, Name = "PulseStatus" }; worker.Start();
        new Thread(delegate()
        {
            while(running)
                if(openEvent.WaitOne(500) && running && IsHandleCreated)
                    BeginInvoke((Action)delegate { Show(); WindowState=FormWindowState.Normal; Activate(); });
        }) { IsBackground=true,Name="PulseOpen" }.Start();
    }

    void SaveSelections(object sender, EventArgs e)
    {
        if (loading) return;
        lock(sync)
        {
            AudioOutput a = headset.SelectedItem as AudioOutput, b = monitor.SelectedItem as AudioOutput;
            if(a!=null) settings.HeadsetId=a.Id;
            if(b!=null) settings.MonitorId=b.Id;
            settings.Save();
        }
    }

    void RefreshOutputs()
    {
        loading = true;
        try
        {
            List<AudioOutput> found = PulseAudio.List(); outputs.Clear(); outputs.AddRange(found);
            headset.Items.Clear(); monitor.Items.Clear();
            headset.Items.AddRange(found.Cast<object>().ToArray()); monitor.Items.AddRange(found.Cast<object>().ToArray());
            AudioOutput a = found.Find(x => String.Equals(x.Id, settings.HeadsetId, StringComparison.OrdinalIgnoreCase))
                ?? found.Find(x=>x.Name.IndexOf("Wireless Stereo Headset",StringComparison.OrdinalIgnoreCase)>=0);
            AudioOutput b = found.Find(x => String.Equals(x.Id, settings.MonitorId, StringComparison.OrdinalIgnoreCase))
                ?? found.Find(x=>x.Name.IndexOf("LG ULTRAWIDE",StringComparison.OrdinalIgnoreCase)>=0);
            if(a!=null) { headset.SelectedItem=a; settings.HeadsetId=a.Id; }
            if(b!=null) { monitor.SelectedItem=b; settings.MonitorId=b.Id; }
            settings.Save();
        }
        finally { loading=false; }
    }

    void Log(string line)
    {
        if(line == lastLog) return; lastLog=line;
        try
        {
            if(File.Exists(logPath) && new FileInfo(logPath).Length>1048576)
            {
                if(File.Exists(logPath+".previous")) File.Delete(logPath+".previous");
                File.Move(logPath,logPath+".previous");
            }
            File.AppendAllText(logPath,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" "+line+Environment.NewLine);
        }
        catch { }
    }

    void UpdateBattery(int? level,bool? connected)
    {
        batteryLevel=level; batteryConnected=connected;
        string caption=connected==true ? (level.HasValue ? "PULSE battery: "+level+"% reported by receiver" : "PULSE battery: unavailable") : "PULSE off · "+(level.HasValue ? "last reading: "+level+"%" : "no battery reading");
        if(!connected.HasValue) caption="PULSE unavailable · "+(level.HasValue ? "last reading: "+level+"%" : "no battery reading");
        batteryLabel.Text=level.HasValue ? level.Value+"%" : "—"; batteryNote.Text=connected==true ? "Receiver reading" : "Last reading"; batteryBar.Value=connected==true && level.HasValue ? level.Value : 0;
        batteryTray.Text=caption.Length>63 ? caption.Substring(0,63) : caption;
        string key=connected+":"+level;
        if(key!=batteryIconKey)
        {
            Icon old=batteryIcon; batteryIcon=PulseIcons.Battery(level,connected); batteryTray.Icon=batteryIcon;
            batteryIconKey=key; if(old!=null) old.Dispose();
        }
    }

    void Poll()
    {
        using(PulseUsb device=new PulseUsb())
        {
            bool? candidate=null, stable=null;
            int repetitions=0, failures=0, tick=0, inputCounter=0;
            string applied=null, lastRaw=null;
            int? lastBattery=null;
            int? previousVolume=null;
            PulseButtons buttons=new PulseButtons();
            List<AudioOutput> available=new List<AudioOutput>();
            while(running)
            {
                try
                {
                    if(tick++%4==0) available=PulseAudio.List();
                    byte[] raw=device.Read();
                    var inputPackets=new List<byte[]>();
                    for(int n=0;n<8;n++)
                    {
                        byte[] input=device.ReadInput(); if(input==null) break;
                        inputPackets.Add(input);
                        Log("INPUT #"+(++inputCounter)+" "+BitConverter.ToString(input));
                    }
                    string rawText=raw==null ? device.Error : BitConverter.ToString(raw);
                    if(rawText!=lastRaw) { Log("B0 "+rawText); lastRaw=rawText; }
                    PulseSettings snapshot;
                    lock(sync) snapshot = new PulseSettings { Enabled=settings.Enabled, MediaButtons=settings.MediaButtons, HeadsetId=settings.HeadsetId, MonitorId=settings.MonitorId, LinkByte=settings.LinkByte, LinkMask=settings.LinkMask, OnValue=settings.OnValue, OffValue=settings.OffValue };
                    bool? now=snapshot.Connected(raw);
                    bool dongleAbsent=!available.Exists(x=>x.Name.IndexOf("Wireless Stereo Headset",StringComparison.OrdinalIgnoreCase)>=0);
                    if(raw==null) { if(++failures>=4) device.Dispose(); if(dongleAbsent) now=false; }
                    else failures=0;
                    if(now==true && raw!=null && raw[3]<=100) lastBattery=raw[3];
                    int? volume=now==true && raw!=null && raw[7]<=100 ? (int?)raw[7] : null;
                    bool volumeChanged=volume.HasValue && previousVolume.HasValue && volume.Value!=previousVolume.Value;
                    previousVolume=volume;
                    if(now.HasValue && now==candidate) repetitions++;
                    else { candidate=now; repetitions=1; }
                    if(now.HasValue && repetitions>=3) stable=now;
                    if(now==true && repetitions>=3 && snapshot.MediaButtons)
                    {
                        inputPackets.Add(raw);
                        bool inputCommand=false;
                        for(int i=0;i<inputPackets.Count;i++)
                        {
                            ushort? key=buttons.Read(inputPackets[i],i<inputPackets.Count-1);
                            if(key.HasValue)
                            {
                                if(i==inputPackets.Count-1 && inputCommand) continue;
                                inputCommand=true;
                                try { PulseMedia.Send(key.Value); Log("MEDIA "+(key.Value==PulseMedia.Next ? "next" : key.Value==PulseMedia.Previous ? "previous" : "play/pause")); }
                                catch(Exception exc) { Log("MEDIA ERROR "+exc.Message); }
                            }
                        }
                    }
                    else { buttons.Reset(); if(now==true) buttons.Read(raw,false); }
                    string caption=now.HasValue ? (now.Value ? "Headset connected" : (dongleAbsent ? "Receiver removed" : "Headset off")) : "Headset status unavailable";
                    string currentRoute="Automation paused";
                    if(snapshot.Enabled && stable.HasValue && now.HasValue && repetitions>=3)
                    {
                        string target=stable.Value ? snapshot.HeadsetId : snapshot.MonitorId;
                        AudioOutput endpoint=available.Find(x=>String.Equals(x.Id,target,StringComparison.OrdinalIgnoreCase));
                        if(endpoint==null)
                        {
                            string needle=stable.Value ? "Wireless Stereo Headset" : "LG ULTRAWIDE";
                            var matches=available.FindAll(x=>x.Name.IndexOf(needle,StringComparison.OrdinalIgnoreCase)>=0);
                            if(matches.Count==1) endpoint=matches[0];
                        }
                        if(endpoint==null) { currentRoute="Configured output unavailable"; applied=null; }
                        else
                        {
                            string transition=stable.Value+":"+endpoint.Id;
                            if(applied!=transition)
                            {
                                PulseAudio.Select(endpoint.Id); applied=transition;
                                Log("OUTPUT "+endpoint.Name);
                            }
                            currentRoute="Output: "+endpoint.Name;
                        }
                    }
                    else { applied=null; if(snapshot.Enabled) currentRoute="Waiting for connection confirmation"; }
                    if(IsHandleCreated && running)
                    {
                        string s=caption,r=currentRoute,d=rawText;
                        bool? state=now;
                        int? level=lastBattery;
                        int? shownVolume=volume; bool showVolume=volumeChanged;
                        BeginInvoke((Action)delegate {
                            status.Text="●  "+s; status.ForeColor=state==true ? Color.FromArgb(30,137,88) : Color.FromArgb(111,116,132); route.Text=r; report.Text="B0 diagnostic: "+d; tray.Text="PULSE: "+s; tray.Icon=state.HasValue ? (state.Value ? connectedIcon : disconnectedIcon) : unknownIcon; UpdateBattery(level,state);
                            volumeLabel.Text=shownVolume.HasValue ? shownVolume.Value+"%" : "—"; volumeBar.Value=shownVolume ?? 0;
                            if(showVolume) { if(volumePopup==null) volumePopup=new PulseVolumePopup(); volumePopup.Display(shownVolume.Value); }
                            else if(!shownVolume.HasValue && volumePopup!=null) volumePopup.Dismiss();
                        });
                    }
                }
                catch(Exception exc) { previousVolume=null; Log("ERROR "+exc.Message); device.Dispose(); }
                Thread.Sleep(250);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        running=false;
        if(disposing && volumePopup!=null) volumePopup.Dispose();
        if(disposing) { tray.Visible=false; tray.Dispose(); batteryTray.Visible=false; batteryTray.Dispose(); if(batteryIcon!=null) batteryIcon.Dispose(); connectedIcon.Dispose(); disconnectedIcon.Dispose(); unknownIcon.Dispose(); }
        base.Dispose(disposing);
    }
}

public static class PulseApp
{
    [STAThread] public static void Main(string[] args)
    {
        bool created;
        using(Mutex mutex=new Mutex(true,@"Local\PulseAutoSwitch054C0D5E",out created))
        {
            if(!created)
            {
                if(args.Contains("--tray")) return;
                try { using(EventWaitHandle signal=EventWaitHandle.OpenExisting(@"Local\PulseAutoSwitch054C0D5E.Open")) signal.Set(); }
                catch { MessageBox.Show("PULSE AutoSwitch is starting. Open it from the system tray icon.","PULSE AutoSwitch"); }
                return;
            }
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            using(EventWaitHandle signal=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\PulseAutoSwitch054C0D5E.Open"))
                Application.Run(new PulseWindow(PulseSettings.Load(),args.Contains("--tray"),signal));
        }
    }
}
