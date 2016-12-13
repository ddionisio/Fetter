using UnityEngine;
using System.Collections;

public class PlayerController : M8.SingletonBehaviour<PlayerController> {
    public PlayerEntity player { get { return mPlayer; } }
    public Tumor tumor { get { return mTumor; } }

    private PlayerEntity mPlayer;
    private Tumor mTumor;

    private bool mIsFocus = true;

    public void ProcessScore(Tumor tumor, Debris debris) {
        int amount = debris.stats[StatID.Score].valueI;
    }
    
    protected override void OnInstanceDeinit() {
        if(M8.InputManager.instance)
            M8.InputManager.instance.RemoveButtonCall(0, InputAction.Menu, OnInputMenu);

        if(M8.UIModal.Manager.instance)
            M8.UIModal.Manager.instance.activeCallback -= OnUIModalActive;
    }

    protected override void OnInstanceInit() {
        mPlayer = GetComponentInChildren<PlayerEntity>();
        mTumor = GetComponentInChildren<Tumor>();

        M8.UIModal.Manager.instance.activeCallback += OnUIModalActive;

        M8.InputManager.instance.AddButtonCall(0, InputAction.Menu, OnInputMenu);
    }

    IEnumerator Start () {
        yield return null;

        mPlayer.Activate();
	}

    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;

        if(M8.UIModal.Manager.instance != null && M8.UIModal.Manager.instance.activeCount > 0) {
            Cursor.visible = true;
        }
        else {
            if(M8.SceneManager.instance != null) {
                if(mIsFocus) {
                    M8.SceneManager.instance.Resume();
                }
                else {
                    M8.SceneManager.instance.Pause();

                    //open pause menu
                }
            }

            Cursor.visible = !mIsFocus;
        }
    }

    void OnUIModalActive(bool active) {
        if(active) {
            Cursor.visible = true;

            mPlayer.inputEnabled = false;
        }
        else {
            if(mIsFocus)
                M8.SceneManager.instance.Resume();

            Cursor.visible = !mIsFocus;

            if(mPlayer.state == (int)EntityState.Normal)
                mPlayer.inputEnabled = true;
        }
    }
    
    void OnInputMenu(M8.InputManager.Info dat) {
        if(dat.state == M8.InputManager.State.Pressed) {
            if(M8.UIModal.Manager.instance != null && M8.UIModal.Manager.instance.activeCount == 0) {
                M8.SceneManager.instance.Pause();

                //open pause menu
            }
            //temp
            else if(M8.SceneManager.instance.isPaused && mIsFocus) {
                M8.SceneManager.instance.Resume();
                Cursor.visible = !mIsFocus;
            }
        }
    }
}
