using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ShipView : MonoBehaviour
{
    [Header("Farver")]
    [SerializeField] private Color normalColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color validColor = new Color(0.3f, 0.9f, 0.3f, 0.7f);
    [SerializeField] private Color invalidColor = new Color(0.9f, 0.3f, 0.3f, 0.7f);

    public ShipModel Model { get; private set; }

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float cellSize = 1.0f;
    private float step = 1.0f; // cellSize + cellSpacing fra BoardView

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // cellSize = selve cellens størrelse (skibets "tykkelse")
    // step = afstand fra cellecenter til nabocellecenter (cellSize + cellSpacing)
    public void Initialize(ShipModel model, float cellSize, float step)
    {
        this.Model = model;
        this.cellSize = cellSize;
        this.step = step;
        UpdateShape();
        SetVisualState(ShipVisualState.Normal);
    }

    public void UpdateShape()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        // Skibet skal dække Size celler med deres mellemrum imellem sig:
        // længde = (Size-1) * step + cellSize
        // Tykkelse i tværretning = cellSize
        float length = (Model.Size - 1) * step + cellSize;

        if (Model.Orientation == ShipOrientation.Horizontal)
        {
            transform.localScale = new Vector3(length, cellSize, 1f);
        }
        else
        {
            transform.localScale = new Vector3(cellSize, length, 1f);
        }

        // Sørg for at BoxCollider2D altid dækker hele sprite'n (localScale gør arbejdet)
        boxCollider.size = Vector2.one;
        boxCollider.offset = Vector2.zero;
    }

    public void SetVisualState(ShipVisualState state)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (state)
        {
            case ShipVisualState.Normal:  spriteRenderer.color = normalColor; break;
            case ShipVisualState.Valid:   spriteRenderer.color = validColor; break;
            case ShipVisualState.Invalid: spriteRenderer.color = invalidColor; break;
        }
    }
}

public enum ShipVisualState
{
    Normal,
    Valid,
    Invalid
}
