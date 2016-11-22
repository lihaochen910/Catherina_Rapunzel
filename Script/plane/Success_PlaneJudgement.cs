using UnityEngine;
using System.Collections;

public class Success_PlaneJudgement : MonoBehaviour {

    private SuccessJudgement sj;
    void Awake()
    {
        sj = GameObject.Find("EndBox_Prefab").GetComponent<SuccessJudgement>();
    }
    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.GetComponent<playerManager>() != null)
        {
            sj.Success();
            Destroy(c.gameObject.GetComponent<playerManager>());
        }
            
    }
}
