using UnityEngine;

[CreateAssetMenu]

public class Gun : Item
{
    [Range(1,10)] public int shootDmg;
    [Range(5, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCur;
    [Range(5, 50)] public int ammoMax;
}
