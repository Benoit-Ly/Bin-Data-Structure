using UnityEngine;
using System.Collections;

public class Unit : SelectableEntity
{
    private Movement movement;

    // Use this for initialization
    override protected void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
	}

    public void SetTargetPos(Vector3 pos)
    {
        movement.TargetPos = pos;
    }
}
