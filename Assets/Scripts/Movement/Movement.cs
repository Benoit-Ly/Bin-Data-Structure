using UnityEngine;

public class Movement : MonoBehaviour {
    [SerializeField]
    private float MaxSpeed = 10f;

    private Vector3 velocity = Vector3.zero;
    private float rotation = 0f;

    private float posOffsetY = 0.5f;
    private Vector3 targetPos;
    public Vector3 TargetPos
    {
        get { return targetPos; }
        set {
            targetPos = value;
            targetPos.y += posOffsetY;

            Navigation.PathFinder.Instance.ComputePath(transform.position, targetPos);
        }
    }

    private void Start()
    {
        targetPos = transform.position;
    }

	public void Stop()
	{
		velocity = Vector3.zero;
	}

    protected float GetOrientationFromDirection(Vector3 direction)
    {
        if (direction.magnitude > 0)
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
        return rotation;
    }

    // Update is called once per frame
    private void Update ()
	{
        //velocity = targetPos - transform.position;
        velocity = Navigation.PathFinder.Instance.GetVelocity(transform);
        rotation = GetOrientationFromDirection(velocity);

		// truncate to max speed
        if (velocity.magnitude > MaxSpeed)
		{
			velocity.Normalize();
            velocity *= MaxSpeed;
		}

		// Update position and rotation
		transform.position += velocity * Time.deltaTime;
		transform.eulerAngles = Vector3.up * rotation;

  //      // keep position above the floor
		//RaycastHit hitInfo = new RaycastHit();
  //      if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hitInfo, 100, 1 << LayerMask.NameToLayer("Floor")))
  //      {
  //          transform.position = hitInfo.point + Vector3.up * posOffsetY;
  //      }
	}

    private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, velocity);
	}
}
