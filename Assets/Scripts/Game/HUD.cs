using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    public string heartActiveRef = "heart";
    public string heartInactiveRef = "blank";
    public float heartInactiveScale = 2.0f;
    public GameObject heartContainer;

    private UISprite[] mHearts;
    
    public UISlider lineSlider;

    public void SetHearts(int count) {
        int ind = 0;

        for(; ind < count; ind++) {
            mHearts[ind].spriteName = heartActiveRef;
            mHearts[ind].MakePixelPerfect();
        }

        for(; ind < mHearts.Length; ind++) {
            mHearts[ind].spriteName = heartInactiveRef;
            mHearts[ind].transform.localScale = new Vector3(heartInactiveScale, heartInactiveScale, 1.0f);
        }
    }

    public void SetLinePercent(float s) {
        lineSlider.sliderValue = s;
    }

    void Awake() {
        mHearts = M8.Util.GetComponentsInChildrenAlphaSort<UISprite>(heartContainer, true);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
