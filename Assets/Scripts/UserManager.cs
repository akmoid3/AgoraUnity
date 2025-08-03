using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public static UserManager instance;
    
    [SerializeField] private GameObject userPrefab;
    
    private List<User> users = new List<User>();
    private Queue<User> waitingUsers = new Queue<User>();

    public Queue<User> WaitingUsers
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
        if(instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }
    }

    public void AddUser(User user)
    {
        users.Add(user);
        AnchorManager.instance.AssignAnchorToUser(user);
    }

    public void RemoveUser(User user)
    {
        users.Remove(user);
    }

    public void AddWaitingUser(User user)
    {
        waitingUsers.Enqueue(user);
    }

    public void RemoveWaitingUser()
    {
        waitingUsers.Dequeue();
    }

}
