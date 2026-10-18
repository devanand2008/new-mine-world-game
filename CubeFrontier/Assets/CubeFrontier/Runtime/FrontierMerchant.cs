using UnityEngine;
using CubeFrontier.Core;
namespace CubeFrontier
{
    public sealed class FrontierMerchant : MonoBehaviour
    {
        public GameSession Session;
        public bool CanTrade { get { return Session&&Session.Player&&Vector3.Distance(transform.position,Session.Player.transform.position)<4 && !Physics.Linecast(transform.position+Vector3.up*1.5f,Session.Player.transform.position+Vector3.up*1.5f,1<<0); } }
        void Start() {
            gameObject.layer=9;
            ReferenceCharacters.Create(transform,9);
            var collider=gameObject.AddComponent<CapsuleCollider>();collider.height=1.8f;collider.radius=.35f;collider.center=Vector3.up*.9f;
            var labelObj=new GameObject("Merchant sign");labelObj.transform.SetParent(transform,false);labelObj.transform.localPosition=Vector3.up*2.5f;
            var label=labelObj.AddComponent<TextMesh>();label.text="MARA\n[T] Trade";label.fontSize=40;label.characterSize=.045f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.8f,1,.55f);
        }
        void Update() {
            if(!Session||!Session.Player||Session.Paused)return;
            Transform sign=transform.Find("Merchant sign");if(sign)sign.rotation=Session.Player.View.transform.rotation;
            // Remain grounded when terrain beneath the merchant is edited.
            Vector3 p=transform.position;int x=Mathf.FloorToInt(p.x),y=Mathf.FloorToInt(p.y),z=Mathf.FloorToInt(p.z);
            if(!Blocks.Solid(Session.World.Data.Get(x,y-1,z))||Blocks.Solid(Session.World.Data.Get(x,y,z)))transform.position=Session.World.FindSpawn(x,z);
        }
    }
}
