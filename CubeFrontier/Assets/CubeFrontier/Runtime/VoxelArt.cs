using System.Collections.Generic;
using UnityEngine;

namespace CubeFrontier
{
    public static class VoxelArt
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();
        public static Material Material(Color color, bool glow = false)
        {
            Color key = color; key.a = glow ? .5f : 1;
            Material result;
            if (Materials.TryGetValue(key, out result) && result) return result;
            result = new Material(Shader.Find("Universal Render Pipeline/Lit")); result.color = color; result.enableInstancing = true;
            if (glow) { result.EnableKeyword("_EMISSION"); result.SetColor("_EmissionColor", color * 2); }
            Materials[key] = result; return result;
        }
        public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool glow = false)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.name = name; cube.layer = parent.gameObject.layer;
            // Keep the concrete primitive collider in stripped Web builds.
            var collider = cube.GetComponent<BoxCollider>(); collider.enabled = false; Object.Destroy(collider);
            cube.transform.SetParent(parent, false); cube.transform.localPosition = position; cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = Material(color, glow); return cube;
        }
        public static GameObject Explorer(Transform parent) { return ReferenceCharacters.Create(parent, ReferenceCharacters.Selected); }
        public static Transform Monster(Transform parent, CreatureKind kind) { return ReferenceCreatures.Create(parent, (int)kind).transform; }
    }
}
