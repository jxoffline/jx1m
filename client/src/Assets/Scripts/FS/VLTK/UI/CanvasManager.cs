using FS.VLTK.UI.Main.MessageBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.CoreUI;
using FS.VLTK.UI.Main.SytemNotification;
using FS.VLTK.UI.Main;
using FS.VLTK.Factory;

namespace FS.VLTK.UI
{
    /// <summary>
    /// Đối tượng Canvas
    /// </summary>
    public class CanvasManager : TTMonoBehaviour
    {
        #region Singleton Instance
        /// <summary>
        /// Đối tượng Canvas
        /// </summary>
        public static CanvasManager Instance { get; private set; }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            CanvasManager.Instance = this;
        }
        #endregion

        #region Define
        /// <summary>
        /// Gốc chứa UI tĩnh
        /// </summary>
        [SerializeField]
        private GameObject StaticUIRoot;

        /// <summary>
        /// Gốc chứa UI động
        /// </summary>
        [SerializeField]
        private GameObject DynamicUIRoot;

        /// <summary>
        /// Gốc chứa UI dưới cùng
        /// </summary>
        [SerializeField]
        private GameObject UnderLayUIRoot;

        /// <summary>
        /// Gốc chứa các UI trong màn hình Game
        /// </summary>
        [SerializeField]
        private GameObject MainUIRoot;

        /// <summary>
        /// Gốc chứa các UI ưu tiên hiển thị ở đầu
        /// </summary>
        [SerializeField]
        private GameObject OnTopUIRoot;

        /// <summary>
        /// Khung Tooltip góc trên chính giữa màn hình
        /// </summary>
        public UINotificationTip UINotificationTip;

        /// <summary>
        /// Khung chữ chạy hệ thống
        /// </summary>
        public UISystemNotification UISystemNotification;

        /// <summary>
        /// Khung tải dữ liệu gì đó
        /// </summary>
        public UILoadingProgress UILoadingProgress;
        #endregion

        #region Properties
        /// <summary>
        /// Bảng thông báo
        /// </summary>
        public UIMessageBox UIMessageBox { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Tải UI từ Prefab tương ứng
        /// </summary>
        /// <param name="name"></param>
        public T LoadUIPrefab<T>(string name) where T : MonoBehaviour
        {
            T prefab = Resources.Load<T>("VLTK/Prefabs/UINew/" + name);
            T obj = GameObject.Instantiate<T>(prefab);
            return obj;
        }

        /// <summary>
        /// Thêm UI vào Canvas
        /// </summary>
        /// <param name="ui"></param>
        public void AddUI(MonoBehaviour ui)
        {
            ui.gameObject.transform.SetParent(this.DynamicUIRoot.transform, false);
        }

        /// <summary>
        /// Thêm UI vào Canvas
        /// </summary>
        /// <param name="ui"></param>
        public void AddUI(RectTransform ui)
        {
            ui.gameObject.transform.SetParent(this.DynamicUIRoot.transform, false);
        }

        /// <summary>
        /// Thêm UI vào MainUI trong Canvas
        /// </summary>
        /// <param name="ui"></param>
        public void AddMainUI(MonoBehaviour ui)
        {
            ui.gameObject.transform.SetParent(this.MainUIRoot.transform, false);
        }

        /// <summary>
        /// Thêm UI vào Canvas
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="isOnTop"></param>
        public void AddUI(MonoBehaviour ui, bool isOnTop)
        {
            if (!isOnTop)
            {
                ui.gameObject.transform.SetParent(this.DynamicUIRoot.transform, false);
            }
            else
            {
                ui.gameObject.transform.SetParent(this.OnTopUIRoot.transform, false);
            }
        }

        /// <summary>
        /// Thêm vào UI cố định
        /// </summary>
        /// <param name="ui"></param>
        public void AddStaticUI(MonoBehaviour ui)
        {
            ui.gameObject.transform.SetParent(this.StaticUIRoot.transform, false);
        }

        /// <summary>
        /// Thêm UI vào UnderLayer của Canvas
        /// </summary>
        /// <param name="ui"></param>
        public void AddUnderLayerUI(MonoBehaviour ui)
        {
            ui.gameObject.transform.SetParent(this.UnderLayUIRoot.transform, false);
        }

        /// <summary>
        /// Xóa Main UI
        /// </summary>
        public void DestroyMainUI()
        {
            foreach (Transform childUI in this.MainUIRoot.transform)
            {
                KTUIElementPoolManager.Instance.ReturnToPool(childUI.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// Xóa On Top UI
        /// </summary>
        public void DestroyOnTopUI()
        {
            foreach (Transform childUI in this.OnTopUIRoot.transform)
            {
                KTUIElementPoolManager.Instance.ReturnToPool(childUI.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// Xóa Under Layer UI
        /// </summary>
        public void DestroyUnderLayerUI()
        {
            foreach (Transform childUI in this.UnderLayUIRoot.transform)
            {
                KTUIElementPoolManager.Instance.ReturnToPool(childUI.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// Xóa Dynamic UI
        /// </summary>
        public void DestroyDynamicUI()
        {
            this.DestroyMainUI();
            foreach (Transform childUI in this.DynamicUIRoot.transform)
            {
                if (childUI.gameObject != this.MainUIRoot.gameObject)
                {
                    KTUIElementPoolManager.Instance.ReturnToPool(childUI.GetComponent<RectTransform>());
                }
            }
        }

        /// <summary>
        /// Xóa UI khỏi Canvas
        /// </summary>
        /// <param name="ui"></param>
        public void RemoveUI(MonoBehaviour ui)
        {
            if (ui.gameObject)
            {
                KTUIElementPoolManager.Instance.ReturnToPool(ui.GetComponent<RectTransform>());
            }
        }
        #endregion
    }
}
