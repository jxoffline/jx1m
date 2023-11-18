using System;
using System.Collections.Generic;
using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Server.Tools.AStarEx
{
    #region Structs
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeFast
    {
        #region Variables Declaration
        public float f;
        public float g;
        public float h;
        public int parentX;
        public int parentY;
		public byte ClosedStatus;
        #endregion
    }
    #endregion

    public class NodeGrid
    {		
		private int _startNodeX;
        private int _startNodeY;
		private int _endNodeX;
        private int _endNodeY;

        private static NodeFast[,] _nodes;

        private byte[,] _Obstructions;

        private static int _numCols;
        private static int _numRows;
		
		/**
		 * Constructor.
		 */
		public NodeGrid(int numCols, int numRows)
		{
			setSize( numCols, numRows );
		}
		
		
		////////////////////////////////////////
		// public methods
		////////////////////////////////////////
		
		/** 设置网格尺寸 */
		public void setSize( int numCols, int numRows)
		{
            if (_nodes == null || _numCols < numCols || _numRows < numRows)
            {
                _numCols = Math.Max(numCols, _numCols);
                _numRows = Math.Max(numRows, _numRows);

                _nodes = new NodeFast[_numCols, _numRows];
            }

            _Obstructions = new byte[numCols, numRows];

            for (int i = 0; i < numCols; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    _Obstructions[i, j] = 1;
                }
            }
		}

        /// <summary>
        /// 获取障碍物二维数组
        /// </summary>
        public byte[,] Obstructions
        {
            get { return _Obstructions;  }
        }

        public void Clear()
        {
            //
            //清空上次寻路余留的数据
            Array.Clear(_nodes, 0, _nodes.Length);
        }

        public NodeFast[,] Nodes
        {
            get
            {
                return _nodes;
            }
        }

        /** 判断两个节点的对角线路线是否可走 */
        public bool isDiagonalWalkable(int node1, int node2)
        {
            int node1x = ANode.GetGUID_X(node1);
            int node1y = ANode.GetGUID_Y(node1);

            int node2x = ANode.GetGUID_X(node2);
            int node2y = ANode.GetGUID_Y(node2);

            if (1 == _Obstructions[node1x, node2y] && 1 == _Obstructions[node2x, node1y])
            {
                return true;
            }
			return false;
        }
		
		/**
		 * Sets the node at the given coords as the end node.
		 * @param x The x coord.
		 * @param y The y coord.
		 */
		public void setEndNode(int x, int y)
		{
            _endNodeX = x;
            _endNodeY = y;
		}
		
		/**
		 * Sets the node at the given coords as the start node.
		 * @param x The x coord.
		 * @param y The y coord.
		 */
		public void setStartNode(int x, int y)
		{
            _startNodeX = x;
            _startNodeY = y;
		}
		
		/**
		 * Sets the node at the given coords as walkable or not.
		 * @param x The x coord.
		 * @param y The y coord.
		 */
		public void setWalkable(int x, int y, bool value)
		{
            if (value)
            {
                _Obstructions[x, y] = 1;
            }
            else
            {
                _Obstructions[x, y] = 0;
            }
		}

        /// <summary>
        /// 是否可行走
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool isWalkable(int x, int y)
        {
            return 1 == _Obstructions[x, y];
        }
		
		////////////////////////////////////////
		// getters / setters
		////////////////////////////////////////
		
		/**
		 * Returns the end node.
		 */
		public int endNodeX
		{
            get { return  _endNodeX; }
		}

        /**
         * Returns the end node.
         */
        public int endNodeY
        {
            get { return _endNodeY; }
        }
		
		/**
		 * Returns the number of columns in the grid.
		 */
		public int numCols
		{
            get { return _numCols; }
		}
		
		/**
		 * Returns the number of rows in the grid.
		 */
		public int numRows
		{
            get { return _numRows; }
		}
		
		/**
		 * Returns the start node.
		 */
		public int startNodeX
		{
            get {  return _startNodeX; }
		}

        /**
         * Returns the start node.
         */
        public int startNodeY
        {
            get { return _startNodeY; }
        }
    }
}
