using System.Security.Cryptography;

namespace NIMSAP_Lib;

public class TCPPacket
{
    public PacketType packetType;
    public byte[] data;

    public TCPPacket(byte[] data)
    {
        packetType = (PacketType)data[0];
        data = data.Skip(1).ToArray();
    }

    public void ReadPacket(byte[] data)
    {
        byte[] newData = new byte[data.Length + this.data.Length];
        
        for (int i = 0; i < this.data.Length; i++)
        {
            newData[i] = data[i];
        }

        for (int i = 0; i < data.Length; i++)
        {
            newData[i + this.data.Length] = data[i];
        }
        
        this.data = newData;
    }
}