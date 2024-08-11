namespace Nimble.Layout
{
	public struct LayoutMargins(float left, float top, float right, float bottom)
	{
		public float Left { get; set; } = left;
		public float Top { get; set; } = top;
		public float Right { get; set; } = right;
		public float Bottom { get; set; } = bottom;

		public LayoutMargins() : this(0, 0, 0, 0) { }
		public LayoutMargins(float m) : this(m, m, m, m) { }
		public LayoutMargins(float h, float v) : this(h, v, h, v) { }

		public float this[int index]
		{
			get => index switch {
				0 => Left,
				1 => Top,
				2 => Right,
				3 => Bottom,
				_ => throw new IndexOutOfRangeException(),
			};
			set {
				switch (index) {
					case 0: Left = value; break;
					case 1: Top = value; break;
					case 2: Right = value; break;
					case 3: Bottom = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public override readonly string ToString() => $"<l:{Left}, t:{Top}, r:{Right}, b:{Bottom}>";
	}
}
