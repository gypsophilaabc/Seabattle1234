using System.Collections.Generic;
using UnityEngine;

public class ShipInstance
{
    public int typeId;
    public List<Vector2Int> cells = new List<Vector2Int>();
    public bool sunk = false;
}