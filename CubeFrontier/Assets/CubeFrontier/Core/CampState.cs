using System;
using System.Collections.Generic;
using System.IO;

namespace CubeFrontier.Core
{
    // Local camp data is kept apart from terrain; the runtime checks that the
    // associated chest or bed block still exists before using it.
    public sealed class CampState
    {
        public const int MaxChests = 256;
        public readonly Dictionary<Cell, Inventory> Chests = new Dictionary<Cell, Inventory>();
        public bool HasRespawn;
        public Cell RespawnCell;

        public Inventory GetChest(Cell cell)
        {
            if (!ValidCell(cell)) throw new ArgumentOutOfRangeException("cell", "Camp blocks must be inside the world");
            Inventory chest;
            if (Chests.TryGetValue(cell, out chest)) return chest;
            if (Chests.Count >= MaxChests) throw new InvalidOperationException("The world has reached its chest storage limit");
            chest = new Inventory(); Chests.Add(cell, chest); return chest;
        }

        public bool RemoveChest(Cell cell)
        {
            Inventory chest;
            if (!Chests.TryGetValue(cell, out chest)) return true;
            if (chest == null) return false;
            for (int i = 0; i < Inventory.Capacity; i++) if (chest.At(i).Count != 0) return false;
            return Chests.Remove(cell);
        }

        // Move an exact quantity from one slot. Candidate arrays let capacity
        // checks fail without consuming items, and avoid Add resetting tool wear.
        public static bool Transfer(Inventory source, int slot, Inventory destination, int count)
        {
            if (source == null || destination == null || ReferenceEquals(source, destination) ||
                slot < 0 || slot >= Inventory.Capacity || count <= 0) return false;
            Slot[] from = source.Export(), to = destination.Export();
            Slot item = from[slot];
            if (item.Count < count) return false;
            int capacity = 0, stack = Items.Stack(item.Item);
            foreach (Slot target in to) {
                if (target.Count == 0) capacity += stack;
                else if (target.Item == item.Item && target.Durability == item.Durability) capacity += stack - target.Count;
            }
            if (capacity < count) return false;
            int remaining = count;
            for (int pass = 0; pass < 2 && remaining > 0; pass++) foreach (Slot target in to) {
                if (remaining == 0) break;
                bool matches = target.Count > 0 && target.Item == item.Item && target.Durability == item.Durability;
                if ((pass == 0 && matches) || (pass == 1 && target.Count == 0)) {
                    int moved = Math.Min(remaining, stack - target.Count);
                    target.Item = item.Item; target.Durability = item.Durability;
                    target.Count += moved; remaining -= moved;
                }
            }
            item.Count -= count;
            if (item.Count == 0) { item.Item = 0; item.Durability = 0; }
            // Validate both candidates before changing either live inventory.
            new Inventory().Restore(from); new Inventory().Restore(to);
            source.Restore(from); destination.Restore(to); return true;
        }

        public static bool TrySmelt(Inventory inventory)
        {
            if (inventory == null) return false;
            return inventory.Craft(new Recipe((int)Block.Iron, 1,
                new Ingredient((int)Block.Ore, 2), new Ingredient((int)Block.Wood, 1)));
        }

        public CampState Clone()
        {
            Validate();
            var copy = new CampState { HasRespawn = HasRespawn, RespawnCell = RespawnCell };
            foreach (var entry in Chests) {
                var inventory = new Inventory(); inventory.Restore(entry.Value.Export());
                copy.Chests.Add(entry.Key, inventory);
            }
            return copy;
        }

        public void Validate()
        {
            if (HasRespawn && !ValidCell(RespawnCell)) throw new InvalidDataException("Invalid bed position");
            if (Chests.Count > MaxChests) throw new InvalidDataException("Too many saved chests");
            foreach (var entry in Chests) {
                if (!ValidCell(entry.Key) || entry.Value == null) throw new InvalidDataException("Invalid saved chest");
                try { new Inventory().Restore(entry.Value.Export()); }
                catch (ArgumentException error) { throw new InvalidDataException("Invalid chest inventory", error); }
            }
        }

        private static bool ValidCell(Cell cell)
        {
            return cell.Y > 0 && cell.Y < Terrain.Height &&
                Math.Abs((long)cell.X) <= WorldData.Limit && Math.Abs((long)cell.Z) <= WorldData.Limit;
        }
    }
}
