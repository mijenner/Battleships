using UnityEngine;

[System.Serializable]
public class ShipConfig
{
    public string shipName;
    public int size;
    public int count = 1;

    [Header("Sprites (valgfrit - falder tilbage til default på ShipView hvis null)")]
    public Sprite frontSprite;
    public Sprite middleSprite;
    public Sprite backSprite;
}
