//#define ENABLE_DYNAMIC_OBS

using System;
using FS.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Server.Tools.AStarEx;

namespace FS.Tools.AStar 
{
    public class PathFinderFast : IPathFinder
    {
        #region Structs
	    [StructLayout(LayoutKind.Sequential, Pack=1)] 
        internal struct PathFinderNodeFast
        {
            #region Variables Declaration
            public int     F; // f = gone + heuristic
            public int     G;
            public ushort  PX; // Parent
            public ushort  PY;
            public byte    Status;
            #endregion
        }
        #endregion

        #region Events
        public event PathFinderDebugHandler PathFinderDebug;
        #endregion

        #region Variables Declaration
        // Heap variables are initializated to default, but I like to do it anyway
        private byte[,]                         mDynObs                 = null;
        private HashSet<byte>                   mOpenedDynObsLabels     = null;
        private byte[,]                         mGrid                   = null;
        private PriorityQueueB<int>             mOpen                   = null;
        private List<PathFinderNode>            mClose                  = new List<PathFinderNode>();
        private bool                            mStop                   = false;
        private bool                            mStopped                = true;
        private int                             mHoriz                  = 0;
        //private HeuristicFormula                mFormula              = HeuristicFormula.Manhattan;
        private HeuristicFormula                mFormula = HeuristicFormula.DiagonalShortCut;
        private bool                            mDiagonals              = true;
        private int                             mHEstimate              = 2;
        private bool                            mPunishChangeDirection  = false;
        private bool                            mReopenCloseNodes       = true;
        private bool                            mTieBreaker             = false;
        private bool                            mHeavyDiagonals         = false;
        private int                             mSearchLimit            = 2000;
        private double                          mCompletedTime          = 0;
        private bool                            mDebugProgress          = false;
        private bool                            mDebugFoundPath         = false;
        private static PathFinderNodeFast[]     mCalcGrid               = null;
        private byte                            mOpenNodeValue          = 1;
        private byte                            mCloseNodeValue         = 2;
        private int[,] mPunish = null;
        private int mMaxNum = 0;
        private bool mEnablePunish = false;
        
        //Promoted local variables to member variables to avoid recreation between calls
        private int                             mH                      = 0;
        private int                             mLocation               = 0;
        private int                             mNewLocation            = 0;
        private ushort                          mLocationX              = 0;
        private ushort                          mLocationY              = 0;
        private ushort                          mNewLocationX           = 0;
        private ushort                          mNewLocationY           = 0;
        private int                             mCloseNodeCounter       = 0;
        private ushort                          mGridX                  = 0;
        private ushort                          mGridY                  = 0;
        private ushort                          mGridXMinus1            = 0;
        private ushort                          mGridYLog2              = 0;
        private bool                            mFound                  = false;
        private sbyte[,]                        mDirection              = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
        private int                             mEndLocation            = 0;
        private int                             mNewG                   = 0;
        #endregion

        #region Constructors
        public PathFinderFast(byte[,] grid, byte[,] dynObs, HashSet<byte> openedDynObsLabels)
        {
            if (grid == null)
                throw new Exception("Grid cannot be null");

            mDynObs         = dynObs;
            mOpenedDynObsLabels = openedDynObsLabels;
            mGrid           = grid;
            mGridX          = (ushort) (mGrid.GetUpperBound(0) + 1);
            mGridY          = (ushort) (mGrid.GetUpperBound(1) + 1);
            mGridXMinus1    = (ushort) (mGridX - 1);
            mGridYLog2      = (ushort) Math.Log(mGridY, 2);

            // This should be done at the constructor, for now we leave it here.
            if (Math.Log(mGridX, 2) != (int)Math.Log(mGridX, 2) ||
                Math.Log(mGridY, 2) != (int)Math.Log(mGridY, 2))
                throw new Exception("Invalid Grid, size in X and Y must be power of 2");

            //if (mCalcGrid == null || mCalcGrid.Length != (mGridX * mGridY))
            if (mCalcGrid == null || mCalcGrid.Length < (mGridX * mGridY))
            {
                mCalcGrid = new PathFinderNodeFast[mGridX * mGridY];
            }

            mOpen   = new PriorityQueueB<int>(new ComparePFNodeMatrix(mCalcGrid));
        }

        #endregion

        #region Properties
        public bool Stopped
        {
            get { return mStopped; }
        }

        public HeuristicFormula Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        public bool Diagonals
        {
            get { return mDiagonals; }
            set 
            { 
                mDiagonals = value; 
                if (mDiagonals)
                    mDirection = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
                else
                    mDirection = new sbyte[4,2]{{0,-1} , {1,0}, {0,1}, {-1,0}};
            }
        }

        public bool HeavyDiagonals
        {
            get { return mHeavyDiagonals; }
            set { mHeavyDiagonals = value; }
        }

        public int HeuristicEstimate
        {
            get { return mHEstimate; }
            set { mHEstimate = value; }
        }

        public bool PunishChangeDirection
        {
            get { return mPunishChangeDirection; }
            set { mPunishChangeDirection = value; }
        }

        public bool ReopenCloseNodes
        {
            get { return mReopenCloseNodes; }
            set { mReopenCloseNodes = value; }
        }

        public bool TieBreaker
        {
            get { return mTieBreaker; }
            set { mTieBreaker = value; }
        }

        public int SearchLimit
        {
            get { return mSearchLimit; }
            set { mSearchLimit = value; }
        }

        public double CompletedTime
        {
            get { return mCompletedTime; }
            set { mCompletedTime = value; }
        }

        public bool DebugProgress
        {
            get { return mDebugProgress; }
            set { mDebugProgress = value; }
        }

        public bool DebugFoundPath
        {
            get { return mDebugFoundPath; }
            set { mDebugFoundPath = value; }
        }

        public int[,] Punish
        {
            get { return mPunish; }
            set { mPunish = value; }
        }

        public int MaxNum
        {
            get { return mMaxNum; }
            set { mMaxNum = value; }
        }

        public bool EnablePunish
        {
            get { return mEnablePunish; }
            set { mEnablePunish = value; }
        }

        #endregion

        #region Methods
        public void FindPathStop()
        {
            mStop = true;
        }

        public List<PathFinderNode> FindPath(Point start, Point end)
        {
            return FindPath(new Point2D((int)start.X, (int)start.Y), new Point2D((int)end.X, (int)end.Y));
        }

        private int GetPunishNum(int x, int y)
        {
            if (!mEnablePunish) return 0;
            if (null == mPunish) return 0;
            return mMaxNum - Math.Min(mPunish[x, y], 3);
        }

        public List<PathFinderNode> FindPath(Point2D start, Point2D end)
        {
            //start = new Point2D(88, 159);
            //end = new Point2D(81, 160);

            lock (this)
            {
                Array.Clear(mCalcGrid, 0, mCalcGrid.Length);

                mFound              = false;
                mStop               = false;
                mStopped            = false;
                mCloseNodeCounter   = 0;
                //mOpenNodeValue      += 2;
                //mCloseNodeValue     += 2;
                mOpen.Clear();
                mClose.Clear();

                #if DEBUGON
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
                #endif

                mLocation                      = (start.Y << mGridYLog2) + start.X;
                mEndLocation                   = (end.Y << mGridYLog2) + end.X;
                mCalcGrid[mLocation].G         = 0;
                mCalcGrid[mLocation].F         = mHEstimate;
                mCalcGrid[mLocation].PX        = (ushort) start.X;
                mCalcGrid[mLocation].PY        = (ushort) start.Y;
                mCalcGrid[mLocation].Status    = mOpenNodeValue;

                mOpen.Push(mLocation);
                while(mOpen.Count > 0 && !mStop)
                {
                    mLocation    = mOpen.Pop();

                    //Is it in closed list? means this node was already processed
                    if (mCalcGrid[mLocation].Status == mCloseNodeValue)
                        continue;

                    mLocationX   = (ushort) (mLocation & mGridXMinus1);
                    mLocationY   = (ushort) (mLocation >> mGridYLog2);
                    
                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocation & mGridXMinus1, mLocation >> mGridYLog2, PathFinderNodeType.Current, -1, -1);
                    #endif

                    if (mLocation == mEndLocation)
                    {
                        mCalcGrid[mLocation].Status = mCloseNodeValue;
                        mFound = true;
                        break;
                    }

                    if (mCloseNodeCounter > mSearchLimit)
                    {
                        mStopped = true;
                        return null;
                    }

                    if (mPunishChangeDirection)
                        mHoriz = (mLocationX - mCalcGrid[mLocation].PX); 

                    //Lets calculate each successors
                    for (int i=0; i<(mDiagonals ? 8 : 4); i++)
                    {
                        mNewLocationX = (ushort) (mLocationX + mDirection[i,0]);
                        mNewLocationY = (ushort) (mLocationY + mDirection[i,1]);
                        mNewLocation  = (mNewLocationY << mGridYLog2) + mNewLocationX;

                        if (mNewLocationX >= mGridX || mNewLocationY >= mGridY)
                            continue;

                        if (mCalcGrid[mNewLocation].Status == mCloseNodeValue && !mReopenCloseNodes)
                            continue;

                        // Unbreakeable?
                        if (mGrid[mNewLocationX, mNewLocationY] == 0)
                            continue;
#if ENABLE_DYNAMIC_OBS
                        if (mDynObs != null && mDynObs[mNewLocationX, mNewLocationY] > 0 && !mOpenedDynObsLabels.Contains(mDynObs[mNewLocationX, mNewLocationY]))
                            continue;
#endif

                        if (mHeavyDiagonals && i>3)
                            mNewG = mCalcGrid[mLocation].G + (int) (mGrid[mNewLocationX, mNewLocationY] * 2.41);
                        else
                            mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY];

                        if (mPunishChangeDirection)
                        {
                            if ((mNewLocationX - mLocationX) != 0)
                            {
                                if (mHoriz == 0)
                                    mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                            }
                            if ((mNewLocationY - mLocationY) != 0)
                            {
                                if (mHoriz != 0)
                                    mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                            }
                        }

                        mNewG = mNewG + GetPunishNum(mNewLocationX, mNewLocationY);

                        //Is it open or closed?
                        if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue)
                        {
                            // The current node has less code than the previous? then skip this node
                            if (mCalcGrid[mNewLocation].G <= mNewG)
                                continue;
                        }

                        mCalcGrid[mNewLocation].PX      = mLocationX;
                        mCalcGrid[mNewLocation].PY      = mLocationY;
                        mCalcGrid[mNewLocation].G       = mNewG;

                        switch(mFormula)
                        {
                            default:
                            case HeuristicFormula.Manhattan:
                                mH = mHEstimate * (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                                break;
                            case HeuristicFormula.MaxDXDY:
                                mH = mHEstimate * (Math.Max(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y)));
                                break;
                            case HeuristicFormula.DiagonalShortCut:
                                int h_diagonal  = Math.Min(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
                                int h_straight  = (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                                mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                                break;
                            case HeuristicFormula.Euclidean:
                                mH = (int) (mHEstimate * Math.Sqrt(Math.Pow((mNewLocationY - end.X) , 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                                break;
                            case HeuristicFormula.EuclideanNoSQR:
                                mH = (int) (mHEstimate * (Math.Pow((mNewLocationX - end.X) , 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                                break;
                            case HeuristicFormula.Custom1:
                                Point2D dxy = new Point2D(Math.Abs(end.X - mNewLocationX), Math.Abs(end.Y - mNewLocationY));
                                int Orthogonal  = Math.Abs(dxy.X - dxy.Y);
                                int Diagonal    = Math.Abs(((dxy.X + dxy.Y) - Orthogonal) / 2);
                                mH = mHEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);
                                break;
                        }
                        if (mTieBreaker)
                        {
                            int dx1 = mLocationX - end.X;
                            int dy1 = mLocationY - end.Y;
                            int dx2 = start.X - end.X;
                            int dy2 = start.Y - end.Y;
                            int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                            mH = (int) (mH + cross * 0.001);
                        }
                        mCalcGrid[mNewLocation].F = mNewG + mH;

                        #if DEBUGON
                        if (mDebugProgress && PathFinderDebug != null)
                            PathFinderDebug(mLocationX, mLocationY, mNewLocationX, mNewLocationY, PathFinderNodeType.Open, mCalcGrid[mNewLocation].F, mCalcGrid[mNewLocation].G);
                        #endif

                        //It is faster if we leave the open node in the priority queue
                        //When it is removed, it will be already closed, it will be ignored automatically
                        //if (tmpGrid[newLocation].Status == 1)
                        //{
                        //    //int removeX   = newLocation & gridXMinus1;
                        //    //int removeY   = newLocation >> gridYLog2;
                        //    mOpen.RemoveLocation(newLocation);
                        //}

                        //if (tmpGrid[newLocation].Status != 1)
                        //{
                            mOpen.Push(mNewLocation);
                        //}
                        mCalcGrid[mNewLocation].Status = mOpenNodeValue;
                    }

                    mCloseNodeCounter++;
                    mCalcGrid[mLocation].Status = mCloseNodeValue;

                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocationX, mLocationY, PathFinderNodeType.Close, mCalcGrid[mLocation].F, mCalcGrid[mLocation].G);
                    #endif
                }
				
                if (mFound)
                {
                    mClose.Clear();
                    int posX = end.X;
                    int posY = end.Y;

                    PathFinderNodeFast fNodeTmp = mCalcGrid[(end.Y << mGridYLog2) + end.X];
                    PathFinderNode fNode;
                    fNode.F  = fNodeTmp.F;
                    fNode.G  = fNodeTmp.G;
                    fNode.H  = 0;
                    fNode.PX = fNodeTmp.PX;
                    fNode.PY = fNodeTmp.PY;
                    fNode.X  = end.X;
                    fNode.Y  = end.Y;

                    while(fNode.X != fNode.PX || fNode.Y != fNode.PY)
                    {
                        mClose.Add(fNode);
                        #if DEBUGON
                        if (mDebugFoundPath && PathFinderDebug != null)
                            PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                        #endif
                        posX = fNode.PX;
                        posY = fNode.PY;
                        fNodeTmp = mCalcGrid[(posY << mGridYLog2) + posX];
                        fNode.F  = fNodeTmp.F;
                        fNode.G  = fNodeTmp.G;
                        fNode.H  = 0;
                        fNode.PX = fNodeTmp.PX;
                        fNode.PY = fNodeTmp.PY;
                        fNode.X  = posX;
                        fNode.Y  = posY;
                    } 

                    mClose.Add(fNode);
                    #if DEBUGON
                    if (mDebugFoundPath && PathFinderDebug != null)
                        PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                    #endif

                    ////进行简单的平滑处理
                    mClose = Floyd(mClose, mGrid, mDynObs, mOpenedDynObsLabels);

                    mStopped = true;
                    return mClose;
                }
                mStopped = true;
                return null;
            }
        }
        #endregion

        #region Inner Classes
        internal class ComparePFNodeMatrix : IComparer<int>
        {
            #region Variables Declaration
            PathFinderNodeFast[] mMatrix;
            #endregion

            #region Constructors
            public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
            {
                mMatrix = matrix;
            }
            #endregion

            #region IComparer Members
            public int Compare(int a, int b)
            {
                if (mMatrix[a].F > mMatrix[b].F)
                    return 1;
                else if (mMatrix[a].F < mMatrix[b].F)
                    return -1;
                return 0;
            }
            #endregion
        }
        #endregion

        #region 弗洛伊德路径平滑处理

        /* 弗洛伊德路径平滑处理 
		form http://wonderfl.net/c/aWCe
        */
        private static List<PathFinderNode> Floyd(List<PathFinderNode> _floydPath, byte[,] grid, byte[,] dynObs, HashSet<byte> openedDynObsLabels)
        {
            if (null == _floydPath || _floydPath.Count <= 0)
            {
                return null;
            }

            _floydPath = ReverseList(_floydPath);

            int len = _floydPath.Count;
            if (len > 2)
            {
                PathFinderNode vector = new PathFinderNode();
                PathFinderNode tempVector = new PathFinderNode();

                //遍历路径数组中全部路径节点，合并在同一直线上的路径节点
                //假设有1,2,3,三点，若2与1的横、纵坐标差值分别与3与2的横、纵坐标差值相等则
                //判断此三点共线，此时可以删除中间点2
                FloydVector(ref vector, _floydPath[len - 1], _floydPath[len - 2]);

                for (int i = _floydPath.Count - 3; i >= 0; i--)
                {
                    FloydVector(ref tempVector, _floydPath[i + 1], _floydPath[i]);
					
                    if (vector.X == tempVector.X && vector.Y == tempVector.Y)
                    {
                        _floydPath.RemoveAt(i + 1);
                    }
                    else
                    {
                        vector.X = tempVector.X;
                        vector.Y = tempVector.Y;
                    }
                }
            }

            //合并共线节点后进行第二步，消除拐点操作。算法流程如下：
            //如果一个路径由1-10十个节点组成，那么由节点10从1开始检查
            //节点间是否存在障碍物，若它们之间不存在障碍物，则直接合并
            //此两路径节点间所有节点。
            len = _floydPath.Count;
            for (int i = len - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i - 2; j++)
                {
                    if (hasBarrier(grid, dynObs, openedDynObsLabels, _floydPath[i].X, _floydPath[i].Y, _floydPath[j].X, _floydPath[j].Y) == false)
                    {
                        for (int k = i - 1; k > j; k--)
                        {
                            _floydPath.RemoveAt(k);
                        }
                        i = j;
                        len = _floydPath.Count;
                        break;
                    }
                }
            }

            return _floydPath;
        }

        private static List<PathFinderNode> ReverseList(List<PathFinderNode> floydPath)
        {
            floydPath.Reverse(0, floydPath.Count);
            return floydPath;
        }

        private static void FloydVector(ref PathFinderNode target, PathFinderNode n1, PathFinderNode n2)
        {
            target.X = n1.X - n2.X;
            target.Y = n1.Y - n2.Y;
        }

        /**
         * 判断两节点之间是否存在障碍物 
         * @param point1
         * @param point2
         * @return 
         * 
         */
        private static bool hasBarrier(byte[,] grid, byte[,] dynObs, HashSet<byte> openedDynObsLabels, int startX, int startY, int endX, int endY)
        {
            //如果起点终点是同一个点那傻子都知道它们间是没有障碍物的
            if( startX == endX && startY == endY )return false;
            if (grid[endX, endY] == 0) return true;
#if ENABLE_DYNAMIC_OBS
            if (dynObs != null && dynObs[endX, endY] > 0 && !openedDynObsLabels.Contains(dynObs[endX, endY])) return true;
#endif

            //两节点中心位置
            PointF point1 = new PointF( startX + 0.5f, startY + 0.5f );
            PointF point2 = new PointF( endX + 0.5f, endY + 0.5f );
			
            float distX = Math.Abs(endX - startX);
            float distY = Math.Abs(endY - startY);									
			
            /**遍历方向，为true则为横向遍历，否则为纵向遍历*/
            bool loopDirection = distX > distY ? true : false;

            CalcLineHandler lineFuction = null;
					
            /** 循环递增量 */
            float i = 0;
			
            /** 循环起始值 */
            int loopStart = 0;
			
            /** 循环终结值 */
            int loopEnd = 0;
			
            /** 起终点连线所经过的节点 */
            List<ANode> nodesPassed = null;
            ANode elem = null;
			
            //为了运算方便，以下运算全部假设格子尺寸为1，格子坐标就等于它们的行、列号
            if( loopDirection )
            {				
                lineFuction = MathUtilX.getLineFunc(point1, point2, 0);
				
                loopStart = Math.Min( startX, endX );
                loopEnd = Math.Max( startX, endX );
				
                //开始横向遍历起点与终点间的节点看是否存在障碍(不可移动点) 
                for( i=loopStart; i<=loopEnd; i++ )
                {
                    //由于线段方程是根据终起点中心点连线算出的，所以对于起始点来说需要根据其中心点
                    //位置来算，而对于其他点则根据左上角来算
                    if( i==loopStart ) i += 0.5f;

                    //根据x得到直线上的y值
                    float yPos = lineFuction(i);				
					
                    nodesPassed = getNodesUnderPoint( i, yPos );
                    for (int n = 0; n < nodesPassed.Count; n++)
                    {
                        elem = nodesPassed[n];
                        if (grid[elem.x, elem.y] == 0) return true;
#if ENABLE_DYNAMIC_OBS
                        if (dynObs != null && dynObs[elem.x, elem.y] > 0 && !openedDynObsLabels.Contains(dynObs[elem.x, elem.y])) return true;
#endif
                    }

					
                    if( i == loopStart + 0.5f ) i -= 0.5f;
                }
            }
            else
            {
                lineFuction = MathUtilX.getLineFunc(point1, point2, 1);
				
                loopStart = Math.Min( startY, endY );
                loopEnd = Math.Max( startY, endY );
				
                //开始纵向遍历起点与终点间的节点看是否存在障碍(不可移动点)
                for( i=loopStart; i<=loopEnd; i++ )
                {
                    if( i==loopStart ) i += 0.5f;

                    //根据y得到直线上的x值
                    float xPos = lineFuction(i);
					
                    nodesPassed = getNodesUnderPoint( xPos, i );
                    for (int n = 0; n < nodesPassed.Count; n++)
                    {
                        elem = nodesPassed[n];
                        if (grid[elem.x, elem.y] == 0) return true;
#if ENABLE_DYNAMIC_OBS
                        if (dynObs != null && dynObs[elem.x, elem.y] > 0 && !openedDynObsLabels.Contains(dynObs[elem.x, elem.y])) return true;
#endif
                    }
										
                    if( i == loopStart + 0.5f ) i -= 0.5f;
                }
            }			
			
            return false;			
        }

  		/**
		 * 得到一个点下的所有节点 
		 * @param xPos		点的横向位置
		 * @param yPos		点的纵向位置
		 * @param grid		所在网格
		 * @param exception	例外格，若其值不为空，则在得到一个点下的所有节点后会排除这些例外格
		 * @return 			共享此点的所有节点
		 * 
		 */
        private static List<ANode> getNodesUnderPoint(float xPos, float yPos)
		{
			List<ANode> result = new List<ANode>();
			bool xIsInt = xPos % 1 == 0;
			bool yIsInt = yPos % 1 == 0;
			
			//点由四节点共享情况
			if( xIsInt && yIsInt )
			{
				result.Add(new ANode( (int)xPos - 1, (int)yPos - 1));
				result.Add(new ANode( (int)xPos, (int)yPos - 1));
				result.Add(new ANode( (int)xPos - 1, (int)yPos));
				result.Add(new ANode( (int)xPos, (int)yPos));
			}
				//点由2节点共享情况
				//点落在两节点左右临边上
			else if( xIsInt && !yIsInt )
			{
				result.Add(new ANode( (int)xPos - 1, (int)(yPos) ));
				result.Add(new ANode( (int)xPos, (int)(yPos) ));
			}
				//点落在两节点上下临边上
			else if( !xIsInt && yIsInt )
			{
				result.Add(new ANode( (int)(xPos), (int)yPos - 1 ));
				result.Add(new ANode( (int)(xPos), (int)yPos ));
			}
				//点由一节点独享情况
			else
			{
                result.Add(new ANode((int)(xPos), (int)(yPos)));
			}
			
			return result;
        }

        #endregion 弗洛伊德路径平滑处理
    }
}
