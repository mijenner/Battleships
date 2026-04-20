using System.Collections.Generic;
using System.Linq;

public class Ship
{
    public string Name { get; }
    public int Size { get; }
    public List<ShipPart> Parts { get; } = new List<ShipPart>();

    public Ship(string name, int size)
    {
        Name = name;
        Size = size;
    }

    public void PlaceAt(List<Position> positions)
    {
        Parts.Clear();
        foreach (var pos in positions)
        {
            Parts.Add(new ShipPart(this, pos));
        }
    }

    public bool IsSunk()
    {
        return Parts.Count > 0 && Parts.All(p => p.IsHit);
    }
}
