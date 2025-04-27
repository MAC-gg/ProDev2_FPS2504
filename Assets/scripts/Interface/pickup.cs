using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] Item item;

    private void OnTriggerEnter(Collider other)
    {
        IPickup canPickup = other.GetComponent<IPickup>();
        if(canPickup != null )
        {
            canPickup.getItem(item);
            if (item is Gun)
            {
                Gun gun = (Gun)item;
                gun.ammoCur = gun.ammoMax;
                gamemanager.instance.playerScript.updatePlayerUI();
                Destroy(gameObject);
            }
        }
    }
}
