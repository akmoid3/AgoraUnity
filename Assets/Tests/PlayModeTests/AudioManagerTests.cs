using NUnit.Framework;
using UnityEngine;

public class AudioManagerTests
{
    private GameObject go1;
    private GameObject go2;

    [SetUp]
    public void Setup()
    {
        // Reset tra i test
        AudioManager.instance = null;
    }

    [TearDown]
    public void Teardown()
    {
        if (go1 != null) Object.DestroyImmediate(go1);
        if (go2 != null) Object.DestroyImmediate(go2);
    }

    [Test]
    public void Start_SetsInstance_WhenFirstCreated()
    {
        go1 = new GameObject("AudioManager1");
        go1.AddComponent<AudioSource>();
        var manager = go1.AddComponent<AudioManager>();

        manager.Start();

        Assert.AreEqual(manager, AudioManager.instance);
        Assert.IsNotNull(manager.audioSource);
    }

    [Test]
    public void Start_DestroysSecondInstance()
    {
        go1 = new GameObject("AudioManager1");
        go1.AddComponent<AudioSource>();
        var first = go1.AddComponent<AudioManager>();
        first.Start();

        go2 = new GameObject("AudioManager2");
        go2.AddComponent<AudioSource>();
        var second = go2.AddComponent<AudioManager>();
        second.Start();

        Assert.AreEqual(first, AudioManager.instance);
    }

    [Test]
    public void PlayOneShot_CallsAudioSourcePlayOneShot()
    {
        go1 = new GameObject("AudioManager1");
        var source = go1.AddComponent<AudioSource>();
        var manager = go1.AddComponent<AudioManager>();
        manager.Start();

        var clip = AudioClip.Create("test", 44100, 1, 44100, false);

        Assert.DoesNotThrow(() => manager.PlayOneShot(clip));
    }
}