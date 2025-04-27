using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;

    [SerializeField] public int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    [SerializeField] List<Item> inv = new List<Item>();
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject itemModel;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    // helpers
    int jumpCount;
    int invPos;
    float shootTimer;
    float healTimer;
    Vector3 moveDir;
    Vector3 playerVel;

    // cache
    int origHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origHP = HP;
        spawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.yellow);

        if(!gamemanager.instance.isPaused)
        {
            movement();
        }

        sprint();
    }

    void movement()
    {
        // reset jump count and playervel
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        // update movedir
        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);

        // move the controller
        controller.Move(moveDir * speed * Time.deltaTime);

        // check for jump
        jump();
        // always subtracting gravity
        playerVel.y -= gravity * Time.deltaTime;
        // move again
        controller.Move(playerVel * Time.deltaTime);

        // shoot timer
        shootTimer += Time.deltaTime;
        if (Input.GetButton("Fire1") &&
            inv.Count > 0)
        {
            // SWITCH
            Item item = inv[invPos];
            if (item is Gun)
            {
                // cast item as gun
                Gun gun = (Gun)item;
                // check ammo and timer
                if (gun.ammoCur > 0 &&
                   shootTimer >= shootRate)
                { shoot(gun); } // SHOOT
            }
            else if (item is Heal)
            {
                // cast item as heal
                Heal heal = (Heal)item;
                healPlayer(heal.instantAmt, heal.hotAmt, heal.sec);
            }
            else if (item is Trap)
            {
                // cast item as trap
                Trap trap = (Trap)item;
                placeTrap(trap);
            }
        }

        selectItem();
        reload();
    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    void sprint()
    {
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void shoot(Gun gun)
    {
        // reset timer
        shootTimer = 0;
        gun.ammoCur--;
        updatePlayerUI();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Instantiate(gun.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamageScreen());

        if (HP <= 0)
        {
            // You lose!
            gamemanager.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        gamemanager.instance.playerHPBar.fillAmount = (float)HP / origHP;

        if (inv[invPos] is Gun)
        {
            // show ammo
            gamemanager.instance.ammoCur.text = ((Gun)inv[invPos]).ammoCur.ToString("F0");
            gamemanager.instance.ammoMax.text = ((Gun)inv[invPos]).ammoMax.ToString("F0");
        }
        else
        {
            // hide
            gamemanager.instance.ammoCur.text = "";
            gamemanager.instance.ammoMax.text = "";
        }
    }

    IEnumerator flashDamageScreen()
    {
        gamemanager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageScreen.SetActive(false);
    }

    public void getItem(Item item)
    {
        inv.Add(item);
        invPos = inv.Count - 1;
        changeItem(item);
    }

    void selectItem()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && invPos < inv.Count - 1)
        {
            invPos++;
            changeItem(inv[invPos]);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && invPos > 0)
        {
            invPos--;
            changeItem(inv[invPos]);
        }
    }

    void changeItem(Item item)
    {
        
        if (item is Gun)
        {
            // change shoot stats
            changeGun((Gun)item);
            // bring gun mesh
            gunModel.GetComponent<MeshFilter>().sharedMesh = item.model.GetComponent<MeshFilter>().sharedMesh;
            // bring gun material (1) - shader
            gunModel.GetComponent<MeshRenderer>().sharedMaterial = item.model.GetComponent<MeshRenderer>().sharedMaterial;
        } else
        {
            // bring gun mesh
            itemModel.GetComponent<MeshFilter>().sharedMesh = item.model.GetComponent<MeshFilter>().sharedMesh;
            // bring gun material (1) - shader
            itemModel.GetComponent<MeshRenderer>().sharedMaterial = item.model.GetComponent<MeshRenderer>().sharedMaterial;
        }

        updatePlayerUI();
    }
    void changeGun(Gun gun)
    {
        shootDamage = gun.shootDmg;
        shootDist = gun.shootDist;
        shootRate = gun.shootRate;
    }

    void reload()
    {
        if(Input.GetButtonDown("Reload") && inv.Count > 0 && inv[invPos] is Gun)
        {
            Gun gun = (Gun)inv[invPos];
            if(gun.ammoCur != gun.ammoMax)
            {
                gun.ammoCur = gun.ammoMax;
                updatePlayerUI();
            }
        }
    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;
        HP = origHP;
        updatePlayerUI();
    }

    void healPlayer(int instantAmt, int hotAmt, int sec)
    {
        // instant heal
        if (HP + instantAmt < origHP)
            HP += instantAmt;
        else
            HP = origHP; // full

        // hot
        if (hotAmt > 0 && HP != origHP) // not full health
        {
            while (sec > 0)
            {
                StartCoroutine(healSecond(hotAmt));
                sec--;
            }
        }
    }

    IEnumerator healSecond(int hotAmt)
    {
        yield return new WaitForSeconds(1f);
        HP += hotAmt;
    }

    void placeTrap(Trap trap)
    {
        // crouch
        // instantiate trap.trap??
        // uncrouch
    }
}
