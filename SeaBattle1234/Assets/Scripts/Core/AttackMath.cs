using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class AttackMath
{
    /// <summary>
    /// 获取直线路径上的所有格子（包括起点）
    /// </summary>
    /// <param name="anchor">起点</param>
    /// <param name="dir">方向</param>
    /// <param name="length">长度</param>
    /// <returns>路径上的格子坐标列表</returns>

    public static List<Vector2Int> GetLine(Vector2Int anchor, Dir4 dir, int length)
    {
        return GetLine(anchor, ToDirection(dir), length);
    }

    public static List<Vector2Int> GetLineInside(Vector2Int anchor, Dir4 dir, int length, int boardRows, int boardCols)
    {
        return GetLineInside(anchor, ToDirection(dir), length, boardRows, boardCols);
    }

    private static Direction ToDirection(Dir4 dir)
    {
        return dir switch
        {
            Dir4.Up => Direction.Up,
            Dir4.Down => Direction.Down,
            Dir4.Left => Direction.Left,
            Dir4.Right => Direction.Right,
            _ => Direction.Up
        };
    }//桥接重载，这样实现了和dir4的接入
    public static List<Vector2Int> GetLine(Vector2Int anchor, Direction dir, int length)
    {
        var result = new List<Vector2Int>();
        var current = anchor;

        for (int i = 0; i < length; i++)
        {
            result.Add(current);

            switch (dir)
            {
                case Direction.Up: current = new Vector2Int(current.x - 1, current.y); break;
                case Direction.Down: current = new Vector2Int(current.x + 1, current.y); break;
                case Direction.Left: current = new Vector2Int(current.x, current.y - 1); break;
                case Direction.Right: current = new Vector2Int(current.x, current.y + 1); break;
            }
        }

        return result;
    }

    /// <summary>
    /// 获取直线路径上的所有格子，并过滤掉超出边界的格子
    /// </summary>
    /// <param name="anchor">起点</param>
    /// <param name="dir">方向</param>
    /// <param name="length">长度</param>
    /// <param name="boardRows">棋盘行数</param>
    /// <param name="boardCols">棋盘列数</param>
    /// <returns>在棋盘内的路径格子列表</returns>
    public static List<Vector2Int> GetLineInside(Vector2Int anchor, Direction dir, int length, int boardRows, int boardCols)
    {
        var result = new List<Vector2Int>();
        var current = anchor;

        for (int i = 0; i < length; i++)
        {
            if (IsInside(current, boardRows, boardCols))
                result.Add(current);

            switch (dir)
            {
                case Direction.Up: current = new Vector2Int(current.x - 1, current.y); break;
                case Direction.Down: current = new Vector2Int(current.x + 1, current.y); break;
                case Direction.Left: current = new Vector2Int(current.x, current.y - 1); break;
                case Direction.Right: current = new Vector2Int(current.x, current.y + 1); break;
            }
        }

        return result;
    }

    /// <summary>
    /// 获取矩形区域内的所有格子（左上角为锚点）
    /// </summary>
    /// <param name="topLeft">左上角坐标</param>
    /// <param name="height">高度（行数）</param>
    /// <param name="width">宽度（列数）</param>
    /// <returns>矩形区域内的所有格子坐标</returns>
    public static List<Vector2Int> GetRect(Vector2Int topLeft, int height, int width)
    {
        var result = new List<Vector2Int>(height * width);

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                result.Add(new Vector2Int(topLeft.x + r, topLeft.y + c));
            }
        }

        return result;
    }

    /// <summary>
    /// 获取矩形区域内的所有格子，并过滤掉超出边界的格子
    /// </summary>
    /// <param name="topLeft">左上角坐标</param>
    /// <param name="height">高度（行数）</param>
    /// <param name="width">宽度（列数）</param>
    /// <param name="boardRows">棋盘行数</param>
    /// <param name="boardCols">棋盘列数</param>
    /// <returns>在棋盘内的矩形格子列表</returns>
    public static List<Vector2Int> GetRectInside(Vector2Int topLeft, int height, int width, int boardRows, int boardCols)
    {
        var result = new List<Vector2Int>();

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                var pos = new Vector2Int(topLeft.x + r, topLeft.y + c);
                if (IsInside(pos, boardRows, boardCols))
                    result.Add(pos);
            }
        }

        return result;
    }

    /// <summary>
    /// 获取十字形区域（用于可能的特殊武器）
    /// </summary>
    /// <param name="center">中心点</param>
    /// <param name="radius">半径</param>
    /// <returns>十字形区域的所有格子</returns>
    public static List<Vector2Int> GetCross(Vector2Int center, int radius)
    {
        var result = new List<Vector2Int>();

        // 添加中心点
        result.Add(center);

        for (int i = 1; i <= radius; i++)
        {
            result.Add(new Vector2Int(center.x - i, center.y)); // 上
            result.Add(new Vector2Int(center.x + i, center.y)); // 下
            result.Add(new Vector2Int(center.x, center.y - i)); // 左
            result.Add(new Vector2Int(center.x, center.y + i)); // 右
        }

        return result;
    }

    /// <summary>
    /// 获取菱形区域（曼哈顿距离范围）
    /// </summary>
    /// <param name="center">中心点</param>
    /// <param name="range">曼哈顿距离范围</param>
    /// <returns>菱形区域内的所有格子</returns>
    public static List<Vector2Int> GetDiamond(Vector2Int center, int range)
    {
        var result = new List<Vector2Int>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= range)
                {
                    result.Add(new Vector2Int(center.x + dx, center.y + dy));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取环形区域（指定内外半径的圆环）
    /// </summary>
    /// <param name="center">中心点</param>
    /// <param name="minRadius">最小半径</param>
    /// <param name="maxRadius">最大半径</param>
    /// <returns>环形区域内的所有格子</returns>
    public static List<Vector2Int> GetRing(Vector2Int center, int minRadius, int maxRadius)
    {
        var result = new List<Vector2Int>();

        for (int dx = -maxRadius; dx <= maxRadius; dx++)
        {
            for (int dy = -maxRadius; dy <= maxRadius; dy++)
            {
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance >= minRadius && distance <= maxRadius + 0.5f)
                {
                    result.Add(new Vector2Int(center.x + dx, center.y + dy));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 检查坐标是否在棋盘内
    /// </summary>
    public static bool IsInside(Vector2Int pos, int boardRows, int boardCols)
    {
        return pos.x >= 0 && pos.x < boardRows && pos.y >= 0 && pos.y < boardCols;
    }

    /// <summary>
    /// 检查矩形区域是否完全在棋盘内
    /// </summary>
    public static bool IsRectInside(Vector2Int topLeft, int height, int width, int boardRows, int boardCols)
    {
        return IsInside(topLeft, boardRows, boardCols) &&
               IsInside(new Vector2Int(topLeft.x + height - 1, topLeft.y + width - 1), boardRows, boardCols);
    }

    /// <summary>
    /// 获取两个点之间的直线路径（布雷森汉姆直线算法）
    /// </summary>
    public static List<Vector2Int> GetLineBetween(Vector2Int from, Vector2Int to)
    {
        var result = new List<Vector2Int>();

        int x = from.x;
        int y = from.y;
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            result.Add(new Vector2Int(x, y));

            if (x == to.x && y == to.y)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return result;
    }

    /// <summary>
    /// 获取所有相邻的格子（四方向）
    /// </summary>
    public static List<Vector2Int> GetAdjacent4(Vector2Int pos)
    {
        return new List<Vector2Int>
            {
                new Vector2Int(pos.x - 1, pos.y),
                new Vector2Int(pos.x + 1, pos.y),
                new Vector2Int(pos.x, pos.y - 1),
                new Vector2Int(pos.x, pos.y + 1)
            };
    }

    /// <summary>
    /// 获取所有相邻的格子（八方向）
    /// </summary>
    public static List<Vector2Int> GetAdjacent8(Vector2Int pos)
    {
        var result = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                result.Add(new Vector2Int(pos.x + dx, pos.y + dy));
            }
        }

        return result;
    }

    /// <summary>
    /// 方向转向量
    /// </summary>
    public static Vector2Int DirectionToVector(Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Vector2Int(-1, 0),
            Direction.Down => new Vector2Int(1, 0),
            Direction.Left => new Vector2Int(0, -1),
            Direction.Right => new Vector2Int(0, 1),
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// 向量转方向
    /// </summary>
    public static Direction? VectorToDirection(Vector2Int vec)
    {
        if (vec == new Vector2Int(-1, 0)) return Direction.Up;
        if (vec == new Vector2Int(1, 0)) return Direction.Down;
        if (vec == new Vector2Int(0, -1)) return Direction.Left;
        if (vec == new Vector2Int(0, 1)) return Direction.Right;
        return null;
    }
}