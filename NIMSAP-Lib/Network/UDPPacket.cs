using System.Net;

namespace NIMSAP_Lib;

public class UDPPacket
{
    public PacketType packetType = PacketType.Null;         // 1 байт пакета
    public int count = 1;                                   // 2 байт пакета
    public IPEndPoint endPoint;                             // Владелец пакета
    public byte[] data = new byte[0];                       // Оставшийся буфер пакета
    
    public void ReadPacket(byte[] data)
    {
        if (packetType == PacketType.Null)
        {
            packetType = (PacketType)data[0];
            count = data[1];
            this.data = data.Skip(2).ToArray();
        }
        else
        {
            // На переработку
            byte[] newData = new byte[data.Length + this.data.Length];
            this.data =this. data.Concat(data).ToArray();

            /*for (int i = 0; i < this.data.Length; i++)
            {
                newData[i] = this.data[i];
            }

            for (int i = 0; i < data.Length; i++)
            {
                newData[i + this.data.Length] = data[i];
            }

            this.data = newData;*/
        }
    }

    public void CreatePacket(PacketType packetType, byte[] data, int bufferSize)
    {
        this.packetType = packetType;
        if (data != null)
        {
            count = Convert.ToByte(Math.Ceiling((this.data.Length+2) / (double)bufferSize));
            this.data = data;
        }
    }
    
    public List<byte[]> CreateBytePacket(int bufferSize)
    {
        List<byte[]> newPacket = new List<byte[]>();
        byte[] data = new byte[this.data.Length + 2];
        
        data[0] = (byte)packetType;
        data[1] = Convert.ToByte(Math.Ceiling((this.data.Length+2) / (double)bufferSize));
        if (this.data.Length > 0)
        {
            this.data.CopyTo(data, 2);
            newPacket = data.Chunk(bufferSize).ToList();
        }
        else
        {
            newPacket.Add(data);
        }

        return newPacket;
    }
}