using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Prefab og layout")]
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private float cellSpacing = 0.05f;

    [Header("Visning")]
    [SerializeField] private bool showShips = true;

    private BoardModel model;
    private CellView[,] cellViews;

    public BoardModel Model => model;
    public bool ShowShips => showShips;
    public float CellSize => cellSize;
    public float CellSpacing => cellSpacing;
    public float Step => cellSize + cellSpacing;

    // Bindes til en BoardModel og bygger grid'et
    public void Bind(BoardModel model)
    {
        this.model = model;

        // Ryd gamle celler, hvis bind kaldes igen (fx ved genstart)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        cellViews = new CellView[model.Rows, model.Cols];

        float step = cellSize + cellSpacing;
        // Offset så brættet er centreret omkring GameObjectets origo
        Vector3 origin = new Vector3(
            -(model.Cols - 1) * step / 2f,
            -(model.Rows - 1) * step / 2f,
            0f);

        for (int r = 0; r < model.Rows; r++)
        {
            for (int c = 0; c < model.Cols; c++)
            {
                Vector3 localPos = origin + new Vector3(c * step, r * step, 0f);
                CellView cv = Instantiate(cellPrefab, transform);
                cv.transform.localPosition = localPos;
                cv.transform.localScale = Vector3.one * cellSize;
                cv.name = $"Cell_{r}_{c}";
                cv.Initialize(this, new Position(r, c));
                cellViews[r, c] = cv;
            }
        }

        Render();
    }

    // Opdaterer alle celler ud fra modelens nuværende tilstand
    public void Render()
    {
        if (model == null) return;

        for (int r = 0; r < model.Rows; r++)
        {
            for (int c = 0; c < model.Cols; c++)
            {
                Cell cell = model.GetCell(new Position(r, c));
                cellViews[r, c].SetState(cell.State, showShips);
            }
        }
    }

    public CellView GetCellView(Position pos)
    {
        return cellViews[pos.Row, pos.Col];
    }
}
