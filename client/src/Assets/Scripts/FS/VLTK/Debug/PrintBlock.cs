using FS.GameEngine.Logic;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Debug
{
    /// <summary>
    /// In vị trí Block bằng các ô vuông đỏ để Test
    /// </summary>
    public class PrintBlock : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng Sprite ô tương ứng
        /// </summary>
        [SerializeField]
        private Sprite _Sprite;

        /// <summary>
        /// Đối tượng Leader
        /// </summary>
        [SerializeField]
        private GameObject _Leader;

        /// <summary>
        /// Sử dụng vị trí cục bộ
        /// </summary>
        [SerializeField]
        private bool _UseLocalPosition = true;
        #endregion

        #region Constants
        /// <summary>
        /// Kích thước lưới
        /// </summary>
        private const int GridSize = 30;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách các ô màu tương ứng
        /// </summary>
        private GameObject[][] markObjects;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;

            /// Tạo mới Root
            GameObject debugRoot = new GameObject("DebugPosRoot");
            debugRoot.transform.SetParent(this._Leader.transform, false);
            debugRoot.transform.localPosition = Vector2.zero;

            /// Tạo danh sách các ô màu
            this.markObjects = new GameObject[PrintBlock.GridSize + 1][];
            for (int i = 0; i <= PrintBlock.GridSize; i++)
            {
                this.markObjects[i] = new GameObject[PrintBlock.GridSize + 1];
                for (int j = 0; j <= PrintBlock.GridSize; j++)
                {
                    /// Tạo mới object tương ứng
                    GameObject gameObject = new GameObject();
                    gameObject.transform.SetParent(debugRoot.transform, false);
                    gameObject.layer = 9;
                    gameObject.transform.localPosition = Vector2.zero;
                    gameObject.transform.localScale = Vector3.one;
                    //gameObject.layer = 9;
                    gameObject.AddComponent<SpriteRenderer>();
                    SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
                    renderer.sprite = this._Sprite;
                    renderer.drawMode = SpriteDrawMode.Sliced;
                    renderer.sortingLayerName = "2D Object";
                    renderer.sortingOrder = 10000;
                    gameObject.SetActive(false);

                    /// Lưu lại
                    this.markObjects[i][j] = gameObject;
                }
            }

            /// Chạy luồng đồng bộ
            this.StartCoroutine(this.SyncPos());
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Chạy luồng đồng bộ
            this.StartCoroutine(this.SyncPos());
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// đồng bộ vị trí của Leader
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncPos()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Nếu chưa vào Game
                if (Global.Data == null || Global.Data.GameScene == null || Global.Data.Leader == null)
                {
                    /// Bỏ qua
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    /// Vị trí hiện tại của Leader
                    Vector2 position;
                    /// Nếu sử dụng vị trí cục bộ
                    if (this._UseLocalPosition)
                    {
                        position = new Vector2(this._Leader.transform.localPosition.x / Global.CurrentMapData.GridSizeX, this._Leader.transform.localPosition.y / Global.CurrentMapData.GridSizeX);
                    }
                    /// Nếu sử dụng vị trí từ GS truyền về
                    else
                    {
                        position = new Vector2(Global.Data.Leader.PosX / Global.CurrentMapData.GridSizeX, Global.Data.Leader.PosY / Global.CurrentMapData.GridSizeY);
                    }

                    /// Danh sách OBS
                    byte[,] obs = Global.CurrentMapData.Obstructions;
                    /// Tầm nhìn
                    int range = PrintBlock.GridSize / 2;

                    /// Duyệt các ô trong tầm nhìn
                    for (int i = (int) position.y - range; i <= position.y + range; i++)
                    {
                        /// Vượt quá phạm vi
                        if (i < 0 || i >= obs.GetUpperBound(1))
                        {
                            /// Bỏ qua
                            continue;
                        }

                        for (int j = (int) position.x - range; j <= position.x + range; j++)
                        {
                            /// Vượt quá phạm vi
                            if (j < 0 || j >= obs.GetUpperBound(0))
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Nếu không phải ô chứa vật cản
                            if (obs[j, i] == 1)
                            {
                                /// Hủy ô màu tương ứng
                                this.markObjects[-(int) position.x + j + range][-(int) position.y + i + range].SetActive(false);
                                /// Bỏ qua
                                continue;
                            }

                            /// Tham chiếu tới ô tương ứng và kích hoạt
                            GameObject gameObject = this.markObjects[-(int) position.x + j + range][-(int) position.y + i + range];
                            gameObject.SetActive(true);
                            gameObject.transform.position = new Vector2((j) * Global.CurrentMapData.GridSizeX, (i) * Global.CurrentMapData.GridSizeY);
                            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
                            renderer.size = new Vector2(Global.CurrentMapData.GridSizeX, Global.CurrentMapData.GridSizeY);
                        }
                    }
                    /// Bỏ qua Frame
                    yield return null;
                }
            }
        }
        #endregion
    }
}
