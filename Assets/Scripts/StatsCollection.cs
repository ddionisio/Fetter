using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatsCollection : M8.SingletonBehaviour<StatsCollection> {
    [System.Serializable]
    public struct Item {
        public string id;
        public float value;
        public bool clamp;
    }

    [System.Serializable]
    public struct Set {
        public string name;
        public Item[] items;

        public M8.StatItem[] GenerateStatItems(List<M8.StatTemplateData> template) {
            var ret = new M8.StatItem[items.Length];
            for(int i = 0; i < items.Length; i++) {
                //get ID
                int id = -1;
                foreach(var templateItem in template) {
                    if(templateItem.name == items[i].id) {
                        id = templateItem.id;
                        break;
                    }
                }

                ret[i] = new M8.StatItem(id, items[i].value, items[i].clamp);
            }

            return ret;
        }
    }

    [System.Serializable]
    public struct SetList {
        public List<Set> items;

        public static string ToJSON(List<Set> items, bool prettyPrint) {
            return JsonUtility.ToJson(new SetList() { items=items }, prettyPrint);
        }

        public static List<Set> FromJSON(string json) {
            return !string.IsNullOrEmpty(json) ? JsonUtility.FromJson<SetList>(json).items : new List<Set>();
        }
    }

    public TextAsset templateFile;
    public TextAsset collectionFile;

    private Dictionary<string, M8.StatItem[]> mStats;

    public M8.StatItem[] GetStatItems(string setName) {
        M8.StatItem[] ret = null;
        mStats.TryGetValue(setName, out ret);
        return ret;
    }

    protected override void OnInstanceInit() {
        var template = M8.StatTemplateList.FromJSON(templateFile.text);
        var setList = SetList.FromJSON(collectionFile.text);

        mStats = new Dictionary<string, M8.StatItem[]>(setList.Count);

        foreach(var set in setList)
            mStats.Add(set.name, set.GenerateStatItems(template));
    }
}
