using UnityEngine;
using System.Collections.Generic;
using Navigation;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private GameObject TargetCursorPrefab = null;
    private GameObject targetCursor = null;
    [SerializeField]
    private GameObject UnitPrefab = null;
    [SerializeField]
    private Transform PlayerStart = null;
    private Unit unit;

    delegate void InputEventHandler();
    event InputEventHandler OnMouseClicked;


    private GameObject GetTargetCursor()
    {
        if (targetCursor == null)
            targetCursor = Instantiate(TargetCursorPrefab);
        return targetCursor;
    }

    // Use this for initialization
    private void Start ()
    {
        if (UnitPrefab)
        {
            GameObject unitInst = Instantiate(UnitPrefab, PlayerStart, false);
            unitInst.transform.parent = null;
            unit = unitInst.GetComponent<Unit>();

            RaycastHit raycastInfo;
            Ray ray = new Ray(unitInst.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << LayerMask.NameToLayer("Floor")))
            {
                unitInst.transform.position = raycastInfo.point;
            }

            unit.SetSelected(true);
        }

        OnMouseClicked += () =>
        {
            int floorLayer = 1 << LayerMask.NameToLayer("Floor");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastInfo;
            // unit move target
            if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorLayer))
            {
                Vector3 newPos = raycastInfo.point;
                Vector3 targetPos = newPos;
                targetPos.y += 0.1f;
                GetTargetCursor().transform.position = targetPos;

                if (TileNavGraph.Instance.IsPosValid(newPos))
                {
                    unit.SetTargetPos(newPos);
                }
            }
        };
    }

    // Update is called once per frame
    private void Update ()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseClicked();
	}
}
