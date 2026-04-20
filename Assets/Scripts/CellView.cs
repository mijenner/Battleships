using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CellView : MonoBehaviour
{
    [Header("Farver pr. celle-tilstand")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.5f, 0.9f);   // blå vand
    [SerializeField] private Color shipColor = new Color(0.5f, 0.5f, 0.5f);   // grå skib
    [SerializeField] private Color missColor = new Color(1.0f, 1.0f, 1.0f);   // hvid plet
    [SerializeField] private Color hitColor = new Color(1.0f, 0.5f, 0.0f);   // orange
    [SerializeField] private Color sunkColor = new Color(0.8f, 0.1f, 0.1f);   // rød

    public Position Position { get; private set; }
    public BoardView OwnerBoard { get; private set; }

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Kaldes af BoardView lige efter instansiering
    public void Initialize(BoardView owner, Position position)
    {
        OwnerBoard = owner;
        Position = position;
        SetState(CellState.Empty, showShip: false);
    }

    public void SetState(CellState state, bool showShip)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (state)
        {
            case CellState.Empty: spriteRenderer.color = emptyColor; break;
            case CellState.Ship: spriteRenderer.color = showShip ? shipColor : emptyColor; break;
            case CellState.Miss: spriteRenderer.color = missColor; break;
            case CellState.Hit: spriteRenderer.color = hitColor; break;
            case CellState.Sunk: spriteRenderer.color = sunkColor; break;
        }
    }
}
