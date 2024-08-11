namespace Nimble.Layout
{
	[Flags]
	public enum BehaveFlags : uint // aka: layout flags
	{
		// attachments (bit 5-8)
		// fully valid when parent uses LAY_LAYOUT model
		// partially valid when in LAY_FLEX model

		// anchor to left item or left side of parent
		Left = 0x020,
		// anchor to top item or top side of parent
		Top = 0x040,
		// anchor to right item or right side of parent
		Right = 0x080,
		// anchor to bottom item or bottom side of parent
		Bottom = 0x100,
		// anchor to both left and right item or parent borders
		HFill = 0x0a0,
		// anchor to both top and bottom item or parent borders
		VFill = 0x140,
		// center horizontally, with left margin as offset
		HCenter = 0x000,
		// center vertically, with top margin as offset
		VCenter = 0x000,
		// center in both directions, with left/top margin as offset
		Center = 0x000,
		// anchor to all four directions
		Fill = 0x1e0,

		// When in a wrapping container, put this element on a new line. Wrapping
		// layout code auto-inserts LAY_BREAK flags as needed. See GitHub issues for
		// TODO related to this.
		//
		// Drawing routines can read this via item pointers as needed after
		// performing layout calculations.
		Break = 0x200,
	}
}
