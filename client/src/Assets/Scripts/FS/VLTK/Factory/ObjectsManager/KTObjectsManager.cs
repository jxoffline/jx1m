using FS.GameEngine.Interface;
using FS.GameEngine.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Factory
{
    /// <summary>
    /// Lớp quản lý Object trong Game
    /// </summary>
    public class KTObjectsManager : MonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Lớp quản lý Object trong Game
        /// </summary>
        public static KTObjectsManager Instance { get; private set; }
        #endregion

        #region Define
        /// <summary>
        /// Lớp định nghĩa loại đối tượng và Render tương ứng
        /// </summary>
        [Serializable]
        private class ObjectRender
        {
            /// <summary>
            /// Loại đối tượng
            /// </summary>
            public GSpriteTypes Type;

            /// <summary>
            /// Thực thi tối đa số Object trong 1 Frame
            /// </summary>
            public int MaxRenderPerFrame;

            /// <summary>
            /// Kích hoạt kiểm tra hàm Render Frame
            /// </summary>
            public bool EnableFrameRender = true;
        }

        /// <summary>
        /// Danh sách Render theo loại
        /// </summary>
        [SerializeField]
        private ObjectRender[] Renders;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách đối tượng trong Game theo loại
        /// </summary>
        private readonly Dictionary<GSpriteTypes, Dictionary<int, IObject>> objectsList = new Dictionary<GSpriteTypes, Dictionary<int, IObject>>();
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTObjectsManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            foreach (ObjectRender render in this.Renders)
            {
                this.objectsList[render.Type] = new Dictionary<int, IObject>();
                /// Nếu có kích hoạt Render
                if (render.EnableFrameRender)
                {
                    this.StartCoroutine(this.ProcessOnRender(render));
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện cho đối tượng trong mỗi Frame
        /// </summary>
        /// <param name="objectRender"></param>
        /// <returns></returns>
        private IEnumerator ProcessOnRender(ObjectRender objectRender)
        {
            /// Biến đếm tổng số đối tượng đã được Render ở Frame hiện tại
            int totalRenderedThisFrame = 0;
            /// Lặp liên tục
            while (true)
            {
                /// Nếu danh sách rỗng
                if (this.objectsList[objectRender.Type].Count <= 0)
                {
                    yield return null;
                    continue;
                }

                int i = 0;
                List<int> keys = this.objectsList[objectRender.Type].Keys.ToList();
                //KTDebug.LogWarning(string.Format("Type = {0}, TotalObjs = {1}", objectRender.Type, keys.Count));
                /// Lặp chừng nào chưa hết danh sách
                foreach (int objID in keys)
                {
                    /// Nếu tổng số Render đã vượt quá ngưỡng Config
                    if (totalRenderedThisFrame > objectRender.MaxRenderPerFrame)
                    {
                        totalRenderedThisFrame = 0;
                        yield return null;
                    }

                    /// Nếu objID không tồn tại tức đối tượng đã bị xóa thì bỏ qua
                    if (!this.objectsList[objectRender.Type].TryGetValue(objID, out IObject obj))
                    {
                        continue;
                    }

                    /// Nếu toác
                    if (obj == null)
					{
                        this.objectsList[objectRender.Type].Remove(objID);
                        continue;
					}

                    try
                    {
                        /// Thực hiện sự kiẹn OnFrameRender
                        obj.OnFrameRender();
                    }
                    catch (Exception ex)
                    {
                        KTDebug.LogError(ex.ToString());
                    }

                    /// Tăng biến chạy
                    i++;
                    /// Tăng tổng số đối tượng đã được Render ở Frame hiện tại
                    totalRenderedThisFrame++;
                }
                /// Bỏ qua Frame
                yield return null;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về danh sách các đối tượng đang quản lý hiện tại
        /// </summary>
        /// <returns></returns>
        public List<IObject> GetObjects()
        {
            List<IObject> objList = new List<IObject>();
            foreach (ObjectRender render in this.Renders)
            {
                objList.AddRange(this.objectsList[render.Type].Values);
            }
            return objList;
        }

        /// <summary>
        /// Trả về danh sách các đối tượng có loại tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<IObject> GetObjects(GSpriteTypes spriteTypes)
        {
            List<IObject> objList = new List<IObject>();
            if (this.objectsList.TryGetValue(spriteTypes, out Dictionary<int, IObject> objs))
            {
                objList.AddRange(objs.Values);
            }
            return objList;
        }

        /// <summary>
        /// Thêm đối tượng vào danh sách quản lý
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(IObject obj)
        {
            this.objectsList[obj.SpriteType][obj.BaseID] = obj;
        }

        /// <summary>
        /// Xóa đối tượng khỏi danh sách quản lý
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(IObject obj)
        {
            this.objectsList[obj.SpriteType].Remove(obj.BaseID);
        }

        /// <summary>
        /// Tìm đối tượng có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IObject FindObject(int id)
        {
            foreach (ObjectRender render in this.Renders)
            {
                if (this.objectsList[render.Type].TryGetValue(id, out IObject obj))
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Tìm đối tượng có ID tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T FindObject<T>(int id) where T : IObject
        {
            IObject obj = this.FindObject(id);
            if (obj != null && obj is T)
            {
                return (T) obj;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Tìm danh sách đối tượng theo điều kiện
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<IObject> FindObjects(Predicate<IObject> predicate)
        {
            List<IObject> listObjs = new List<IObject>();
            foreach (ObjectRender render in this.Renders)
            {
                //List<IObject> objs = this.objectsList[render.Type].Values.Where(x => predicate(x)).ToList();
                List<IObject> objs = new List<IObject>();
                foreach (IObject obj in this.objectsList[render.Type].Values)
				{
                    if (obj != null && predicate(obj))
					{
                        objs.Add(obj);
					}
				}

                if (objs.Count > 0)
                {
                    listObjs.AddRange(objs);
                }
            }
            return listObjs;
        }

        /// <summary>
        /// Tìm danh sách đối tượng theo điều kiện
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<T> FindObjects<T>(Predicate<T> predicate) where T : IObject
        {
            List<T> listObjs = new List<T>();
            foreach (ObjectRender render in this.Renders)
            {
                IObject first = this.objectsList[render.Type].Values.FirstOrDefault();
                if (first == null || !(first is T))
                {
                    continue;
                }

                //List<T> objs = this.objectsList[render.Type].Values.Where(x => x is T obj && predicate(obj)).Select(x => (T) x).ToList();
                List<T> objs = new List<T>();
                foreach (IObject obj in this.objectsList[render.Type].Values)
				{
                    if (obj != null && obj is T && predicate((T) obj))
					{
                        objs.Add((T) obj);
					}
				}

                if (objs.Count > 0)
                {
                    listObjs.AddRange(objs);
                }
            }
            return listObjs;
        }

        /// <summary>
        /// Làm rỗng danh sách đối tượng
        /// </summary>
        public void Clear()
        {
            foreach (ObjectRender render in this.Renders)
            {
                this.objectsList[render.Type].Clear();
            }
        }
        #endregion
    }
}
