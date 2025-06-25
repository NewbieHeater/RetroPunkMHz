using UnityEngine;
using System.Collections.Generic;

public class CharacterProfileManager : MonoBehaviour
{
    public static CharacterProfileManager Instance;
    public List<CharacterProfile> profiles; // 에디터에서 할당하거나 Resources.LoadAll<CharacterProfile>("Profiles") 등을 이용

    private Dictionary<string, CharacterProfile> profileDictionary = new Dictionary<string, CharacterProfile>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            InitializeProfiles();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void InitializeProfiles()
    {
        foreach (var profile in profiles)
        {
            if (profile != null && !profileDictionary.ContainsKey(profile.id))
                profileDictionary.Add(profile.id, profile);
        }
    }

    public CharacterProfile GetProfile(string id)
    {
        if (profileDictionary.TryGetValue(id, out CharacterProfile profile))
            return profile;
        return null;
    }
}
