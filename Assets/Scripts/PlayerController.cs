using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public PlayerInput playerInput;
    PlayerInput.MainActions input;

     CharacterController controller;
    Animator animator;
    AudioSource audioSource;

    [Header("Controller")]
    public float health;
    public float moveSpeed = 5;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;
    public float sprintSpeed = 10;
    public float crouchSpeed = 3;
    public float crouchYScale;
    public float startYScale;

    Vector3 _PlayerVelocity;

    bool isGrounded;
    bool isSprinting;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;

    [Header("Particles")]
    public GameObject Water;

    [Header("Images")]
    public Image Healthbar;
    public Image CooldownBar1;

    float xRotation = 0f;

    void Awake()
    { 
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Water.SetActive(false);
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        // Repeat Inputs
        if(input.Attack.IsPressed())
        { Attack(); }

        SetAnimations();
        ActiveParticle();
    }

    void FixedUpdate() 
    { MoveInput(input.Movement.ReadValue<Vector2>()); }

    void LateUpdate() 
    { LookInput(input.Look.ReadValue<Vector2>()); }

    void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        controller.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;
        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable() 
    { input.Enable(); }

    void OnDisable()
    { input.Disable(); }

    void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
            _PlayerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }

    void Sprinting()
    {
        moveSpeed = sprintSpeed;
    }

    void StopSprinting()
    {
        moveSpeed = 5;
    }

    void Crouching()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        moveSpeed = crouchSpeed;
    }

    void StopCrouching()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        moveSpeed = 5;
    }


    void AssignInputs()
    {
        input.Jump.performed += ctx => Jump();
        input.Attack.started += ctx => Attack();
        input.SprintStart.performed += ctx => Sprinting();
        input.SprintFinish.performed += ctx => StopSprinting();
        input.CrouchStart.performed += ctx => Crouching();
        input.CrouchFinish.performed += ctx => StopCrouching();
        input.Ability1.performed += ctx => Skills();
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";
    public const string SKILL1 = "Skill 1";

    string currentAnimationState;

    public void ChangeAnimationState(string newState) 
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if(!attacking)
        {
            if(_PlayerVelocity.x == 0 &&_PlayerVelocity.z == 0)
            { ChangeAnimationState(IDLE); }
            else
            { ChangeAnimationState(WALK); }
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 2f;
    public int attackDamage = 1;

    public LayerMask attackLayer;

    public GameObject hitEffect;

    [Header("Skills")]
    public float skillCooldown;
    public Transform attackPoint;
    public Transform Cam;
    public GameObject Waves;
    public float throwForce;
    public float throwUpwardForce;
    public float SkillDelay;

    [SerializeField]
    bool attacking = false;
    [SerializeField]
    bool readyToAttack = true;
    [SerializeField]
    bool readyToSkill = true;
    [SerializeField]
    bool onCooldown = true;
    int attackCount;

    public void Attack()
    {
        if(!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        if(attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }
    
    void ActiveParticle()
    {
        if(attacking == true)
        {
            Water.SetActive(true);
        }
        else if(attacking == false)
        {
            Water.SetActive(false);
        } 
    }  

    IEnumerator IncrementCooldownBar()
    {
        float incrementAmount = 0.02f;
        float targetFillAmount = 1f;    

        while (CooldownBar1.fillAmount < targetFillAmount)
        {
            yield return new WaitForSeconds(0.01f);
            CooldownBar1.fillAmount += incrementAmount;
        }

        CooldownBar1.fillAmount = targetFillAmount;


        // float targetFillAmount = 1f;
        // float totalTime = 5f;
        // float incrementAmount = targetFillAmount / totalTime;
        // float waitTime = totalTime / (targetFillAmount / incrementAmount);
        
        // while (CooldownBar1.fillAmount < targetFillAmount)
        // {
        //     yield return new WaitForSeconds(waitTime);
        //     CooldownBar1.fillAmount += incrementAmount;
        // }

        // CooldownBar1.fillAmount = targetFillAmount;
    }

    void Skills()
    {
        if(!readyToSkill || attacking) return;

        CooldownBar1.fillAmount = 0;

        attacking = true;
        readyToSkill = false;
        onCooldown = true;
        
        if (onCooldown)
        {
            StartCoroutine(IncrementCooldownBar());
            // CooldownBar1.fillAmount += 1 / skillCooldown * Time.deltaTime;

            // if (CooldownBar1.fillAmount <= 1)
            // {
            //     CooldownBar1.fillAmount = 1;
            // }


        }

        ChangeAnimationState(SKILL1);

        // instantiate object to throw
        GameObject projectile = Instantiate(Waves, attackPoint.position, Cam.rotation);

        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 forceDirection = Cam.transform.forward;

        RaycastHit hit;

        if(Physics.Raycast(Cam.position, Cam.forward, out hit, 600f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }
        
        // add force
        Vector3 forceToAdd = forceDirection * throwForce + transform.up;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        // implement throwCooldown
        Invoke(nameof(ResetCooldown), skillCooldown);
        Invoke(nameof(ResetAttack), SkillDelay);
        Destroy(projectile, 2);
    }

    private void ResetCooldown()
    {
        readyToSkill = true;
        onCooldown = false;
    }

    void AttackRaycast()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        { 
            if(hit.transform.TryGetComponent<EnemyAi>(out EnemyAi T))
            { T.TakeDamage(attackDamage); }
        } 
    }

    public void DamagePlayer(int damage)
    {
        health -= damage;
        Healthbar.fillAmount = health / 100f;
    }
}