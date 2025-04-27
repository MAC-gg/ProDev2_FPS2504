using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    [SerializeField] public GameObject model;
    [SerializeField] public Sprite itemIcon;
    [SerializeField] public ItemType itemType;

    public ParticleSystem hitEffect;
    public AudioClip[] sound;
    [Range(0, 1)] public float soundVol;
}

public enum ItemType
{
    Gun,
    Heal,
    Trap
}
