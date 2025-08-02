using UnityEngine;
using System.Collections.Generic;

public class CharacterProfileManager : Singleton<CharacterProfileManager>
{
    public List<CharacterProfile> profiles; // �����Ϳ��� �Ҵ��ϰų� Resources.LoadAll<CharacterProfile>("Profiles") ���� �̿�

    private Dictionary<string, CharacterProfile> profileDictionary = new Dictionary<string, CharacterProfile>();

    protected override void Initialize()
    {
        InitializeProfiles();
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
