using System;
public sealed class PulseButtons
{
    byte[] previous;
    int consumerHeld;
    public void Reset() { previous=null; consumerHeld=0; }
    public ushort? Read(byte[] raw,bool inputEvent)
    {
        if(inputEvent && raw!=null && raw.Length==5 && raw[0]==1)
        {
            int held=raw[2]&7, pressed=held&~consumerHeld;
            consumerHeld=held;
            if(pressed==1) return PulseMedia.Previous;
            if(pressed==2) return PulseMedia.PlayPause;
            if(pressed==4) return PulseMedia.Next;
            return null;
        }
        if(raw!=null && raw.Length>0 && raw[0]!=0xB0) return null;
        if(raw==null || raw.Length<8 || raw[0]!=0xB0 || (raw[4]&4)==0 || raw[2]>100)
        { Reset(); return null; }
        byte[] old=previous; previous=(byte[])raw.Clone();
        if(old==null) return null;
        int difference=raw[2]-old[2];
        if(raw[5]==0x13 && difference>0) return PulseMedia.Next;
        if(raw[5]==0x14 && difference<0) return PulseMedia.Previous;
        return null;
    }
}
