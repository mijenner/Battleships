using System.Collections.Generic;
using UnityEngine;

// Fire orienteringer svarende til hvilken retning skibets BOV peger.
// Bemærk at "Right" dækker samme celler som "Left", blot visuelt spejlvendt.
// Ligesådan for Up og Down. For brættets logik er de parvis ens, men visuelt
// skal skibet pege forskelligt.
public enum ShipOrientation
{
    Right,  // bov peger mod højre, celler strækker sig i +col retning
    Up,     // bov peger opad, celler strækker sig i +row retning
    Left,   // bov peger mod venstre, celler strækker sig i +col retning (samme som Right)
    Down    // bov peger nedad, celler strækker sig i +row retning (samme som Up)
}

public class ShipModel
{
    public string Name { get; }
    public int Size { get; }
    public ShipOrientation Orientation { get; private set; } = ShipOrientation.Right;
    public Position? Anchor { get; private set; }
    public bool IsPlaced => Anchor.HasValue;

    public Sprite FrontSprite { get; }
    public Sprite MiddleSprite { get; }
    public Sprite BackSprite { get; }

    public ShipModel(string name, int size,
        Sprite frontSprite = null, Sprite middleSprite = null, Sprite backSprite = null)
    {
        Name = name;
        Size = size;
        FrontSprite = frontSprite;
        MiddleSprite = middleSprite;
        BackSprite = backSprite;
    }

    // Roterer 90 grader mod uret: Right -> Up -> Left -> Down -> Right
    public void Rotate()
    {
        Orientation = (ShipOrientation)(((int)Orientation + 1) % 4);
    }

    // Er skibet vandret eller lodret (for celle-beregning)?
    public bool IsHorizontal => Orientation == ShipOrientation.Right || Orientation == ShipOrientation.Left;

    public void SetAnchor(Position anchor) => Anchor = anchor;
    public void ClearAnchor() => Anchor = null;

    // Beregner hvilke celler et skib dækker ved en given anker-position.
    // Ankeret er altid top-left cellen i den dækkede rektangel (laveste row, laveste col).
    public List<Position> GetPositionsAt(Position anchor)
    {
        var positions = new List<Position>(Size);
        for (int i = 0; i < Size; i++)
        {
            int r = IsHorizontal ? anchor.Row : anchor.Row + i;
            int c = IsHorizontal ? anchor.Col + i : anchor.Col;
            positions.Add(new Position(r, c));
        }
        return positions;
    }

    public List<Position> GetCurrentPositions()
    {
        if (!Anchor.HasValue) return new List<Position>();
        return GetPositionsAt(Anchor.Value);
    }
}
