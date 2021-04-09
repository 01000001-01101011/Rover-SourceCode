using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class roverDriver : Agent
{
    public float power = 50000f;
    public float steeringAngle = 45;
    public float brakeTorque = 100000;

    private Rigidbody rb;

    public WheelCollider frontR, frontL;
    public WheelCollider rearR, rearL;
    public WheelCollider middleR, middleL;
    private WheelCollider[] wheelColliders = new WheelCollider[6];

    public GameObject frontRW, frontLW;
    public GameObject rearRW, rearLW;
    private GameObject[] Wheels = new GameObject[4];
    private Vector3[] originalRotation = new Vector3[4];
    
    private float vertical;
    private float horizontal;
    private float braking;

    /// <summary>
    /// Read only.  Car's exact current speed in m/s.
    /// </summary>
    public float speed;
    [Space]
    public float progressReward = 0.2f;
    public float backwardsPunishment = 0.3f;
    public float baseForwardInput = 0.1f;
    public float collisionPunishment = 5;
    private Quaternion startingRotation;
    public GameObject targetBox;
    private float previousDistance;
    public float targetReward = 5;

    public Vector2 spawnArea = new Vector2(-720, 600);
    public float height = 0;
    public Collider[] top = new Collider[2];

    public override void Initialize()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        startingRotation = gameObject.transform.rotation;

        wheelColliders[0] = frontR;
        wheelColliders[1] = frontL;
        wheelColliders[2] = rearR;
        wheelColliders[3] = rearL;
        wheelColliders[4] = middleR;
        wheelColliders[5] = middleL;
        Wheels[0] = frontRW;
        Wheels[1] = frontLW;
        Wheels[2] = rearRW;
        Wheels[3] = rearLW;

        for (int i = 0; i < 4; i++)
        {
            originalRotation[i] = Wheels[i].transform.localRotation.eulerAngles;
        }

        foreach (WheelCollider wheel in wheelColliders)
        {
            wheel.ConfigureVehicleSubsteps(300, 5, 5);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Out
        //vertical = Input.GetAxis("Vertical");
        //horizontal = Input.GetAxis("Horizontal");
        //braking = Input.GetAxis("Jump");

        //In
        //In CollectObservations() currentWheelAngle = horizontal * steeringAngle;
        //speed variable


        //foreach (WheelCollider wheel in wheelColliders)
        //{

        //    wheel.motorTorque = (-vertical * power);

        //    wheel.brakeTorque = braking * brakeTorque;

        //}

        for (int i = 0; i < 2; i++)
        {
            //Physics steering
            wheelColliders[i].steerAngle = horizontal * steeringAngle;

            //Update visual wheel position
            Wheels[i].transform.localEulerAngles = new Vector3(originalRotation[i].x, originalRotation[i].y , originalRotation[i].z+ (horizontal * steeringAngle));
        }

        float distance = (targetBox.transform.position - transform.position).magnitude;
        Debug.DrawRay(transform.position, targetBox.transform.position - transform.position);

        if (previousDistance == 0)
        {
            previousDistance = distance;
        }
        else
        {
            AddReward((previousDistance-distance)/100);
            previousDistance = distance;
        }

    }

    void Update()
    {
        Debug.Log("Vertical: " + vertical.ToString() + " Horizontal: " + horizontal.ToString() + " Braking: " + braking.ToString() + " Speed: " + Mathf.Round(speed * 3.6f).ToString() + "KM/H" + " Reward: " + GetCumulativeReward());
        speed = rb.velocity.magnitude;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
        //actionsOut[2] = Input.GetAxis("Jump");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(speed);
        sensor.AddObservation(Vector3.Dot(transform.right, targetBox.transform.position - transform.position));
        sensor.AddObservation(Vector3.Dot(transform.up, targetBox.transform.position - transform.position));
        sensor.AddObservation(Vector3.Dot(transform.forward, targetBox.transform.position - transform.position));
        sensor.AddObservation(rb.velocity / 100);

        sensor.AddObservation(transform.rotation.eulerAngles);
        //3

        //7 observations
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        vertical = vectorAction[0] + baseForwardInput;
        horizontal = vectorAction[1];
        //braking = vectorAction[2];

        foreach (WheelCollider wheel in wheelColliders)
        {

            wheel.motorTorque = (-vertical * power);

            //wheel.brakeTorque = braking * brakeTorque;

        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Starting new training round...");
        transform.position = new Vector3(Random.Range(0, spawnArea.x), height, Random.Range(0, spawnArea.y));
        transform.rotation = startingRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        resetTargetBox();

        Resources.UnloadUnusedAssets();


    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == targetBox)
        {
            AddReward(targetReward);
            EndEpisode();
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                ContactPoint contact = other.GetContact(i);

                foreach(Collider col in top)
                {
                    if (contact.thisCollider == col)
                    {
                        AddReward(-collisionPunishment);
                    }
                }
            }
        }
    }

    public void resetTargetBox()
    {
        targetBox.transform.position = new Vector3(Random.Range(0, spawnArea.x), height, Random.Range(0, spawnArea.y));
        targetBox.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
