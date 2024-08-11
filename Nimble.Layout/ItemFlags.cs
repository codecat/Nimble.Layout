namespace Nimble.Layout
{
	[Flags]
	public enum ItemFlags : uint
	{
		// item has been inserted
		Inserted = 0x400,

		// horizontal size has been explicitly set
		HFixed = 0x800,
		// vertical size has been explicitly set
		VFixed = 0x1000,
	}
}
