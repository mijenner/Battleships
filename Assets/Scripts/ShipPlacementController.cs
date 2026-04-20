using UnityEngine;
using UnityEngine.InputSystem;

// Håndterer drag-and-drop af skibe under placerings-fasen.
// Spilleren kan trække skibe fra panelet til brættet, rotere med R-tasten,
// og flytte allerede placerede skibe.
public class ShipPlacementController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoardView playerBoardView;
    [SerializeField] private ShipPanel shipPanel;
    [SerializeField] private Camera sceneCamera;

    // Aktiv kun under Placing-state
    public bool IsActive { get; set; } = false;

    private ShipView draggedShip;
    private Vector3 dragOffset; // hvor på skibet der blev klikket (i lokale enheder)
    private Position? lastPreviewAnchor;

    void Start()
    {
        if (sceneCamera == null) sceneCamera = Camera.main;
    }

    void Update()
    {
        if (!IsActive) return;
        if (Mouse.current == null) return;

        // Start drag
        if (Mouse.current.leftButton.wasPressedThisFrame && draggedShip == null)
        {
            TryStartDrag();
        }

        // Under drag
        if (draggedShip != null)
        {
            UpdateDrag();

            // Rotation under drag
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                draggedShip.Model.Rotate();
                draggedShip.UpdateShape();
                // Efter rotation: centrer skibet under musen så offset ikke bliver underlig
                dragOffset = Vector3.zero;
                UpdateDrag();
            }

            // Slip
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndDrag();
            }
        }
    }

    private void TryStartDrag()
    {
        Vector3 worldPos = ScreenToWorld(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return;

        ShipView ship = hit.collider.GetComponent<ShipView>();
        if (ship == null) return;

        draggedShip = ship;
        dragOffset = ship.transform.position - worldPos;

        // Hvis skibet allerede er placeret, fjern det fra brættet
        // (vi placerer det igen når spilleren slipper - eller sender det tilbage til panelet)
        if (ship.Model.IsPlaced)
        {
            BoardModel board = playerBoardView.Model;
            // Find det Ship-objekt der svarer til denne ShipModel
            Ship boardShip = FindBoardShipFor(ship.Model);
            if (boardShip != null) board.RemoveShip(boardShip);
            ship.Model.ClearAnchor();
            playerBoardView.Render();
        }
    }

    private void UpdateDrag()
    {
        Vector3 worldPos = ScreenToWorld(Mouse.current.position.ReadValue());
        Vector3 followPos = worldPos + dragOffset;

        // Forsøg at snappe til nærmeste celle på brættet
        Position? anchor = WorldToBoardAnchor(followPos);

        if (anchor.HasValue)
        {
            // Skibet sidder over brættet - vis preview-snap til celler
            var positions = draggedShip.Model.GetPositionsAt(anchor.Value);
            BoardModel board = playerBoardView.Model;
            bool valid = board.CanPlaceShip(positions, ignoreShip: null);

            // Læg skibet på brættets verdens-position for ankerets celle
            CellView anchorCell = playerBoardView.GetCellView(anchor.Value);
            draggedShip.transform.position = ComputeShipWorldPos(anchorCell, draggedShip.Model);

            draggedShip.SetVisualState(valid ? ShipVisualState.Valid : ShipVisualState.Invalid);
            lastPreviewAnchor = anchor;
        }
        else
        {
            // Uden for brættet - skibet følger musen frit
            draggedShip.transform.position = followPos;
            draggedShip.SetVisualState(ShipVisualState.Normal);
            lastPreviewAnchor = null;
        }
    }

    private void EndDrag()
    {
        if (draggedShip == null) return;

        BoardModel board = playerBoardView.Model;

        if (lastPreviewAnchor.HasValue)
        {
            var positions = draggedShip.Model.GetPositionsAt(lastPreviewAnchor.Value);
            if (board.CanPlaceShip(positions))
            {
                // Gyldig placering - opret et Ship og læg det på brættet
                Ship ship = new Ship(draggedShip.Model.Name, draggedShip.Model.Size);
                if (board.PlaceShip(ship, positions))
                {
                    draggedShip.Model.SetAnchor(lastPreviewAnchor.Value);
                    draggedShip.SetVisualState(ShipVisualState.Normal);
                    playerBoardView.Render();
                }
                else
                {
                    shipPanel.ReturnShip(draggedShip);
                }
            }
            else
            {
                // Ugyldig - send tilbage til panelet
                shipPanel.ReturnShip(draggedShip);
            }
        }
        else
        {
            // Sluppet uden for brættet - send tilbage til panelet
            shipPanel.ReturnShip(draggedShip);
        }

        draggedShip = null;
        lastPreviewAnchor = null;

        // Notify GameManager hvis alle skibe nu er placeret
        if (shipPanel.AllShipsPlaced())
        {
            gameManager.OnAllShipsPlaced();
        }
    }

    // ---------- Hjælpe-metoder ----------

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 wp = sceneCamera.ScreenToWorldPoint(screenPos);
        wp.z = 0f;
        return wp;
    }

    // Konverterer en verdens-position til en gyldig anker-position på brættet,
    // eller null hvis positionen er for langt fra brættet.
    private Position? WorldToBoardAnchor(Vector3 worldPos)
    {
        BoardModel board = playerBoardView.Model;

        // Find nærmeste celle ved at iterere - simpelt og robust
        float bestDistSq = float.MaxValue;
        Position? bestPos = null;

        for (int r = 0; r < board.Rows; r++)
        {
            for (int c = 0; c < board.Cols; c++)
            {
                CellView cv = playerBoardView.GetCellView(new Position(r, c));
                Vector3 cellWorld = cv.transform.position;
                float dx = cellWorld.x - worldPos.x;
                float dy = cellWorld.y - worldPos.y;
                float distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestPos = new Position(r, c);
                }
            }
        }

        // Hvis musen er for langt fra nærmeste celle, returner null
        // (1.5 cellestørrelser er en rimelig tærskel)
        if (bestDistSq > 1.5f * 1.5f) return null;

        return bestPos;
    }

    // Skibet er forankret med top-venstre celle. Beregn hvor skibets center
    // skal være i verden, så det dækker celle-rækkefølgen pænt (inkl. mellemrum).
    private Vector3 ComputeShipWorldPos(CellView anchorCell, ShipModel model)
    {
        Vector3 anchorPos = anchorCell.transform.position;
        float step = playerBoardView.Step;

        if (model.Orientation == ShipOrientation.Horizontal)
        {
            float offsetX = (model.Size - 1) * step * 0.5f;
            return new Vector3(anchorPos.x + offsetX, anchorPos.y, anchorPos.z);
        }
        else
        {
            float offsetY = -(model.Size - 1) * step * 0.5f;
            return new Vector3(anchorPos.x, anchorPos.y + offsetY, anchorPos.z);
        }
    }

    private Ship FindBoardShipFor(ShipModel shipModel)
    {
        // Find det Ship-objekt på brættet der dækker model'ens nuværende positioner
        if (!shipModel.Anchor.HasValue) return null;
        var positions = shipModel.GetCurrentPositions();
        if (positions.Count == 0) return null;

        Cell anchorCell = playerBoardView.Model.GetCell(positions[0]);
        return anchorCell.ShipPart?.Ship;
    }
}
