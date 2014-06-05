using UnityEngine;
using System.Collections;

public class ChainLine : MonoBehaviour {

    public float lineBlinkDelay;

    private SpringJoint mSpring;
    private MatAutoTileScale mLineTileMat;
    private Transform mLineTrans;
    private bool mBlink;
    private IEnumerator mBlinkRoutine;

    public bool blink {
        get { return mBlink; }
        set {
            if(mBlink != value) {
                mBlink = value;
                if(mBlink) {
                    mBlinkRoutine = DoBlink();
                    StartCoroutine(mBlinkRoutine);
                }
                else {
                    StopCoroutine(mBlinkRoutine);
                    mBlinkRoutine = null;

                    mLineTileMat.color = Color.white;
                }
            }
        }
    }

    void OnDisable() {
        blink = false;
    }

    void Awake() {
        mSpring = GetComponent<SpringJoint>();
        mLineTileMat = GetComponentInChildren<MatAutoTileScale>();
        mLineTrans = mLineTileMat.transform;
    }

    // Update is called once per frame
    void Update() {
        Vector2 pos = transform.position;
        Vector2 attachPos = mSpring.connectedBody.transform.position;
        Vector2 dirToAttach = attachPos - pos;
        float len = dirToAttach.magnitude;
        if(len > 0.0f) {
            mLineTrans.up = dirToAttach;

            Vector3 lineS = mLineTrans.localScale;
            lineS.y = len;
            mLineTrans.localScale = lineS;
        }
    }

    IEnumerator DoBlink() {
        WaitForSeconds wait = new WaitForSeconds(lineBlinkDelay);
        while(mBlink) {
            mLineTileMat.color = Color.clear;
            yield return wait;
            mLineTileMat.color = mLineTileMat.defaultColor;
            yield return wait;
        }
    }
}
