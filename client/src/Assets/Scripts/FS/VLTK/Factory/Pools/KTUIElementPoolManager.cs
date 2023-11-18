using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory
{
    /// <summary>
    /// Đối tượng quản lý thêm/trả các thành phần của UI
    /// </summary>
    public class KTUIElementPoolManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Spawn
        /// </summary>
        public static KTUIElementPoolManager Instance { get; private set; }
        #endregion

        /// <summary>
        /// Lớp định nghĩa đối tượng mẫu trong Pool
        /// </summary>
        [Serializable]
        private class Model
        {
            /// <summary>
            /// Đối tượng
            /// </summary>
            public RectTransform Object;

            /// <summary>
            /// Tên đối tượng
            /// </summary>
            public string Name;

            /// <summary>
            /// Số lượng Spawn mặc định ban đầu
            /// </summary>
            public int SpawnCount = 50;

            /// <summary>
            /// Có thể mở rộng không
            /// </summary>
            public bool Expandable = true;

            /// <summary>
            /// Số lượng Spawn tối đa, vượt quá số này thì không cho Spawn nữa
            /// </summary>
            public int MaxSpawn = -1;

#if UNITY_EDITOR
            /// <summary>
            /// Số Spawn khả dụng
            /// </summary>
            public int AvailableSpawns = 0;
#endif
        }

        /// <summary>
        /// Đối tượng Spawn
        /// </summary>
        private class Spawn
        {
            /// <summary>
            /// Tên đối tượng
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Đối tượng
            /// </summary>
            public RectTransform Object { get; set; }
        }

        #region Define
        /// <summary>
        /// Danh sách Spawn
        /// </summary>
        [SerializeField]
        private Model[] Models;

        /// <summary>
        /// Số đối tượng tạo ra tối đa trong 1 Frame
        /// </summary>
        [SerializeField]
        private int MaxSpawnEachFrame = 10;
        #endregion

        /// <summary>
        /// Danh sách spawn khả dụng
        /// </summary>
        private readonly Dictionary<string, List<Spawn>> listSpawns = new Dictionary<string, List<Spawn>>();

        /// <summary>
        /// Danh sách spawn không còn khả dụng
        /// </summary>
        private readonly Dictionary<RectTransform, Spawn> listUnavailableSpawns = new Dictionary<RectTransform, Spawn>();

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTUIElementPoolManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.MakeSpawns());
        }

#if UNITY_EDITOR
        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.Editor_Statistify();
        }
#endif
        #endregion

        #region Private methods
#if UNITY_EDITOR
        /// <summary>
        /// Thống kê số lượng đối tượng khả dụng
        /// </summary>
        private void Editor_Statistify()
        {
            foreach (Model model in this.Models)
            {
                if (this.listSpawns.TryGetValue(model.Name, out _))
                {
                    model.AvailableSpawns = this.listSpawns[model.Name].Count;
                }
            }
        }
#endif

        /// <summary>
        /// Tạo đối tượng Spawn từ Model tương ứng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Spawn MakeSpawn(Model model)
        {
            /// Spawn đối tượng
            RectTransform go = GameObject.Instantiate<RectTransform>(model.Object);
            go.transform.SetParent(this.gameObject.transform, false);
            go.gameObject.SetActive(false);
            go.name = model.Object.name;

            /// Tạo mới đối tượng Spawn
            Spawn spawn = new Spawn()
            {
                Name = model.Name,
                Object = go,
            };
            /// Trả về đối tượng Spawn vừa tạo ra
            return spawn;
        }

        /// <summary>
        /// Tạo các đối tượng Spawn tương ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator MakeSpawns()
        {
            /// Tổng số đã spawn ở frame này
            int spawnedThisFrame = 0;
            /// Duyệt toàn bộ danh sách Model hiện có
            foreach (Model model in this.Models)
            {
                this.listSpawns[model.Name] = new List<Spawn>();

                for (int i = 1; i <= model.SpawnCount; i++)
                {
                    /// Tọa mới đối tượng Spawn
                    Spawn spawn = this.MakeSpawn(model);

                    /// Thêm đối tượng mới Spawn vào danh sách
                    this.listSpawns[model.Name].Add(spawn);

                    /// Tăng số đối tượng đã spawn ở frame này lên
                    spawnedThisFrame++;

                    /// Nếu tổng số spawn ở Frame này lớn hơn Max
                    if (this.MaxSpawnEachFrame != -1 && spawnedThisFrame >= this.MaxSpawnEachFrame)
                    {
                        /// Bỏ qua frame này
                        yield return null;
                        /// Reset giá trị tính tổng số spawn tạo ra ở frame tương ứng
                        spawnedThisFrame = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy đối tượng Spawn tương ứng ra khỏi Pool, hoặc tạo mới nếu Pool đã đầy
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Spawn TakeSpawn(string name)
        {
            /// Nếu trong danh sách không tồn tại đối tượng nào có tên tương ứng
            if (!this.listSpawns.TryGetValue(name, out List<Spawn> spawns))
            {
                KTDebug.LogError(string.Format("No object named '{0}' inside Pool!", name));
                return null;
            }

            /// Nếu không còn đối tượng nào khả dụng
            if (spawns.Count <= 0)
            {
                /// Lấy đối tượng Model tương ứng
                Model model = this.Models.Where(x => x.Name == name).FirstOrDefault();
                /// Nếu Model không tồn tại
                if (model == null)
                {
                    KTDebug.LogError(string.Format("No model named '{0}' inside Pool!", name));
                    return null;
                }
                /// Nếu số lượng Spawn đã vượt quá giới hạn
                if (model.MaxSpawn != -1 && model.SpawnCount >= model.MaxSpawn)
                {
                    //KTDebug.LogError(string.Format("Pool is full, can not spawn anymore!"));
                    return null;
                }
                /// Tạo mới đối tượng
                Spawn spawn = this.MakeSpawn(model);
                /// Nếu Model có thể mở rộng
                if (model.Expandable)
                {
                    model.SpawnCount++;
                    this.listUnavailableSpawns.Add(spawn.Object, spawn);
                }

                /// Trả về thành phần tương ứng
                return spawn;
            }
            /// Nếu còn đối tượng khả dụng
            else
            {
                /// Lấy đối tượng spawn tương ứng
                Spawn spawn = this.listSpawns[name][0];
                /// Nếu đối tượng không tồn tại
                if (spawn == null)
                {
                    KTDebug.LogError(string.Format("No object named '{0}' inside Pool!", name));
                    return null;
                }
                /// Xóa đối tượng khỏi danh sách khả dụng
                this.listSpawns[name].Remove(spawn);
                /// Thêm vào danh sách không khả dụng
                this.listUnavailableSpawns.Add(spawn.Object, spawn);

                /// Trả về thành phần tương ứng
                return spawn;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về đối tượng tương ứng trong Pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Instantiate<T>(string name) where T : Component
        {
            /// Đối tượng Spawn tương ứng
            Spawn spawn = this.TakeSpawn(name);
            /// Nếu đối tượng không tồn tại
            if (spawn == null)
            {
                return null;
            }
            else
            {
                /// Kích hoạt đối tượng
                spawn.Object.gameObject.SetActive(true);

                /// Trả về thành phần T của đối tượng
                return spawn.Object.GetComponent<T>();
            }
        }

        /// <summary>
        /// Trả về đối tượng tương ứng trong Pool
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RectTransform Instantiate(string name)
        {
            /// Đối tượng Spawn tương ứng
            Spawn spawn = this.TakeSpawn(name);
            /// Nếu đối tượng không tồn tại
            if (spawn == null)
            {
                return null;
            }
            else
            {
                /// Kích hoạt đối tượng
                spawn.Object.gameObject.SetActive(true);
                /// Trả về đối tượng
                return spawn.Object;
            }
        }

        /// <summary>
        /// Trả đối tượng lại Pool
        /// <param name="go"></param>
        /// </summary>
        public void ReturnToPool(RectTransform go)
        {
            /// Nếu đối tượng có tồn tại trong danh sách không khả dụng của Pool
            if (this.listUnavailableSpawns.TryGetValue(go, out Spawn spawn))
            {
                /// Xóa đối tượng khỏi danh sách không khả dụng
                this.listUnavailableSpawns.Remove(go);
                /// Thêm đối tượng vào dan sách khả dụng để tái sử dụng
                this.listSpawns[spawn.Name].Add(spawn);

                /// Dịch đối tượng về vị trí mặc định
                spawn.Object.transform.localPosition = Vector3.zero;
                /// Hủy kích hoạt đối tượng
                spawn.Object.gameObject.SetActive(false);
                /// Thiết lập cha của đối tượng
                spawn.Object.transform.SetParent(this.transform, false);
            }
            /// Nếu không tồn tại trong Pool thì xóa đối tượng
            else
            {
                GameObject.Destroy(go.gameObject);
            }
        }
        #endregion
    }
}
