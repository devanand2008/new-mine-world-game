using System;
using System.Collections.Generic;

namespace CubeFrontier.Core
{
    public static class Items
    {
        // Tool/weapon IDs 100+. Append only.
        public const int Pickaxe = 100, Axe = 101, Sword = 102, Ration = 103,
            Shovel = 104, Bow = 105, Arrow = 106, Shield = 107,
            Dagger = 108, Greatsword = 109,
            IronPickaxe = 110, IronAxe = 111, IronSword = 112,
            LeatherHelm = 120, LeatherChest = 121, LeatherLegs = 122,
            IronHelm = 123, IronChest = 124, IronLegs = 125,
            CrystalHelm = 126, CrystalChest = 127, CrystalLegs = 128,
            CookedMeat = 130, GoldenApple = 131,
            Torch = 132;
        public static bool Valid(int id) { return Blocks.Placeable(id) || (id >= Pickaxe && id <= IronSword) || Armor(id) || (id >= CookedMeat && id <= Torch); }
        public static bool Tool(int id) { return id==Pickaxe || id==Axe || id==Sword || id==Shovel || id==Bow || id==Dagger || id==Greatsword || (id>=IronPickaxe && id<=IronSword); }
        public static bool Armor(int id) { return id >= LeatherHelm && id <= CrystalLegs; }
        public static bool Weapon(int id) { return id == Sword || id == Dagger || id == Greatsword || id == IronSword || id == Bow; }
        public static bool Consumable(int id) { return id == Ration || id == CookedMeat || id == GoldenApple; }
        public static int Stack(int id) { return Tool(id) || Weapon(id) || Armor(id) || id == Shield ? 1 : id == Arrow ? 64 : id == Torch ? 64 : 64; }
        public static int Durability(int id)
        {
            if (id == Pickaxe || id == Axe || id == Shovel) return 100;
            if (id == IronPickaxe || id == IronAxe) return 200;
            if (id == Sword || id == Dagger) return 80;
            if (id == IronSword || id == Greatsword) return 150;
            if (id == Bow) return 120;
            if (id == Shield) return 160;
            if (Armor(id)) return id >= CrystalHelm ? 250 : id >= IronHelm ? 180 : 120;
            return 0;
        }
        public static float Damage(int id)
        {
            switch (id) {
                case Sword: return 7;
                case Dagger: return 5;
                case Greatsword: return 14;
                case IronSword: return 10;
                default: return 3;
            }
        }
        public static float AttackSpeed(int id)
        {
            switch (id) {
                case Dagger: return .3f;
                case Sword: case IronSword: return .55f;
                case Greatsword: return .9f;
                case Bow: return 1.0f;
                default: return .55f;
            }
        }
        public static float ArmorValue(int id)
        {
            switch (id) {
                case LeatherHelm: return 2; case LeatherChest: return 5; case LeatherLegs: return 3;
                case IronHelm: return 4; case IronChest: return 8; case IronLegs: return 5;
                case CrystalHelm: return 6; case CrystalChest: return 12; case CrystalLegs: return 7;
                case Shield: return 6;
                default: return 0;
            }
        }
        public static int ArmorSlotIndex(int id)
        {
            if (id == LeatherHelm || id == IronHelm || id == CrystalHelm) return 0;
            if (id == LeatherChest || id == IronChest || id == CrystalChest) return 1;
            if (id == LeatherLegs || id == IronLegs || id == CrystalLegs) return 2;
            if (id == Shield) return 3;
            return -1;
        }
        public static float HungerRestore(int id)
        {
            switch (id) {
                case Ration: return 28;
                case CookedMeat: return 40;
                case GoldenApple: return 60;
                default: return 0;
            }
        }
        public static string Name(int id)
        {
            switch (id) {
                case 0: return "Empty";
                case Pickaxe: return "Shard pickaxe";
                case Axe: return "Trail axe";
                case Sword: return "Frontier blade";
                case Ration: return "Trail ration";
                case Shovel: return "Stone shovel";
                case Bow: return "Crystal bow";
                case Arrow: return "Arrow";
                case Shield: return "Iron shield";
                case Dagger: return "Shadow dagger";
                case Greatsword: return "Eclipse greatsword";
                case IronPickaxe: return "Iron pickaxe";
                case IronAxe: return "Iron axe";
                case IronSword: return "Iron sword";
                case LeatherHelm: return "Leather helm";
                case LeatherChest: return "Leather vest";
                case LeatherLegs: return "Leather greaves";
                case IronHelm: return "Iron helm";
                case IronChest: return "Iron chestplate";
                case IronLegs: return "Iron greaves";
                case CrystalHelm: return "Crystal crown";
                case CrystalChest: return "Crystal plate";
                case CrystalLegs: return "Crystal greaves";
                case CookedMeat: return "Cooked meat";
                case GoldenApple: return "Golden apple";
                case Torch: return "Torch";
                default: return id > 0 && id < Blocks.Count ? Blocks.Name((Block)id) : "Unknown";
            }
        }
    }
    [Serializable]
    public sealed class Slot
    {
        public int Item, Count, Durability;
        public Slot Clone() { return new Slot { Item = Item, Count = Count, Durability = Durability }; }
    }
    public sealed class Ingredient
    {
        public readonly int Item, Count;
        public Ingredient(int item, int count) { Item = item; Count = count; }
    }
    public sealed class Recipe
    {
        public readonly int Output, Count;
        public readonly Ingredient[] Inputs;
        public readonly string Station; // null = hand, "Workbench", "Forge", "Altar"
        public Recipe(int output, int count, params Ingredient[] inputs) { Output = output; Count = count; Inputs = inputs; Station = null; }
        public Recipe(int output, int count, string station, params Ingredient[] inputs) { Output = output; Count = count; Station = station; Inputs = inputs; }
    }
    public static class Recipes
    {
        public static readonly Recipe[] All = {
            // Basic tools
            new Recipe(Items.Pickaxe, 1, new Ingredient((int)Block.Wood, 3), new Ingredient((int)Block.Stone, 5)),
            new Recipe(Items.Axe, 1, new Ingredient((int)Block.Wood, 3), new Ingredient((int)Block.Stone, 3)),
            new Recipe(Items.Sword, 1, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Ore, 3)),
            new Recipe(Items.Shovel, 1, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Stone, 2)),
            new Recipe(Items.Dagger, 1, new Ingredient((int)Block.Wood, 1), new Ingredient((int)Block.ShadowOre, 3)),
            // Iron tools
            new Recipe(Items.IronPickaxe, 1, new Ingredient((int)Block.Wood, 3), new Ingredient((int)Block.Iron, 4)),
            new Recipe(Items.IronAxe, 1, new Ingredient((int)Block.Wood, 3), new Ingredient((int)Block.Iron, 3)),
            new Recipe(Items.IronSword, 1, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Iron, 4)),
            // Ranged
            new Recipe(Items.Bow, 1, new Ingredient((int)Block.Wood, 4), new Ingredient((int)Block.Crystal, 2)),
            new Recipe(Items.Arrow, 16, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Stone, 1)),
            // Defense
            new Recipe(Items.Shield, 1, new Ingredient((int)Block.Iron, 5), new Ingredient((int)Block.Wood, 3)),
            new Recipe(Items.Greatsword, 1, new Ingredient((int)Block.Iron, 6), new Ingredient((int)Block.Crystal, 3), new Ingredient((int)Block.EmberCrystal, 1)),
            // Armor
            new Recipe(Items.LeatherHelm, 1, new Ingredient((int)Block.Leaves, 8), new Ingredient((int)Block.Wood, 2)),
            new Recipe(Items.LeatherChest, 1, new Ingredient((int)Block.Leaves, 12), new Ingredient((int)Block.Wood, 4)),
            new Recipe(Items.LeatherLegs, 1, new Ingredient((int)Block.Leaves, 10), new Ingredient((int)Block.Wood, 3)),
            new Recipe(Items.IronHelm, 1, new Ingredient((int)Block.Iron, 5)),
            new Recipe(Items.IronChest, 1, new Ingredient((int)Block.Iron, 8)),
            new Recipe(Items.IronLegs, 1, new Ingredient((int)Block.Iron, 6)),
            new Recipe(Items.CrystalHelm, 1, new Ingredient((int)Block.Crystal, 5), new Ingredient((int)Block.Moonstone, 2)),
            new Recipe(Items.CrystalChest, 1, new Ingredient((int)Block.Crystal, 8), new Ingredient((int)Block.Moonstone, 3)),
            new Recipe(Items.CrystalLegs, 1, new Ingredient((int)Block.Crystal, 6), new Ingredient((int)Block.Moonstone, 2)),
            // Food
            new Recipe(Items.Ration, 2, new Ingredient((int)Block.Leaves, 5)),
            new Recipe(Items.CookedMeat, 1, new Ingredient(Items.Ration, 2), new Ingredient((int)Block.Wood, 1)),
            new Recipe(Items.GoldenApple, 1, new Ingredient(Items.Ration, 3), new Ingredient((int)Block.Gold, 2)),
            // Special block
            new Recipe((int)Block.Beacon, 1, new Ingredient((int)Block.Crystal, 3), new Ingredient((int)Block.Stone, 6)),
            new Recipe((int)Block.Torch, 4, new Ingredient((int)Block.Wood, 1), new Ingredient((int)Block.Ore, 1)),
            // Building materials
            new Recipe((int)Block.OakPlanks, 8, new Ingredient((int)Block.Wood, 2)),
            new Recipe((int)Block.SprucePlanks, 8, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Basalt, 1)),
            new Recipe((int)Block.BirchPlanks, 8, new Ingredient((int)Block.Wood, 2), new Ingredient((int)Block.Sand, 1)),
            new Recipe((int)Block.Cobblestone, 8, new Ingredient((int)Block.Stone, 8)),
            new Recipe((int)Block.StoneBricks, 8, new Ingredient((int)Block.Stone, 8)),
            new Recipe((int)Block.MossyStone, 8, new Ingredient((int)Block.Stone, 6), new Ingredient((int)Block.Leaves, 2)),
            new Recipe((int)Block.ClayBricks, 8, new Ingredient((int)Block.Soil, 6), new Ingredient((int)Block.Stone, 2)),
            new Recipe((int)Block.Sandstone, 8, new Ingredient((int)Block.Sand, 8)),
            new Recipe((int)Block.SmoothStone, 8, new Ingredient((int)Block.Stone, 8)),
            new Recipe((int)Block.WhitePlaster, 8, new Ingredient((int)Block.Sand, 4), new Ingredient((int)Block.Soil, 4)),
            new Recipe((int)Block.SlateTiles, 8, new Ingredient((int)Block.Basalt, 8)),
            new Recipe((int)Block.RedRoof, 8, new Ingredient((int)Block.Soil, 4), new Ingredient((int)Block.Stone, 4)),
            new Recipe((int)Block.Glass, 8, new Ingredient((int)Block.Sand, 6), new Ingredient((int)Block.Crystal, 1)),
            new Recipe((int)Block.DarkGlass, 8, new Ingredient((int)Block.Glass, 8), new Ingredient((int)Block.Basalt, 1)),
            new Recipe((int)Block.Copper, 4, new Ingredient((int)Block.Ore, 4)),
            new Recipe((int)Block.Iron, 4, new Ingredient((int)Block.Ore, 4), new Ingredient((int)Block.Stone, 2)),
            new Recipe((int)Block.Gold, 4, new Ingredient((int)Block.Ore, 4), new Ingredient((int)Block.Crystal, 2)),
            new Recipe((int)Block.Lantern, 4, new Ingredient((int)Block.Crystal, 1), new Ingredient((int)Block.Ore, 2)),
            new Recipe((int)Block.Bookshelf, 2, new Ingredient((int)Block.OakPlanks, 4), new Ingredient((int)Block.Leaves, 2)),
            new Recipe((int)Block.Crate, 2, new Ingredient((int)Block.OakPlanks, 6)),
            new Recipe((int)Block.RedWool, 8, new Ingredient((int)Block.Leaves, 5), new Ingredient((int)Block.Soil, 1)),
            new Recipe((int)Block.BlueWool, 8, new Ingredient((int)Block.Leaves, 5), new Ingredient((int)Block.Crystal, 1)),
            new Recipe((int)Block.WhiteWool, 8, new Ingredient((int)Block.Leaves, 5), new Ingredient((int)Block.Sand, 1)),
            new Recipe((int)Block.GreenWool, 8, new Ingredient((int)Block.Leaves, 6)),
            new Recipe((int)Block.Terracotta, 8, new Ingredient((int)Block.Soil, 8)),
            new Recipe((int)Block.PolishedBasalt, 8, new Ingredient((int)Block.Basalt, 8)),
            new Recipe((int)Block.Marble, 8, new Ingredient((int)Block.Stone, 6), new Ingredient((int)Block.Sand, 2)),
            new Recipe((int)Block.Path, 8, new Ingredient((int)Block.Soil, 4), new Ingredient((int)Block.Sand, 4)),
            new Recipe((int)Block.Gravel, 8, new Ingredient((int)Block.Stone, 6), new Ingredient((int)Block.Soil, 2)),
            new Recipe((int)Block.PackedIce, 8, new Ingredient((int)Block.Snow, 6), new Ingredient((int)Block.Crystal, 1)),
            new Recipe((int)Block.DarkTimber, 4, new Ingredient((int)Block.Wood, 4), new Ingredient((int)Block.Basalt, 1)),
            new Recipe((int)Block.CarvedStone, 8, new Ingredient((int)Block.Stone, 8), new Ingredient((int)Block.Crystal, 1)),
            // Rare blocks
            new Recipe((int)Block.Obsidian, 4, new Ingredient((int)Block.Basalt, 4), new Ingredient((int)Block.EmberCrystal, 1)),
            new Recipe((int)Block.Moonstone, 2, new Ingredient((int)Block.Crystal, 4), new Ingredient((int)Block.Snow, 4)),
            new Recipe((int)Block.SpectralBlock, 2, new Ingredient((int)Block.Crystal, 3), new Ingredient((int)Block.ShadowOre, 3)),
            // Functional camp stations. Append to keep existing recipe indices stable.
            new Recipe((int)Block.StorageChest, 1, new Ingredient((int)Block.OakPlanks, 8)),
            new Recipe((int)Block.Furnace, 1, new Ingredient((int)Block.Stone, 8)),
            new Recipe((int)Block.Bed, 1, new Ingredient((int)Block.WhiteWool, 3), new Ingredient((int)Block.OakPlanks, 4)),
        };
    }

    // Equipment: 4 armor slots (helm, chest, legs, shield)
    [Serializable]
    public sealed class Equipment
    {
        public const int Slots = 4; // 0=Head, 1=Chest, 2=Legs, 3=Shield
        private readonly Slot[] slots = new Slot[Slots];
        public Equipment() { for (int i = 0; i < Slots; i++) slots[i] = new Slot(); }
        public Slot At(int index) { return slots[index].Clone(); }
        public float TotalArmor()
        {
            float total = 0;
            for (int i = 0; i < Slots; i++) if (slots[i].Count > 0) total += Items.ArmorValue(slots[i].Item);
            return total;
        }
        public bool Equip(int index, Slot item)
        {
            if (index < 0 || index >= Slots) return false;
            if (item.Count > 0 && Items.ArmorSlotIndex(item.Item) != index && !(index == 3 && item.Item == Items.Shield)) return false;
            slots[index] = item.Clone();
            return true;
        }
        public Slot Unequip(int index)
        {
            if (index < 0 || index >= Slots) return new Slot();
            var removed = slots[index].Clone();
            slots[index] = new Slot();
            return removed;
        }
        public Slot[] Export() { var copy = new Slot[Slots]; for (int i = 0; i < Slots; i++) copy[i] = slots[i].Clone(); return copy; }
        public void Restore(Slot[] data)
        {
            if (data == null || data.Length != Slots) throw new ArgumentException("Invalid equipment size");
            for(int i=0;i<Slots;i++){
                var s=data[i];
                if(s==null || (s.Count==0 && (s.Item!=0 || s.Durability!=0)) || (s.Count!=0 && (s.Count!=1 || Items.ArmorSlotIndex(s.Item)!=i || s.Durability<=0 || s.Durability>Items.Durability(s.Item))))throw new ArgumentException("Invalid equipment slot");
            }
            for (int i = 0; i < Slots; i++) slots[i] = data[i].Clone();
        }
        public void WearAll()
        {
            for (int i = 0; i < Slots; i++)
                if (slots[i].Count > 0 && (Items.Armor(slots[i].Item) || slots[i].Item == Items.Shield))
                    if (--slots[i].Durability <= 0) { slots[i] = new Slot(); }
        }
    }

    public sealed class Inventory
    {
        public const int Capacity = 27;
        private Slot[] slots = new Slot[Capacity];
        public Inventory() { for (int i = 0; i < Capacity; i++) slots[i] = new Slot(); }
        public Slot At(int index) { return slots[index].Clone(); }
        public int Count(int id) { int n = 0; foreach (var s in slots) if (s.Item == id) n += s.Count; return n; }
        // Explicit durability transfers an existing item; omitted durability creates a fresh one.
        public bool Add(int id, int count, int durability = -1)
        {
            if (!Items.Valid(id) || count <= 0 || count > Capacity * 64) return false;
            int maximumDurability = Items.Durability(id);
            if (durability == -1) durability = maximumDurability;
            if (durability < 0 || durability > maximumDurability || (maximumDurability > 0 && durability == 0)) return false;
            int room = 0;
            foreach (var s in slots) if (s.Count == 0 || s.Item == id) room += Items.Stack(id) - s.Count;
            if (room < count) return false;
            for (int pass = 0; pass < 2; pass++) foreach (var s in slots) {
                if (count == 0) return true;
                if ((pass == 0 && s.Item == id && s.Count > 0) || (pass == 1 && s.Count == 0)) {
                    int add = Math.Min(count, Items.Stack(id) - s.Count);
                    if (s.Count == 0) { s.Item = id; s.Durability = durability; }
                    s.Count += add; count -= add;
                }
            }
            return count == 0;
        }
        public bool Remove(int id, int count)
        {
            if (count <= 0 || Count(id) < count) return false;
            foreach (var s in slots) if (s.Item == id) { int take = Math.Min(count, s.Count); s.Count -= take; count -= take; if (s.Count == 0) { s.Item = 0; s.Durability = 0; } }
            return true;
        }
        public bool ConsumeAt(int index)
        {
            Slot s = slots[index]; if (s.Count == 0) return false;
            s.Count--; if (s.Count == 0) { s.Item = 0; s.Durability = 0; } return true;
        }
        public void Wear(int index) { Slot s = slots[index]; if (Items.Tool(s.Item) && --s.Durability <= 0) { s.Item = 0; s.Count = 0; s.Durability = 0; } }
        public void Swap(int a, int b) { Slot t = slots[a]; slots[a] = slots[b]; slots[b] = t; }
        public bool EquipAt(int bagIndex, Equipment equipment)
        {
            if(bagIndex<0||bagIndex>=Capacity||equipment==null) return false;
            Slot item=slots[bagIndex]; int target=Items.ArmorSlotIndex(item.Item);
            if(target<0||item.Count!=1||item.Durability<=0) return false;
            Slot previous=equipment.At(target);
            if(!equipment.Equip(target,item))return false;
            slots[bagIndex]=previous; return true;
        }
        public bool Unequip(int index, Equipment equipment)
        {
            if(index<0||index>=Equipment.Slots||equipment==null||equipment.At(index).Count==0)return false;
            for(int i=0;i<Capacity;i++)if(slots[i].Count==0){slots[i]=equipment.Unequip(index);return true;}
            return false;
        }
        public bool SelectCreative(int index, int id)
        {
            if(index<0||index>=9||!Blocks.Placeable(id))return false;
            slots[index]=new Slot {Item=id, Count=64};return true;
        }
        public bool Craft(Recipe recipe)
        {
            var candidate = new Inventory(); candidate.Restore(Export());
            foreach (var input in recipe.Inputs) if (!candidate.Remove(input.Item, input.Count)) return false;
            if (!candidate.Add(recipe.Output, recipe.Count)) return false;
            slots = candidate.slots; return true;
        }
        public Slot[] Export() { var copy = new Slot[Capacity]; for (int i = 0; i < Capacity; i++) copy[i] = slots[i].Clone(); return copy; }
        public void Restore(Slot[] data)
        {
            if (data == null || data.Length != Capacity) throw new ArgumentException("Invalid inventory size");
            foreach (var s in data) if (s == null || s.Count < 0 || s.Count > Items.Stack(s.Item) || (s.Count == 0 && (s.Item != 0 || s.Durability != 0)) || (s.Count > 0 && !Items.Valid(s.Item)) || s.Durability < 0 || s.Durability > Items.Durability(s.Item) || (Items.Tool(s.Item) && s.Durability == 0)) throw new ArgumentException("Invalid inventory slot");
            for (int i = 0; i < Capacity; i++) slots[i] = data[i].Clone();
        }
    }
}
