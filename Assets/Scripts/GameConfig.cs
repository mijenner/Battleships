using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Battleship/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int rows = 10;
    public int cols = 10;

    [Tooltip("Hvis false, må skibe ikke ligge op ad hinanden (heller ikke diagonalt)")]
    public bool allowTouching = false;

    public ShipConfig[] ships;
}
