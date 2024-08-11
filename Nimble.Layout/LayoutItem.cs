using System.Diagnostics;

namespace Nimble.Layout
{
	public class LayoutItem
	{
		public const uint ContainFlagsMask = 0x000007;
		public const uint AlignFlagsMask = 0x000018;
		public const uint BehaveFlagsMask = 0x0003E0;
		public const uint ItemFlagsMask = 0x001C00;
		public const uint ItemFlagsFixedMask = 0x001800;

		public uint Flags { get; internal set; }

		public LayoutItem? FirstChild { get; internal set; }
		public LayoutItem? NextSibling { get; internal set; }
		public LayoutMargins Margins { get; set; } = new();
		public LayoutVector Size { get; set; } = new();

		public LayoutRect Rect => m_computedRect;
		private LayoutRect m_computedRect = new();

		public ContainFlags Contain
		{
			get => (ContainFlags)(Flags & ContainFlagsMask);
			set => Flags = (Flags & ~ContainFlagsMask) | ((uint)value & ContainFlagsMask);
		}

		public AlignFlags Align
		{
			get => (AlignFlags)(Flags & AlignFlagsMask);
			set => Flags = (Flags & ~AlignFlagsMask) | ((uint)value & AlignFlagsMask);
		}

		public BehaveFlags Behave
		{
			get => (BehaveFlags)(Flags & BehaveFlagsMask);
			set => Flags = (Flags & ~BehaveFlagsMask) | ((uint)value & BehaveFlagsMask);
		}

		public ItemFlags ItemFlags
		{
			get => (ItemFlags)(Flags & ItemFlagsMask);
			set => Flags = (Flags & ~ItemFlagsMask) | ((uint)value & ItemFlagsMask);
		}

		private ItemFlags ItemFlagsHFixed => (ItemFlags)(Flags & ItemFlagsFixedMask);

		public bool IsInserted
		{
			get => ItemFlags.HasFlag(ItemFlags.Inserted);
			internal set {
				if (value) {
					ItemFlags |= ItemFlags.Inserted;
				} else {
					ItemFlags &= ~ItemFlags.Inserted;
				}
			}
		}

		public bool IsItemBreak
		{
			get => Behave.HasFlag(BehaveFlags.Break);
			set {
				if (value) {
					Behave |= BehaveFlags.Break;
				} else {
					Behave &= ~BehaveFlags.Break;
				}
			}
		}

		public IEnumerable<LayoutItem> Children
		{
			get {
				var child = FirstChild;
				while (child != null) {
					yield return child;
					child = child.NextSibling;
				}
			}
		}

		public IEnumerable<LayoutItem> Siblings
		{
			get {
				var item = this;
				while (item.NextSibling != null) {
					yield return item.NextSibling;
					item = item.NextSibling;
				}
			}
		}

		public LayoutItem? LastChild => FirstChild?.LastSibling;
		public LayoutItem LastSibling => Siblings.LastOrDefault(this);

		/// <summary>
		/// Resets this item so it may be re-used.
		/// </summary>
		/// <param name="withChildren">Whether to recursively reset all children as well.</param>
		public void Reset(bool withChildren = false)
		{
			if (withChildren) {
				foreach (var child in Children) {
					child.Reset();
				}
			}

			Flags = 0;
			FirstChild = null;
			NextSibling = null;
			Margins = new();
			Size = new();
			m_computedRect = new();
		}

		/// <summary>
		/// Inserts an item as a sibling after another item. This allows inserting an item into the middle of an existing list of items within a
		/// parent. It's also more efficient than repeatedly using <seealso cref="AddChild(LayoutItem)"/> in a loop to create a list of items in
		/// a parent, because it does not need to traverse the parent's children each time. So if you're creating a long list of children inside
		/// of a parent, you might prefer to use this after using <seealso cref="AddChild(LayoutItem)"/> to insert the first child.
		/// </summary>
		public void AddSibling(LayoutItem sibling)
		{
			Debug.Assert(sibling != this, "Must not be the same item");
			Debug.Assert(!sibling.IsInserted, "Must not already be inserted");

			sibling.NextSibling = NextSibling;
			sibling.IsInserted = true;

			NextSibling = sibling;
		}

		/// <summary>
		/// Inserts an item into this item, forming a parent - child relationship. An item can contain any number of child items. Items inserted into
		/// a parent are put at the end of the ordering, after any existing siblings.
		/// </summary>
		public void AddChild(LayoutItem child)
		{
			Debug.Assert(child != this, "Must not be the same item");
			Debug.Assert(!child.IsInserted, "Must not already be inserted");

			if (FirstChild == null) {
				// We have no existing children, make inserted item the first child.
				FirstChild = child;
				child.IsInserted = true;
			} else {
				// We have existing items, iterate to find the last child and append the inserted item after it.
				LastChild?.AddSibling(child);
			}
		}

		/// <summary>
		/// Inserts multiple items into this item. Calling this is much faster than repeatedly calling <seealso cref="AddChild(LayoutItem)"/>, as it
		/// will use <seealso cref="AddSibling(LayoutItem)"/> on each child and avoid traversing the children each time.
		/// </summary>
		public void AddChildren(IEnumerable<LayoutItem> children)
		{
			LayoutItem? lastChild = null;
			foreach (var child in children) {
				if (lastChild == null) {
					AddChild(child);
				} else {
					lastChild.AddSibling(child);
				}
				lastChild = child;
			}
		}

		/// <summary>
		/// Like <seealso cref="AddChild(LayoutItem)"/>, but puts the new item as the first child in a parent instead of as the last.
		/// </summary>
		public void PushChild(LayoutItem child)
		{
			Debug.Assert(child != this, "Must not be same item");
			Debug.Assert(!child.IsInserted, "Must not already be inserted");

			var oldChild = FirstChild;
			FirstChild = child;

			child.IsInserted = true;
			child.NextSibling = oldChild;
		}

		/// <summary>
		/// Run layout calculations starting from this item.
		///
		/// Running the layout calculations from a specific item is useful if you want
		/// to iteratively re-run parts of your layout hierarchy, or if you are only
		/// interested in updating certain subsets of it. Be careful when using this --
		/// it's easy to generate bad output if the parent items haven't yet had their
		/// output rectangles calculated, or if they've been invalidated (e.g. due to
		/// re-allocation).
		/// </summary>
		public void Run()
		{
			CalcSize(0);
			Arrange(0);
			CalcSize(1);
			Arrange(1);
		}

		private void CalcSize(int dim)
		{
			foreach (var child in Children) {
				// NOTE: this is recursive and will run out of stack space if items are nested too deeply.
				child.CalcSize(dim);
			}

			// Set the mutable rect output data to the starting input data
			m_computedRect[dim] = Margins[dim];

			// If we have an explicit input size, just set our output size (which other
			// calc_size and arrange procedures will use) to it.
			if (Size[dim] != 0) {
				m_computedRect[2 + dim] = Size[dim];
				return;
			}

			// Calculate our size based on children items. Note that we've already
			// called lay_calc_size on our children at this point.
			float cal_size;
			switch (Contain) {
				case ContainFlags.Column | ContainFlags.Wrap:
					// flex model
					if (dim == 1) { // direction
						cal_size = CalcStackedSize(1);
					} else {
						cal_size = CalcOverlayedSize(0);
					}
					break;

				case ContainFlags.Row | ContainFlags.Wrap:
					// flex model
					if (dim == 0) { // direction
						cal_size = CalcWrappedStackedSize(0);
					} else {
						cal_size = CalcWrappedOverlayedSize(1);
					}
					break;

				case ContainFlags.Column:
				case ContainFlags.Row:
					// flex model
					if ((Flags & 1) == (uint)dim) { // direction
						cal_size = CalcStackedSize(dim);
					} else {
						cal_size = CalcOverlayedSize(dim);
					}
					break;

				default:
					// layout model
					cal_size = CalcOverlayedSize(dim);
					break;
			}

			// Set our output data size. Will be used by parent calc_size procedures,
			// and by arrange procedures.
			m_computedRect[2 + dim] = cal_size;
		}

		private float CalcStackedSize(int dim)
		{
			int wdim = dim + 2;
			float need_size = 0;
			foreach (var child in Children) {
				need_size += child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
			}
			return need_size;
		}

		private float CalcOverlayedSize(int dim)
		{
			int wdim = dim + 2;
			float need_size = 0;
			foreach (var child in Children) {
				// width = start margin + calculated width + end margin
				float child_size = child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
				need_size = Math.Max(need_size, child_size);
			}
			return need_size;
		}

		private float CalcWrappedStackedSize(int dim)
		{
			int wdim = dim + 2;
			float need_size = 0;
			float need_size2 = 0;
			foreach (var child in Children) {
				if (child.Behave.HasFlag(BehaveFlags.Break)) {
					need_size2 = Math.Max(need_size2, need_size);
					need_size = 0;
				}
				need_size += child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
			}
			return Math.Max(need_size2, need_size);
		}

		private float CalcWrappedOverlayedSize(int dim)
		{
			int wdim = dim + 2;
			float need_size = 0;
			float need_size2 = 0;
			foreach (var child in Children) {
				if (child.Behave.HasFlag(BehaveFlags.Break)) {
					need_size2 += need_size;
					need_size = 0;
				}
				float child_size = child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
				need_size = Math.Max(need_size, child_size);
			}
			return need_size2 + need_size;
		}

		private void Arrange(int dim)
		{
			switch (Contain) {
				case ContainFlags.Column | ContainFlags.Wrap:
					if (dim != 0) {
						ArrangeStacked(1, true);
						float offset = ArrangeWrappedOverlaySqueezed(0);
						m_computedRect[2] = offset - m_computedRect[0];
					}
					break;

				case ContainFlags.Row | ContainFlags.Wrap:
					if (dim == 0) {
						ArrangeStacked(0, true);
					} else {
						// discard return value
						ArrangeWrappedOverlaySqueezed(1);
					}
					break;

				case ContainFlags.Column:
				case ContainFlags.Row:
					if ((Flags & 1) == (uint)dim) {
						ArrangeStacked(dim, false);
					} else {
						ArrangeOverlaySqueezedRange(dim, FirstChild, null, m_computedRect[dim], m_computedRect[2 + dim]);
					}
					break;

				default:
					ArrangeOverlay(dim);
					break;
			}

			foreach (var child in Children) {
				// NOTE: this is recursive and will run out of stack space if items are nested too deeply.
				child.Arrange(dim);
			}
		}

		private void ArrangeStacked(int dim, bool wrap)
		{
			int wdim = dim + 2;

			float space = m_computedRect[2 + dim];
			float max_x2 = m_computedRect[dim] + space;

			var startChild = FirstChild;
			while (startChild != null) {
				float used = 0;
				int count = 0; // count of fillers
				int squeezed_count = 0; // count of squeezable elements
				int total = 0;
				bool hardbreak = false;
				// first pass: count items that need to be expanded,
				// and the space that is used
				var child = startChild;
				LayoutItem? endChild = null;
				while (child != null) {
					var flags = (BehaveFlags)((uint)child.Behave >> dim);
					var fflags = (ItemFlags)((uint)child.ItemFlagsHFixed >> dim);
					float extend = used;
					if (flags.HasFlag(BehaveFlags.HFill)) {
						count++;
						extend += child.m_computedRect[dim] + child.Margins[wdim];
					} else {
						if (!fflags.HasFlag(ItemFlags.HFixed)) {
							squeezed_count++;
						}
						extend += child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
					}
					// wrap on end of line or manual flag
					if (wrap && total > 0 && ((extend > space) || child.Behave.HasFlag(BehaveFlags.Break))) {
						endChild = child;
						hardbreak = child.Behave.HasFlag(BehaveFlags.Break);
						// add marker for subsequent queries
						child.Flags |= (uint)BehaveFlags.Break;
						break;
					} else {
						used = extend;
						child = child.NextSibling;
					}
					total++;
				}

				float extra_space = space - used;
				float filler = 0;
				float spacer = 0;
				float extra_margin = 0;
				float eater = 0;

				if (extra_space > 0) {
					if (count > 0) {
						filler = extra_space / count;
					} else if (total > 0) {
						switch (Align) {
							case AlignFlags.AlignJustify:
								// justify when not wrapping or not in last line,
								// or not manually breaking
								if (!wrap || ((endChild != null) && !hardbreak)) {
									spacer = extra_space / (total - 1);
								}
								break;

							case AlignFlags.AlignStart:
								break;

							case AlignFlags.AlignEnd:
								extra_margin = extra_space;
								break;

							default:
								extra_margin = extra_space / 2;
								break;
						}
					}
				} else if (!wrap && (squeezed_count > 0)) {
					// In floating point, it's possible to end up with some small negative
					// value for extra_space, while also have a 0.0 squeezed_count. This
					// would cause divide by zero. Instead, we'll check to see if
					// squeezed_count is > 0. I believe this produces the same results as
					// the original oui int-only code. However, I don't have any tests for
					// it, so I'll leave it if-def'd for now.
					eater = extra_space / squeezed_count;
				}

				// distribute width among items
				float x = m_computedRect[dim];
				float x1;
				// second pass: distribute and rescale
				child = startChild;
				while (child != endChild && child != null) {
					float ix0, ix1;
					var flags = (BehaveFlags)((uint)child.Behave >> dim);
					var fflags = (ItemFlags)((uint)child.ItemFlagsHFixed >> dim);

					x += child.m_computedRect[dim] + extra_margin;
					if (flags.HasFlag(BehaveFlags.HFill)) { // grow
						x1 = x + filler;
					} else if (fflags.HasFlag(ItemFlags.HFixed)) {
						x1 = x + child.m_computedRect[2 + dim];
					} else { // squeeze
						x1 = x + Math.Max(0.0f, child.m_computedRect[2 + dim] + eater);
					}

					ix0 = x;
					if (wrap) {
						ix1 = Math.Min(max_x2 - child.Margins[wdim], x1);
					} else {
						ix1 = x1;
					}
					child.m_computedRect[dim] = ix0; // pos
					child.m_computedRect[dim + 2] = ix1 - ix0; // size
					x = x1 + child.Margins[wdim];
					child = child.NextSibling;
					extra_margin = spacer;
				}

				startChild = endChild;
			}
		}

		private void ArrangeOverlay(int dim)
		{
			int wdim = dim + 2;
			float offset = m_computedRect[dim];
			float space = m_computedRect[2 + dim];

			foreach (var child in Children) {
				var b_flags = (BehaveFlags)((uint)child.Behave >> dim);

				switch (b_flags & BehaveFlags.HFill) {
					case BehaveFlags.HCenter:
						child.m_computedRect[dim] += (space - child.m_computedRect[2 + dim] - child.Margins[wdim]) / 2;
						break;

					case BehaveFlags.Right:
						child.m_computedRect[dim] += space - child.m_computedRect[2 + dim] - child.Margins[dim] - child.Margins[wdim];
						break;

					case BehaveFlags.HFill:
						child.m_computedRect[2 + dim] = Math.Max(0.0f, space - child.m_computedRect[dim] - child.Margins[wdim]);
						break;

					default:
						break;
				}

				child.m_computedRect[dim] += offset;
			}
		}

		private float ArrangeWrappedOverlaySqueezed(int dim)
		{
			int wdim = dim + 2;
			float offset = m_computedRect[dim];
			float need_size = 0;
			var startChild = FirstChild;
			foreach (var child in Children) {
				if (child.Behave.HasFlag(BehaveFlags.Break)) {
					ArrangeOverlaySqueezedRange(dim, startChild, child, offset, need_size);
					offset += need_size;
					startChild = child;
					need_size = 0;
				}
				float child_size = child.m_computedRect[dim] + child.m_computedRect[2 + dim] + child.Margins[wdim];
				need_size = Math.Max(need_size, child_size);
			}
			ArrangeOverlaySqueezedRange(dim, startChild, null, offset, need_size);
			offset += need_size;
			return offset;
		}

		private static void ArrangeOverlaySqueezedRange(int dim, LayoutItem? startItem, LayoutItem? endItem, float offset, float space)
		{
			int wdim = dim + 2;
			var item = startItem;
			while (item != endItem && item != null) {
				var b_flags = (BehaveFlags)((uint)item.Behave >> dim);

				float min_size = Math.Max(0.0f, space - item.m_computedRect[dim] - item.Margins[wdim]);
				switch (b_flags & BehaveFlags.HFill) {
					case BehaveFlags.HCenter:
						item.m_computedRect[2 + dim] = Math.Min(item.m_computedRect[2 + dim], min_size);
						item.m_computedRect[dim] += (space - item.m_computedRect[2 + dim] - item.Margins[wdim]) / 2;
						break;

					case BehaveFlags.Right:
						item.m_computedRect[2 + dim] = Math.Min(item.m_computedRect[2 + dim], min_size);
						item.m_computedRect[dim] = space - item.m_computedRect[2 + dim] - item.Margins[wdim];
						break;

					case BehaveFlags.HFill:
						item.m_computedRect[2 + dim] = min_size;
						break;

					default:
						item.m_computedRect[2 + dim] = Math.Min(item.m_computedRect[2 + dim], min_size);
						break;
				}

				item.m_computedRect[dim] += offset;
				item = item.NextSibling;
			}
		}
	}
}
