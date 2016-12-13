using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TransScaleAt : MonoBehaviour {
    [Header("X-Axis")]
    public Transform xTarget;
    public float xOfs;
    public float xUnit = 1.0f;

    [Header("Y-Axis")]
    public Transform yTarget;
    public float yOfs;
    public float yUnit = 1.0f;

    [Header("Z-Axis")]
    public Transform zTarget;
    public float zOfs;
    public float zUnit = 1.0f;

    float GetScale(Vector3 from, Vector3 to, float ofs, float unit) {
        var dist = (to - from).magnitude + ofs;
        var scale = dist/unit;
        if(scale <= 0f)
            scale = Mathf.Epsilon;
        return scale;
    }

    void Update() {
        var pos = transform.position;
        var s = transform.localScale;

        //TODO: negative?

        if(xTarget)
            s.x = GetScale(pos, xTarget.position, xOfs, xUnit);
        if(yTarget)
            s.y = GetScale(pos, yTarget.position, yOfs, yUnit);
        if(xTarget)
            s.z = GetScale(pos, zTarget.position, zOfs, zUnit);

        transform.localScale = s;
    }
}
