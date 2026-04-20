using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoardView opponentBoardView;
    [SerializeField] private Camera sceneCamera;

    void Start()
    {
        if (sceneCamera == null) sceneCamera = Camera.main;

        if (gameManager == null)
            Debug.LogError("InputController: GameManager-reference mangler.");
        if (opponentBoardView == null)
            Debug.LogError("InputController: OpponentBoardView-reference mangler.");
        if (sceneCamera == null)
            Debug.LogError("InputController: Intet kamera fundet.");
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // Skyd kun når det er spillerens tur
        if (gameManager.State != GameState.PlayerTurn) return;

        // Konverter musens skærmposition til en verdens-position
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = sceneCamera.ScreenToWorldPoint(screenPos);

        // Raycast i 2D fra det punkt
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return;

        // Blev der klikket på en CellView?
        CellView cellView = hit.collider.GetComponent<CellView>();
        if (cellView == null) return;

        // Kun skud på modstanderens bræt
        if (cellView.OwnerBoard != opponentBoardView) return;

        gameManager.PlayerShoot(cellView.Position);
    }
}
