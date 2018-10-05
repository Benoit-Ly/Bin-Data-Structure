using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinQueryBox : MonoBehaviour {

    #region Private Fields
    [SerializeField]
    Vector2 boxSize;

    Vector3 minBound;
    Vector3 maxBound;
    #endregion

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        float xOffset = boxSize.x / 2f;
        float yOffset = boxSize.y / 2f;
        Vector3 curPos = transform.position;

        minBound = new Vector3(curPos.x - xOffset, 0f, curPos.z - yOffset);
        maxBound = new Vector3(curPos.x + xOffset, 0f, curPos.z + yOffset);

        Navigation.TileNavGraph.Instance.ShowOccupiedBin(minBound, maxBound);
	}

    private void OnDrawGizmos()
    {
        Vector3 wireBoxSize = new Vector3(boxSize.x, 3f, boxSize.y);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, wireBoxSize);
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawCube(transform.position, wireBoxSize);
    }
}
