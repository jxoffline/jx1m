using System.Collections.Generic;
using UnityEngine;

namespace SpriteGlow
{
    public class SpriteGlowMaterial : Material
    {
        public Texture SpriteTexture => mainTexture;
        public bool InstancingEnabled => enableInstancing;

        private const string outlineShaderName = "Sprites/Outline";

        private static readonly Shader outlineShader = Shader.Find(outlineShaderName);
        private static readonly List<SpriteGlowMaterial> sharedMaterials = new List<SpriteGlowMaterial>();

        public SpriteGlowMaterial(Texture spriteTexture, bool instancingEnabled = false)
            : base(outlineShader)
        {
            if (!outlineShader)
                Debug.LogError($"`{outlineShaderName}` shader not found. Make sure the shader is included to the build.");

            mainTexture = spriteTexture;
            if (instancingEnabled)
                enableInstancing = true;
        }

        public static Material GetSharedFor(SpriteGlowEffect spriteGlow)
        {
            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                if (sharedMaterials[i].SpriteTexture == spriteGlow.Renderer.sprite.texture &&
                    sharedMaterials[i].InstancingEnabled == spriteGlow.EnableInstancing)
                    return sharedMaterials[i];
            }

            var material = new SpriteGlowMaterial(spriteGlow.Renderer.sprite.texture, spriteGlow.EnableInstancing);
            material.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;
            sharedMaterials.Add(material);

            return material;
        }
    }
}
