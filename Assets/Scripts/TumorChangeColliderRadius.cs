using UnityEngine;
using System.Collections;

public class TumorChangeColliderRadius : MonoBehaviour, ITumorRadiusChange {
    public float delay = 0.3f;

    private CircleCollider2D mColl;

    private float mDefaultRadius;
    private float mToRadius;
    private float mVel;
    private Coroutine mRout;

    void Awake() {
        mColl = GetComponent<CircleCollider2D>();
        mDefaultRadius = mColl.radius;
    }

    void OnDisable() {
        mRout = null;
    }

    void ITumorRadiusChange.OnRadiusChange(Tumor tumor, float delta) {
        mToRadius = mDefaultRadius*tumor.radiusScale;

        if(mRout == null) {
            mVel = 0f;
            mRout = StartCoroutine(DoChange());
        }
    }

    IEnumerator DoChange() {        
        while(mColl.radius != mToRadius) {
            mColl.radius = Mathf.SmoothDamp(mColl.radius, mToRadius, ref mVel, delay);
            yield return null;
        }

        mRout = null;
    }
}
