using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public int maxHealth = 3;
    public float invulDelay = 1.0f;

    public const int StateNormal = 0;
    public const int StatePower = 1;
    public const int StateDead = 2;

    private int mCurHealth;
    private bool mIsFocus = true;

    public void Hurt() {
        if(mCurHealth > 0 && !isBlinking) {
            mCurHealth--;
            if(mCurHealth == 0) {
                //game over
                state = StateDead;
            }
            else {
                Blink(invulDelay);
            }
        }
    }

    protected override void StateChanged() {
    }
    
    protected override void OnDestroy() {
        if(Main.instance != null && Main.instance.input != null)
            Main.instance.input.RemoveButtonCall(0, InputAction.Menu, OnInputMenu);

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        autoSpawnFinish = true;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        Main.instance.input.AddButtonCall(0, InputAction.Menu, OnInputMenu);
    }

    protected override void SpawnStart() {
        mCurHealth = maxHealth;

        //init hud
    }

    public override void SpawnFinish() {
        state = StateNormal;
    }
    
    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;

        if(UIModalManager.instance != null && UIModalManager.instance.activeCount > 0) {
            Screen.showCursor = true;
        }
        else {
            if(Main.instance != null) {
                if(mIsFocus) {
                    Main.instance.sceneManager.Resume();
                }
                else {
                    Main.instance.sceneManager.Pause();

                    //open pause menu
                }
            }

            Screen.showCursor = !mIsFocus;
        }
    }

    void OnUIModalActive() {
        Screen.showCursor = true;
    }

    void OnUIModalInactive() {
        if(mIsFocus)
            Main.instance.sceneManager.Resume();

        Screen.showCursor = !mIsFocus;
    }

    void OnInputMenu(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(UIModalManager.instance != null && UIModalManager.instance.activeCount == 0) {
                Main.instance.sceneManager.Pause();

                //open pause menu
            }
            //temp
            else if(Main.instance.sceneManager.isPaused && mIsFocus) {
                Main.instance.sceneManager.Resume();
                Screen.showCursor = !mIsFocus;
            }
        }
    }
}
