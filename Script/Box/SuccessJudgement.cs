using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Threading;
public class SuccessJudgement : MonoBehaviour {

    public static AudioSource Scene_BGM;
    private GameObject Spotlight, Scene_Light;
    void Awake()
    {
        Scene_BGM = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        PushOverEvent = GameOver;
        PushOverEvent+=GameObject.Find("UI_Camera").GetComponent<MissionUIManager>().GameOver_prop;
    }
    void Start()
    {
        Spotlight = GameObject.Find("Spotlight");
        Scene_Light = GameObject.Find("Scene Light");
    }
    void Update()
    {
    }
    public void Success()
    {
            Debug.LogWarning("玩家达到了终点！");
            Scene_BGM.Pause();
            Scene_Light.SetActive(false);
            Spotlight.SetActive(true);
            StartCoroutine("Play_SuccessSE_and_LoadNextMission");
    }
    IEnumerator Play_SuccessSE_and_LoadNextMission()
    {
        Scene_BGM.clip = Resources.Load("Sound/SE/029.Synth_rapunzel_EX-19_wav", typeof(AudioClip)) as AudioClip;
        Scene_BGM.Play();
        yield return new WaitForSeconds(Scene_BGM.clip.length);
        Debug.LogWarning("029.Synth_rapunzel_EX-19_wav播放完毕");
        Scene_BGM.clip = Resources.Load("Sound/10013_C03_wav.aax", typeof(AudioClip)) as AudioClip;
        Scene_BGM.Play();
        yield return new WaitForSeconds(Scene_BGM.clip.length);
        Debug.LogWarning("10013_C03_wav.aax播放完毕");
        Scene_BGM.clip = Resources.Load("Sound/SE/030.Synth_rapunzel_EX-20_wav", typeof(AudioClip)) as AudioClip;
        Scene_BGM.loop = false;
        Scene_BGM.Play();
        //yield return new WaitForSeconds(Scene_BGM.clip.length);
        //SceneManager.LoadScene("Mission_" + Mission_id);
    }

    public delegate void GameOver_Push();
    public GameOver_Push PushOverEvent;
    void GameOver()
    {
        StartCoroutine("Play_GameOverSE");
        Destroy(GameObject.Find("testPlayer"));
    }
    IEnumerator Play_GameOverSE()
    {
        Scene_BGM.Pause();
        Scene_BGM.clip = Resources.Load("Sound/gameover_16bit_wav.aax", typeof(AudioClip)) as AudioClip;
        Scene_BGM.loop = false;
        yield return new WaitForSeconds(0.7f);
        Scene_BGM.Play();
    }
}
