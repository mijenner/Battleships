using System.Collections.Generic;
using UnityEngine;

public class RandomOpponent : IOpponent
{
    private readonly List<Position> remainingShots = new List<Position>();
    private readonly System.Random rng = new System.Random();

    public void PlaceShips(BoardModel ownBoard, GameConfig config)
    {
        // Byg listen over mulige skud (alle felter på modstanderens bræt)
        remainingShots.Clear();
        for (int r = 0; r < config.rows; r++)
            for (int c = 0; c < config.cols; c++)
                remainingShots.Add(new Position(r, c));

        // Placer hvert skib tilfældigt
        foreach (var shipConfig in config.ships)
        {
            for (int i = 0; i < shipConfig.count; i++)
            {
                PlaceOneShipRandomly(ownBoard, shipConfig);
            }
        }
    }

    private void PlaceOneShipRandomly(BoardModel board, ShipConfig shipConfig)
    {
        const int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool horizontal = rng.Next(2) == 0;
            int row = rng.Next(board.Rows);
            int col = rng.Next(board.Cols);

            List<Position> positions = new List<Position>();
            for (int i = 0; i < shipConfig.size; i++)
            {
                int r = horizontal ? row : row + i;
                int c = horizontal ? col + i : col;
                positions.Add(new Position(r, c));
            }

            Ship ship = new Ship(shipConfig.shipName, shipConfig.size);
            if (board.PlaceShip(ship, positions))
                return;
        }

        Debug.LogWarning($"Kunne ikke placere skib {shipConfig.shipName} efter {maxAttempts} forsøg");
    }

    public Position NextShot()
    {
        int index = rng.Next(remainingShots.Count);
        Position shot = remainingShots[index];
        remainingShots.RemoveAt(index);
        return shot;
    }

    public void ReportResult(Position shot, ShotResult result)
    {
        // RandomOpponent bruger ikke resultatet - den skyder bare tilfældigt
        // En smartere opponent (HunterOpponent) ville bruge info til at følge op på hits
    }
}
