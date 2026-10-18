using System;
using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    /// <summary>World-backed interactions for placed camp blocks. Inventory data lives in the save.</summary>
    public sealed class CampStations : MonoBehaviour
    {
        public GameSession Session;
        public Cell OpenCell { get; private set; }
        public Block OpenBlock { get; private set; }
        public string Status { get; private set; }
        public Inventory Chest { get; private set; }

        public static bool IsStation(Block block)
        {
            return block == Block.StorageChest || block == Block.Furnace || block == Block.Bed;
        }

        public bool CurrentValid
        {
            get {
                return Session && Session.HasGame && Session.World && Session.CampData != null &&
                    IsStation(OpenBlock) && Session.World.Data.Get(OpenCell.X, OpenCell.Y, OpenCell.Z) == OpenBlock;
            }
        }

        public bool TryOpen()
        {
            if (!Session || !Session.HasGame || Session.Paused || !Session.World.Ready ||
                (Session.Builder && Session.Builder.Active)) return false;
            RaycastHit hit;
            Ray ray = Session.Player.View.ViewportPointToRay(new Vector3(.5f, .5f));
            if (!Physics.Raycast(ray, out hit, 5, 1, QueryTriggerInteraction.Ignore)) return false;
            Vector3 point = hit.point - hit.normal * .01f;
            Cell cell = new Cell(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));
            Block block = Session.World.Data.Get(cell.X, cell.Y, cell.Z);
            if (!IsStation(block)) return false;
            Inventory chest = null;
            if (block == Block.StorageChest) {
                try { chest = Session.CampData.GetChest(cell); }
                catch (InvalidOperationException e) { Session.Notify(e.Message); return false; }
            }
            OpenCell = cell; OpenBlock = block; Chest = chest; Status = "";
            Session.Pause();
            Session.GetComponent<FrontierUI>().CampOpen = true;
            Session.Tone(390);
            return true;
        }

        public void TransferChest(bool fromPack, int slot, bool single)
        {
            if (!CurrentValid || OpenBlock != Block.StorageChest || Chest == null || !Session.Paused) return;
            Inventory source = fromPack ? Session.Player.Inventory : Chest;
            Inventory destination = fromPack ? Chest : Session.Player.Inventory;
            Slot item = source.At(slot);
            if (item.Count < 1) return;
            int count = single ? 1 : item.Count;
            if (CampState.Transfer(source, slot, destination, count)) {
                Status = (fromPack ? "Stored " : "Collected ") + count + " " + Items.Name(item.Item) + ".";
                Session.Tone(fromPack ? 330 : 440);
            } else Status = "There is not enough room for this stack. Shift-click to transfer one item.";
        }

        public void Smelt()
        {
            if (!CurrentValid || OpenBlock != Block.Furnace || !Session.Paused) return;
            if (CampState.TrySmelt(Session.Player.Inventory)) {
                Status = "Smelted 1 Iron. It is now in your pack.";
                Session.Tone(520);
            } else Status = "Smelting needs 2 Ore, 1 Wood and room for the finished Iron.";
        }

        public void BindRespawn()
        {
            if (!CurrentValid || OpenBlock != Block.Bed || !Session.Paused) return;
            if (Session.Mode == GameMode.Horror) { Status = "Eclipse runs do not use bed respawns."; return; }
            Vector3 position;
            if (!FindSafeBedPosition(OpenCell, out position)) {
                Status = "Clear a standing space beside this bed: solid floor and two air blocks above it.";
                return;
            }
            Session.CampData.HasRespawn = true;
            Session.CampData.RespawnCell = OpenCell;
            Status = "Home set. Your next respawn will be beside this bed while the space remains clear.";
            Session.Tone(660);
        }

        public bool CanBreak(Cell cell, out string reason)
        {
            reason = "";
            if (!Session || Session.CampData == null) return true;
            Inventory chest;
            if (!Session.CampData.Chests.TryGetValue(cell, out chest)) return true;
            for (int i = 0; i < Inventory.Capacity; i++) if (chest.At(i).Count > 0) {
                reason = "Empty this storage chest before mining it. Aim at it and press V.";
                return false;
            }
            return true;
        }

        public void OnRemoved(Cell cell)
        {
            if (!Session || Session.CampData == null) return;
            Session.CampData.RemoveChest(cell);
            if (Session.CampData.HasRespawn && Session.CampData.RespawnCell.Equals(cell)) {
                Session.CampData.HasRespawn = false;
                Session.Notify("Your home bed was removed. Respawning will use a safe world location.");
            }
        }

        public bool TryRespawn(out Vector3 position)
        {
            position = Vector3.zero;
            return Session && Session.CampData != null && Session.CampData.HasRespawn &&
                FindSafeBedPosition(Session.CampData.RespawnCell, out position);
        }

        private bool FindSafeBedPosition(Cell bed, out Vector3 position)
        {
            position = Vector3.zero;
            if (!Session.World || Session.World.Data.Get(bed.X, bed.Y, bed.Z) != Block.Bed) return false;
            // Read block data rather than Physics so a distant, unloaded home can be used safely.
            for (int radius = 1; radius <= 2; radius++)
                for (int dz = -radius; dz <= radius; dz++) for (int dx = -radius; dx <= radius; dx++) {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)) != radius) continue;
                    int x = bed.X + dx, z = bed.Z + dz;
                    if (Mathf.Abs(x) >= WorldData.Limit - 1 || Mathf.Abs(z) >= WorldData.Limit - 1) continue;
                    for (int offset = 0; offset < 3; offset++) {
                        int y = bed.Y + (offset == 0 ? 0 : offset == 1 ? 1 : -1);
                        if (y < 2 || y + 1 >= CubeFrontier.Core.Terrain.Height) continue;
                        var data = Session.World.Data;
                        if (!Blocks.Solid(data.Get(x, y - 1, z)) || data.Get(x, y, z) != Block.Air ||
                            data.Get(x, y + 1, z) != Block.Air) continue;
                        Vector3 candidate = new Vector3(x + .5f, y + .05f, z + .5f);
                        if (Physics.CheckBox(candidate + Vector3.up * .9f, new Vector3(.33f, .9f, .33f),
                            Quaternion.identity, 1 << 8, QueryTriggerInteraction.Ignore)) continue;
                        position = candidate;
                        return true;
                    }
                }
            return false;
        }
    }
}
