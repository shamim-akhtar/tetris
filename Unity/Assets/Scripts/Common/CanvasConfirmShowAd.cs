using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasConfirmShowAd : MonoBehaviour
{
  public Button mYes;
  public Button mNo;

  public delegate void DelegateOnClick();
  public DelegateOnClick onClickYes;
  public DelegateOnClick onClickNo;

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void OnClickNo()
  {
    onClickNo?.Invoke();
    gameObject.SetActive(false);
  }

  public void OnClickYes()
  {
    onClickYes?.Invoke();
    gameObject.SetActive(false);
  }
}
