﻿using System;
using System.Collections.Generic;
using DeltaEngine.Datatypes;

namespace CreepyTowers.PathFinding
{
	public abstract class GraphSearch
	{
		public abstract bool Search(Graph graph, int startNode, int targetNode);

		public List<Point> GetPath()
		{
			Stack<int> shortestPath = new Stack<int>();
			int currentNode = targetNode;
			while (currentNode != startNode)
			{
				shortestPath.Push(currentNode);
				currentNode = previousNode[currentNode];
			}
			return CalculateListToReturn(shortestPath, currentNode);
		}

		protected int startNode;
		protected int targetNode;
		protected int[] previousNode;

		private List<Point> CalculateListToReturn(Stack<int> shortestPath, int currentNode)
		{
			shortestPath.Push(currentNode);
			var list = new List<Point>();
			foreach (var nodeIndex in shortestPath.ToArray())
				list.Add(graph.Nodes[nodeIndex]);
			return list;
		}

		protected Graph graph;

		protected void Initialize()
		{
			costSoFar = new int[graph.NumberOfNodes];
			previousNode = new int[graph.NumberOfNodes];
			nodesToCheck.Clear();
			visitedNodes = 0;
			for (int i = 0; i < graph.NumberOfNodes; i++)
			{
				costSoFar[i] = Infinity;
				previousNode[i] = InvalidNodeIndex;
				nodesToCheck.Add(i);
			}
			costSoFar[startNode] = 0;
		}

		protected int[] costSoFar;
		protected List<int> nodesToCheck = new List<int>();
		protected const int Infinity = Int32.MaxValue;
		protected const int InvalidNodeIndex = -1;
		protected int visitedNodes;

		protected int GetNextNode()
		{
			int minCost = Infinity;
			int nextNode = InvalidNodeIndex;
			for (int i = 0; i < nodesToCheck.Count; i++)
			{
				if (costSoFar[nodesToCheck[i]] < minCost)
				{
					minCost = costSoFar[nodesToCheck[i]];
					nextNode = nodesToCheck[i];
				}
			}
			nodesToCheck.Remove(nextNode);
			return nextNode;
		}

		public int GetNumberOfVisitedNodes()
		{
			return visitedNodes;
		}
	}
}
