using UnityEngine;
using System.Collections;
using M8;
using System;

public class Debris : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    public const string paramStatRef = "statRef";
    public const string paramTrans = "trans";
    public const string paramMesh = "mesh";
    public const string paramMat = "mat";

    public string collectTake = "collect";
    public float collectDelay = 0.3f;

    public StatsController stats { get { return mStats; } }

    private StatsController mStats;

    private Rigidbody2D mBody;
    private BoxCollider2D mColl;

    private M8.Animator.AnimatorData mAnim;

    //these should be in a child object
    private Transform mMeshTransform;
    private MeshFilter mMeshFilter;
    private MeshRenderer mMeshRenderer;

    private Coroutine mCollectRout;
    
    public void Collect(Transform toTrans, Action<Debris> onFinish) {
        if(mCollectRout != null) //already enroute
            return;

        mColl.enabled = false;

        mBody.velocity = Vector2.zero;
        mBody.isKinematic = false;

        mCollectRout = StartCoroutine(DoCollect(toTrans, onFinish));
    }

    void Awake() {
        mStats = GetComponent<StatsController>();

        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<BoxCollider2D>();

        mAnim = GetComponent<M8.Animator.AnimatorData>();

        mMeshFilter = GetComponentInChildren<MeshFilter>();
        mMeshRenderer = GetComponentInChildren<MeshRenderer>();

        mMeshTransform = mMeshRenderer.transform;
    }

    void IPoolSpawn.OnSpawned(GenericParams parms) {
        //config stats
        var statName = (string)parms[paramStatRef];
        mStats.Override(StatsCollection.instance.GetStatItems(statName));
                
        //get object refs
        var trans = (Transform)parms[paramTrans];
        var mesh = (Mesh)parms[paramMesh];

        mMeshFilter.sharedMesh = mesh;
        mMeshRenderer.sharedMaterial = (Material)parms[paramMat];

        //apply position
        transform.position = trans.position;
        transform.rotation = trans.rotation;

        mMeshTransform.localPosition = trans.localPosition;
        mMeshTransform.localRotation = trans.localRotation;
        mMeshTransform.localScale = trans.localScale;

        //shape the box collision
        var bounds = mMeshRenderer.bounds;

        mColl.offset = transform.InverseTransformPoint(bounds.center);
        mColl.size = bounds.size;
        mColl.enabled = true;

        //Reset physics
        mBody.velocity = Vector2.zero;
        mBody.isKinematic = false;
    }

    void IPoolDespawn.OnDespawned() {
        mMeshFilter.sharedMesh = null;
        mMeshRenderer.sharedMaterial = null;
    }

    IEnumerator DoCollect(Transform toTrans, Action<Debris> onFinish) {
        if(mAnim)
            mAnim.Play(collectTake);

        Vector2 fromPos = transform.position;

        var ease = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InCirc);

        float curTime = 0f;
        while(curTime < collectDelay) {
            yield return null;
            
            curTime += Time.deltaTime;
            if(curTime > collectDelay)
                curTime = collectDelay;
                        
            Vector2 toPos = toTrans.position;

            transform.position = Vector2.Lerp(fromPos, toPos, ease(curTime, collectDelay, 0f, 0f));
        }

        if(onFinish != null)
            onFinish(this);

        PoolController.ReleaseAuto(gameObject);
    }
}
