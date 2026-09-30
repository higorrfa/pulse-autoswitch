using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class PulseVolumePopup : Form
{
    readonly Timer hideTimer=new Timer();
    readonly Label value=new Label();
    int volume;
    public PulseVolumePopup()
    {
        FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; TopMost=true;
        BackColor=Color.FromArgb(35,35,40); ForeColor=Color.White;
        Size=new Size(300,76); StartPosition=FormStartPosition.Manual;
        AccessibleName="PULSE 3D volume";
        var title=new Label { Text="PULSE 3D · Volume", AutoSize=true, Location=new Point(56,12), Font=new Font("Segoe UI",9), ForeColor=Color.FromArgb(220,220,225) };
        value.Location=new Point(238,34); value.Size=new Size(53,24);
        value.Font=new Font("Segoe UI",11); value.TextAlign=ContentAlignment.MiddleRight;
        Controls.AddRange(new Control[]{title,value});
        hideTimer.Interval=1800; hideTimer.Tick+=delegate { hideTimer.Stop(); Hide(); };
    }
    protected override bool ShowWithoutActivation { get { return true; } }
    protected override CreateParams CreateParams
    {
        get { var parameters=base.CreateParams; parameters.ExStyle|=0x08000000|0x80; return parameters; }
    }
    public void Display(int level)
    {
        volume=Math.Max(0,Math.Min(100,level)); value.Text=volume+"%";
        Rectangle area=Screen.FromPoint(Cursor.Position).WorkingArea;
        Location=new Point(area.Left+(area.Width-Width)/2,area.Bottom-Height-16);
        hideTimer.Stop(); Invalidate(); if(!Visible) Show(); hideTimer.Start();
    }
    public void Dismiss() { hideTimer.Stop(); Hide(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using(var pen=new Pen(Color.White,2.5f))
        {
            e.Graphics.DrawArc(pen,18,25,24,25,180,180);
            e.Graphics.DrawLine(pen,18,37,18,50); e.Graphics.DrawLine(pen,42,37,42,50);
            e.Graphics.DrawRectangle(pen,18,40,5,12); e.Graphics.DrawRectangle(pen,37,40,5,12);
        }
        using(var background=new SolidBrush(Color.FromArgb(85,85,95))) e.Graphics.FillRectangle(background,56,46,175,5);
        using(var fill=new SolidBrush(Color.FromArgb(150,100,245))) e.Graphics.FillRectangle(fill,56,46,175*volume/100,5);
    }
    protected override void Dispose(bool disposing)
    { if(disposing) hideTimer.Dispose(); base.Dispose(disposing); }
}
