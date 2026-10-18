using UnityEngine;

namespace CubeFrontier
{
    public interface IPlayerInput
    {
        Vector2 Movement { get; }
        Vector2 Look { get; }
        bool Jump { get; }
        bool Sprint { get; }
        bool Mine { get; }
        bool Use { get; }
    }
    public sealed class DesktopInput : IPlayerInput
    {
        public Vector2 Movement { get { return Vector2.ClampMagnitude(new Vector2((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0), (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)), 1); } }
        public Vector2 Look { get { return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")); } }
        public bool Jump { get { return Input.GetKeyDown(KeyCode.Space); } }
        public bool Sprint { get { return Input.GetKey(KeyCode.LeftShift); } }
        public bool Mine { get { return Input.GetMouseButton(0); } }
        public bool Use { get { return Input.GetMouseButtonDown(1); } }
    }
}
