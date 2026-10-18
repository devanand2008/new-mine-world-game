using System.Collections;
using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;
using Terrain = CubeFrontier.Core.Terrain;

namespace CubeFrontier
{
    public sealed class VoxelWorld : MonoBehaviour
    {
        public WorldData Data { get; private set; }
        public Transform Target;
        public int RenderDistance = 3;
        public Material Material;
        public int ActiveChunks { get { return views.Count; } }
        public bool Ready { get { return Target && views.ContainsKey(ChunkKey.At(Mathf.FloorToInt(Target.position.x), Mathf.FloorToInt(Target.position.z))); } }
        private readonly Dictionary<ChunkKey, ChunkView> views = new Dictionary<ChunkKey, ChunkView>();
        private readonly HashSet<ChunkKey> dirty = new HashSet<ChunkKey>();
        public void Initialize(WorldData data) { Data = data; StartCoroutine(Stream()); }
        private IEnumerator Stream()
        {
            while (true) {
                if (!Target || Data == null) { yield return null; continue; }
                ChunkKey center = ChunkKey.At(Mathf.FloorToInt(Target.position.x), Mathf.FloorToInt(Target.position.z));
                var stale = new List<ChunkKey>();
                foreach (var key in views.Keys) if (Distance(key, center) > RenderDistance) stale.Add(key);
                foreach (var key in stale) { Destroy(views[key].gameObject); views.Remove(key); dirty.Remove(key); }
                dirty.RemoveWhere(key => Distance(key, center) > RenderDistance);
                ChunkKey chosen = center; bool found = false;
                foreach (var key in dirty) if (views.ContainsKey(key)) { chosen = key; found = true; break; }
                if (!found) for (int radius = 0; radius <= RenderDistance && !found; radius++)
                    for (int z = -radius; z <= radius && !found; z++) for (int x = -radius; x <= radius && !found; x++) {
                        var key = new ChunkKey(center.X + x, center.Z + z);
                        if (!views.ContainsKey(key)) { chosen = key; found = true; }
                    }
                if (found) {
                    dirty.Remove(chosen);
                    // Generation is bounded to one chunk, and mesh extraction yields every 8 vertical cells.
                    Data.Load(chosen); yield return null;
                    var faces = new List<VoxelFace>();
                    for (int slice = 0; slice < Terrain.Height; slice += 8) {
                        int offset = slice;
                        var band = Mesher.Faces((x, y, z) => Data.Get(chosen.X * 16 + x, offset + y, chosen.Z * 16 + z), 16, 8, 16);
                        foreach (var f in band) faces.Add(new VoxelFace(f.X, f.Y + offset, f.Z, f.Direction, f.Block));
                        yield return null;
                    }
                    var current = ChunkKey.At(Mathf.FloorToInt(Target.position.x), Mathf.FloorToInt(Target.position.z));
                    if (Distance(chosen, current) <= RenderDistance) {
                        ChunkView view;
                        if (!views.TryGetValue(chosen, out view)) {
                            var obj = new GameObject("Chunk " + chosen.X + ", " + chosen.Z); obj.transform.SetParent(transform);
                            obj.transform.position = new Vector3(chosen.X * 16, 0, chosen.Z * 16); view = obj.AddComponent<ChunkView>(); views.Add(chosen, view);
                        }
                        view.Apply(faces, Material, Data);
                    }
                    Data.EvictOutside(current, RenderDistance + 1);
                }
                yield return null;
            }
        }
        public bool HasColliderAt(Vector3 position) { return views.ContainsKey(ChunkKey.At(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z))); }
        public bool Change(Cell cell, Block block)
        {
            if (!Data.Set(cell, block)) return false;
            dirty.Add(ChunkKey.At(cell.X, cell.Z));
            dirty.Add(ChunkKey.At(cell.X + 1, cell.Z)); dirty.Add(ChunkKey.At(cell.X - 1, cell.Z));
            dirty.Add(ChunkKey.At(cell.X, cell.Z + 1)); dirty.Add(ChunkKey.At(cell.X, cell.Z - 1)); return true;
        }
        public Vector3 FindSpawn(int x = 0, int z = 0)
        {
            for (int radius = 0; radius < 64; radius += 2) for (int dx = -radius; dx <= radius; dx += 2) {
                int sx = x + dx, sz = z + radius; int surface = Data.Terrain.Surface(sx, sz);
                if (surface <= Terrain.SeaLevel + 1) continue;
                for (int y = surface + 1; y < Terrain.Height - 3; y++)
                    if (Blocks.Solid(Data.Get(sx, y - 1, sz)) && Data.Get(sx, y, sz) == Block.Air && Data.Get(sx, y + 1, sz) == Block.Air && Data.Get(sx, y + 2, sz) == Block.Air)
                        return new Vector3(sx + .5f, y + .1f, sz + .5f);
            }
            return new Vector3(x + .5f, Terrain.Height + 2, z + .5f);
        }
        private static int Distance(ChunkKey a, ChunkKey b) { return Mathf.Max(Mathf.Abs(a.X - b.X), Mathf.Abs(a.Z - b.Z)); }
    }
}
