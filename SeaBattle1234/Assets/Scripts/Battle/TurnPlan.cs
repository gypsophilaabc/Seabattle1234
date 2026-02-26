using System.Collections.Generic;
using UnityEngine;

public class TurnPlan
{
    public List<Vector2Int> guns = new List<Vector2Int>();

    public struct Torpedo
    {
        public Vector2Int start;
        public Vector2Int dir; // (0,1) (1,0) (-1,0) (0,-1)

        public Torpedo(Vector2Int s, Vector2Int d)
        {
            start = s;
            dir = d;
        }
    }

    public List<Torpedo> torpedoes = new List<Torpedo>();

    public struct Bomb
    {
        public Vector2Int topLeft;

        public Bomb(Vector2Int tl)
        {
            topLeft = tl;
        }
    }

    public List<Bomb> bombs = new List<Bomb>();
}