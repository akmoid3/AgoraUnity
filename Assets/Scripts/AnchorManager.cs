using System;
using System.Collections.Generic;
using System.Linq;
using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AnchorManager : MonoBehaviour
{
    public static AnchorManager instance;

    [SerializeField] public List<GameObject> anchors = new List<GameObject>();
    public List<GameObject> FreeAnchors = new List<GameObject>();
    public GameObject ancoraProva;

    public void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }
    }

    public void AddAnchor(GameObject anchor)
    {
        anchors.Add(anchor);
        FreeAnchors.Add(anchor);
        TryAssignAnchorToWaitingUser();
    }

    public void TryAssignAnchorToWaitingUser()
    {
        List<User> waitingUsers = UserManager.instance.WaitingUsers;
        while (FreeAnchors.Count > 0 && waitingUsers.Count > 0)
        {
            GameObject anchor = FreeAnchors[0];
            FreeAnchors.RemoveAt(0);

            User user = waitingUsers[0];
            waitingUsers.RemoveAt(0);

            user.Anchor = anchor;
            SetVideo(user);
            
            NameDisplayer nameDisplayer = user.Anchor.GetComponentInChildren<NameDisplayer>();
            String name = user.RtmID;
            // TextMeshProUGUI textMeshPro = user.Anchor.GetComponentInChildren<TextMeshProUGUI>();
            // textMeshPro.text = user.RtmID;
            if (nameDisplayer)
            {
                if (!string.IsNullOrEmpty(user.RtmID))
                {
                    nameDisplayer.DisplayName(user.RtmID);
                }
                else
                {
                    user.OnRtmIdAvailable += (id) => nameDisplayer.DisplayName(id);
                }
            }
        }
    }


    public void AssignAnchorToUser(User user)
    {
        if (FreeAnchors.Count > 0)
        {
            GameObject anchor = FreeAnchors[0];
            FreeAnchors.RemoveAt(0);
        
            user.Anchor = anchor;
        
            SetVideo(user);

            NameDisplayer nameDisplayer = user.Anchor.GetComponentInChildren<NameDisplayer>();
            String name = user.RtmID;
            // TextMeshProUGUI textMeshPro = user.Anchor.GetComponentInChildren<TextMeshProUGUI>();
            // textMeshPro.text = user.RtmID;
            if (nameDisplayer)
            {
                if (!string.IsNullOrEmpty(user.RtmID))
                {
                    nameDisplayer.DisplayName(user.RtmID);
                }
                else
                {
                    user.OnRtmIdAvailable += (id) => nameDisplayer.DisplayName(id);
                }
            }
        }
        else
        {
            UserManager.instance.AddWaitingUser(user);
        }
    }

    private void SetVideo(User user)
    {
        VideoSurface videoSurface = user.Anchor.GetComponentInChildren<VideoSurface>();
        string idString = user.RtcID;
        if (uint.TryParse(idString, out uint rtcId))
        {
            videoSurface.SetForUser(rtcId, user.ChannelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                // var transform = videoSurface.GetComponent<RectTransform>();
                // if (transform)
                // {
                //     //If render in RawImage. just set rawImage size.
                //     transform.sizeDelta = new Vector2(width / 2, height / 2);
                //     transform.localScale = Vector3.one;
                // }
                // else
                // {
                //     // //If render in MeshRenderer, just set localSize with MeshRenderer
                //     // float scale = (float)height / (float)width;
                //     // videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                // }

                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
        }
        else
        {
            Debug.LogError($"Failed to parse RtcID '{idString}' to uint.");
        }
    }

    public void RemoveAnchor(GameObject anchor)
    {
        var assignedUser = UserManager.instance.Users.Find(u => u.Anchor == anchor);
        if (assignedUser != null)
        {
            assignedUser.Anchor = null;
            UserManager.instance.AddWaitingUser(assignedUser);
        }

        anchors.Remove(anchor);
        FreeAnchors.Remove(anchor);
        Destroy(anchor);
    }

    public void OnCallQuit()
    {
        foreach (var anchor in anchors)
        {
            Destroy(anchor);
        }

        anchors.Clear();
        FreeAnchors.Clear();
    }
}