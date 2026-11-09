using UnityEngine;

public class Billboard : MonoBehaviour
{

    public Transform Cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position+ Cam.forward);
    }
}
