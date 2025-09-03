using System.Collections.Generic;
using UnityEngine;

public class NameDisplayer : MonoBehaviour
{
    [Header("Letter Prefabs (A-Z)")]
    [SerializeField] private List<GameObject> letterPrefabs; 
    
    private Dictionary<char, GameObject> letterMap;

    [Header("Display Settings")]
    [SerializeField] private Transform displayParent;
    [SerializeField] private float letterSpacing = 0.1f;
    [SerializeField] private bool arrangeHorizontally = true;

    private List<GameObject> instantiatedLetters = new List<GameObject>();

    private void Awake()
    {
        letterMap = new Dictionary<char, GameObject>();

        for (int i = 0; i < letterPrefabs.Count && i < 26; i++)
        {
            char letter = (char)('A' + i);
            if (letterPrefabs[i] != null)
            {
                letterMap[letter] = letterPrefabs[i];
            }
        }
    }

    public void DisplayName(string name)
    {
        ClearDisplayedLetters();

        if (string.IsNullOrEmpty(name))
            return;

        name = name.ToUpper();
        Vector3 currentPosition = displayParent.position;

        foreach (char letter in name)
        {
            if (letter == ' ')
            {
                if (arrangeHorizontally)
                    currentPosition.x -= letterSpacing;
                else
                    currentPosition.y -= letterSpacing;
                continue;
            }

            if (letterMap.TryGetValue(letter, out GameObject prefab))
            {
                GameObject letterInstance = Instantiate(prefab, currentPosition, displayParent.rotation, displayParent);
                instantiatedLetters.Add(letterInstance);

                // Calcola larghezza della lettera dal suo Renderer
                Renderer rend = letterInstance.GetComponentInChildren<Renderer>();
                float width = (rend != null) ? rend.bounds.size.x : letterSpacing;

                if (arrangeHorizontally)
                    currentPosition.x -= width + letterSpacing;
                else
                    currentPosition.y -= width + letterSpacing;
            }
        }

        CenterLetters();
    }

    private void ClearDisplayedLetters()
    {
        foreach (GameObject letter in instantiatedLetters)
        {
            if (letter != null)
                Destroy(letter);
        }
        instantiatedLetters.Clear();
    }

    private void CenterLetters()
    {
        if (instantiatedLetters.Count == 0)
            return;

        Vector3 totalPosition = Vector3.zero;
        foreach (GameObject letter in instantiatedLetters)
        {
            totalPosition += letter.transform.position;
        }
        Vector3 centerPosition = totalPosition / instantiatedLetters.Count;
        Vector3 offset = displayParent.position - centerPosition;

        foreach (GameObject letter in instantiatedLetters)
        {
            letter.transform.position += offset;
        }
    }
}
