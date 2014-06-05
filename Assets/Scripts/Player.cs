using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public const string takeBallDestroy = "destroy";
    public const string takeBallSpawn = "spawn";

    public Transform ball;
    public float ballBreakForce = 10.0f;
    public float ballBreakAlphaStart = 0.5f;
    public float ballBreakFadeDelay = 0.5f;

    public Transform line;
    public float lineBlinkDelay;
    public float lineMaxLength = 1.5f;
    public float lineDanger = 0.2f;

    public float speed = 1.0f; //if useMouse = false

    public LayerMask harmLayerMask;

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
    private bool mIsFocus = true;

    private SpringJoint mBallJoint;
    private SpringJointConfig mBallJointConfig;
    private bool mBallBreaking;
    private SphereCollider mBallColl;

    private Vector2 mDirToBall;
    private float mLineLength;
    private bool mLineBlink;

    private AnimatorData mBallAnim;

    private MatAutoTileScale mLineTileMat;

    private SphereCollider mColl;
    private bool mInputEnabled = false;
    private Vector2 mCurDir = Vector2.zero;
    private float mCurSpeed = 0.0f;
    private Vector2 mCurVel = Vector2.zero;
    private Vector2 mInputAxis = Vector2.zero;

    private float mLineMinLength;

    private float mLastInputTime;

    public int score { get { return mCurScore; } }

    public Vector2 heartToBallDir { get { return mDirToBall; } }
    public float lineLength { get { return mLineLength; } }

    public Vector2 curDir { get { return mCurDir; } }
    public Vector2 curVel { get { return mCurVel; } }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                if(mInputEnabled) {
                    mLastInputTime = Time.realtimeSinceStartup;
                }
                else {
                }
            }
        }
    }

    protected override void StateChanged() {
        switch((EntityState)prevState) {
            case EntityState.Normal:
                SetLineBlink(false);
                break;
        }

        switch((EntityState)state) {
            case EntityState.Spawn:
            case EntityState.Invalid:
                inputEnabled = false;
                break;

            case EntityState.Normal:
                inputEnabled = true;
                break;

            case EntityState.Dead:
                inputEnabled = false;

                BallBreak();

                //shake hud
                //mHUD.SetLinePercent(0.0f);

                //save stuff
                break;
        }
    }

    void OnTriggerEnter(Collider col) {
        if(((1 << col.gameObject.layer) & harmLayerMask) != 0) {
            state = (int)EntityState.Dead;
        }
    }

    protected override void OnDestroy() {
        if(Main.instance != null && Main.instance.input != null)
            Main.instance.input.RemoveButtonCall(0, InputAction.Menu, OnInputMenu);

        inputEnabled = false;

        Screen.showCursor = true;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //GameObject uiGO = GameObject.FindGameObjectWithTag("UI");
       // mHUD = uiGO.GetComponent<HUD>();

        mColl = collider as SphereCollider;

        mBallAnim = ball.GetComponent<AnimatorData>();
        mBallColl = ball.GetComponent<SphereCollider>();

        mLineTileMat = line.GetComponent<MatAutoTileScale>();

        mBallJoint = ball.GetComponent<SpringJoint>();
        mBallJointConfig = new SpringJointConfig(mBallJoint);

        mLineMinLength = (transform.position - ball.position).magnitude - mBallColl.radius - mColl.radius;
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
        state = (int)EntityState.Spawn;

        //init ball
        mBallBreaking = false;
        BallReset();

        mCurScore = 0;

        //init hud
    }

    public override void SpawnFinish() {
        state = (int)EntityState.Normal;
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
        switch((EntityState)state) {
            case EntityState.Normal:
                if(!mBallBreaking) {
                    //update line
                    Vector2 pos = transform.position;
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
                        lineS.y = len;
                        line.localScale = lineS;

                        //check if line length is going to break
                        if(mLineLength > lineMaxLength) {
                            state = (int)EntityState.Dead;
                        }
                        else {
                            float lineUnit = mLineLength > mLineMinLength ? 1.0f - (mLineLength - mLineMinLength) / (lineMaxLength - mLineMinLength) : 1.0f;

                            //update hud
                            //mHUD.SetLinePercent(lineUnit);

                            SetLineBlink(lineUnit <= lineDanger);
                        }
                    }
                }

                if(mInputEnabled) {
                    Vector2 pos = transform.position;

                    float dt = Time.realtimeSinceStartup - mLastInputTime;
                    mLastInputTime = Time.realtimeSinceStartup;

                    InputManager input = Main.instance.input;

                    mInputAxis.x = input.GetAxis(0, InputAction.DirX);
                    mInputAxis.y = input.GetAxis(0, InputAction.DirY);

                    mCurVel = mInputAxis * speed;

                    mCurSpeed = mCurVel.magnitude;

                    if(mCurSpeed > 0.0f) {
                        mCurDir = mCurVel / mCurSpeed;
                    }

                    UpdatePosition(pos + mCurVel * dt);
                }
                else {
                    mInputAxis = Vector2.zero;
                    mCurVel = Vector2.zero;
                    mCurSpeed = 0.0f;
                }
                break;
        }
    }

    void OnUIModalActive() {
        Screen.showCursor = true;

        inputEnabled = false;
    }

    void OnUIModalInactive() {
        if(mIsFocus)
            Main.instance.sceneManager.Resume();

        Screen.showCursor = !mIsFocus;

        if(state == (int)EntityState.Normal)
            inputEnabled = true;
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
        Vector3 pos = transform.position;
        Vector3 ballPos = ball.position;
        ballPos.x = pos.x;
        ballPos.y = pos.y - mLineMinLength;
        ball.position = ballPos;

        //check if joint is gone
        SpringJoint springJoint = ball.GetComponent<SpringJoint>();
        if(springJoint == null) {
            mBallJoint = mBallJointConfig.GenerateJoint(ball.gameObject, rigidbody);
        }

        //mBallAnim.Play(takeBallSpawn);
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
        WaitForSeconds wait = new WaitForSeconds(lineBlinkDelay);
        while(mLineBlink) {
            mLineTileMat.color = Color.clear;
            yield return wait;
            mLineTileMat.color = mLineTileMat.defaultColor;
            yield return wait;
        }
    }

    IEnumerator DoBallBreak() {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        mBallBreaking = true;

        //mHUD.SetLinePercent(0.0f);

        line.gameObject.SetActive(false);

        if(mBallJoint) {
            Destroy(mBallJoint);
            mBallJoint = null;
        }

        ball.rigidbody.AddForce(mDirToBall * ballBreakForce, ForceMode.Impulse);

        //set to ball destruction
        //mBallAnim.Play(takeBallDestroy);
        //while(mBallAnim.isPlaying)
            //yield return wait;

        ball.collider.enabled = false;
        ball.rigidbody.velocity = Vector3.zero;
        ball.rigidbody.isKinematic = true;

        yield return wait;
        
        //respawn?

        mBallBreaking = false;
    }

    void UpdatePosition(Vector2 pos) {
        //check bound
        Camera2D cam2D = Camera2D.main;
        Rect screen = cam2D.screenExtent;
        Vector3 cam2DPos = cam2D.transform.position;

        float hW = screen.width*0.5f;
        float hH = screen.height*0.5f;

        if(pos.x < cam2DPos.x - hW)
            pos.x = cam2DPos.x - hW;
        else if(pos.x > cam2DPos.y + hW)
            pos.x = cam2DPos.y + hW;

        if(pos.y < cam2DPos.y - hH)
            pos.y = cam2DPos.y - hH;
        else if(pos.y > cam2DPos.y + hH)
            pos.y = cam2DPos.y + hH;

        Rigidbody body = rigidbody;

        body.MovePosition(new Vector3(pos.x, pos.y, body.position.z));
    }
}
