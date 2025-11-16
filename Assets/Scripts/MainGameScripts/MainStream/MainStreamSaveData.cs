using System;

[Serializable]
public class BoolVarEntry
{
    public string key;
    public bool value;
}

[Serializable]
public class IntVarEntry
{
    public string key;
    public int value;
}

[Serializable]
public class FlagEntry
{
    public string key;
    public bool value;
}

[Serializable]
public class MainStreamSaveData
{
    public string currentStateId;
    public BoolVarEntry[] boolVars;
    public IntVarEntry[] intVars;
    public FlagEntry[] flags;
}
