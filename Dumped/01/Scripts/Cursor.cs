using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {
    
    public const int overlapBufferSize = 16;

    public Vector2 areaPixelSize;
    public LayerMask layersMovable;
    public LayerMask layersInvalid;
    public Transform holder;

    private bool mIsHold;
    private Collider2D[] mOverlaps = new Collider2D[overlapBufferSize];
    private int mOverlapCount;

    void OnEnable() {

    }

    void Awake() {
        
    }
    
	// Use this for initialization
	void Start () {
	}

    void LateUpdate() {
        if(mIsHold) {

        }
        else {
            Camera2D cam2D = Camera2D.main;
            Bounds levelBounds = LevelBounds.bounds;
            Vector3 mousePos = Input.mousePosition;
            Vector3 pixelPos = Camera.main.ScreenToWorldPoint(mousePos) * cam2D.pixelPerMeter;

            Vector2 areaPixelHSize = areaPixelSize * 0.5f;
            
            Vector3 curPos = transform.position;
            Vector3 newPos = new Vector3(
                (Mathf.Floor((pixelPos.x + areaPixelHSize.x*0.5f)/areaPixelHSize.x)*areaPixelHSize.x)/cam2D.pixelPerMeter,
                (Mathf.Floor((pixelPos.y + areaPixelHSize.y*0.5f)/areaPixelHSize.y)*areaPixelHSize.y)/cam2D.pixelPerMeter, 
                curPos.z);

            Vector2 areaHSize = areaPixelHSize/cam2D.pixelPerMeter;

            if(newPos.x - areaHSize.x < levelBounds.min.x)
                newPos.x = levelBounds.min.x + areaHSize.x;
            else if(newPos.x + areaHSize.x > levelBounds.max.x)
                newPos.x = levelBounds.max.x - areaHSize.x;

            if(newPos.y - areaHSize.y < levelBounds.min.y)
                newPos.y = levelBounds.min.y + areaHSize.y;
            else if(newPos.y + areaHSize.y > levelBounds.max.y)
                newPos.y = levelBounds.max.y - areaHSize.y;

            transform.position = newPos;
        }
        //compute new cursor position
        //switch()
    }
}
