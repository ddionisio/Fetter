using UnityEngine;
using System.Collections;

public class HeartSpriteController : MonoBehaviour {
    public enum State {
        active,
        power,
        dead
    }

    private tk2dSpriteAnimator mAnim;
    private SpriteColorBlink mBlinker;
    private tk2dSpriteAnimationClip[] mClips;

    void Awake() {
        mAnim = GetComponent<tk2dSpriteAnimator>();
        
        mBlinker = GetComponent<SpriteColorBlink>();
        if(mBlinker != null)
            mBlinker.enabled = false;

        mClips = M8.tk2dUtil.GetSpriteClips(mAnim, typeof(State));
    }
        
    void EntityStart(EntityBase ent) {
        ent.spawnCallback += OnPlayerSpawn;
        ent.setStateCallback += OnPlayerSetState;
        ent.setBlinkCallback += OnPlayerBlink;
    }

    void OnPlayerSpawn(EntityBase ent) {
        if(mBlinker != null)
            mBlinker.enabled = false;
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch(state) {
            case Player.StateNormal:
                mAnim.Play(mClips[(int)State.active]);
                break;
        }
    }

    void OnPlayerBlink(EntityBase ent, bool b) {
        if(mBlinker != null)
            mBlinker.enabled = b;
    }

    // Update is called once per frame
    //void Update () {

    //}
}
