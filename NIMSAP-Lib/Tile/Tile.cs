namespace NIMSAP_Lib;

public class Tile
{
    public byte floor;
    public byte wall;
    
    // Создание пустого тайла
    public Tile()
    {
        floor = 0;
        wall = 0;
    }
    
    // Создание тайла с выбором пола и стен
    public Tile(FloorType floorType, WallType wallType)
    {
        floor = (byte)floorType;
        wall = (byte)wallType;
    }    
    // Копирование тайла
    public Tile(Tile tile)
    {
        floor = tile.floor;
        wall = tile.wall;
    }
}