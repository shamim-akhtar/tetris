using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAdsScript : MonoBehaviour, IUnityAdsListener
{
  [SerializeField] string _androidAdUnitId = "Banner_Android";
  [SerializeField] string _iOsAdUnitId = "Banner_iOS";
  string _adUnitId;

  public delegate void DelegateOnReward();
  public DelegateOnReward onReward;
  public DelegateOnReward onNoReward;

  [HideInInspector]
  public bool mAdLoaded = false;

  void Awake()
  {
    _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
        ? _iOsAdUnitId
        : _androidAdUnitId;

    Advertisement.AddListener(this);
  }

  public void OnUnityAdsDidError(string message)
  {
  }

  public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
  {
  }

  public void OnUnityAdsDidStart(string placementId)
  {
  }

  public void OnUnityAdsReady(string placementId)
  {
  }

  // Start is called before the first frame update
  void Start()
  {
    Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
    StartCoroutine(ShowBannerWhenInitialized());
  }
#if (UNITY_ANDROID || UNITY_IOS)
  IEnumerator ShowBannerWhenInitialized()
  {
    while (!Advertisement.isInitialized)
    {
      yield return new WaitForSeconds(5.0f);
    }
    Advertisement.Banner.Show(_adUnitId);
  }
#endif

}
