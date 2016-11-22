using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public GameObject target;
    public float distanse_X=0,distanse_Y = 4, distanse_Z = 4;
    public float speed = 2;
    private Transform player;
    // Use this for initialization
    void Start()
    {
        player = target.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //new Vector3(0, distanse_Y, distanse_Z)
        Vector3 targetPos = player.position + new Vector3(distanse_X, distanse_Y, distanse_Z-7);
        transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
        //iTween.MoveTo(gameObject, iTween.Hash("x", targetPos.x, "y", targetPos.y, "z", targetPos.z, "easeType", "linear", "delay",.5));
        Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = targetRotation;
    }

}
