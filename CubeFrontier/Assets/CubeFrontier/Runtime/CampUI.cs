using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    public sealed partial class FrontierUI
    {
        private void CampPanel()
        {
            CampStations camp = Session.CampStations;
            Card(new Rect(55, 110, 1170, 610), panel);
            if (!camp || !camp.CurrentValid) {
                Label(87, 145, 1070, 48, "THIS CAMP STATION IS UNAVAILABLE", heading, cyan);
                Label(87, 210, 1050, 60, "Return to the world and aim at a placed chest, furnace or bed, then press V.", body, muted);
                if (Button(87, 650, 280, 44, "Return to expedition", cyan)) Session.Resume();
                return;
            }

            Label(87, 139, 1060, 48, camp.OpenBlock == Block.StorageChest ? "YOUR CAMP STORAGE" :
                camp.OpenBlock == Block.Furnace ? "THE CAMP FURNACE" : "A PLACE TO CALL HOME", heading, cyan);
            string location = "CAMP AT  " + camp.OpenCell.X + " / " + camp.OpenCell.Y + " / " + camp.OpenCell.Z;
            Label(88, 193, 1050, 30, location, small, orange);
            if (camp.OpenBlock == Block.StorageChest) CampChest(camp);
            else if (camp.OpenBlock == Block.Furnace) CampFurnace(camp);
            else CampBed(camp);
            if (!string.IsNullOrEmpty(camp.Status)) Label(88, 573, 1090, 62, camp.Status, body, cyan);
            if (Button(87, 650, 280, 44, "Return to expedition", cyan)) Session.Resume();
            Label(398, 662, 790, 29, "V or Esc closes this station. Save from the pause menu to keep your progress.", small, muted);
        }

        private void CampChest(CampStations camp)
        {
            Label(87, 237, 520, 30, "YOUR PACK  /  CLICK TO STORE", body, textInk);
            Label(675, 237, 520, 30, "CHEST  /  CLICK TO COLLECT", body, textInk);
            CampGrid(Session.Player.Inventory, camp, true, 87, 282);
            CampGrid(camp.Chest, camp, false, 675, 282);
            Label(88, 478, 1090, 35, GUI.tooltip, body, textInk);
            Label(88, 521, 1090, 46, "Click transfers the whole stack; Shift-click transfers one. Empty the chest before mining it. Stored items keep their durability.", small, muted);
        }

        private void CampGrid(Inventory inventory, CampStations camp, bool fromPack, float x0, float y0)
        {
            for (int i = 0; i < Inventory.Capacity; i++) {
                Slot item = inventory.At(i);
                float x = x0 + i % 9 * 58, y = y0 + i / 9 * 58;
                Rect rect = new Rect(x, y, 53, 53);
                Card(rect, rect.Contains(Event.current.mousePosition) ? new Color(.16f, .32f, .36f) : ink);
                string hint = item.Count > 0 ? Items.Name(item.Item) + "  x" + item.Count : "Empty slot";
                if (item.Count > 0 && Items.Durability(item.Item) > 0) hint += "  /  Durability " + item.Durability + "/" + Items.Durability(item.Item);
                if (GUI.Button(rect, new GUIContent("", hint), GUIStyle.none)) camp.TransferChest(fromPack, i, Event.current.shift);
                if (item.Count > 0) {
                    DrawItem(new Rect(x + 10, y + 6, 33, 33), item.Item);
                    Label(x + 5, y + 33, 45, 19, item.Count.ToString(), small, textInk);
                }
            }
        }

        private void CampFurnace(CampStations camp)
        {
            Inventory pack = Session.Player.Inventory;
            Label(88, 237, 1090, 50, "Turn mined ore into Iron for stronger equipment and building materials.", body, textInk);
            CampIngredient(149, 305, (int)Block.Ore, "2 ORE", pack.Count((int)Block.Ore));
            Label(359, 337, 60, 50, "+", heading, muted);
            CampIngredient(439, 305, (int)Block.Wood, "1 WOOD / FUEL", pack.Count((int)Block.Wood));
            Label(670, 337, 100, 50, "TO", heading, cyan);
            CampIngredient(795, 305, (int)Block.Iron, "1 IRON", pack.Count((int)Block.Iron));
            bool ready = pack.Count((int)Block.Ore) >= 2 && pack.Count((int)Block.Wood) >= 1;
            bool wasEnabled = GUI.enabled; GUI.enabled = wasEnabled && ready;
            if (Button(438, 465, 370, 49, "Smelt one Iron", ready ? cyan : muted)) camp.Smelt();
            GUI.enabled = wasEnabled;
            Label(88, 532, 1090, 34, "Each use consumes the ingredients from your pack and adds the finished Iron immediately.", small, muted);
        }

        private void CampIngredient(float x, float y, int item, string label, int count)
        {
            Card(new Rect(x, y, 208, 129), ink);
            DrawItem(new Rect(x + 72, y + 11, 58, 58), item);
            Label(x + 15, y + 74, 190, 26, label, small, textInk);
            Label(x + 15, y + 100, 190, 24, "In your pack: " + count, small, muted);
        }

        private void CampBed(CampStations camp)
        {
            DrawItem(new Rect(105, 282, 134, 134), (int)Block.Bed);
            Label(283, 275, 850, 94, "Choose this bed as your home. If you fall in Survival, you will return to a clear standing space nearby.", body, textInk);
            bool bound = Session.CampData.HasRespawn && Session.CampData.RespawnCell.Equals(camp.OpenCell);
            Label(283, 381, 850, 51, bound ? "This is your current home bed." : "This bed is not your current respawn point.", body, bound ? cyan : muted);
            bool wasEnabled = GUI.enabled; GUI.enabled = wasEnabled && Session.Mode != GameMode.Horror;
            if (Button(283, 456, 425, 49, bound ? "Confirm this home" : "Set this bed as home", cyan)) camp.BindRespawn();
            GUI.enabled = wasEnabled;
            Label(88, 532, 1090, 37, Session.Mode == GameMode.Horror ? "Eclipse runs do not use bed respawns." : "Keep space beside the bed clear. A missing or obstructed bed uses a safe world spawn instead.", small, muted);
        }
    }
}
