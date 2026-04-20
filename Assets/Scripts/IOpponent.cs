using System.Collections.Generic;

public interface IOpponent
{
    // Kaldes i starten af spillet - modstanderen placerer sine skibe på eget bræt
    void PlaceShips(BoardModel ownBoard, GameConfig config);

    // Returnerer modstanderens næste skud (en position på spillerens bræt)
    Position NextShot();

    // Kaldes af GameManager efter hvert skud, så modstanderen kan lære af resultatet
    void ReportResult(Position shot, ShotResult result);
}
