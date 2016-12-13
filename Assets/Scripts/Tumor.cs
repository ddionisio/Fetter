using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tumor : M8.EntityBase {
    public const string parmTumorRoot = "tumor";
    public const string parmPosition = "debrisPos";

    [Header("Setup")]

    [SerializeField]
    float _radius; //starting/min radius
    [SerializeField]
    float _radiusMax;
    [SerializeField]
    Transform _root;
    [SerializeField]
    M8.Auxiliary.AuxTrigger2D _trigger;
    [SerializeField]
    Collider2D _collider;

    [Header("Animation")]
    [SerializeField]
    M8.Animator.AnimatorData _anim;
    [SerializeField]
    string _takeSpawn;

    [Header("Data")]

    [SerializeField]
    float _tumorMax;
    [SerializeField]
    float _tumorGrowthScale = 1.0f; //this is used to scale the value of the debris to be added to tumor

    public float radius { //current radius
        get { return mRadius; }
        set {
            if(mRadius != value) {
                var lastRadius = mRadius;
                mRadius = Mathf.Clamp(value, _radius, _radiusMax);

                for(int i = 0; i < mOnRadiusChange.Length; i++)
                    mOnRadiusChange[i].OnRadiusChange(this, mRadius - lastRadius);
            }
        }
    }

    public float radiusMin { get { return _radius; } }

    public float radiusMax { get { return _radiusMax; } }

    public float radiusScale { get { return _radius > 0f ? mRadius/_radius : 0f; } }

    public float tumorValue { get { return mTumorValue; } }

    public Tumor tumorRoot { get { return mTumorRoot; } }

    public int tumorTreeLevel { get { return mTumorTreeLevel; } }

    public Transform root { get { return _root; } }
    
    private Tumor mTumorRoot;
    private float mRadius;
    private float mTumorValue;
    private int mTumorTreeLevel;

    private bool mIsSpawning;
    private Vector2 mLastDebrisCollectPos;

    private List<Tumor> mSubTumors = new List<Tumor>();

    private ITumorRadiusChange[] mOnRadiusChange;

    protected override void OnDespawned() {
        //reset stuff here
        mTumorRoot = null;
        mIsSpawning = false;

        //recycle tumors
        for(int i = 0; i < mSubTumors.Count; i++)
            mSubTumors[i].Release();

        mSubTumors.Clear();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        ResetData();

        //spawned by another tumor
        mTumorRoot = (Tumor)parms[parmTumorRoot];
        if(mTumorRoot) {
            mTumorTreeLevel = mTumorRoot.tumorTreeLevel + 1;

            //determine position
            var debrisPos = parms[parmPosition];
        }
    }

    protected override void OnDestroy() {
        //dealloc here
        if(_trigger)
            _trigger.enterCallback -= OnAuxTriggerEnter;

        if(_anim)
            _anim.takeCompleteCallback -= OnTakeComplete;

        base.OnDestroy();
    }

    protected override void SpawnStart() {
        //start ai, player control, etc
        if(_anim && !string.IsNullOrEmpty(_takeSpawn)) {
            mIsSpawning = true;
            _anim.Play(_takeSpawn);
        }
    }

    protected override void Awake() {
        base.Awake();

        //initial values
        ResetData();

        _trigger.enterCallback += OnAuxTriggerEnter;

        if(_anim)
            _anim.takeCompleteCallback += OnTakeComplete;

        //grab interfaces
        var radiusChanges = new List<ITumorRadiusChange>();

        var comps = GetComponentsInChildren<MonoBehaviour>(true);
        for(int i = 0; i < comps.Length; i++) {
            var comp = comps[i];

            var radiusChange = comp as ITumorRadiusChange;
            if(radiusChange != null) radiusChanges.Add(radiusChange);
        }

        mOnRadiusChange = radiusChanges.ToArray();
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void ResetData() {
        mTumorTreeLevel = 0;
        mRadius = _radius;
        mTumorValue = 0f;
    }

    void UpdateRadius() {
        radius = Mathf.Lerp(_radius, _radiusMax, mTumorValue/_tumorMax);

        //add new tumor
        if(mTumorValue == _tumorMax) {
            //compute position within the surface

            //generate tumor and add to list
        }
    }

    void OnAuxTriggerEnter(Collider2D other) {
        var debris = other.GetComponent<Debris>();
        if(debris) {
            debris.Collect(_root, OnCollectFinish);
        }
    }

    void OnCollectFinish(Debris debris) {
        mLastDebrisCollectPos = debris.transform.position;

        mTumorValue = Mathf.Clamp(mTumorValue + _tumorGrowthScale*debris.stats[StatID.Growth].value, 0f, _tumorMax);

        PlayerController.instance.ProcessScore(this, debris);

        //wait for spawning
        if(mIsSpawning)
            return;

        UpdateRadius();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0.65f, 0f, 0f, 1f);

        var pos = transform.position;

        if(_radius > 0f)
            Gizmos.DrawWireSphere(pos, _radius);

        if(_radiusMax > 0f)
            Gizmos.DrawWireSphere(pos, _radiusMax);

        if(Application.isPlaying) {
            Gizmos.color = Color.red;

            if(mRadius > 0f)
                Gizmos.DrawWireSphere(pos, mRadius);
        }
    }

    void OnTakeComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == _takeSpawn) {
            mIsSpawning = false;
            UpdateRadius();
        }
    }
}
