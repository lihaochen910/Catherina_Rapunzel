using System;
using UnityEngine;
using System.Collections;

public class BoxManager : MonoBehaviour
{
    public playerManager player; 
    private Rigidbody rigidbody_self;

    public Vector3 targetpos;

    public float smooth = 1;
    public float drag_distanse = 1f;
    private AudioSource edge_SE;
	// Use this for initialization
    void Awake()
    {
        //switch (Random.Range(1, 5))
        //{
        //    case 1: transform.Rotate(transform.rotation.x, 0, transform.rotation.z); break;
        //    case 2: transform.Rotate(transform.rotation.x, 90, transform.rotation.z); break;
        //    case 3: transform.Rotate(transform.rotation.x, 180, transform.rotation.z); break;
        //    case 4: transform.Rotate(transform.rotation.x, 270, transform.rotation.z); break;
        //}
        edge_SE = gameObject.AddComponent<AudioSource>();
        edge_SE.clip = Resources.Load("Sound/SE/042.Synth_kitajyo_PZL_SE_edge_mix_wav", typeof(AudioClip)) as AudioClip;
    }
    private bool Kinematic = false;
	void Start ()
	{
        //player = GameObject.FindWithTag("Player").GetComponent<playerManager>(); 
        //player.Notify_boxManager += box_forward_check;//接收playerManager传出的Event
        rigidbody_self = GetComponent<Rigidbody>();
        if (rigidbody_self.isKinematic)
            Kinematic = true;
        supportbox = new support_box(this.gameObject);
        Affected_boxes = new effect_on(this.gameObject);
        Affected_boxes.check_effect_onBox();
	}
    public bool Need_to_check_supportBox = true;
    public support_box supportbox;
    public effect_on Affected_boxes;
    void Update()
    {
        if (Need_to_check_supportBox)
        {
            supportbox.checkSupport();
            if (supportbox.hasSupport)
            {
                rigidbody_self.isKinematic = true; rigidbody_self.useGravity = false; Need_to_check_supportBox = false; 
                Debug.Log(gameObject.name+"校正位置"+supportbox.baseBox);
                supportbox.Correct_basePosition();
                Affected_boxes.check_effect_onBox(); edge_SE.Play();
            }
            else
            {
                add_gravity();
            }
        }
            
    }

    
    void OnCollisionEnter(Collision c)
    {

            //rigidbody.isKinematic = true;
            //rigidbody.useGravity = false;

        //Debug.Log(gameObject.name+"碰到了" + c.gameObject.name);
        
    }

    
    void box_forward_check(object sender, PlayerMoveDistance e)//被推动时检查前方
    {

        //player推力的方向
        float direction_X_of_the_force = e.d.x;
        float direction_Z_of_the_force = e.d.z;

            if (transform.position.y == e.player.transform.position.y)
            {//与player的力在相同方向上的box才能被推动
                if (direction_X_of_the_force != 0 && direction_Z_of_the_force == 0)
                {//X轴受力时,目标box的y，z轴坐标值相同
                    if (transform.position.y == e.player.transform.position.y &&
                        transform.position.z == e.player.transform.position.z)
                    {
                        RaycastHit hit;
                        Ray ray = new Ray(transform.position, e.d);
                        bool isHit = Physics.Raycast(ray, out hit, drag_distanse);
                        Debug.Log("e.d=" + e.d);
                        if (isHit)
                        {
                            Debug.Log("box前方有box,");
                            targetpos = new Vector3(transform.position.x + direction_X_of_the_force,
                                transform.position.y,
                                transform.position.z);
                            rigidbody_self.MovePosition(Vector3.Lerp(transform.position, targetpos, smooth));
                            hit.transform.position += new Vector3(direction_X_of_the_force, 0, 0);
                        }
                    }
                }
                if (direction_X_of_the_force == 0 && direction_Z_of_the_force != 0)
                {//Z轴受力，目标box的x，y轴坐标值相同
                    if (transform.position.y == e.player.transform.position.y &&
                        transform.position.x == e.player.transform.position.x)
                    {
                        RaycastHit hit;
                        Ray ray = new Ray(transform.position, e.d);
                        bool isHit = Physics.Raycast(ray, out hit, drag_distanse);
                        Debug.Log("e.d=" + e.d);
                        if (isHit)
                        {
                            Debug.Log("box前方有box,");
                            targetpos = new Vector3(transform.position.x, transform.position.y,
                                transform.position.z + direction_Z_of_the_force);
                            rigidbody_self.MovePosition(Vector3.Lerp(transform.position, targetpos, smooth));
                            hit.transform.position += new Vector3(0, 0, direction_Z_of_the_force);
                        }
                    }
                }
            }
        
    }
    void add_gravity()
    {
        rigidbody_self.isKinematic = false;
        rigidbody_self.useGravity = true;
    }
    private class Four_direct//四个的方向
    {
        public static Vector3 Left_dir=new Vector3(-1,0,0)
            , Right_dir = new Vector3(1, 0, 0), Forward_dir = new Vector3(0, 0, 1), Back_dir = new Vector3(0, 0, -1);
    }
    public static float Correct_speed = 1f;

    public delegate void Affected_box_notice();
    public Affected_box_notice Notify_Affectedbox;
    public class effect_on
    {
        private RaycastHit hit_left_box, hit_right_box, hit_forward_box, hit_back_box;
        public Transform top_left, top_right, top_forward, top_back,top_center;
        public GameObject the_box;
        public effect_on(GameObject g)
        {
            the_box = g;
        }
        public void check_effect_onBox()
        {
            //Debug.Log(the_box.name+"检查受影响的box");
            Vector3 the_box_top = new Vector3(the_box.transform.position.x, the_box.transform.position.y + 0.6f, the_box.transform.position.z);

            top_center = ray_check(the_box.transform.position, new Vector3(0, 1, 0), 0.6f).transform;
            top_left = ray_check(the_box_top, Four_direct.Left_dir, 0.5f).transform;
            top_right = ray_check(the_box_top, Four_direct.Right_dir, 0.5f).transform;
            top_forward = ray_check(the_box_top, Four_direct.Forward_dir, 0.5f).transform;
            top_back = ray_check(the_box_top, Four_direct.Back_dir, 0.5f).transform;
        }
        public void Notify_Affectedbox(float delay)
        {
            the_box.GetComponent<BoxManager>().StartCoroutine(the_box.GetComponent<BoxManager>().Delayed_check(delay));
        }
        public void afterDelayed()
        {
            the_box.GetComponent<BoxManager>().Need_to_check_supportBox = true;
            float delay = 0.3f;
            if (top_center != null)
            {
                Debug.Log(top_center.gameObject.name + "需要检查support");
                top_center.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(delay);
            }
            if (top_left != null)
            {
                Debug.Log(top_left.gameObject.name+"需要检查support");
                top_left.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(delay);
            }
            if (top_right != null)
            {
                Debug.Log(top_right.gameObject.name + "需要检查support");
                top_right.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(delay);
            }
            if (top_forward != null)
            {
                Debug.Log(top_forward.gameObject.name + "需要检查support");
                top_forward.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(delay);
            }
            if (top_back != null)
            {
                Debug.Log(top_back.gameObject.name + "需要检查support");
                top_back.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(delay);
            } 
        }
        
    }//上方会受到影响的box
    IEnumerator Delayed_check(float delay)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            Affected_boxes.afterDelayed();
        }
        catch (NullReferenceException ex) {
            Debug.Log(ex.Message);
        }
    }
    public class support_box//支撑box不会下落的四个底边的box
    {
        public bool hasSupport = false;
        public Vector3 baseBox;//校正后的位置
        private RaycastHit hit_left_box, hit_right_box, hit_forward_box, hit_back_box;
        public Transform left, right, forward, back,center;
        public GameObject the_box;
        private BoxManager tempBM;
        public support_box(GameObject g)
        {
            the_box = g;
            tempBM = the_box.GetComponent<BoxManager>();
        }
        public void checkSupport()
        {
            Vector3 the_box_bottom = new Vector3(the_box.transform.position.x, the_box.transform.position.y - 0.6f, the_box.transform.position.z);

            center = ray_check(the_box.transform.position, new Vector3(0, -1, 0), 0.5f).transform;
            left=ray_check(the_box_bottom, Four_direct.Left_dir, 0.5f).transform;
            right = ray_check(the_box_bottom, Four_direct.Right_dir, 0.5f).transform;
            forward = ray_check(the_box_bottom, Four_direct.Forward_dir, 0.5f).transform;
            back = ray_check(the_box_bottom, Four_direct.Back_dir, 0.5f).transform;

            if (center != null)
            {
                hasSupport = true; baseBox = new Vector3(center.position.x, center.position.y + 1, center.position.z);
                center.gameObject.GetComponent<BoxManager>().Affected_boxes.check_effect_onBox();
            }
            if (left != null ){
                hasSupport = true; baseBox = new Vector3(left.position.x + 1, left.position.y + 1, left.position.z);
                left.gameObject.GetComponent<BoxManager>().Affected_boxes.check_effect_onBox();
            }
            else if(right!=null){
                hasSupport = true;baseBox = new Vector3(right.position.x - 1, right.position.y + 1, right.position.z);
                right.gameObject.GetComponent<BoxManager>().Affected_boxes.check_effect_onBox();
            }
            else if(forward!=null){
                hasSupport = true; baseBox = new Vector3(forward.position.x, forward.position.y + 1, forward.position.z - 1);
                forward.gameObject.GetComponent<BoxManager>().Affected_boxes.check_effect_onBox();
            }
            else if (back != null)
            {
                hasSupport = true;
                //Debug.Log(the_box + "bottom <back> hasSupport."); 
                baseBox = new Vector3(back.position.x, back.position.y + 1, back.position.z + 1);
                back.gameObject.GetComponent<BoxManager>().Affected_boxes.check_effect_onBox();
            }
            else hasSupport = false;
            //if (tempBM.Notify_Affectedbox!=null)
            //    tempBM.Notify_Affectedbox();
        }
        public void Correct_basePosition()
        {
            the_box.transform.position= baseBox;
        }
    }
    static RaycastHit ray_check(Vector3 source, Vector3 direct, float ray_length)//射线检测
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(source, direct);
        Physics.Raycast(ray, out hit, ray_length);
        return hit;
    }
	
}
