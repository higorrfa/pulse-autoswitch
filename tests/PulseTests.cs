using System;
public static class PulseTests
{
    static int count;
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); count++; }
    public static void Main()
    {
        Check(!new PulseSettings().Enabled && !new PulseSettings().MediaButtons,"New installations must allow configuration before enabling automation and shortcuts.");
        Check(new PulseSettings().Connected(new byte[]{0xB0,3,100,100,0xEF,0x5A,0x11,60})==true,"Portable defaults must recognize the validated PULSE 3D connection state.");
        var cfg=new PulseSettings { LinkByte=4,LinkMask=4,OnValue=4,OffValue=0 };
        byte[][] linked={new byte[]{0xB0,3,100,100,0xEF,0x5A,0x11,60},new byte[]{0xB0,3,100,100,0xED,0x15,0x11,60},new byte[]{0xB0,3,100,100,0xEF,0x59,0x11,60}};
        byte[][] off={new byte[]{0xB0,3,100,100,0xEB,0x59,0x11,60},new byte[]{0xB0,3,100,100,0xE3,0x5B,0x11,60}};
        foreach(var report in linked) Check(cfg.Connected(report)==true,"Mute and transition reports must not disconnect the headset.");
        foreach(var report in off) Check(cfg.Connected(report)==false,"Power off must be detected without removing the receiver.");
        Check(cfg.Connected(null)==null,"A read failure does not prove disconnection.");
        Check(cfg.Connected(new byte[]{0xB0,3})==null,"Truncated reports must be ignored.");
        Check(cfg.Connected(new byte[]{0xA1,3,100,100,0xEF,0x5A,0x11,60})==null,"An unexpected report must not switch audio.");
        cfg.LinkByte=-1;Check(cfg.Connected(linked[0])==null,"Invalid connection settings must not infer a connection state.");
        var buttons=new PulseButtons();
        byte[] sample=new byte[]{0xB0,3,100,100,0xEF,0x13,0x11,60};
        Check(buttons.Read(sample,false)==null,"The first reading must not control playback.");
        sample[2]=90;sample[5]=0x14;
        Check(buttons.Read(sample,true)==PulseMedia.Previous,"CHAT must select the previous track.");
        Check(buttons.Read(sample,false)==null,"Polling after an event must not duplicate the key press.");
        sample[2]=100;sample[5]=0x13;
        Check(buttons.Read(sample,true)==PulseMedia.Next,"GAME must select the next track.");
        Check(buttons.Read(sample,false)==null,"A stable balance limit must not repeat track changes.");
        Check(buttons.Read(sample,true)==null,"A B0 snapshot at the limit does not prove a new button press.");
        sample[3]=90;
        Check(buttons.Read(sample,true)==null,"Battery updates must not change tracks.");
        sample[7]=70;
        Check(buttons.Read(sample,true)==null,"Volume changes must not change tracks.");
        sample[4]=0xED;
        Check(buttons.Read(sample,true)==null,"Mute changes must not change tracks.");
        buttons.Read(null,false);
        Check(buttons.Read(sample,false)==null,"Reconnection must not trigger a shortcut.");
        Check(buttons.Read(new byte[]{1,0,4,0,0},true)==PulseMedia.Next,"A declared NEXT consumer event must select the next track.");
        Check(buttons.Read(new byte[]{1,0,4,0,0},true)==null,"Holding a button must not repeat the track change.");
        Check(buttons.Read(new byte[]{1,0,0,0,0},true)==null,"Releasing a button must not send a command.");
        Check(buttons.Read(new byte[]{1,0,2,0,0},true)==PulseMedia.PlayPause,"A PLAY/PAUSE consumer event must toggle playback.");
        buttons.Read(new byte[]{1,0,0,0,0},true);
        Check(buttons.Read(new byte[]{1,0,1,0,0},true)==PulseMedia.Previous,"A PREVIOUS consumer event must select the previous track.");
        buttons.Reset();
        Check(buttons.Read(new byte[]{1,3,0,0,0},true)==null,"Consumer volume reports must not control playback.");
        Check(buttons.Read(new byte[]{1,0,4,0,0},false)==null,"State polling must not simulate a consumer button press.");
        Check(buttons.Read(new byte[]{1,0,4},true)==null,"Truncated consumer reports must be ignored.");
        Console.WriteLine(count+" checks passed.");
    }
}
