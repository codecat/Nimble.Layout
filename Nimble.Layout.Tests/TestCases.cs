namespace Nimble.Layout.Tests
{
	[TestClass]
	public class TestCases
	{
		[TestMethod]
		public void SimpleFill()
		{
			var child = new LayoutItem {
				Behave = BehaveFlags.Fill,
			};

			var root = new LayoutItem {
				Size = new(30, 40),
			};

			root.AddChild(child);
			root.Run();

			Assert.AreEqual(new(0, 0, 30, 40), root.Rect);
			Assert.AreEqual(new(0, 0, 30, 40), child.Rect);
			Assert.AreEqual(new(30, 40), root.Size);
		}

		[TestMethod]
		public void MultipleUninserted()
		{
			var root = new LayoutItem {
				Size = new(155, 177),
			};
			var child1 = new LayoutItem();
			var child2 = new LayoutItem {
				Size = new(1, 1),
			};

			root.Run();

			Assert.AreEqual(new(0, 0, 155, 177), root.Rect);
			Assert.AreEqual(new(), child1.Rect); // Should be 0, because it is not inserted into root
			Assert.AreEqual(new(), child2.Rect); // Should be 0, because it is not inserted into root, so Rect is not calculated
		}

		[TestMethod]
		public void ColumnEvenFill()
		{
			var root = new LayoutItem {
				Size = new(50, 60),
				Contain = ContainFlags.Column,
			};

			var childA = new LayoutItem { Behave = BehaveFlags.Fill };
			var childB = new LayoutItem { Behave = BehaveFlags.Fill };
			var childC = new LayoutItem { Behave = BehaveFlags.Fill };

			root.AddChildren([childA, childB, childC]);
			root.Run();

			Assert.AreEqual(new(0, 0, 50, 60), root.Rect);
			Assert.AreEqual(new(0, 0, 50, 20), childA.Rect);
			Assert.AreEqual(new(0, 20, 50, 20), childB.Rect);
			Assert.AreEqual(new(0, 40, 50, 20), childC.Rect);
		}

		[TestMethod]
		public void RowEvenFill()
		{
			var root = new LayoutItem {
				Size = new(90, 3),
				Contain = ContainFlags.Row,
			};

			var childA = new LayoutItem {
				Size = new(0, 1),
				Behave = BehaveFlags.HFill | BehaveFlags.Top,
			};
			var childB = new LayoutItem {
				Size = new(0, 1),
				Behave = BehaveFlags.HFill | BehaveFlags.VCenter,
			};
			var childC = new LayoutItem {
				Size = new(0, 1),
				Behave = BehaveFlags.HFill | BehaveFlags.Bottom,
			};

			root.AddChildren([childA, childB, childC]);
			root.Run();

			Assert.AreEqual(new(0, 0, 90, 3), root.Rect);
			Assert.AreEqual(new(0, 0, 30, 1), childA.Rect);
			Assert.AreEqual(new(30, 1, 30, 1), childB.Rect);
			Assert.AreEqual(new(60, 2, 30, 1), childC.Rect);
		}

		[TestMethod]
		public void FixedAndFill()
		{
			var root = new LayoutItem {
				Size = new(50, 60),
				Contain = ContainFlags.Column,
			};

			var fixedA = new LayoutItem {
				Size = new(50, 15),
			};
			var fixedB = new LayoutItem {
				Size = new(50, 15),
			};
			var filler = new LayoutItem {
				Behave = BehaveFlags.Fill,
			};

			root.AddChildren([fixedA, filler, fixedB]);
			root.Run();

			Assert.AreEqual(new(0, 0, 50, 60), root.Rect);
			Assert.AreEqual(new(0, 0, 50, 15), fixedA.Rect);
			Assert.AreEqual(new(0, 15, 50, 30), filler.Rect);
			Assert.AreEqual(new(0, 45, 50, 15), fixedB.Rect);
		}

		[TestMethod]
		public void SimpleMargins1()
		{
			var root = new LayoutItem {
				Size = new(100, 90),
				Contain = ContainFlags.Column,
			};

			var childA = new LayoutItem {
				Size = new(0, 30 - (5 + 10)),
				Margins = new(3, 5, 7, 10),
				Behave = BehaveFlags.HFill,
			};
			var childB = new LayoutItem {
				Behave = BehaveFlags.Fill,
			};
			var childC = new LayoutItem {
				Size = new(0, 30),
				Behave = BehaveFlags.HFill,
			};

			root.AddChildren([childA, childB, childC]);
			root.Run();

			Assert.AreEqual(3, childA.Margins.Left);
			Assert.AreEqual(5, childA.Margins.Top);
			Assert.AreEqual(7, childA.Margins.Right);
			Assert.AreEqual(10, childA.Margins.Bottom);

			Assert.AreEqual(new(3, 5, 90, 5 + 10), childA.Rect);
			Assert.AreEqual(new(0, 30, 100, 30), childB.Rect);
			Assert.AreEqual(new(0, 60, 100, 30), childC.Rect);
		}

		[TestMethod]
		public void NestedBoxes1()
		{
			const int numRows = 5;
			// One of the rows is "fake" and will have 0 units tall height
			const int numRowsWithHeight = numRows - 1;

			var root = new LayoutItem {
				Size = new(70, numRowsWithHeight * 10 + 2 * 10),
			};

			var mainChild = new LayoutItem {
				Margins = new(10),
				Contain = ContainFlags.Column,
				Behave = BehaveFlags.Fill,
			};
			root.AddChild(mainChild);

			var rows = new LayoutItem[numRows];

			// Auto-filling columns-in-row, each one should end up being 10 units wide
			rows[0] = new LayoutItem {
				Contain = ContainFlags.Row,
				Behave = BehaveFlags.Fill,
			};
			var cols1 = new LayoutItem[5];
			for (int i = 0; i < cols1.Length; i++) {
				cols1[i] = new LayoutItem {
					Behave = BehaveFlags.Fill,
				};
			}
			rows[0].AddChildren(cols1);

			rows[1] = new LayoutItem {
				Contain = ContainFlags.Row,
				Behave = BehaveFlags.VFill,
			};
			var cols2 = new LayoutItem[5];
			for (int i = 0; i < cols2.Length; i++) {
				// Fixed-size horizontally, fill vertically
				cols2[i] = new LayoutItem {
					Size = new(10, 0),
					Behave = BehaveFlags.VFill,
				};
			}
			rows[1].AddChildren(cols2);

			// These columns have an inner item which sizes them
			rows[2] = new LayoutItem {
				Contain = ContainFlags.Row,
			};
			var cols3 = new LayoutItem[2];
			for (int i = 0; i < cols3.Length; i++) {
				var col = new LayoutItem {
					Behave = BehaveFlags.Bottom,
				};
				var innerSize = new LayoutItem {
					Size = new(25, 10 * i),
				};
				col.AddChild(innerSize);
				cols3[i] = col;
			}
			rows[2].AddChildren(cols3);

			// Row 4 should end up being 0 units tall after layout
			rows[3] = new LayoutItem {
				Contain = ContainFlags.Row,
				Behave = BehaveFlags.HFill,
			};
			var cols4 = new LayoutItem[99];
			for (int i = 0; i < cols4.Length; i++) {
				cols4[i] = new LayoutItem();
			}
			rows[3].AddChildren(cols4);

			// row 5 should be 10 pixels tall after layout, and each of its columns should be 1 pixel wide
			rows[4] = new LayoutItem {
				Contain = ContainFlags.Row,
				Behave = BehaveFlags.Fill,
			};
			var cols5 = new LayoutItem[50];
			for (int i = 0; i < cols5.Length; i++) {
				cols5[i] = new LayoutItem {
					Behave = BehaveFlags.Fill,
				};
			}
			rows[4].AddChildren(cols5);

			mainChild.AddChildren(rows);

			// Repeat the run and tests multiple times to make sure we get the expected
			// results each time. The original version of oui would overwrite its input
			// state (intentionally) with the output state, so the context's input data
			// (margins, size) had to be "rebuilt" by the client code by doing a reset
			// and then filling it back up for each run. 'lay' does not have that
			// restriction.
			//
			// This is one of the more complex tests, so it's a good
			// choice for testing multiple runs of the same context.
			for (int run = 0; run < 5; run++) {
				root.Run();

				Assert.AreEqual(new(10, 10, 50, 40), mainChild.Rect);
				// These rows should all be 10 units in height
				Assert.AreEqual(new(10, 10, 50, 10), rows[0].Rect);
				Assert.AreEqual(new(10, 20, 50, 10), rows[1].Rect);
				Assert.AreEqual(new(10, 30, 50, 10), rows[2].Rect);
				// This row should have 0 height
				Assert.AreEqual(new(10, 40, 50, 0), rows[3].Rect);
				Assert.AreEqual(new(10, 40, 50, 10), rows[4].Rect);

				// Each of these should be 10 units wide, and stacked horizontally
				Assert.AreEqual(5, cols1.Length);
				for (int i = 0; i < cols1.Length; i++) {
					Assert.AreEqual(new(10 + 10 * i, 10, 10, 10), cols1[i].Rect);
				}

				// The cols in the second row are similar to first row
				Assert.AreEqual(5, cols2.Length);
				for (int i = 0; i < cols2.Length; i++) {
					Assert.AreEqual(new(10 + 10 * i, 20, 10, 10), cols2[i].Rect);
				}

				// Leftmost (first of two items), aligned to bottom of row, 0 units tall
				Assert.AreEqual(new(10, 40, 25, 0), cols3[0].Rect);
				// Rightmost (second of two items), same height as row, which is 10 units tall
				Assert.AreEqual(new(35, 30, 25, 10), cols3[1].Rect);

				// These should all have size 0 and be in the middle of the row
				Assert.AreEqual(99, cols4.Length);
				for (int i = 0; i < cols4.Length; i++) {
					Assert.AreEqual(new(25 + 10, 40, 0, 0), cols4[i].Rect);
				}

				// These should all be 1 unit wide and 10 units tall
				Assert.AreEqual(50, cols5.Length);
				for (int i = 0; i < cols5.Length; i++) {
					Assert.AreEqual(new(10 + i, 40, 1, 10), cols5[i].Rect);
				}
			}
		}

		[TestMethod]
		public void DeepNest1()
		{
			const int numItems = 500;

			var root = new LayoutItem();
			var parent = root;
			for (int i = 0; i < numItems; i++) {
				var item = new LayoutItem();
				parent.AddChild(item);
				parent = item;
			}

			parent.Size = new(77, 99);
			root.Run();

			Assert.AreEqual(new(0, 0, 77, 99), root.Rect);
		}

		private static IEnumerable<LayoutItem> ManyChildrenGenerate(int num)
		{
			for (int i = 0; i < num; i++) {
				yield return new() {
					Size = new(1, 1),
				};
			}
		}

		[TestMethod]
		public void ManyChildren1()
		{
			const int numItems = 20000;

			var root = new LayoutItem {
				Size = new(1, 0),
				Contain = ContainFlags.Column,
			};

			root.AddChildren(ManyChildrenGenerate(numItems));
			root.Run();

			Assert.AreEqual(new(0, 0, 1, numItems), root.Rect);
		}

		[TestMethod]
		public void ChildAlign1()
		{
			var root = new LayoutItem {
				Size = new(50, 50),
			};

			var alignedBoxes = new LayoutItem[9];

			root.AddChild(alignedBoxes[0] = new() { Size = new(10, 10), Behave = BehaveFlags.Top | BehaveFlags.Left });
			root.AddChild(alignedBoxes[1] = new() { Size = new(10, 10), Behave = BehaveFlags.Top | BehaveFlags.Right });
			root.AddChild(alignedBoxes[2] = new() { Size = new(10, 10), Behave = BehaveFlags.Top | BehaveFlags.HCenter });

			root.AddChild(alignedBoxes[3] = new() { Size = new(10, 10), Behave = BehaveFlags.VCenter | BehaveFlags.Left });
			root.AddChild(alignedBoxes[4] = new() { Size = new(10, 10), Behave = BehaveFlags.VCenter | BehaveFlags.Right });
			root.AddChild(alignedBoxes[5] = new() { Size = new(10, 10), Behave = BehaveFlags.VCenter | BehaveFlags.HCenter });

			root.AddChild(alignedBoxes[6] = new() { Size = new(10, 10), Behave = BehaveFlags.Bottom | BehaveFlags.Left });
			root.AddChild(alignedBoxes[7] = new() { Size = new(10, 10), Behave = BehaveFlags.Bottom | BehaveFlags.Right });
			root.AddChild(alignedBoxes[8] = new() { Size = new(10, 10), Behave = BehaveFlags.Bottom | BehaveFlags.HCenter });

			root.Run();

			Assert.AreEqual(new(0, 0, 10, 10), alignedBoxes[0].Rect);
			Assert.AreEqual(new(40, 0, 10, 10), alignedBoxes[1].Rect);
			Assert.AreEqual(new(20, 0, 10, 10), alignedBoxes[2].Rect);

			Assert.AreEqual(new(0, 20, 10, 10), alignedBoxes[3].Rect);
			Assert.AreEqual(new(40, 20, 10, 10), alignedBoxes[4].Rect);
			Assert.AreEqual(new(20, 20, 10, 10), alignedBoxes[5].Rect);

			Assert.AreEqual(new(0, 40, 10, 10), alignedBoxes[6].Rect);
			Assert.AreEqual(new(40, 40, 10, 10), alignedBoxes[7].Rect);
			Assert.AreEqual(new(20, 40, 10, 10), alignedBoxes[8].Rect);
		}

		[TestMethod]
		public void ChildAlign2()
		{
			var root = new LayoutItem {
				Size = new(50, 50),
			};

			var alignedBoxes = new LayoutItem[6];

			root.AddChild(alignedBoxes[0] = new() { Size = new(10, 10), Behave = BehaveFlags.Top | BehaveFlags.HFill });
			root.AddChild(alignedBoxes[1] = new() { Size = new(10, 10), Behave = BehaveFlags.VCenter | BehaveFlags.HFill });
			root.AddChild(alignedBoxes[2] = new() { Size = new(10, 10), Behave = BehaveFlags.Bottom | BehaveFlags.HFill });

			root.AddChild(alignedBoxes[3] = new() { Size = new(10, 10), Behave = BehaveFlags.VFill | BehaveFlags.Left });
			root.AddChild(alignedBoxes[4] = new() { Size = new(10, 10), Behave = BehaveFlags.VFill | BehaveFlags.Right });
			root.AddChild(alignedBoxes[5] = new() { Size = new(10, 10), Behave = BehaveFlags.VFill | BehaveFlags.HCenter });

			root.Run();

			Assert.AreEqual(new(0, 0, 50, 10), alignedBoxes[0].Rect);
			Assert.AreEqual(new(0, 20, 50, 10), alignedBoxes[1].Rect);
			Assert.AreEqual(new(0, 40, 50, 10), alignedBoxes[2].Rect);

			Assert.AreEqual(new(0, 0, 10, 50), alignedBoxes[3].Rect);
			Assert.AreEqual(new(40, 0, 10, 50), alignedBoxes[4].Rect);
			Assert.AreEqual(new(20, 0, 10, 50), alignedBoxes[5].Rect);
		}

		[TestMethod]
		public void WrapRow1()
		{
			var root = new LayoutItem {
				Size = new(50, 50),
				Contain = ContainFlags.Row | ContainFlags.Wrap,
			};

			// We will create a 5x5 grid of boxes that are 10x10 units per each box.
			// There should be no empty space, gaps, or extra wrapping.

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int x = i % 5;
				int y = i / 5;
				Assert.AreEqual(new(x * 10, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapRow2()
		{
			var root = new LayoutItem {
				Size = new(57, 57),
				Contain = ContainFlags.Row | ContainFlags.Wrap,
				Align = AlignFlags.AlignStart,
			};

			// This one should ahve extra space on the right edge and bottom (7 units)

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int x = i % 5;
				int y = i / 5;
				Assert.AreEqual(new(x * 10, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapRow3()
		{
			var root = new LayoutItem {
				Size = new(57, 57),
				Contain = ContainFlags.Row | ContainFlags.Wrap,
				Align = AlignFlags.AlignEnd,
			};

			// This one should have extra space on the left edge and bottom (7 units)

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int x = i % 5;
				int y = i / 5;
				Assert.AreEqual(new(7 + x * 10, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapRow4()
		{
			var root = new LayoutItem {
				Size = new(58, 57),
				Contain = ContainFlags.Row | ContainFlags.Wrap,
				Align = AlignFlags.AlignMiddle,
			};

			root.AddChild(new LayoutItem {
				Size = new(58, 7),
			});

			// This one should split the horizontal extra space between the left and
			// right, and have the vertical extra space at the top (via extra inserted
			// spacer item, with explicit size)

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int x = i % 5;
				int y = i / 5;
				Assert.AreEqual(new(4 + x * 10, 7 + y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapRow5()
		{
			var root = new LayoutItem {
				Size = new(54, 50),
				Contain = ContainFlags.Row | ContainFlags.Wrap,
				Align = AlignFlags.AlignJustify,
			};

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			// Note that we are ignoring the last line here, as it will behave like AlignFlags.AlignStart.
			// Typically justifying does not need to be done here (it will behave like AlignFlags.AlignStart instead).
			// The author of layout.h calls this a bug, but we deem this intentional behavior.
			for (int i = 0; i < items.Length - 5; i++) {
				int x = i % 5;
				int y = i / 5;
				Assert.AreEqual(new(x * 11, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapColumn1()
		{
			var root = new LayoutItem {
				Size = new(50, 50),
				Contain = ContainFlags.Column | ContainFlags.Wrap,
			};

			// We will create a 5x5 grid of boxes that are 10x10 units per each box.
			// There should be no empty space, gaps, or extra wrapping.

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int y = i % 5;
				int x = i / 5;
				Assert.AreEqual(new(x * 10, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapColumn2()
		{
			var root = new LayoutItem {
				Size = new(57, 57),
				Contain = ContainFlags.Column | ContainFlags.Wrap,
				Align = AlignFlags.AlignStart,
			};

			// This one should have extra space on the right and bottom (7 units)

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int y = i % 5;
				int x = i / 5;
				Assert.AreEqual(new(x * 10, y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapColumn3()
		{
			var root = new LayoutItem {
				Size = new(57, 57),
				Contain = ContainFlags.Column | ContainFlags.Wrap,
				Align = AlignFlags.AlignEnd,
			};

			// This one should have extra space on the top and right (7 units)

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int y = i % 5;
				int x = i / 5;
				Assert.AreEqual(new(x * 10, 7 + y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void WrapColumn4()
		{
			var root = new LayoutItem {
				Size = new(57, 58),
				Contain = ContainFlags.Column | ContainFlags.Wrap,
				Align = AlignFlags.AlignMiddle,
			};

			root.AddChild(new LayoutItem {
				Size = new(7, 58),
			});

			// Just like wrap_row_4, but as columns instead of rows

			var items = new LayoutItem[5 * 5];
			for (int i = 0; i < items.Length; i++) {
				items[i] = new LayoutItem {
					Size = new(10, 10),
				};
			}
			root.AddChildren(items);
			root.Run();

			for (int i = 0; i < items.Length; i++) {
				int y = i % 5;
				int x = i / 5;
				Assert.AreEqual(new(7 + x * 10, 4 + y * 10, 10, 10), items[i].Rect);
			}
		}

		[TestMethod]
		public void AnchorRightMargin1()
		{
			var root = new LayoutItem {
				Size = new(100, 100),
			};

			var child = new LayoutItem {
				Size = new(50, 50),
				Margins = new(5, 5, 0, 0),
				Behave = BehaveFlags.Bottom | BehaveFlags.Right,
			};

			root.AddChild(child);
			root.Run();

			Assert.AreEqual(new(50, 50, 50, 50), child.Rect);
		}

		[TestMethod]
		public void AnchorRightMargin2()
		{
			var root = new LayoutItem {
				Size = new(100, 100),
			};

			var child = new LayoutItem {
				Size = new(50, 50),
				Margins = new(5, 5, 10, 10),
				Behave = BehaveFlags.Bottom | BehaveFlags.Right,
			};

			root.AddChild(child);
			root.Run();

			Assert.AreEqual(new(40, 40, 50, 50), child.Rect);
		}
	}
}
