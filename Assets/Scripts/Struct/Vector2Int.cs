

/*
 * Vector2와 유사하지만, 각 요소가 float이 아닌 int값을 가지는 구조체.
 * 여기서는 상하좌우(혹은 대각선)의 방향을 나타내기 위해 사용된다.
 */
public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int a, int b)
    {
        x = a;
        y = b;
    }
    
    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }
    
    public static bool operator ==(Vector2Int a, Vector2Int b)
    {
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(Vector2Int a, Vector2Int b)
    {
        return a.x != b.x || a.y != b.y;
    }
}