namespace Nimble.Layout
{
	public struct LayoutVector(float x, float y)
	{
		public float X { get; set; } = x;
		public float Y { get; set; } = y;

		public LayoutVector() : this(0, 0) { }

		public float this[int index]
		{
			readonly get => index switch {
				0 => X,
				1 => Y,
				_ => throw new IndexOutOfRangeException(),
			};
			set {
				switch (index) {
					case 0: X = value; break;
					case 1: Y = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public override readonly string ToString() => $"<{X}, {Y}>";

		public readonly bool Equals(LayoutVector other) => X == other.X && Y == other.Y;
		public override readonly bool Equals(object? obj) => obj is LayoutVector other && Equals(other);
		public override readonly int GetHashCode() => HashCode.Combine(X, Y);

		public static bool operator ==(LayoutVector left, LayoutVector right) => left.Equals(right);
		public static bool operator !=(LayoutVector left, LayoutVector right) => !(left == right);
	}
}
