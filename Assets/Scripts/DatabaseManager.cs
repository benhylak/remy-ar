using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using Newtonsoft.Json.Linq;
using SimpleFirebaseUnity;
using UnityEngine;
using UniRx;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private bool hasRun = false;
    private Firebase _firebase;

    private ReactiveCollection<Burner> _burners;
    
    void Start()
    {
        _firebase = Firebase.CreateNew(Secrets.FIREBASE_URL,
            Secrets.FIREBASE_CREDENTIAL);
        
        _burners = new ReactiveCollection<Burner>();
    }

    public ReactiveCollection<Burner> getBurners()
    {
        return _burners;
    }
    
    void Update()
    {
        UpdateBurners();
    }

    async void UpdateBurners()
    {     
        DataSnapshot snapshot = await _firebase.Child("Stove").Child("burners").GetValue();
        
        foreach (string key in snapshot.Keys)
        {
            var dict = snapshot.GetValueForKey<IDictionary<string, object>>(key);

            var existingBurner = _burners.FirstOrDefault(x => x.Position == Burner.ParsePosition(key));

            if (existingBurner == default(Burner))
            {
                var b = Burner.CreateFromDict(dict);
                _burners.Add(b);
                Debug.Log("Creating Burner: " + b);
                b.IsPotDetected.Subscribe(_ => Debug.Log(b.Position + " Pot Detected: " + b.IsPotDetected.Value));

            }
            else
            {
                existingBurner.MapFromDict(dict);
            }
        }
        
    }
}