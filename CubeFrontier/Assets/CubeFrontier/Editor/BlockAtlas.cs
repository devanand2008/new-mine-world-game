using System.IO;
using CubeFrontier.Core;
using UnityEditor;
using UnityEngine;
namespace CubeFrontier.Editor
{
 public static class BlockAtlas
 {
  public static Texture2D Generate(string path)
  {
   const int tileSize=32;
   var t=new Texture2D(Blocks.AtlasColumns*tileSize,Blocks.AtlasRows*tileSize,TextureFormat.RGBA32,false);
   Color[] palette={Color.clear,C("6FA34C"),C("866043"),C("8B9595"),C("DAC68C"),C("448699"),C("E1EAF1"),C("85867E"),C("786045"),C("4E804A"),C("4CCFDF"),C("444653"),C("5ADBC2"),C("292B32"),
    C("BD945B"),C("69513C"),C("D9C69A"),C("838881"),C("9DA19D"),C("718466"),C("B7664B"),C("DCC591"),C("A5ABAD"),C("E2D8BA"),C("4B6071"),C("A9563B"),C("C4E2DB"),C("455966"),C("BC835A"),C("AAB9BD"),C("D9B45A"),C("F3C76B"),C("8D6740"),C("A7814D"),C("B6544F"),C("537C9C"),C("DCDDD2"),C("6D8B52"),C("C3825D"),C("444851"),C("D8D5C7"),C("AE9468"),C("8A8981"),C("A3D5E8"),C("514235"),C("888F8D"),C("866043"),C("A38454"),C("685342"),
    /* padding for tiles 49-57 */ C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),C("8B9595"),
    /* expansion tiles 58-69 */
    C("B09070"),C("5C8244"),C("C4A86A"),C("B8D8F4"),C("E85A28"),C("3A2844"),C("7B6848"),C("A060D0"),C("5B4030"),C("2A2A32"),C("E84820"),C("D8A038"),
    /* camp tiles 70-74 */ C("AB7C48"),C("BD9258"),C("858D87"),C("4E9194"),C("715335")};
   for(int tile=0;tile<Blocks.AtlasColumns*Blocks.AtlasRows;tile++)for(int py=0;py<tileSize;py++)for(int px=0;px<tileSize;px++){
    // Original 16-pixel patterns, expanded to the existing atlas layout.
    int x=px/2*2,y=py/2*2;
    uint h=Noise.Hash(x/2,tile,y/2,1927),grain=Noise.Hash(x,tile,y,73);
    float value=.90f+(h%11)*.018f+(grain%5)*.009f;Color color=palette[tile<palette.Length?tile:3];
    if(tile==1||tile==9){uint patch=Noise.Hash(x/4,tile,y/3,985);value=.84f+(patch%9)*.026f;if((x+2*y)%17==0)value*=1.13f;}
    if(tile==2||tile==46){if(h%19<3)color=C("A57D52");if(h%23==0)color=C("544636");}
    if(tile==46){int edge=24+(int)(Noise.Hash(x/3,0,0,35)%5);if(y>=edge){color=palette[1];value=.87f+grain%7*.03f;}else if(y==edge-1)value*=.63f;}
    if(tile==3){value=.84f+(Noise.Hash(x/2,3,y/2,472)%9)*.024f;if(Noise.Hash(x/4,3,y/4,591)%5==0)value*=.87f;}
    if(tile==11||tile==13||tile==7){
     uint patch=Noise.Hash((x+(y/8%2)*5)/9,tile,y/8,537);value=.75f+(patch%9)*.038f+(grain%7)*.006f;
     if(y%8==0||((x+(y/8%2)*5)%9==0&&y%8>0))value*=.83f;
     if(y%8==7)value*=1.09f;
     if(tile==7&&Noise.Hash(x/3,0,y/3,613)%7<2){color=C("CF9B61");value*=1.08f;}
    }
    if(tile==8||tile==44){value=.70f+Noise.Hash(x/3,0,y/9,633)%12*.040f;if(x%8==0)value*=.65f;if(x%8==1)value*=1.2f;}
    if(tile==47||tile==48){float r=Mathf.Max(Mathf.Abs(x-15.5f),Mathf.Abs(y-15.5f));value=((int)r%4==0)?.64f:1.02f;if(r>13)color=palette[tile==47?8:44];}
    if(tile==4){value=.90f+(grain%7)*.018f;if(y%7==0&&x%9<5)value*=.90f;}
    if(tile==6){value=.93f+(h%5)*.012f;if(y<3)color=C("B6CBD8");}
    if(tile==10||tile==12){value=.64f+Noise.Hash(x/5,tile,y/7,731)%9*.09f;if((x+y*2)%17<2||(x*3-y+64)%29==0){color=C("CAFFFF");value=1;} }
    if(tile>=14&&tile<=16){value=.90f+Noise.Hash(x/7,tile,y/2,532)%8*.019f;if(y%8==0||(x+(y/8%2)*16)%32==0)value=.58f;else if(y%8==1)value=1.16f;if((x+y*2)%23==0)value*=.87f;}
    if(tile==17||tile==19){int row=y/8,xx=(x+row%2*5)%11;value=.76f+Noise.Hash((x+row%2*5)/11,0,row,716)%8*.047f;if(y%8==0||xx==0)color=C("535B55");if(y%8==7)value*=1.11f;if(tile==19&&h%7<3)color=C("5D7749");}
    if(tile==18||tile==20||tile==21){int hh=tile==20?6:8;value=.87f+Noise.Hash((x+(y/hh%2)*8)/16,tile,y/hh,28)%7*.034f;if(y%hh==0||(x+(y/hh%2)*8)%16==0){color=tile==20?C("D0B693"):palette[tile]*.71f;value=1;}}
    if(tile==22||tile==23||tile==38){value=.96f+(grain%5)*.012f;if(tile==22&&(x==0||y==0))value=.77f;}
    if(tile==24||tile==25){int xx=(x+(y/8%2)*4)%8;value=.77f+(y%8)*.036f;if(xx==0)value*=.65f;if(y%8==0)value*=.60f;if(xx==1)value*=1.12f;}
    if(tile==26||tile==27||tile==43){value=1;if(x<2||y<2||x>29||y>29)color=tile==27?C("839493"):C("E4F4E8");else if((x-y+32)%32<2)value=1.12f;}
    if(tile>=28&&tile<=30){value=.90f+(x+y)*.002f;if(x==0||y==0||x==31||y==31)value=.56f;else if(x==1||y==30)value=1.18f;if((x==4||x==27)&&(y==4||y==27))value=.56f;}
    if(tile==31){value=.92f+(grain%9)*.02f;if(x<4||x>27||y<4||y>27||x==15||x==16){color=C("584933");value=1;}}
    if(tile==32){if(y%16<3||x<2||x>29)color=C("B79259");else{string[] books={"688988","984D40","C3A65D","6A6386","928A60","4F7285"};color=C(books[(x/4+y/16)%6]);if(x%4==0)value*=.64f;if(y%16==6||y%16==13)color=C("C5B480");}}
    if(tile==33){value=.83f+Noise.Hash(x/6,0,y/4,31)%9*.029f;if(x<4||x>27||y<4||y>27||Mathf.Abs(x-y)<3||Mathf.Abs(31-x-y)<3)color=C("6A4F30");if((x==5||x==26)&&(y==5||y==26))color=C("B7B4A1");}
    if(tile>=34&&tile<=37){value=.91f+((x+y)%2)*.04f+grain%5*.008f;}
    if(tile==39){value=.93f+grain%5*.008f;if(x==0||y==0)value=.65f;if((x+y*3)%53<2)value=1.20f;}
    if(tile==40){value=.96f+grain%5*.012f;if((x+(int)(Mathf.Sin(y*.23f)*4)+y/2)%23<2)color=C("A5ABA6");}
    if(tile==41){value=.88f+h%11*.021f;if(h%13==0)color=C("776B50");}
    if(tile==42){value=.64f+Noise.Hash(x/3,0,y/3,65)%13*.05f;if(x%3==0||y%3==0)value*=.83f;}
    if(tile==45){int r=Mathf.Max(Mathf.Abs(x-16),Mathf.Abs(y-16));value=r%6==0?.55f:r%6==1?1.13f:1;}

    // === Expansion block textures (tiles 58-69) ===
    // 58: Clay - smooth earthy brown with subtle streaks
    if(tile==58){value=.88f+(grain%7)*.016f;if(y%6<2)value*=.92f;if(h%17==0)color=C("967860");}
    // 59: Moss - organic green with lumpy texture
    if(tile==59){value=.78f+(h%11)*.035f;if((x+y)%5<2){color=C("4A6E30");value*=1.12f;}if(h%13==0)color=C("7BAF58");}
    // 60: Mushroom - tan cap with spots
    if(tile==60){float dist=Mathf.Sqrt((x-16)*(x-16)+(y-16)*(y-16));value=.80f+(grain%7)*.02f;if(dist<10)color=C("D4B070");if(dist<3||((x+y*3)%11<2&&dist<9)){color=C("F0E4C0");value=1.1f;}}
    // 61: Moonstone - pale blue glow with crystalline pattern
    if(tile==61){value=.85f+Noise.Hash(x/4,0,y/5,183)%9*.04f;if((x+y*2)%13<3||(x*3-y+32)%19==0){color=C("E8F4FF");value=1.2f;}if(h%23==0){color=C("CAFFFF");value=1.3f;}}
    // 62: Ember Crystal - fiery orange with glowing veins
    if(tile==62){value=.70f+Noise.Hash(x/3,0,y/4,291)%11*.05f;if((x+y)%7<2||(x*2-y+32)%11==0){color=C("FFB830");value=1.3f;}if(h%11<3){color=C("FF6020");value=1.1f;}}
    // 63: Shadow Ore - dark purple with faint violet streaks
    if(tile==63){value=.55f+(h%9)*.03f;if((x+y*3)%17<3){color=C("6838A0");value=.9f;}if(h%19==0){color=C("A060E0");value=1.15f;}}
    // 64: Ancient Relic - weathered stone with golden rune engravings
    if(tile==64){value=.78f+Noise.Hash(x/5,0,y/6,447)%8*.03f;if(y%8<2||x%10<2){color=C("D8B060");value=1.05f;}if(Mathf.Abs(x-16)<2&&Mathf.Abs(y-16)<5){color=C("F0D878");value=1.2f;}}
    // 65: Spectral Block - ghostly purple with shimmering highlights
    if(tile==65){value=.72f+(grain%9)*.04f;float wave=Mathf.Sin(x*.3f+y*.5f)*.5f+.5f;color=Color.Lerp(C("6040A0"),C("C0A0F0"),wave);if(h%7<2)value=1.25f;}
    // 66: Roots - dark brown twisted pattern
    if(tile==66){value=.65f+Noise.Hash(x/4,0,y/3,551)%9*.04f;if((int)(Mathf.Sin(x*.4f)*3+y)%6<2){color=C("4A3520");value*=.85f;}if(h%11==0)color=C("6B5030");}
    // 67: Obsidian - very dark with glassy highlights
    if(tile==67){value=.35f+(grain%5)*.02f;if((x+y)%11<2||h%31==0){color=C("404858");value=.6f;}if((x*2+y)%23==0)value=.85f;}
    // 68: Lava - bright orange-red flowing pattern
    if(tile==68){float flow=Mathf.Sin(x*.25f+y*.15f)*.5f+.5f;value=.80f+flow*.3f;color=Color.Lerp(C("C02010"),C("FF8020"),flow);if(h%5<2){color=C("FFD040");value=1.2f;}}
    // 69: Torch - wooden stick with flame top
    if(tile==69){bool stick=Mathf.Abs(x-16)<3&&y<22;bool flame=Mathf.Abs(x-16)<5&&y>=18&&y<30;value=.85f;if(stick){color=C("8B6840");value=.75f+grain%5*.03f;}else if(flame){float fy=(y-18)/12f;color=Color.Lerp(C("FF6020"),C("FFE040"),fy);value=1.1f+fy*.2f;}else{color=Color.clear;value=0;}}

    // Camp blocks have distinct faces and silhouettes in the item catalogue.
    if(tile==70||tile==71){value=.88f+Noise.Hash(x/6,tile,y/4,85)%7*.025f;if(x<2||x>29||y<2||y>29)color=C("57432E");if(y%8==0)value*=.76f;if(tile==70){if(y==20)color=C("4A3828");if(x>=12&&x<=18&&y>=14&&y<=22){color=C("D6C08A");value=1;}if(x==16&&y==16)color=C("4A4540");}else if(x<6||x>25)color=C("745638");}
    if(tile==72){value=.85f+Noise.Hash(x/4,tile,y/4,95)%8*.03f;if(x<2||y<2||x>29||y>29)color=C("515D59");if(x>=6&&x<=24&&y>=8&&y<=22){color=C("202B2D");value=1;}if(x>=8&&x<=22&&y>=8&&y<=12){color=C("DB8D47");value=.7f+grain%5*.06f;}if(y==24&&x>=6&&x<=24)color=C("B1BAB1");}
    if(tile==73){color=y>21?C("E6E3CE"):C("4D9398");if(x<2||x>29||y<2||y>29)color=C("785B3C");if(y==20)color=C("37686D");value=.93f+grain%5*.014f;}
    if(tile==74){color=y>=20?C("4D9398"):C("715335");if(y==18)color=C("B58D59");if(y<12&&x>5&&x<26)color=C("423A2C");value=.90f+grain%7*.018f;}
    color*=value;color.a=tile==69&&!(Mathf.Abs(x-16)<3&&y<22)&&!(Mathf.Abs(x-16)<5&&y>=18&&y<30)?0:1;
    t.SetPixel(tile%8*32+px,tile/8*32+py,color);
   }
   t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.filterMode=FilterMode.Point;imp.mipmapEnabled=true;imp.anisoLevel=2;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.wrapMode=TextureWrapMode.Clamp;imp.SaveAndReimport();
   return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  static Color C(string hex){Color c;ColorUtility.TryParseHtmlString("#"+hex,out c);return c;}
 }
}
