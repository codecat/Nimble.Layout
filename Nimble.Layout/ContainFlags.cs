namespace Nimble.Layout
{
	[Flags]
	public enum ContainFlags : uint // aka: box flags
	{
		// flex-direction (bit 0+1)

		// left to right
		Row = 0x002,
		// top to bottom
		Column = 0x003,

		// model (bit 1)

		// free layout
		Layout = 0x000,
		// flex model
		Flex = 0x002,

		// flex-wrap (bit 2)

		// single-line
		NoWrap = 0x000,
		// multi-line, wrap left to right
		Wrap = 0x004,


		// align-items
		// can be implemented by putting a flex container in a layout container,
		// then using LAY_TOP, LAY_BOTTOM, LAY_VFILL, LAY_VCENTER, etc.
		// FILL is equivalent to stretch/grow

		// align-content (start, end, center, stretch)
		// can be implemented by putting a flex container in a layout container,
		// then using LAY_TOP, LAY_BOTTOM, LAY_VFILL, LAY_VCENTER, etc.
		// FILL is equivalent to stretch; space-between is not supported.
	}
}
