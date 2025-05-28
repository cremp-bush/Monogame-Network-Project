using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using NIMSAP_Lib;
using NIMSAP_Lib.Entity;
using NIMSAP_Lib.Map;

namespace NIMSAP_Server;

/* Ядерный хаос */
/*public class Server
{
    static void Main()
    {
        Map map = MapLoader.Load("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\testmap.txt");
        Logger Logger = new Logger();
        UdpClient server;
        IPEndPoint endPoint;
        Thread receiveThread;
        Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
        
        server = new UdpClient(1488); 
        endPoint = new IPEndPoint(IPAddress.Any, 1487);

        Listen();
        
        void Listen()
        {
            try
            {
                Logger.Log("Waiting for broadcast ");

                while (true)
                {
                    foreach (var player in players)
                    {
                        if (player.Value.connected)
                        {
                            if (DateTime.Now.Subtract(player.Value.lastPing).TotalSeconds > 5)
                            {
                                Logger.Log($"{player.Key} disconnected from the server");
                                player.Value.connected = false;
                            }
                        }
                    }
                    if (server.Available > 0)
                    {
                        byte[] packetType = new byte[1];
                        packetType = server.Receive(ref endPoint);

                        switch ((PacketType)packetType[0])
                        {
                            case PacketType.Ping:
                            {
                                Logger.Log($"Ping from {endPoint}");
                                players[endPoint].lastPing = DateTime.Now;
                                server.Send(new byte[1] {(byte)PacketType.Ping}, endPoint);
                                break;
                            }
                            case PacketType.Connection:
                            {
                                server.Send(new byte[1] {(byte)PacketType.Connection}, endPoint);
                                server.Send(PacketAdapter.Pack(map), endPoint);
                                if (!players.ContainsKey(endPoint))
                                {
                                    Logger.Log($"New connection from {endPoint}");
                                    players.Add(endPoint, new Player());
                                }
                                else
                                {
                                    Logger.Log($"Reconnection from {endPoint}");
                                    players[endPoint].connected = true;
                                    players[endPoint].connectTime = DateTime.Now;
                                    players[endPoint].lastPing = DateTime.Now;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Logger.Log($"Приём сообщений был прерван: {e.Message}");
            }
        }
    }
}*/

/*class Server
{
    UdpClient udpServer;
    
    private Map map;
    List<ClientInfo> clients;
    
    void Main()
    {
        Logger.Initialise("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\logs.txt");

        map = MapLoader.Load("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\testmap.txt");
        

        udpServer.BeginReceive(ReceivePackets, null);
    }

    void Connect()
    {
        udpServer = new UdpClient(1489);

        udpServer.Connect();
    }

    void ReceivePackets(IAsyncResult result)
    {
        try
        {
            
        }
    }
}*/

/*public static class Server
{
    public static int maxPlayers;
    public static int port;
    
    private static UdpClient udpListener;
    private static Dictionary<int, UdpClient> clients = new Dictionary<int, UdpClient>();

    public static void StartServer(int _maxPlayers, int _port)
    {
        maxPlayers = _maxPlayers;
        port = _port;
        
        Logger.Log("Запуск сервера...");
        
        udpListener = new UdpClient(port);
        udpListener.BeginReceive(UdpReceive, null);
    }

    private static void UdpReceive(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UdpReceive, null);

            if (_data.Length < 1)
            {
                return;
            }

            Packet _packet = new Packet();
            int _clientId = 0;  //Здесь должен считываться id клиента
            udpListener.endPoint ;
            if (clients[_clientId].) ;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}*/

/* Временный запуск сервера */
public class Aboba
{ 
    static void Main()
    {
        Server testServer = new Server();
        
        Logger.Initialise("logs");

        testServer.StartServer(1488, "127.0.0.1");

        // while (Console.ReadKey().Key != ConsoleKey.Escape);
        while (true) ;
    }
}

/* TODO: Решить пользоваться ли TCP или только свойский UDP */
/* Основной класс сервера */
public class Server
{
    public TcpListener tcpServer; // Test dont use now, use only UDP
    public UdpClient udpServer;
    // public IPEndPoint udpClientEndPoint;

    private short tick = 60;
    private int port;
    private IPEndPoint ip;
    private int bufferSize;

    // private Dictionary<TcpClient, Player> tcpClients;    // Капец
    private Dictionary<IPEndPoint, Player> udpClients;
    
    private Map map;

    public void StartServer(int port, string ip)
    {
        this.port = port;
        this.ip = IPEndPoint.Parse(ip + ":" + port);
        bufferSize = 4096;
        // tcpClients = new Dictionary<TcpClient, Player>();
        udpClients = new Dictionary<IPEndPoint, Player>();
        
        map = MapLoader.Load("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\testmap.txt");
        // map = PacketAdapter.Unpack<Map>(PacketAdapter.Pack(map));
        Logger.Log("Запуск сервера");
        
        Thread udpThread = new Thread(UdpServer);
        // Thread tcpThread = new Thread(TcpServer);
        Thread gameLogic = new Thread(GameLogic);

        udpThread.Start();
        gameLogic.Start();
    }

    /* Игровая логика */
    async void GameLogic()
    {
        PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000/tick));
        while (await periodicTimer.WaitForNextTickAsync())
        {
            // Обработка движений игроков
            foreach (Player player in udpClients.Values)
            {
                if (player.connected == true)
                {
                    if (player.motion != Vector2.Zero)
                    {
                        Creature creature = map.GetEntity(player.entityId) as Creature;
                        Console.WriteLine(creature.position);
                        // Движение
                        creature.position += player.motion * 0.1f;
                        // Разворотики и поворотики
                        if (player.lastMotion != player.motion)
                        {
                            if (player.motion.X != 0)
                            {
                                if (player.lastMotion.X != player.motion.X)
                                {
                                    if (player.motion.X < 0) creature.rotation = 2;
                                    else creature.rotation = 4;
                                }
                                else
                                {
                                    if (player.motion.X < 0) creature.rotation = 2;
                                    else creature.rotation = 4;
                                }
                            }
                            else if (player.motion.Y != 0)
                            {
                                if (player.lastMotion.Y != player.motion.Y)
                                {
                                    if (player.motion.Y < 0) creature.rotation = 1;
                                    else creature.rotation = 3;
                                }
                                else
                                {
                                    if (player.motion.Y < 0) creature.rotation = 1;
                                    else creature.rotation = 3;
                                }
                            }

                            player.lastMotion = player.motion;
                        }
                        
                        map.UpdateEntity(creature);
                        UdpSendPacketToClients(PacketType.UpdateEntity, PacketAdapter.Pack(creature));
                    }
                }
            }
        }
    }
    
    /* TODO: TCP сервер же нужен? Правда?? */
    /*public async void TcpServer()
    {
        try
        {
            tcpServer = new TcpListener(IPAddress.Any, port);
            tcpServer.Start();

            while (true)
            {
                TcpClient tcpClient = await tcpServer.AcceptTcpClientAsync();

                if (clients.ContainsKey(tcpClient))
                {
                    Logger.Log($"Клиент {tcpClient.Client.RemoteEndPoint} снова подключился");
                }
                else
                {
                    Logger.Log($"Новый клиент {tcpClient.Client.RemoteEndPoint} подключился");
                }
                tcpClient.Client.Disconnect(true);

                foreach (var client in clients)
                {
                    client.Key
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log("Ошибка TCP потока: " + e);
        }
    }*/
    
    /* Прослушивание клиентов */
    public void UdpServer()
    {
        Logger.Log("Запущен UDP поток");
        try
        {
            udpServer = new UdpClient(port);
            udpServer.Client.SendTimeout = 5000;
            udpServer.Client.ReceiveTimeout = 5000;
            
            // while (udpServer.Available > 0) udpServer.Receive(ref udpClientEndPoint);
            
            // udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                foreach (var client in udpClients)
                {
                    if (client.Value.connected == true)
                    {
                        if (DateTime.Now.Subtract(client.Value.lastPing).TotalSeconds > 5)
                        {
                            Logger.Log($"Клиент {client.Value.guid} не отправлял пинги более 5000 мс");
                            DisconnectClientOverride(client.Key);
                        }
                    }
                }
                if (udpServer.Available > 0)
                {
                    var packet = UdpGetPacket();

                    switch (packet.packetType)
                    {
                        case PacketType.Connection:
                        {
                            // Logger.Log("Получен запрос подключения");
                            ConnectClient(packet.endPoint, new Guid(packet.data));
                            
                            UdpSendPacket(PacketType.Connection, packet.endPoint, BitConverter.GetBytes(udpClients[packet.endPoint].entityId));
                            break;
                        }
                        case PacketType.Ping:
                        {
                            // Logger.Log("Получен пинг");
                            
                            // UdpSendPacket(PacketType.Ping);

                            if (udpClients.ContainsKey(packet.endPoint))
                            {
                                udpClients[packet.endPoint].lastPing = DateTime.Now;
                            }
                            break;
                        }
                        case PacketType.Disconnection:
                        {
                            // Logger.Log("Получен запрос отключения");
                            
                            DisconnectClient(packet.endPoint);
                            break;
                        }
                        case PacketType.Map:
                        {
                            // Logger.Log("Получен запрос карты");

                            UdpSendPacket(PacketType.Map, packet.endPoint, PacketAdapter.Pack(map));
                            break;
                        }
                        // TODO: Заменить упрощённое полноценным
                        case PacketType.InputMotion:
                        {
                            byte[] data = packet.data;
                            udpClients[packet.endPoint].motion = new Vector2(packet.data[0]-128, packet.data[1]-128);
                            Console.WriteLine($"User Input: {udpClients[packet.endPoint].motion}");
                            
                            // Просто ужас
                            /*switch ((InputType)packet.data[0])
                            {
                                case InputType.MotionUpLeft:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(-1, 1);
                                    break;
                                }                                
                                case InputType.MotionUp:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(0, 1);
                                    break;
                                }                                
                                case InputType.MotionUpRight:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(1, 1);
                                    break;
                                }                                
                                case InputType.MotionLeft:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(-1, 0);
                                    break;
                                }
                                case InputType.MotionStop:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(0, 0);
                                    break;
                                }
                                case InputType.MotionRight:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(1, 0);
                                    break;
                                }
                                case InputType.MotionDownLeft:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(-1, -1);
                                    break;
                                }                                
                                case InputType.MotionDown:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(0, -1);
                                    break;
                                }                                
                                case InputType.MotionDownRight:
                                {
                                    udpClients[udpClientEndPoint].motion = new Vector2(1, -1);
                                    break;
                                } 
                            }*/
                            break;
                        }
                    }
                }
            }
        }

        catch (Exception e)
        {
            Logger.Log("Ошибка UDP потока", e);
        }
    }

    /* TODO: Добавить действия по попытке передать данные снова */
    /* Отправка пакетов */
    async void UdpSendPacket(PacketType packetType, IPEndPoint endPoint, byte[] data = null)
    {
        try
        {
            UDPPacket packet = new UDPPacket();
            packet.CreatePacket(packetType, data, bufferSize);

            List<byte[]> packets = packet.CreateBytePacket(bufferSize);

            Logger.Log($"НАЧАЛО ОТПРАВКИ {packetType} ПАКЕТА К {endPoint}");
            foreach (byte[] sendPacket in packets)
            {
                udpServer.Send(sendPacket, endPoint);
                
                Logger.Log($"Отправлен {packet.packetType} пакет размером {sendPacket.Length} байт на {endPoint}");
                
                // Проверка целостности отправленного пакета
                // TODO: Временно отключил Check для проверки
                Thread.Sleep(5);
                /*byte[] buffer = udpServer.Receive(ref endPoint);

                UDPPacket checkPacket = new UDPPacket(); 
                checkPacket.ReadPacket(buffer);

                if (checkPacket.packetType != PacketType.Check)
                {
                    throw new Exception("Data is corrupted");
                }

                int count = BitConverter.ToInt32(checkPacket.data);
            
                Logger.Log($"Клиент {endPoint} получил {count} байт");
                
                if (count != sendPacket.Length)
                {
                    // Добавить действия при потере данных пакета
                    Logger.Log($"Клиент {endPoint} получил неверные данные: {count} из {sendPacket.Length}");
                    throw new Exception("Data is corrupted");
                }*/
                
                // Обновляем пинг клиента
                if (udpClients.ContainsKey(endPoint)) udpClients[endPoint].lastPing = DateTime.Now;
            }
            Logger.Log($"{packet.packetType} ПАКЕТ УСПЕШНО ОТПРАВЛЕН В РАЗМЕРЕ {packet.data.Length + 2} БАЙТ К {endPoint}");
        }
        catch (Exception e)
        {
            Logger.Log($"Ошибка отправки UDP пакета", e);
        }
    }
    
    /* Отправка пакетов всем подключённым клиентам */
    async void UdpSendPacketToClients(PacketType packetType, byte[] data = null)
    {
        foreach (var client in udpClients)
        {
            if (client.Value.connected == true)
            {
                UdpSendPacket(packetType, client.Key, data);
            }
        }
    }

    /* TODO: Добавить действия по попытке получить данные снова */
    /* Получение пакетов */
    UDPPacket UdpGetPacket()
    {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UDPPacket packet = new UDPPacket();
        try
        {
            Logger.Log($"НАЧАЛО ПОЛУЧЕНИЯ ПАКЕТА");
            for (int count = 0; count < packet.count; count++)
            {
                byte[] buffer = udpServer.Receive(ref clientEndPoint);
                
                packet.ReadPacket(buffer);
                
                Logger.Log($"Получен {packet.packetType} пакет размером {buffer.Length} байт от {clientEndPoint}");
            }
            packet.endPoint = clientEndPoint;
            Logger.Log($"{packet.packetType} ПАКЕТ УСПЕШНО ПОЛУЧЕН В РАЗМЕРЕ {packet.data.Length + 2} БАЙТ ОТ {clientEndPoint}");
        }
        catch (Exception e)
        {
            Logger.Log("Ошибка получения UDP пакета", e);
            
            DisconnectClientOverride(clientEndPoint);
        }
        return packet;
    }

    /* Подключение клиента к серверу */
    void ConnectClient(IPEndPoint endPoint, Guid guid)
    {
        // Проверка наличия клиента в словаре по IP
        if (udpClients.ContainsKey(endPoint))
        {
            udpClients[endPoint].connected = true;
            udpClients[endPoint].lastPing = DateTime.Now;
            Logger.Log($"Клиент {guid} переподключился");
        }
        else
        {
            bool exist = false;
            // Проверка наличия клиента в словаре по Guid
            foreach (var client in udpClients)
            {
                if (client.Value.guid == guid)
                {
                    Player player = client.Value;
                    player.connected = true;
                    player.lastPing = DateTime.Now;
                    
                    // Удалить старый IP и отсоединить клиента если он подключен
                    UdpSendPacket(PacketType.Disconnection, client.Key);
                    udpClients.Remove(client.Key);
                    // Добавить того же игрока с новым IP
                    udpClients.Add(endPoint, player);
                    
                    Logger.Log($"Клиент {guid} зашёл с новым IP адресом");

                    exist = true;
                    break;
                }
            }
            // Подключение нового клиента к серверу
            if (!exist)
            {
                // Создание нового человека
                Player player = new Player();
                player.guid = guid;
                
                Human human = Human.CreateEntity(Vector2.One, guid);
                
                player.entityId = map.AddEntity(human);
                
                Logger.Log($"Создан новый игрок");

                // Отправка нового человека всем подключённым клиентам
                UdpSendPacketToClients(PacketType.CreateEntity, PacketAdapter.Pack(human));
                
                // Добавление клиента в словарь клиентов
                udpClients.Add(endPoint, player);
                
                Logger.Log($"Новый клиент {guid} зашёл");
            }
        }
    }

    /* Разрыв связи сервера с клиентом */
    void DisconnectClient(IPEndPoint clientEndPoint)
    {
        if (udpClients.ContainsKey(clientEndPoint))
        {
            udpClients[clientEndPoint].connected = false;
            Logger.Log($"Клиент {clientEndPoint} отключился");
        }
    }
    
    /* Принудительный разрыв связи сервера с клиентом */
    void DisconnectClientOverride(IPEndPoint clientEndPoint)
    {
        if (udpClients.ContainsKey(clientEndPoint))
        {
            udpClients[clientEndPoint].connected = false;
            Logger.Log($"Клиент {clientEndPoint} потерял соединение с сервером");
        }
    }
}