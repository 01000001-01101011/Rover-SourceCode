using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class smoothRigidbodyFollowCameraV2 : MonoBehaviour
{
    public GameObject target;
    public float speed = 30;
    private Rigidbody rb;
    public bool followingTarget = true;
    public float followStartDelay = 1;
    [Space]
    public float drag = 15;
    public float minDistance = 10;
    private Vector3 offsetVector;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.drag = drag;

        if (speed < 0f)
        {
            if (speed != 0f)
            {
                speed = Mathf.Abs(speed);
            }
            else
            {
                speed = 10f;
            }
        }

        offsetVector = target.transform.position - transform.position;

        StartCoroutine(Wait());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movementVector = target.transform.position - gameObject.transform.position;

        if (followingTarget && movementVector.magnitude > minDistance) { rb.AddForce(movementVector * speed); }

        gameObject.transform.LookAt(target.transform);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(followStartDelay);
        followingTarget = true;
    }
}
