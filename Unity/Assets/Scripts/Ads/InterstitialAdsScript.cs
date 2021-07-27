using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdsScript : MonoBehaviour, IUnityAdsListener
{
#if UNITY_ANDROID
  readonly string gameId = "4218775";
  readonly string mInterstitialSurfaceId = "Interstitial_Android";
  readonly string mBannerSurfaceId = "Banner_Android";
  readonly string mRewardSurfaceId = "Rewarded_Android";
#endif
#if UNITY_IOS
    readonly string gameId = "4218774";
    readonly string mInterstitialSurfaceId = "Interstitial_iOS";
    readonly string mBannerSurfaceId = "Banner_iOS";
#endif
#if (UNITY_ANDROID || UNITY_IOS)
  bool mTestMode = true;
#endif

  public delegate void DelegateAdFinish(string surfacingId, ShowResult showResult);
  public delegate void DelegateAdReady(string surfacingId);
  public DelegateAdFinish onAdFinish;
  public DelegateAdFinish onAdSkipped;
  public DelegateAdFinish onAdError;
  public DelegateAdReady onAdReady;

  void Start()
  {
#if (UNITY_ANDROID || UNITY_IOS)
    Advertisement.AddListener(this);
    Advertisement.Initialize(gameId, mTestMode);
    Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
    StartCoroutine(ShowBannerWhenInitialized());
#endif
  }

  public void ShowInterstitialAd()
  {
#if (UNITY_ANDROID || UNITY_IOS)
    if (Advertisement.IsReady())
    {
      Advertisement.Show(mInterstitialSurfaceId);
    }
    else
    {
      Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
    }
#endif
  }

  public void ShowRewardAd()
  {
#if (UNITY_ANDROID || UNITY_IOS)
    if (Advertisement.IsReady())
    {
      Advertisement.Show(mRewardSurfaceId);
    }
    else
    {
      Debug.Log("Reward ad not ready at the moment! Please try again later!");
    }
#endif
  }

#if (UNITY_ANDROID || UNITY_IOS)
  IEnumerator ShowBannerWhenInitialized()
  {
    while (!Advertisement.isInitialized)
    {
      yield return new WaitForSeconds(2.0f);
    }
    Advertisement.Banner.Show(mBannerSurfaceId);
  }
#endif

  public void OnUnityAdsReady(string surfacingId)
  {
    onAdReady?.Invoke(surfacingId);
  }

  public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult)
  {
    // Define conditional logic for each ad completion status:
    if (showResult == ShowResult.Finished)
    {
      // Reward the user for watching the ad to completion.
      onAdFinish?.Invoke(surfacingId, showResult);
    }
    else if (showResult == ShowResult.Skipped)
    {
      // Do not reward the user for skipping the ad.
      onAdSkipped?.Invoke(surfacingId, showResult);
    }
    else if (showResult == ShowResult.Failed)
    {
      //Debug.LogWarning(“The ad did not finish due to an error.”);
      onAdError?.Invoke(surfacingId, showResult);
    }
  }

  public void OnUnityAdsDidError(string message)
  {
    // Log the error.
  }

  public void OnUnityAdsDidStart(string surfacingId)
  {
    // Optional actions to take when the end-users triggers an ad.
  }
}
