using System.Collections.Generic;
using UnityEngine;

public class CharacterProfileManager
{
    private Dictionary<string, CharacterProfile> profileDictionary = new Dictionary<string, CharacterProfile>();

    public void Init()
    {
        InitializeProfiles();
    }

    private void InitializeProfiles()
    {
        CharacterProfile[] profiles = Managers.Resource.LoadAll<CharacterProfile>($"Profiles");
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
