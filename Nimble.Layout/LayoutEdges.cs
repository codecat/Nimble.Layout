namespace Nimble.Layout
{
	public struct LayoutEdges(float left, float top, float right, float bottom)
	{
		public float Left = left;
		public float Top = top;
		public float Right = right;
		public float Bottom = bottom;

		public readonly float Horizontal => Left + Right;
		public readonly float Vertical => Top + Bottom;

		public LayoutEdges() : this(0, 0, 0, 0) { }
		public LayoutEdges(float m) : this(m, m, m, m) { }
		public LayoutEdges(float h, float v) : this(h, v, h, v) { }

		public readonly float GetDimension(int dim)
			=> dim switch {
				0 => Left + Right,
				1 => Top + Bottom,
				_ => throw new IndexOutOfRangeException(),
			};

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

		public static LayoutEdges Parse(string text)
		{
			var parse = text.Split(' ');
			if (parse.Length == 1) {
				return new(float.Parse(parse[0]));
			} else if (parse.Length == 2) {
				return new(float.Parse(parse[0]), float.Parse(parse[1]));
			} else if (parse.Length == 4) {
				return new(float.Parse(parse[0]), float.Parse(parse[1]), float.Parse(parse[2]), float.Parse(parse[3]));
			}
			throw new FormatException("Invalid layout margins format");
		}
	}
}
