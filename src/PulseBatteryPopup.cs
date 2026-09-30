using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class PulseBatteryPopup : Form
{
    public PulseBatteryPopup(int? level, bool? connected)
    {
        FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; TopMost=true;
        BackColor=Color.FromArgb(250,248,255); Size=new Size(330,215);
        StartPosition=FormStartPosition.Manual;
        Font=new Font("Segoe UI",10);
        var title=new Label { Text="Battery · PULSE 3D", AutoSize=true, Location=new Point(18,15), ForeColor=Color.FromArgb(124,58,237) };
        var value=new Label { Text=level.HasValue ? level.Value+"%" : "—", AutoSize=true, Location=new Point(15,39), Font=new Font("Segoe UI",30,FontStyle.Bold) };
        var line=new Label { Text=connected==true ? (level.HasValue ? "Headset on · receiver battery reading" : "Battery level unavailable") : (connected==false ? "Headset off · last reading" : "Connection unavailable · last reading"), AutoSize=true, Location=new Point(18,112), Font=new Font("Segoe UI",9) };
        var note=new Label { Text="Apnextte receiver reading; may update in steps.", Size=new Size(294,36), Location=new Point(18,170), Font=new Font("Segoe UI",9), ForeColor=Color.FromArgb(95,90,110) };
        Controls.AddRange(new Control[]{title,value,line,note});
        Paint += delegate(object sender,PaintEventArgs e)
        {
            using(Pen border=new Pen(Color.FromArgb(204,190,235))) e.Graphics.DrawRectangle(border,0,0,Width-1,Height-1);
            using(Brush background=new SolidBrush(Color.FromArgb(222,213,241))) e.Graphics.FillRectangle(background,18,148,294,12);
            if(level.HasValue && connected==true)
                using(Brush fill=new SolidBrush(level.Value<=20 ? Color.FromArgb(220,75,45) : Color.FromArgb(124,58,237)))
                    e.Graphics.FillRectangle(fill,18,148,294*level.Value/100,12);
        };
        Rectangle area=Screen.FromPoint(Cursor.Position).WorkingArea;
        Location=new Point(Math.Max(area.Left,Math.Min(Cursor.Position.X-Width/2,area.Right-Width-8)),area.Bottom-Height-8);
        Deactivate += delegate { Close(); };
    }
}
