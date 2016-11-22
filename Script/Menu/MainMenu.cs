using System;
using System.CodeDom;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private GameObject Label,selectMenu;
    public GameObject selectMission_Label, Forward_Button, Back_Button, misson;
    //public const String TAG_selectMission_Label="Label (UnityEngine.GameObject)",
    //                        TAG_Forward_Label="forwardLabel (UnityEngine.GameObject)",
    //                        TAG_Back_Label = "backLabel (UnityEngine.GameObject)",
    //                        TAG_missonLabel = "missionLabel (UnityEngine.GameObject)";
    private Animation Label_Anime;

    void Awake()
    {
        Label = GameObject.FindGameObjectWithTag("TouchLabel");
        selectMenu = GameObject.Find("selectMenu");

        selectMission_Label.SetActive(false);
        Forward_Button.SetActive(false);
        Back_Button.SetActive(false);
        misson.SetActive(false);

    }

    private bool play_once = false;
    public void TouchToStart()
    {
        if (!play_once)
        {
            Label_Anime = Label.GetComponent<Animation>();
            Label_Anime.Stop();
            Label_Anime.clip = Label_Anime.GetClip("Label_Animation_2");
            Label_Anime.Play();
            GameObject.Find("Label").GetComponent<AudioSource>().Play();
            play_once = true;
        }
    }

    public void onAnimeEnd()
    {
        Label.SetActive(false);
        selectMission_Label.SetActive(true);
        Forward_Button.SetActive(true);
        if (!misson.GetComponent<UILabel>().text.Equals("1"))
            Back_Button.SetActive(true);
        misson.SetActive(true);
    }

    public void press_ButtonForward()
    {
        GameObject.Find("forwardButton").GetComponent<AudioSource>().Play();
        int Mission_id = int.Parse(misson.GetComponent<UILabel>().text);
        if (!Back_Button.activeInHierarchy && Mission_id == 1)
            Back_Button.SetActive(true);
        Mission_id++;
        misson.GetComponent<UILabel>().text = "" + Mission_id;
    }

    public void press_ButtonBack()
    {
        GameObject.Find("backButton").GetComponent<AudioSource>().Play();
        int Mission_id = int.Parse(misson.GetComponent<UILabel>().text);
        if (Mission_id - 2 == 0)
            Back_Button.SetActive(false);
        Mission_id--;
        misson.GetComponent<UILabel>().text = "" + Mission_id;
    }
    public void loadMission()
    {
        int Mission_id =int.Parse(misson.GetComponent<UILabel>().text);
        PlayerPrefs.SetInt("Mission_id", Mission_id);
        ES2.Save(Mission_id, Application.persistentDataPath+"/LvInfo.kanb?tag=Mission_id");
        Debug.LogWarning("加载场景：Mission_" + Mission_id);
        SceneManager.LoadScene("Mission_" + Mission_id);
    }
     
    public void loadTestMission()
    {
        SceneManager.LoadScene("Mission test");
    }
}
