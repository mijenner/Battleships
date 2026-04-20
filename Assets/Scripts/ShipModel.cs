using System.Collections.Generic;

public enum ShipOrientation
{
    Horizontal, // skibet strækker sig hen ad kolonner (samme række)
    Vertical    // skibet strækker sig ned ad rækker (samme kolonne)
}

// Repræsenterer et skib der endnu ikke er placeret på brættet,
// eller som er placeret men kan flyttes.
public class ShipModel
{
    public string Name { get; }
    public int Size { get; }
    public ShipOrientation Orientation { get; private set; } = ShipOrientation.Horizontal;

    // Skibets nuværende "anker" - top-venstre celle. Kan være null før placering.
    public Position? Anchor { get; private set; }

    public bool IsPlaced => Anchor.HasValue;

    public ShipModel(string name, int size)
    {
        Name = name;
        Size = size;
    }

    public void Rotate()
    {
        Orientation = Orientation == ShipOrientation.Horizontal
            ? ShipOrientation.Vertical
            : ShipOrientation.Horizontal;
    }

    public void SetAnchor(Position anchor)
    {
        Anchor = anchor;
    }

    public void ClearAnchor()
    {
        Anchor = null;
    }

    // Beregner alle de positioner skibet ville dække hvis det blev placeret med
    // anker'et på den givne position med den nuværende orientering.
    public List<Position> GetPositionsAt(Position anchor)
    {
        var positions = new List<Position>(Size);
        for (int i = 0; i < Size; i++)
        {
            int r = Orientation == ShipOrientation.Horizontal ? anchor.Row : anchor.Row + i;
            int c = Orientation == ShipOrientation.Horizontal ? anchor.Col + i : anchor.Col;
            positions.Add(new Position(r, c));
        }
        return positions;
    }

    // Bekvem genvej når skibet allerede er placeret
    public List<Position> GetCurrentPositions()
    {
        if (!Anchor.HasValue) return new List<Position>();
        return GetPositionsAt(Anchor.Value);
    }
}
