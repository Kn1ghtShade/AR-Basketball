using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoopScript : MonoBehaviour
{
    public TextMeshPro scoreboard;
    public ParticleSystem confetti;
    bool cooldown = false;
    int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!cooldown)
        {
            score++;
            scoreboard.text = "" + score;
            StartCoroutine("RunCooldown");
        }
    }

    IEnumerator RunCooldown()
    {
        cooldown = true;
        confetti.Play();
        yield return new WaitForSeconds(1);
        cooldown = false;
    }
}
