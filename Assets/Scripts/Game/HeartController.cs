using UnityEngine;
using System.Collections;

public class HeartController : MonoBehaviour {
    public float speed = 1.0f; //if useMouse = false

    public Transform ball;
    public Transform line;

    private Player mPlayer;
    private bool mInputEnabled = false;
    private Vector2 mCurDir = Vector2.zero;
    private float mCurSpeed = 0.0f;
    private Vector2 mCurVel = Vector2.zero;
    private Vector2 mInputAxis = Vector2.zero;
    private float mZ;

    private Vector2 mDirToBall;
    private float mLineLength;

    public Player player { get { return mPlayer; } }
    public Vector2 curDir { get { return mCurDir; } }
    public Vector2 curVel { get { return mCurVel; } }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                if(mInputEnabled) {
                }
                else {
                }
            }
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mZ = transform.position.z;
    }

    void EntityStart(EntityBase ent) {
        mPlayer = (Player)ent;

        ent.setStateCallback += OnPlayerSetState;
    }

    void Update() {
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
            lineS.y = mLineLength;
            line.localScale = lineS;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if(mInputEnabled) {
            Vector2 pos = transform.position;

            float dt = Time.fixedDeltaTime;

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
    }
        
    void OnPlayerSetState(EntityBase ent, int state) {
        switch(state) {
            case Player.StateNormal:
                inputEnabled = true;
                break;
        }
    }

    void UpdatePosition(Vector2 pos) {
        //check bound
        LevelController level = LevelController.instance;
        level.ClampToBounds(ref pos);

        rigidbody.MovePosition(new Vector3(pos.x, pos.y, mZ));
    }
}
