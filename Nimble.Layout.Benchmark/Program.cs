namespace Nimble.Layout.Benchmark
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Running...");

			const int numRuns = 100_000;

			double totalTime = 0;

			for (int i = 0; i < numRuns; i++) {
				var start = DateTime.Now;
				var root = ConstructNestedBoxes();
				for (int j = 0; j < 5; j++) {
					root.Run();
				}
				totalTime += (DateTime.Now - start).TotalMilliseconds;
			}

			Console.WriteLine($"Total time: {totalTime}");
			Console.WriteLine($"  Avg time: {totalTime / numRuns}");
			Console.ReadKey();
		}

		static LayoutItem ConstructNestedBoxes()
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

			return root;
		}
	}
}

