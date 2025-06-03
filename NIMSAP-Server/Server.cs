using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Xna.Framework;
using NIMSAP_Lib;
using NIMSAP_Lib.Entity;
using NIMSAP_Lib.Map;

namespace NIMSAP_Server;

// TODO: Доабвить коллайдеры и хитбоксы сущностям
// TODO: Добавить инвентарь игроку и возможность подбирать предметы
// TODO: Добавить препятствия сущностям (стены)
// TODO: Добавить гранаты
// TODO: Добавить интерфейс для отладки!!!
// TODO: Пофиксить ходьбу в далёкие края чтобы не крашило
// TODO: Интерполяция!!!
// TODO: Снизить нагрузку на сеть от ходьбы!!!

/* Временный запуск сервера */
public class Aboba
{ 
    static void Main()
    {
        Server testServer = new Server();
        
        Logger.Initialise("logs");

        testServer.StartServer(1488, "127.0.0.1");

        while (Console.ReadKey().Key != ConsoleKey.Escape);
    }
}

/* TODO: Решить пользоваться ли TCP или только свойский UDP */
/* Основной класс сервера */
public class Server
{
    private UdpClient udpServer;
    private IPEndPoint ip;
    
    private Map map;
    private short tick = 20;
    private int bufferSize;
    
    private Dictionary<IPEndPoint, Player> udpClients;
    
    public void StartServer(int port, string ip)
    {
        this.ip = IPEndPoint.Parse(ip + ":" + port);
        bufferSize = 4096;
        udpClients = new Dictionary<IPEndPoint, Player>();

        udpServer = new UdpClient(this.ip);
        
        map = MapLoader.Load("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\testmap.txt");
        Logger.Log("Запуск сервера");
        
        Thread listener = new Thread(Listener);
        Thread gameLogic = new Thread(GameLogic);

        listener.Start();
        gameLogic.Start();
    }

    /* Игровая логика */
    async void GameLogic()
    {
        GameTime gameTime = new GameTime();
        PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000/tick));
        while (await periodicTimer.WaitForNextTickAsync())
        {
            // Обработка движений игроков
            foreach (Player player in udpClients.Values.ToList())
            {
                if (player.connected == true && player.loaded == true)
                {
                    if (player.motion != Vector2.Zero)
                    {
                        Creature creature = map.GetEntity(player.entityId) as Creature;
                        // Движение
                        creature.position += player.motion * 0.5f;
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
            // gameTime = gameTime.TotalGameTime + gameTime.ElapsedGameTime;
        }
    }
    
    /* Прослушивание клиентов (многопоток!) */
    async void Listener()
    {
        Logger.Log("Начало прослушивания UDP клиентов");

        while (true)
        {
            // Проверка на статус подключения клиентов
            foreach (var client in udpClients.Values.ToList())
            {
                if (client.connected == true)
                {
                    if (DateTime.Now.Subtract(client.lastPing).TotalSeconds > 10)
                    {
                        Logger.Log($"Клиент {client.guid} не отправлял пинги более 10000 мс");
                        DisconnectClientOverride(client.endPoint);
                    }
                }
            }

            try
            {
                if (udpServer.Available > 0)
                {
                    // Получаем пакет
                    UDPPacket packet = UdpGetPacket();
                    Task.Run(() => PacketHandler(packet));
                }
            }
            catch (Exception e)
            {
                Logger.Log("Ошибка прослушивания клиентов", e);
            }
        }
    }

    async Task PacketHandler(UDPPacket packet)
    {
        switch (packet.packetType)
        {
            // Подключение клиента к серверу
            case PacketType.Connection:
            {
                ConnectClient(packet.endPoint, new Guid(packet.data));
                // Отправляем пакет об успешном подключении
                UdpSendPacket(PacketType.Connection, packet.endPoint, BitConverter.GetBytes(udpClients[packet.endPoint].entityId));
                // Отправляем карту
                UdpSendPacket(PacketType.Map, packet.endPoint, PacketAdapter.Pack(map));
                udpClients[packet.endPoint].loaded = true;
                
                break;
            }
            // Проверка соединения клиента с сервером
            case PacketType.Ping:
            {
                // Обновляем время последнего пинга клиенту
                udpClients[packet.endPoint].lastPing = DateTime.Now;
                // Отправляем пинг
                UdpSendPacket(PacketType.Ping, packet.endPoint);

                break;
            }
            // Запрос игровой карты
            case PacketType.Map:
            {
                // Отправляем карту
                UdpSendPacket(PacketType.Map, packet.endPoint, PacketAdapter.Pack(map));

                break;
            }
            // Проверочный пакет
            case PacketType.Check:
            {
                // Подаём сигнал о успешном получении пакета клиентом
                udpClients[packet.endPoint].checkKey = BitConverter.ToInt32(packet.data);
                udpClients[packet.endPoint].check.Set();

                break;
            }
            // Запрос отключения клиента
            case PacketType.Disconnection:
            {
                // Отключаем клиента
                DisconnectClient(packet.endPoint);

                break;
            }
            // Пакет управления клиента
            // TODO: Заменить упрощённое полноценным
            case PacketType.InputMotion:
            {
                // Меняем движение персонажу клиента
                udpClients[packet.endPoint].motion = new Vector2(packet.data[0] - 128, packet.data[1] - 128);

                break;
            }
        }
    }

    /* TODO: Добавить действия по попытке передать данные снова */
    /* Отправка пакетов */
    void UdpSendPacket(PacketType packetType, IPEndPoint endPoint, byte[] data = null)
    {
        try
        {
            UDPPacket packet = new UDPPacket();
            packet.CreatePacket(packetType, data, bufferSize);

            List<byte[]> packets = packet.CreateBytePacket(bufferSize);

            Logger.Log($"НАЧАЛО ОТПРАВКИ {packetType} ПАКЕТА К {endPoint}");
            foreach (byte[] sendPacket in packets)
            {
                if (udpClients[endPoint].connected == false) throw new Exception("Sending stopped, client not connected");
                udpServer.Send(sendPacket, endPoint);
                
                Logger.Log($"Отправлен {packet.packetType} пакет размером {sendPacket.Length} байт на {endPoint}");
                
                // Проверка целостности отправленного пакета
                if (udpClients[endPoint].check.WaitOne(10000) == false) throw new Exception("Check packet not delivered");

                int count = udpClients[endPoint].checkKey;
                Logger.Log($"Клиент {endPoint} получил {count} байт");
                 
                if (count != sendPacket.Length)
                { 
                    // TODO: Добавить действия при потере данных пакета
                    Logger.Log($"Клиент {endPoint} получил неверные данные: {count} из {sendPacket.Length}");
                    throw new Exception("Data is corrupted");
                }
                
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
    void UdpSendPacketToClients(PacketType packetType, byte[] data = null)
    {
        foreach (var client in udpClients.Values.ToList())
        {
            if (client.connected == true)
            {
                UdpSendPacket(packetType, client.endPoint, data);
            }
        }
    }
    
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
            udpClients[endPoint].endPoint = endPoint;
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
                    player.endPoint = endPoint;
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
                player.connected = true;
                player.endPoint = endPoint;
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
            udpClients[clientEndPoint].check.Set();
            udpClients[clientEndPoint].connected = false;
            udpClients[clientEndPoint].loaded = false;
            Logger.Log($"Клиент {clientEndPoint} отключился");
        }
    }
    
    /* Принудительный разрыв связи сервера с клиентом */
    void DisconnectClientOverride(IPEndPoint clientEndPoint)
    {
        if (udpClients.ContainsKey(clientEndPoint))
        {
            udpClients[clientEndPoint].check.Set();
            udpClients[clientEndPoint].connected = false;
            udpClients[clientEndPoint].loaded = false;
            Logger.Log($"Клиент {clientEndPoint} потерял соединение с сервером");
        }
    }
}