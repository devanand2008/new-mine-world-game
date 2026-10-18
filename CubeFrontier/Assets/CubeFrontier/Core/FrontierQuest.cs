using System;
namespace CubeFrontier.Core
{
    // Event-driven world progression. Starter inventory and generated houses never count.
    public sealed class FrontierQuest
    {
        public int Wood, Stone, Placed;
        public bool Crafted, Sheltered, NightVictory, Claimed;
        public bool Ready { get { return Wood >= 4 && Stone >= 8 && Crafted && Sheltered && NightVictory; } }
        public void Mine(Block block) { if (block == Block.Wood) Wood = Math.Min(4, Wood + 1); if (block == Block.Stone) Stone = Math.Min(8, Stone + 1); }
        public void Craft(int item) { if (Items.Tool(item)) Crafted = true; }
        public void Place() { Placed = Math.Min(12, Placed + 1); }
        public bool Claim(Inventory pack) { if (Claimed || !Ready || !pack.Add((int)Block.Crystal, 8)) return false; Claimed = true; return true; }
        public int Flags { get { return (Crafted?1:0)|(Sheltered?2:0)|(NightVictory?4:0)|(Claimed?8:0); } }
        public static FrontierQuest Restore(int wood, int stone, int placed, int flags)
        {
            if (wood<0||wood>4||stone<0||stone>8||placed<0||placed>12||flags<0||flags>15) throw new ArgumentException("Invalid quest progress");
            var q = new FrontierQuest { Wood=wood, Stone=stone, Placed=placed, Crafted=(flags&1)!=0, Sheltered=(flags&2)!=0, NightVictory=(flags&4)!=0, Claimed=(flags&8)!=0 };
            if ((q.Sheltered && placed<12)||(q.Claimed&&!q.Ready)) throw new ArgumentException("Inconsistent quest progress");
            return q;
        }
        public static bool IsShelter(WorldData world, Cell feet)
        {
            bool roof=false; for(int y=2;y<=4;y++) if(Blocks.Solid(world.Get(feet.X,feet.Y+y,feet.Z))) {roof=true;break;}
            if(!roof) return false;
            int walls=0; int[] dx={1,-1,0,0}, dz={0,0,1,-1};
            for(int i=0;i<4;i++) for(int d=1;d<=3;d++) {
                if(Blocks.Solid(world.Get(feet.X+dx[i]*d,feet.Y,feet.Z+dz[i]*d)) && Blocks.Solid(world.Get(feet.X+dx[i]*d,feet.Y+1,feet.Z+dz[i]*d))) {walls++;break;}
            }
            return walls>=3;
        }
    }
    public static class MerchantOffers
    {
        public static readonly Recipe[] All = {
            new Recipe(Items.Ration, 2, new Ingredient((int)Block.Wood, 4)),
            new Recipe((int)Block.Ore, 3, new Ingredient((int)Block.Stone, 8)),
            new Recipe(Items.LeatherChest, 1, new Ingredient((int)Block.Crystal, 4)),
            new Recipe(Items.IronSword, 1, new Ingredient((int)Block.Crystal, 8))
        };
    }
}
