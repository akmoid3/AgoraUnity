using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Object", menuName = "Scriptable Objects/App Variables")]
public class AppVariables : ScriptableObject
{
    public string username;
    public string appID;
    public string tokenChannel;
    public string channelName;
    public string rtmToken;
}
