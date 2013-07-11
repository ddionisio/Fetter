using UnityEngine;
using System.Collections;

public class HeartController : MonoBehaviour {
    public float speed = 1.0f; //if useMouse = false
    
    public LayerMask harmLayerMask;

    private Player mPlayer;
    private SphereCollider mSphereColl;
    private bool mInputEnabled = false;
    private Vector2 mCurDir = Vector2.zero;
    private float mCurSpeed = 0.0f;
    private Vector2 mCurVel = Vector2.zero;
    private Vector2 mInputAxis = Vector2.zero;
    private float mZ;
        
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

    void OnTriggerEnter(Collider col) {
        if(((1 << col.gameObject.layer) & harmLayerMask) != 0) {
            mPlayer.Hurt();
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mSphereColl = collider as SphereCollider;
                
        mZ = transform.position.z;
    }

    void EntityStart(EntityBase ent) {
        mPlayer = (Player)ent;

        ent.setStateCallback += OnPlayerSetState;
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

            case Player.StateDead:
            case Player.StateInvalid:
                inputEnabled = false;
                break;
        }
    }

    void UpdatePosition(Vector2 pos) {
        //check bound
        LevelController level = LevelController.instance;
        level.ClampToBounds(mSphereColl.radius, ref pos);

        rigidbody.MovePosition(new Vector3(pos.x, pos.y, mZ));
    }
}
