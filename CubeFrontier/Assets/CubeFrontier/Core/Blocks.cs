using System;
using System.Collections.Generic;

namespace CubeFrontier.Core
{
    // Persisted IDs: append only, so existing worlds retain their original blocks.
    public enum Block : byte { Air, Grass, Soil, Stone, Sand, Water, Snow, Ore, Wood, Leaves, Crystal, Basalt, Beacon, Bedrock,
        OakPlanks, SprucePlanks, BirchPlanks, Cobblestone, StoneBricks, MossyStone, ClayBricks, Sandstone, SmoothStone,
        WhitePlaster, SlateTiles, RedRoof, Glass, DarkGlass, Copper, Iron, Gold, Lantern, Bookshelf, Crate,
        RedWool, BlueWool, WhiteWool, GreenWool, Terracotta, PolishedBasalt, Marble, Path, Gravel, PackedIce, DarkTimber, CarvedStone,
        /* 0.4 expansion blocks – append only */
        Clay, Moss, Mushroom, Moonstone, EmberCrystal, ShadowOre, AncientRelic, SpectralBlock, Roots, Obsidian, Lava, Torch,
        StorageChest, Furnace, Bed }
    public enum GameMode : byte { Survival, Creative, Horror }
    public enum Biome { EvergreenWilds, AshenMountains, CrystalMarsh, EclipseWastes,
        /* 0.4 expanded biomes */
        DesertDunes, FrozenHighlands, SwampWetlands, DeepCaverns, AncientRuins, HauntedForest }

    public struct Cell : IEquatable<Cell>
    {
        public readonly int X, Y, Z;
        public Cell(int x, int y, int z) { X = x; Y = y; Z = z; }
        public bool Equals(Cell b) { return X == b.X && Y == b.Y && Z == b.Z; }
        public override bool Equals(object o) { return o is Cell && Equals((Cell)o); }
        public override int GetHashCode() { unchecked { return X * 73856093 ^ Y * 19349663 ^ Z * 83492791; } }
    }

    public struct ChunkKey : IEquatable<ChunkKey>
    {
        public readonly int X, Z;
        public ChunkKey(int x, int z) { X = x; Z = z; }
        public bool Equals(ChunkKey b) { return X == b.X && Z == b.Z; }
        public override bool Equals(object o) { return o is ChunkKey && Equals((ChunkKey)o); }
        public override int GetHashCode() { unchecked { return X * 73856093 ^ Z * 19349663; } }
        public static int FloorDiv(int n, int d) { return (int)Math.Floor((double)n / d); }
        public static ChunkKey At(int x, int z) { return new ChunkKey(FloorDiv(x, 16), FloorDiv(z, 16)); }
    }

    public static class Blocks
    {
        public const int Count = 61, PlaceableCount = 57, AtlasColumns = 8, AtlasRows = 16, AtlasTileSize = 32;
        public static bool Transparent(Block b) { return b == Block.Water || b == Block.Glass || b == Block.DarkGlass || b == Block.PackedIce || b == Block.Lava; }
        public static bool Occludes(Block b) { return Solid(b) && !Transparent(b); }
        public static bool Emits(Block b) { return b == Block.Crystal || b == Block.Beacon || b == Block.Lantern || b == Block.Moonstone || b == Block.EmberCrystal || b == Block.SpectralBlock || b == Block.Torch || b == Block.Lava; }
        public static int Tile(Block b, int face)
        {
            if (b == Block.Grass) return face == 2 ? 1 : face == 3 ? 2 : 46;
            if (b == Block.Wood) return face == 2 || face == 3 ? 47 : 8;
            if (b == Block.Bookshelf && (face == 2 || face == 3)) return (int)Block.OakPlanks;
            if (b == Block.DarkTimber && (face == 2 || face == 3)) return 48;
            // Expansion blocks map to atlas tiles beyond the special-use range
            if (b == Block.Clay) return 58;
            if (b == Block.Moss) return 59;
            if (b == Block.Mushroom) return 60;
            if (b == Block.Moonstone) return 61;
            if (b == Block.EmberCrystal) return 62;
            if (b == Block.ShadowOre) return 63;
            if (b == Block.AncientRelic) return 64;
            if (b == Block.SpectralBlock) return 65;
            if (b == Block.Roots) return 66;
            if (b == Block.Obsidian) return 67;
            if (b == Block.Lava) return 68;
            if (b == Block.Torch) return 69;
            if (b == Block.StorageChest) return face == 2 || face == 3 ? 71 : 70;
            if (b == Block.Furnace) return face == 2 || face == 3 ? 17 : 72;
            if (b == Block.Bed) return face == 2 ? 73 : face == 3 ? 14 : 74;
            return (int)b;
        }
        public static string Name(Block b)
        {
            string value = b.ToString();
            return System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
        }
        public static string Category(Block b)
        {
            int id=(int)b;
            if (id<14 || b==Block.Path || b==Block.Gravel || b==Block.PackedIce || b==Block.Clay || b==Block.Moss || b==Block.Mushroom || b==Block.Roots || b==Block.Lava) return "Nature";
            if (b==Block.Moonstone || b==Block.EmberCrystal || b==Block.ShadowOre || b==Block.AncientRelic || b==Block.SpectralBlock) return "Rare";
            if (b==Block.Glass || b==Block.DarkGlass || b==Block.Lantern || b==Block.Bookshelf || b==Block.Crate || b==Block.Torch || b==Block.Obsidian || b==Block.StorageChest || b==Block.Furnace || b==Block.Bed || (id>=34&&id<=37)) return "Decor";
            return "Building";
        }
        public static bool Solid(Block b) { return b != Block.Air && b != Block.Water && b != Block.Lava; }
        public static bool Breakable(Block b) { return Solid(b) && b != Block.Bedrock; }
        public static bool Placeable(int id) { return id > 0 && id < Count && id != (int)Block.Water && id != (int)Block.Bedrock && id != (int)Block.Lava; }
        public static float Hardness(Block b)
        {
            switch (b) {
                case Block.Leaves: case Block.Mushroom: return .2f;
                case Block.Glass: case Block.DarkGlass: return .35f;
                case Block.Soil: case Block.Sand: case Block.Snow: case Block.Grass: case Block.Clay: case Block.Moss: return .55f;
                case Block.Wood: case Block.OakPlanks: case Block.BirchPlanks: case Block.SprucePlanks: case Block.DarkTimber: case Block.Bookshelf: case Block.Crate: case Block.Roots: case Block.Torch: case Block.StorageChest: case Block.Bed: return 1.2f;
                case Block.Ore: case Block.Crystal: case Block.ShadowOre: return 2.8f;
                case Block.Basalt: case Block.Beacon: case Block.Obsidian: return 3.5f;
                case Block.Moonstone: case Block.EmberCrystal: case Block.AncientRelic: case Block.SpectralBlock: return 4.0f;
                default: return 1.8f;
            }
        }
        public static bool Liquid(Block b) { return b == Block.Water || b == Block.Lava; }
    }

    public static class Noise
    {
        public static uint Hash(int x, int y, int z, int seed)
        {
            unchecked {
                uint h = (uint)seed ^ (uint)x * 374761393u ^ (uint)y * 668265263u ^ (uint)z * 2246822519u;
                h = (h ^ (h >> 13)) * 1274126177u;
                return h ^ (h >> 16);
            }
        }
        private static double Value(int x, int z, int seed) { return Hash(x, 0, z, seed) / (double)uint.MaxValue; }
        private static double Lerp(double a, double b, double t) { return a + (b - a) * t; }
        public static double Sample(double x, double z, int seed)
        {
            int ix = (int)Math.Floor(x), iz = (int)Math.Floor(z);
            double tx = x - ix, tz = z - iz;
            tx = tx * tx * (3 - 2 * tx); tz = tz * tz * (3 - 2 * tz);
            return Lerp(Lerp(Value(ix, iz, seed), Value(ix + 1, iz, seed), tx),
                Lerp(Value(ix, iz + 1, seed), Value(ix + 1, iz + 1, seed), tx), tz);
        }
    }
}
