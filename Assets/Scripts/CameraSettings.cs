using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    [System.Serializable]
    public struct CameraData
    {
        public float xminus;     
        public float xplus;      
        public float moveSpeed; 
    }

    public CameraData data;
    public GameObject canvasToToggle;

    void Start()
    {

    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F) && canvasToToggle != null)
        {
            canvasToToggle.SetActive(!canvasToToggle.activeSelf);
        }

        Vector3 pos = transform.position;

        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= data.moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            pos.x += data.moveSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, data.xminus, data.xplus);

        transform.position = pos;
    }
}