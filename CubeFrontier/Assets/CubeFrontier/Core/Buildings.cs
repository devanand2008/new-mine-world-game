using System;
using System.Collections.Generic;
namespace CubeFrontier.Core
{
    public sealed class BuildingPlan
    {
        public readonly string Name, Description;
        public readonly int Width, Depth, Height;
        public readonly Dictionary<Cell,Block> Cells = new Dictionary<Cell,Block>();
        public BuildingPlan(string name,string description,int width,int depth,int height)
        {Name=name;Description=description;Width=width;Depth=depth;Height=height;}
        void Put(int x,int y,int z,Block b){Cells[new Cell(x,y,z)]=b;}
        public int RotatedWidth(int rotation){return rotation%2==0?Width:Depth;}
        public int RotatedDepth(int rotation){return rotation%2==0?Depth:Width;}
        public Cell Position(Cell local,Cell origin,int rotation)
        {
            int x=local.X,z=local.Z;
            switch((rotation%4+4)%4){case 1:x=Depth-1-local.Z;z=local.X;break;case 2:x=Width-1-local.X;z=Depth-1-local.Z;break;case 3:x=local.Z;z=Width-1-local.X;break;}
            return new Cell(origin.X+x,origin.Y+local.Y,origin.Z+z);
        }
        public Dictionary<Block,int> Materials()
        {var result=new Dictionary<Block,int>();foreach(var b in Cells.Values){if(b==Block.Air)continue;if(!result.ContainsKey(b))result[b]=0;result[b]++;}return result;}
        public static readonly BuildingPlan[] All={House(false),House(true),GlassHouse(),Tower()};
        static BuildingPlan House(bool lodge)
        {
            int w=lodge?11:9,d=11;
            var p=new BuildingPlan(lodge?"Stone & timber lodge":"Meadow cottage",lodge?"Stone walls, dark beams, slate gable, library and carpeted hall.":"Cream plaster, oak beams, terracotta gable, windows and a furnished room.",w,d,lodge?11:10);
            Block wall=lodge?Block.StoneBricks:Block.WhitePlaster,beam=lodge?Block.DarkTimber:Block.Wood,roof=lodge?Block.SlateTiles:Block.RedRoof;
            for(int z=0;z<d;z++)for(int x=0;x<w;x++) {
                p.Put(x,0,z,Block.Cobblestone);
                if(x>0&&x<w-1&&z>0&&z<d-1)p.Put(x,0,z,lodge?Block.SprucePlanks:Block.OakPlanks);
                for(int y=1;y<=4;y++){
                    bool edge=x==1||x==w-2||z==1||z==d-2;
                    bool inside=x>=1&&x<=w-2&&z>=1&&z<=d-2;
                    if(!inside)continue;
                    Block b=edge?wall:Block.Air;
                    if(edge&&(y==4||((x==1||x==w-2)&&(z==1||z==d-2))))b=beam;
                    if(edge&&(y==2||y==3)&&((z==1||z==d-2)&&Math.Abs(x-w/2)>=2&&x>1&&x<w-2||(x==1||x==w-2)&&(z==4||z==5||z==6)))b=Block.Glass;
                    if(z==1&&x==w/2&&y<=3)b=Block.Air;
                    p.Put(x,y,z,b);
                }
                // Full-block stepped gable, including solid end walls below the roof.
                int rise=Math.Min(x,w-1-x),ry=5+rise;
                p.Put(x,ry,z,roof);
                if((z==1||z==d-2)&&x>0&&x<w-1)for(int y=5;y<ry;y++)p.Put(x,y,z,y==6&&x==w/2?Block.Glass:wall);
            }
            for(int y=1;y<=8;y++)p.Put(w-3,y,d-3,Block.ClayBricks);
            p.Put(w-3,9,d-3,Block.CarvedStone);
            for(int z=3;z<d-3;z++)p.Put(w/2,0,z,lodge?Block.BlueWool:Block.RedWool);
            p.Put(2,1,d-3,Block.Bookshelf);p.Put(3,1,d-3,Block.Bookshelf);p.Put(w-3,1,3,Block.Crate);
            p.Put(2,1,4,Block.OakPlanks);p.Put(2,2,4,Block.Lantern);
            p.Put(w/2-1,3,0,Block.Lantern);p.Put(w/2+1,3,0,Block.Lantern);
            return p;
        }
        static BuildingPlan GlassHouse()
        {
            var p=new BuildingPlan("Crystal conservatory","Copper frame, transparent walls, skylight and indoor planters.",9,9,7);
            for(int z=0;z<9;z++)for(int x=0;x<9;x++){
                p.Put(x,0,z,(x+z)%2==0?Block.Marble:Block.SmoothStone);
                for(int y=1;y<=4;y++){
                    bool edge=x==0||x==8||z==0||z==8;
                    Block b=edge?Block.Glass:Block.Air;
                    if(edge&&(y==4||((x==0||x==8)&&(z==0||z==8))))b=Block.Copper;
                    if(z==0&&x==4&&y<4)b=Block.Air;p.Put(x,y,z,b);
                }
                int roof=5+(x>1&&x<7?1:0);p.Put(x,roof,z,x%4==0||z%4==0?Block.Copper:Block.Glass);
            }
            for(int x=1;x<=7;x+=6)for(int z=2;z<=6;z+=2){p.Put(x,1,z,Block.Terracotta);p.Put(x,2,z,Block.Leaves);}
            p.Put(4,5,4,Block.Lantern);return p;
        }
        static BuildingPlan Tower()
        {
            var p=new BuildingPlan("Frontier watchtower","Stone tower with a climbable block staircase and open battlements.",7,7,14);
            for(int z=0;z<7;z++)for(int x=0;x<7;x++){
                p.Put(x,0,z,Block.Cobblestone);
                for(int y=1;y<=10;y++){
                    bool edge=x==0||x==6||z==0||z==6;
                    Block b=edge?(y%4==0?Block.CarvedStone:Block.StoneBricks):Block.Air;
                    if(edge&&((x==3&&(z==0||z==6))||(z==3&&(x==0||x==6)))&&(y==5||y==6))b=Block.Glass;
                    if(z==0&&x==3&&y<=3)b=Block.Air;p.Put(x,y,z,b);
                }
                if((x==0||x==6||z==0||z==6)&&((x+z)%2==0))p.Put(x,12,z,Block.StoneBricks);
                p.Put(x,11,z,Block.OakPlanks);
            }
            int[,] stairs={{2,1},{1,1},{1,2},{1,3},{1,4},{1,5},{2,5},{3,5},{4,5},{5,5},{5,4}};
            for(int i=0;i<stairs.GetLength(0);i++){p.Put(stairs[i,0],i+1,stairs[i,1],Block.SprucePlanks);if(i>=8)p.Put(stairs[i,0],11,stairs[i,1],Block.Air);}
            p.Put(3,12,3,Block.Lantern);return p;
        }
    }
    public sealed class BlockChange
    {
        public readonly Cell Cell;public readonly Block Before,After;
        public BlockChange(Cell cell,Block before,Block after){Cell=cell;Before=before;After=after;}
    }
    public static class BuildingPlacement
    {
        // Validate the entire plan before writing any cells: a red preview cannot partly destroy a site.
        public static bool TryPlan(WorldData world,BuildingPlan plan,Cell origin,int rotation,out List<BlockChange> changes,out string reason)
        {
            changes=new List<BlockChange>();reason="";
            int w=plan.RotatedWidth(rotation),d=plan.RotatedDepth(rotation);
            if(origin.Y<2||origin.Y+plan.Height>=Terrain.Height||Math.Abs((long)origin.X)+w>WorldData.Limit||Math.Abs((long)origin.Z)+d>WorldData.Limit){reason="Outside the world's building limits.";return false;}
            for(int x=0;x<w;x++)for(int z=0;z<d;z++){
                // Reserve the whole volume, including the interior and roof voids.
                for(int y=0;y<plan.Height;y++){
                    Block b=world.Get(origin.X+x,origin.Y+y,origin.Z+z);
                    if(b!=Block.Air&&b!=Block.Water){reason="Site occupied. Clear the outlined area or choose another site.";return false;}
                }
                int depth=0;
                while(depth<6&&!Blocks.Solid(world.Get(origin.X+x,origin.Y-1-depth,origin.Z+z)))depth++;
                if(depth==6){reason="Ground is too uneven. Build a level platform first.";return false;}
                for(int j=1;j<=depth;j++){var c=new Cell(origin.X+x,origin.Y-j,origin.Z+z);changes.Add(new BlockChange(c,world.Get(c.X,c.Y,c.Z),Block.Cobblestone));}
            }
            foreach(var entry in plan.Cells){Cell c=plan.Position(entry.Key,origin,rotation);Block before=world.Get(c.X,c.Y,c.Z);if(before!=entry.Value)changes.Add(new BlockChange(c,before,entry.Value));}
            if(world.EditCount+changes.Count>240000){reason="This world is close to its save edit limit.";changes.Clear();return false;}
            return true;
        }
        public static bool CanUndo(WorldData world,List<BlockChange> changes)
        {if(changes==null||changes.Count==0)return false;foreach(var c in changes)if(world.Get(c.Cell.X,c.Cell.Y,c.Cell.Z)!=c.After)return false;return true;}
    }
}
