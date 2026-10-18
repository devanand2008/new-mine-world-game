using System;
using System.Collections.Generic;
using System.Linq;
using CubeFrontier.Core;

public static class CoreTests
{
    private static int passed;
    private static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("PASS: " + name); }
    private static void Reject(Action action, string name) { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } catch (System.IO.InvalidDataException) { rejected = true; } Check(rejected, name); }
    public static void Main()
    {
        Check(ChunkKey.At(-1, -17).Equals(new ChunkKey(-1, -2)), "negative coordinates floor correctly");
        Check(ChunkKey.At(-16, 16).Equals(new ChunkKey(-1, 1)), "exact chunk boundaries");
        var a = new Terrain(714); var b = new Terrain(714);
        byte[] first = a.Generate(new ChunkKey(-2, 1));
        Check(first.SequenceEqual(b.Generate(new ChunkKey(-2, 1))), "seed determinism");
        Check(!first.SequenceEqual(new Terrain(715).Generate(new ChunkKey(-2, 1))), "different seeds differ");
        Check(first.Length == 16384 && first.All(v => v < Blocks.Count), "valid chunk dimensions and blocks");
        var world = new WorldData(714); var cell = new Cell(-1, 50, -17);
        Check(world.Set(cell, Block.Crystal) && world.Get(-1, 50, -17) == Block.Crystal, "edit at negative coordinate");
        world.EvictOutside(new ChunkKey(30, 30), 1);
        Check(world.CachedChunks == 0 && world.Get(-1, 50, -17) == Block.Crystal, "edits survive chunk eviction");
        Check(world.Set(cell, Block.Air) && world.EditCount == 0, "reverting to procedural block removes delta");
        Check(!world.Set(new Cell(0, 0, 0), Block.Air) && !world.Set(new Cell(0, 64, 0), Block.Wood), "world vertical bounds");
        Check(!world.Set(new Cell(int.MinValue, 2, 0), Block.Wood), "hostile coordinate does not overflow");
        Check(Mesher.Faces((x,y,z) => x == 0 && y == 0 && z == 0 ? Block.Stone : Block.Air, 1, 1, 1).Count == 6, "isolated voxel has six faces");
        Check(Mesher.Faces((x,y,z) => x >= 0 && x < 2 && y == 0 && z == 0 ? Block.Stone : Block.Air, 2, 1, 1).Count == 10, "internal faces culled");
        Check(Mesher.Faces((x,y,z) => x >= 0 && x < 2 && y == 0 && z == 0 ? Block.Stone : Block.Air, 1, 1, 1).Count == 5, "neighbor chunk occludes border face");
        var inv = new Inventory();
        Check(inv.Add((int)Block.Wood, 65) && inv.At(0).Count == 64 && inv.At(1).Count == 1, "stack overflow splits slots");
        Check(!inv.Add((int)Block.Wood, 2000) && inv.Count((int)Block.Wood) == 65, "full inventory rejects atomically");
        Check(!inv.Remove((int)Block.Wood, 66) && inv.Count((int)Block.Wood) == 65, "insufficient removal is atomic");
        Check(!inv.Add(-1, 2) && !inv.Add(Items.Sword, -1), "invalid item and quantity rejected");
        Check(!inv.Craft(Recipes.All[0]) && inv.Count((int)Block.Wood) == 65, "missing ingredients preserve resources");
        inv.Add((int)Block.Stone, 5);
        Check(inv.Craft(Recipes.All[0]) && inv.Count(Items.Pickaxe) == 1 && inv.Count((int)Block.Wood) == 62, "craft transaction");
        int pickSlot = Enumerable.Range(0, Inventory.Capacity).First(i => inv.At(i).Item == Items.Pickaxe);
        for (int i = 0; i < 100; i++) inv.Wear(pickSlot);
        Check(inv.Count(Items.Pickaxe) == 0, "tool breaks after durability exhausted");
        var full = new Inventory(); full.Add((int)Block.Soil, 26 * 64); full.Add((int)Block.Wood, 64);
        var before = full.Export();
        Check(!full.Craft(new Recipe(Items.Sword, 1, new Ingredient((int)Block.Wood, 1))) && full.Count((int)Block.Wood) == 64, "output capacity failure restores ingredients");
        var invalid = inv.Export(); invalid[0].Count = 65;
        Reject(() => inv.Restore(invalid), "corrupt inventory rejected");
        var save = new WorldSave { Seed = 714, X = -3, Y = 51, Z = -17, Inventory = inv.Export() };
        save.Edits.Add(new KeyValuePair<Cell, Block>(cell, Block.Crystal));
        byte[] encoded = SaveCodec.Encode(save); WorldSave decoded = SaveCodec.Decode(encoded);
        Check(decoded.Seed == save.Seed && decoded.Edits[0].Key.Equals(cell) && decoded.Inventory[0].Count == inv.At(0).Count, "binary save round trip");
        Check(encoded.SequenceEqual(SaveCodec.Encode(decoded)), "canonical save serialization");
        var restored = new WorldData(decoded.Seed); foreach (var edit in decoded.Edits) restored.Set(edit.Key, edit.Value);
        Check(restored.Get(-1, 50, -17) == Block.Crystal, "saved delta restores modified world");
        encoded[25] ^= 1; Reject(() => SaveCodec.Decode(encoded), "tampered save rejected");
        Reject(() => SaveCodec.Decode(new byte[20]), "truncated save rejected");
        save.X = float.NaN; Reject(() => SaveCodec.Encode(save), "non-finite player coordinate rejected"); save.X = 0;
        save.Edits.Add(save.Edits[0]); Reject(() => SaveCodec.Encode(save), "duplicate edits rejected");
        Check(Enumerable.Range(1,Blocks.Count-1).Count(Blocks.Placeable)==57 && Blocks.PlaceableCount==57, "57 selectable building materials");
        Check(Mesher.Faces((x,y,z) => y==0&&z==0&&x>=0&&x<2?Block.Glass:Block.Air,2,1,1).Count==10,"glass removes internal faces while retaining solid collision classification");
        Check(Blocks.Solid(Block.Glass)&&!Blocks.Occludes(Block.Glass)&&Blocks.Emits(Block.Lantern),"transparent windows and emissive lantern properties");
        Check(Mesher.Faces((x,y,z)=>y==0&&z==0&&x==0?Block.Stone:y==0&&z==0&&x==1?Block.Glass:Block.Air,1,1,1).Count==6,"stone remains visible behind a glass neighbour");
        Check(Enumerable.Range(1,Blocks.Count-1).All(id=>Enumerable.Range(0,6).All(face=>Blocks.Tile((Block)id,face)>=0&&Blocks.Tile((Block)id,face)<Blocks.AtlasColumns*Blocks.AtlasRows)),"every block face has a valid atlas tile");
        var creative=new Inventory();creative.Add(Items.Pickaxe,1);
        Check(creative.SelectCreative(0,(int)Block.Glass)&&creative.At(0).Item==(int)Block.Glass&&creative.At(0).Count==64&&creative.At(0).Durability==0,"creative palette replaces a selected hotbar slot cleanly");
        Check(!creative.SelectCreative(9,(int)Block.Glass)&&!creative.SelectCreative(0,(int)Block.Bedrock),"creative selection rejects invalid slot and unplaceable material");
        Check(Enumerable.Range(14,32).All(id=>Recipes.All.Any(r=>r.Output==id)),"all construction materials have survival recipes");
        var buildWorld=new WorldData(714);var plan=BuildingPlan.All[0];var origin=new Cell(-4,50,-8);
        for(int x=0;x<plan.Width;x++)for(int z=0;z<plan.Depth;z++)buildWorld.Set(new Cell(origin.X+x,49,origin.Z+z),Block.Stone);
        List<BlockChange> changes;string reason;
        Check(BuildingPlacement.TryPlan(buildWorld,plan,origin,0,out changes,out reason)&&changes.Count>100,"house plan accepts a clear supported site across negative chunk boundaries");
        int beforePlan=buildWorld.EditCount;
        Check(!BuildingPlacement.TryPlan(buildWorld,plan,new Cell(0,60,0),0,out changes,out reason)&&buildWorld.EditCount==beforePlan,"out-of-height house placement is atomic");
        var obstruct=new Cell(origin.X+3,origin.Y+2,origin.Z+3);buildWorld.Set(obstruct,Block.Gold);int occupiedCount=buildWorld.EditCount;
        Check(!BuildingPlacement.TryPlan(buildWorld,plan,origin,0,out changes,out reason)&&buildWorld.EditCount==occupiedCount&&buildWorld.Get(obstruct.X,obstruct.Y,obstruct.Z)==Block.Gold,"house rejects occupied interiors without overwriting player blocks");
        buildWorld.Set(obstruct,Block.Air);BuildingPlacement.TryPlan(buildWorld,plan,origin,0,out changes,out reason);
        foreach(var c in changes)buildWorld.Set(c.Cell,c.After);
        Check(BuildingPlacement.CanUndo(buildWorld,changes),"freshly placed house can be undone");
        var firstChange=changes.First(c=>c.After==Block.WhitePlaster);buildWorld.Set(firstChange.Cell,Block.Glass);
        Check(!BuildingPlacement.CanUndo(buildWorld,changes),"undo refuses to erase subsequent manual edits");
        buildWorld.Set(firstChange.Cell,firstChange.After);foreach(var c in changes)buildWorld.Set(c.Cell,c.Before);
        Check(buildWorld.EditCount==beforePlan,"undo restores original site including sparse edit count");
        foreach(var p in BuildingPlan.All){
            Check(p.Cells.All(e=>e.Key.X>=0&&e.Key.X<p.Width&&e.Key.Z>=0&&e.Key.Z<p.Depth&&e.Key.Y>=0&&e.Key.Y<p.Height),p.Name+" fits its advertised bounds");
            for(int r=0;r<4;r++){var positions=p.Cells.Keys.Select(c=>p.Position(c,new Cell(0,0,0),r)).ToList();Check(positions.Distinct().Count()==positions.Count&&positions.All(c=>c.X>=0&&c.X<p.RotatedWidth(r)&&c.Z>=0&&c.Z<p.RotatedDepth(r)),p.Name+" rotation "+r+" preserves unique in-bounds cells");}
        }
        var modernSave=new WorldSave {Seed=714,Inventory=creative.Export()};
        foreach(var c in plan.Cells)if(c.Value!=Block.Air)modernSave.Edits.Add(new KeyValuePair<Cell,Block>(plan.Position(c.Key,origin,0),c.Value));
        var modernRoundTrip=SaveCodec.Decode(SaveCodec.Encode(modernSave));
        Check(modernRoundTrip.Edits.Count==modernSave.Edits.Count&&modernRoundTrip.Edits.Any(e=>e.Value==Block.Glass)&&modernRoundTrip.Inventory[0].Item==(int)Block.Glass,"houses and new block inventory persist in the compatible binary format");
        var survival=new Inventory();survival.Add(Items.Ration,4);survival.Add(Items.Arrow,16);survival.Add(Items.IronPickaxe,1);
        var survivalSave=new WorldSave {Inventory=survival.Export(),Experience=17};
        survivalSave.Equipment[0]=new Slot {Item=Items.IronHelm,Count=1,Durability=100};
        var survivalRestored=SaveCodec.Decode(SaveCodec.Encode(survivalSave));
        Check(survivalRestored.Inventory[0].Count==4&&survivalRestored.Inventory[1].Count==16,"food and ammunition stack and save without tool durability");
        Check(survivalRestored.Equipment[0].Durability==100&&survivalRestored.Experience==17,"equipment durability and experience round trip");
        for(int i=0;i<200;i++)survival.Wear(2);
        Check(survival.At(2).Count==0&&!Items.Valid(119)&&!Items.Valid(129),"iron tools wear out and undefined item IDs are rejected");
        survivalSave.Equipment[0].Item=Items.IronChest;
        Reject(()=>SaveCodec.Encode(survivalSave),"equipment in the wrong body slot is rejected");survivalSave.Equipment[0].Item=Items.IronHelm;
        survivalSave.GeneratorVersion=3;Reject(()=>SaveCodec.Encode(survivalSave),"unknown terrain generator is rejected");survivalSave.GeneratorVersion=2;
        var legacy=SaveCodec.Decode(System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"../Tests/Fixtures/platform-v1.cfe")));
        Check(legacy.GeneratorVersion==1&&legacy.Inventory[0].Item==(int)Block.OakPlanks,"actual version-1 fixture loads with original terrain and inventory");
        Check(SaveCodec.Decode(SaveCodec.Encode(legacy)).GeneratorVersion==1,"saving an older world retains its original generator");
        // Golden hashes captured from the pre-expansion compiled generator, not from this implementation.
        int[] seeds={714,-912,2076,42};int[,] coords={{0,0},{-2,1},{5,-8},{20,20}};
        string[] hashes={
            "12AF03F6BF2B4DD4F396222D433FFF6B600ECCDE1BDE9DFE51E175ED1C1FF48C","C80E01A8EB64EB753B508BC8052ECFFFBE58DC2C6DDDAF6F3DE60EA7A1847D18","B8DD5F35FB04D14FCA7BDBF11884007504144C4E8FFAA1B5C03107201BFA1E79","673865EE89D019F3414EEC240EB5912728BAA0039BEAF658953B726202BE2208",
            "21E5218A71C4032C41177B75B7F39579427BCE1877355849A1270009B308F80D","80316AD418934E76099476033B4C01B267AC8AC83BDDE2094A1C71D597EA1EC6","805662389888C6D10B67C8F69BBE157850C6C16FBDA9B40E45B8574197EE705C","D7B864E1848826685385397C8FBCC70626F4576EBDA3D28621D14C847AE4B0C8",
            "B1571AAD4DA9F28CCB695C519DB3D837890DC05A7BB380CA1FB03E29534F475E","EB4E59391A763D0E38F2019689B7E7CBA8F063E353DC36B493C64A8E255E4BFF","8BB0AD678077536CD3A9FB9F56B16CAC6EB4205FACF2FE7F066DC51D1C1DCDDF","456533DC06F37645424EC57799D1F0D46EC427386645BD6E2B88146C27EF4232",
            "E98FDEB9FEB53715CF74E9E339D37CB984EE3FC2489D1A5271FF37E636BBF3B5","266FBCE291924BF9A27F2BB6BEFAF25C26BB1D653340BDBA7B09AF44B36EEE89","598256FBB56220AD2A6F0C99DEBCEAD5D6E15AB1F48BA3C9FC7C718B1EFE8177","6F128E8A7AC81F63961A4BCBA007C6939877D1A71E266FB88270AFFB91C10680"};
        using(var sha=System.Security.Cryptography.SHA256.Create())for(int i=0;i<seeds.Length;i++)for(int j=0;j<4;j++){
            byte[] chunk=new Terrain(seeds[i],1).Generate(new ChunkKey(coords[j,0],coords[j,1]));
            Check(BitConverter.ToString(sha.ComputeHash(chunk)).Replace("-","")==hashes[i*4+j],"legacy terrain matches released generator: "+seeds[i]+" / "+j);
        }
        var quest=new FrontierQuest();var rewardPack=new Inventory();
        Check(!quest.Claim(rewardPack),"unfinished commission cannot award crystals");
        for(int i=0;i<20;i++){quest.Mine(Block.Wood);quest.Mine(Block.Stone);quest.Place();}
        Check(quest.Wood==4&&quest.Stone==8&&quest.Placed==12,"quest counters cap and track actual resource event types");
        quest.Craft((int)Block.OakPlanks);Check(!quest.Crafted,"building materials do not count as crafted tools");
        quest.Craft(Items.Axe);quest.Sheltered=true;quest.NightVictory=true;
        Check(quest.Ready&&quest.Claim(rewardPack)&&rewardPack.Count((int)Block.Crystal)==8,"completed commission grants real crystals");
        Check(!quest.Claim(rewardPack)&&rewardPack.Count((int)Block.Crystal)==8,"quest reward cannot be duplicated");
        var qs=new WorldSave {Character=12,Quest=quest};var qr=SaveCodec.Decode(SaveCodec.Encode(qs));
        Check(qr.Character==12&&qr.Quest.Claimed&&qr.Quest.Ready,"world character and claimed quest survive reload");
        Reject(()=>FrontierQuest.Restore(0,0,0,8),"invalid claimed quest is rejected");
        Reject(()=>FrontierQuest.Restore(4,8,0,2),"shelter without placement progress is rejected");
        var trader=new Inventory();trader.Add((int)Block.Wood,8);
        Check(trader.Craft(MerchantOffers.All[0])&&trader.Count((int)Block.Wood)==4&&trader.Count(Items.Ration)==2,"merchant trade consumes exact price and delivers supplies");
        Check(!trader.Craft(MerchantOffers.All[3])&&trader.Count((int)Block.Wood)==4,"unaffordable exchange leaves pack intact");
        var rewardFull=new Inventory();for(int i=0;i<Inventory.Capacity;i++)rewardFull.Add(Items.Pickaxe,1);
        var almost=FrontierQuest.Restore(4,8,12,7);
        Check(!almost.Claim(rewardFull)&&!almost.Claimed,"full pack retains unclaimed reward for retry");
        var armourPack=new Inventory();armourPack.Add(Items.LeatherChest,1);var worn=armourPack.Export();worn[0].Durability=71;armourPack.Restore(worn);
        var gear=new Equipment();Check(armourPack.EquipAt(0,gear)&&gear.At(1).Durability==71&&armourPack.At(0).Count==0,"equipping transfers rather than copies worn armour");
        armourPack.Add(Items.IronChest,1);Check(armourPack.EquipAt(0,gear)&&armourPack.At(0).Durability==71&&gear.At(1).Item==Items.IronChest,"equipment swap preserves previous durability");
        Check(armourPack.Unequip(1,gear)&&gear.At(1).Count==0&&armourPack.Count(Items.IronChest)==1,"unequip transfers into empty pack slot");
        gear.Equip(0,new Slot {Item=Items.IronHelm,Count=1,Durability=99});
        Check(!rewardFull.Unequip(0,gear)&&gear.At(0).Durability==99,"full pack prevents equipment loss");
        var shelterWorld=new WorldData(714);var feet=new Cell(0,50,0);
        for(int x=-4;x<=4;x++)for(int z=-4;z<=4;z++)for(int y=50;y<56;y++)shelterWorld.Set(new Cell(x,y,z),Block.Air);
        shelterWorld.Set(new Cell(0,53,0),Block.OakPlanks);
        Check(!FrontierQuest.IsShelter(shelterWorld,feet),"roof alone is not shelter");
        for(int y=50;y<=51;y++){shelterWorld.Set(new Cell(-2,y,0),Block.OakPlanks);shelterWorld.Set(new Cell(2,y,0),Block.OakPlanks);shelterWorld.Set(new Cell(0,y,2),Block.OakPlanks);}
        Check(FrontierQuest.IsShelter(shelterWorld,feet),"roof and three two-block walls count as a shelter");
        shelterWorld.Set(new Cell(0,53,0),Block.Air);Check(!FrontierQuest.IsShelter(shelterWorld,feet),"walls without roof are not shelter");
        Console.WriteLine("\n" + passed + " checks passed.");
    }
}
