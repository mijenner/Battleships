using System.Collections;
using UnityEngine;

public enum GameState
{
    Initializing,     // Byg brætter, AI placerer sine skibe
    Placing,          // Spilleren placerer sine skibe
    PlayerTurn,       // Venter på spillerens skud
    OpponentThinking, // Kort forsinkelse før AI skyder
    ResolvingShot,    // Opdater bræt/HUD, tjek om spillet er slut
    GameOver          // Spillet er slut
}

public class GameManager : MonoBehaviour
{
    [Header("Konfiguration")]
    [SerializeField] private GameConfig config;

    [Header("Brætter")]
    [SerializeField] private BoardView playerBoardView;
    [SerializeField] private BoardView opponentBoardView;

    [Header("Placerings-fasen")]
    [SerializeField] private ShipPanel shipPanel;
    [SerializeField] private ShipPlacementController placementController;

    [Header("HUD")]
    [SerializeField] private GameHUD hud;

    [Header("Timing")]
    [SerializeField] private float opponentThinkSeconds = 0.8f;

    private BoardModel playerBoard;
    private BoardModel opponentBoard;
    private IOpponent opponent;

    public GameState State { get; private set; } = GameState.Initializing;
    public BoardModel PlayerBoard => playerBoard;
    public BoardModel OpponentBoard => opponentBoard;

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("GameManager: GameConfig mangler.");
            return;
        }
        if (playerBoardView == null || opponentBoardView == null)
        {
            Debug.LogError("GameManager: BoardView-referencer mangler.");
            return;
        }

        EnterInitializing();
    }

    private void ChangeState(GameState newState)
    {
        State = newState;
        Debug.Log($"State: {newState}");
    }

    private void EnterInitializing()
    {
        ChangeState(GameState.Initializing);

        playerBoard = new BoardModel(config.rows, config.cols);
        playerBoard.AllowTouching = config.allowTouching;
        opponentBoard = new BoardModel(config.rows, config.cols);
        opponentBoard.AllowTouching = config.allowTouching;

        opponent = new RandomOpponent();
        opponent.PlaceShips(opponentBoard, config);

        playerBoardView.Bind(playerBoard);
        opponentBoardView.Bind(opponentBoard);

        EnterPlacing();
    }

    private void EnterPlacing()
    {
        ChangeState(GameState.Placing);

        if (shipPanel != null) shipPanel.Build(config);
        if (placementController != null) placementController.IsActive = true;

        hud?.SetStatusPermanent("Placer dine skibe - tryk R for rotation. Klik 'Klar' når du er færdig.");
    }

    // Kaldes af ShipPlacementController når alle skibe er placeret
    public void OnAllShipsPlaced()
    {
        if (State != GameState.Placing) return;
        hud?.SetStatusPermanent("Alle skibe placeret. Klik 'Klar' for at starte spillet.");
    }

    // Kaldes af "Klar"-knappen
    public void PlayerReady()
    {
        if (State != GameState.Placing) return;
        if (shipPanel != null && !shipPanel.AllShipsPlaced())
        {
            hud?.SetStatus("Du skal placere alle skibe først");
            return;
        }

        if (placementController != null) placementController.IsActive = false;
        if (shipPanel != null) shipPanel.Hide();
        EnterPlayerTurn();
    }

    private void EnterPlayerTurn()
    {
        ChangeState(GameState.PlayerTurn);
        hud?.SetStatus("Din tur - klik på modstanderens bræt");
    }

    private void EnterOpponentThinking()
    {
        ChangeState(GameState.OpponentThinking);
        hud?.SetStatus("Modstanderens tur...");
        StartCoroutine(OpponentThinkingRoutine());
    }

    private IEnumerator OpponentThinkingRoutine()
    {
        yield return new WaitForSeconds(opponentThinkSeconds);

        Position shot = opponent.NextShot();
        ShotResult result = playerBoard.Shoot(shot);
        opponent.ReportResult(shot, result);
        Debug.Log($"Modstander skød på {shot}: {result}");

        EnterResolvingShot(wasPlayerShot: false, result);
    }

    private void EnterResolvingShot(bool wasPlayerShot, ShotResult result)
    {
        ChangeState(GameState.ResolvingShot);

        if (wasPlayerShot) opponentBoardView.Render();
        else playerBoardView.Render();

        hud?.UpdateStats(playerBoard, opponentBoard);

        if (opponentBoard.AllShipsSunk())
        {
            EnterGameOver("Du vandt!");
            return;
        }
        if (playerBoard.AllShipsSunk())
        {
            EnterGameOver("Du tabte!");
            return;
        }

        if (wasPlayerShot) EnterOpponentThinking();
        else EnterPlayerTurn();
    }

    private void EnterGameOver(string message)
    {
        ChangeState(GameState.GameOver);
        hud?.SetStatusPermanent(message);
    }

    public void PlayerShoot(Position pos)
    {
        if (State != GameState.PlayerTurn) return;

        Cell target = opponentBoard.GetCell(pos);
        if (target.State == CellState.Miss || target.State == CellState.Hit || target.State == CellState.Sunk)
        {
            hud?.SetStatus("Der er allerede skudt der");
            return;
        }

        ShotResult result = opponentBoard.Shoot(pos);
        Debug.Log($"Spiller skød på {pos}: {result}");

        EnterResolvingShot(wasPlayerShot: true, result);
    }
}
