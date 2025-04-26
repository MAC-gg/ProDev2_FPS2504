using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] gunStats gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup canPickup = other.GetComponent<IPickup>();
        if(canPickup != null )
        {
            canPickup.getGunStats(gun);
            gun.ammoCur = gun.ammoMax;
            gamemanager.instance.playerScript.updatePlayerUI();
            Destroy(gameObject);
        }
    }
}
