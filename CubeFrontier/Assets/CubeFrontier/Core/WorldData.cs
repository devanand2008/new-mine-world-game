using System;
using System.Collections.Generic;

namespace CubeFrontier.Core
{
    public sealed class Terrain
    {
        public const int Height = 64, SeaLevel = 19;
        public readonly int Seed;
        public readonly int GeneratorVersion;
        public Terrain(int seed, int generatorVersion = 2) { if(generatorVersion<1||generatorVersion>2)throw new ArgumentException("Unsupported terrain generator");Seed = seed; GeneratorVersion=generatorVersion; }
        public Biome BiomeAt(int x, int z)
        {
            double v = Noise.Sample(x / 170.0, z / 170.0, Seed ^ 937);
            if(GeneratorVersion==1)return v<.30?Biome.CrystalMarsh:v<.57?Biome.EvergreenWilds:v<.76?Biome.AshenMountains:Biome.EclipseWastes;
            double t = Noise.Sample(x / 200.0, z / 200.0, Seed ^ 1117);
            if (v < .15) return Biome.SwampWetlands;
            if (v < .30) return Biome.CrystalMarsh;
            if (v < .42) return Biome.EvergreenWilds;
            if (v < .52) return t < .5 ? Biome.AncientRuins : Biome.HauntedForest;
            if (v < .65) return Biome.AshenMountains;
            if (v < .76) return t < .4 ? Biome.FrozenHighlands : Biome.DesertDunes;
            return Biome.EclipseWastes;
        }
        public int Surface(int x, int z)
        {
            double broad = Noise.Sample(x / 90.0, z / 90.0, Seed);
            double detail = Noise.Sample(x / 23.0, z / 23.0, Seed ^ 17);
            double river = Math.Abs(Noise.Sample(x / 120.0, z / 120.0, Seed ^ 311) - .5);
            int h = 17 + (int)(broad * 17 + detail * 5);
            Biome biome = BiomeAt(x, z);
            switch (biome) {
                case Biome.AshenMountains: case Biome.FrozenHighlands: h += (int)(detail * 10); break;
                case Biome.DesertDunes: h += (int)(Noise.Sample(x / 30.0, z / 30.0, Seed ^ 555) * 6); break;
                case Biome.SwampWetlands: h = Math.Min(h, SeaLevel + 3); break;
                case Biome.DeepCaverns: h = Math.Max(10, h - 8); break;
            }
            if (river < .04) h -= (int)((.04 - river) * 210);
            return Math.Max(9, Math.Min(48, h));
        }
        public byte[] Generate(ChunkKey key)
        {
            byte[] data = new byte[16 * Height * 16];
            for (int z = 0; z < 16; z++) for (int x = 0; x < 16; x++) {
                int wx = key.X * 16 + x, wz = key.Z * 16 + z;
                int top = Surface(wx, wz); Biome biome = BiomeAt(wx, wz);
                for (int y = 0; y < Height; y++) {
                    Block block = Block.Air;
                    if (y == 0) block = Block.Bedrock;
                    else if (y <= top) {
                        block = y < top - 3 ? Block.Stone : Block.Soil;
                        if (y == top) block = SurfaceBlock(biome, top);
                        // Underground ore and cave generation
                        if (y < top - 4 && y > 3) {
                            double cave = Noise.Sample(wx / 16.0 + y * .08, wz / 16.0 - y * .07, Seed ^ 881);
                            if (cave > .72 && y < 25) block = Block.Air;
                            else {
                                uint oreHash = Noise.Hash(wx, y, wz, Seed);
                                if (oreHash % 89 == 0) block = Block.Ore;
                                else if (Noise.Hash(wx, y, wz, Seed ^ 83) % 191 == 0) block = Block.Crystal;
                                else if (GeneratorVersion>=2 && y < 15 && Noise.Hash(wx, y, wz, Seed ^ 441) % 247 == 0) block = Block.ShadowOre;
                                else if (GeneratorVersion>=2 && y < 12 && Noise.Hash(wx, y, wz, Seed ^ 593) % 311 == 0) block = Block.EmberCrystal;
                                else if (GeneratorVersion>=2 && y < 10 && Noise.Hash(wx, y, wz, Seed ^ 719) % 503 == 0) block = Block.Moonstone;
                            }
                            // Lava pools deep underground
                            if (GeneratorVersion>=2 && y < 8 && cave > .68 && cave < .72) block = Block.Lava;
                        }
                        // Biome-specific underground
                        if (biome == Biome.SwampWetlands && y == top - 1 && top > SeaLevel) block = Block.Clay;
                        if (biome == Biome.AncientRuins && y < top - 5 && y > 6 && Noise.Hash(wx, y, wz, Seed ^ 997) % 397 == 0) block = Block.AncientRelic;
                    } else if (y <= SeaLevel) block = Block.Water;
                    data[Index(x, y, z)] = (byte)block;
                }
            }
            // Tree and vegetation generation using world-space grid anchors
            for (int gz = key.Z * 2 - 1; gz <= key.Z * 2 + 2; gz++) for (int gx = key.X * 2 - 1; gx <= key.X * 2 + 2; gx++) {
                uint hash = Noise.Hash(gx, 77, gz, Seed);
                int ax = gx * 8 + (int)(hash % 5) + 1, az = gz * 8 + (int)((hash >> 5) % 5) + 1;
                int top = Surface(ax, az); Biome biome = BiomeAt(ax, az);
                if (top <= SeaLevel + 1 || hash % 3 == 0) continue;
                switch (biome) {
                    case Biome.EvergreenWilds:
                    case Biome.HauntedForest:
                        int treeH = biome == Biome.HauntedForest ? 7 : 5;
                        for (int y = top + 1; y <= top + treeH; y++) SetLocal(data, key, ax, y, az, Block.Wood);
                        for (int dz = -2; dz <= 2; dz++) for (int dx = -2; dx <= 2; dx++) for (int dy = 3; dy <= treeH + 1; dy++)
                            if (Math.Abs(dx) + Math.Abs(dz) + Math.Max(0, dy - 4) < 5 && (dx != 0 || dz != 0 || dy == treeH + 1))
                                SetLocal(data, key, ax + dx, top + dy, az + dz, Block.Leaves);
                        if (biome == Biome.HauntedForest && hash % 7 == 0) {
                            // Moss and roots on haunted trees
                            for (int dy = 1; dy <= 3; dy++) SetLocal(data, key, ax + 1, top + dy, az, Block.Moss);
                            SetLocal(data, key, ax - 1, top, az, Block.Roots);
                            SetLocal(data, key, ax, top, az + 1, Block.Roots);
                        }
                        break;
                    case Biome.CrystalMarsh:
                        for (int y = top + 1; y <= top + 2 + hash % 3; y++) SetLocal(data, key, ax, y, az, Block.Crystal);
                        break;
                    case Biome.SwampWetlands:
                        // Swamp trees with roots and mushrooms
                        for (int y = top + 1; y <= top + 4; y++) SetLocal(data, key, ax, y, az, Block.Wood);
                        for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                            SetLocal(data, key, ax + dx, top + 4, az + dz, Block.Leaves);
                        SetLocal(data, key, ax + 1, top, az, Block.Roots);
                        SetLocal(data, key, ax - 1, top, az, Block.Roots);
                        if (hash % 5 == 0) SetLocal(data, key, ax + 2, top + 1, az, Block.Mushroom);
                        break;
                    case Biome.DesertDunes:
                        // Cactus-like crystal columns
                        if (hash % 5 < 2)
                            for (int y = top + 1; y <= top + 2 + hash % 2; y++) SetLocal(data, key, ax, y, az, Block.Crystal);
                        break;
                    case Biome.FrozenHighlands:
                        // Sparse snowy pines
                        if (hash % 4 == 0) {
                            for (int y = top + 1; y <= top + 4; y++) SetLocal(data, key, ax, y, az, Block.Wood);
                            for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                                if (Math.Abs(dx) + Math.Abs(dz) < 2)
                                    for (int dy = 3; dy <= 5; dy++) SetLocal(data, key, ax + dx, top + dy, az + dz, Block.Snow);
                        }
                        break;
                    case Biome.AncientRuins:
                        // Ruined wall segments
                        if (hash % 7 == 0) {
                            int wallH = 2 + (int)(hash % 3);
                            for (int y = top + 1; y <= top + wallH; y++) {
                                SetLocal(data, key, ax, y, az, Block.StoneBricks);
                                if (hash % 2 == 0) SetLocal(data, key, ax + 1, y, az, Block.MossyStone);
                            }
                            if (hash % 11 == 0) SetLocal(data, key, ax, top + wallH + 1, az, Block.AncientRelic);
                        }
                        break;
                    case Biome.EclipseWastes:
                        if(GeneratorVersion==1)break;
                        // Shadow ore outcrops
                        if (hash % 5 == 0) {
                            for (int y = top + 1; y <= top + 1 + hash % 2; y++) SetLocal(data, key, ax, y, az, Block.Basalt);
                            if (hash % 9 == 0) SetLocal(data, key, ax, top + 2, az, Block.ShadowOre);
                        }
                        break;
                }
            }
            return data;
        }
        private static Block SurfaceBlock(Biome biome, int top)
        {
            if (top <= SeaLevel + 1) return Block.Sand;
            switch (biome) {
                case Biome.AshenMountains: return Block.Snow;
                case Biome.FrozenHighlands: return Block.Snow;
                case Biome.EclipseWastes: return Block.Basalt;
                case Biome.DesertDunes: return Block.Sand;
                case Biome.SwampWetlands: return Block.Moss;
                case Biome.CrystalMarsh: return Block.Grass;
                case Biome.AncientRuins: return Block.Stone;
                case Biome.HauntedForest: return Block.Grass;
                default: return Block.Grass;
            }
        }
        private static void SetLocal(byte[] data, ChunkKey key, int x, int y, int z, Block block)
        {
            x -= key.X * 16; z -= key.Z * 16;
            if (x >= 0 && x < 16 && z >= 0 && z < 16 && y > 0 && y < Height) data[Index(x, y, z)] = (byte)block;
        }
        public static int Index(int x, int y, int z) { return x + 16 * (z + 16 * y); }
    }

    public sealed class WorldData
    {
        public const int Limit = 100000;
        public readonly Terrain Terrain;
        private readonly Dictionary<ChunkKey, byte[]> chunks = new Dictionary<ChunkKey, byte[]>();
        private readonly Dictionary<Cell, Block> edits = new Dictionary<Cell, Block>();
        public IEnumerable<KeyValuePair<Cell, Block>> Edits { get { return edits; } }
        public int EditCount { get { return edits.Count; } }
        public int CachedChunks { get { return chunks.Count; } }
        public WorldData(int seed,int generatorVersion=2) { Terrain = new Terrain(seed,generatorVersion); }
        public byte[] Load(ChunkKey key)
        {
            byte[] data;
            if (!chunks.TryGetValue(key, out data)) { data = Terrain.Generate(key); chunks.Add(key, data); }
            return data;
        }
        public Block Get(int x, int y, int z)
        {
            if (y < 0) return Block.Bedrock;
            if (y >= Terrain.Height) return Block.Air;
            if (Math.Abs((long)x) > Limit || Math.Abs((long)z) > Limit) return Block.Bedrock;
            Block changed;
            if (edits.TryGetValue(new Cell(x, y, z), out changed)) return changed;
            ChunkKey key = ChunkKey.At(x, z);
            return (Block)Load(key)[Terrain.Index(x - key.X * 16, y, z - key.Z * 16)];
        }
        public bool Set(Cell cell, Block block)
        {
            if (cell.Y <= 0 || cell.Y >= Terrain.Height || Math.Abs((long)cell.X) > Limit || Math.Abs((long)cell.Z) > Limit || (int)block >= Blocks.Count || block == Block.Bedrock) return false;
            if (Get(cell.X, cell.Y, cell.Z) == block) return false;
            ChunkKey key = ChunkKey.At(cell.X, cell.Z);
            Block original = (Block)Load(key)[Terrain.Index(cell.X - key.X * 16, cell.Y, cell.Z - key.Z * 16)];
            if (block == original) edits.Remove(cell); else edits[cell] = block;
            return true;
        }
        public void EvictOutside(ChunkKey center, int radius)
        {
            var stale = new List<ChunkKey>();
            foreach (var key in chunks.Keys) if (Math.Abs(key.X - center.X) > radius || Math.Abs(key.Z - center.Z) > radius) stale.Add(key);
            foreach (var key in stale) chunks.Remove(key);
        }
    }

    public struct VoxelFace
    {
        public int X, Y, Z, Direction;
        public Block Block;
        public VoxelFace(int x, int y, int z, int direction, Block block) { X = x; Y = y; Z = z; Direction = direction; Block = block; }
    }
    public static class Mesher
    {
        public static readonly int[,] Directions = { { 1, 0, 0 }, { -1, 0, 0 }, { 0, 1, 0 }, { 0, -1, 0 }, { 0, 0, 1 }, { 0, 0, -1 } };
        public static List<VoxelFace> Faces(Func<int, int, int, Block> get, int width, int height, int depth)
        {
            var faces = new List<VoxelFace>();
            for (int y = 0; y < height; y++) for (int z = 0; z < depth; z++) for (int x = 0; x < width; x++) {
                Block b = get(x, y, z); if (b == Block.Air) continue;
                for (int d = 0; d < 6; d++) {
                    Block neighbor = get(x + Directions[d, 0], y + Directions[d, 1], z + Directions[d, 2]);
                    if (neighbor == Block.Air || (Blocks.Transparent(neighbor) && neighbor != b && (!Blocks.Transparent(b) || (int)b > (int)neighbor))) faces.Add(new VoxelFace(x, y, z, d, b));
                }
            }
            return faces;
        }
    }
}
