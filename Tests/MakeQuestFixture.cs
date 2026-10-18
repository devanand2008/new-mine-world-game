using System.IO;
using CubeFrontier.Core;
// Controlled scenarios only; written into a disposable browser profile, never a user save.
class MakeQuestFixture
{
    static void Main(string[] args)
    {
        var world=new WorldData(714);
        for(int x=-12;x<=20;x++)for(int z=-12;z<=20;z++)for(int y=1;y<64;y++)world.Set(new Cell(x,y,z),y<=44?Block.Stone:Block.Air);
        var inventory=new Inventory();inventory.Add(Items.Pickaxe,1);inventory.Add((int)Block.Wood,12);inventory.Add((int)Block.Stone,12);inventory.Add(Items.Ration,4);
        var save=new WorldSave {Seed=714,Mode=GameMode.Survival,X=5.5f,Y=45.1f,Z=-2.5f,Character=4,Inventory=inventory.Export(),Day=.39f};
        foreach(var edit in world.Edits)save.Edits.Add(edit);
        File.WriteAllBytes(Path.Combine(args[0],"quest-start-v4.cfe"),SaveCodec.Encode(save));
        save.Quest=FrontierQuest.Restore(4,8,12,7);
        File.WriteAllBytes(Path.Combine(args[0],"quest-ready-v4.cfe"),SaveCodec.Encode(save));
        save.Quest=new FrontierQuest();save.X=.5f;save.Z=.5f;
        world.Set(new Cell(0,46,3),Block.Wood);save.Edits.Clear();foreach(var edit in world.Edits)save.Edits.Add(edit);
        File.WriteAllBytes(Path.Combine(args[0],"quest-mine-v4.cfe"),SaveCodec.Encode(save));
    }
}
