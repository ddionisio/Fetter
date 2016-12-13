using UnityEngine;
using System.Collections;
using M8.Animator;

public class TumorChangeAnimator : MonoBehaviour, ITumorRadiusChange {
    public string take;

    private AnimatorData mAnim;
    
    private Coroutine mRout;

    void Awake() {
        mAnim = GetComponent<AnimatorData>();
    }

    void OnDisable() {
        mRout = null;
    }

    void ITumorRadiusChange.OnRadiusChange(Tumor tumor, float delta) {
        
        if(mRout == null) {
            mRout = StartCoroutine(DoChange());
        }
    }

    IEnumerator DoChange() {
        yield return null;

        mRout = null;
    }
}
