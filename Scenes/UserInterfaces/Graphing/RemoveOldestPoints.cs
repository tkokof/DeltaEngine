﻿using DeltaEngine.Datatypes;

namespace DeltaEngine.Scenes.UserInterfaces.Graphing
{
	/// <summary>
	/// Restricts a graph to a limited number of points. Once more than that number are added
	/// points are removed from the start of the graph and all points are shifted backwards.
	/// </summary>
	internal class RemoveOldestPoints
	{
		public void Process(Graph graph)
		{
			if (MaximumNumberOfPoints > 0)
			foreach (GraphLine line in graph.Lines)
				PrunePointsFromLine(line);
		}

		public int MaximumNumberOfPoints;

		private void PrunePointsFromLine(GraphLine line)
		{
			var numberOfPointsToRemove = line.points.Count - MaximumNumberOfPoints;
			if (numberOfPointsToRemove <= 0)
				return;
			for (int i = 0; i < numberOfPointsToRemove; i++)
				RemoveFirstPointAndShiftOthersBack(line);
			line.Refresh();
		}

		private static void RemoveFirstPointAndShiftOthersBack(GraphLine line)
		{
			float interval = line.points[1].X - line.points[0].X;
			line.points.RemoveAt(0);
			for (int i = 0; i < line.points.Count; i++)
				line.points[i] = new Point(line.points[i].X - interval, line.points[i].Y);
		}
	}
}