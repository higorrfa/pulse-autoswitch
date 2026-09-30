using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public sealed class PulseCard : Panel
{
    public PulseCard() { BackColor=Color.White; Padding=new Padding(18); DoubleBuffered=true; }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if(Width<24 || Height<24) return;
        using(var path=new GraphicsPath())
        {
            path.AddArc(0,0,24,24,180,90); path.AddArc(Width-25,0,24,24,270,90);
            path.AddArc(Width-25,Height-25,24,24,0,90); path.AddArc(0,Height-25,24,24,90,90); path.CloseFigure();
            Region old=Region; Region=new Region(path); if(old!=null) old.Dispose();
        }
    }
}

public sealed class PulseMeter : Control
{
    int level;
    public int Value { get { return level; } set { level=Math.Max(0,Math.Min(100,value)); Invalidate(); } }
    public PulseMeter() { Height=5; BackColor=Color.FromArgb(236,233,244); ForeColor=Color.FromArgb(117,82,215); DoubleBuffered=true; }
    protected override void OnPaint(PaintEventArgs e)
    { base.OnPaint(e); using(var brush=new SolidBrush(ForeColor)) e.Graphics.FillRectangle(brush,0,0,Width*level/100,Height); }
}
