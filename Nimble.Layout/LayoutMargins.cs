namespace Nimble.Layout
{
	public struct LayoutMargins(float left, float top, float right, float bottom)
	{
		public float Left = left;
		public float Top = top;
		public float Right = right;
		public float Bottom = bottom;

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
