using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CubeFrontier.Core;

public static class CampCoreTests
{
    private static int passed;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        passed++; Console.WriteLine("PASS: " + name);
    }
    private static void Reject(Action action, string name)
    {
        bool rejected = false;
        try { action(); }
        catch (ArgumentException) { rejected = true; }
        catch (InvalidDataException) { rejected = true; }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, name);
    }
    private static bool Same(Slot[] a, Slot[] b)
    {
        return a.Length == b.Length && a.Zip(b, (x, y) => x.Item == y.Item && x.Count == y.Count && x.Durability == y.Durability).All(x => x);
    }
    private static byte[] Checksum(byte[] payload)
    {
        using (var sha = SHA256.Create()) return payload.Concat(sha.ComputeHash(payload)).ToArray();
    }
    // Independent legacy-format fixture writer: these layouts predate camp data.
    private static byte[] Legacy(int version)
    {
        using (var stream = new MemoryStream()) using (var writer = new BinaryWriter(stream)) {
            writer.Write(0x43464543); writer.Write(version); writer.Write(714); writer.Write((byte)GameMode.Survival);
            foreach (float value in new[] { -17f, 45f, -2f, 27f, 12f, 100f, 91f, 80f, .39f }) writer.Write(value);
            writer.Write(2);
            if (version >= 3) writer.Write(1);
            if (version >= 2) writer.Write(19);
            var bag = new Inventory(); bag.Add((int)Block.Wood, 7); bag.Add(Items.Pickaxe, 1); bag.Wear(1);
            foreach (Slot slot in bag.Export()) { writer.Write(slot.Item); writer.Write(slot.Count); writer.Write(slot.Durability); }
            if (version >= 2) for (int i = 0; i < Equipment.Slots; i++) { writer.Write(0); writer.Write(0); writer.Write(0); }
            writer.Write(1); writer.Write(-8); writer.Write(40); writer.Write(-13); writer.Write((byte)Block.Crate);
            if (version >= 4) { writer.Write(12); writer.Write(4); writer.Write(8); writer.Write(12); writer.Write(3); }
            writer.Flush(); return Checksum(stream.ToArray());
        }
    }
    private static byte[] Mutate(byte[] bytes, int offset, int value)
    {
        byte[] payload = bytes.Take(bytes.Length - 32).ToArray();
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, payload, offset, 4); return Checksum(payload);
    }
    public static void Main()
    {
        var camp = new CampState(); var cell = new Cell(-17, 35, -33);
        var chest = camp.GetChest(cell);
        Check(ReferenceEquals(chest, camp.GetChest(cell)) && camp.Chests.Count == 1, "reopening a chest retains one inventory");
        Check(chest.Add((int)Block.Wood, 61) && !camp.RemoveChest(cell), "a nonempty chest cannot be removed");
        var bag = new Inventory(); bag.Add((int)Block.Wood, 10);
        Check(CampState.Transfer(bag, 0, chest, 5) && bag.Count((int)Block.Wood) == 5 && chest.At(0).Count == 64 && chest.At(1).Count == 2, "transfer fills existing stacks before empty slots");
        Check(CampState.Transfer(chest, 1, bag, 2) && bag.Count((int)Block.Wood) == 7 && chest.At(1).Item == 0, "withdrawal clears empty source slots");
        Slot[] beforeBag = bag.Export(), beforeChest = chest.Export();
        Check(!CampState.Transfer(bag, 0, bag, 1) && !CampState.Transfer(bag, 0, chest, 8) &&
              !CampState.Transfer(bag, -1, chest, 1) && !CampState.Transfer(bag, 27, chest, 1) &&
              !CampState.Transfer(bag, 0, chest, 0) && !CampState.Transfer(bag, 0, chest, -1) &&
              !CampState.Transfer(null, 0, chest, 1) && !CampState.Transfer(bag, 0, null, 1) &&
              Same(beforeBag, bag.Export()) && Same(beforeChest, chest.Export()), "invalid transfers do not mutate either inventory");
        var full = new Inventory(); full.Add((int)Block.Stone, Inventory.Capacity * 64);
        Slot[] beforeFull = full.Export();
        Check(!CampState.Transfer(bag, 0, full, 7) && Same(beforeFull, full.Export()) && Same(beforeBag, bag.Export()), "full destination rejects transfer without item loss");
        full.Remove((int)Block.Stone, 1); bag.Add((int)Block.Stone, 2);
        beforeFull = full.Export(); beforeBag = bag.Export();
        Check(!CampState.Transfer(bag, 1, full, 2) && Same(beforeFull, full.Export()) && Same(beforeBag, bag.Export()), "insufficient partial-stack space rejects the entire transfer");
        var worn = new Inventory(); worn.Add(Items.Pickaxe, 1); worn.Wear(0); worn.Wear(0);
        Check(CampState.Transfer(worn, 0, chest, 1) && worn.At(0).Count == 0 && chest.At(1).Durability == 98, "deposit preserves a worn tool's durability");
        Check(CampState.Transfer(chest, 1, worn, 1) && worn.At(0).Durability == 98, "withdrawal preserves a worn tool's durability");
        var armor = new Inventory(); armor.Add(Items.IronChest, 1); var armorSlots = armor.Export(); armorSlots[0].Durability = 77; armor.Restore(armorSlots);
        Check(CampState.Transfer(armor, 0, chest, 1) && chest.At(1).Durability == 77, "armor transfer retains wear without stacking gear");
        Check(chest.Remove((int)Block.Wood, 64) && chest.Remove(Items.IronChest, 1) && camp.RemoveChest(cell) && camp.RemoveChest(cell), "only an empty chest can be removed, and repeat removal is harmless");
        Check(!CampState.TrySmelt(null), "null furnace inventory is rejected");
        var furnace = new Inventory(); furnace.Add((int)Block.Ore, 5); furnace.Add((int)Block.Wood, 2);
        Check(CampState.TrySmelt(furnace) && furnace.Count((int)Block.Ore) == 3 && furnace.Count((int)Block.Wood) == 1 && furnace.Count((int)Block.Iron) == 1, "smelting consumes two ore and one wood for one iron");
        furnace.Remove((int)Block.Wood, 1); var beforeFurnace = furnace.Export();
        Check(!CampState.TrySmelt(furnace) && Same(beforeFurnace, furnace.Export()), "missing fuel does not consume ore");
        var fullFurnace = new Inventory(); fullFurnace.Add((int)Block.Ore, 64); fullFurnace.Add((int)Block.Wood, 64); fullFurnace.Add((int)Block.Stone, 25 * 64);
        beforeFurnace = fullFurnace.Export();
        Check(!CampState.TrySmelt(fullFurnace) && Same(beforeFurnace, fullFurnace.Export()), "full output inventory restores all furnace ingredients");
        var lastFuel = new Inventory(); lastFuel.Add((int)Block.Ore, 2); lastFuel.Add((int)Block.Wood, 1); lastFuel.Add((int)Block.Stone, 25 * 64);
        Check(CampState.TrySmelt(lastFuel) && lastFuel.Count((int)Block.Iron) == 1, "smelting can reuse a slot freed by consumed ingredients");
        camp.GetChest(cell).Add(Items.Pickaxe, 1); camp.GetChest(cell).Wear(0);
        camp.GetChest(new Cell(100000, 63, -100000)).Add((int)Block.Glass, 64);
        camp.HasRespawn = true; camp.RespawnCell = new Cell(-21, 34, -30);
        var snapshot = camp.Clone(); snapshot.GetChest(cell).Wear(0);
        Check(camp.GetChest(cell).At(0).Durability == 99 && snapshot.GetChest(cell).At(0).Durability == 98, "camp snapshots deeply clone chest slots");
        var save = new WorldSave { Seed = 714, Camp = camp };
        var encoded = SaveCodec.Encode(save); var decoded = SaveCodec.Decode(encoded);
        Check(BitConverter.ToInt32(encoded, 4) == 5 && decoded.Camp.HasRespawn && decoded.Camp.RespawnCell.Equals(camp.RespawnCell) &&
              decoded.Camp.Chests.Count == 2 && decoded.Camp.GetChest(cell).At(0).Durability == 99, "v5 save round trip restores beds, negative chest coordinates, and worn contents");
        Check(SaveCodec.Encode(decoded).SequenceEqual(encoded), "camp saves have canonical byte-for-byte encoding");
        var reordered = new CampState { HasRespawn = camp.HasRespawn, RespawnCell = camp.RespawnCell };
        foreach (var entry in camp.Chests.Reverse()) { var copy = new Inventory(); copy.Restore(entry.Value.Export()); reordered.Chests.Add(entry.Key, copy); }
        Check(SaveCodec.Encode(new WorldSave { Seed = 714, Camp = reordered }).SequenceEqual(encoded), "dictionary insertion order does not change save bytes");
        for (int version = 1; version <= 4; version++) {
            var legacy = SaveCodec.Decode(Legacy(version));
            Check(legacy.Camp.Chests.Count == 0 && !legacy.Camp.HasRespawn && legacy.Inventory[1].Durability == 99 && legacy.Edits[0].Key.X == -8,
                "v" + version + " fixture migrates with empty camp and intact inventory/edits");
            Check(SaveCodec.Decode(SaveCodec.Encode(legacy)).GeneratorVersion == (version == 2 ? 2 : 1), "v" + version + " migration retains terrain generation");
            if (version == 4) Check(legacy.Character == 12 && legacy.Quest.Crafted && legacy.Quest.Sheltered && legacy.Quest.Placed == 12 && legacy.Experience == 19, "v4 migration retains character, quest and experience");
        }
        Reject(() => camp.GetChest(new Cell(int.MinValue, 1, 0)), "extreme negative chest coordinate is rejected without overflow");
        Reject(() => camp.GetChest(new Cell(0, 0, 0)), "bedrock-level chest is rejected");
        Reject(() => camp.GetChest(new Cell(0, 64, 0)), "out-of-height chest is rejected");
        Reject(() => camp.GetChest(new Cell(100001, 10, 0)), "out-of-world chest is rejected");
        var limited = new CampState(); for (int i = 0; i < CampState.MaxChests; i++) limited.GetChest(new Cell(i, 2, 0));
        Check(SaveCodec.Decode(SaveCodec.Encode(new WorldSave { Camp = limited })).Camp.Chests.Count == 256, "maximum supported chest count round trips");
        Reject(() => limited.GetChest(new Cell(256, 2, 0)), "creating a 257th chest fails without changing stored chests");
        limited.Chests.Add(new Cell(257, 2, 0), new Inventory()); Reject(() => SaveCodec.Encode(new WorldSave { Camp = limited }), "oversize camp dictionary cannot be saved");
        var invalid = new CampState(); invalid.Chests.Add(new Cell(0, 1, 0), null);
        Reject(() => SaveCodec.Encode(new WorldSave { Camp = invalid }), "null chest inventory cannot be saved");
        invalid.Chests.Clear(); invalid.HasRespawn = true; invalid.RespawnCell = new Cell(0, 64, 0);
        Reject(() => SaveCodec.Encode(new WorldSave { Camp = invalid }), "invalid bed position cannot be saved");
        Reject(() => SaveCodec.Encode(new WorldSave { Camp = null }), "missing camp cannot be saved");
        var single = new WorldSave(); single.Camp.GetChest(new Cell(-1, 3, -7)).Add((int)Block.Wood, 5);
        byte[] singleBytes = SaveCodec.Encode(single);
        // v4 tail ends at 457 for zero terrain edits; v5 flag=457/count=458, first chest=462.
        const int campTail = 457, chestStart = campTail + 5;
        Reject(() => SaveCodec.Decode(Mutate(singleBytes, chestStart, int.MinValue)), "valid-checksum save with invalid chest position is rejected");
        Reject(() => SaveCodec.Decode(Mutate(singleBytes, chestStart + 12, 999)), "valid-checksum save with invalid chest item is rejected");
        Reject(() => SaveCodec.Decode(Mutate(singleBytes, chestStart + 16, 65)), "valid-checksum save with oversized chest stack is rejected");
        Reject(() => SaveCodec.Decode(Mutate(singleBytes, campTail + 1, -1)), "negative serialized chest count is rejected");
        Reject(() => SaveCodec.Decode(Mutate(singleBytes, campTail + 1, 257)), "oversized serialized chest count is rejected");
        var malformedFlag = singleBytes.Take(singleBytes.Length - 32).ToArray(); malformedFlag[campTail] = 2;
        Reject(() => SaveCodec.Decode(Checksum(malformedFlag)), "non-boolean respawn byte is rejected");
        single.Camp.GetChest(new Cell(5, 3, 6)); byte[] two = SaveCodec.Encode(single);
        byte[] duplicatePayload = two.Take(two.Length - 32).ToArray();
        Buffer.BlockCopy(duplicatePayload, chestStart, duplicatePayload, chestStart + 336, 12);
        Reject(() => SaveCodec.Decode(Checksum(duplicatePayload)), "duplicate serialized chest coordinates are rejected");
        byte[] truncatedPayload = singleBytes.Take(singleBytes.Length - 33).ToArray();
        Reject(() => SaveCodec.Decode(Checksum(truncatedPayload)), "valid-checksum truncated chest inventory is rejected");
        Console.WriteLine("All " + passed + " camp checks passed.");
    }
}
