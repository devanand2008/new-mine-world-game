using System;
using System.Linq;
using CubeFrontier.Core;

public static class InventoryIntegrityTests
{
    private static int passed;
    private static void Check(bool valid, string name)
    {
        if (!valid) throw new Exception("FAIL: " + name);
        passed++; Console.WriteLine("PASS: " + name);
    }
    public static void Main()
    {
        var pack = new Inventory();
        Check(pack.Add(Items.Pickaxe, 1), "New tools start at their standard durability");
        for (int i = 0; i < 37; i++) pack.Wear(0);
        Slot dropped = pack.At(0);
        Check(dropped.Durability == 63 && pack.ConsumeAt(0), "A worn tool can leave its source slot");
        Check(pack.Add(dropped.Item, dropped.Count, dropped.Durability) && pack.At(0).Durability == 63,
            "Picking up an existing tool preserves its exact wear");
        Check(pack.Add(Items.Pickaxe, 1, 8) && pack.At(1).Durability == 8 && pack.At(0).Durability == 63,
            "Two worn tools retain distinct durability in separate slots");
        Check(pack.Add(Items.LeatherChest, 1, 7) && pack.At(2).Durability == 7,
            "Armour transfers preserve durability");
        int countBefore = pack.Count(Items.Pickaxe);
        Check(!pack.Add(Items.Pickaxe, 1, 0) && !pack.Add(Items.Pickaxe, 1, 101)
            && !pack.Add(Items.Pickaxe, 1, -2) && pack.Count(Items.Pickaxe) == countBefore,
            "Invalid transferred durability is rejected without changing inventory");
        Check(!pack.Add((int)Block.Wood, 1, 1) && pack.Add((int)Block.Wood, 65, 0),
            "Resource transfers require zero durability and still split stacks");

        var full = new Inventory(); full.Add((int)Block.Stone, Inventory.Capacity * 64);
        Check(!full.Add(Items.Pickaxe, 1, 23) && full.Count((int)Block.Stone) == Inventory.Capacity * 64,
            "A full pack rejects worn pickups without mutating its existing stacks");
        full.Remove((int)Block.Stone, 64);
        Check(full.Add(Items.Pickaxe, 1, 23) && full.At(0).Durability == 23,
            "Retrying a pickup after freeing space retains its original durability");

        var equipped = new Equipment(); var shieldPack = new Inventory(); shieldPack.Add(Items.Shield, 1, 2);
        Check(shieldPack.EquipAt(0, equipped) && equipped.TotalArmor() == 6,
            "An equipped iron shield contributes real damage reduction");
        equipped.WearAll();
        Check(equipped.At(3).Durability == 1 && equipped.TotalArmor() == 6,
            "An equipped shield wears on a damage event");
        equipped.WearAll();
        Check(equipped.At(3).Count == 0 && equipped.TotalArmor() == 0,
            "A broken shield is removed and no longer protects the player");

        Check(Recipes.All[0].Output == Items.Pickaxe && Recipes.All[1].Output == Items.Axe
            && Recipes.All[2].Output == Items.Sword, "Existing starter recipe indices remain stable");
        Recipe[] appended = Recipes.All.Skip(Recipes.All.Length - 3).ToArray();
        Check(appended[0].Output == (int)Block.StorageChest && appended[1].Output == (int)Block.Furnace
            && appended[2].Output == (int)Block.Bed, "New camp recipes are appended in the expected order");
        var materials = new Inventory(); materials.Add((int)Block.OakPlanks, 12);
        materials.Add((int)Block.Stone, 8); materials.Add((int)Block.WhiteWool, 3);
        Check(materials.Craft(appended[0]) && materials.Craft(appended[1]) && materials.Craft(appended[2]),
            "Chest, furnace and bed craft from their actual ingredient transactions");
        Check(materials.Count((int)Block.OakPlanks) == 0 && materials.Count((int)Block.Stone) == 0
            && materials.Count((int)Block.WhiteWool) == 0 && materials.Count((int)Block.StorageChest) == 1
            && materials.Count((int)Block.Furnace) == 1 && materials.Count((int)Block.Bed) == 1,
            "Station recipes consume precisely the advertised materials");
        Console.WriteLine("Inventory integrity: " + passed + " checks passed.");
    }
}
