namespace Nimble.Layout
{
	[Flags]
	public enum AlignFlags : uint // aka: box flags
	{
		// justify-content (start, end, center, space-between)
		// at start of row/column
		Start = 0x008,
		// at center of row/column
		Middle = 0x000,
		// at end of row/column
		End = 0x010,
		// insert spacing to stretch across whole row/column
		Justify = 0x018,
	}
}
