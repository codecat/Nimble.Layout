namespace Nimble.Layout
{
	public struct LayoutRect(float x, float y, float width, float height)
	{
		public float X { get; set; } = x;
		public float Y { get; set; } = y;
		public float Width { get; set; } = width;
		public float Height { get; set; } = height;

		public LayoutRect() : this(0, 0, 0, 0) { }

		public float this[int index]
		{
			readonly get => index switch {
				0 => X,
				1 => Y,
				2 => Width,
				3 => Height,
				_ => throw new IndexOutOfRangeException(),
			};
			set {
				switch (index) {
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Width = value; break;
					case 3: Height = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public override readonly string ToString() => $"<{X}, {Y}> <{Width}x{Height}>";

		public readonly bool Equals(LayoutRect other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
		public override readonly bool Equals(object? obj) => obj is LayoutRect other && Equals(other);
		public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

		public static bool operator ==(LayoutRect left, LayoutRect right) => left.Equals(right);
		public static bool operator !=(LayoutRect left, LayoutRect right) => !(left == right);
	}
}
