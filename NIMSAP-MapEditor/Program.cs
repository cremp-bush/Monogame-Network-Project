
using NIMSAP_Lib;
using NIMSAP_Lib.Map;

Map map = new Map(100, 100);

// Создание мира из колец пола без стен
for(int y = 0; y < 100; y++)
{
    for (int x = 0; x < 100; x++)
    {
        if (x % 2 == 0 && y % 2 == 0) map.tileMap[x, y] = new Tile(FloorType.NoneFloor, WallType.NoneWall);
        else map.tileMap[x, y] = new Tile(FloorType.SteelFloor, WallType.NoneWall);
    }
}

MapLoader.Save("D:\\projects\\Codename NIMSAP\\NIMSAP-Server\\testmap.txt", map);