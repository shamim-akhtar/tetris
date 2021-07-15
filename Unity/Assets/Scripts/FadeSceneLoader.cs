using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeSceneLoader : Patterns.Singleton<FadeSceneLoader>
{
    public Image Filler;
    public Text LoadingText;
    public void FadeSceneLoad(string sceneName)
    {
        StartCoroutine(Coroutine_FadeSceneLoad(sceneName));
    }

    IEnumerator Coroutine_FadeSceneLoad(string sceneName)
    {
        yield return StartCoroutine(Utils.Coroutine_KeepValue(Filler, 0.1f, 1.0f));
        LoadingText.gameObject.SetActive(true);
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(Utils.Coroutine_FadeOut(Filler, 1.0f));
        LoadingText.gameObject.SetActive(false);
    }
}
