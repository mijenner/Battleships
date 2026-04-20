public class ShipPart
{
    public Ship Ship { get; }
    public Position Position { get; }
    public bool IsHit { get; private set; }

    public ShipPart(Ship ship, Position position)
    {
        Ship = ship;
        Position = position;
    }

    public void Hit()
    {
        IsHit = true;
    }
}
