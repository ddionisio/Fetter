using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {
    public float boundHeightPadding = 16.0f;

    private static LevelController mInstance;

    private float mBoundRadius;
    private tk2dCamera mCam;

    public static LevelController instance { get { return mInstance; } }

    public float boundRadius { get { return mBoundRadius; } }
    public tk2dCamera gameCamera { get { return mCam; } }

    public bool IsInBounds(float radius, Vector2 pos) {
        Vector2 origin = transform.position;

        Vector2 dpos = pos - origin;
        float sqrMag = dpos.sqrMagnitude;

        float r = mBoundRadius - radius;

        return sqrMag > r*r;
    }

    public void ClampToBounds(float radius, ref Vector2 pos) {
        Vector2 origin = transform.position;

        Vector2 dpos = pos - origin;
        float sqrMag = dpos.sqrMagnitude;

        float r = mBoundRadius - radius;

        if(sqrMag > r*r) {
            pos = origin + dpos.normalized * r;
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mCam = Camera.mainCamera.GetComponent<tk2dCamera>();

            Rect screenExts = mCam.ScreenExtents;

            float boundSize = screenExts.height;
            float yPad = boundHeightPadding / mCam.CameraSettings.orthographicPixelsPerMeter;

            mBoundRadius = (boundSize * 0.5f) - yPad;
            //Vector3 boundExt = new Vector3(
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void OnDrawGizmos() {
        Gizmos.color = Color.magenta*0.25f;

        if(Application.isPlaying) {
            if(mBoundRadius > 0.0f) {
                Gizmos.DrawWireSphere(transform.position, mBoundRadius);
            }
        }
        else {
            tk2dCamera cam = Camera.mainCamera != null ? Camera.mainCamera.GetComponent<tk2dCamera>() : null;
            if(cam != null) {
                Rect screenExts = cam.ScreenExtents;

                float boundSize = screenExts.height;
                float yPad = boundHeightPadding / cam.CameraSettings.orthographicPixelsPerMeter;

                Gizmos.DrawWireSphere(transform.position, (boundSize * 0.5f) - yPad);
            }
        }
    }
}
