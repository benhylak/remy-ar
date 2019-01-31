using SimpleFirebaseUnity;
using UnityEngine;

public class RemyFirebaseManager : MonoBehaviour
{
    void Start()
    {
        Firebase firebase = Firebase.CreateNew(Secrets.FIREBASE_URL,
            Secrets.FIREBASE_CREDENTIAL);

        var res = Newtonsoft.Json.JsonConvert.SerializeObject(new {foo = "bar"});
            
        firebase.Child("broadcasts", true)
            .SetValue(res, true);
       // firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));
    }
}