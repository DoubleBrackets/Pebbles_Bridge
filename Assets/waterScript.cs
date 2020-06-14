using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterScript : MonoBehaviour
{
    // Start is called before the first frame update

    float startY;

    public GameObject[] water;

    private float speed = 1.25f;

    Vector3 startPos;
    Vector3 endPos;

    private void Awake()
    {
        startY = transform.position.y;
        endPos = new Vector3(-61, -13, 6);
        startPos = new Vector3(73, -13, 6);
    }
    private void FixedUpdate()
    {
        float yOffset = Mathf.Sin(Time.realtimeSinceStartup/1.5f);
        transform.position = new Vector3(transform.position.x, startY + yOffset, transform.position.z);

        foreach(GameObject w in water)
        {
            w.transform.position += Vector3.left * speed * Time.fixedDeltaTime;
            if(w.transform.position.x <= endPos.x)
            {
                w.transform.position = new Vector3(startPos.x,w.transform.position.y, w.transform.position.z);
            }
        }
    }

}
