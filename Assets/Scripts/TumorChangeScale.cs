using UnityEngine;
using System.Collections;

public class TumorChangeScale : MonoBehaviour, ITumorRadiusChange {
    public float delay = 0.3f;

    private Vector2 mOrigScale;
    private Vector2 mToScale;
    private Vector2 mVel;
    private Coroutine mRout;

    void OnDisable() {        
        mRout = null;
    }

    void Awake() {
        mOrigScale = transform.localScale;
    }

    void ITumorRadiusChange.OnRadiusChange(Tumor tumor, float delta) {
        mToScale = mOrigScale*tumor.radiusScale;

        if(mRout == null) {
            mVel = Vector2.zero;
            mRout = StartCoroutine(DoChange());
        }
    }

    IEnumerator DoChange() {
        var t = transform;

        Vector2 s = t.localScale;

        while(s != mToScale) {
            t.localScale = Vector2.SmoothDamp(s, mToScale, ref mVel, delay, 60f, Time.deltaTime);

            yield return null;

            s = t.localScale;
        }

        mRout = null;
    }
}
