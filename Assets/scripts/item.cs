using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    [SerializeField] public GameObject model;
    [SerializeField] public Sprite itemIcon;

    public ParticleSystem hitEffect;
    public AudioClip[] sound;
    [Range(0, 1)] public float soundVol;
}