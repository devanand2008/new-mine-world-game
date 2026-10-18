using System.IO;
using CubeFrontier.Core;
// Isolated browser-test fixture: a level construction platform, never a user save.
class MakeBuildingFixture
{
    static void Main(string[] args)
    {
        var world=new WorldData(714);
        for(int x=-14;x<=14;x++)for(int z=-4;z<=32;z++){
            world.Set(new Cell(x,44,z),Block.SmoothStone);
            for(int y=45;y<63;y++)world.Set(new Cell(x,y,z),Block.Air);
        }
        var inv=new Inventory();inv.Add((int)Block.OakPlanks,64);inv.Add((int)Block.Glass,64);inv.Add((int)Block.ClayBricks,64);
        var save=new WorldSave {Seed=714,Mode=GameMode.Creative,X=.5f,Y=45.1f,Z=.5f,Pitch=12,Day=.39f,Inventory=inv.Export()};
        foreach(var edit in world.Edits)save.Edits.Add(edit);
        File.WriteAllBytes(args[0],SaveCodec.Encode(save));
    }
}
