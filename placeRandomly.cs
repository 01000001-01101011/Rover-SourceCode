using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class placeRandomly : MonoBehaviour
{
    public Vector2 area = Vector2.one * 100;
    public float spawnHeight = 100;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = new Vector3(Random.Range(0, area.x), spawnHeight, Random.Range(0, area.y));
    }
}
