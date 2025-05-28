using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codename_NIMSAP.Managers;
using NIMSAP_Lib;
using NIMSAP_Lib.Map;
using NIMSAP_Server;

namespace Codename_NIMSAP;

/*public class Network
{
    private UdpClient client = new UdpClient(1487);
    private IPEndPoint endPoint;
    private Thread receiveThread;
    private Thread sendThread;
    private DateTime pingSendTime;
    private bool connected = false;
    private int ping = 0;
    public event Action<byte[]> OnDataReceived;

    public void Start()
    {
        client.EnableBroadcast = true;
        endPoint = IPEndPoint.Parse("26.177.219.48:1488");
        
        Thread network = new Thread(Connect);
        network.Start();
    }
    
    public void Connect()
    {
        int connectionTry = 1;
        client.Connect(endPoint);

        while (!connected)
        {
            try
            {
                byte[] type = new byte[1] {(byte)PacketType.Connection};
                client.Send(type);
                Console.WriteLine($"Sent connection to {endPoint}");
                
                if (client.Receive(ref endPoint)[0] != (byte)PacketType.Connection)
                {
                    Console.WriteLine("Connection failed");
                    throw new Exception();
                }

                Console.WriteLine("Connection established");

                // Загрузка карты
                Console.WriteLine("Getting map...");
                byte[] byteMap = client.Receive(ref endPoint);
                OnDataReceived?.Invoke(byteMap);
                Console.WriteLine($"Got map of {byteMap.Length} bytes");
                
                connected = true;
                Recieve();
                connectionTry = 1;
            }
            catch (Exception e)
            {
                Thread.Sleep(5000);
                
                if (connectionTry == 5)
                {
                    Console.WriteLine("Connection failed");
                    Disconnect();
                    break;
                }
                else
                {
                    Console.WriteLine($"Reconnecting try {connectionTry}");
                    connectionTry++;
                    continue;
                }
            }
        }
    }

    public void Ping()
    {
        int loss = 0;
        while (loss < 5)
        try
        {
            while (true)
            {
                byte[] sendbuf = new byte[1] { (byte)PacketType.Ping};

                client.Send(sendbuf);
                Console.WriteLine(sendbuf.Length + " bytes sent to " + endPoint);
                ping++;
                Thread.Sleep(1000);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }
    
    public void Send(PacketType? packetType)
    {
        try
        {
            byte[] sendbuf = new byte[1] {(byte)packetType};
            
            if (packetType == PacketType.Ping) pingSendTime = DateTime.Now;
            client.Send(sendbuf);
            Console.WriteLine(sendbuf.Length + " bytes sent to " + endPoint);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }

    public void Recieve()
    {
        try
        {
            pingSendTime = DateTime.Now;
            while (DateTime.Now.Subtract(pingSendTime).TotalSeconds < 5)
            {
                if (DateTime.Now.Subtract(pingSendTime).TotalSeconds >= 1)
                {
                    pingSendTime = DateTime.Now;
                    byte[] type = new byte[1] { (byte)PacketType.Ping };
                    client.Send(type);
                    Console.WriteLine($"Sent ping to {endPoint}");
                    ping++;
                }
                
                if (client.Available > 0)
                {
                    byte[] packetType = new byte[1];
                    packetType = client.Receive(ref endPoint);
                    switch ((PacketType)packetType[0])
                    {
                        case PacketType.Ping:
                        {
                            Console.WriteLine(
                                $"Returned ping from {endPoint} with {DateTime.Now.Subtract(pingSendTime).TotalMilliseconds} ms");
                            ping = 0;
                            break;
                        }
                        case PacketType.Disconnection:
                        {
                            break;
                        }
                    }
                }
            }

            Console.WriteLine($"Ping limit exceeded");
        }
        catch (Exception e)
        {
            Console.WriteLine("Recieve failed");
        }
        finally
        {
            connected = false;
        }
    }
    
    public void Disconnect()
    {
        Console.WriteLine("Disconnected from server");
        client.Close();
    }
}*/

/* Класс клиента */
public class Client
{
    private UdpClient udpClient;
    private IPEndPoint udpServerEndPoint;
    private Guid guid = Guid.Empty;
    private int bufferSize = 4096;
    private Thread udpThread;
    private bool connected = false;
    private DateTime lastPing;

    public byte[] input = null;
    public event Action<PacketType, byte[]> DataReceived;

    /* Запуск сетевого клиента */
    public void Start(int port)
    {
        udpClient = new UdpClient();
        udpClient.Client.SendTimeout = 10000;
        udpClient.Client.ReceiveTimeout = 10000;

        while (udpClient.Available > 0) udpClient.Receive(ref udpServerEndPoint);
        
        udpServerEndPoint = IPEndPoint.Parse("26.177.219.48:" + port);
        
        udpThread = new Thread(Connect);
        
        udpThread.Start();
    }
    
    /* TODO: Мне не нравится данное состояние Guid, его легко поменять/удалить */
    /* Подключение к серверу */
    public void Connect()
    {
        Logger.Log("Попытка соединения с сервером");
        
        udpClient.Connect(udpServerEndPoint);
        
        // Попытка прочитать GUID
        if (File.Exists("guid.txt"))
        {
            guid = Guid.Parse(File.ReadAllText("guid.txt"));
        }
        // Создание нового Guid
        else
        {
            guid = Guid.NewGuid();
        }
        UdpSendPacket(PacketType.Connection, guid.ToByteArray());
        
        UDPPacket packet = UdpGetPacket();
        
        if (packet.packetType == PacketType.Connection)
        {
            DataManager.playerId = BitConverter.ToInt32(packet.data);
            File.WriteAllText("guid.txt", guid.ToString());
            connected = true;
            Logger.Log("Подключение установлено");
            UdpSendPacket(PacketType.Map);
            Listener();
        }
        else
        {
            Logger.Log("Не удалось установить подключение");
        }
    }

    /* Прослушивание сервера */
    public void Listener()
    {
        // udpClient.Dispose();
        // Logger.Log("Запущен UDP поток");
        try
        {
            lastPing = DateTime.Now;
            while (connected)
            {
                // Отправка пинга каждую секунду
                if (DateTime.Now.Subtract(lastPing).TotalSeconds > 1)
                {
                    UdpSendPacket(PacketType.Ping);

                    lastPing = DateTime.Now;
                }
                // Обработка полученных пакетов
                if (udpClient.Available > 0)
                {
                    UDPPacket packet = UdpGetPacket();
                    
                    switch (packet.packetType)
                    {
                        // Загрузка игровой карты
                        case PacketType.Map:
                        {
                            Logger.Log("Получена карта мира");
                            DataReceived?.Invoke(packet.packetType, packet.data);

                            break;
                        }
                        // Создание новой сущности
                        case PacketType.CreateEntity:
                        {
                            Logger.Log("Получено создание сущности");
                            DataReceived?.Invoke(packet.packetType, packet.data);
                            
                            break;
                        }
                        // Модификация уже имеющейся сущности
                        case PacketType.UpdateEntity:
                        {
                            Logger.Log("Получено изменение сущности");
                            DataReceived?.Invoke(packet.packetType, packet.data);
                            
                            break;
                        }
                        // Удаление сущности
                        case PacketType.DeleteEntity:
                        {
                            Logger.Log("Получено удаление сущности");
                            DataReceived?.Invoke(packet.packetType, packet.data);
                            
                            break;
                        }
                    }
                }
                // Отправка действий клиента
                if (input != null)
                {
                    UdpSendPacket(PacketType.InputMotion, input);
                    input = null;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log("Ошибка UDP потока", e);
            Disconnect();
        }
    }

    /* Отправка пинга серверу */
    void SendPing()
    {
        while (connected)
        {
            UdpSendPacket(PacketType.Ping);
        }
    }

    /* Отключение от сервера */
    public void Disconnect()
    {
        UdpSendPacket(PacketType.Disconnection);
        Logger.Log("Соединение с сервером разорвано");
        
        connected = false;
        udpClient.Close();
    }
    /* Отправка  */
    
    /* Отправка пакетов серверу */
    public void UdpSendPacket(PacketType packetType, byte[] data = null)
    {
        try
        {
            UDPPacket packet = new UDPPacket();
            packet.CreatePacket(packetType, data, bufferSize);

            List<byte[]> packets = packet.CreateBytePacket(bufferSize);

            Logger.Log($"НАЧАЛО ОТПРАВКИ {packetType} ПАКЕТА К {udpServerEndPoint}");
            foreach (byte[] sendPacket in packets)
            {
                udpClient.Send(sendPacket);
                
                Logger.Log($"Отправлен {packet.packetType} пакет размером {sendPacket.Length} байт на {udpServerEndPoint}");
                
                // Обновление пинга
                lastPing = DateTime.Now;
            }
            Logger.Log($"{packet.packetType} ПАКЕТ УСПЕШНО ОТПРАВЛЕН В РАЗМЕРЕ {packet.data.Length + 2} БАЙТ К {udpServerEndPoint}");
        }
        catch (Exception e)
        {
            Logger.Log($"Ошибка отправки UDP пакета", e);
        }
    }
    
    /* Получение пакетов от сервера */
    UDPPacket UdpGetPacket()
    {
        UDPPacket packet = new UDPPacket();
        
        try
        {
            Logger.Log($"НАЧАЛО ПОЛУЧЕНИЯ ПАКЕТА");
            for (int count = 0; count < packet.count; count++)
            {
                byte[] receive = udpClient.Receive(ref udpServerEndPoint);
                packet.ReadPacket(receive);
                
                Logger.Log($"Получен {packet.packetType} пакет размером {receive.Length} байт от {udpServerEndPoint}");

                // Добавить проверку корректности данных
                // TODO: Временно отключил Check для проверки
                // udpClient.Send(new byte[2] {(byte)PacketType.Check, 1});
                /*UDPPacket checkPacket = new UDPPacket();
                checkPacket.CreatePacket(PacketType.Check, BitConverter.GetBytes(receive.Length), bufferSize);
                List<byte[]> checkP = checkPacket.CreateBytePacket(bufferSize);
                
                udpClient.Send(checkP[0]);
                Logger.Log($"Отправлен {checkPacket.packetType} пакет размером {checkP[0].Length} байт на {udpServerEndPoint}");*/
                
                // Обновление пинга
                lastPing = DateTime.Now;
            }
            Logger.Log($"{packet.packetType} ПАКЕТ УСПЕШНО ПОЛУЧЕН В РАЗМЕРЕ {packet.data.Length + 2} БАЙТ ОТ {udpServerEndPoint}");
        }
        catch (Exception e)
        {
            packet = new UDPPacket();
            Logger.Log($"Ошибка получения UDP пакета", e);
        }
        
        return packet;
    }
}