using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{

    public GameObject cameraLookTo;
    private float radius = 5.0f;
    private float speed = 50.0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(cameraLookTo.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("a"))
        {
            transform.RotateAround(cameraLookTo.transform.position, Vector3.up, speed * Time.deltaTime);
        }   
        if (Input.GetKey("d"))
        {
            transform.RotateAround(cameraLookTo.transform.position, Vector3.up, -speed * Time.deltaTime);
        }  

        if(Input.GetKey("w"))
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
            transform.LookAt(cameraLookTo.transform);
        }
        if(Input.GetKey("s"))
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
            transform.LookAt(cameraLookTo.transform);
        }

        if(Input.GetKey("q"))
        {
            Vector3 dir = (cameraLookTo.transform.position - transform.position);
            dir.Normalize();
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
            transform.LookAt(cameraLookTo.transform);
        }

        if(Input.GetKey("e"))
        {
            Vector3 dir = (cameraLookTo.transform.position - transform.position);
            dir.Normalize();
            transform.Translate(-dir * speed * Time.deltaTime, Space.World);
            transform.LookAt(cameraLookTo.transform);
        }
    }
}
