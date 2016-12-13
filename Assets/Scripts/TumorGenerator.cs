using UnityEngine;
using System.Collections;

public class TumorGenerator : M8.SingletonBehaviour<TumorGenerator> {
    public const string poolGroup = "tumor";

    [System.Serializable]
    public struct Data {
        public string[] poolRefs;

        public string poolRef { get { return poolRefs[Random.Range(0, poolRefs.Length)]; } }
    }

    public Data[] levels;

    public Tumor Generate(Tumor tumorParent, Vector2 debrisPos) {
        if(tumorParent.tumorTreeLevel >= levels.Length)
            return null;

        var level = levels[tumorParent.tumorTreeLevel];
        var poolRef = level.poolRef;

        var parms = new M8.GenericParams();
        parms.Add(Tumor.parmTumorRoot, tumorParent);
        parms.Add(Tumor.parmPosition, debrisPos);

        var tumorT = M8.PoolController.Spawn(poolGroup, poolRef, poolRef, tumorParent.root, parms);
        var tumor = tumorT.GetComponent<Tumor>();

        return tumor;
    }
}
