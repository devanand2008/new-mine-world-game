using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace CubeFrontier
{
    public sealed class ChunkView : MonoBehaviour
    {
        private Mesh rendered, collision;
        private Transform plants;
        private static readonly Vector3[,] Corners = {
            { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
            { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
            { new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(0,1,0) },
            { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
            { new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1) },
            { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) }
        };
        public void Apply(List<VoxelFace> faces, Material material, WorldData data)
        {
            var vertices = new List<Vector3>(faces.Count * 4); var normals = new List<Vector3>(faces.Count * 4);
            var colors = new List<Color>(faces.Count * 4); var uv = new List<Vector2>(faces.Count * 4);
            var triangles = new List<int>(faces.Count * 6); var solidTriangles = new List<int>(faces.Count * 6);
            var waterTriangles = new List<int>(); var glassTriangles = new List<int>(); var foliage = new ArtMesh();
            int ox=Mathf.RoundToInt(transform.position.x), oz=Mathf.RoundToInt(transform.position.z);
            foreach (var face in faces) {
                int start = vertices.Count; int tile = Blocks.Tile(face.Block,face.Direction);
                float span=1f/Blocks.AtlasColumns,spanY=1f/Blocks.AtlasRows;
                float u = tile % Blocks.AtlasColumns * span, v = tile / Blocks.AtlasColumns * spanY;
                float inset = .5f/(Blocks.AtlasColumns*Blocks.AtlasTileSize),insetY=.5f/(Blocks.AtlasRows*Blocks.AtlasTileSize);
                float shade = .90f + (Noise.Hash(face.X, face.Y, face.Z, 37) % 100) * .001f;
                Color tint = new Color(shade, shade, shade, Blocks.Emits(face.Block) ? .8f : 0);
                for (int i = 0; i < 4; i++) {
                    Vector3 corner=Corners[face.Direction,i];
                    Vector3 position=new Vector3(face.X,face.Y,face.Z)+corner;
                    if(face.Block==Block.Water&&corner.y>.5f)position.y-=.13f;
                    vertices.Add(position);
                    normals.Add(new Vector3(Mesher.Directions[face.Direction, 0], Mesher.Directions[face.Direction, 1], Mesher.Directions[face.Direction, 2]));
                    float ao=face.Block==Block.Water?1:Occlusion(data,ox+face.X,face.Y,oz+face.Z,face.Direction,corner);
                    colors.Add(new Color(tint.r*ao,tint.g*ao,tint.b*ao,tint.a));
                    float tx=face.Direction<2?corner.z:corner.x, ty=face.Direction==2||face.Direction==3?corner.z:corner.y;
                    uv.Add(new Vector2(u + (tx>.5f ? span-inset:inset), v + (ty>.5f ? spanY-insetY:insetY)));
                }
                AddQuad(face.Block==Block.Water?waterTriangles:Blocks.Transparent(face.Block)&&face.Block!=Block.Lava?glassTriangles:triangles, start); if (Blocks.Solid(face.Block)) AddQuad(solidTriangles, start);
                if(face.Direction==2&&face.Block==Block.Grass&&data.Get(ox+face.X,face.Y+1,oz+face.Z)==Block.Air) {
                    uint h=Noise.Hash(ox+face.X,0,oz+face.Z,724);
                    if(h%9==0) {
                        Vector3 p=new Vector3(face.X+.25f+(h%7)*.07f,face.Y+1,face.Z+.35f);
                        Color green=ReferenceCharacters.Hex("72964A");
                        for(int k=-1;k<=1;k++)foliage.Box(p+new Vector3(k*.07f,.13f,0),new Vector3(.035f,.22f+(k==0?.10f:0),.035f),green,new Vector3(0,0,-k*25));
                        if(h%3==0) {
                            Color flower=h%2==0?ReferenceCharacters.Hex("F2CA66"):ReferenceCharacters.Hex("CBB9DD");
                            foliage.Box(p+Vector3.up*.30f,new Vector3(.14f,.045f,.14f),flower);
                            foliage.Box(p+Vector3.up*.33f,new Vector3(.047f,.028f,.047f),ReferenceCharacters.Hex("F6E3A3"));
                        }
                    }
                }
            }
            Mesh next = MakeMesh(vertices, triangles); next.SetNormals(normals); next.SetColors(colors); next.SetUVs(0, uv);
            next.subMeshCount=3;next.SetTriangles(waterTriangles,1);next.SetTriangles(glassTriangles,2);
            Mesh nextCollision = MakeMesh(vertices, solidTriangles);
            var filter = GetComponent<MeshFilter>(); if (!filter) filter = gameObject.AddComponent<MeshFilter>();
            var renderer = GetComponent<MeshRenderer>(); if (!renderer) renderer = gameObject.AddComponent<MeshRenderer>();
            var collider = GetComponent<MeshCollider>(); if (!collider) collider = gameObject.AddComponent<MeshCollider>();
            filter.sharedMesh = next; renderer.sharedMaterials = new[] {material,Resources.Load<Material>("FrontierWater"),Resources.Load<Material>("FrontierGlass")}; collider.sharedMesh = null; collider.sharedMesh = nextCollision;
            if(plants){plants.gameObject.SetActive(false);Destroy(plants.gameObject);}
            if(foliage.Count>0)plants=foliage.Build(transform,"Meadow grass and wildflowers");
            if (rendered) Destroy(rendered); if (collision) Destroy(collision); rendered = next; collision = nextCollision;
        }
        private static float Occlusion(WorldData data,int x,int y,int z,int direction,Vector3 corner)
        {
            int nx=Mesher.Directions[direction,0],ny=Mesher.Directions[direction,1],nz=Mesher.Directions[direction,2];
            int ax=0,ay=0,az=0,bx=0,by=0,bz=0;
            if(nx!=0){ay=corner.y>.5f?1:-1;bz=corner.z>.5f?1:-1;}
            else if(ny!=0){ax=corner.x>.5f?1:-1;bz=corner.z>.5f?1:-1;}
            else{ax=corner.x>.5f?1:-1;by=corner.y>.5f?1:-1;}
            x+=nx;y+=ny;z+=nz;
            bool a=Blocks.Occludes(data.Get(x+ax,y+ay,z+az)),b=Blocks.Occludes(data.Get(x+bx,y+by,z+bz));
            bool c=Blocks.Occludes(data.Get(x+ax+bx,y+ay+by,z+az+bz));
            return 1-(a&&b?3:(a?1:0)+(b?1:0)+(c?1:0))*.145f;
        }
        private static void AddQuad(List<int> list, int n) { list.Add(n); list.Add(n + 1); list.Add(n + 2); list.Add(n); list.Add(n + 2); list.Add(n + 3); }
        private static Mesh MakeMesh(List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh { indexFormat = IndexFormat.UInt32 }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); return mesh;
        }
        private void OnDestroy() { if (rendered) Destroy(rendered); if (collision) Destroy(collision); }
    }
}
