using System.Collections.Generic;

public class BoardModel
{
    public int Rows { get; }
    public int Cols { get; }
    public bool AllowTouching { get; set; } = true;

    private readonly Cell[,] cells;
    private readonly List<Ship> ships = new List<Ship>();

    public IReadOnlyList<Ship> Ships => ships;

    public BoardModel(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        cells = new Cell[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                cells[r, c] = new Cell();
    }

    public Cell GetCell(Position pos) => cells[pos.Row, pos.Col];

    public bool IsInside(Position pos) =>
        pos.Row >= 0 && pos.Row < Rows && pos.Col >= 0 && pos.Col < Cols;

    // Tjekker om et skib kan placeres på de givne positioner.
    // ignoreShip kan være null. Hvis sat, ignoreres celler optaget af det skib
    // (bruges når et eksisterende skib flyttes).
    public bool CanPlaceShip(List<Position> positions, Ship ignoreShip = null)
    {
        foreach (var pos in positions)
        {
            if (!IsInside(pos)) return false;

            ShipPart partHere = cells[pos.Row, pos.Col].ShipPart;
            if (partHere != null && partHere.Ship != ignoreShip) return false;
        }

        if (!AllowTouching)
        {
            foreach (var pos in positions)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        Position neighbor = new Position(pos.Row + dr, pos.Col + dc);
                        if (!IsInside(neighbor)) continue;

                        // En nabocelle med et andet skibs part = ulovligt
                        ShipPart neighborPart = cells[neighbor.Row, neighbor.Col].ShipPart;
                        if (neighborPart != null && neighborPart.Ship != ignoreShip) return false;
                    }
                }
            }
        }

        return true;
    }

    public bool PlaceShip(Ship ship, List<Position> positions)
    {
        if (!CanPlaceShip(positions)) return false;

        ship.PlaceAt(positions);
        foreach (var part in ship.Parts)
        {
            cells[part.Position.Row, part.Position.Col].ShipPart = part;
            cells[part.Position.Row, part.Position.Col].SetState(CellState.Ship);
        }
        ships.Add(ship);
        return true;
    }

    // Fjerner et skib fra brættet. Cellerne bliver tomme igen.
    public void RemoveShip(Ship ship)
    {
        if (!ships.Contains(ship)) return;

        foreach (var part in ship.Parts)
        {
            Cell c = cells[part.Position.Row, part.Position.Col];
            c.ShipPart = null;
            c.SetState(CellState.Empty);
        }
        ships.Remove(ship);
    }

    public ShotResult Shoot(Position pos)
    {
        if (!IsInside(pos)) return ShotResult.Miss;
        return cells[pos.Row, pos.Col].Hit();
    }

    public bool AllShipsSunk()
    {
        if (ships.Count == 0) return false;
        foreach (var ship in ships)
            if (!ship.IsSunk()) return false;
        return true;
    }
}
