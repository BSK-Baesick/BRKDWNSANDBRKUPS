using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructBEAT : MonoBehaviour
{

    // Configuration Parameters
    [SerializeField] float selfDestructTimer = 5f;

    // Cached References
    // ScoreManager scoreManager;

    private void Start()
    {
        // Sets the scoreManager variable to "ScoreManager.cs" component
        // scoreManager = GameObject.Find("Score Manager").GetComponent<ScoreManager>();

        // Destroy this gameObject after a period of time
        StartCoroutine(SelfDestruct());
    }

    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(selfDestructTimer);
        // Updates the scoreGUI
        // scoreManager.SubtractScore();
        Destroy(this.gameObject);
    }
}
