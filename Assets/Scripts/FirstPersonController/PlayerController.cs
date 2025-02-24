﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private HeadBobData headBobData = null;

    #region Movement
    [Space, Header("Movement Settings")]
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float jumpSpeed = 5f;
    [Range(0f, 1f)] [SerializeField] private float moveBackwardsSpeedPercent = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float moveSideSpeedPercent = 0.75f;
    #endregion

    #region Run Settings
    [Space, Header("Run Settings")]
    [Range(-1f, 1f)] [SerializeField] private float canRunThreshold = 0.8f;
    [SerializeField] private AnimationCurve runTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    #endregion

    #region Crouch Settings
    [Space, Header("Crouch Settings")]
    [Range(0.2f, 0.9f)] [SerializeField] private float crouchPercent = 0.6f;
    [SerializeField] private float crouchTransitionDuration = 1f;
    [SerializeField] private AnimationCurve crouchTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    #endregion

    #region Landing Settings
    [Space, Header("Landing Settings")]
    [Range(0.05f, 0.5f)] [SerializeField] private float lowLandAmount = 0.1f;
    [Range(0.2f, 0.9f)] [SerializeField] private float highLandAmount = 0.6f;
    [SerializeField] private float landTimer = 0.5f;
    [SerializeField] private float landDuration = 1f;
    [SerializeField] private AnimationCurve landCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    #endregion

    #region Gravity
    [Space, Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float stickToGroundForce = 5f;

    [SerializeField] private LayerMask groundLayer = ~0;
    [Range(0f, 1f)] [SerializeField] private float rayLength = 0.1f;
    [Range(0.01f, 1f)] [SerializeField] private float raySphereRadius = 0.1f;
    #endregion

    #region Wall Settings
    [Space, Header("Check Wall Settings")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
    [Range(0.01f, 1f)] [SerializeField] private float rayObstacleSphereRadius = 0.1f;
    #endregion

    #region Smooth Settings
    [Space, Header("Smooth Settings")]
    [Range(1f, 100f)] [SerializeField] private float smoothRotateSpeed = 5f;
    [Range(1f, 100f)] [SerializeField] private float smoothInputSpeed = 5f;
    [Range(1f, 100f)] [SerializeField] private float smoothVelocitySpeed = 5f;
    [Range(1f, 100f)] [SerializeField] private float smoothFinalDirectionSpeed = 5f;
    [Range(1f, 100f)] [SerializeField] private float smoothHeadBobSpeed = 5f;
    #endregion


    #region Components / Custom Classes / Caches
    private CharacterController m_characterController;
    private Transform m_yawTransform;
    private Transform m_camTransform;
    private HeadBob m_headBob;
    private CameraController m_cameraController;
    private PlayerControls m_playerControls;

    private RaycastHit m_hitInfo;
    private IEnumerator m_CrouchRoutine;
    private IEnumerator m_LandRoutine;
    #endregion

    #region Debug
    [Space]
    [SerializeField] [ReadOnly] private Vector2 m_inputVector;
    [SerializeField] [ReadOnly] private Vector2 m_smoothInputVector;

    [Space]
    [SerializeField] [ReadOnly] private Vector3 m_finalMoveDir;
    [SerializeField] [ReadOnly] private Vector3 m_smoothFinalMoveDir;
    [Space]
    [SerializeField] [ReadOnly] private Vector3 m_finalMoveVector;

    [Space]
    [SerializeField] [ReadOnly] private float m_currentSpeed;
    [SerializeField] [ReadOnly] private float m_smoothCurrentSpeed;
    [SerializeField] [ReadOnly] private float m_finalSmoothCurrentSpeed;
    [SerializeField] [ReadOnly] private float m_walkRunSpeedDifference;

    [Space]
    [SerializeField] [ReadOnly] private float m_finalRayLength;
    [SerializeField] [ReadOnly] private bool m_hitWall;
    [SerializeField] [ReadOnly] private bool m_isGrounded;
    [SerializeField] [ReadOnly] private bool m_previouslyGrounded;

    [Space]
    [SerializeField] [ReadOnly] private float m_initHeight;
    [SerializeField] [ReadOnly] private float m_crouchHeight;
    [SerializeField] [ReadOnly] private Vector3 m_initCenter;
    [SerializeField] [ReadOnly] private Vector3 m_crouchCenter;
    [Space]
    [SerializeField] [ReadOnly] private float m_initCamHeight;
    [SerializeField] [ReadOnly] private float m_crouchCamHeight;
    [SerializeField] [ReadOnly] private float m_crouchStandHeightDifference;
    [SerializeField] [ReadOnly] private bool m_duringCrouchAnimation;
    [SerializeField] [ReadOnly] private bool m_duringRunAnimation;
    [Space]
    [SerializeField] [ReadOnly] private float m_inAirTimer;
    #endregion

    protected virtual void Awake() {
        m_playerControls = new PlayerControls();
    }
    #region BuiltIn Methods     
    protected virtual void Start()
    {
        GetComponents();
        InitVariables();
    }

    protected virtual void Update()
    {
        if (m_yawTransform != null)
            RotateTowardsCamera();

        if (m_characterController)
        {
            // Check if Grounded,Wall etc
            CheckIfGrounded();
            CheckIfWall();

            // Apply Smoothing
            SmoothInput();
            SmoothSpeed();
            SmoothDir();

            // Calculate Movement
            CalculateMovementDirection();
            CalculateSpeed();
            CalculateFinalMovement();

            // Handle Player Movement, Gravity, Jump, Crouch etc.
            //HandleCrouch();
            HandleHeadBob();
            HandleRunFOV();
            HandleCameraSway();
            HandleLanding();

            ApplyGravity();
            ApplyMovement();

            m_previouslyGrounded = m_isGrounded;
        }
    }

    private void OnEnable() {
        m_playerControls.Enable();
    }

    private void OnDisable() {
        m_playerControls.Disable();
    }

    #endregion

    #region Custom Methods
    #region Initialize Methods    
    protected virtual void GetComponents()
    {
        m_characterController = GetComponent<CharacterController>();
        m_cameraController = GetComponentInChildren<CameraController>();
        m_yawTransform = m_cameraController.transform;
        m_camTransform = GetComponentInChildren<Camera>().transform;
        m_headBob = new HeadBob(headBobData, moveBackwardsSpeedPercent, moveSideSpeedPercent);

    }

    protected virtual void InitVariables()
    {
        // Calculate where our character center should be based on height and skin width
        m_characterController.center = new Vector3(0f, m_characterController.height / 2f + m_characterController.skinWidth, 0f);

        m_initCenter = m_characterController.center;
        m_initHeight = m_characterController.height;

        m_crouchHeight = m_initHeight * crouchPercent;
        m_crouchCenter = (m_crouchHeight / 2f + m_characterController.skinWidth) * Vector3.up;

        m_crouchStandHeightDifference = m_initHeight - m_crouchHeight;

        m_initCamHeight = m_yawTransform.localPosition.y;
        m_crouchCamHeight = m_initCamHeight - m_crouchStandHeightDifference;

        // Sphere radius not included. If you want it to be included just decrease by sphere radius at the end of this equation
        m_finalRayLength = rayLength + m_characterController.center.y;

        m_isGrounded = true;
        m_previouslyGrounded = true;

        m_inAirTimer = 0f;
        m_headBob.CurrentStateHeight = m_initCamHeight;

        m_walkRunSpeedDifference = runSpeed - walkSpeed;
    }
    #endregion

    #region Smoothing Methods
    protected virtual void SmoothInput()
    {
        m_inputVector = GetInputMovementVector().normalized;
        m_smoothInputVector = Vector2.Lerp(m_smoothInputVector, m_inputVector, Time.deltaTime * smoothInputSpeed);
        //Debug.DrawRay(transform.position, new Vector3(m_smoothInputVector.x,0f,m_smoothInputVector.y), Color.green);
    }

    protected virtual void SmoothSpeed()
    {
        m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, Time.deltaTime * smoothVelocitySpeed);

        if (GetInputRunning() && CanRun())
        {
            float _walkRunPercent = Mathf.InverseLerp(walkSpeed, runSpeed, m_smoothCurrentSpeed);
            m_finalSmoothCurrentSpeed = runTransitionCurve.Evaluate(_walkRunPercent) * m_walkRunSpeedDifference + walkSpeed;
        }
        else
        {
            m_finalSmoothCurrentSpeed = m_smoothCurrentSpeed;
        }
    }

    protected virtual void SmoothDir()
    {
        m_smoothFinalMoveDir = Vector3.Lerp(m_smoothFinalMoveDir, m_finalMoveDir, Time.deltaTime * smoothFinalDirectionSpeed);
    }
    #endregion

    #region Locomotion Calculation Methods
    protected virtual void CheckIfGrounded()
    {
        Vector3 _origin = transform.position + m_characterController.center;

        bool _hitGround = Physics.SphereCast(_origin, raySphereRadius, Vector3.down, out m_hitInfo, m_finalRayLength, groundLayer);

        m_isGrounded = _hitGround ? true : false;
    }

    protected virtual void CheckIfWall()
    {

        Vector3 _origin = transform.position + m_characterController.center;
        RaycastHit _wallInfo;

        bool _hitWall = false;

        if (GetInputMovementVector() != Vector2.zero && m_finalMoveDir.sqrMagnitude > 0)
            _hitWall = Physics.SphereCast(_origin, rayObstacleSphereRadius, m_finalMoveDir, out _wallInfo, rayObstacleLength, obstacleLayers);

        m_hitWall = _hitWall ? true : false;
    }

    protected virtual bool CheckIfRoof() /// TO FIX
    {
        Vector3 _origin = transform.position;
        RaycastHit _roofInfo;

        bool _hitRoof = Physics.SphereCast(_origin, raySphereRadius, Vector3.up, out _roofInfo, m_initHeight);

        return _hitRoof;
    }

    protected virtual bool CanRun()
    {
        Vector3 _normalizedDir = Vector3.zero;

        if (m_smoothFinalMoveDir != Vector3.zero)
            _normalizedDir = m_smoothFinalMoveDir.normalized;

        float _dot = Vector3.Dot(transform.forward, _normalizedDir);
        return _dot >= canRunThreshold && !GetInputCrouching() ? true : false;
    }

    protected virtual void CalculateMovementDirection()
    {

        Vector3 _vDir = transform.forward * m_smoothInputVector.y;
        Vector3 _hDir = transform.right * m_smoothInputVector.x;

        Vector3 _desiredDir = _vDir + _hDir;
        Vector3 _flattenDir = FlattenVectorOnSlopes(_desiredDir);

        m_finalMoveDir = _flattenDir;
    }

    protected virtual Vector3 FlattenVectorOnSlopes(Vector3 _vectorToFlat)
    {
        if (m_isGrounded)
            _vectorToFlat = Vector3.ProjectOnPlane(_vectorToFlat, m_hitInfo.normal);

        return _vectorToFlat;
    }

    protected virtual void CalculateSpeed()
    {
        m_currentSpeed = GetInputRunning() && CanRun() ? runSpeed : walkSpeed;
        m_currentSpeed = GetInputCrouching() ? crouchSpeed : m_currentSpeed;
        m_currentSpeed = (GetInputMovementVector() == Vector2.zero) ? 0f : m_currentSpeed;
        m_currentSpeed = GetInputMovementVector().y == -1 ? m_currentSpeed * moveBackwardsSpeedPercent : m_currentSpeed;
        m_currentSpeed = GetInputMovementVector().x != 0 && GetInputMovementVector().y == 0 ? m_currentSpeed * moveSideSpeedPercent : m_currentSpeed;
    }

    protected virtual void CalculateFinalMovement()
    {
        Vector3 _finalVector = m_smoothFinalMoveDir * m_finalSmoothCurrentSpeed;

        // We have to assign individually in order to make our character jump properly because before it was overwriting Y value and that's why it was jerky now we are adding to Y value and it's working
        m_finalMoveVector.x = _finalVector.x;
        m_finalMoveVector.z = _finalVector.z;

        if (m_characterController.isGrounded) // Thanks to this check we are not applying extra y velocity when in air so jump will be consistent
            m_finalMoveVector.y += _finalVector.y; //so this makes our player go in forward dir using slope normal but when jumping this is making it go higher so this is weird
    }
    #endregion

    #region Crouching Methods
    protected virtual void HandleCrouch()
    {
        if (GetInputCrouchingTriggered() && m_isGrounded)
            InvokeCrouchRoutine();
    }

    protected virtual void InvokeCrouchRoutine()
    {
        if (GetInputCrouching())
            if (CheckIfRoof())
                return;

        if (m_LandRoutine != null)
            StopCoroutine(m_LandRoutine);

        if (m_CrouchRoutine != null)
            StopCoroutine(m_CrouchRoutine);

        m_CrouchRoutine = CrouchRoutine();
        StartCoroutine(m_CrouchRoutine);
    }

    protected virtual IEnumerator CrouchRoutine()
    {
        m_duringCrouchAnimation = true;

        float _percent = 0f;
        float _smoothPercent = 0f;
        float _speed = 1f / crouchTransitionDuration;

        float _currentHeight = m_characterController.height;
        Vector3 _currentCenter = m_characterController.center;

        float _desiredHeight = !GetInputCrouching() ? m_initHeight : m_crouchHeight;
        Vector3 _desiredCenter = !GetInputCrouching() ? m_initCenter : m_crouchCenter;

        Vector3 _camPos = m_yawTransform.localPosition;
        float _camCurrentHeight = _camPos.y;
        float _camDesiredHeight = !GetInputCrouching() ? m_initCamHeight : m_crouchCamHeight;

        //GetInputCrouching() = !GetInputCrouching();
        m_headBob.CurrentStateHeight = !GetInputCrouching() ? m_crouchCamHeight : m_initCamHeight;

        while (_percent < 1f)
        {
            _percent += Time.deltaTime * _speed;
            _smoothPercent = crouchTransitionCurve.Evaluate(_percent);

            m_characterController.height = Mathf.Lerp(_currentHeight, _desiredHeight, _smoothPercent);
            m_characterController.center = Vector3.Lerp(_currentCenter, _desiredCenter, _smoothPercent);

            _camPos.y = Mathf.Lerp(_camCurrentHeight, _camDesiredHeight, _smoothPercent);
            m_yawTransform.localPosition = _camPos;

            yield return null;
        }

        m_duringCrouchAnimation = false;
    }

    #endregion

    #region Landing Methods
    protected virtual void HandleLanding()
    {
        if (!m_previouslyGrounded && m_isGrounded)
        {
            InvokeLandingRoutine();
        }
    }

    protected virtual void InvokeLandingRoutine()
    {
        if (m_LandRoutine != null)
            StopCoroutine(m_LandRoutine);

        m_LandRoutine = LandingRoutine();
        StartCoroutine(m_LandRoutine);
    }

    protected virtual IEnumerator LandingRoutine()
    {
        float _percent = 0f;
        float _landAmount = 0f;

        float _speed = 1f / landDuration;

        Vector3 _localPos = m_yawTransform.localPosition;
        float _initLandHeight = _localPos.y;

        _landAmount = m_inAirTimer > landTimer ? highLandAmount : lowLandAmount;

        while (_percent < 1f)
        {
            _percent += Time.deltaTime * _speed;
            float _desiredY = landCurve.Evaluate(_percent) * _landAmount;

            _localPos.y = _initLandHeight + _desiredY;
            m_yawTransform.localPosition = _localPos;

            yield return null;
        }
    }
    #endregion

    #region Locomotion Apply Methods

    protected virtual void HandleHeadBob()
    {

        if (GetInputMovementVector() != Vector2.zero && m_isGrounded && !m_hitWall)
        {
            if (!m_duringCrouchAnimation) // we want to make our head bob only if we are moving and not during crouch routine
            {
                m_headBob.ScrollHeadBob(GetInputRunning() && CanRun(), GetInputCrouching(), GetInputMovementVector());
                m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, (Vector3.up * m_headBob.CurrentStateHeight) + m_headBob.FinalOffset, Time.deltaTime * smoothHeadBobSpeed);
            }
        }
        else // if we are not moving or we are not grounded
        {
            if (!m_headBob.Resetted)
            {
                m_headBob.ResetHeadBob();
            }

            if (!m_duringCrouchAnimation) // we want to reset our head bob only if we are standing still and not during crouch routine
                m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, new Vector3(0f, m_headBob.CurrentStateHeight, 0f), Time.deltaTime * smoothHeadBobSpeed);
        }

        //m_camTransform.localPosition = Vector3.Lerp(m_camTransform.localPosition,m_headBob.FinalOffset,Time.deltaTime * smoothHeadBobSpeed);
    }

    protected virtual void HandleCameraSway()
    {
        m_cameraController.HandleSway(m_smoothInputVector, GetInputMovementVector().x);
    }

    protected virtual void HandleRunFOV()
    {
        if (GetInputMovementVector() != Vector2.zero && m_isGrounded && !m_hitWall)
        {
            if (GetInputRunning() && CanRun())
            {
                m_duringRunAnimation = true;
                m_cameraController.ChangeRunFOV(false);
            }

            /*if (movementInputData.IsRunning && CanRun() && !m_duringRunAnimation)
            {
                m_duringRunAnimation = true;
                m_cameraController.ChangeRunFOV(false);
            }*/
        }

        if (!GetInputRunning() || GetInputMovementVector() == Vector2.zero || m_hitWall)
        {
            if (m_duringRunAnimation)
            {
                m_duringRunAnimation = false;
                m_cameraController.ChangeRunFOV(true);
            }
        }
    }
    protected virtual void HandleJump()
    {
        if (GetInputJump() && !GetInputCrouching())
        {
            //m_finalMoveVector.y += jumpSpeed /* m_currentSpeed */; // we are adding because ex. when we are going on slope we want to keep Y value not overwriting it
            m_finalMoveVector.y = jumpSpeed /* m_currentSpeed */; // turns out that when adding to Y it is too much and it doesn't feel correct because jumping on slope is much faster and higher;

            m_previouslyGrounded = true;
            m_isGrounded = false;
        }
    }
    protected virtual void ApplyGravity()
    {
        if (m_characterController.isGrounded) // if we would use our own m_isGrounded it would not work that good, this one is more precise
        {
            m_inAirTimer = 0f;
            m_finalMoveVector.y = -stickToGroundForce;

            HandleJump();
        }
        else
        {
            m_inAirTimer += Time.deltaTime;
            m_finalMoveVector += Physics.gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    protected virtual void ApplyMovement()
    {
        m_characterController.Move(m_finalMoveVector * Time.deltaTime);
    }

    protected virtual void RotateTowardsCamera()
    {
        Quaternion _currentRot = transform.rotation;
        Quaternion _desiredRot = m_yawTransform.rotation;

        transform.rotation = Quaternion.Slerp(_currentRot, _desiredRot, Time.deltaTime * smoothRotateSpeed);
    }

    protected Vector2 GetInputMovementVector() {
        
        Vector2 val = m_playerControls.Walking.Movement.ReadValue<Vector2>();
        return val;
    }

    protected bool GetInputRunning() {
        return m_playerControls.Walking.Run.ReadValue<float>() == 1;
    }

    protected bool GetInputCrouching() {
        return m_playerControls.Walking.Crouch.ReadValue<float>() == 1;
    }

    protected bool GetInputCrouchingTriggered() {
        return m_playerControls.Walking.Crouch.triggered;
    }

    protected bool GetInputJump() {
        return m_playerControls.Walking.Jump.triggered;
    }
    #endregion
    #endregion
}
