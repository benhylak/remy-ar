using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SimpleFirebaseUnity;
using UnityEngine;

public class RemyFirebaseManager : MonoBehaviour
{
    private bool hasRun = false;
    private Firebase _firebase;
    async void Start()
    {
        _firebase = Firebase.CreateNew(Secrets.FIREBASE_URL,
            Secrets.FIREBASE_CREDENTIAL);
        
        //var res = Newtonsoft.Json.JsonConvert.Deser

        var res = Newtonsoft.Json.JsonConvert.SerializeObject(new {foo = "bar"});
            
//        firebase.Child("broadcasts", true)
//            .SetValue(res, true);
       // firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));
    }
    
    void Update()
    {
        if (!hasRun)
        {
            DeserializeBurners();
            hasRun = true;
        }
    }

    async void DeserializeBurners()
    {     
        DataSnapshot snapshot = await _firebase.Child("Stove").Child("burners").GetValue();
        
        JObject snapshotJson = JObject.Parse(snapshot.RawJson);
        List<JToken> results = snapshotJson.Children().ToList();

        IList<Burner> burners = new List<Burner>();
        
        foreach (JToken result in results)
        {
            // JToken.ToObject is a helper method that uses JsonSerializer internally
            Burner b = result.First().ToObject<Burner>();
            burners.Add(b);
            Debug.Log(b.ToString());
        }
    }
}