using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Patterns;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Advertisements;

namespace Tetris
{
  public class Board : MonoBehaviour
  {
    public static int width = 10;
    public static int height = 20;
    private float mFallTime = 1.2f;
    private int mScoreForThisLevel = 20;
    public float[] FallTimeArray;
    public int[] ScoreArray;
    public int mLevel = 1;
    public Transform mSpawnPoint;

    public GameObject[] mBlockPrefabs;

    private int mLinesRemoved = 0;
    private float mLastLineRemovedTime = 0.0f;
    public Transform[,] mGrid;

    public Text mLevelText;
    public Text mScoreText;
    public Text mLinesText;
    public Text mMaxScoreText;
    private int mScore = 0;

    public AudioSource mAudioSource;
    public AudioClip mClearLineAudioClip;
    public AudioClip mLevelUpAudioClip;
    public AudioClip mLostAudioClip;
    public AudioClip mAddToGridAudioClip;

    public GameObject mNextBlock;
    private GameObject mNextBlockInstantiated = null;

    FiniteStateMachine<StateID> mFsm = new FiniteStateMachine<StateID>();
    Block mCurrentBlock = null;

    public CanvasConfirmA mCanvasConfirm;

    private bool mPausing = false;
    private bool mShowingAd = false;

    public Button mBtnPause;
    public Button mBtnPlay;

    private bool mDownKeyPressed = false;
    public FixedButton mBtnDown;
    int mMaxScore = 0;

    public GameObject[] mNextBlockUI;

#if (UNITY_ANDROID || UNITY_IOS)
    public RewardedAdsScipt mRewardedAds;
#endif

    #region SAVE/LOAD
    void Save()
    {
      string filename = Application.persistentDataPath + "/tetris";
      using (BinaryWriter Writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
      {
        Writer.Write(mMaxScore);
      }
    }
    void Load()
    {
      string filename = Application.persistentDataPath + "/tetris";
      if (File.Exists(filename))
      {
        using (BinaryReader Reader = new BinaryReader(File.Open(filename, FileMode.Open)))
        {
          mMaxScore = Reader.ReadInt32();
        }
      }
    }
    #endregion

    public void InstantiateBlock(int id)
    {
      GameObject obj = null;
      for(int i = 0; i < mNextBlockUI.Length; ++i)
      {
        mNextBlockUI[i].SetActive(false);
      }
      if (mNextBlockInstantiated != null)
      {
        obj = mNextBlockInstantiated;// Instantiate(mNextBlockInstantiated, mSpawnPoint.position, mSpawnPoint.rotation); ;
        obj.SetActive(true);
        obj.transform.position = mSpawnPoint.position;
        mNextBlockInstantiated = Instantiate(mBlockPrefabs[id], mNextBlock.transform.position, mNextBlock.transform.rotation);
        mNextBlockInstantiated.SetActive(false);
        mNextBlockUI[id].SetActive(true);
      }
      else
      {
        obj = Instantiate(mBlockPrefabs[id], mSpawnPoint.position, mSpawnPoint.rotation);
        int index = Random.Range(0, mBlockPrefabs.Length);
        mNextBlockInstantiated = Instantiate(mBlockPrefabs[index], mNextBlock.transform.position, mNextBlock.transform.rotation);
        mNextBlockInstantiated.SetActive(false);
        mNextBlockUI[index].SetActive(true);
      }
      //GameObject obj = Instantiate(mBlockPrefabs[id], mSpawnPoint.position, mSpawnPoint.rotation);
      mCurrentBlock = obj.GetComponent<Block>();
      if (mCurrentBlock != null)
      {
        if (!ValidMove(mCurrentBlock))
        {
          Destroy(obj);
          Lost();
        }
      }
      mCurrentBlock.mPrevTime = Time.time;
    }

    public void InstantiateRandomizeBlock()
    {
      int index = Random.Range(0, mBlockPrefabs.Length);
      InstantiateBlock(index);
    }

    public void AddToGrid(Transform child)
    {
      child.SetParent(null, true);

      int x = Mathf.RoundToInt(child.transform.position.x);
      int y = Mathf.RoundToInt(child.transform.position.y);
      mGrid[x, y] = child;
    }

    public void AddToGrid(Block blk)
    {
      StartCoroutine(AmbientSound.Coroutine_PlayShot(mAudioSource, mAddToGridAudioClip));

      // Manual reverse iteration is possible using the GetChild() and childCount variables
      // Using normal forward traversal we cannot remove the parent.
      for (int i = blk.transform.childCount - 1; i >= 0; --i)
      {
        Transform child = blk.transform.GetChild(i);
        child.SetParent(null, true);

        int x = Mathf.RoundToInt(child.transform.position.x);
        int y = Mathf.RoundToInt(child.transform.position.y);
        mGrid[x, y] = child;
      }
    }

    void UpdateScore()
    {
      mScore += mScoreForThisLevel;
      //if (mLastLineRemovedTime < 10)
      //{
      //  mScore += 100;
      //}
      //else if (mLastLineRemovedTime < 20)
      //{
      //  mScore += 80;
      //}
      //else if (mLastLineRemovedTime < 40)
      //{
      //  mScore += 40;
      //}
      //else if (mLastLineRemovedTime < 60)
      //{
      //  mScore += 20;
      //}
      mScoreText.text = mScore.ToString();
      if (mMaxScore < mScore)
      {
        mMaxScore = mScore;
      }
      mMaxScoreText.text = mMaxScore.ToString();
    }

    void OnApplicationPause()
    {
      Save();
    }

    void Awake()
    {
      Load();
    }

    void Lost()
    {
      mFsm.SetCurrentState(StateID.LOST);
    }

    void LevelUp()
    {
      mLevel += 1;
      if(FallTimeArray.Length >= mLevel)
      {
        mFallTime = FallTimeArray[mLevel-1];
      }
      if (ScoreArray.Length >= mLevel)
      {
        mScoreForThisLevel = ScoreArray[mLevel - 1];
      }
      Debug.Log("Falltime: " + mFallTime + ", LevelScore: " + mScoreForThisLevel);
      //mFallTime *= 0.75f;
      mLinesRemoved = 0;
      mLastLineRemovedTime = 0.0f;
      mLevelText.text = mLevel.ToString();
      mLinesText.text = GetLinesToClearLevel(mLevel).ToString();
    }

    public GameObject mBoardSquare;
    void CreateBoardSquares()
    {
      for (int i = 0; i < width; ++i)
      {
        for (int j = 0; j < height; ++j)
        {
          Instantiate(mBoardSquare, new Vector3(i, j, 0.0f), Quaternion.identity);
        }
      }
    }

    void Start()
    {
      mGrid = new Transform[width, height];
      mLevelText.text = mLevel.ToString();

      CreateBoardSquares();

      mFsm.Add(
          new State<StateID>(
              StateID.NEW_LEVEL,
              OnEnterNewLevel)
          );
      mFsm.Add(
          new State<StateID>(
              StateID.CLEAR_LINES,
              OnEnterClearLines)
          );
      mFsm.Add(
          new State<StateID>(
              StateID.PLAYING,
              OnEnterPlaying,
              null,
              OnUpdatePlaying)
          );
      mFsm.Add(
          new State<StateID>(
              StateID.LOST,
              OnEnterLost)
          );

      mCanvasConfirm.onClickNo = OnCancelExitGame;
      //mCanvasConfirm.onClickYes = LoadMainMenu;
      mCanvasConfirm.onClickYes = QuitGame;
      mFsm.SetCurrentState(StateID.PLAYING);

      mMaxScoreText.text = mMaxScore.ToString();
      mLinesText.text = GetLinesToClearLevel(mLevel).ToString();

#if (UNITY_ANDROID || UNITY_IOS)
      GameApp.Instance.mConfirmShowAd.onClickYes = OnShowAd;
      GameApp.Instance.mConfirmShowAd.onClickNo = OnNoReward;

      mRewardedAds.onNoReward = OnNoReward;
      mRewardedAds.onReward = OnReward;
      mRewardedAds.LoadAd();
#endif
    }

    void Update()
    {
      mFsm.Update();
    }

    IEnumerator Coroutine_OnLost()
    {
      yield return StartCoroutine(Coroutine_ClearBoard());

      // Show a reward ad. If the user watches the reward ad
      // then start with the same level.
      // else go to level 1.
#if (UNITY_ANDROID || UNITY_IOS)
      if (mRewardedAds.mAdLoaded)
      {
        GameApp.Instance.mConfirmShowAd.gameObject.SetActive(true);

        mPausing = true;
        mShowingAd = true;
      }
      else
      {
        OnNoReward();
      }
#else
      mLevel = 1;
      mScore = 0;
      mScoreText.text = mScore.ToString();
#endif
    }
#if (UNITY_ANDROID || UNITY_IOS)
    void OnShowAd()
    {
      GameApp.Instance.mConfirmShowAd.gameObject.SetActive(false);
      mRewardedAds.ShowAd();
    }
    void OnReward()
    {
      mPausing = false;
      mShowingAd = false;
      mLevel -= 1;
      LevelUp();
      mFsm.SetCurrentState(StateID.PLAYING);
      Debug.Log("Rewared with life");
    }
    void OnNoReward()
    {
      mPausing = false;
      mShowingAd = false;
      mLevel = 1;
      mScore = 0;
      mScoreText.text = mScore.ToString();
      mFsm.SetCurrentState(StateID.PLAYING);
      GameApp.Instance.mConfirmShowAd.gameObject.SetActive(false);
    }
#endif

    void OnEnterLost()
    {
      StartCoroutine(AmbientSound.Coroutine_PlayShot(mAudioSource, mLostAudioClip));
      StartCoroutine(Coroutine_OnLost());
    }

    IEnumerator Coroutine_ClearBoard()
    {
      // clear board.
      for (int i = 0; i < width; ++i)
      {
        for (int j = 0; j < height; ++j)
        {
          if (mGrid[i, j] != null)
          {
            Destroy(mGrid[i, j].gameObject);
          }
          mGrid[i, j] = null;
          yield return null;
        }
      }
    }

    IEnumerator Coroutine_LevelUp()
    {
      yield return StartCoroutine(Coroutine_ClearBoard());
      LevelUp();
      mFsm.SetCurrentState(StateID.PLAYING);
    }
    void OnEnterNewLevel()
    {
      StartCoroutine(AmbientSound.Coroutine_PlayShot(mAudioSource, mLevelUpAudioClip));
      StartCoroutine(Coroutine_LevelUp());
    }

    void OnEnterPlaying()
    {
      //Debug.Log("OnEnterPlaying");
      InstantiateRandomizeBlock();
    }

    public void BlockRotate()
    {
      if (mPausing) return;
      mCurrentBlock.transform.RotateAround(
          mCurrentBlock.transform.TransformPoint(mCurrentBlock.mRotationPoint),
          new Vector3(0.0f, 0.0f, 1.0f),
          90.0f
      );

      if (!ValidMove(mCurrentBlock))
      {
        mCurrentBlock.transform.RotateAround(
            mCurrentBlock.transform.TransformPoint(mCurrentBlock.mRotationPoint),
            new Vector3(0.0f, 0.0f, 1.0f),
            -90.0f
        );
      }
    }

    public void BlockLeft()
    {
      if (mPausing) return;
      mCurrentBlock.transform.position += new Vector3(-1.0f, 0.0f, 0.0f);
      if (!ValidMove(mCurrentBlock))
      {
        mCurrentBlock.transform.position -= new Vector3(-1.0f, 0.0f, 0.0f);
      }
    }
    public void BlockRight()
    {
      if (mPausing) return;
      mCurrentBlock.transform.position += new Vector3(1.0f, 0.0f, 0.0f);
      if (!ValidMove(mCurrentBlock))
      {
        mCurrentBlock.transform.position -= new Vector3(1.0f, 0.0f, 0.0f);
      }
    }

    public void BlockDownKeyPresses()
    {
      mDownKeyPressed = true;
    }

    void OnUpdatePlaying()
    {
      if (mShowingAd) return;
      if (mPausing) return;
#if TESTING
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                InstantiateBlock(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                InstantiateBlock(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                InstantiateBlock(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                InstantiateBlock(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                InstantiateBlock(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                InstantiateBlock(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                InstantiateBlock(6);
            }
#endif
      if (mCurrentBlock == null)
      {
        return;
      }
      if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
        BlockLeft();
      }
      if (Input.GetKeyDown(KeyCode.RightArrow))
      {
        BlockRight();
      }

      if (Input.GetKeyDown(KeyCode.UpArrow))
      {
        BlockRotate();
      }

      float fallTime = mFallTime;
      if (Input.GetKey(KeyCode.DownArrow))
      {
        mDownKeyPressed = true;
      }
      if (mBtnDown.Pressed)
      {
        mDownKeyPressed = true;
      }
      if (mDownKeyPressed)
      {
        fallTime /= 10.0f;
        mDownKeyPressed = false;
      }

      if (Time.time - mCurrentBlock.mPrevTime > fallTime)
      {
        mCurrentBlock.transform.position += new Vector3(0.0f, -1.0f, 0.0f);
        if (!ValidMove(mCurrentBlock))
        {
          mCurrentBlock.transform.position -= new Vector3(0.0f, -1.0f, 0.0f);
          mCurrentBlock.enabled = false;
          AddToGrid(mCurrentBlock);
          mFsm.SetCurrentState(StateID.CLEAR_LINES);
        }
        mCurrentBlock.mPrevTime = Time.time;
      }
    }

    public int CheckIfNeedToClearLines()
    {
      for (int j = 0; j < height; ++j)
      {
        bool removeLine = true;
        for (int i = 0; i < width; ++i)
        {
          removeLine = removeLine && mGrid[i, j] != null;
        }
        if (removeLine)
        {
          return j;
        }
      }
      return -1;
    }

    public bool ValidMove(Transform child)
    {
      int x = Mathf.RoundToInt(child.transform.position.x);
      int y = Mathf.RoundToInt(child.transform.position.y);

      if (x < 0 || x >= Board.width || y < 0 || y >= Board.height)
      {
        return false;
      }

      if (mGrid[x, y] != null)
      {
        return false;
      }
      return true;
    }

    public bool ValidMove(Block blk)
    {
      foreach (Transform child in blk.transform)
      {
        int x = Mathf.RoundToInt(child.transform.position.x);
        int y = Mathf.RoundToInt(child.transform.position.y);

        if (x < 0 || x >= Board.width || y < 0 || y >= Board.height)
        {
          return false;
        }

        if (mGrid[x, y] != null)
        {
          return false;
        }
      }
      return true;
    }


    IEnumerator RemoveLine(int id)
    {
      for (int i = 0; i < width; ++i)
      {
        Transform t = mGrid[i, id];
        Destroy(t.gameObject);
        mGrid[i, id] = null;
      }

      // bring down all the above tiles.
      // how many blocks are there above this height.
      List<Transform> blocks = new List<Transform>();
      for (int j = id; j < height; ++j)
      {
        for (int i = 0; i < width; ++i)
        {
          if (mGrid[i, j] != null)
          {
            blocks.Add(mGrid[i, j]);
            mGrid[i, j] = null;
          }
        }
      }

      for (int i = 0; i < blocks.Count; ++i)
      {
        bool flag = true;
        while (flag)
        {
          blocks[i].transform.position += new Vector3(0.0f, -1.0f, 0.0f);
          if (!ValidMove(blocks[i]))
          {
            blocks[i].transform.position -= new Vector3(0.0f, -1.0f, 0.0f);
            AddToGrid(blocks[i]);
            flag = false;
          }
        }
      }
      mLinesRemoved += 1;
      mLinesText.text = (GetLinesToClearLevel(mLevel) - mLinesRemoved).ToString();
      UpdateScore();
      mLastLineRemovedTime = Time.time - mLastLineRemovedTime;
      //Debug.Log("mLastLineRemovedTime: " + mLastLineRemovedTime);
      yield return null;
    }

    public int GetLinesToClearLevel(int level)
    {
      return 20;
    }

    IEnumerator Coroutine_RemoveLines()
    {
      int id = CheckIfNeedToClearLines();
      while (id != -1)
      {
        yield return StartCoroutine(AmbientSound.Coroutine_PlayShot(mAudioSource, mClearLineAudioClip));
        yield return StartCoroutine(RemoveLine(id));
        //yield return new WaitForSeconds(1.0f);
        id = CheckIfNeedToClearLines();
      }

      if (mLinesRemoved >= GetLinesToClearLevel(mLevel))
      {
        mFsm.SetCurrentState(StateID.NEW_LEVEL);
      }
      else
      {
        mFsm.SetCurrentState(StateID.PLAYING);
      }
    }

    void OnEnterClearLines()
    {
      StartCoroutine(Coroutine_RemoveLines());
    }

    public void OnClickExitGame()
    {
      if (mShowingAd) return;
      mCanvasConfirm.gameObject.SetActive(true);
      mPausing = true;
      mBtnPause.gameObject.SetActive(false);
      mBtnPlay.gameObject.SetActive(true);
    }

    public void OnCancelExitGame()
    {
      if (mShowingAd) return;
      mCanvasConfirm.gameObject.SetActive(false);
      mPausing = false;
      mBtnPause.gameObject.SetActive(true);
      mBtnPlay.gameObject.SetActive(false);
    }

    public void LoadMainMenu()
    {
      SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
      Save();
      Application.Quit();
    }

    public void OnClickPause()
    {
      if (mShowingAd) return;
      mPausing = true;
      mBtnPause.gameObject.SetActive(false);
      mBtnPlay.gameObject.SetActive(true);
    }

    public void OnClickPlay()
    {
      if (mShowingAd) return;
      mPausing = false;
      mBtnPause.gameObject.SetActive(true);
      mBtnPlay.gameObject.SetActive(false);
    }
  }
}
