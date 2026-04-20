using System.Collections.Generic;
using UnityEngine;

// ShipView bygger et skib af flere child-sprites (Front/Middle/Back).
// Skibet er altid "vandret" i sit lokale koordinatsystem - rotation håndteres
// ved at rotere hele transform, så sprites følger med.
[RequireComponent(typeof(BoxCollider2D))]
public class ShipView : MonoBehaviour
{
    [Header("Default sprites (bruges hvis ShipConfig ikke har egne)")]
    [SerializeField] private Sprite defaultFrontSprite;
    [SerializeField] private Sprite defaultMiddleSprite;
    [SerializeField] private Sprite defaultBackSprite;

    [Header("Farver til preview")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color validColor = new Color(0.3f, 1f, 0.3f, 0.8f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.8f);

    public ShipModel Model { get; private set; }

    private BoxCollider2D boxCollider;
    private readonly List<SpriteRenderer> partRenderers = new List<SpriteRenderer>();
    private float cellSize = 1.0f;
    private float step = 1.0f;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(ShipModel model, float cellSize, float step)
    {
        this.Model = model;
        this.cellSize = cellSize;
        this.step = step;

        BuildSpriteParts();
        UpdateShape();
        SetVisualState(ShipVisualState.Normal);
    }

    // Rydder gamle child-sprites og bygger nye ud fra skibets størrelse.
    // Layout: Front + (Size-2) × Middle + Back
    private void BuildSpriteParts()
    {
        // Fjern eventuelle gamle parts
        foreach (var r in partRenderers)
        {
            if (r != null) Destroy(r.gameObject);
        }
        partRenderers.Clear();

        // Vælg sprites: model's egne, ellers default fra prefab
        Sprite front = Model.FrontSprite != null ? Model.FrontSprite : defaultFrontSprite;
        Sprite middle = Model.MiddleSprite != null ? Model.MiddleSprite : defaultMiddleSprite;
        Sprite back = Model.BackSprite != null ? Model.BackSprite : defaultBackSprite;

        for (int i = 0; i < Model.Size; i++)
        {
            Sprite partSprite;
            if (Model.Size == 1)
            {
                // Kun én del: brug front
                partSprite = front;
            }
            else if (i == Model.Size - 1)
            {
                // Højre ende = fronten (skibets bov, peger i +X retning)
                partSprite = front;
            }
            else if (i == 0)
            {
                // Venstre ende = back (skibets agter)
                partSprite = back;
            }
            else
            {
                partSprite = middle;
            }

            GameObject partGo = new GameObject($"Part_{i}");
            partGo.transform.SetParent(transform, worldPositionStays: false);

            SpriteRenderer sr = partGo.AddComponent<SpriteRenderer>();
            sr.sprite = partSprite;
            sr.sortingOrder = 1;

            partRenderers.Add(sr);
        }
    }

    // Placerer sprites i en række og opdaterer collider til at dække hele skibet.
    public void UpdateShape()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        // Skibet er altid "vandret" i sit lokale koordinatsystem.
        // Sprites placeres fra venstre til højre med step-afstand.
        // Første parts center: -(Size-1)/2 × step, sidste: +(Size-1)/2 × step
        float startX = -(Model.Size - 1) * step * 0.5f;

        for (int i = 0; i < partRenderers.Count; i++)
        {
            SpriteRenderer sr = partRenderers[i];
            if (sr == null) continue;

            sr.transform.localPosition = new Vector3(startX + i * step, 0f, 0f);

            // Skaler hver sprite så den fylder præcis cellSize × cellSize
            float ppu = sr.sprite != null ? sr.sprite.pixelsPerUnit : 100f;
            float spriteUnitsX = sr.sprite != null ? sr.sprite.rect.width / ppu : 1f;
            float spriteUnitsY = sr.sprite != null ? sr.sprite.rect.height / ppu : 1f;
            sr.transform.localScale = new Vector3(
                cellSize / spriteUnitsX,
                cellSize / spriteUnitsY,
                1f);
        }

        // Collider skal dække hele skibet i lokal horizontal retning
        float length = (Model.Size - 1) * step + cellSize;
        boxCollider.size = new Vector2(length, cellSize);
        boxCollider.offset = Vector2.zero;

        // Rotation: parent-objektet drejes afhængigt af orientering.
        // 0° for Right, 90° for Up, 180° for Left, 270° for Down.
        float zRot = 0f;
        switch (Model.Orientation)
        {
            case ShipOrientation.Right: zRot = 0f; break;
            case ShipOrientation.Up:    zRot = 90f; break;
            case ShipOrientation.Left:  zRot = 180f; break;
            case ShipOrientation.Down:  zRot = 270f; break;
        }
        transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
    }

    public void SetVisualState(ShipVisualState state)
    {
        Color color;
        switch (state)
        {
            case ShipVisualState.Valid: color = validColor; break;
            case ShipVisualState.Invalid: color = invalidColor; break;
            default: color = normalColor; break;
        }
        foreach (var sr in partRenderers)
        {
            if (sr != null) sr.color = color;
        }
    }
}

public enum ShipVisualState
{
    Normal,
    Valid,
    Invalid
}
