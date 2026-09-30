using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

public static class PulseIcons
{
    public static Icon Battery(int? level, bool? connected)
    {
        Color color=connected==true ? Color.FromArgb(124,58,237) : Color.FromArgb(110,118,130);
        using(Bitmap bitmap=new Bitmap(32,32))
        {
            using(Graphics graphics=Graphics.FromImage(bitmap))
            using(Pen outline=new Pen(color,2.5f))
            using(Brush body=new SolidBrush(color))
            {
                graphics.SmoothingMode=SmoothingMode.AntiAlias;
                if(connected==true && level.HasValue)
                {
                    graphics.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    using(Brush charge=new SolidBrush(level.Value<=20 ? Color.FromArgb(220,75,45) : color))
                    using(Font font=new Font("Segoe UI",level.Value==100 ? 16 : 22,FontStyle.Bold,GraphicsUnit.Pixel))
                    using(StringFormat format=new StringFormat(StringFormat.GenericTypographic) { Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap })
                        graphics.DrawString(level.Value.ToString(),font,charge,new RectangleF(0,-2,32,25),format);
                }
                else
                {
                    graphics.DrawRectangle(outline,2,3,25,14);
                    graphics.FillRectangle(body,28,7,3,6);
                    if(connected!=false)
                        using(Font font=new Font("Segoe UI",9,FontStyle.Bold)) graphics.DrawString("?",font,body,10,1);
                }
                graphics.DrawArc(outline,10,21,12,10,180,180);
                graphics.FillRectangle(body,8,25,4,6); graphics.FillRectangle(body,20,25,4,6);
            }
            IntPtr handle=bitmap.GetHicon();
            try { using(Icon icon=Icon.FromHandle(handle)) return (Icon)icon.Clone(); }
            finally { DestroyIcon(handle); }
        }
    }
    public static Icon Create(Color color, Color badge)
    {
        using(Bitmap bitmap=new Bitmap(32,32))
        {
            using(Graphics graphics=Graphics.FromImage(bitmap))
            using(Pen pen=new Pen(color,4))
            using(Brush fill=new SolidBrush(color))
            using(Brush dot=new SolidBrush(badge))
            using(Pen border=new Pen(Color.White,1.5f))
            {
                graphics.SmoothingMode=SmoothingMode.AntiAlias;
                pen.StartCap=LineCap.Round; pen.EndCap=LineCap.Round;
                graphics.DrawArc(pen,5,4,22,23,180,180);
                graphics.FillRectangle(fill,3,14,7,12);
                graphics.FillRectangle(fill,22,14,7,12);
                graphics.FillEllipse(dot,22,23,9,9);
                graphics.DrawEllipse(border,22,23,9,9);
            }
            IntPtr handle=bitmap.GetHicon();
            try { using(Icon icon=Icon.FromHandle(handle)) return (Icon)icon.Clone(); }
            finally { DestroyIcon(handle); }
        }
    }
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
}
