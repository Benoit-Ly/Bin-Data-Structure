using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace Navigation
{
    public class TileNavGraph : MonoBehaviour
    {
	    static TileNavGraph instance = null;
	    static public TileNavGraph Instance
	    {
		    get
		    {
			    if (instance == null)
				    instance = FindObjectOfType<TileNavGraph>();
			    return instance;
		    }
	    }

        [SerializeField]
        private int GrassCost = 1;
        [SerializeField]
        private int UnreachableCost = int.MaxValue;

        [SerializeField]
        private int GridSizeH = 100;
        [SerializeField]
        private int GridSizeV = 100;
        [SerializeField]
        private int SquareSize = 1;
        [SerializeField]
        private float MaxHeight = 10f;
        [SerializeField]
        private float MaxWalkableHeight = 2.5f;

        // enable / disable debug Gizmos
        [SerializeField]
        private bool DrawGrid = false;
        [SerializeField]
        private bool DisplayAllNodes = false;
        [SerializeField]
        private bool DisplayAllLinks = false;

        private Vector3 gridStartPos = Vector3.zero;
        private Vector3 gridEndPos = Vector3.zero;
        private int NbTilesH = 0;
        private int NbTilesV = 0;
        private List<Node> LNode = new List<Node>();
        private Dictionary<Node, List<Connection>> connectionsGraph = new Dictionary<Node, List<Connection>>();

        public Dictionary<Node, List<Connection>> ConnectionsGraph { get { return connectionsGraph; } }

        // threading
        Thread GraphThread = null;

        // BitArray for bin data structure
        BitArray Bits_BinList;

        // Use this for initialization
        private void Awake ()
        {
            CreateTiledGrid();
	    }

        private void Start()
        {
            FieldObjectManager.Instance.OnNewObject += OnNewObject;

            ThreadStart threadStart = new ThreadStart(CreateGraph);
            GraphThread = new Thread(threadStart);
            GraphThread.Start();
        }

        private void OnDestroy()
        {
            FieldObjectManager.Instance.OnNewObject -= OnNewObject;
        }

        #region graph construction
        // Create all nodes for the tiled grid
        private void CreateTiledGrid()
	    {
		    LNode.Clear();

            gridStartPos = transform.position + new Vector3(- GridSizeH / 2f, 0f, - GridSizeV / 2f);
            gridEndPos = transform.position + new Vector3(GridSizeH / 2f, 0f, GridSizeV / 2f);

            NbTilesH = GridSizeH / SquareSize;
		    NbTilesV = GridSizeV / SquareSize;

		    for(int i = 0; i < NbTilesV; i++)
		    {
			    for(int j = 0; j < NbTilesH; j++)
			    {
				    Node node = new Node();
                    Vector3 nodePos = gridStartPos + new Vector3((j + 0.5f) * SquareSize, 0f, (i + 0.5f) * SquareSize);

				    int Weight = 0;
				    RaycastHit hitInfo = new RaycastHit();

                    // always compute node Y pos from floor collision
                    if (Physics.Raycast(nodePos + Vector3.up * MaxHeight, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer("Floor")))
                    {
                        if (Weight == 0)
                            Weight = hitInfo.point.y >= MaxWalkableHeight ? UnreachableCost : GrassCost;
                        nodePos.y = hitInfo.point.y;
                    }

                    node.Weight = Weight;
				    node.Position = nodePos;
				    LNode.Add(node);
			    }
		    }

            Bits_BinList = new BitArray(NbTilesH * NbTilesV);
        }

        // cast a ray for each possible corner of a tile node for better accuracy
        private bool RaycastNode(Vector3 nodePos, string layerName, out RaycastHit hitInfo)
        {
            if (Physics.Raycast(nodePos - new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos - new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            return false;
        }
        
        // Compute possible connections between each nodes
        private void CreateGraph()
        {
            foreach (Node node in LNode)
            {
                if (IsNodeWalkable(node))
                {
                    connectionsGraph.Add(node, new List<Connection>());
                    foreach (Node neighbour in GetNeighbours(node))
                    {
                        Connection connection = new Connection();
                        connection.Cost = ComputeConnectionCost(node, neighbour);
                        connection.FromNode = node;
                        connection.ToNode = neighbour;
                        connectionsGraph[node].Add(connection);
                    }
                }
            }
        }

        private int ComputeConnectionCost(Node fromNode, Node toNode)
        {
            return fromNode.Weight + toNode.Weight;
        }

        private List<Node> GetNeighbours(Node node)
        {
            Vector2Int tileCoord = GetTileCoordFromPos(node.Position);
            int x = tileCoord.x;
            int y = tileCoord.y;

            List<Node> nodes = new List<Node>();

            if (x > 0)
            {
                if (y > 0)
                    TryToAddNode(nodes, GetNode(x - 1, y - 1));
                TryToAddNode(nodes, LNode[(x - 1) + y * NbTilesH]);
                if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x - 1) + (y + 1) * NbTilesH]);
            }

            if (y > 0)
                TryToAddNode(nodes, LNode[x + (y - 1) * NbTilesH]);
            if (y < NbTilesV - 1)
                TryToAddNode(nodes, LNode[x + (y + 1) * NbTilesH]);

            if (x < NbTilesH - 1)
            {
                if (y > 0)
                    TryToAddNode(nodes, LNode[(x + 1) + (y - 1) * NbTilesH]);
                TryToAddNode(nodes, LNode[(x + 1) + y * NbTilesH]);
                if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x + 1) + (y + 1) * NbTilesH]);
            }

            return nodes;
        }

        private bool IsNodeWalkable(Node node)
        {
            return node.Weight < UnreachableCost;
        }

        private void TryToAddNode(List<Node> list, Node node)
        {
            if (IsNodeWalkable(node))
            {
                list.Add(node);
            }
        }
        #endregion

        #region node / pos methods
        public Node GetNode(Vector3 pos)
        {
            return GetNode(GetTileCoordFromPos(pos));
        }

        public bool IsPosValid(Vector3 pos)
        {
            if (GraphThread.ThreadState == ThreadState.Running)
                return false;

            if (pos.x > (-GridSizeH / 2) && pos.x < (GridSizeH / 2) && pos.z > (-GridSizeV / 2) && pos.z < (GridSizeV / 2))
                return true;
            return false;
        }

        // converts world 3d pos to tile 2d pos
        private Vector2Int GetTileCoordFromPos(Vector3 pos)
	    {
            Vector3 realPos = pos - gridStartPos;
            Vector2Int tileCoords = Vector2Int.zero;
            tileCoords.x = Mathf.FloorToInt(realPos.x / SquareSize);
            tileCoords.y = Mathf.FloorToInt(realPos.z / SquareSize);
		    return tileCoords;
	    }

        private Node GetNode(Vector2Int pos)
        {
            return GetNode(pos.x, pos.y);
        }

        private Node GetNode(int x, int y)
        {
            int index = y * NbTilesH + x;
            return GetNode(index);
        }

        private Node GetNode(int index)
        {
            if (index >= LNode.Count || index < 0)
                return null;

            return LNode[index];
        }
        #endregion

        #region Bin Methods
        void OnNewObject(FieldObject newObject, Bounds bounds)
        {
            Node minNode = GetNode(bounds.min);
            Node maxNode = GetNode(bounds.max);

            int firstZ = (int)minNode.Position.z;
            int firstX = (int)minNode.Position.x;

            int lastZ = (int)maxNode.Position.z;
            int lastX = (int)maxNode.Position.x;

            for (int i = firstZ; i <= lastZ; i++)
            {
                for (int j = firstX; j <= lastX; j++)
                {
                    Vector2Int gridPos = GetTileCoordFromPos(new Vector3(j, 0f, i));
                    Node node = GetNode(gridPos);
                    node.binObjects.Add(newObject);

                    int index = gridPos.y * NbTilesH + gridPos.x;
                    Bits_BinList[index] = true;
                }
            }
        }

        public void ShowOccupiedBin(Vector3 minBound, Vector3 maxBound)
        {
            Vector3 minBorder = LNode[0].Position;
            Vector3 maxBorder = LNode[LNode.Count - 1].Position;

            Vector3 clampedMinBound = new Vector3((minBound.x < minBorder.x) ? minBorder.x : minBound.x, minBound.y, (minBound.z < minBorder.z) ? minBorder.z : minBound.z);
            Vector3 clampedMaxBound = new Vector3((maxBound.x > maxBorder.x) ? maxBorder.x : maxBound.x, maxBound.y, (maxBound.z > maxBorder.z) ? maxBorder.z : maxBound.z);

            Node minNode = GetNode(clampedMinBound);
            Node maxNode = GetNode(clampedMaxBound);

            if (minNode == null || maxNode == null)
                return;

            int firstZ = (int)minNode.Position.z;
            int firstX = (int)minNode.Position.x;

            int lastZ = (int)maxNode.Position.z;
            int lastX = (int)maxNode.Position.x;

            for (int i = firstZ; i <= lastZ; i++)
            {
                for (int j = firstX; j <= lastX; j++)
                {
                    Vector2Int gridPos = GetTileCoordFromPos(new Vector3(j, 0f, i));
                    int index = gridPos.y * NbTilesH + gridPos.x;

                    if (Bits_BinList[index])
                    {
                        Node node = GetNode(index);
                        node.EnableRender(true);
                    }
                }
            }
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
	    {
            if (DrawGrid)
            {
                float gridHeight = 0.01f;
                Gizmos.color = Color.yellow;
                for (int i = 0; i < NbTilesV + 1; i++)
                {
                    Vector3 startPos = new Vector3(-GridSizeH / 2f, gridHeight, -GridSizeV / 2f + i * SquareSize);
                    Gizmos.DrawLine(startPos, startPos + Vector3.right * GridSizeV);

                    for (int j = 0; j < NbTilesH + 1; j++)
                    {
                        startPos = new Vector3(-GridSizeH / 2f + j * SquareSize, gridHeight, -GridSizeV / 2f);
                        Gizmos.DrawLine(startPos, startPos + Vector3.forward * GridSizeV);
                    }
                }
            }

            if (DisplayAllNodes)
            {
		        for(int i = 0; i < LNode.Count; i++)
		        {
                    Node node = LNode[i];
                    //if (node.state == NodeState.CLOSED)
                    //    Gizmos.color = Color.blue;
                    //else if(node.state == NodeState.OPEN)
                    //    Gizmos.color = Color.black;
                    //else
                    //    Gizmos.color = IsNodeWalkable(node) ? Color.green : Color.red;

                    if (node.binObjects.Count > 0)
                        Gizmos.color = Color.black;
                    else
                        Gizmos.color = IsNodeWalkable(node) ? Color.green : Color.red;

                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
		        }
            }
            if (DisplayAllLinks)
            {
                foreach (Node crtNode in LNode)
                {
                    if (connectionsGraph.ContainsKey(crtNode))
                    {
                        foreach (Connection c in connectionsGraph[crtNode])
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(c.FromNode.Position, c.ToNode.Position);
                        }
                    }
                }
            }
	    }
#endregion
    }
}


