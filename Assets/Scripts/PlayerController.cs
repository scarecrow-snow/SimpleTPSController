using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;




[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;

    [SerializeField]
    private float rotationSpeed = 5f;

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private Transform barrelTransform;

    [SerializeField]
    private Transform bulletParentTransform;

    [SerializeField]
    private float bulletHitMissDistance = 25f;

    [SerializeField]
    private float animationSmothTime = 0.1f;

    [SerializeField]
    private float animationTransitionTime = 0.15f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;



    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;


    [SerializeField] Animator animator;

    int jumpAnimation;
    int moveXAnimationParameterId;
    int moveZAnimationParameterId;

    Vector2 currentAnimationBlendVevtor;
    Vector2 animationVelecity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];


        // カーソルを消す
        Cursor.lockState = CursorLockMode.Locked;


        // Animator
        animator = GetComponent<Animator>();
        jumpAnimation = Animator.StringToHash("Jump");
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
        
    }

    void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        currentAnimationBlendVevtor = Vector2.SmoothDamp(currentAnimationBlendVevtor, input, ref animationVelecity, animationSmothTime);
        Vector3 move = new Vector3(currentAnimationBlendVevtor.x, 0, currentAnimationBlendVevtor.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVevtor.x);
        animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVevtor.y);

        // ジャンプ
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.CrossFade(jumpAnimation, animationTransitionTime);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // カメラ方向への回転
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }


    private void ShootGun()
    {
        RaycastHit hit;
        var bullet = Instantiate(bulletPrefab, barrelTransform.position, Quaternion.identity, bulletParentTransform);
        BulletController bulletController = bullet.GetComponent<BulletController>();

        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            bulletController.target = hit.point;
            bulletController.hit = true;
        }
        else
        {
            bulletController.target = cameraTransform.position + cameraTransform.forward * bulletHitMissDistance;
            bulletController.hit = false;
        }
    }
}