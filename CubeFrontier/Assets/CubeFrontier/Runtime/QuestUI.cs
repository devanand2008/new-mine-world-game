using UnityEngine;
using CubeFrontier.Core;
namespace CubeFrontier
{
    public sealed partial class FrontierUI
    {
        void QuestLine(float y, bool done, string text) {RectFill(new Rect(85,y-3,655,39),done?new Color(.10f,.25f,.25f):ink);RectFill(new Rect(96,y+7,14,14),done?cyan:muted*.4f);Label(125,y,610,40,text,body,done?cyan:textInk);}
        void Journal() {
            Card(new Rect(55,105,1170,620),panel);RectFill(new Rect(766,184,425,458),ink);Label(85,130,1100,45,"YOUR FIELD JOURNAL",heading,cyan);
            var q=Session.Quest;var player=Session.Player;
            if(Session.Mode==GameMode.Survival) {
                Label(90,185,660,55,"FIRST FRONTIER / Mara's commission\nEstablish a foothold. Protect it through the night.",body,textInk);
                QuestLine(254,q.Wood>=4,"Mine timber  "+q.Wood+" / 4");
                QuestLine(301,q.Stone>=8,"Mine stone  "+q.Stone+" / 8");
                QuestLine(348,q.Crafted,"Craft a tool using E > Field recipes");
                QuestLine(395,q.Sheltered,"Build shelter  "+q.Placed+" / 12 blocks placed");
                Label(116,433,580,48,"Stand beneath a roof (2-4 blocks up), with two-block-high walls on three sides, within 3 blocks.",small,muted);
                QuestLine(492,q.NightVictory,"Defeat a creature at night");
                Label(90,546,650,44,q.Claimed?"Commission complete. Reward claimed: 8 crystals.":q.Ready?"Return to Mara for your 8-crystal reward.":"After building shelter, Mara can start the night watch.",body,q.Claimed||q.Ready?cyan:muted);
                if(Session.Merchant) {var p=Session.Merchant.transform.position;Label(90,603,660,42,"Mara: "+Vector3.Distance(player.transform.position,p).ToString("0")+"m  |  X "+p.x.ToString("0")+"  Z "+p.z.ToString("0")+"  |  T nearby to trade",small,orange);}
            } else Label(90,260,630,110,"Mara's First Frontier commission is available in Survival worlds. Your equipment can be managed here in every mode.",body,muted);
            Label(790,195,390,35,"EQUIPPED  /  Armour "+player.Equipment.TotalArmor().ToString("0"),body,cyan);
            string[] names={"Head","Chest","Legs","Shield"};
            for(int i=0;i<4;i++) {var s=player.Equipment.At(i);float y=246+i*67;
                Label(790,y,390,27,names[i]+": "+(s.Count==0?"Not equipped":Items.Name(s.Item)+"  "+s.Durability),small,textInk);
                if(s.Count>0&&Button(790,y+28,370,30,"Unequip to pack",muted))Session.Notify(player.Inventory.Unequip(i,player.Equipment)?"Equipment returned to pack.":"Make an empty pack slot first.");
            }
            var held=player.Inventory.At(player.Selected);
            Label(790,527,380,49,"Selected hotbar: "+Items.Name(held.Item)+"\nChoose equipment in your pack with E.",small,muted);
            if(Button(790,586,370,43,"Equip selected hotbar item",cyan))Session.Notify(player.Inventory.EquipAt(player.Selected,player.Equipment)?"Equipment fitted. Previous item returned to its pack slot.":"Select armour or a shield in your hotbar first.");
            if(Button(90,662,300,42,"Return to expedition  [J]",cyan))Session.Resume();
            if(Button(408,662,300,42,"Open pack & crafting",muted)){JournalOpen=false;InventoryOpen=true;}
        }
        void Trade() {
            Card(new Rect(180,105,920,620),panel);Label(215,133,840,45,"MARA'S FRONTIER EXCHANGE",heading,cyan);
            Label(215,186,840,50,"A few supplies for the road? Trade your gathered materials for food, ore and dependable equipment.",body,muted);
            if(!Session.Merchant||!Session.Merchant.CanTrade){Label(215,275,800,70,"Mara is out of reach. Return to the expedition and approach her.",body,orange);}
            else {
                for(int i=0;i<MerchantOffers.All.Length;i++) {
                    Recipe r=MerchantOffers.All[i];var input=r.Inputs[0];float y=260+i*65;
                    Card(new Rect(210,y-4,54,52),ink);
                    DrawItem(new Rect(218,y,40,40),r.Output);
                    string text="Give "+input.Count+" "+Items.Name(input.Item)+"  >  Receive "+r.Count+" "+Items.Name(r.Output);
                    if(Button(280,y,770,44,text,Session.Player.Inventory.Count(input.Item)>=input.Count?cyan:muted))Session.Notify(Session.Player.Inventory.Craft(r)?"Trade complete: "+r.Count+" "+Items.Name(r.Output):"Trade unavailable: check ingredients and pack space.");
                }
                if(Button(215,536,835,45,Session.Quest.Claimed?"Commission reward already claimed":"Claim First Frontier reward: 8 crystals",Session.Quest.Ready&&!Session.Quest.Claimed?cyan:muted))Session.Notify(Session.Quest.Claim(Session.Player.Inventory)?"First Frontier complete! Eight crystals added to your pack.":"Complete every J objective and leave room for 8 crystals. Rewards can be claimed once.");
                if(Button(215,593,835,42,"Begin night watch after building shelter",orange)) {if(Session.Quest.Sheltered&&!Session.Quest.NightVictory)Session.NightWatch();else Session.Notify("Build and verify your shelter first. A completed night encounter cannot be repeated here.");}
            }
            if(Button(215,662,380,42,"Return to expedition",cyan))Session.Resume();
            if(Button(615,662,435,42,"Quest journal & equipment",muted)){MerchantOpen=false;JournalOpen=true;}
        }
    }
}
