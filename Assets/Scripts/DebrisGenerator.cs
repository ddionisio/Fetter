using UnityEngine;
using System.Collections;

public class DebrisGenerator : MonoBehaviour {
    public const string debrisPoolGroup = "debris";

    public string debrisRef = "debrisDefault"; //pool reference
    public string debrisStatRef = "common"; //reference in StatsCollection

    public bool autoGenerate;

    public GameObject[] meshObjects; //use this if objectsFromChildren is set to false
    public bool meshObjectsFromChildren = true;

    private struct MeshItem {
        public MeshFilter filter;
        public MeshRenderer renderer;
    }

    private MeshItem[] mMeshItems;

    public void Generate() {
        var pool = M8.PoolController.GetPool(debrisPoolGroup);

        for(int i = 0; i < mMeshItems.Length; i++) {
            var parms = new M8.GenericParams();
            parms.Add(Debris.paramTrans, transform);
            parms.Add(Debris.paramStatRef, debrisStatRef);
            parms.Add(Debris.paramMesh, mMeshItems[i].filter.sharedMesh);
            parms.Add(Debris.paramMat, mMeshItems[i].renderer.sharedMaterial);

            pool.Spawn(debrisRef, name, null, parms);

            //object is no longer valid
            mMeshItems[i].renderer.gameObject.SetActive(false);
        }
    }

    void Awake() {
        if(meshObjectsFromChildren) {
            var meshRenderers = GetComponentsInChildren<MeshRenderer>();

            mMeshItems = new MeshItem[meshRenderers.Length];

            for(int i = 0; i < mMeshItems.Length; i++) {
                mMeshItems[i] = new MeshItem() { renderer = meshRenderers[i], filter = meshRenderers[i].GetComponent<MeshFilter>() };
            }
        }
        else {
            mMeshItems = new MeshItem[meshObjects.Length];

            for(int i = 0; i < mMeshItems.Length; i++) {
                mMeshItems[i] = new MeshItem() { renderer = meshObjects[i].GetComponent<MeshRenderer>(), filter = meshObjects[i].GetComponent<MeshFilter>() };
            }
        }
    }

    IEnumerator Start() {
        if(autoGenerate) {
            yield return null;

            Generate();
        }
    }
}
