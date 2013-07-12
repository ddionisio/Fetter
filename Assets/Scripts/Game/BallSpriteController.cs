using UnityEngine;
using System.Collections;

public class BallSpriteController : MonoBehaviour {
    public enum State {
        idle,
        power
    }

    public const string spriteEyePrefix = "ball_eye_";
    public const string spriteMouthPrefix = "ball_mouth_";

    public tk2dSpriteAnimator body;
    public tk2dBaseSprite[] eyes;
    public tk2dBaseSprite mouth;

    public int eyeSpriteCount;
    public int mouthSpriteCount;

    private int mEyeSpriteInd;
    private int mMouthSpriteInd;

    private tk2dSpriteAnimationClip[] mClips;

    private Color mColor = Color.white;

    public Color color {
        get { return mColor; }
        set {
            if(mColor != value) {
                mColor = value;

                body.Sprite.color = mColor;
                mouth.color = mColor;

                for(int i = 0, max = eyes.Length; i < max; i++)
                    eyes[i].color = mColor;
            }
        }
    }

    void Awake() {
        mEyeSpriteInd = Random.Range(0, eyeSpriteCount);
        
        int eyeCount = eyes.Length;
        if(eyeCount > 0) {
            int eyeSprId = eyes[0].GetSpriteIdByName(spriteEyePrefix + mEyeSpriteInd);
            for(int i = 0, max = eyes.Length; i < max; i++)
                eyes[i].SetSprite(eyeSprId);
        }

        mMouthSpriteInd = Random.Range(0, mouthSpriteCount);

        mouth.SetSprite(spriteMouthPrefix + mMouthSpriteInd);

        mClips = M8.tk2dUtil.GetSpriteClips(body, typeof(State));
    }

    // Use this for initialization
    void Start() {

    }

    void EntityStart(EntityBase ent) {
        ent.setBlinkCallback += OnPlayerBlink;
    }

    void OnPlayerBlink(EntityBase ent, bool blink) {
        body.Play(mClips[(int)(blink ? State.power : State.idle)]);
    }

    // Update is called once per frame
    void Update() {

    }
}
