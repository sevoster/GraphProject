﻿using QuickGraph;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphComponent.GraphBuilder
{
    static public class GraphBuilderStrategy
    {
        static public bool ValidateCode(List<int> code)
        {
            if (code.Max() > code.Count() + 2)
                return false;
            return true;
        }

        static public bool ValidateOrientedGraph(Tree tree)
        {
            if (tree == null || tree.VertexCount == 0)
                return false;

            int rootCounter = 0;
            CustomVertex root = null;
            foreach (var vertex in tree.Vertices)
            {
                IEnumerable<CustomEdge> edges;
                if (tree.TryGetInEdges(vertex, out edges) && edges.Count() == 0)
                {
                    rootCounter++;
                    root = vertex;
                }
            }
            if (rootCounter != 1 || root == null)
                return false;

            bool hasCycle = false;

            BreadthFirstSearchAlgorithm<CustomVertex, CustomEdge> bfs = new BreadthFirstSearchAlgorithm<CustomVertex, CustomEdge>(tree);
            bfs.NonTreeEdge += u => hasCycle = true;
            bfs.Compute(root);

            bool notAllVisited = bfs.VertexColors.Any(x => x.Value == GraphColor.White);

            return !(hasCycle || notAllVisited);
        }

        static public bool ValidateNonOrientedGraph(Tree tree)
        {
            if (tree == null || tree.VertexCount == 0)
                return false;

            bool bfsCycle = false;
            List<CustomEdge> nonTreeEdges = new List<CustomEdge>();
            UndirectedBidirectionalGraph<CustomVertex, CustomEdge> undirectTree = new UndirectedBidirectionalGraph<CustomVertex, CustomEdge>(tree);
            UndirectedBreadthFirstSearchAlgorithm<CustomVertex, CustomEdge> bfs = new UndirectedBreadthFirstSearchAlgorithm<CustomVertex, CustomEdge>(undirectTree);
            bfs.GrayTarget += (u,v) => bfsCycle=true;
            bfs.Compute(tree.Vertices.ElementAt(0));
            List<CustomVertex> verticies = tree.Vertices.ToList();
            List<int> numerate = new List<int>();
            foreach (var vert in verticies)
            {
                numerate.Add(vert.ID);
            }
            if (numerate.Max() > numerate.Count())
                return false;
            return !(bfsCycle || bfs.VertexColors.Any(x => x.Value == GraphColor.White));
        }

        static public Tree CodeToGraph(List<int> code)
        {
            Tree NewGraph = new Tree();
            List<CustomVertex> Vertices = new List<CustomVertex>();
            List<int> used = new List<int>();
            for (int i = 0; i < code.Count() + 2; i++)
            {
                used.Add(i + 1);
                Vertices.Add(new CustomVertex(i + 1));
            }
            NewGraph.AddVertexRange(Vertices);
            while (code.Count() != 0)
            {
                int use = 0;
                while (code.Contains(used[use]))
                    use++;
                CustomEdge Edgee = new CustomEdge(Vertices[code[0] - 1], Vertices[used[use] - 1]);
                NewGraph.AddEdge(Edgee);
                used.RemoveAt(use);
                code.RemoveAt(0);
            }
            CustomEdge E = new CustomEdge(Vertices[used[0] - 1], Vertices[used[1] - 1]);
            NewGraph.AddEdge(E);
            return NewGraph;
        }

        static public List<int> GraphToCode(Tree Graph)
        {
            List<CustomVertex> Verticies = Graph.Vertices.ToList();
            List<CustomEdge> Edges = Graph.Edges.ToList();
            List<int[]> ListEdges = new List<int[]>();
            for (int i = 0; i < Edges.Count(); i++)
            {
                int[] current_edge = new int[2] { Edges[i].Source.ID, Edges[i].Target.ID };
                ListEdges.Add(current_edge);
            }
            List<int> Answer = new List<int>();

            while (Verticies.Count() > 2)
            {
                int min = int.MaxValue;
                int min_ref = int.MaxValue;
                int for_remove_vert = int.MaxValue;
                int for_remove_edge = int.MaxValue;
                for (int k = 0; k < Verticies.Count(); k++)
                {
                    int maybe_ref = int.MaxValue;
                    int counter = 0;
                    int id = Verticies[k].ID;
                    int may_rem = int.MaxValue;
                    for (int i = 0; i < ListEdges.Count(); i++)
                        for (int j = 0; j < 2; j++)
                            if (id == ListEdges[i][j])
                            {
                                counter++;
                                maybe_ref = ListEdges[i][(j + 1) % 2];
                                may_rem = i;
                            }
                    if ((counter == 1) && (min > id))
                    {
                        min = id;
                        min_ref = maybe_ref;
                        for_remove_vert = k;
                        for_remove_edge = may_rem;
                    }

                }
                Answer.Add(min_ref);
                Verticies.Remove(Verticies[for_remove_vert]);
                ListEdges.RemoveAt(for_remove_edge);
            }

            return Answer;
        }

        static public string GeneratePruferCode(int num)
        {
            List<int> code = new List<int>();
            Random random = new Random();
            for (int i = 0; i < num - 2; i++)
            {
                code.Add(random.Next(1, num + 1));
            }
            return string.Join(" ", code.Select(x => x.ToString()).ToArray());
        }

        static public Tree GenerateGraph(int vertices)
        {
            return CodeToGraph(GeneratePruferCode(vertices).Split(' ', ',', ';').Select(int.Parse).ToList());
        }
    }
}
