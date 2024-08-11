namespace Nimble.Layout
{
	[Flags]
	public enum AlignFlags : uint // aka: box flags
	{
		// justify-content (start, end, center, space-between)
		// at start of row/column
		AlignStart = 0x008,
		// at center of row/column
		AlignMiddle = 0x000,
		// at end of row/column
		AlignEnd = 0x010,
		// insert spacing to stretch across whole row/column
		AlignJustify = 0x018,
	}
}
