using UnityEngine;
using System.Collections;

public class LevelBounds : MonoBehaviour {
    private static Bounds mBounds = new Bounds();

    public string[] tags;

    public static Bounds bounds { get { return mBounds; } }

    void OnEnable() {
        Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 max = Vector3.zero;

        for(int i = 0; i < tags.Length; i++) {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tags[i]);
            for(int j = 0; j < gos.Length; j++) {
                Collider2D col = gos[j].collider2D;
                if(col) {
                    Bounds b = col.bounds;
                    
                    min = Vector3.Min(min, b.min);
                    max = Vector3.Max(max, b.max);
                }
            }
        }

        mBounds.SetMinMax(min, max);
    }
}
