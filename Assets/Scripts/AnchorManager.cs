using System;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AnchorManager : MonoBehaviour
{
   public static AnchorManager instance;

   [SerializeField] List<GameObject> anchors = new List<GameObject>();
   private Queue<GameObject> FreeAnchors = new Queue<GameObject>();

   private void Start()
   {
      if(instance == null)
         instance = this;
      else
      {
         Destroy(this);
      }
   }

   public void AddAnchor(GameObject anchor)
   {
      anchors.Add(anchor);
      FreeAnchors.Enqueue(anchor);
      TryAssignAnchorToWaitingUser();
   }

   private void TryAssignAnchorToWaitingUser()
   {
      Queue<User> waitingUsers =  UserManager.instance.WaitingUsers;
      while (FreeAnchors.Count > 0 && waitingUsers.Count > 0)
      {
         GameObject anchor = FreeAnchors.Dequeue();
         User user = waitingUsers.Dequeue();
         user.Anchor = anchor;
         SetVideo(user);
      }
   }

   public void AssignAnchorToUser(User user)
   {
      if (FreeAnchors.Count > 0)
      {
         GameObject anchor = FreeAnchors.Dequeue();
         user.Anchor = anchor;
         SetVideo(user);
      }
      else
      {
         UserManager.instance.AddWaitingUser(user);
      }
   }

   private void SetVideo(User user)
   {
      VideoSurface videoSurface = user.Anchor.GetComponent<VideoSurface>();
      string idString = user.RtcID;
      if (uint.TryParse(idString, out uint rtcId))
      {
         videoSurface.SetForUser(rtcId, user.ChannelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
      }
      else
      {
         Debug.LogError($"Failed to parse RtcID '{idString}' to uint.");
      }
   }

   public void RemoveAnchor(GameObject anchor)
   {
      anchors.Remove(anchor);
   }
}
