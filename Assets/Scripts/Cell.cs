public enum CellState
{
    Empty,
    Ship,
    Miss,
    Hit,
    Sunk
}

public class Cell
{
    public CellState State { get; private set; } = CellState.Empty;
    public ShipPart ShipPart { get; set; }

    public void SetState(CellState state)
    {
        State = state;
    }

    public ShotResult Hit()
    {
        if (ShipPart == null)
        {
            State = CellState.Miss;
            return ShotResult.Miss;
        }

        ShipPart.Hit();
        if (ShipPart.Ship.IsSunk())
        {
            State = CellState.Sunk;
            return ShotResult.Sunk;
        }
        State = CellState.Hit;
        return ShotResult.Hit;
    }
}

