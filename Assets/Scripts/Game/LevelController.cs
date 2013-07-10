using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {
    public float boundHeightPadding = 16.0f;

    private static LevelController mInstance;

    private Bounds mBounds;
    private tk2dCamera mCam;

    public static LevelController instance { get { return mInstance; } }

    public Bounds gameBounds { get { return mBounds; } }
    public tk2dCamera gameCamera { get { return mCam; } }

    public bool IsInBounds(Vector2 pos) {
        return mBounds.Contains(pos);
    }

    public void ClampToBounds(ref Vector2 pos) {
        if(pos.x < mBounds.min.x) {
            pos.x = mBounds.min.x;
        }
        else if(pos.x > mBounds.max.x) {
            pos.x = mBounds.max.x;
        }

        if(pos.y < mBounds.min.y) {
            pos.y = mBounds.min.y;
        }
        else if(pos.y > mBounds.max.y) {
            pos.y = mBounds.max.y;
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

            mBounds.size = new Vector3(boundSize, boundSize - yPad * 2.0f, 1.0f);
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
        Gizmos.color = Color.magenta;

        if(Application.isPlaying) {
            if(mBounds.size.x > 0.0f && mBounds.size.y > 0.0f) {
                Gizmos.DrawWireCube(mBounds.center, mBounds.size);
            }
        }
        else {
            tk2dCamera cam = Camera.mainCamera != null ? Camera.mainCamera.GetComponent<tk2dCamera>() : null;
            if(cam != null) {
                Rect screenExts = cam.ScreenExtents;

                float boundSize = screenExts.height;
                float yPad = boundHeightPadding / cam.CameraSettings.orthographicPixelsPerMeter;

                Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundSize, boundSize - yPad * 2.0f, 1.0f));
            }
        }
    }
}
