using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HighlightingSystem;

public class playerManager : MonoBehaviour
{

    public event PushBoxEvent Notify_boxManager;

    public float per_unit = 1;//单位体积大小
    public bool Edge_Mode = false;

    private Rigidbody rigidbody_self;
    private BoxCollider collider_self;

    public Vector3 lastRunTarget,targetpos;
    public RaycastHit last_hit_below;//记录玩家上次动作脚下的box信息
    private float h, v;

    public float speed = 1;
    public float restTime = 0.2f;
    private float timer = 0;

    private AudioSource Player_SE;
    private AudioClip Normal_Move, Climbing_Move, Fall_Move, Enter_EdgeMode, Edge_Move_SE;
    private static GameObject JoyStick_ButtonUp, JoyStick_ButtonDown, JoyStick_ButtonLeft, JoyStick_ButtonRight, JoyStick_ButtonDrag;
    void Awake()
    {//加载声音资源
        Player_SE = GetComponent<AudioSource>();
        Normal_Move=Resources.Load("Sound/SE/026.Synth_rapunzel_EX-16_wav", typeof(AudioClip)) as AudioClip;
        Climbing_Move = Resources.Load("Sound/SE/000.Synth_rapunzel_A-03_wav", typeof(AudioClip)) as AudioClip;
        Fall_Move = Resources.Load("Sound/SE/002.Synth_rapunzel_A-06_wav", typeof(AudioClip)) as AudioClip;
        Enter_EdgeMode = Resources.Load("Sound/SE/001.Synth_rapunzel_A-05_wav", typeof(AudioClip)) as AudioClip;
        Edge_Move_SE = Resources.Load("Sound/SE/003.Synth_rapunzel_A-08_wav", typeof(AudioClip)) as AudioClip;
    }
    public static void loadJoyStick(){
        Debug.Log("加载虚拟按键");
        JoyStick_ButtonUp = GameObject.Find("h=1");
        JoyStick_ButtonDown = GameObject.Find("h=-1");
        JoyStick_ButtonLeft = GameObject.Find("v=-1");
        JoyStick_ButtonRight = GameObject.Find("v=1");
        JoyStick_ButtonDrag = GameObject.Find("DragButton");
        //加载虚拟按键
        UIEventListener.Get(JoyStick_ButtonUp).onPress += MyJoyStick.JoyStick_ButtonUp_OnPress;
        UIEventListener.Get(JoyStick_ButtonDown).onPress += MyJoyStick.JoyStick_ButtonDown_OnPress;
        UIEventListener.Get(JoyStick_ButtonLeft).onPress += MyJoyStick.JoyStick_ButtonLeft_OnPress;
        UIEventListener.Get(JoyStick_ButtonRight).onPress += MyJoyStick.JoyStick_ButtonRight_OnPress;
        UIEventListener.Get(JoyStick_ButtonDrag).onPress += MyJoyStick.JoyStick_Drag_OnPress;
    }
    private void play_AudioSE(AudioClip SE)
    {
        if (Player_SE.isPlaying)
            Player_SE.Stop();
        Player_SE.clip = SE;
        Player_SE.Play();
    }
    // Use this for initialization
    void Start()
    {
        rigidbody_self = GetComponent<Rigidbody>();
        collider_self = GetComponent<BoxCollider>();
        //debugText = GameObject.Find("Label").GetComponent<UILabel>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < restTime) return;

        h = MyJoyStick.GetHorizontalAxisRaw();//水平
        v = MyJoyStick.GetVerticalAxisRaw();//垂直
        
        if (h != 0 && v != 0) return;
        //changePlayerFace();
        if (Edge_Mode){Edge_Move();return;}

        Record_last_below_box();
        if (!Can_be_controlled_check()) { timer = 0; return; }
        if (check_velocity()) { timer = 0; return; }//player下落时无法被控制

        if (h != 0 || v != 0)
        {
            
            collider_self.enabled = false;
            RaycastHit hit;
            Ray ray = new Ray(transform.position, new Vector3(h , 0, v ));
            bool isHit = Physics.Raycast(ray, out hit, 1.1f );//检测移动时是否碰到物体
            try
            {
                if (drag()) { timer = 0; collider_self.enabled = true; return; }//拖拽检测
            }catch (NullReferenceException e) { }
                       
            rigidbody_self.isKinematic = true;
            if (!isHit)
            {
                //Debug.Log("正常行走.");
                play_AudioSE(Normal_Move);
                RaycastHit player_down;
                player_down = ray_check(transform.position, new Vector3(0, -1, 0), 0.5f);
                if (player_down.transform == null)
                    return;
                iTween.MoveTo(gameObject, iTween.Hash("x", transform.position.x + h, "z", transform.position.z + v, "easeType", "linear", "speed", speed));
                //iTween.MoveTo(gameObject, iTween.Hash("x", h, "z", v, "easeType", "linear", "speed", speed));
            }
            else
            {   //前方有物体的情况
                if (check_player_up()) { collider_self.enabled = true; timer = 0; return; }//正上方有物体时无法前进

                if (check_player_forward_up(hit))
                {   //可以爬上box的情况
                    //向上前进时保持在站立物体的中心
                    //Debug.Log("前方有且只有一个物体");
                    targetpos = new Vector3(hit.transform.position.x, hit.transform.position.y + 1 , hit.transform.position.z);
                    //rigidbody.MovePosition(Vector3.Lerp(transform.position, targetpos, speed * Time.deltaTime));
                    play_AudioSE(Climbing_Move);
                    iTween.MoveTo(gameObject, iTween.Hash("x", targetpos.x, "y", targetpos.y, "z", targetpos.z, "easeType", "linear", "speed", speed));
                    //iTween.MoveBy(gameObject, iTween.Hash("amount", targetpos-transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                }
                else Debug.Log("前方的物体上有1个或多个物体");
            }
            collider_self.enabled = true;
            //Debug.Log("h=" + h + "  v=" + v);
                
        }
        
        timer = 0;

        //if (m_Camera_transform != null)
        //{
        //    m_CamForward = Vector3.Scale(m_Camera_transform.forward, new Vector3(1, 0, 1)).normalized;
        //    m_Move = m_CamForward * v + m_Camera_transform.right * h;
        //}

    }

    private float last_h, last_v;
    void Record_last_below_box()
    { 
        if (h != 0 || v != 0)
        {
            last_h = h;
            last_v = v;
        }
        lastRunTarget = transform.position;
        RaycastHit temp=new RaycastHit();
        if(last_hit_below.transform!=null)
            temp = last_hit_below;
        Ray ray_below = new Ray(transform.position, new Vector3(0, -1, 0));
        if (!Physics.Raycast(ray_below, out last_hit_below, 0.5f))
        {
            last_hit_below = temp;
        }
    }

    void smoothRotate(Vector3 target)
    {
        float rotateT;
        if (lastRunTarget != target) {  
            lastRunTarget = target;  
            rotateT = 0.0f;  
        }  
        Vector3 tempVec = target;  
        tempVec.y = transform.position.y;  
        Vector3 relativePos = tempVec - transform.position;  
        Quaternion rotation = Quaternion.LookRotation(relativePos);  
        rotateT = 500 / Quaternion.Angle(transform.rotation, rotation) * Time.deltaTime;  
        if (rotateT != 1.0f) {  
            rotateT = 1.0f;  
            transform.rotation = rotation;  
        } else  
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateT);  
    }  

    private static UILabel debugText ;
    private bool from_EdgeMode2_Normal = false;
    private bool from_drag2_EdgeMode = false;
    bool Can_be_controlled_check()//player状态检查
    {//检查下方是否存在box
        if (from_EdgeMode2_Normal)
        {//从边缘模式退出后，直到玩家落地后，才能被控制
            RaycastHit player_below;
            player_below=ray_check(transform.position,  new Vector3(0, -1, 0), 0.5f);
            if (player_below.transform != null) {
                from_EdgeMode2_Normal = false;
            }
            return false;    
        }
        RaycastHit hit_below;
        hit_below=ray_check(transform.position, new Vector3(0, -1 , 0),1+0.5f);
        if (hit_below.transform!=null)
        {
            iTween.MoveTo(gameObject, iTween.Hash("x", hit_below.transform.position.x, "y", hit_below.transform.position.y + 1, "z", hit_below.transform.position.z, "easeType", "linear", "speed", speed));
            rigidbody_self.isKinematic = true;
            return true;
        }
        else
        {//下方没有物体时，切换到边缘状态
            if (!Edge_Mode)
            {
                if (last_hit_below.transform != null)
                {
					Debug.LogWarning("Player进入边缘状态,并悬挂在"+last_hit_below.transform.position); 
                    Vector3 t=new Vector3(last_hit_below.transform.position.x+last_h, last_hit_below.transform.position.y, last_hit_below.transform.position.z+last_v);
                     //transform.position = t;
                    play_AudioSE(Enter_EdgeMode);
                    iTween.MoveTo(gameObject, iTween.Hash("x", t.x, "y", t.y, "z", t.z, "easeType", "linear", "speed", speed));
                    //iTween.MoveBy(gameObject, iTween.Hash("amount", t - transform.position, "easeType", "spring", "speed", speed, "delay", .1));
                    //debugText.text = "player:" + transform.position.ToString() + "\n" +
                    //                 "last_h=" + last_h + "  last_v=" + last_v + "\n" +
                    //                   "last_hit_below:" + last_hit_below.transform.position.ToString()+"\n"+
                    //                   "targetPos:"+t.ToString();
                    rigidbody_self.isKinematic = true;
                    
                }
                Edge_Mode = true;
            }
                
            //rigidbody.useGravity = true;
            //rigidbody.isKinematic = false;
            return false;
        }
    }

    void Edge_Move()//player在边缘状态移动时的函数
    {
        
		if (h != 0 && v != 0)
			return;
        RaycastHit player_down = ray_check(transform.position, new Vector3(0, -1, 0), 0.5f);
        if (player_down.transform != null)
        {
            try
            {
                if (drag()) { timer = 0; Edge_Mode = false; return; }
            }catch(NullReferenceException e){}
        }
        if (v == -1)//退出边缘模式
        {
            rigidbody_self.useGravity = true; rigidbody_self.isKinematic = false; Edge_Mode = false;
            from_EdgeMode2_Normal = true; last_h = 0; last_v = 0;
            //debugText.text = "exit Edge_mode.";
            timer = 0;
            return;
        }
        if (v == 1)//玩家爬上box检测
        {
            if (check_player_up()) { timer = 0; return; }//正上方有物体时无法移动
			RaycastHit box_up=new RaycastHit();
			Ray ray_box_up = new Ray(last_hit_below.transform.position, new Vector3(0, 1 , 0));
			bool b = Physics.Raycast(ray_box_up, out box_up, 0.5f);
            if (!b)
            {
                //rigidbody.MovePosition(new Vector3(last_hit_below.transform.position.x, last_hit_below.transform.position.y + 1, last_hit_below.transform.position.z));
                play_AudioSE(Climbing_Move);
                iTween.MoveTo(gameObject, iTween.Hash("x", last_hit_below.transform.position.x, "y", last_hit_below.transform.position.y + 1, "z", last_hit_below.transform.position.z, "easeType", "linear", "speed", speed));
                Edge_Mode = false; last_h = 0; last_v = 0; timer = 0;
            } timer = 0;
            return;
        }
        if (h != 0 || v != 0)
        {
            check_edge(); 
            //play_AudioSE(Edge_Move_SE);
            timer = 0;
            return;
        }	
    }

    void check_edge()//边缘移动_逻辑处理
    {
        DOA_position currentBox = new DOA_position(last_hit_below.transform.position);

        if (currentBox.box_Back == transform.position || currentBox.box_Front == transform.position)
        {
            Vector3 player_e1 = new Vector3(transform.position.x + 1, transform.position.y,transform.position.z);
            Vector3 player_e2 = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
            
                RaycastHit player_side1 = ray_check(transform.position, new Vector3(1,0,0),0.5f);
                RaycastHit player_side2 = ray_check(transform.position,  new Vector3(-1, 0, 0), 0.5f);
                RaycastHit box_side1 = ray_check(last_hit_below.transform.position, 
                    new Vector3(1, 0, 0), 0.5f);
                RaycastHit box_side2 = ray_check(last_hit_below.transform.position, 
                    new Vector3(-1, 0, 0), 0.5f);
                if (h == 1)
                {
                    if (player_side1.transform != null)
                    {//player右边
                        DOA_position readyToMove_Box = new DOA_position(player_side1.transform.position);
                        if (readyToMove_Box.box_Left == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Left);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Left - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side1;
                        }
                    }
                    else if (box_side1.transform != null)
                    {//box右边
                        DOA_position readyToMove_Box = new DOA_position(box_side1.transform.position);
                        if (currentBox.box_Back == transform.position)
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            //rigidbody.MovePosition(readyToMove_Box.box_Back);
                        else iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            //rigidbody.MovePosition(readyToMove_Box.box_Front);
                        last_hit_below = box_side1;
                    }
                    else
                    {
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Right - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        //rigidbody.MovePosition(currentBox.box_Right);
                    }
                }
                if (h == -1)
                {
                    if (player_side2.transform != null)
                    {//player左边
                        DOA_position readyToMove_Box = new DOA_position(player_side2.transform.position);
                        if (readyToMove_Box.box_Right == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Back);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side2;
                        }
                    }
                    else if (box_side2.transform != null)
                    {//box左边
                        DOA_position readyToMove_Box = new DOA_position(box_side2.transform.position);
                        if (currentBox.box_Back == transform.position)
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            //rigidbody.MovePosition(readyToMove_Box.box_Back);
                        else iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            //rigidbody.MovePosition(readyToMove_Box.box_Front);
                        last_hit_below = box_side2;
                    }
                    else
                    {
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Left - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        //rigidbody.MovePosition(currentBox.box_Left);
                    }
                }    
        }
        if (currentBox.box_Left == transform.position || currentBox.box_Right == transform.position)
        {
            Vector3 player_e1 = new Vector3(transform.position.x , transform.position.y, transform.position.z+1);
            Vector3 player_e2 = new Vector3(transform.position.x , transform.position.y, transform.position.z-1);
       
            RaycastHit player_side1 = ray_check(transform.position,  new Vector3(0, 0, 1), 0.5f);
            RaycastHit player_side2 = ray_check(transform.position,  new Vector3(0, 0, -1), 0.5f);
            RaycastHit box_side1 = ray_check(last_hit_below.transform.position,
                new Vector3(0, 0, 1), 0.5f);
            RaycastHit box_side2 = ray_check(last_hit_below.transform.position,
                new Vector3(0, 0, -1), 0.5f);
            if (currentBox.box_Right == transform.position)
            {//player在currentBox的右边
                if (h == 1)
                {
                    if (player_side1.transform != null)
                    {//player前边
                        DOA_position readyToMove_Box = new DOA_position(player_side1.transform.position);
                        if (readyToMove_Box.box_Back == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Back);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side1;
                        }
                    }
                    else if (box_side1.transform != null)
                    {//box前边
                        DOA_position readyToMove_Box = new DOA_position(box_side1.transform.position);
                        //rigidbody.MovePosition(readyToMove_Box.box_Right);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Right - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        last_hit_below = box_side1;
                    }
                    else
                    {
                        //rigidbody.MovePosition(currentBox.box_Front);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                    }
                }
                if (h == -1)
                {
                    if (player_side2.transform != null)
                    {//player后边
                        DOA_position readyToMove_Box = new DOA_position(player_side2.transform.position);
                        if (readyToMove_Box.box_Front == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Front);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side2;
                        }
                    }
                    else if (box_side2.transform != null)
                    {//box后边
                        DOA_position readyToMove_Box = new DOA_position(box_side2.transform.position);
                        //rigidbody.MovePosition(readyToMove_Box.box_Right);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Right - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        last_hit_below = box_side2;
                    }
                    else
                    {
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        //rigidbody.MovePosition(currentBox.box_Back);
                    }
                }
            }
            else if (currentBox.box_Left == transform.position)
            {
                if (h == 1)
                {
                    if (player_side2.transform != null)
                    {//player后边
                        DOA_position readyToMove_Box = new DOA_position(player_side2.transform.position);
                        if (readyToMove_Box.box_Front == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Front);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side2;
                        }
                    }
                    else if (box_side2.transform != null)
                    {//box后边
                        DOA_position readyToMove_Box = new DOA_position(box_side2.transform.position);
                        //rigidbody.MovePosition(readyToMove_Box.box_Left);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Left - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        last_hit_below = box_side2;
                    }
                    else
                    {
                        //rigidbody.MovePosition(currentBox.box_Back);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                    }
                }
                if (h == -1)
                {
                    if (player_side1.transform != null)
                    {//player前边
                        DOA_position readyToMove_Box = new DOA_position(player_side1.transform.position);
                        if (readyToMove_Box.box_Back == transform.position)
                        {
                            //rigidbody.MovePosition(readyToMove_Box.box_Back);
                            iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Back - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                            last_hit_below = player_side1;
                        }
                    }
                    else if (box_side1.transform != null)
                    {//box前边
                        DOA_position readyToMove_Box = new DOA_position(box_side1.transform.position);
                        //rigidbody.MovePosition(readyToMove_Box.box_Left);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", readyToMove_Box.box_Left - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                        last_hit_below = box_side1;
                    }
                    else
                    {
                        //rigidbody.MovePosition(currentBox.box_Front);
                        iTween.MoveBy(gameObject, iTween.Hash("amount", currentBox.box_Front - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                    }
                }
            }
        }
    }

    RaycastHit ray_check(Vector3 source,Vector3 direct,float ray_length)//射线检测
    {
       RaycastHit temp=new RaycastHit();
       Ray ray = new Ray(source, direct);
       Physics.Raycast(ray, out temp, ray_length);
       return temp;
    }
    bool Check_destination_above(Vector3 destination)//检查目标上方是否存在物体
    {
        //function Edge_Move() Internal call only.
        RaycastHit destination_up = new RaycastHit();
        Ray ray = new Ray(destination, new Vector3(0, 1, 0));

        if (Physics.Raycast(ray, out destination_up, 0.5f))
        {
            Debug.Log("目标上方有物体，无法移动。");
            return true;
        } 
        else return false;
    }


    private class DOA_position//生成目标box的前后左右坐标
    {
        public Vector3 box_Left, box_Right, box_Front, box_Back;
        public DOA_position(Vector3 boxInfo)
        {
            
            box_Left = new Vector3(boxInfo.x - 1, boxInfo.y, boxInfo.z);
            box_Right = new Vector3(boxInfo.x + 1, boxInfo.y, boxInfo.z);
            box_Front = new Vector3(boxInfo.x, boxInfo.y, boxInfo.z + 1);
            box_Back = new Vector3(boxInfo.x, boxInfo.y, boxInfo.z - 1);
        }
    }
    private class MyJoyStick
    {
        static float h=0, v=0;
        public static bool JoyStick_Drag = false;
        public static void JoyStick_Drag_OnPress(GameObject g, bool isPress) { if (isPress) JoyStick_Drag = true; else JoyStick_Drag = false;  }
        public static void JoyStick_ButtonUp_OnPress(GameObject g, bool isPress) { if (isPress) v = 1; else v = 0; }
        public static void JoyStick_ButtonDown_OnPress(GameObject g, bool isPress) { if (isPress) v = -1; else v = 0; }
        public static void JoyStick_ButtonLeft_OnPress(GameObject g, bool isPress) { if (isPress) h = -1; else h = 0; }
        public static void JoyStick_ButtonRight_OnPress(GameObject g, bool isPress) { if (isPress) h = 1; else  h = 0; }
        public static float GetHorizontalAxisRaw()
        { 
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return h;
                //return Input.GetAxisRaw("Horizontal");
            }
            else
            {
                return Input.GetAxisRaw("Horizontal");
            }
        }
        public static float GetVerticalAxisRaw(){
            
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return v;
                //return Input.GetAxisRaw("Vertical");
            }
            else return Input.GetAxisRaw("Vertical");
        }    
    }
    
    public float drag_distanse=1f;
    private float drag_h,drag_v;
    bool drag()//拖动物体的实现
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0) || MyJoyStick.JoyStick_Drag)//Xbox360_Button_A
        {
            rigidbody_self.isKinematic = true;
            drag_h = MyJoyStick.GetHorizontalAxisRaw();
            drag_v = MyJoyStick.GetVerticalAxisRaw();
            if (drag_h != 0 && drag_v != 0) return true;
            RaycastHit hit;
            Ray ray = new Ray(transform.position, new Vector3(-drag_h , 0, -drag_v ));
            bool isHit = Physics.Raycast(ray, out hit, drag_distanse );

            RaycastHit hit_down;
            hit_down=ray_check(transform.position, new Vector3(0, -1, 0), 0.5f);
            if (hit_down.transform == null)
            {
                Debug.Log("玩家下方没有box,不能执行拖动"); return true;
            }
            switch (isHit)
            {
                case true:
                    //Debug.Log("向后拖动物体");
                    if (!drag_check())
                    {
                        goto onlyPush;
                    }
                    //addHighLight(hit.rigidbody.gameObject);
                    targetpos = new Vector3(transform.position.x + drag_h, transform.position.y,
                        transform.position.z + drag_v);
                    //iTween.MoveTo(gameObject, iTween.Hash("x", drag_h, "z", drag_v, "easeType", "linear", "speed", speed));
                    iTween.MoveBy(gameObject, iTween.Hash("amount", targetpos - transform.position, "easeType", "linear", "speed", speed, "delay", .1));
                    iTween.MoveBy(hit.transform.gameObject, iTween.Hash("x", drag_h, "z", drag_v, "easeType", "linear", "speed", speed, "delay", .2));
                    Debug.Log("通知" + hit.transform.gameObject.name);
                    hit.transform.gameObject.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(1f);
                    return true;
                case false:
                    //向前推动
onlyPush:           //Debug.Log("向前推动物体");
                    ray = new Ray(transform.position, new Vector3(drag_h, 0, drag_v));
                    isHit = Physics.Raycast(ray, out hit, drag_distanse);
                    //通知boxManager
                    if (Notify_boxManager!=null)
                        Notify_boxManager(this, new PlayerMoveDistance(new Vector3(drag_h, 0, drag_v),this.gameObject));
                    else Debug.Log("Notify_boxManager==null");
                    //addHighLight(hit.rigidbody.gameObject);
                    //targetpos = new Vector3(transform.position.x + drag_h, transform.position.y,
                    //    transform.position.z + drag_v);
                    //rigidbody.MovePosition(Vector3.Lerp(transform.position, targetpos, smooth));
                    
                    iTween.MoveBy(hit.transform.gameObject, iTween.Hash("x", drag_h, "z", drag_v, "easeType", "linear", "speed", speed, "delay", .1));
                    Debug.Log("player推动" + hit.transform.gameObject.name);
                    hit.transform.gameObject.GetComponent<BoxManager>().Affected_boxes.Notify_Affectedbox(1f);
                    return true;
            }
        }
            
        return false;
    }

    bool drag_check()//向后拖动时检查后方
    {
        RaycastHit hit;
        Ray ray_back = new Ray(transform.position, new Vector3(drag_h, 0, drag_v));
        bool isHit = Physics.Raycast(ray_back, out hit, drag_distanse);
        
        if (isHit)
        {
            Debug.Log("拖拽时检测到后方有box,不能向后拖动"); return false;
        }
        else return true;
    }

    bool check_velocity() //检查player的下落速度
    {
        //Debug.Log(rigidbody.velocity.y);
        if (rigidbody_self.velocity.y != 0)
            return true;
        else return false;
    }

    bool check_player_up()//检查player正上方是否有物体
    {
        RaycastHit hit_up;//前方物体上方的物体
        Ray ray_up = new Ray(transform.position, new Vector3(0, 1 , 0)); 
        bool b = Physics.Raycast(ray_up, out hit_up, 0.5f);
        if (b)
        {
            Debug.Log("player正上方存在物体");
            return true;
        }
        return false;
    }
    bool check_player_forward_up(RaycastHit hit_forward)//检查前方物体上方的物体
    {

        RaycastHit forwards_up;
        Ray ray_forwards_up;
        if (hit_forward.transform != null)
        {
            if (h != 0 || v != 0)
            {

                if (hit_forward.transform.position != transform.position)
                {
                    //Debug.Log("player坐标=" + transform.position);
                    //Debug.Log("前方物体的坐标=" + hit_forward.transform.position);
                    //检查前方物体正上方有没有物体
                    ray_forwards_up = new Ray(hit_forward.transform.position, new Vector3(0, 1 , 0));
                    bool b = Physics.Raycast(ray_forwards_up, out forwards_up, 0.5f );
                    if (!b)
                    {
                        return true;
                    }
                    else Debug.Log("物体正上方的物体的坐标=" + forwards_up.transform.position);

                }
            }
        }
        return false;
    }

    private HighlighterController HL;
    void addHighLight(GameObject g)
    {  
        
            HL = g.GetComponent<HighlighterController>();
            if (HL == null)
                HL = g.AddComponent<HighlighterController>();
            HL.MouseOver();
        
        //mHL.on(Color.red);
    }
}

