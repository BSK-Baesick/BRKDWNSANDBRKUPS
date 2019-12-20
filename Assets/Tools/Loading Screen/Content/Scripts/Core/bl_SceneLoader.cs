using UnityEngine;
using UnityEngine.UI;
using Lovatto.SceneLoader;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class bl_SceneLoader : MonoBehaviour
{
    [Header("Settings")]
    public SceneSkipType SkipType = SceneSkipType.Button;
    [Range(0.5f,7)]public float SceneSmoothLoad = 3;
    [Range(0.5f,7)]public float FadeInSpeed = 2;
    [Range(0.5f,7)]public float FadeOutSpeed = 2;
    public bool useTimeScale = false;
    [Header("Background")]
    public bool useBackgrounds = true;
    [Range(1,60)]public float TimePerBackground = 5;
    [Range(0.5f,7)]public float FadeBackgroundSpeed = 2;
    [Range(0.5f,5)]public float TimeBetweenBackground = 0.5f;
    [Header("Tips")]
    public bool RandomTips = false;
    [Range(1,60)]public float TimePerTip = 5;
    [Range(0.5f,5)]public float FadeTipsSpeed = 2;
    [Header("Loading")]
    public bool FadeLoadingBarOnFinish = false;
    [Range(50,1000)]public float LoadingCircleSpeed = 300;
    [TextArea(2,2)]public string LoadingTextFormat = "{0}";

    [Header("Audio")]
    [Range(0.1f,1)]public float AudioVolume = 1f;
    [Range(0.5f,5)]public float FadeAudioSpeed = 0.5f;
    [Range(0.1f,1)]public float FinishSoundVolume = 0.5f;
    [SerializeField]private AudioClip FinishSound = null;
    [SerializeField]private AudioClip BackgroundAudio = null;

    [Header("References")]
    [SerializeField]private Text SceneNameText = null;
    [SerializeField]private Text DescriptionText = null;
    [SerializeField]private Text ProgressText = null;
    [SerializeField]private Text TipText = null;
    [SerializeField]private Image BackgroundImage = null;
    [SerializeField]private Image FilledImage = null;
    [SerializeField]private Slider LoadBarSlider = null;
    [SerializeField]private GameObject ContinueUI = null;
    [SerializeField]private GameObject RootUI;
    [SerializeField]private GameObject FlashImage = null;
    [SerializeField]private GameObject SkipKeyText = null;
    [SerializeField]private RectTransform LoadingCircle = null;
    [SerializeField]private CanvasGroup LoadingCircleCanvas = null;
    [SerializeField]private CanvasGroup FadeImageCanvas = null;

    private bl_SceneLoaderManager Manager = null;
    private AsyncOperation async;
    private bool isOperationStarted = false;
    private bool FinishLoad = false;
    private CanvasGroup RootAlpha;
    private CanvasGroup BackgroundAlpha = null;
    private CanvasGroup LoadingBarAlpha = null;
    private bool isTipFadeOut = false;
    private int CurrentTip = 0;
    private List<string> cacheTips = new List<string>();
    private int CurrentBackground = 0;
    private List<Sprite> cacheBackgrounds = new List<Sprite>();
    private AudioSource Source = null;
    private float lerpValue = 0;
    private bool canSkipWithKey = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Manager = bl_SceneLoaderUtils.GetSceneLoaderManager();

        //Setup Audio Source
        Source = GetComponent<AudioSource>();
        Source.volume = 0;
        Source.loop = true;
        if (BackgroundAudio != null) { Source.clip = BackgroundAudio; }

        //Setup UI
        RootUI.SetActive(false);
        RootAlpha = RootUI.GetComponent<CanvasGroup>();
        if (ContinueUI != null)
        {
            ContinueUI.SetActive(false);
        }
        if(FlashImage != null) { FlashImage.SetActive(false); }
        if (FadeImageCanvas != null)
        {
            FadeImageCanvas.alpha = 1;
            StartCoroutine(FadeOutCanvas(FadeImageCanvas));
        }
        if(SkipKeyText != null) { SkipKeyText.SetActive(false); }
        if(LoadBarSlider != null) { LoadingBarAlpha = LoadBarSlider.GetComponent<CanvasGroup>(); }
        if(BackgroundImage != null) { BackgroundAlpha = BackgroundImage.GetComponent<CanvasGroup>(); }
        if (Manager.HasTips) { cacheTips = Manager.TipsList; }
        if(FilledImage != null) { FilledImage.type = Image.Type.Filled; FilledImage.fillAmount = 0; }
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (!isOperationStarted)
            return;
        if (async == null)
            return;

        UpdateUI();
        LoadingRotator();
        SkipWithKey();
    }

    /// <summary>
    /// 
    /// </summary>
    void SkipWithKey()
    {
        if (!canSkipWithKey)
            return;

        if (Input.anyKeyDown)
        {
            LoadNextScene();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateUI()
    {
        if (LoadBarSlider != null)
        {
            //Get progress of load level
            float Extra = (SkipType == SceneSkipType.InstantComplete) ? 0 : 0.1f;
            float p = (async.progress + Extra); //Fix problem of 90%
            //Smooth slider to percent.
            LoadBarSlider.value = Mathf.Lerp(LoadBarSlider.value, p, DeltaTime * SceneSmoothLoad);
            if(FilledImage != null) { FilledImage.fillAmount = LoadBarSlider.value; }
            //When already load the next level
            if (async.isDone || LoadBarSlider.value > 0.99f)
            {
                //Called one time what is inside in this function.
                if (!FinishLoad)
                {
                    OnFinish();
                }
            }
            if (ProgressText != null)
            {
                string percent = (LoadBarSlider.value * 100).ToString("F0");
                ProgressText.text = string.Format(LoadingTextFormat, percent);
            }
        }
        else
        {
            //Get progress of load level
            float Extra = (SkipType == SceneSkipType.InstantComplete) ? 0 : 0.1f;
            float p = (async.progress + Extra); //Fix problem of 90%
            lerpValue = Mathf.Lerp(lerpValue, p, DeltaTime * SceneSmoothLoad);
            if (FilledImage != null) { FilledImage.fillAmount = lerpValue; }
            if (async.isDone || lerpValue > 0.99f)
            {
                //Called one time what is inside in this function.
                if (!FinishLoad)
                {
                    OnFinish();
                }
            }

            if (ProgressText != null)
            {
                string percent = (lerpValue * 100).ToString("F0");
                ProgressText.text = string.Format(LoadingTextFormat, percent);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void LoadingRotator()
    {
        if (LoadingCircle == null)
            return;

        LoadingCircle.Rotate(-Vector3.forward * DeltaTime * LoadingCircleSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnFinish()
    {
        FinishLoad = true;
        if (FlashImage != null) { FlashImage.SetActive(true); }
        //Can skip when next level is loaded.
        if (SkipType == SceneSkipType.Button)
        {
            if (ContinueUI != null)
            {
                ContinueUI.SetActive(true);
            }
        }
        else if (SkipType == SceneSkipType.Instant)
        {
            LoadNextScene();
        }
        else if (SkipType == SceneSkipType.InstantComplete)
        {
            LoadNextScene();
        }
        else if (SkipType == SceneSkipType.AnyKey)
        {
            canSkipWithKey = true;
            if (SkipKeyText != null) { SkipKeyText.SetActive(true); }
        }

        if (FinishSound != null) { AudioSource.PlayClipAtPoint(FinishSound, transform.position,FinishSoundVolume); }
        if (LoadingCircleCanvas != null) { StartCoroutine(FadeOutCanvas(LoadingCircleCanvas, 0.5f)); }
        if (LoadingBarAlpha != null && FadeLoadingBarOnFinish) { StartCoroutine(FadeOutCanvas(LoadingBarAlpha, 1)); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadLevel(string level)
    {
        bl_SceneLoaderInfo sli = Manager.GetSceneInfo(level);
        if (sli == null)
            return;

        SetupUI(sli);
        StartCoroutine(StartAsyncOperation(sli.SceneName));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void SetupUI(bl_SceneLoaderInfo info)
    {
        if (BackgroundImage != null && useBackgrounds)
        {
            if (info.Backgrounds.Length > 1)
            {
                cacheBackgrounds.AddRange(info.Backgrounds);
                StartCoroutine(BackgroundTransition());
                BackgroundImage.color = Color.white;
            }
            else if(info.Backgrounds != null && info.Backgrounds.Length > 0)
            {
                BackgroundImage.sprite = info.Backgrounds[0];
                BackgroundImage.color = Color.white;
            }
        }
        if(SceneNameText != null) { SceneNameText.text = info.DisplayName; }
        if(DescriptionText != null) { DescriptionText.text = info.Description; }
        if(LoadBarSlider != null) { LoadBarSlider.value = 0; }
        if (ProgressText != null)
        {
            ProgressText.text = string.Format(LoadingTextFormat, 0);
        }
        if (Manager.HasTips && TipText != null)
        {
            if (RandomTips)
            {
                CurrentTip = Random.Range(0, cacheTips.Count);
                TipText.text = cacheTips[CurrentTip];
            }
            else
            {
                TipText.text = cacheTips[0];
            }
            StartCoroutine(TipsLoop());
        }
        //Show all UI
        RootAlpha.alpha = 0;
        RootUI.SetActive(true);

        //start audio loop
        Source.Play();
        StartCoroutine(FadeAudio(true));
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadNextScene()
    {
        //fade audio loop
        StartCoroutine(FadeAudio(false));
        StartCoroutine(LoadNextSceneIE());
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator StartAsyncOperation(string level)
    {
        while(RootAlpha.alpha < 1)
        {
            RootAlpha.alpha += DeltaTime * FadeInSpeed;
            yield return null;
        }
        async = bl_SceneLoaderUtils.LoadLevelAsync(level);
        if (SkipType != SceneSkipType.InstantComplete)
        {
            async.allowSceneActivation = false;
        }
        else
        {
            async.allowSceneActivation = true;
        }
        isOperationStarted = true;
        yield return async;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator BackgroundTransition()
    {
        while (true)
        {
            BackgroundImage.sprite = cacheBackgrounds[CurrentBackground];
            while (BackgroundAlpha.alpha < 1)
            {
                BackgroundAlpha.alpha += DeltaTime * FadeBackgroundSpeed * 0.8f;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(TimePerBackground);
            while(BackgroundAlpha.alpha > 0)
            {
                BackgroundAlpha.alpha -= DeltaTime * FadeBackgroundSpeed;
                yield return new WaitForEndOfFrame();
            }
            CurrentBackground = (CurrentBackground + 1) % cacheBackgrounds.Count;
            yield return new WaitForSeconds(TimeBetweenBackground);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator TipsLoop()
    {
        if (TipText == null)
            yield break;

        Color alpha = TipText.color;
        if (isTipFadeOut)
        {
            while (alpha.a < 1)
            {
                alpha.a += DeltaTime * FadeTipsSpeed;
                TipText.color = alpha;
                yield return null;
            }
            StartCoroutine(WaitNextTip(TimePerTip));
        }
        else
        {
            while (alpha.a > 0)
            {
                alpha.a -= DeltaTime * FadeTipsSpeed;
                TipText.color = alpha;
                yield return null;
            }
            StartCoroutine(WaitNextTip(0.75f));
        }
        if (isTipFadeOut)
        {
            if (RandomTips)
            {
                int lastTip = CurrentTip;
                CurrentTip = Random.Range(0, cacheTips.Count);
                while (CurrentTip == lastTip)
                {
                    CurrentTip = Random.Range(0, cacheTips.Count);
                    yield return null;
                }
                TipText.text = cacheTips[CurrentTip];
            }
            else
            {
                CurrentTip = (CurrentTip + 1) % cacheTips.Count;
                TipText.text = cacheTips[CurrentTip];
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeAudio(bool FadeIn)
    {
        if (BackgroundAudio == null)
            yield break;

        if (FadeIn)
        {
            while(Source.volume < AudioVolume)
            {
                Source.volume += DeltaTime * FadeAudioSpeed;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (Source.volume > 0)
            {
                Source.volume -= DeltaTime * FadeAudioSpeed * 3;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator WaitNextTip(float t)
    {
        isTipFadeOut = !isTipFadeOut;
        yield return new WaitForSeconds(t);
        StartCoroutine(TipsLoop());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadNextSceneIE()
    {
        FadeImageCanvas.alpha = 0;
        while (FadeImageCanvas.alpha < 1)
        {
            FadeImageCanvas.alpha += DeltaTime * FadeInSpeed;
            yield return null;
        }
        async.allowSceneActivation = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator FadeOutCanvas(CanvasGroup alpha,float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        while (alpha.alpha > 0)
        {
            alpha.alpha -= DeltaTime * FadeOutSpeed;
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator FadeInCanvas(CanvasGroup alpha, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        while (alpha.alpha < 1)
        {
            alpha.alpha += DeltaTime * FadeInSpeed;
            yield return null;
        }
    }

    private float DeltaTime
    {
        get { return (useTimeScale) ? Time.deltaTime : Time.unscaledDeltaTime; }
    }
}