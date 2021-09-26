using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public struct Point : IPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator float2(Point v)
        {
            return new float2(v.X, v.Y);
        }
        
        public override string ToString() => $"{X},{Y}";
    }

}
