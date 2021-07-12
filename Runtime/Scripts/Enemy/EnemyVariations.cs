using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ClassicFPS.Enemy;

public class EnemyVariations : MonoBehaviour
{
    /*
    Enemy 0 (Male Jeans) = Enemy1Map_Male Jeans_PSD, Enemy1 (Cube), Speed 1, Health 100
    Enemy 1 (Male Jeans & Shirt) = Enemy1Map_Male Jeans And Shirt_PSD, Enemy1 (Cube), 1, Health 100
    Enemy 2 (Male Bearded) = Enemy1Map_Male Jeans Button Down Shirt Beard_PSD, Enemy1_Bearded (Cube), 1, Health 100
    Enemy 3 (Male Naked) = Enemy1Map_PSD, Default, Health 100, Enemy1 (Cube), Health 100
    Enemy 4 (Female) = Enemy1Map_Female Jeans Shirt_PSD,  Enemy1_Female (Cube), Health 100
    Enemy 5 (Female Large) = Enemy1Map_Female Jeans Shirt_PSD,  Enemy1_Female (Cube), Health 200
    Enemy 6 (Male Jeans & Shirt & Short Hair) = Enemy1Map_Male Jeans And Shirt Short Hair_PSD, Enemy1_ShortHair (Cube), 1, Health 100
    Enemy 7 (Male Large Jeans & Shirt & Short Hair)
    Enemy 8 (Male Large Jeans & Short Hair)
     */

    [Header("Variations")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private int variationID = -1;
    [SerializeField] Texture[] textures;
    [SerializeField] Mesh[] meshes;
    [SerializeField] float[] speedMultipliers;
    [SerializeField] int[] maxHealths; //If this is zero, it does not influece the default health
    [SerializeField] AudioClip[] awakenSounds;
    [SerializeField] float[] pitches;
    [SerializeField] float[] scaleMultipliers;
    [SerializeField] float[] shootDelays;
    [SerializeField] float[] projectileSpeeds;

    [Header("References")]
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] Enemy enemy;
    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        if(isActive) SetupVariations();
    }

    private void SetupVariations()
    {
        if (variationID == -1) variationID = Random.Range(0, textures.Length);
        skinnedMeshRenderer.transform.parent.transform.localScale *= Random.Range(.9f, 1.1f);
        if (scaleMultipliers.Length > 0) skinnedMeshRenderer.transform.parent.transform.localScale *= scaleMultipliers[variationID];
        if (awakenSounds.Length > 0) enemy.awakenSound = awakenSounds[variationID];
        audioSource.pitch = pitches[variationID];
        if (maxHealths[0] != 0) enemy.health = maxHealths[variationID];
        enemy.agent.speed *= speedMultipliers[variationID];
        enemy.agent.speed += Random.Range(-2f, 2f);
        skinnedMeshRenderer.material.mainTexture = textures[variationID];
        skinnedMeshRenderer.sharedMesh = meshes[variationID];
        enemy.animator.speed = speedMultipliers[variationID];
        if (shootDelays.Length > 0) enemy.shootDelay = shootDelays[variationID];
        if (projectileSpeeds.Length > 0) enemy.projectileSpeed = projectileSpeeds[variationID];
        enemy.shootDelay *= Random.Range(.8f, 1.2f);
        enemy.projectileSpeed *= Random.Range(.8f, 1.2f);
    }
}