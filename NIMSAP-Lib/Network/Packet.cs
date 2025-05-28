namespace NIMSAP_Lib;

public class Packet
{
    // Количество пакетов данных
    public int count;
    // Количество полученных пакетов данных
    public int received = 0;
    // Тип пакета
    public PacketType packetType;
    // Данные пакета
    public byte[] data;

    public Packet(byte[] data)
    {
        packetType = (PacketType)data[0];
        count = data[1];
    }

    public void GetData(byte[] data)
    {
        
    }
}