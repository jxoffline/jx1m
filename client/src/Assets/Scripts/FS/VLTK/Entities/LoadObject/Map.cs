namespace FS.VLTK.Entities.LoadObject
{
    [UnityEngine.ExecuteAlways]
    public class Map : TTMonoBehaviour
    {
        public UnityEngine.Sprite Minimap;
        public UnityEngine.Vector2 MinimapSize
        {
            get
            {
                return this.Minimap.rect.size;
            }
        }

#if UNITY_EDITOR
        public UnityEngine.Vector2 _Size;
        private void Update()
        {
            if (this.Minimap != null)
            {
                this._Size = this.MinimapSize;
            }
        }
#endif
    }
}
