using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public enum NodeState
    {
        UNUSED = 0,
        OPEN,
        CLOSED
    }

    struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int _x = 0, int _y = 0) { x = _x; y = _y; }

        static public Vector2Int zero { get { return new Vector2Int(); } }
    }

    public class Node
    {
        public Vector3 Position = Vector3.zero;
        public int Weight = 0;

        public Node parent = null;

        public NodeState state = NodeState.UNUSED;

        public bool selected = false;

        public List<FieldObject> binObjects = new List<FieldObject>();

        public void EnableRender(bool enable)
        {
            for (int i = 0; i < binObjects.Count; i++)
            {
                binObjects[i].EnableRender(enable);
            }
        }
    }

    public class Connection
    {
        public int Cost;
        public Node FromNode;
        public Node ToNode;
    }
}
