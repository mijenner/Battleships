using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BoardModelTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== BoardModel test start ===");

        // 1. Lav et 10x10 bræt
        BoardModel board = new BoardModel(10, 10);
        Debug.Log($"Oprettet bræt: {board.Rows}x{board.Cols}");

        // 2. Placer et skib horisontalt på række 2, kolonne 3-6
        Ship destroyer = new Ship("Destroyer", 4);
        List<Position> positions = new List<Position>
        {
            new Position(2, 3),
            new Position(2, 4),
            new Position(2, 5),
            new Position(2, 6),
        };
        bool placed = board.PlaceShip(destroyer, positions);
        Debug.Log($"Placerede Destroyer: {placed}");

        // 3. Forsøg at placere et skib der overlapper (skal fejle)
        Ship carrier = new Ship("Carrier", 3);
        List<Position> overlapping = new List<Position>
        {
            new Position(2, 5),
            new Position(2, 6),
            new Position(2, 7),
        };
        bool placedOverlap = board.PlaceShip(carrier, overlapping);
        Debug.Log($"Forsøgte at overlappe (skal være False): {placedOverlap}");

        // 4. Print brættet
        Debug.Log("Bræt med skib placeret:\n" + RenderBoard(board, showShips: true));

        // 5. Skyd - et miss og flere hits
        Debug.Log($"Skud på (0,0): {board.Shoot(new Position(0, 0))}");
        Debug.Log($"Skud på (2,3): {board.Shoot(new Position(2, 3))}");
        Debug.Log($"Skud på (2,4): {board.Shoot(new Position(2, 4))}");
        Debug.Log($"Skud på (2,5): {board.Shoot(new Position(2, 5))}");
        Debug.Log($"Skud på (2,6): {board.Shoot(new Position(2, 6))}");

        Debug.Log($"Alle skibe sunket? {board.AllShipsSunk()}");
        Debug.Log("Bræt efter skud:\n" + RenderBoard(board, showShips: true));

        Debug.Log("=== BoardModel test slut ===");
    }

    // Hjælpemetode: tegner brættet som tekst
    private string RenderBoard(BoardModel board, bool showShips)
    {
        StringBuilder sb = new StringBuilder();

        // Kolonnenumre
        sb.Append("   ");
        for (int c = 0; c < board.Cols; c++) sb.Append(c + " ");
        sb.AppendLine();

        for (int r = 0; r < board.Rows; r++)
        {
            sb.Append(r.ToString().PadLeft(2) + " ");
            for (int c = 0; c < board.Cols; c++)
            {
                Cell cell = board.GetCell(new Position(r, c));
                sb.Append(CellToChar(cell, showShips) + " ");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private char CellToChar(Cell cell, bool showShips)
    {
        switch (cell.State)
        {
            case CellState.Empty: return '.';
            case CellState.Ship: return showShips ? 'S' : '.';
            case CellState.Miss: return 'o';
            case CellState.Hit: return 'X';
            case CellState.Sunk: return '#';
            default: return '?';
        }
    }
}