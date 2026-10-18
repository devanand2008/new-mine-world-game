using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CubeFrontier.Core
{
    public sealed class WorldSave
    {
        public int Seed;
        public GameMode Mode;
        public float X, Y, Z, Yaw, Pitch, Health = 100, Hunger = 100, Stamina = 100, Day = .25f;
        public int Selected, Experience, GeneratorVersion = 2;
        public int Character;
        public FrontierQuest Quest = new FrontierQuest();
        public CampState Camp = new CampState();
        public Slot[] Inventory = new Inventory().Export();
        public Slot[] Equipment = new Equipment().Export();
        public List<KeyValuePair<Cell, Block>> Edits = new List<KeyValuePair<Cell, Block>>();
    }
    // This checksum detects local corruption. It is not authentication for multiplayer saves.
    public static class SaveCodec
    {
        private const int Magic = 0x43464543, Version = 5, MaxEdits = 250000;
        public static byte[] Encode(WorldSave save)
        {
            Validate(save);
            byte[] payload;
            using (var stream = new MemoryStream()) using (var writer = new BinaryWriter(stream)) {
                writer.Write(Magic); writer.Write(Version); writer.Write(save.Seed); writer.Write((byte)save.Mode);
                writer.Write(save.X); writer.Write(save.Y); writer.Write(save.Z); writer.Write(save.Yaw); writer.Write(save.Pitch);
                writer.Write(save.Health); writer.Write(save.Hunger); writer.Write(save.Stamina); writer.Write(save.Day); writer.Write(save.Selected);
                writer.Write(save.GeneratorVersion);writer.Write(save.Experience);
                foreach (var slot in save.Inventory) { writer.Write(slot.Item); writer.Write(slot.Count); writer.Write(slot.Durability); }
                // Equipment slots (v2)
                foreach (var slot in save.Equipment) { writer.Write(slot.Item); writer.Write(slot.Count); writer.Write(slot.Durability); }
                var sorted = new List<KeyValuePair<Cell, Block>>(save.Edits);
                sorted.Sort((a, b) => { int c = a.Key.X.CompareTo(b.Key.X); if (c != 0) return c; c = a.Key.Y.CompareTo(b.Key.Y); return c != 0 ? c : a.Key.Z.CompareTo(b.Key.Z); });
                writer.Write(sorted.Count);
                foreach (var edit in sorted) { writer.Write(edit.Key.X); writer.Write(edit.Key.Y); writer.Write(edit.Key.Z); writer.Write((byte)edit.Value); }
                writer.Write(save.Character); writer.Write(save.Quest.Wood); writer.Write(save.Quest.Stone); writer.Write(save.Quest.Placed); writer.Write(save.Quest.Flags);
                // Camp tail (v5). Older fields retain their original layout.
                writer.Write(save.Camp.HasRespawn);
                if (save.Camp.HasRespawn) WriteCell(writer, save.Camp.RespawnCell);
                var chests = new List<KeyValuePair<Cell, Inventory>>(save.Camp.Chests);
                chests.Sort((a, b) => { int c = a.Key.X.CompareTo(b.Key.X); if (c != 0) return c; c = a.Key.Y.CompareTo(b.Key.Y); return c != 0 ? c : a.Key.Z.CompareTo(b.Key.Z); });
                writer.Write(chests.Count);
                foreach (var chest in chests) {
                    WriteCell(writer, chest.Key);
                    foreach (var slot in chest.Value.Export()) { writer.Write(slot.Item); writer.Write(slot.Count); writer.Write(slot.Durability); }
                }
                writer.Flush(); payload = stream.ToArray();
            }
            using (var sha = SHA256.Create()) {
                var hash = sha.ComputeHash(payload); var result = new byte[payload.Length + hash.Length];
                Buffer.BlockCopy(payload, 0, result, 0, payload.Length); Buffer.BlockCopy(hash, 0, result, payload.Length, hash.Length); return result;
            }
        }
        public static WorldSave Decode(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 413 || bytes.Length > 4000000) throw new InvalidDataException("Invalid save size");
            int length = bytes.Length - 32;
            using (var sha = SHA256.Create()) {
                var hash = sha.ComputeHash(bytes, 0, length); int diff = 0;
                for (int i = 0; i < 32; i++) diff |= hash[i] ^ bytes[length + i];
                if (diff != 0) throw new InvalidDataException("Save checksum mismatch");
            }
            using (var stream = new MemoryStream(bytes, 0, length)) using (var reader = new BinaryReader(stream)) {
                int magic = reader.ReadInt32(); int version = reader.ReadInt32();
                if (magic != Magic || version < 1 || version > Version) throw new InvalidDataException("Unsupported save format");
                var s = new WorldSave { Seed = reader.ReadInt32(), Mode = (GameMode)reader.ReadByte() };
                s.X = reader.ReadSingle(); s.Y = reader.ReadSingle(); s.Z = reader.ReadSingle(); s.Yaw = reader.ReadSingle(); s.Pitch = reader.ReadSingle();
                s.Health = reader.ReadSingle(); s.Hunger = reader.ReadSingle(); s.Stamina = reader.ReadSingle(); s.Day = reader.ReadSingle(); s.Selected = reader.ReadInt32();
                s.GeneratorVersion = version>=3?reader.ReadInt32():version==1?1:2;
                if (version >= 2) s.Experience = reader.ReadInt32();
                for (int i = 0; i < Core.Inventory.Capacity; i++) s.Inventory[i] = new Slot { Item = reader.ReadInt32(), Count = reader.ReadInt32(), Durability = reader.ReadInt32() };
                // Equipment slots (v2)
                if (version >= 2) {
                    for (int i = 0; i < Core.Equipment.Slots; i++) s.Equipment[i] = new Slot { Item = reader.ReadInt32(), Count = reader.ReadInt32(), Durability = reader.ReadInt32() };
                }
                int count = reader.ReadInt32();
                if (count < 0 || count > MaxEdits) throw new InvalidDataException("Invalid edit count");
                for (int i = 0; i < count; i++) s.Edits.Add(new KeyValuePair<Cell, Block>(new Cell(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()), (Block)reader.ReadByte()));
                if(version>=4) { s.Character=reader.ReadInt32(); s.Quest=FrontierQuest.Restore(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32()); }
                if (version >= 5) {
                    byte hasRespawn = reader.ReadByte();
                    if (hasRespawn > 1) throw new InvalidDataException("Invalid respawn flag");
                    s.Camp.HasRespawn = hasRespawn == 1;
                    if (s.Camp.HasRespawn) s.Camp.RespawnCell = ReadCell(reader);
                    int chests = reader.ReadInt32();
                    if (chests < 0 || chests > CampState.MaxChests) throw new InvalidDataException("Invalid chest count");
                    // Each chest has a position and exactly 27 three-int slots.
                    if (stream.Length - stream.Position != (long)chests * (12 + Core.Inventory.Capacity * 12)) throw new InvalidDataException("Invalid chest payload size");
                    for (int chest = 0; chest < chests; chest++) {
                        Cell position = ReadCell(reader);
                        if (s.Camp.Chests.ContainsKey(position)) throw new InvalidDataException("Duplicate saved chest");
                        var slots = new Slot[Core.Inventory.Capacity];
                        for (int i = 0; i < slots.Length; i++) slots[i] = new Slot { Item = reader.ReadInt32(), Count = reader.ReadInt32(), Durability = reader.ReadInt32() };
                        var inventory = new Inventory();
                        try { inventory.Restore(slots); }
                        catch (ArgumentException error) { throw new InvalidDataException("Invalid chest inventory", error); }
                        s.Camp.Chests.Add(position, inventory);
                    }
                }
                if(stream.Position!=length)throw new InvalidDataException("Unexpected save data");
                Validate(s); return s;
            }
        }
        private static void WriteCell(BinaryWriter writer, Cell cell) { writer.Write(cell.X); writer.Write(cell.Y); writer.Write(cell.Z); }
        private static Cell ReadCell(BinaryReader reader) { return new Cell(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()); }
        private static bool Range(float v, float min, float max) { return !float.IsNaN(v) && !float.IsInfinity(v) && v >= min && v <= max; }
        private static void Validate(WorldSave s)
        {
            if (s == null || (int)s.Mode > 2 || !Range(s.X, -WorldData.Limit, WorldData.Limit) || !Range(s.Z, -WorldData.Limit, WorldData.Limit) || !Range(s.Y, -10, 256) || !Range(s.Yaw, -360, 360) || !Range(s.Pitch, -90, 90) || !Range(s.Health, 0, 100) || !Range(s.Hunger, 0, 100) || !Range(s.Stamina, 0, 100) || !Range(s.Day, 0, 1) || s.Selected < 0 || s.Selected > 8) throw new InvalidDataException("Invalid player state");
            new Inventory().Restore(s.Inventory);
            new Equipment().Restore(s.Equipment);
            if(s.Character<0||s.Character>12||s.Quest==null)throw new InvalidDataException("Invalid character or quest");
            FrontierQuest.Restore(s.Quest.Wood,s.Quest.Stone,s.Quest.Placed,s.Quest.Flags);
            if (s.Camp == null) throw new InvalidDataException("Missing camp state");
            s.Camp.Validate();
            if(s.Experience<0 || s.GeneratorVersion<1 || s.GeneratorVersion>2)throw new InvalidDataException("Invalid progression or terrain version");
            if (s.Edits == null || s.Edits.Count > MaxEdits) throw new InvalidDataException("Too many edits");
            var seen = new HashSet<Cell>();
            foreach (var e in s.Edits) if (e.Key.Y <= 0 || e.Key.Y >= Terrain.Height || Math.Abs((long)e.Key.X) > WorldData.Limit || Math.Abs((long)e.Key.Z) > WorldData.Limit || (int)e.Value >= Blocks.Count || e.Value == Block.Bedrock || !seen.Add(e.Key)) throw new InvalidDataException("Invalid or duplicate edit");
        }
    }
}
