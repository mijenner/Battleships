using System.Collections.Generic;
using UnityEngine;

// Opstiller skibe vandret ved siden af hinanden under brættet.
// Skibene lever i samme verden som brættet og bruger samme cellestørrelse + mellemrum.
public class ShipPanel : MonoBehaviour
{
    [Header("Prefab og reference til bræt")]
    [SerializeField] private ShipView shipPrefab;
    [SerializeField] private BoardView referenceBoardView; // henter cellSize og step herfra

    [Header("Layout")]
    [SerializeField] private float horizontalSpacing = 0.5f;

    private readonly List<ShipView> shipViews = new List<ShipView>();
    private readonly List<Vector3> homePositions = new List<Vector3>();

    public IReadOnlyList<ShipView> ShipViews => shipViews;

    public void Build(GameConfig config)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        shipViews.Clear();
        homePositions.Clear();

        float cellSize = referenceBoardView != null ? referenceBoardView.CellSize : 1f;
        float step = referenceBoardView != null ? referenceBoardView.Step : 1f;

        // Første skib starter med venstre kant ved xCursor = 0 (panelets origo).
        // Skibets længde = (Size-1) * step + cellSize, center = xCursor + length/2.
        float xCursor = 0f;

        foreach (var shipConfig in config.ships)
        {
            for (int i = 0; i < shipConfig.count; i++)
            {
                ShipModel model = new ShipModel(shipConfig.shipName, shipConfig.size);

                ShipView view = Instantiate(shipPrefab, transform);
                view.Initialize(model, cellSize, step);

                float length = (shipConfig.size - 1) * step + cellSize;
                float shipCenterX = xCursor + length * 0.5f;
                Vector3 home = new Vector3(shipCenterX, 0f, 0f);
                view.transform.localPosition = home;
                view.name = $"Ship_{shipConfig.shipName}_{i}";

                shipViews.Add(view);
                homePositions.Add(home);

                xCursor += length + horizontalSpacing;
            }
        }
    }

    public void ReturnShip(ShipView view)
    {
        int index = shipViews.IndexOf(view);
        if (index < 0) return;

        if (view.Model.Orientation == ShipOrientation.Vertical)
        {
            view.Model.Rotate();
            view.UpdateShape();
        }

        view.Model.ClearAnchor();
        view.transform.localPosition = homePositions[index];
        view.SetVisualState(ShipVisualState.Normal);
    }

    public bool AllShipsPlaced()
    {
        foreach (var view in shipViews)
        {
            if (!view.Model.IsPlaced) return false;
        }
        return true;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
