using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FS.Drawing;

namespace FS.Tools.AStar
{
    public delegate float CalcLineHandler(float value);

    /// <summary>
    /// 寻路数学扩展函数
    /// </summary>
	public static class MathUtilX
	{
  		/**
		 * 根据两点确定这两点连线的二元一次方程 y = ax + b或者 x = ay + b
		 * @param ponit1
		 * @param point2
		 * @param type		指定返回函数的形式。为0则根据x值得到y，为1则根据y得到x
		 * 
		 * @return 由参数中两点确定的直线的二元一次函数
		 */		
		public static CalcLineHandler getLineFunc(PointF ponit1, PointF point2, int type =0)
		{
			CalcLineHandler resultFuc = null;
			

			// 先考虑两点在一条垂直于坐标轴直线的情况，此时直线方程为 y = a 或者 x = a 的形式
			if( ponit1.X == point2.X )
			{
				if( type == 0 )
				{
					throw new Exception("两点所确定直线垂直于y轴，不能根据x值得到y值");
				}
				else if( type == 1 )
				{
					resultFuc =	( float y ) =>
								{
									return ponit1.X;
								};
						
				}
				return resultFuc;
			}
			else if( ponit1.Y == point2.Y )
			{
				if( type == 0 )
				{
					resultFuc =	( float y ) =>
					{
						return ponit1.Y;
					};
				}
				else if( type == 1 )
				{
					throw new Exception("两点所确定直线垂直于y轴，不能根据x值得到y值");
				}
				return resultFuc;
			}
			
			// 当两点确定直线不垂直于坐标轴时直线方程设为 y = ax + b
			float a;
			
			// 根据
			// y1 = ax1 + b
			// y2 = ax2 + b
			// 上下两式相减消去b, 得到 a = ( y1 - y2 ) / ( x1 - x2 ) 
			a = (ponit1.Y - point2.Y) / (ponit1.X - point2.X);
			
			float b;
			
			//将a的值代入任一方程式即可得到b
			b = ponit1.Y - a * ponit1.X;
			
			//把a,b值代入即可得到结果函数
			if( type == 0 )
			{
				resultFuc =	( float x ) =>
							{
								return a * x + b;
							};
			}
			else if( type == 1 )
			{
				resultFuc =	( float y ) =>
				{
					return (y - b) / a;
				};
			}
			
			return resultFuc;
		}
		
		/**
		 * 得到两点间连线的斜率 
		 * @param ponit1	
		 * @param point2
		 * @return 			两点间连线的斜率 
		 * 
		 */		
		public static float getSlope( PointF ponit1, PointF point2 )
		{
			return (point2.Y - ponit1.Y) / (point2.X - ponit1.X); 
		}
	}
}
