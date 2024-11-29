using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Animator))]
public class TransicionManager : MonoBehaviour
{
    private static TransicionManager instance;
    public static TransicionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<TransicionManager>("TansicionManager"));
                instance.Init();
            }
            return instance;
        }
    }

    public const string SCENE_NAME_MAIN_MENU = "MainMenu";
    public const string SCENE_NAME_GAME = "Mapa";

    //public Slider progresoSlider;
    public Scrollbar progresoSlider;
    public TextMeshProUGUI progresoLabel;
    //public TextMeshProUGUI transitionInfLabel;
    [Multiline]
    public string[] gameInfo = new string[0];

    private Animator animator;
    private int HashShowAnim = Animator.StringToHash("Show");


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Init();
        }else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadCoroutine(sceneName));
    }

    IEnumerator LoadCoroutine(string sceneName)
    {
        animator.SetBool(HashShowAnim, true);
        //if (transitionInfLabel != null)
        //    transitionInfLabel.text = gameInfo[Random.Range(0, gameInfo.Length -1)];

        UpdateProgressValue(0);

        yield return new WaitForSeconds(0.5f);
        var sceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!sceneAsync.isDone)
        {
            UpdateProgressValue(sceneAsync.progress);

            yield return null;
        }

        UpdateProgressValue(1);
        animator.SetBool(HashShowAnim, false);
    }

    void UpdateProgressValue(float progress)
    {
        if (progresoSlider != null)
            progresoSlider.value = progress;

        if (progresoLabel.text != null)
            progresoLabel.text = $"{progress * 100}%";
    }
}
