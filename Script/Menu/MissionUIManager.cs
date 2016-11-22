using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Threading;
public class MissionUIManager : MonoBehaviour {

    public AudioSource UIAudioSrc,Scene_BGM;
    public GameObject Main_Camera, BackGroundPic, ULabel, UI_Camera, GameOver_Menu;
    public GameObject JoyStickPanel_Left, DragButton;
    public float SceneBGM_volume = 0.5f;
    private GameObject Spotlight;
    public int Mission_id=0;
    void Awake()
    {
        Main_Camera = GameObject.Find("Main Camera");
        Spotlight = GameObject.Find("Spotlight");
        BackGroundPic = GameObject.Find("BackGroundPic");
        ULabel = GameObject.Find("ULabel");
        UI_Camera = GameObject.Find("UI_Camera");
        UIAudioSrc = GameObject.Find("UI_Camera").GetComponent<AudioSource>();
        JoyStickPanel_Left = GameObject.Find("JoyStickPanel_Left");
        DragButton = GameObject.Find("DragButton");
        GameOver_Menu = GameObject.Find("GameOver_Menu");
        Scene_BGM = Main_Camera.GetComponent<AudioSource>();
        Scene_BGM.clip = Resources.Load("Sound/10000_A01_wav.aax", typeof(AudioClip)) as AudioClip;
        Scene_BGM.loop = true;
        Scene_BGM.volume = SceneBGM_volume;
    }
    void Start()
    {
        //Mission_id = ES2.Load<int>(Application.persistentDataPath+"/LvInfo.kanb?tag=Mission_id");
        Mission_id = PlayerPrefs.GetInt("Mission_id");
        StartCoroutine("MissionShow");
        JoyStickPanel_Left.SetActive(false);
        DragButton.SetActive(false);
        GameOver_Menu.SetActive(false);
    }
    IEnumerator MissionShow()
    {      
        Main_Camera.SetActive(false);
        UI_Camera.AddComponent<AudioListener>();
        UIAudioSrc.Play();
        yield return new WaitForSeconds(UIAudioSrc.clip.length);
        Debug.LogWarning("10012_C02_wav.aax播放完毕，进入关卡");
        BackGroundPic.SetActive(false);
        ULabel.SetActive(false);
        Main_Camera.SetActive(true);
        Spotlight.SetActive(false);
        Scene_BGM.Play();
        JoyStickPanel_Left.SetActive(true);
        DragButton.SetActive(true);
        playerManager.loadJoyStick();
    }

    public void GameOver_prop()
    {
        BackGroundPic.SetActive(true);
        JoyStickPanel_Left.SetActive(false);
        DragButton.SetActive(false);
        GameOver_Menu.SetActive(true);
        Destroy(Main_Camera.GetComponent<CameraFollow>());
    }
    public void Retry_Button_OnClick()
    {
        if (int.Parse(GameObject.Find("Coin").GetComponent<UILabel>().text) > 0)
        {
            SceneManager.LoadScene("Mission_" + Mission_id);
        }
    }
    public void Exit_Button_OnClick()
    {
        SceneManager.LoadScene("Main_menu");
    }
}
