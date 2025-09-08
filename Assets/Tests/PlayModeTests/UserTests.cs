using NUnit.Framework;
using UnityEngine;

public class UserTests
{
    private GameObject testAnchor;
    private Animator animator;
    private User user;

    [SetUp]
    public void Setup()
    {
        // Creiamo un GameObject con Animator
        testAnchor = new GameObject("Anchor");
        animator = testAnchor.AddComponent<Animator>();

        // Creiamo l'utente
        user = new User("rtm123", "rtc456", null, "TestChannel");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testAnchor);
    }

    [Test]
    public void Constructor_AssignsValuesCorrectly()
    {
        Assert.AreEqual("rtm123", user.RtmID);
        Assert.AreEqual("rtc456", user.RtcID);
        Assert.AreEqual("TestChannel", user.ChannelName);
        Assert.IsNull(user.Anchor); // inizialmente null
    }

    [Test]
    public void SettingAnchor_InitializesAnimator()
    {
        // Assegno l'anchor
        user.Anchor = testAnchor;

        Assert.AreEqual(testAnchor, user.Anchor);
        Assert.IsNotNull(animator);
    }

    [Test]
    public void HandRaise()
    {
        user.Anchor = testAnchor; // inizializza l’animator
        user.IsHandRaised = true;

        Assert.IsTrue(user.IsHandRaised);
    }

    [Test]
    public void HandRaise_DoesNotThrow_WhenAnimatorIsMissing()
    {
        // Creo un anchor senza Animator
        GameObject noAnimatorAnchor = new GameObject("NoAnimator");
        user.Anchor = noAnimatorAnchor;

        // Non deve lanciare eccezioni
        Assert.DoesNotThrow(() => user.IsHandRaised = true);

        Object.DestroyImmediate(noAnimatorAnchor);
    }

  
    
    [Test]
    public void Setters_UpdateValuesCorrectly()
    {
        // Valori iniziali
        Assert.AreEqual("rtm123", user.RtmID);
        Assert.AreEqual("rtc456", user.RtcID);
        Assert.AreEqual("TestChannel", user.ChannelName);

        // Aggiorno i valori tramite i setter
        user.RtmID = "newRtm";
        user.RtcID = "newRtc";
        user.ChannelName = "NewChannel";

        // Verifico che siano stati aggiornati correttamente
        Assert.AreEqual("newRtm", user.RtmID);
        Assert.AreEqual("newRtc", user.RtcID);
        Assert.AreEqual("NewChannel", user.ChannelName);
    }

}
