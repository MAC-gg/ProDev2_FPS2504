using UnityEngine;

[CreateAssetMenu]

public class Heal : Item
{
    [Range(0, 50)] public int instantAmt;
    [Range(0, 10)] public int hotAmt;
    [Range(1, 10)] public int sec;
}
