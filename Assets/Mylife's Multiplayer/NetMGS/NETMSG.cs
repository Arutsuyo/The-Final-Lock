public static class NetID
{
    public const byte None = 0;
    public const byte Buffer = 1;
    public const byte ConnID = 2; // Do not use!
    public const byte Msg = 3;
    public const byte ObjMsg = 4;
    public const byte User = 5;
    
}

[System.Serializable]
public abstract class NETMSG
{
    public byte OP { set; get; } // Operation code (Message ID)
    public int dataSize { set; get; } // Additional data size (added ontop of bytesize...)
    public NETMSG()
    {
        OP = NetID.None;
        dataSize = 0;
    }
}
[System.Serializable]
public class MessageNETMSG : NETMSG
{
    public string msg;
    public MessageNETMSG()
    {
        OP = NetID.Msg;
        dataSize = 0;
    }
}

[System.Serializable]
public class UserNETMSG : NETMSG
{
    public int subID;
    public UserNETMSG()
    {
        OP = NetID.User;
        dataSize = 0;
    }
}
[System.Serializable]
public class UserMessageNETMSG : UserNETMSG
{
    public string msg;
}

public static class BTYPE
{
    public const byte i = 1;
    public const byte by = 2;
    public const byte fl = 3;
    public const byte db = 4;
    public const byte bo = 5;
}
[System.Serializable]
public class BNETMSG : NETMSG
{
    public byte bufferType;
    public int bufferSize;
}
[System.Serializable]
public class FNETMSG : BNETMSG
{
    public float[] data;
    public new byte bufferType = BTYPE.fl;
}

