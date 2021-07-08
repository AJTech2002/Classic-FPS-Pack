using System.Collections;
using System.Collections.Generic;
using ClassicFPS.Managers;
using UnityEngine;

public class LayeredMusic : MonoBehaviour
{
    [SerializeField] int startWhenAmountOfEnemiesFollowingIs = 0;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Vector2 minMaxVolume = new Vector2(0, 1);
    float goToVolume = 0;
    [SerializeField] float fadeSpeed = 1;
    [SerializeField] bool requireNumberOfEnemiesToBeExact = false; //If this is true, volume will only fade in if the enemies followins is exactly the same as startWhenAmountOfEnemiesFollowingIs!
    [SerializeField] bool requireAllEnemiesToBeDeadToFadeOut = false; 
    // Start is called before the first frame update
    void Start()
    {
        audioSource.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if ((GameManager.PlayerController.enemiesFollowing >= startWhenAmountOfEnemiesFollowingIs && !requireNumberOfEnemiesToBeExact)
           ||(GameManager.PlayerController.enemiesFollowing == startWhenAmountOfEnemiesFollowingIs && requireNumberOfEnemiesToBeExact))
        {
            goToVolume = minMaxVolume.y;
        }
        else
        {
            if (!requireAllEnemiesToBeDeadToFadeOut ||
            (requireAllEnemiesToBeDeadToFadeOut && GameManager.PlayerController.enemiesFollowing == 0))
            {
                goToVolume = minMaxVolume.x;
            }
        }

        audioSource.volume += (goToVolume - audioSource.volume) * (Time.deltaTime * fadeSpeed);
    }
}
