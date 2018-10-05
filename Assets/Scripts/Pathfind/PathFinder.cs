using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public class PathNode
    {
        public Node node;
        public PathNode parent = null;
    }

    public class PathFinder : MonoBehaviour
    {

        static private PathFinder p_Instance = null;
        static public PathFinder Instance { get { return p_Instance; } }

        List<Node> m_OpenList;
        List<Node> m_ClosedList;
        List<Node> m_FinalPath;     public List<Node> FinalPath { get { return m_FinalPath; } }

        Node m_currentComputeNode = null;

        int m_currentTravelIndex = 0;

        float m_optimalDist = float.MaxValue;

        bool m_reachedDeadEnd = false;

        void Awake()
        {
            if (PathFinder.Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            p_Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            m_OpenList = new List<Node>();
            m_ClosedList = new List<Node>();
            m_FinalPath = new List<Node>();
        }

        public Vector3 GetNodePosition(int index)
        {
            if (index < 0 || index >= m_FinalPath.Count)
                return Vector3.zero;

            return m_FinalPath[index].Position;
        }

        public Vector3 GetVelocity(Transform actor)
        {
            if (m_FinalPath.Count == 0)
                return Vector3.zero;

            float distance = Vector3.Distance(actor.position, m_FinalPath[m_currentTravelIndex].Position);
            if (distance < 2f && m_currentTravelIndex > 0)
                --m_currentTravelIndex;

            return m_FinalPath[m_currentTravelIndex].Position - actor.position;
        }

        public void ComputePath(Vector3 start, Vector3 end)
        {
            ClearLists();
            ResetPath();

            StartCoroutine(ComputePathCoroutine(start, end));
        }

        void ClearLists()
        {
            int count = m_ClosedList.Count;
            for (int i = 0; i < count; i++)
            {
                m_ClosedList[i].parent = null;
                m_ClosedList[i].state = NodeState.UNUSED;
                m_ClosedList[i].Weight = 0;
            }

            int openCount = m_OpenList.Count;
            for (int i = 0; i < openCount; i++)
            {
                m_OpenList[i].parent = null;
                m_OpenList[i].state = NodeState.UNUSED;
                m_OpenList[i].Weight = 0;
            }

            m_ClosedList.Clear();
            m_OpenList.Clear();
        }

        void ResetPath()
        {
            m_currentComputeNode = null;
            m_optimalDist = float.MaxValue;

            m_FinalPath.Clear();
        }

        IEnumerator ComputePathCoroutine(Vector3 start, Vector3 end)
        {
            Node startNode = TileNavGraph.Instance.GetNode(start);
            Node endNode = TileNavGraph.Instance.GetNode(end);

            if (startNode != null)
                m_OpenList.Add(startNode);

            while (m_OpenList.Count > 0)
            {
                yield return StartCoroutine(GetOptimalNode(start, end));

                if (m_currentComputeNode == endNode)
                {
                    Debug.Log(Vector3.Distance(m_currentComputeNode.Position, end));
                    BuildPath(m_currentComputeNode);
                    break;
                }

                List<Connection> neighbours = TileNavGraph.Instance.ConnectionsGraph[m_currentComputeNode];
                for (int i = 0; i < neighbours.Count; i++)
                {
                    Node neighbour = neighbours[i].ToNode;
                    if (neighbour.state == NodeState.UNUSED)
                    {
                        neighbour.parent = m_currentComputeNode;
                        neighbour.Weight = m_currentComputeNode.Weight + (int)Vector3.Distance(m_currentComputeNode.Position, neighbour.Position);
                        m_OpenList.Add(neighbour);
                        neighbour.state = NodeState.OPEN;
                    }
                    else if(m_currentComputeNode.Weight < neighbour.Weight)
                    {
                        neighbour.parent = m_currentComputeNode;
                        neighbour.Weight = m_currentComputeNode.Weight + (int)Vector3.Distance(m_currentComputeNode.Position, neighbour.Position);
                    }
                }

                m_currentComputeNode.state = NodeState.CLOSED;
                m_ClosedList.Add(m_currentComputeNode);
                m_OpenList.Remove(m_currentComputeNode);
            }

            yield return null;
        }

        void BuildPath(Node lastNode)
        {
            if (lastNode == null)
                return;

            m_FinalPath.Add(lastNode);
            m_currentTravelIndex = m_FinalPath.Count - 1;

            if (lastNode.parent != null)
                BuildPath(lastNode.parent);
        }

        IEnumerator GetOptimalNode(Vector3 start, Vector3 end)
        {
            if (m_OpenList.Count == 0)
                yield break;

            float currentDist = 0f;
            m_optimalDist = float.MaxValue;

            for (int i = 0; i < m_OpenList.Count; i++)
            {
                currentDist = Vector3.Distance(m_OpenList[i].Position, end);

                if (currentDist < m_optimalDist)
                {
                    m_optimalDist = currentDist;
                    m_currentComputeNode = m_OpenList[i];
                }
            }

            yield return null;
        }
    }
}