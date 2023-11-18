using UnityEngine;

namespace SpriteGlow
{
    /// <summary>
    /// Adds an HDR outline over the <see cref="SpriteRenderer"/>'s sprite borders.
    /// Can be used in conjuction with bloom post-processing to create a glow effect.
    /// </summary>
    [AddComponentMenu("Effects/Sprite Glow")]
    [RequireComponent(typeof(SpriteRenderer)), DisallowMultipleComponent, ExecuteInEditMode]
    public class SpriteGlowEffect : MonoBehaviour
    {
        public SpriteRenderer Renderer { get; private set; }
        public Color GlowColor
        {
            get => glowColor;
            set { if (glowColor != value) { glowColor = value; SetMaterialProperties(); } }
        }
        public float GlowBrightness
        {
            get => glowBrightness;
            set { if (glowBrightness != value) { glowBrightness = value; SetMaterialProperties(); } }
        }
        public float AlphaThreshold
        {
            get => alphaThreshold;
            set { if (alphaThreshold != value) { alphaThreshold = value; SetMaterialProperties(); } }
        }
        public bool EnableInstancing
        {
            get => enableInstancing;
            set { if (enableInstancing != value) { enableInstancing = value; SetMaterialProperties(); } }
        }

        [Tooltip("Base color of the glow.")]
        [SerializeField] private Color glowColor = Color.white;
        [Tooltip("The brightness (power) of the glow."), Range(1, 1000)]
        [SerializeField] private float glowBrightness = 2f;
        [Tooltip("Threshold to determine sprite borders."), Range(0f, 1f)]
        [SerializeField] private float alphaThreshold = .01f;
        [Tooltip("Whether to enable GPU instancing.")]
        [SerializeField] private bool enableInstancing = false;

        private static readonly int isOutlineEnabledId = Shader.PropertyToID("_IsOutlineEnabled");
        private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int outlineSizeId = Shader.PropertyToID("_OutlineSize");
        private static readonly int alphaThresholdId = Shader.PropertyToID("_AlphaThreshold");

        private MaterialPropertyBlock materialProperties;

        private void Awake()
        {
            this.Renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            SetMaterialProperties();
        }

        private void OnDisable()
        {
            SetMaterialProperties();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            // Update material properties when changing serialized fields with editor GUI.
            SetMaterialProperties();
        }

        public void Apply()
        {
            // Update material properties when changing serialized fields with Unity animation.
            SetMaterialProperties();
        }

        private void SetMaterialProperties()
        {
            if (!this.Renderer)
			{
                return;
            }
            /// Steven Huang avoid NullReferenceException
            if (this.Renderer == null)
            {
                return;
            }

            if (this.Renderer.sprite != null)
			{
                this.Renderer.sharedMaterial = SpriteGlowMaterial.GetSharedFor(this);
            }

            if (this.materialProperties == null)
			{
                this.materialProperties = new MaterialPropertyBlock();
            }


            this.materialProperties.SetFloat(isOutlineEnabledId, isActiveAndEnabled ? 1 : 0);
            this.materialProperties.SetColor(outlineColorId, GlowColor * GlowBrightness);
            this.materialProperties.SetFloat(alphaThresholdId, AlphaThreshold);

            this.Renderer.SetPropertyBlock(this.materialProperties);
        }
    }
}
