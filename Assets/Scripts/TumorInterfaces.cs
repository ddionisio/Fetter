using UnityEngine;
using System.Collections;

public interface ITumorRadiusChange {
    void OnRadiusChange(Tumor tumor, float delta);
}