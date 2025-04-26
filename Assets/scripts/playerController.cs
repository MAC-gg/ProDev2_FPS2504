using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    // helpers
    int jumpCount;
    int gunListPos;
    float shootTimer;
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
            gunList.Count > 0 &&
            gunList[gunListPos].ammoCur > 0 &&
            shootTimer >= shootRate)
        { shoot(); }

        selectGun();
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

    void shoot()
    {
        // reset timer
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        updatePlayerUI();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.identity);

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
        if (gunList.Count > 0)
        {
            gamemanager.instance.ammoCur.text = gunList[gunListPos].ammoCur.ToString("F0");
            gamemanager.instance.ammoMax.text = gunList[gunListPos].ammoMax.ToString("F0");
        }
    }

    IEnumerator flashDamageScreen()
    {
        gamemanager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageScreen.SetActive(false);
    }

    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }
    }

    void changeGun()
    {
        shootDamage = gunList[gunListPos].shootDmg;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        // bring gun mesh
        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].model.GetComponent<MeshFilter>().sharedMesh;
        // bring gun material (1) - shader
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].model.GetComponent<MeshRenderer>().sharedMaterial;

        updatePlayerUI();
    }

    void reload()
    {
        if(Input.GetButtonDown("Reload") && gunList.Count > 0 && gunList[gunListPos].ammoCur != gunList[gunListPos].ammoMax)
        {
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
            updatePlayerUI();
        }
    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;
        HP = origHP;
        updatePlayerUI();
    }
}
