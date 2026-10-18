using System.IO;
using CubeFrontier.Core;
// Only used in a disposable browser profile: a solid wall across a level runway.
class MakeFlightFixture
{
    static void Main(string[] args)
    {
        var world = new WorldData(714);
        for (int x=-12;x<=12;x++) for(int z=-8;z<=28;z++) {
            world.Set(new Cell(x,44,z),Block.SmoothStone);
            for(int y=45;y<64;y++) world.Set(new Cell(x,y,z),Block.Air);
        }
        for(int x=-10;x<=10;x++) for(int z=5;z<=7;z++) for(int y=45;y<=49;y++)
            world.Set(new Cell(x,y,z),Block.StoneBricks);
        var save = new WorldSave {Seed=714,Mode=GameMode.Creative,X=.5f,Y=45.1f,Z=.5f,Pitch=0,Day=.39f};
        var inventory = new Inventory(); inventory.Add((int)Block.StoneBricks,64); save.Inventory=inventory.Export();
        foreach(var edit in world.Edits)save.Edits.Add(edit);
        File.WriteAllBytes(args[0],SaveCodec.Encode(save));
    }
}
