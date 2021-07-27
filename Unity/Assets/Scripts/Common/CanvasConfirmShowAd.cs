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
    gameObject.SetActive(false);
    onClickNo?.Invoke();
  }

  public void OnClickYes()
  {
    gameObject.SetActive(false);
    onClickYes?.Invoke();
  }
}
