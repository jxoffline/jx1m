using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager.MapGridManager;

namespace HSGameEngine.Tools.AStarEx
{
    //二叉堆
    public class BinaryStack
    {
        public NodeGrid _nodeGrid = null;
        public List<long> _data = null;
		private Dictionary<long, int> _dict = null;
		/**
		 * @param compareValue	排序字段，若为空字符串则直接比较被添加元素本身的值,这个排序字段默认就是f
		 * 
		 */		
		public  BinaryStack(String compareValue = "f")
		{
            _data = new List<long>(1000);
			_dict = new Dictionary<long, int>(1000);
		}
		
		/** 向二叉堆中添加元素 
		 * @param node			欲添加的元素对象
		 */
		public void push(long guid)
		{
			//将新节点添至末尾先
            _data.Add(guid);
            _dict[guid] = _data.Count - 1;

			int len = _data.Count;
			
			//若数组中只有一个元素则省略排序过程，否则对新元素执行上浮过程
			if( len > 1 )
			{
				/** 新添入节点当前所在索引 */
				int index = len;
				
				/** 新节点当前父节点所在索引 */
				int parentIndex = index / 2 - 1;
				
				long temp;
				
				//和它的父节点（位置为当前位置除以2取整，比如第4个元素的父节点位置是2，第7个元素的父节点位置是3）比较，
				//如果新元素比父节点元素小则交换这两个元素，然后再和新位置的父节点比较，直到它的父节点不再比它大，
				//或者已经到达顶端，及第1的位置

                while (compareTwoNodes(guid, _data[parentIndex]))
				{
					temp = _data[parentIndex];
                    _data[parentIndex] = guid;
                    _dict[guid] = parentIndex;
					_data[index - 1] = temp;
                    _dict[temp] = index - 1;
					index /= 2;
					parentIndex = index / 2 - 1;

                    if (parentIndex < 0)
                    {
                        break;
                    }
				}
				
			}
			
		}
		
		/** 弹出开启列表中第一个元素 */
		public long shift()
		{
			//先弹出列首元素
			long result =  _data.ElementAt(0); 
            _data.RemoveAt(0);

            _dict.Remove(result);
			
			/** 数组长度 */
			int len = _data.Count;
			
			//若弹出列首元素后数组空了或者其中只有一个元素了则省略排序过程，否则对列尾元素执行下沉过程
			if( len > 1 )
			{
				/** 列尾节点 */
				long lastNode = _data.ElementAt(_data.Count - 1);
                _data.RemoveAt(_data.Count - 1);
				
				//将列尾元素排至首位
				_data.Insert(0, lastNode );
                _dict[lastNode] = 0;
				
				/** 末尾节点当前所在索引 */
				int index = 0;
				
				/** 末尾节点当前第一子节点所在索引 */
				int childIndex = (index + 1) * 2 - 1;
				
				/** 末尾节点当前两个子节点中较小的一个的索引 */
				int comparedIndex;
				
				long temp;
				
				//和它的两个子节点比较，如果较小的子节点比它小就将它们交换，直到两个子节点都比它大
				while( childIndex < len )
				{
					//只有一个子节点的情况
					if( childIndex + 1 == len )
					{
						comparedIndex = childIndex;
					}
					//有两个子节点则取其中较小的那个
					else
					{
						comparedIndex = compareTwoNodes(_data[childIndex], _data[childIndex + 1]) ? childIndex : childIndex + 1;
					}

                    if (comparedIndex < 0)
                    {
                        break;
                    }
					
					if( compareTwoNodes(_data[comparedIndex], lastNode) )
					{
						temp = _data[comparedIndex];
						_data[comparedIndex] = lastNode;
                        _dict[lastNode] = comparedIndex;
						_data[index] = temp;
                        _dict[temp] = index;
						index = comparedIndex;
						childIndex = (index + 1) * 2 - 1;
					}
					else
					{
						break;
					}
				}
				
			}
		
			return result;
		}
		
		/** 更新某一个节点的值。在你改变了二叉堆中某一节点的值以后二叉堆不会自动进行排序，所以你需要手动
		 *  调用此方法进行二叉树更新 */
		public void updateNode(long node)
		{
            int indexObj = indexOf(node);

            if (indexObj < 0)
            {
                return;
            }

			int index = indexObj + 1;

			int parentIndex = index / 2 - 1;

            if (parentIndex < 0)
            {
                return;
            }

			long temp;
			//上浮过程开始喽
			while( compareTwoNodes(node, _data[parentIndex]) )
			{
				temp = _data[parentIndex];
				_data[parentIndex] = node;
                _dict[node] = parentIndex;
				_data[index - 1] = temp;
                _dict[temp] = index - 1;
				index /= 2;
				parentIndex = index / 2 - 1;

                if (parentIndex < 0)
                {
                    break;
                }
			}
		}
		
		/** 查找某节点所在索引位置 */
        //程序内部没有使用list的index of，而是使用了一个词典存储index位置，这样查询
        //位置信息采用儿叉树，会比较快
		public int indexOf(long node)
		{
            int findIndex = -1;
            if (_dict.TryGetValue(node, out findIndex))
            {
                return findIndex;
            }

            return -1;
		}		
		
		public int getLength()
		{
			return _data.Count;
		}
		
		/**比较两个节点，返回true则表示第一个节点小于第二个*/
		private Boolean compareTwoNodes(long node1, long node2)
		{
            double f1 = _nodeGrid.Nodes[ANode.GetGUID_X(node1), ANode.GetGUID_Y(node1)].f;
            double f2 = _nodeGrid.Nodes[ANode.GetGUID_X(node2), ANode.GetGUID_Y(node2)].f;
            return f1 < f2;
		}
		
		/** 写此方法的目的在于快速trace出所需要查看的结果，直接trace一个Binary对象即可得到其中全部元素的值或
		 * 排序字段的值 */
		public String toString()
		{
			String result = "";

            int len = _data.Count;
            for (int i = 0; i < len; i++)
            {
                double f = _nodeGrid.Nodes[ANode.GetGUID_X(_data[i]), ANode.GetGUID_Y(_data[i])].f;
                result += f;
                if (i < len - 1) result += ",";
            }

			return result;
		}

        public void ClearAll()
        {
            _data.Clear();
            _dict.Clear();
        }
    }
}
