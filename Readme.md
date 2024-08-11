# Nimble.Layout
Layout system for .Net based on [`layout.h`](https://github.com/randrew/layout).

## Example
```cs
var root = new LayoutItem {
	Size = new(1280, 720),
	Contain = ContainFlags.Row,
};

var masterList = new LayoutItem {
	Size = new(400, 0),
	Behave = BehaveFlags.VFill,
	Contain = ContainFlags.Column,
};
root.AddChild(masterList);

var contentView = new LayoutItem {
	Behave = BehaveFlags.HFill | BehaveFlags.VFill,
};
root.AddChild(contentView);

root.Run();

MyUiLibrary.DrawBoxXYWH(
	masterList.Rect.X, masterList.Rect.Y,
	masterList.Rect.Width, masterList.Rect.Height
);
```

## Notes
This is a direct port of `layout.h` without too many modifications besides making it more OOP-friendly and some common C# things.

Tests from `layout.h` were ported to C#, and they all pass, so I'm rather confident in the reliability of this library.

Performance is still mostly untested, although there is a `Nimble.Layout.Benchmark` project that should match the one in `layout.h`. Initial testing shows that this library is about 4 times slower than its C counterpart. I have not yet narrowed down the exact cause.

Most of the documentation is copied straight from the C code. This will be fixed eventually.
