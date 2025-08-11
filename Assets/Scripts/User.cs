using UnityEngine;

public class User
{
   private string rtmID;
   private string rtcID;
   private string channelName;

   private GameObject anchor;
   private bool isHandRaised = false;
   private Animator animator;

   public bool IsHandRaised
   {
      get => isHandRaised;
      set
      {
         isHandRaised = value;
         if(animator)
            animator.SetBool("HandRaised", value);
      }
   }

   public GameObject Anchor
   {
      get => anchor;
      set
      {
         anchor = value;
         if (anchor && animator == null)
         {
            animator = anchor.GetComponentInChildren<Animator>();
            
            if(animator)
               animator.SetBool("HandRaised", isHandRaised);
         }
      }
   }

   public string ChannelName
   {
      get => channelName;
      set => channelName = value;
   }

   public string RtmID
   {
      get => rtmID;
      set => rtmID = value;
   }

   public string RtcID
   {
      get => rtcID;
      set => rtcID = value;
   }

   public User(string rtmID, string rtcID, GameObject anchor, string channelName)
   {
      this.rtmID = rtmID;
      this.rtcID = rtcID;
      this.anchor = anchor;
      this.channelName = channelName;
   }

   
}
