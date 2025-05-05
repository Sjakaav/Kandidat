using System.Collections;
using UnityEngine;

public class ThinkingBubble : MonoBehaviour
{
    [Header("Assign your three dot GameObjects here")]
    public GameObject dot1;
    public GameObject dot2;
    public GameObject dot3;

    [Header("Time between steps (seconds)")]
    public float stepDelay = 0.5f;

    private Coroutine _anim;

    void OnEnable()
    {
        // start the animation whenever this object becomes active
        _anim = StartCoroutine(AnimateDots());
    }

    void OnDisable()
    {
        // stop it if we get disabled
        if (_anim != null)
            StopCoroutine(_anim);
    }

    private IEnumerator AnimateDots()
    {
        while (true)
        {
            // 1) show dot1 only
            dot1.SetActive(false);
            dot2.SetActive(false);
            dot3.SetActive(false);
            yield return new WaitForSeconds(stepDelay);
            
            // 1) show dot1 only
            dot1.SetActive(true);
            dot2.SetActive(false);
            dot3.SetActive(false);
            yield return new WaitForSeconds(stepDelay);

            // 2) show dot1 + dot2
            dot1.SetActive(true);
            dot2.SetActive(true);
            dot3.SetActive(false);
            yield return new WaitForSeconds(stepDelay);

            // 3) show dot1 + dot2 + dot3
            dot1.SetActive(true);
            dot2.SetActive(true);
            dot3.SetActive(true);
            yield return new WaitForSeconds(stepDelay);

            // loop back (you could also blank them all briefly if you like)
        }
    }
}