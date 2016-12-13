using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Line : MonoBehaviour {
    [Header("Stats")]
    public float dangerDistanceStart;
    public float dangerDistanceEnd;

    public float dangerBlinkDelay = 0.05f;

    [Header("Telemetry")]
    [SerializeField]
    Transform _player;
    [SerializeField]
    Transform _ball;
    [SerializeField]
    Transform _lineDisplayRoot;
    [SerializeField]
    float _lineUnitScale = 1f;

    [Header("Render")]
    [SerializeField]
    Renderer _lineRenderer;
    [SerializeField]
    string _lineDangerColorProp = "_Color";
    [SerializeField]
    Color _lineDangerColor = Color.red;

    public float distance { get { return mDistance; } }

    public bool isDanger { get { return mDistance >= dangerDistanceStart; } }

    private float mDistance;
    private Vector2 mDirBallToPlayer;

    private Material mLineMat;
    private int mLineColorVarID;
    private Color mLineColorDefault;

    bool isValid { get { return _ball && _player && _lineDisplayRoot; } }

    private float mLineDangerCurTime;
    private bool mIsLineDangerBlink;

    void OnDestroy() {
        if(mLineMat)
            Destroy(mLineMat);
    }

    void Awake() {
#if UNITY_EDITOR
        if(!Application.isPlaying) //don't allow during edit
            return;
#endif

        if(_lineRenderer) {
            mLineMat = _lineRenderer.material;
            mLineColorVarID = Shader.PropertyToID(_lineDangerColorProp);
            mLineColorDefault = mLineMat.GetColor(mLineColorVarID);
        }
    }

    // Use this for initialization
    void Start() {

    }

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            if(!isValid)
                return;

            UpdateDistance();
            UpdateTelemetry();
        }
    }
#endif

    // Update is called once per frame
    void LateUpdate() {
        if(!isValid)
            return;

        UpdateDistance();
        UpdateTelemetry();

        //update danger
        var lastBlink = mIsLineDangerBlink;

        if(isDanger) {
            var dist = Mathf.Clamp(mDistance, dangerDistanceStart, dangerDistanceEnd);

            mLineDangerCurTime += Time.deltaTime;
            if(mLineDangerCurTime >= dangerBlinkDelay) {
                mIsLineDangerBlink = !mIsLineDangerBlink;
                mLineDangerCurTime = 0f;
            }
        }
        else {
            mIsLineDangerBlink = false;
            mLineDangerCurTime = dangerBlinkDelay; //allow blink to activate right away when it becomes danger
        }

        if(mIsLineDangerBlink != lastBlink) {
            mLineMat.SetColor(mLineColorVarID, mIsLineDangerBlink ? _lineDangerColor : mLineColorDefault);
        }
    }

    void UpdateDistance() {
        mDirBallToPlayer = _player.position - _ball.position;
        mDistance = mDirBallToPlayer.magnitude;
        if(mDistance > 0f)
            mDirBallToPlayer /= mDistance;
    }

    void UpdateTelemetry() {
        transform.position = _ball.position;
        transform.up = mDirBallToPlayer;

        if(_lineUnitScale > 0f && mDistance > 0f) {
            var s = _lineDisplayRoot.localScale;

            s.y = mDistance/_lineUnitScale;

            _lineDisplayRoot.localScale = s;
        }
    }
}
