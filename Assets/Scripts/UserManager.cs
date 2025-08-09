using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager instance;
    
    [SerializeField] private GameObject userPrefab;
    
    [SerializeField] private List<User> users = new List<User>();
    private List<User> waitingUsers = new List<User>();

    public List<User> WaitingUsers
    {
        get => waitingUsers;
        set => waitingUsers = value;
    }
    public GameObject UserPrefab
    {
        get => userPrefab;
        set => userPrefab = value;
    }
    public List<User> Users
    {
        get => users;
        set => users = value;
    }

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void AddUser(User user)
    {
        users.Add(user);
        AnchorManager.instance.AssignAnchorToUser(user);
    }

    public void RemoveUser(User user)
    {
        users.Remove(user);
        waitingUsers.Remove(user); 
    }

    public void AddWaitingUser(User user)
    {
        if (!waitingUsers.Contains(user))
            waitingUsers.Add(user);
    }

    public void RemoveWaitingUser(User user)
    {
        waitingUsers.Remove(user);
    }

    public void OnCallQuit()
    {
        users.Clear();
        waitingUsers.Clear();
    }

    public User GetUser(string uid)
    {
        foreach (var user in users)
        {
            if (user.RtcID == uid)
                return user;
        }
        return null;
    }
}