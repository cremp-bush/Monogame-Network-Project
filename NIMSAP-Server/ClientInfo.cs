using System.Net;
using System.Net.Sockets;

namespace NIMSAP_Server;

public struct ClientInfo
{
    string name;
    bool active;
    TcpClient tcp;
    IPEndPoint udp;
}