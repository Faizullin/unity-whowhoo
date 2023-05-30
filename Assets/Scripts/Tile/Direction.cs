using UnityEngine;

public enum Direction
{
    up,
    right,
    down,
    left,
}

public static class Directions { 

    public static Vector2 GetVector2FromDirection(Direction direction)
    {
        return direction switch
        {
            Direction.up => Vector2.up,
            Direction.right => Vector2.right,
            Direction.down => Vector2.down,
            Direction.left => Vector2.left,
            _ => Vector2.up,
        };
    }
    public static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.right => Direction.left,
            Direction.down => Direction.up,
            Direction.left => Direction.right,
            Direction.up => Direction.down,
            _ => Direction.up,
        };
    }
}