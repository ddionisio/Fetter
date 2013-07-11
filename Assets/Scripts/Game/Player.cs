using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public int maxHealth = 3;
    public float invulDelay = 1.0f;

    public const int StateNormal = 0;
    public const int StatePower = 1;
    public const int StateDead = 2;

    public Transform heart;

    public Transform ball;
    public float ballBreakForce = 10.0f;
    public float ballBreakAlphaStart = 0.5f;
    public float ballBreakFadeDelay = 1.0f;

    public Transform line;
    public float lineBlinkDelay;
    public float lineMinLength = 0.3f;
    public float lineMaxLength = 2.65f;
    public float lineDanger = 0.2f;

    private struct SpringJointConfig {
        public Vector3 anchor;
        public float spring;
        public float damper;
        public float minDist;
        public float maxDist;

        public SpringJointConfig(SpringJoint joint) {
            anchor = joint.anchor;
            spring = joint.spring;
            damper = joint.damper;
            minDist = joint.minDistance;
            maxDist = joint.maxDistance;
        }

        public SpringJoint GenerateJoint(GameObject go, Rigidbody attach) {
            SpringJoint ret = go.AddComponent<SpringJoint>();
            ret.connectedBody = attach;
            ret.anchor = anchor;
            ret.spring = spring;
            ret.damper = damper;
            ret.minDistance = minDist;
            ret.maxDistance = maxDist;
            return ret;
        }
    }

    private int mCurScore;
    private int mCurHealth;
    private bool mIsFocus = true;
    private HUD mHUD;

    private SpringJoint mBallJoint;
    private SpringJointConfig mBallJointConfig;
    private tk2dBaseSprite mBallSprite;
    private bool mBallBreaking;

    private Vector2 mDirToBall;
    private float mLineLength;
    private bool mLineBlink;

    private MatRepeatTileScrollScale mLineTileMat;
    private WaitForSeconds mLineBlinkWait;

    public HUD hud { get { return mHUD; } }
    public int health { get { return mCurHealth; } }
    public int score { get { return mCurScore; } }

    public Vector2 heartToBallDir { get { return mDirToBall; } }
    public float lineLength { get { return mLineLength; } }

    /// <summary>
    /// returns true if we got hurt.
    /// </summary>
    public bool Hurt() {
        bool ret = false;

        if(mCurHealth > 0 && !isBlinking) {
            BallBreak();

            mCurHealth--;

            mHUD.SetHearts(mCurHealth);

            if(mCurHealth == 0) {
                //game over
                state = StateDead;
            }
            else {
                Blink(invulDelay);

                ret = true;
            }
        }

        return ret;
    }

    protected override void StateChanged() {
        switch(prevState) {
            case StateNormal:
                SetLineBlink(false);
                break;
        }

        switch(state) {
            case StateDead:
                //shake hud
                mHUD.SetLinePercent(0.0f);

                //save stuff
                break;
        }
    }
    
    protected override void OnDestroy() {
        if(Main.instance != null && Main.instance.input != null)
            Main.instance.input.RemoveButtonCall(0, InputAction.Menu, OnInputMenu);

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        GameObject uiGO = GameObject.FindGameObjectWithTag("UI");
        mHUD = uiGO.GetComponent<HUD>();

        mLineTileMat = line.GetComponent<MatRepeatTileScrollScale>();
        mLineBlinkWait = new WaitForSeconds(lineBlinkDelay);

        mBallJoint = ball.GetComponent<SpringJoint>();
        mBallJointConfig = new SpringJointConfig(mBallJoint);
        mBallSprite = ball.GetComponentInChildren<tk2dBaseSprite>();

        autoSpawnFinish = true;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        Main.instance.input.AddButtonCall(0, InputAction.Menu, OnInputMenu);
    }

    protected override void OnDespawned() {
        state = StateInvalid;
    }

    protected override void SpawnStart() {
        //init ball
        mBallBreaking = false;
        BallReset();

        mCurScore = 0;
        mCurHealth = maxHealth;

        //init hud
        mHUD.SetHearts(mCurHealth);
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

    void Update() {
        switch(state) {
            case StateNormal:
                if(!mBallBreaking) {
                    //update line
                    Vector2 pos = heart.position;
                    Vector2 ballPos = ball.position;
                    Vector2 dirToBall = ballPos - pos;
                    float len = dirToBall.magnitude;
                    if(len > 0.0f) {
                        mDirToBall = dirToBall;
                        mLineLength = len;

                        line.up = mDirToBall;

                        Vector3 linePos = line.position;
                        line.position = new Vector3(pos.x, pos.y, linePos.z);

                        Vector3 lineS = line.localScale;
                        lineS.y = mLineLength;
                        line.localScale = lineS;

                        //check if line length is going to break
                        if(mLineLength > lineMaxLength) {
                            Hurt();
                        }
                        else {
                            float lineUnit = mLineLength > lineMinLength ? 1.0f - (mLineLength - lineMinLength) / (lineMaxLength - lineMinLength) : 1.0f;

                            //update hud
                            mHUD.SetLinePercent(lineUnit);

                            SetLineBlink(lineUnit <= lineDanger);
                        }
                    }
                }
                break;
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

    void BallReset() {
        Vector3 heartPos = heart.position;
        Vector3 ballPos = ball.position;
        ballPos.x = heartPos.x;
        ballPos.y = heartPos.y - lineMinLength;
        ball.position = ballPos;

        //check if joint is gone
        SpringJoint springJoint = ball.GetComponent<SpringJoint>();
        if(springJoint == null) {
            mBallJoint = mBallJointConfig.GenerateJoint(ball.gameObject, heart.rigidbody);
        }
    }

    void BallBreak() {
        if(!mBallBreaking)
            StartCoroutine(DoBallBreak());
    }

    void SetLineBlink(bool blink) {
        if(mLineBlink != blink) {
            mLineBlink = blink;
            if(mLineBlink)
                StartCoroutine(DoLineBlink());
            else {
                mLineTileMat.color = mLineTileMat.defaultColor;
                StopCoroutine("DoLineBlink");
            }
        }
    }

    IEnumerator DoLineBlink() {
        while(mLineBlink) {
            mLineTileMat.color = Color.clear;
            yield return mLineBlinkWait;
            mLineTileMat.color = mLineTileMat.defaultColor;
            yield return mLineBlinkWait;
        }
    }

    IEnumerator DoBallBreak() {
        mBallBreaking = true;

        mHUD.SetLinePercent(0.0f);

        line.gameObject.SetActive(false);

        mBallJoint.breakForce = 0.0001f;
        ball.rigidbody.AddForce(mDirToBall * ballBreakForce, ForceMode.Impulse);

        //fade out
        Color colorFrom = new Color(1.0f, 1.0f, ballBreakAlphaStart), colorTo = Color.clear;
        float dt = 0.0f;

        while(dt < ballBreakFadeDelay) {
            yield return new WaitForFixedUpdate();

            dt = Mathf.Clamp(dt + Time.fixedDeltaTime, 0.0f, ballBreakFadeDelay);

            mBallSprite.color = Color.Lerp(colorFrom, colorTo, dt / ballBreakFadeDelay);
        }

        ball.collider.enabled = false;
        ball.rigidbody.velocity = Vector3.zero;
        ball.rigidbody.isKinematic = true;

        yield return new WaitForFixedUpdate();

        //make sure we are still alive
        if(state == StateNormal) {
            ball.collider.enabled = true;
            ball.rigidbody.isKinematic = false;

            BallReset();

            //fade in
            dt = 0.0f;

            while(dt < ballBreakFadeDelay) {
                yield return new WaitForFixedUpdate();

                dt = Mathf.Clamp(dt + Time.fixedDeltaTime, 0.0f, ballBreakFadeDelay);

                mBallSprite.color = Color.Lerp(colorTo, colorFrom, dt / ballBreakFadeDelay);
            }

            mBallSprite.color = Color.white;

            line.gameObject.SetActive(true);
        }

        mBallBreaking = false;
    }
}
