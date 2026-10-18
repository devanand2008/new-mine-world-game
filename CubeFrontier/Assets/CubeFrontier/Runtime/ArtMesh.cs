using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CubeFrontier
{
    // One vertex-coloured mesh per articulated part, rather than a renderer per voxel.
    public sealed class ArtMesh
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Color> colors = new List<Color>();
        readonly List<int> triangles = new List<int>();
        public int Count { get { return vertices.Count; } }
        public void Face(Vector3 a, Vector3 b, Vector3 c, Color color, bool glow = false)
        {
            int n = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c);
            color = color.linear; color.a = glow ? .85f : 0;
            colors.Add(color); colors.Add(color); colors.Add(color);
            triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2);
        }
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, bool glow = false)
        { Face(a,b,c,color,glow); Face(a,c,d,color,glow); }
        public void Box(Vector3 p, Vector3 size, Color color, Vector3 angles = default(Vector3), bool glow = false)
        {
            Quaternion r = Quaternion.Euler(angles); Vector3 h = size * .5f;
            Vector3[] v = new Vector3[8];
            for (int i=0;i<8;i++) v[i] = p + r * new Vector3((i&1)==0?-h.x:h.x,(i&2)==0?-h.y:h.y,(i&4)==0?-h.z:h.z);
            Quad(v[0],v[2],v[3],v[1],color,glow); Quad(v[5],v[7],v[6],v[4],color,glow);
            Quad(v[1],v[3],v[7],v[5],color,glow); Quad(v[4],v[6],v[2],v[0],color,glow);
            Quad(v[2],v[6],v[7],v[3],color,glow); Quad(v[4],v[0],v[1],v[5],color,glow);
        }
        public void Bar(Vector3 a, Vector3 b, float width, Color color, bool glow=false)
        {
            Quaternion r = Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            Box((a+b)*.5f,new Vector3(width,(b-a).magnitude,width),color,r.eulerAngles,glow);
        }
        public void Crystal(Vector3 p, float width, float height, Color color, Vector3 angles=default(Vector3))
        {
            Quaternion r=Quaternion.Euler(angles);
            Vector3 top=p+r*Vector3.up*height*.6f, bottom=p-r*Vector3.up*height*.4f;
            for(int i=0;i<6;i++) {
                float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                Vector3 va=p+r*new Vector3(Mathf.Cos(a)*width,0,Mathf.Sin(a)*width);
                Vector3 vb=p+r*new Vector3(Mathf.Cos(b)*width,0,Mathf.Sin(b)*width);
                Color shade=color*Mathf.Lerp(.65f,1.15f,i/5f); shade.a=1;
                Face(top,vb,va,shade,true); Face(bottom,va,vb,shade,true);
            }
        }
        public Transform Build(Transform parent,string name,Vector3 pivot=default(Vector3), Material material=null)
        {
            var obj=new GameObject(name); obj.layer=parent.gameObject.layer; obj.transform.SetParent(parent,false); obj.transform.localPosition=pivot;
            for(int i=0;i<vertices.Count;i++) vertices[i]-=pivot;
            var mesh=new Mesh { name=name, indexFormat=IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial=material ? material : Resources.Load<Material>("FrontierArt");
            obj.AddComponent<OwnedArtMesh>().Mesh=mesh;
            return obj.transform;
        }
    }
    public sealed class OwnedArtMesh : MonoBehaviour
    {
        public Mesh Mesh;
        void OnDestroy() { if(Mesh) Destroy(Mesh); }
    }
    public sealed class ArtRig : MonoBehaviour
    {
        public Transform LeftArm,RightArm,LeftLeg,RightLeg;
        public bool Wings, Floating, Preview;
        Vector3 last; float stride;
        void OnEnable() { last=transform.position; }
        void LateUpdate()
        {
            float dt=Preview?Time.unscaledDeltaTime:Time.deltaTime;
            if(dt<=0) return;
            float speed=Preview?.35f:Mathf.Min(1,(transform.position-last).magnitude/dt*.2f); last=transform.position;
            stride+=dt*(Preview?1.8f:8);
            float swing=Mathf.Sin(stride)*speed*27;
            float idle=Mathf.Sin(Preview?Time.unscaledTime*1.8f:Time.time*1.8f)*2;
            if(LeftArm) LeftArm.localRotation=Quaternion.Euler(Wings?0:swing,0,Wings?Mathf.Sin(stride*1.8f)*24:idle);
            if(RightArm) RightArm.localRotation=Quaternion.Euler(Wings?0:-swing,0,Wings?-Mathf.Sin(stride*1.8f)*24:-idle);
            if(LeftLeg) LeftLeg.localRotation=Quaternion.Euler(-swing,0,0);
            if(RightLeg) RightLeg.localRotation=Quaternion.Euler(swing,0,0);
        }
    }
}
