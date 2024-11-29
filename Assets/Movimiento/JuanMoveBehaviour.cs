using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using CamaraTerceraPersona;


#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
#endif

///TODO
// Better implement the new input system.
// create compatibility layers for Unity 2017 and 2018
// better implement animation calls(?)
// more camera animations
namespace SUPERCharacte
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
    [AddComponentMenu("SUPER Character/JuanMove")]
    public class JuanMoveBehaviour : MonoBehaviour
    {
        #region Variables
        public Controller controller;
        private CamaraBahaviour camaraPro;
        //public GameObject posCamera;
        public bool cubriendose;
        public bool atacando;
        public bool tengoFacon;
        public bool tengoRifle;
        public bool tengoBoleadoras;
        public bool controllerPaused = false;
        public Vector3 ultimaPosicion;

        public bool estaMuerto;
        public bool murio;

        #region Boleadoras
        public GameObject boleadoraPrefab;  // Prefab del objeto a lanzar
        public Transform lanzamientoPos;    // Empty en la mano del jugador
        public float fuerzaLanzamiento = 15f;
        #endregion

        #region Movimiento2
        public float speed = 5f;
        public float speedRotate = 200f;
        public float horizontal;
        public float vertical;
        #endregion


        #region Movement
        [Header("Movement Settings")]

        //
        //Public
        //
        public bool enableMovementControl = true;

        //Walking/Sprinting/Crouching
        [Range(1.0f, 650.0f)] public float walkingSpeed = 140, sprintingSpeed = 260, crouchingSpeed = 45;
        [Range(1.0f, 400.0f)] public float decelerationSpeed = 240;
#if ENABLE_INPUT_SYSTEM
    public Key sprintKey = Key.LeftShift, crouchKey = Key.LeftCtrl, slideKey = Key.V;
#else
        public KeyCode sprintKey_L = KeyCode.LeftShift, crouchKey_L = KeyCode.LeftControl, slideKey_L = KeyCode.V;
#endif
        public bool canSprint = true, isSprinting, toggleSprint, sprintOverride, canCrouch = true, isCrouching, toggleCrouch, crouchOverride, isIdle;
        public Stances currentStance = Stances.Standing;
        public float stanceTransitionSpeed = 5.0f, crouchingHeight = 0.80f;
        public GroundSpeedProfiles currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
        public LayerMask whatIsGround = -1;

        //Slope affectors
        public float hardSlopeLimit = 70, slopeInfluenceOnSpeed = 1, maxStairRise = 0.25f, stepUpSpeed = 0.2f;

        //Jumping
        public bool canJump = true, holdJump = false, jumpEnhancements = true, Jumped;
#if ENABLE_INPUT_SYSTEM
        public Key jumpKey = Key.Space;
#else
        public KeyCode jumpKey_L = KeyCode.Space;
#endif
        [Range(1.0f, 650.0f)] public float jumpPower = 40;
        [Range(0.0f, 1.0f)] public float airControlFactor = 1;
        public float decentMultiplier = 2.5f, tapJumpMultiplier = 2.1f;
        float jumpBlankingPeriod;

        //Sliding
        public bool isSliding, canSlide = true;
        public float slidingDeceleration = 150.0f, slidingTransitionSpeed = 4, maxFlatSlideDistance = 10;


        //
        //Internal
        //

        //Walking/Sprinting/Crouching
        public GroundInfo currentGroundInfo = new GroundInfo();
        float standingHeight;
        float currentGroundSpeed;
        public Vector3 InputDir;
        float HeadRotDirForInput;
        Vector2 MovInput;
        Vector2 MovInput_Smoothed;
        public Vector2 _2DVelocity;
        float _2DVelocityMag, speedToVelocityRatio;
        PhysicMaterial _ZeroFriction, _MaxFriction;
        public CapsuleCollider capsule;
        public Rigidbody p_Rigidbody;
        bool crouchInput_Momentary, crouchInput_FrameOf, sprintInput_FrameOf, sprintInput_Momentary, slideInput_FrameOf, slideInput_Momentary;
        public bool changingStances = false;

        //Slope Affectors

        //Jumping
        bool jumpInput_Momentary, jumpInput_FrameOf;

        //Sliding
        Vector3 cachedDirPreSlide, cachedPosPreSlide;



        [Space(20)]
        #endregion

        #region Stamina System
        //Public
        public bool enableStaminaSystem = true, jumpingDepletesStamina = true;
        [Range(0.0f, 250.0f)] public float Stamina = 50.0f, currentStaminaLevel = 0, s_minimumStaminaToSprint = 5.0f, s_depletionSpeed = 2.0f, s_regenerationSpeed = 1.2f, s_JumpStaminaDepletion = 5.0f, s_FacaStaminaDepletion = 2.0f;

        //Internal
        public bool staminaIsChanging;
        bool ignoreStamina = false;
        #endregion

        #region Footstep System
        [Header("Footstep System")]
        public bool enableFootstepSounds = true;
        public FootstepTriggeringMode footstepTriggeringMode = FootstepTriggeringMode.calculatedTiming;
        [Range(0.0f, 1.0f)] public float stepTiming = 0.15f;
        [Range(0.0f, 1.0f)] public float modificadorCorriendo = 0.50f;
        public List<GroundMaterialProfile> footstepSoundSet = new List<GroundMaterialProfile>();
        bool shouldCalculateFootstepTriggers = true;
        public float StepCycle = 0;
        AudioSource playerAudioSource;
        List<AudioClip> currentClipSet = new List<AudioClip>();
        [Space(18)]
        #endregion

        #region  Survival Stats
        //
        //Public
        //
        public bool enableSurvivalStats = true;
        public SurvivalStats defaultSurvivalStats = new SurvivalStats();
        public float statTickRate = 6.0f, hungerDepletionRate = 0.06f, hydrationDepletionRate = 0.14f;
        public SurvivalStats currentSurvivalStats = new SurvivalStats();

        //
        //Internal
        //
        float StatTickTimer;
        #endregion

        #region Collectables
        #endregion

        #region Animation
        //
        //Pulbic
        //

        //Firstperson
        //public Animator _1stPersonCharacterAnimator;
        //ThirdPerson
        public Animator _3rdPersonCharacterAnimator;
        public string a_velocity, a_2DVelocity, a_Grounded, a_Idle, a_Jumped,
            a_Sliding, a_Sprinting, a_Crouching, a_facon, a_faconazo, a_esquivar,
            a_poncho, a_velXZ, a_rifle, a_boleadoras, a_VelX, a_VelY, a_lanzar, a_isDeath;
        public bool stickRendererToCapsuleBottom = true;
        public Vector3 velXZ;

        #endregion

        [Space(18)]
        public bool enableGroundingDebugging = false, enableMovementDebugging = false, enableMouseAndCameraDebugging = false, enableVaultDebugging = false;
        #endregion
        private void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
        }
        void Start()
        {
            camaraPro = GetComponent<CamaraBahaviour>();
            tengoFacon = false;
            tengoRifle = false;
            tengoBoleadoras = false;
            cubriendose = false;
            estaMuerto = false;
            murio = false;


            #region Movement
            p_Rigidbody = GetComponent<Rigidbody>();
            //capsule = GetComponent<CapsuleCollider>();
            standingHeight = capsule.height;
            currentGroundSpeed = walkingSpeed;
            _ZeroFriction = new PhysicMaterial("Zero_Friction");
            _ZeroFriction.dynamicFriction = 0f;
            _ZeroFriction.staticFriction = 0;
            _ZeroFriction.frictionCombine = PhysicMaterialCombine.Minimum;
            _ZeroFriction.bounceCombine = PhysicMaterialCombine.Minimum;
            _MaxFriction = new PhysicMaterial("Max_Friction");
            _MaxFriction.dynamicFriction = 1;
            _MaxFriction.staticFriction = 1;
            _MaxFriction.frictionCombine = PhysicMaterialCombine.Maximum;
            _MaxFriction.bounceCombine = PhysicMaterialCombine.Average;
            #endregion

            #region Stamina System
            currentStaminaLevel = Stamina;
            #endregion

            #region Footstep
            playerAudioSource = GetComponent<AudioSource>();
            #endregion

        }
        void Update()
        {
            if (!estaMuerto)
            {
                if (!controllerPaused)
                {
                    //EquiparFacon();
                    #region Input
                    /*#if ENABLE_INPUT_SYSTEM
                                MouseXY.x = Mouse.current.delta.y.ReadValue()/50;
                                MouseXY.y = Mouse.current.delta.x.ReadValue()/50;

                                mouseScrollWheel = Mouse.current.scroll.y.ReadValue()/1000;
                                if(perspectiveSwitchingKey!=Key.None)perspecTog = Keyboard.current[perspectiveSwitchingKey].wasPressedThisFrame;
                                if(interactKey!=Key.None)interactInput = Keyboard.current[interactKey].wasPressedThisFrame;
                                //movement

                                 if(jumpKey!=Key.None)jumpInput_Momentary =  Keyboard.current[jumpKey].isPressed;
                                 if(jumpKey!=Key.None)jumpInput_FrameOf =  Keyboard.current[jumpKey].wasPressedThisFrame;

                                 if(crouchKey!=Key.None){
                                    crouchInput_Momentary =  Keyboard.current[crouchKey].isPressed;
                                    crouchInput_FrameOf = Keyboard.current[crouchKey].wasPressedThisFrame;
                                 }
                                 if(sprintKey!=Key.None){
                                    sprintInput_Momentary = Keyboard.current[sprintKey].isPressed;
                                    sprintInput_FrameOf = Keyboard.current[sprintKey].wasPressedThisFrame;
                                 }
                                 if(slideKey != Key.None){
                                    slideInput_Momentary = Keyboard.current[slideKey].isPressed;
                                    slideInput_FrameOf = Keyboard.current[slideKey].wasPressedThisFrame;
                                 }
                    #if SAIO_ENABLE_PARKOUR
                                vaultInput = Keyboard.current[VaultKey].isPressed;
                    #endif
                                MovInput.x = Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0;
                                MovInput.y = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;
                    #else */
                    //camera
                    //MouseXY.x = Input.GetAxis("Mouse Y");
                    //MouseXY.y = Input.GetAxis("Mouse X");
                    //mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                    //perspecTog = Input.GetKeyDown(perspectiveSwitchingKey_L);
                    //interactInput = Input.GetKeyDown(interactKey_L);
                    //movement

                    jumpInput_Momentary = Input.GetKey(jumpKey_L);
                    jumpInput_FrameOf = Input.GetKeyDown(jumpKey_L);
                    crouchInput_Momentary = Input.GetKey(crouchKey_L);
                    crouchInput_FrameOf = Input.GetKeyDown(crouchKey_L);
                    sprintInput_Momentary = Input.GetKey(sprintKey_L);
                    sprintInput_FrameOf = Input.GetKeyDown(sprintKey_L);
                    slideInput_Momentary = Input.GetKey(slideKey_L);
                    slideInput_FrameOf = Input.GetKeyDown(slideKey_L);
#if SAIO_ENABLE_PARKOUR

            vaultInput = Input.GetKeyDown(VaultKey_L);
#endif
                    MovInput = Vector2.up * Input.GetAxisRaw("Vertical") + Vector2.right * Input.GetAxisRaw("Horizontal");
                    //#endif
                    #endregion

                    if (!tengoBoleadoras)
                    {
                        
                        //if(Input.GetKeyDown(KeyCode.Mouse0))
                        if (!atacando && !cubriendose)
                        {
                            #region Movement
                            if (camaraPro.cameraPerspective == PerspectiveModes._3rdPerson && !atacando)
                            {
                                HeadRotDirForInput = Mathf.MoveTowardsAngle(HeadRotDirForInput, camaraPro.headRot.y, camaraPro.bodyCatchupSpeed * (1 + Time.deltaTime));
                                MovInput_Smoothed = Vector2.MoveTowards(MovInput_Smoothed, MovInput, camaraPro.inputResponseFiltering * (1 + Time.deltaTime));
                            }
                            InputDir = camaraPro.cameraPerspective == PerspectiveModes._1stPerson ? Vector3.ClampMagnitude((transform.forward * MovInput.y + transform.right * (camaraPro.viewInputMethods == ViewInputModes.Traditional ? MovInput.x : 0)), 1) : Quaternion.AngleAxis(HeadRotDirForInput, Vector3.up) * (Vector3.ClampMagnitude((Vector3.forward * MovInput_Smoothed.y + Vector3.right * MovInput_Smoothed.x), 1));
                            GroundMovementSpeedUpdate();
                            if (canJump && !tengoFacon && !tengoRifle && !tengoBoleadoras && (holdJump ? jumpInput_Momentary : jumpInput_FrameOf)) { Jump(jumpPower); }
                            #endregion
                            #region Footstep
                            CalculateFootstepTriggers();
                            #endregion
                        }
                    }



                    #region Stamina system
                    if (enableStaminaSystem) { CalculateStamina(); }
                    #endregion

                    #region Survival Stats
                    if (enableSurvivalStats && Time.time > StatTickTimer)
                    {
                        TickStats();
                    }
                    #endregion

                }
                else
                {
                    jumpInput_FrameOf = false;
                    jumpInput_Momentary = false;
                }
            }
            
            #region Animation
            UpdateAnimationTriggers(controllerPaused);
            #endregion
        }
        void FixedUpdate()
        {
            if (!controllerPaused && !estaMuerto)
            {
                if (!tengoBoleadoras)
                {
                    if (!atacando && !cubriendose)
                    {
                        #region Movement
                        if (enableMovementControl)
                        {
                            GetGroundInfo();
                            MovePlayer(InputDir, currentGroundSpeed);
                            velXZ = new Vector3(p_Rigidbody.velocity.x, 0, p_Rigidbody.velocity.z);

                            //Debug.Log(velXZ.magnitude);

                            // if (isSliding) { Slide(); }
                        }
                        #endregion

                    }

                }
                else
                {
                    MovePlayer();
                    //transform.forward = Camera.main.transform.forward;
                }

            }
        }

        void DejaDeGolpear()
        {
            atacando = false;
        }

        void EquiparFacon()
        {

        }
        #region Movement2
        public void MovePlayer()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            transform.Translate(new Vector3(horizontal, 0.0f, vertical) * Time.deltaTime * speed);

            float rotationY = (Input.GetAxis("Mouse X"));
            transform.Rotate(new Vector3(0, rotationY, 0) * Time.deltaTime * speedRotate);
            

        }
        #endregion

        /*   private void OnTriggerEnter(Collider other)
           {
               #region Collectables
               other.GetComponent<ICollectable>()?.Collect();
               #endregion
           }*/

        

        #region Movement Functions
        void MovePlayer(Vector3 Direction, float Speed)
        {
            // GroundInfo gI = GetGroundInfo();
            isIdle = Direction.normalized.magnitude <= 0;
            _2DVelocity = Vector2.right * p_Rigidbody.velocity.x + Vector2.up * p_Rigidbody.velocity.z;
            speedToVelocityRatio = (Mathf.Lerp(0, 2, Mathf.InverseLerp(0, (sprintingSpeed / 50), _2DVelocity.magnitude)));
            _2DVelocityMag = Mathf.Clamp((walkingSpeed / 50) / _2DVelocity.magnitude, 0f, 2f);


            //Movement
            if ((currentGroundInfo.isGettingGroundInfo) && !Jumped && !isSliding && !atacando /*&& !doingPosInterp*/)
            {
                //Deceleration
                if (Direction.magnitude == 0 && p_Rigidbody.velocity.normalized.magnitude > 0.1f && !atacando)
                {
                    p_Rigidbody.AddForce(-new Vector3(p_Rigidbody.velocity.x, currentGroundInfo.isInContactWithGround ? p_Rigidbody.velocity.y - Physics.gravity.y : 0, p_Rigidbody.velocity.z) * (decelerationSpeed * Time.fixedDeltaTime), ForceMode.Force);
                }
                //normal speed
                else if ((currentGroundInfo.isGettingGroundInfo) && currentGroundInfo.groundAngle < hardSlopeLimit && currentGroundInfo.groundAngle_Raw < hardSlopeLimit && !atacando)
                {
                    p_Rigidbody.velocity = (Vector3.MoveTowards(p_Rigidbody.velocity, Vector3.ClampMagnitude(((Direction) * ((Speed) * Time.fixedDeltaTime)) + (Vector3.down), Speed / 50), 1));
                }
                capsule.sharedMaterial = InputDir.magnitude > 0 ? _ZeroFriction : _MaxFriction;
            }
            //Sliding
            else if (isSliding)
            {
                p_Rigidbody.AddForce(-(p_Rigidbody.velocity - Physics.gravity) * (slidingDeceleration * Time.fixedDeltaTime), ForceMode.Force);
            }

            //Air Control
            else if (!currentGroundInfo.isGettingGroundInfo)
            {
                p_Rigidbody.AddForce((((Direction * (walkingSpeed)) * Time.fixedDeltaTime) * airControlFactor * 5) * currentGroundInfo.groundAngleMultiplier_Inverse_persistent, ForceMode.Acceleration);
                p_Rigidbody.velocity = Vector3.ClampMagnitude((Vector3.right * p_Rigidbody.velocity.x + Vector3.forward * p_Rigidbody.velocity.z), (walkingSpeed / 50)) + (Vector3.up * p_Rigidbody.velocity.y);
                if (!currentGroundInfo.potentialStair && jumpEnhancements)
                {
                    if (p_Rigidbody.velocity.y < 0 && p_Rigidbody.velocity.y > Physics.gravity.y * 1.5f)
                    {
                        p_Rigidbody.velocity += Vector3.up * (Physics.gravity.y * (decentMultiplier) * Time.fixedDeltaTime);
                    }
                    else if (p_Rigidbody.velocity.y > 0 && !jumpInput_Momentary)
                    {
                        p_Rigidbody.velocity += Vector3.up * (Physics.gravity.y * (tapJumpMultiplier - 1) * Time.fixedDeltaTime);
                    }
                }
            }


        }
        void Jump(float Force)
        {
            if ((currentGroundInfo.isInContactWithGround) &&
                (currentGroundInfo.groundAngle < hardSlopeLimit) &&
                ((enableStaminaSystem && jumpingDepletesStamina) ? currentStaminaLevel > s_JumpStaminaDepletion * 1.2f : true) &&
                (Time.time > (jumpBlankingPeriod + 0.1f)) &&
                (currentStance == Stances.Standing && !Jumped))
            {

                Jumped = true;
                p_Rigidbody.velocity = (Vector3.right * p_Rigidbody.velocity.x) + (Vector3.forward * p_Rigidbody.velocity.z);
                p_Rigidbody.AddForce(Vector3.up * (Force / 10), ForceMode.Impulse);
                if (enableStaminaSystem && jumpingDepletesStamina)
                {
                    InstantStaminaReduction(s_JumpStaminaDepletion);
                }
                capsule.sharedMaterial = _ZeroFriction;
                jumpBlankingPeriod = Time.time;
            }
        }
        public void DoJump(float Force = 10.0f)
        {
            if (
                (Time.time > (jumpBlankingPeriod + 0.1f)) &&
                (currentStance == Stances.Standing))
            {
                Jumped = true;
                p_Rigidbody.velocity = (Vector3.right * p_Rigidbody.velocity.x) + (Vector3.forward * p_Rigidbody.velocity.z);
                p_Rigidbody.AddForce(Vector3.up * (Force / 10), ForceMode.Impulse);
                if (enableStaminaSystem && jumpingDepletesStamina)
                {
                    InstantStaminaReduction(s_JumpStaminaDepletion);
                }
                capsule.sharedMaterial = _ZeroFriction;
                jumpBlankingPeriod = Time.time;
            }
        }
     /*   void Slide()
        {
            if (!isSliding)
            {
                if (currentGroundInfo.isInContactWithGround)
                {
                    //do debug print
                    if (enableMovementDebugging) { print("Starting Slide."); }
                    p_Rigidbody.AddForce((transform.forward * ((sprintingSpeed)) + (Vector3.up * currentGroundInfo.groundInfluenceDirection.y)), ForceMode.Force);
                    cachedDirPreSlide = transform.forward;
                    cachedPosPreSlide = transform.position;
                    capsule.sharedMaterial = _ZeroFriction;
                    StartCoroutine(ApplyStance(slidingTransitionSpeed, Stances.Crouching));
                    isSliding = true;
                }
            }
            else if (slideInput_Momentary)
            {
                if (enableMovementDebugging) { print("Continuing Slide."); }
                if (Vector3.Distance(transform.position, cachedPosPreSlide) < maxFlatSlideDistance) { p_Rigidbody.AddForce(cachedDirPreSlide * (sprintingSpeed / 50), ForceMode.Force); }
                if (p_Rigidbody.velocity.magnitude > sprintingSpeed / 50) { p_Rigidbody.velocity = p_Rigidbody.velocity.normalized * (sprintingSpeed / 50); }
                else if (p_Rigidbody.velocity.magnitude < (crouchingSpeed / 25))
                {
                    if (enableMovementDebugging) { print("Slide too slow, ending slide into crouch."); }
                    //capsule.sharedMaterial = _MaxFrix;
                    isSliding = false;
                    isSprinting = false;
                    StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Crouching));
                    currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                }
            }
            else
            {
                if (OverheadCheck())
                {
                    if (p_Rigidbody.velocity.magnitude > (walkingSpeed / 50))
                    {
                        if (enableMovementDebugging) { print("Key realeased, ending slide into a sprint."); }
                        isSliding = false;
                        StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                        currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                    }
                    else
                    {
                        if (enableMovementDebugging) { print("Key realeased, ending slide into a walk."); }
                        isSliding = false;
                        StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                        currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                    }
                }
                else
                {
                    if (enableMovementDebugging) { print("Key realeased but there is an obstruction. Ending slide into crouch."); }
                    isSliding = false;
                    isSprinting = false;
                    StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Crouching));
                    currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                }

            }
        } */
        void GetGroundInfo()
        {
            //to Get if we're actually touching ground.
            //to act as a normal and point buffer.
            currentGroundInfo.groundFromSweep = null;

            currentGroundInfo.groundFromSweep = Physics.SphereCastAll(transform.position, capsule.radius - 0.001f, Vector3.down, ((capsule.height / 2)) - (capsule.radius / 2), whatIsGround);
            currentGroundInfo.isInContactWithGround = Physics.Raycast(transform.position, Vector3.down, out currentGroundInfo.groundFromRay, (capsule.height / 2) + 0.25f, whatIsGround);
            Debug.DrawRay(transform.position, Vector3.down, Color.red, (capsule.height / 2) + 0.25f);

            if (Jumped && (Physics.Raycast(transform.position, Vector3.down, (capsule.height / 2) + 0.1f, whatIsGround) || Physics.CheckSphere(transform.position - (Vector3.up * ((capsule.height / 2) - (capsule.radius - 0.05f))), capsule.radius, whatIsGround)) && Time.time > (jumpBlankingPeriod + 0.1f))
            {
                Jumped = false;
            }

            //if(Result.isGrounded){
            if (currentGroundInfo.groundFromSweep != null && currentGroundInfo.groundFromSweep.Length != 0)
            {
                currentGroundInfo.isGettingGroundInfo = true;
                currentGroundInfo.groundNormals_lowgrade.Clear();
                currentGroundInfo.groundNormals_highgrade.Clear();
                foreach (RaycastHit hit in currentGroundInfo.groundFromSweep)
                {
                    if (hit.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Angle(hit.normal, Vector3.up) < hardSlopeLimit)
                    {
                        currentGroundInfo.groundNormals_lowgrade.Add(hit.normal);
                    }
                    else
                    {
                        currentGroundInfo.groundNormals_highgrade.Add(hit.normal);
                    }
                }
                if (currentGroundInfo.groundNormals_lowgrade.Any())
                {
                    currentGroundInfo.groundNormal_Averaged = Average(currentGroundInfo.groundNormals_lowgrade);
                }
                else
                {
                    currentGroundInfo.groundNormal_Averaged = Average(currentGroundInfo.groundNormals_highgrade);
                }
                currentGroundInfo.groundNormal_Raw = currentGroundInfo.groundFromRay.normal;
                currentGroundInfo.groundRawYPosition = currentGroundInfo.groundFromSweep.Average(x => (x.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Angle(x.normal, Vector3.up) < hardSlopeLimit) ? x.point.y : currentGroundInfo.groundFromRay.point.y); //Mathf.MoveTowards(currentGroundInfo.groundRawYPosition, currentGroundInfo.groundFromSweep.Average(x=> (x.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Dot(x.normal,Vector3.up)<-0.25f) ? x.point.y :  currentGroundInfo.groundFromRay.point.y),Time.deltaTime*2);

            }
            else
            {
                currentGroundInfo.isGettingGroundInfo = false;
                currentGroundInfo.groundNormal_Averaged = currentGroundInfo.groundFromRay.normal;
                currentGroundInfo.groundNormal_Raw = currentGroundInfo.groundFromRay.normal;
                currentGroundInfo.groundRawYPosition = currentGroundInfo.groundFromRay.point.y;
            }

            if (currentGroundInfo.isGettingGroundInfo) { currentGroundInfo.groundAngleMultiplier_Inverse_persistent = currentGroundInfo.groundAngleMultiplier_Inverse; }
            //{
            currentGroundInfo.groundInfluenceDirection = Vector3.MoveTowards(currentGroundInfo.groundInfluenceDirection, Vector3.Cross(currentGroundInfo.groundNormal_Averaged, Vector3.Cross(currentGroundInfo.groundNormal_Averaged, Vector3.up)).normalized, 2 * Time.fixedDeltaTime);
            currentGroundInfo.groundInfluenceDirection.y = 0;
            currentGroundInfo.groundAngle = Vector3.Angle(currentGroundInfo.groundNormal_Averaged, Vector3.up);
            currentGroundInfo.groundAngle_Raw = Vector3.Angle(currentGroundInfo.groundNormal_Raw, Vector3.up);
            currentGroundInfo.groundAngleMultiplier_Inverse = ((currentGroundInfo.groundAngle - 90) * -1) / 90;
            currentGroundInfo.groundAngleMultiplier = ((currentGroundInfo.groundAngle)) / 90;
            //
            currentGroundInfo.groundTag = currentGroundInfo.isInContactWithGround ? currentGroundInfo.groundFromRay.transform.tag : string.Empty;
            if (Physics.Raycast(transform.position + (Vector3.down * ((capsule.height * 0.5f) - 0.1f)), InputDir, out currentGroundInfo.stairCheck_RiserCheck, capsule.radius + 0.1f, whatIsGround))
            {
                if (Physics.Raycast(currentGroundInfo.stairCheck_RiserCheck.point + (currentGroundInfo.stairCheck_RiserCheck.normal * -0.05f) + Vector3.up, Vector3.down, out currentGroundInfo.stairCheck_HeightCheck, 1.1f))
                {
                    if (!Physics.Raycast(transform.position + (Vector3.down * ((capsule.height * 0.5f) - maxStairRise)) + InputDir * (capsule.radius - 0.05f), InputDir, 0.2f, whatIsGround))
                    {
                        if (!isIdle && currentGroundInfo.stairCheck_HeightCheck.point.y > (currentGroundInfo.stairCheck_RiserCheck.point.y + 0.025f) /* Vector3.Angle(currentGroundInfo.groundFromRay.normal, Vector3.up)<5 */ && Vector3.Angle(currentGroundInfo.groundNormal_Averaged, currentGroundInfo.stairCheck_RiserCheck.normal) > 0.5f)
                        {
                            p_Rigidbody.position -= Vector3.up * -0.1f;
                            currentGroundInfo.potentialStair = true;
                        }
                    }
                    else { currentGroundInfo.potentialStair = false; }
                }
            }
            else { currentGroundInfo.potentialStair = false; }


            currentGroundInfo.playerGroundPosition = Mathf.MoveTowards(currentGroundInfo.playerGroundPosition, currentGroundInfo.groundRawYPosition + (capsule.height / 2) + 0.01f, 0.05f);
            //}

            if (currentGroundInfo.isInContactWithGround && enableFootstepSounds && shouldCalculateFootstepTriggers)
            {
                if (currentGroundInfo.groundFromRay.collider is TerrainCollider)
                {
                    currentGroundInfo.groundMaterial = null;
                    currentGroundInfo.groundPhysicMaterial = currentGroundInfo.groundFromRay.collider.sharedMaterial;
                    currentGroundInfo.currentTerrain = currentGroundInfo.groundFromRay.transform.GetComponent<Terrain>();
                    if (currentGroundInfo.currentTerrain)
                    {
                        Vector2 XZ = (Vector2.right * (((transform.position.x - currentGroundInfo.currentTerrain.transform.position.x) / currentGroundInfo.currentTerrain.terrainData.size.x)) * currentGroundInfo.currentTerrain.terrainData.alphamapWidth) + (Vector2.up * (((transform.position.z - currentGroundInfo.currentTerrain.transform.position.z) / currentGroundInfo.currentTerrain.terrainData.size.z)) * currentGroundInfo.currentTerrain.terrainData.alphamapHeight);
                        float[,,] aMap = currentGroundInfo.currentTerrain.terrainData.GetAlphamaps((int)XZ.x, (int)XZ.y, 1, 1);
                        for (int i = 0; i < aMap.Length; i++)
                        {
                            if (aMap[0, 0, i] == 1)
                            {
                                currentGroundInfo.groundLayer = currentGroundInfo.currentTerrain.terrainData.terrainLayers[i];
                                break;
                            }
                        }
                    }
                    else { currentGroundInfo.groundLayer = null; }
                }
                else
                {
                    currentGroundInfo.groundLayer = null;
                    currentGroundInfo.groundPhysicMaterial = currentGroundInfo.groundFromRay.collider.sharedMaterial;
                    currentGroundInfo.currentMesh = currentGroundInfo.groundFromRay.transform.GetComponent<MeshFilter>().sharedMesh;
                    if (currentGroundInfo.currentMesh && currentGroundInfo.currentMesh.isReadable)
                    {
                        int limit = currentGroundInfo.groundFromRay.triangleIndex * 3, submesh;
                        for (submesh = 0; submesh < currentGroundInfo.currentMesh.subMeshCount; submesh++)
                        {
                            int indices = currentGroundInfo.currentMesh.GetTriangles(submesh).Length;
                            if (indices > limit) { break; }
                            limit -= indices;
                        }
                        currentGroundInfo.groundMaterial = currentGroundInfo.groundFromRay.transform.GetComponent<Renderer>().sharedMaterials[submesh];
                    }
                    else { currentGroundInfo.groundMaterial = currentGroundInfo.groundFromRay.collider.GetComponent<MeshRenderer>().sharedMaterial; }
                }
            }
            else { currentGroundInfo.groundMaterial = null; currentGroundInfo.groundLayer = null; currentGroundInfo.groundPhysicMaterial = null; }
#if UNITY_EDITOR
            if (enableGroundingDebugging)
            {
                print("Grounded: " + currentGroundInfo.isInContactWithGround + ", Ground Hits: " + currentGroundInfo.groundFromSweep.Length + ", Ground Angle: " + currentGroundInfo.groundAngle.ToString("0.00") + ", Ground Multi: " + currentGroundInfo.groundAngleMultiplier.ToString("0.00") + ", Ground Multi Inverse: " + currentGroundInfo.groundAngleMultiplier_Inverse.ToString("0.00"));
                print("Ground mesh readable for dynamic foot steps: " + currentGroundInfo.currentMesh?.isReadable);
                Debug.DrawRay(transform.position, Vector3.down * ((capsule.height / 2) + 0.1f), Color.green);
                Debug.DrawRay(transform.position, currentGroundInfo.groundInfluenceDirection, Color.magenta);
                Debug.DrawRay(transform.position + (Vector3.down * ((capsule.height * 0.5f) - 0.05f)) + InputDir * (capsule.radius - 0.05f), InputDir * (capsule.radius + 0.1f), Color.cyan);
                Debug.DrawRay(transform.position + (Vector3.down * ((capsule.height * 0.5f) - 0.5f)) + InputDir * (capsule.radius - 0.05f), InputDir * (capsule.radius + 0.3f), new Color(0, .2f, 1, 1));
            }
#endif
        }
        void GroundMovementSpeedUpdate()
        {
#if SAIO_ENABLE_PARKOUR
        if(!isVaulting)
#endif
            {
                switch (currentGroundMovementSpeed)
                {
                    case GroundSpeedProfiles.Walking:
                        {
                            if (isCrouching || isSprinting)
                            {
                                isSprinting = false;
                                isCrouching = false;
                                currentGroundSpeed = walkingSpeed;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                            }
#if SAIO_ENABLE_PARKOUR
                    if(vaultInput && canVault){VaultCheck();}
#endif
                            //check for state change call
                            if ((canCrouch && crouchInput_FrameOf) || crouchOverride)
                            {
                                isCrouching = true;
                                isSprinting = false;
                                currentGroundSpeed = crouchingSpeed;
                                currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Crouching));
                                break;
                            }
                            else if ((canSprint && sprintInput_FrameOf && ((enableStaminaSystem && jumpingDepletesStamina) ? currentStaminaLevel > s_minimumStaminaToSprint : true)) /* && (enableSurvivalStats ? (!currentSurvivalStats.isDehydrated && !currentSurvivalStats.isStarving) : true)) */ || sprintOverride)
                            {
                                isCrouching = false;
                                isSprinting = true;
                                currentGroundSpeed = sprintingSpeed;
                                currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                            }
                            break;
                        }

                    case GroundSpeedProfiles.Crouching:
                        {
                            if (!isCrouching)
                            {
                                isCrouching = true;
                                isSprinting = false;
                                currentGroundSpeed = crouchingSpeed;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Crouching));
                            }


                            //check for state change call
                            if ((toggleCrouch ? crouchInput_FrameOf : !crouchInput_Momentary) && !crouchOverride && OverheadCheck())
                            {
                                isCrouching = false;
                                isSprinting = false;
                                currentGroundSpeed = walkingSpeed;
                                currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                                break;
                            }
                            else if (((canSprint && sprintInput_FrameOf && ((enableStaminaSystem && jumpingDepletesStamina) ? currentStaminaLevel > s_minimumStaminaToSprint : true) /*&& (enableSurvivalStats ? (!currentSurvivalStats.isDehydrated && !currentSurvivalStats.isStarving) : true)*/) || sprintOverride) && OverheadCheck())
                            {
                                isCrouching = false;
                                isSprinting = true;
                                currentGroundSpeed = sprintingSpeed;
                                currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                                StopCoroutine("ApplyStance");
                                StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                            }
                            break;
                        }

                    case GroundSpeedProfiles.Sprinting:
                        {
                            //if(!isIdle)
                            {
                                if (!isSprinting)
                                {
                                    isCrouching = false;
                                    isSprinting = true;
                                    currentGroundSpeed = sprintingSpeed;
                                    StopCoroutine("ApplyStance");
                                    StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                                }
#if SAIO_ENABLE_PARKOUR
                        if((vaultInput || autoVaultWhenSpringing) && canVault){VaultCheck();}
#endif
                                //check for state change call
                                if (canSlide && !isIdle && slideInput_FrameOf && currentGroundInfo.isInContactWithGround)
                                {
                                    //Slide();
                                    currentGroundMovementSpeed = GroundSpeedProfiles.Sliding;
                                    break;
                                }


                                else if ((canCrouch && crouchInput_FrameOf) || crouchOverride)
                                {
                                    isCrouching = true;
                                    isSprinting = false;
                                    currentGroundSpeed = crouchingSpeed;
                                    currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                                    StopCoroutine("ApplyStance");
                                    StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Crouching));
                                    break;
                                    //Can't leave sprint in toggle sprint.
                                }
                                else if ((toggleSprint ? sprintInput_FrameOf : !sprintInput_Momentary) && !sprintOverride)
                                {
                                    isCrouching = false;
                                    isSprinting = false;
                                    currentGroundSpeed = walkingSpeed;
                                    currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                                    StopCoroutine("ApplyStance");
                                    StartCoroutine(ApplyStance(stanceTransitionSpeed, Stances.Standing));
                                }
                                break;
                            }
                        }
                    case GroundSpeedProfiles.Sliding:
                        {
                        }
                        break;
                }
            }
        }
        IEnumerator ApplyStance(float smoothSpeed, Stances newStance)
        {
            currentStance = newStance;
            float targetCapsuleHeight = currentStance == Stances.Standing ? standingHeight : crouchingHeight;
            float targetEyeHeight = currentStance == Stances.Standing ? camaraPro.standingEyeHeight : camaraPro.crouchingEyeHeight;
            while (!Mathf.Approximately(capsule.height, targetCapsuleHeight))
            {
                changingStances = true;
                capsule.height = (smoothSpeed > 0 ? Mathf.MoveTowards(capsule.height, targetCapsuleHeight, stanceTransitionSpeed * Time.fixedDeltaTime) : targetCapsuleHeight);
                camaraPro.internalEyeHeight = (smoothSpeed > 0 ? Mathf.MoveTowards(camaraPro.internalEyeHeight, targetEyeHeight, stanceTransitionSpeed * Time.fixedDeltaTime) : targetCapsuleHeight);

                if (currentStance == Stances.Crouching && currentGroundInfo.isGettingGroundInfo)
                {
                    p_Rigidbody.velocity = p_Rigidbody.velocity + (Vector3.down * 2);
                    if (enableMovementDebugging) { print("Applying Stance and applying down force "); }
                }
                yield return new WaitForFixedUpdate();
            }
            changingStances = false;
            yield return null;
        }
        bool OverheadCheck()
        {    //Returns true when there is no obstruction.
            bool result = false;
            if (Physics.Raycast(transform.position, Vector3.up, standingHeight - (capsule.height / 2), whatIsGround)) { result = true; }
            return !result;
        }
        Vector3 Average(List<Vector3> vectors)
        {
            Vector3 returnVal = default(Vector3);
            vectors.ForEach(x => { returnVal += x; });
            returnVal /= vectors.Count();
            return returnVal;
        }

        #endregion

        #region Stamina System
        private void CalculateStamina()
        {
            if (isSprinting && !ignoreStamina && !isIdle)
            {
                if (currentStaminaLevel != 0)
                {
                    currentStaminaLevel = Mathf.MoveTowards(currentStaminaLevel, 0, s_depletionSpeed * Time.deltaTime);
                }
                else if (!isSliding) { currentGroundMovementSpeed = GroundSpeedProfiles.Walking; }
                staminaIsChanging = true;
            }
            else if (currentStaminaLevel != Stamina && !ignoreStamina /*&& (enableSurvivalStats ? (!currentSurvivalStats.isDehydrated && !currentSurvivalStats.isStarving) : true)*/)
            {
                currentStaminaLevel = Mathf.MoveTowards(currentStaminaLevel, Stamina, s_regenerationSpeed * Time.deltaTime);
                staminaIsChanging = true;
            }
            else
            {
                staminaIsChanging = false;
            }
        }
        public void InstantStaminaReduction(float Reduction)
        {
            if (!ignoreStamina && enableStaminaSystem) { currentStaminaLevel = Mathf.Clamp(currentStaminaLevel -= Reduction, 0, Stamina); }
        }
        #endregion

        #region Footstep System
        void CalculateFootstepTriggers()
        {
            if (enableFootstepSounds && footstepTriggeringMode == FootstepTriggeringMode.calculatedTiming && shouldCalculateFootstepTriggers)
            {
                if (_2DVelocity.magnitude > (currentGroundSpeed / 100) && !isIdle)
                {
                    if (camaraPro.cameraPerspective == PerspectiveModes._1stPerson)
                    {
                        /*if ((enableHeadbob ? headbobCyclePosition : Time.time) > StepCycle && currentGroundInfo.isGettingGroundInfo && !isSliding)
                        {
                            //print("Steped");
                            CallFootstepClip();
                            StepCycle = enableHeadbob ? (headbobCyclePosition + 0.5f) : (Time.time + ((stepTiming * _2DVelocityMag) * 2));
                        }*/
                    }
                    else
                    {
                        if (Time.time > StepCycle && currentGroundInfo.isGettingGroundInfo && !isSliding)
                        {
                            //print("Steped");
                            CallFootstepClip();
                            //StepCycle = (Time.time+((stepTiming*_2DVelocityMag)*2));


                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                StepCycle = (Time.time + ((modificadorCorriendo * _2DVelocityMag) * 2));
                            }
                            else
                            {
                                StepCycle = (Time.time + ((stepTiming * _2DVelocityMag) * 2));
                            }
                        }
                    }
                }
            }
        }
        public void CallFootstepClip()
        {
            if (playerAudioSource)
            {
                if (enableFootstepSounds && footstepSoundSet.Any())
                {
                    for (int i = 0; i < footstepSoundSet.Count(); i++)
                    {

                        if (footstepSoundSet[i].profileTriggerType == MatProfileType.Material)
                        {
                            if (footstepSoundSet[i]._Materials.Contains(currentGroundInfo.groundMaterial))
                            {
                                currentClipSet = footstepSoundSet[i].footstepClips;
                                break;
                            }
                            else if (i == footstepSoundSet.Count - 1)
                            {
                                currentClipSet = null;
                            }
                        }

                        else if (footstepSoundSet[i].profileTriggerType == MatProfileType.physicMaterial)
                        {
                            if (footstepSoundSet[i]._physicMaterials.Contains(currentGroundInfo.groundPhysicMaterial))
                            {
                                currentClipSet = footstepSoundSet[i].footstepClips;
                                break;
                            }
                            else if (i == footstepSoundSet.Count - 1)
                            {
                                currentClipSet = null;
                            }
                        }

                        else if (footstepSoundSet[i].profileTriggerType == MatProfileType.terrainLayer)
                        {
                            if (footstepSoundSet[i]._Layers.Contains(currentGroundInfo.groundLayer))
                            {
                                currentClipSet = footstepSoundSet[i].footstepClips;
                                break;
                            }
                            else if (i == footstepSoundSet.Count - 1)
                            {
                                currentClipSet = null;
                            }
                        }
                    }

                    if (currentClipSet != null && currentClipSet.Any())
                    {
                        playerAudioSource.PlayOneShot(currentClipSet[Random.Range(0, currentClipSet.Count())]);
                    }
                }
            }
        }
        #endregion

        #region Survival Stat Functions
        public void TickStats()
        {
            if (currentSurvivalStats.Hunger > 0)
            {
                currentSurvivalStats.Hunger = Mathf.Clamp(currentSurvivalStats.Hunger - (hungerDepletionRate + (isSprinting && !isIdle ? 0.1f : 0)), 0, defaultSurvivalStats.Hunger);
                currentSurvivalStats.isStarving = (currentSurvivalStats.Hunger < (defaultSurvivalStats.Hunger / 10));
            }
            if (currentSurvivalStats.Hydration > 0)
            {
                currentSurvivalStats.Hydration = Mathf.Clamp(currentSurvivalStats.Hydration - (hydrationDepletionRate + (isSprinting && !isIdle ? 0.1f : 0)), 0, defaultSurvivalStats.Hydration);
                currentSurvivalStats.isDehydrated = (currentSurvivalStats.Hydration < (defaultSurvivalStats.Hydration / 8));
            }
            currentSurvivalStats.hasLowHealth = (currentSurvivalStats.Health < (defaultSurvivalStats.Health / 10));

            StatTickTimer = Time.time + (60 / statTickRate);
        }
        public void ImmediateStateChange(float Amount, StatSelector Stat = StatSelector.Health)
        {
            switch (Stat)
            {
                case StatSelector.Health:
                    {
                        currentSurvivalStats.Health = Mathf.Clamp(currentSurvivalStats.Health + Amount, 0, defaultSurvivalStats.Health);
                        currentSurvivalStats.hasLowHealth = (currentSurvivalStats.Health < (defaultSurvivalStats.Health / 10));

                    }
                    break;

                case StatSelector.Hunger:
                    {
                        currentSurvivalStats.Hunger = Mathf.Clamp(currentSurvivalStats.Hunger + Amount, 0, defaultSurvivalStats.Hunger);
                        currentSurvivalStats.isStarving = (currentSurvivalStats.Hunger < (defaultSurvivalStats.Hunger / 10));
                    }
                    break;

                case StatSelector.Hydration:
                    {
                        currentSurvivalStats.Hydration = Mathf.Clamp(currentSurvivalStats.Hydration + Amount, 0, defaultSurvivalStats.Hydration);
                        currentSurvivalStats.isDehydrated = (currentSurvivalStats.Hydration < (defaultSurvivalStats.Hydration / 8));
                    }
                    break;
            }
        }
        public void LevelUpStat(float newMaxStatLevel, StatSelector Stat = StatSelector.Health, bool Refill = true)
        {
            switch (Stat)
            {
                case StatSelector.Health:
                    {
                        defaultSurvivalStats.Health = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); ;
                        if (Refill) { currentSurvivalStats.Health = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); }
                        currentSurvivalStats.hasLowHealth = (currentSurvivalStats.Health < (defaultSurvivalStats.Health / 10));

                    }
                    break;
                case StatSelector.Hunger:
                    {
                        defaultSurvivalStats.Hunger = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); ;
                        if (Refill) { currentSurvivalStats.Hunger = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); }
                        currentSurvivalStats.isStarving = (currentSurvivalStats.Hunger < (defaultSurvivalStats.Hunger / 10));

                    }
                    break;
                case StatSelector.Hydration:
                    {
                        defaultSurvivalStats.Hydration = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); ;
                        if (Refill) { currentSurvivalStats.Hydration = Mathf.Clamp(newMaxStatLevel, 0, newMaxStatLevel); }
                        currentSurvivalStats.isDehydrated = (currentSurvivalStats.Hydration < (defaultSurvivalStats.Hydration / 8));

                    }
                    break;
            }
        }

        #endregion

        #region Animator Update
        void UpdateAnimationTriggers(bool zeroOut = false)
        {
            switch (camaraPro.cameraPerspective)
            {
                case PerspectiveModes._1stPerson:
                    {
                        /*if (_1stPersonCharacterAnimator)
                        {
                            //Setup Fistperson animation triggers here.

                        }*/
                        if (_3rdPersonCharacterAnimator)
                        {
                            if (stickRendererToCapsuleBottom)
                            {
                                _3rdPersonCharacterAnimator.transform.position = (Vector3.right * _3rdPersonCharacterAnimator.transform.position.x) + (Vector3.up * (transform.position.y - (capsule.height / 2))) + (Vector3.forward * _3rdPersonCharacterAnimator.transform.position.z);
                            }
                            if (!zeroOut)
                            {
                                //Setup Thirdperson animation triggers here.
                                if (a_velocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velocity, p_Rigidbody.velocity.sqrMagnitude);
                                }
                                if (a_2DVelocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_2DVelocity, _2DVelocity.magnitude);
                                }
                                if (a_Idle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Idle, isIdle);
                                }
                                if (a_Sprinting != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sprinting, isSprinting);
                                }
                                if (a_Crouching != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Crouching, isCrouching);
                                }
                                if (a_Sliding != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sliding, isSliding);
                                }
                                if (a_Jumped != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Jumped, Jumped);
                                }
                                if (a_Grounded != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Grounded, currentGroundInfo.isInContactWithGround);
                                }
                                if (a_facon != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_facon, tengoFacon);
                                }
                                if (a_faconazo != "" && Input.GetKey(KeyCode.Mouse0) && !Jumped && !atacando && tengoFacon && !cubriendose && !controller.enMenuRadial)
                                {
                                    atacando = true;
                                    p_Rigidbody.velocity = Vector3.zero;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_faconazo);
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_esquivar != "" && Input.GetKey(KeyCode.Space) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    //Vector3 ultPos = transform.position;
                                    //ultimaPosicion = ultPos;
                                    atacando = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_esquivar);
                                    p_Rigidbody.velocity = Vector3.zero;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_poncho != "" && Input.GetKeyDown(KeyCode.Mouse1) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    cubriendose = true;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, true);
                                    p_Rigidbody.velocity = Vector3.zero;
                                }
                                if (Input.GetKeyUp(KeyCode.Mouse1) && cubriendose)
                                {
                                    cubriendose = false;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, false);
                                }
                                if (a_velXZ != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velXZ, velXZ.magnitude);
                                }
                                if (a_rifle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_rifle, tengoRifle);
                                }
                                if (a_boleadoras != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_boleadoras, tengoBoleadoras);
                                }
                                if (a_VelX != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelX, horizontal);
                                }
                                if (a_VelY != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelY, vertical);
                                }
                                if (a_lanzar != "" && tengoBoleadoras && Input.GetKey(KeyCode.Mouse0) && !controller.enMenuRadial)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_lanzar);
                                }
                                if (a_isDeath != "" && estaMuerto && !murio)
                                {
                                    murio = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_isDeath);
                                }

                            }
                            else
                            {
                                if (a_velocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velocity, 0);
                                }
                                if (a_2DVelocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_2DVelocity, 0);
                                }
                                if (a_Idle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Idle, true);
                                }
                                if (a_Sprinting != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sprinting, false);
                                }
                                if (a_Crouching != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Crouching, false);
                                }
                                if (a_Sliding != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sliding, false);
                                }
                                if (a_Jumped != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Jumped, false);
                                }
                                if (a_Grounded != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Grounded, true);
                                }
                                if (a_facon != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_facon, false);
                                }
                                if (a_faconazo != "" && Input.GetKey(KeyCode.Mouse0) && !Jumped && !atacando && tengoFacon)
                                {
                                    p_Rigidbody.velocity = Vector3.zero;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_faconazo);
                                    atacando = true;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_esquivar != "" && Input.GetKey(KeyCode.Space) && !Jumped && !atacando && tengoFacon)
                                {
                                    atacando = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_esquivar);
                                    p_Rigidbody.velocity = Vector3.zero;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_poncho != "" && Input.GetKeyDown(KeyCode.Mouse1) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    cubriendose = true;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, true);
                                    p_Rigidbody.velocity = Vector3.zero;
                                }
                                if (Input.GetKeyUp(KeyCode.Mouse1) && cubriendose)
                                {
                                    cubriendose = false;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, false);
                                }
                                if (a_velXZ != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velXZ, 0);
                                }
                                if (a_rifle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_rifle, false);
                                }
                                if (a_boleadoras != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_boleadoras, false);
                                }
                                if (a_VelX != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelX, 0);
                                }
                                if (a_VelY != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelY, 0);
                                }
                                if (a_lanzar != "" && tengoBoleadoras && Input.GetKey(KeyCode.Mouse0) && !controller.enMenuRadial)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_lanzar);
                                }
                                if (a_isDeath != "" && estaMuerto && !murio)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_isDeath);
                                }
                            }

                        }

                    }
                    break;

                case PerspectiveModes._3rdPerson:
                    {
                        if (_3rdPersonCharacterAnimator)
                        {
                            if (stickRendererToCapsuleBottom)
                            {
                                _3rdPersonCharacterAnimator.transform.position = (Vector3.right * _3rdPersonCharacterAnimator.transform.position.x) + (Vector3.up * (transform.position.y - (capsule.height / 2))) + (Vector3.forward * _3rdPersonCharacterAnimator.transform.position.z);
                            }
                            if (!zeroOut)
                            {
                                //Setup Thirdperson animation triggers here.
                                if (a_velocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velocity, p_Rigidbody.velocity.sqrMagnitude);
                                }
                                if (a_2DVelocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_2DVelocity, _2DVelocity.magnitude);
                                }
                                if (a_Idle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Idle, isIdle);
                                }
                                if (a_Sprinting != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sprinting, isSprinting);
                                }
                                if (a_Crouching != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Crouching, isCrouching);
                                }
                                if (a_Sliding != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sliding, isSliding);
                                }
                                if (a_Jumped != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Jumped, Jumped);
                                }
                                if (a_Grounded != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Grounded, currentGroundInfo.isInContactWithGround);
                                }
                                if(a_facon != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_facon, tengoFacon);
                                }
                                if(a_faconazo != "" && Input.GetKey(KeyCode.Mouse0) && !Jumped && !atacando && tengoFacon && !cubriendose && !controller.enMenuRadial)
                                {
                                    atacando = true;
                                    p_Rigidbody.velocity = Vector3.zero;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_faconazo);
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_esquivar != "" && Input.GetKey(KeyCode.Space) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    //Vector3 ultPos = transform.position;
                                    //ultimaPosicion = ultPos;
                                    atacando = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_esquivar);
                                    p_Rigidbody.velocity = Vector3.zero;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_poncho != "" && Input.GetKeyDown(KeyCode.Mouse1) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    cubriendose = true;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, true);
                                    p_Rigidbody.velocity = Vector3.zero;
                                }
                                if (Input.GetKeyUp(KeyCode.Mouse1) && cubriendose)
                                {
                                    cubriendose = false;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, false);
                                }
                                if (a_velXZ != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velXZ, velXZ.magnitude);
                                }
                                if(a_rifle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_rifle, tengoRifle);
                                }
                                if (a_boleadoras != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_boleadoras, tengoBoleadoras);
                                }
                                if (a_VelX != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelX, horizontal);
                                }
                                if (a_VelY != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelY, vertical);
                                }
                                if (a_lanzar != "" && tengoBoleadoras && Input.GetKey(KeyCode.Mouse0) && !controller.enMenuRadial)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_lanzar);
                                }
                                if(a_isDeath != "" && estaMuerto && !murio)
                                {
                                    murio = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_isDeath);
                                }

                            }
                            else
                            {
                                if (a_velocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velocity, 0);
                                }
                                if (a_2DVelocity != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_2DVelocity, 0);
                                }
                                if (a_Idle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Idle, true);
                                }
                                if (a_Sprinting != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sprinting, false);
                                }
                                if (a_Crouching != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Crouching, false);
                                }
                                if (a_Sliding != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Sliding, false);
                                }
                                if (a_Jumped != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Jumped, false);
                                }
                                if (a_Grounded != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_Grounded, true);
                                }
                                if(a_facon != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_facon, false);
                                }
                                if (a_faconazo != "" && Input.GetKey(KeyCode.Mouse0) && !Jumped && !atacando && tengoFacon)
                                {
                                    p_Rigidbody.velocity = Vector3.zero;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_faconazo);
                                    atacando = true;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_esquivar != "" && Input.GetKey(KeyCode.Space) && !Jumped && !atacando && tengoFacon)
                                {
                                    atacando = true;
                                    _3rdPersonCharacterAnimator.SetTrigger(a_esquivar);
                                    p_Rigidbody.velocity = Vector3.zero;
                                    InstantStaminaReduction(s_FacaStaminaDepletion);
                                }
                                if (a_poncho != "" && Input.GetKeyDown(KeyCode.Mouse1) && !Jumped && !atacando && tengoFacon && !cubriendose)
                                {
                                    cubriendose = true;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, true);
                                    p_Rigidbody.velocity = Vector3.zero;
                                }
                                if (Input.GetKeyUp(KeyCode.Mouse1) && cubriendose)
                                {
                                    cubriendose = false;
                                    _3rdPersonCharacterAnimator.SetBool(a_poncho, false);
                                }
                                if (a_velXZ != "")
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_velXZ, 0);
                                }
                                if (a_rifle != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_rifle, false);
                                }
                                if (a_boleadoras != "")
                                {
                                    _3rdPersonCharacterAnimator.SetBool(a_boleadoras, false);
                                }
                                if (a_VelX != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelX, 0);
                                }
                                if (a_VelY != "" && tengoBoleadoras)
                                {
                                    _3rdPersonCharacterAnimator.SetFloat(a_VelY, 0);
                                }
                                if (a_lanzar != "" && tengoBoleadoras && Input.GetKey(KeyCode.Mouse0) && !controller.enMenuRadial)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_lanzar);
                                }
                                if (a_isDeath != "" && estaMuerto && !murio)
                                {
                                    _3rdPersonCharacterAnimator.SetTrigger(a_isDeath);
                                }
                            }

                        }

                    }
                    break;
            }
        }
        #endregion

      /*  public void PausePlayer(PauseModes pauseMode)
        {
            controllerPaused = true;
            switch (pauseMode)
            {
                case PauseModes.MakeKinematic:
                    {
                        p_Rigidbody.isKinematic = true;
                    }
                    break;

                case PauseModes.FreezeInPlace:
                    {
                        p_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    break;

                case PauseModes.BlockInputOnly:
                    {

                    }
                    break;
            }

            p_Rigidbody.velocity = Vector3.zero;
            InputDir = Vector2.zero;
            MovInput = Vector2.zero;
            MovInput_Smoothed = Vector2.zero;
            capsule.sharedMaterial = _MaxFriction;

            UpdateAnimationTriggers(true);
            if (a_velocity != "")
            {
                _3rdPersonCharacterAnimator.SetFloat(a_velocity, 0);
            }
        }
        public void UnpausePlayer(float delay = 0)
        {
            if (delay == 0)
            {
                controllerPaused = false;
                p_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                p_Rigidbody.isKinematic = false;
            }
            else
            {
                StartCoroutine(UnpausePlayerI(delay));
            }
        }
        IEnumerator UnpausePlayerI(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            controllerPaused = false;
            p_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            p_Rigidbody.isKinematic = false;
        }*/

    }


    #region Classes and Enums
    [System.Serializable]
    public class GroundInfo
    {
        public bool isInContactWithGround, isGettingGroundInfo, potentialStair;
        public float groundAngleMultiplier_Inverse = 1, groundAngleMultiplier_Inverse_persistent = 1, groundAngleMultiplier = 0, groundAngle, groundAngle_Raw, playerGroundPosition, groundRawYPosition;
        public Vector3 groundInfluenceDirection, groundNormal_Averaged, groundNormal_Raw;
        public List<Vector3> groundNormals_lowgrade = new List<Vector3>(), groundNormals_highgrade;
        public string groundTag;
        public Material groundMaterial;
        public TerrainLayer groundLayer;
        public PhysicMaterial groundPhysicMaterial;
        internal Terrain currentTerrain;
        internal Mesh currentMesh;
        internal RaycastHit groundFromRay, stairCheck_RiserCheck, stairCheck_HeightCheck;
        internal RaycastHit[] groundFromSweep;


    }
    [System.Serializable]
    public class GroundMaterialProfile
    {
        public MatProfileType profileTriggerType = MatProfileType.Material;
        public List<Material> _Materials;
        public List<PhysicMaterial> _physicMaterials;
        public List<TerrainLayer> _Layers;
        public List<AudioClip> footstepClips = new List<AudioClip>();
    }
    [System.Serializable]
    public class SurvivalStats
    {
        public float Health = 250.0f, Hunger = 100.0f, Hydration = 100f;
        public bool hasLowHealth, isStarving, isDehydrated;
    }
    public enum StatSelector { Health, Hunger, Hydration }
    public enum MatProfileType { Material, terrainLayer, physicMaterial }
    public enum FootstepTriggeringMode { calculatedTiming, calledFromAnimations }
    //public enum PerspectiveModes { _1stPerson, _3rdPerson }
    //public enum ViewInputModes { Traditional, Retro }
    //public enum MouseInputInversionModes { None, X, Y, Both }
    public enum GroundSpeedProfiles { Crouching, Walking, Sprinting, Sliding }
    public enum Stances { Standing, Crouching }
    public enum PauseModes { MakeKinematic, FreezeInPlace, BlockInputOnly }
    #endregion


    #region Editor Scripting
#if UNITY_EDITOR
    [CustomEditor(typeof(JuanMoveBehaviour))]
    public class SuperFPEditor : Editor
    {
        Color32 statBackingColor = new Color32(64, 64, 64, 255);

        GUIStyle labelHeaderStyle;
        GUIStyle l_scriptHeaderStyle;
        GUIStyle labelSubHeaderStyle;
        GUIStyle clipSetLabelStyle;
        GUIStyle SupportButtonStyle;
        GUIStyle ShowMoreStyle;
        GUIStyle BoxPanel;
        Texture2D BoxPanelColor;
        JuanMoveBehaviour t;
        CamaraBahaviour s;
        SerializedObject tSO, SurvivalStatsTSO;
        SerializedProperty interactableLayer, obstructionMaskField, groundLayerMask, groundMatProf, defaultSurvivalStats, currentSurvivalStats;
        static bool cameraSettingsFoldout = false, movementSettingFoldout = false, survivalStatsFoldout, footStepFoldout = false;

        public void OnEnable()
        {
            t = (JuanMoveBehaviour)target;
            //s = (CamaraBahaviour)target;
            tSO = new SerializedObject(t);
            SurvivalStatsTSO = new SerializedObject(t);
            obstructionMaskField = tSO.FindProperty("cameraObstructionIgnore");
            groundLayerMask = tSO.FindProperty("whatIsGround");
            groundMatProf = tSO.FindProperty("footstepSoundSet");
            interactableLayer = tSO.FindProperty("interactableLayer");
            BoxPanelColor = new Texture2D(1, 1, TextureFormat.RGBAFloat, false); ;
            BoxPanelColor.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.2f));
            BoxPanelColor.Apply();
        }

        public override void OnInspectorGUI()
        {

            #region Style Null Check
            labelHeaderStyle = labelHeaderStyle != null ? labelHeaderStyle : new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 };
            l_scriptHeaderStyle = l_scriptHeaderStyle != null ? l_scriptHeaderStyle : new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, richText = true, fontSize = 16 };
            labelSubHeaderStyle = labelSubHeaderStyle != null ? labelSubHeaderStyle : new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 10, richText = true };
            ShowMoreStyle = ShowMoreStyle != null ? ShowMoreStyle : new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, margin = new RectOffset(15, 0, 0, 0), fontStyle = FontStyle.Bold, fontSize = 11, richText = true };
            clipSetLabelStyle = labelSubHeaderStyle != null ? labelSubHeaderStyle : new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 };
            SupportButtonStyle = SupportButtonStyle != null ? SupportButtonStyle : new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 10, richText = true };
            BoxPanel = BoxPanel != null ? BoxPanel : new GUIStyle(GUI.skin.box) { normal = { background = BoxPanelColor } };
            #endregion

           
            t.controller = (Controller)EditorGUILayout.ObjectField(new GUIContent("Player", "The "), t.controller, typeof(Controller), true);
            //t.posCamera = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Posicion Camara", "The "), t.posCamera, typeof(GameObject), true);
            t.speed = EditorGUILayout.Slider(new GUIContent("", ""), t.speed, 0.0f, 10.0f);
            t.speedRotate = EditorGUILayout.Slider(new GUIContent("", ""), t.speedRotate, 50.0f, 300.0f);

            t.boleadoraPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab Boleadoras",
                "Referencia al prefab de las boleadoras que se van a lanzar"), t.boleadoraPrefab, typeof(GameObject), true);
            t.lanzamientoPos = (Transform)EditorGUILayout.ObjectField(new GUIContent("Posición de lanxamiento",
                "Referencia a la posicion de dónde salen lanzadas las boleadoras"), t.lanzamientoPos, typeof(Transform), true);
            t.fuerzaLanzamiento = EditorGUILayout.Slider(new GUIContent("Fuerza de Lanzamiento", ""), t.fuerzaLanzamiento, 1.0f, 30.0f);


            #region Movement Settings

            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();
            GUILayout.Label("Movement Settings", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginVertical(BoxPanel);
            if (movementSettingFoldout)
            {
                #region Stances and Speed
                t.enableMovementControl = EditorGUILayout.ToggleLeft(new GUIContent("Enable Movement", "Should the player have control over the character's movement?"), t.enableMovementControl);
                GUILayout.Label("<color=grey>Stances and Speed</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                EditorGUILayout.Space(15);

                GUI.enabled = false;
                t.currentGroundMovementSpeed = (GroundSpeedProfiles)EditorGUILayout.EnumPopup(new GUIContent("Current Movement Speed", "Displays the player's current movement speed"), t.currentGroundMovementSpeed);
                GUI.enabled = true;

                EditorGUILayout.Space();
                t.walkingSpeed = EditorGUILayout.Slider(new GUIContent("Walking Speed", "How quickly can the player move while walking?"), t.walkingSpeed, 1, 400);

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(BoxPanel);
                t.canSprint = EditorGUILayout.ToggleLeft(new GUIContent("Can Sprint", "Is the player allowed to enter a sprint?"), t.canSprint);
                GUI.enabled = t.canSprint;
                t.toggleSprint = EditorGUILayout.ToggleLeft(new GUIContent("Toggle Sprint", "Should the spring key act as a toggle?"), t.toggleSprint);
#if ENABLE_INPUT_SYSTEM
            t.sprintKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "The Key used to enter a sprint."),t.sprintKey);
#else
                t.sprintKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "The Key used to enter a sprint."), t.sprintKey_L);
#endif
                t.sprintingSpeed = EditorGUILayout.Slider(new GUIContent("Sprinting Speed", "How quickly can the player move while sprinting?"), t.sprintingSpeed, t.walkingSpeed + 1, 650);
                t.decelerationSpeed = EditorGUILayout.Slider(new GUIContent("Deceleration Factor", "Behaves somewhat like a braking force"), t.decelerationSpeed, 1, 300);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(BoxPanel);
                t.canCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Can Crouch", "Is the player allowed to crouch?"), t.canCrouch);
                GUI.enabled = t.canCrouch;
                t.toggleCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Toggle Crouch", "Should pressing the crouch button act as a toggle?"), t.toggleCrouch);
#if ENABLE_INPUT_SYSTEM
            t.crouchKey= (Key)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "The Key used to start a crouch."),t.crouchKey);
#else
                t.crouchKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "The Key used to start a crouch."), t.crouchKey_L);
#endif
                t.crouchingSpeed = EditorGUILayout.Slider(new GUIContent("Crouching Speed", "How quickly can the player move while crouching?"), t.crouchingSpeed, 1, t.walkingSpeed - 1);
                t.crouchingHeight = EditorGUILayout.Slider(new GUIContent("Crouching Height", "How small should the character's capsule collider be when crouching?"), t.crouchingHeight, 0.01f, 2);
                EditorGUILayout.EndVertical();

                GUI.enabled = true;


                EditorGUILayout.Space(20);
                GUI.enabled = false;
                t.currentStance = (Stances)EditorGUILayout.EnumPopup(new GUIContent("Current Stance", "Displays the character's current stance"), t.currentStance);
                GUI.enabled = true;
                t.stanceTransitionSpeed = EditorGUILayout.Slider(new GUIContent("Stance Transition Speed", "How quickly should the character change stances?"), t.stanceTransitionSpeed, 0.1f, 10);

                EditorGUILayout.PropertyField(groundLayerMask, new GUIContent("What Is Ground", "What physics layers should be considered to be ground?"));

                #region Slope affectors
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(BoxPanel);
                GUILayout.Label("<color=grey>Slope Affectors</color>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 10, richText = true }, GUILayout.ExpandWidth(true));

                t.hardSlopeLimit = EditorGUILayout.Slider(new GUIContent("Hard Slope Limit", "At what slope angle should the player no longer be able to walk up?"), t.hardSlopeLimit, 45, 89);
                t.maxStairRise = EditorGUILayout.Slider(new GUIContent("Maximum Stair Rise", "How tall can a single stair rise?"), t.maxStairRise, 0, 1.5f);
                t.stepUpSpeed = EditorGUILayout.Slider(new GUIContent("Step Up Speed", "How quickly will the player climb a step?"), t.stepUpSpeed, 0.01f, 0.45f);
                EditorGUILayout.EndVertical();
                #endregion
                EditorGUILayout.EndVertical();
                #endregion

                #region Jumping
                EditorGUILayout.Space();
                GUILayout.Label("<color=grey>Jumping Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                //EditorGUILayout.Space(15);

                t.canJump = EditorGUILayout.ToggleLeft(new GUIContent("Can Jump", "Is the player allowed to jump?"), t.canJump);
                GUI.enabled = t.canJump;
#if ENABLE_INPUT_SYSTEM
            t.jumpKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "The Key used to jump."),t.jumpKey);
#else
                t.jumpKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "The Key used to jump."), t.jumpKey_L);
#endif
                t.holdJump = EditorGUILayout.ToggleLeft(new GUIContent("Continuous Jumping", "Should the player be able to continue jumping without letting go of the Jump key"), t.holdJump);
                t.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "How much power should a jump have?"), t.jumpPower, 1, 650f);
                t.airControlFactor = EditorGUILayout.Slider(new GUIContent("Air Control Factor", "EXPERIMENTAL: How much control should the player have over their direction while in the air"), t.airControlFactor, 0, 1);
                GUI.enabled = t.enableStaminaSystem;
                t.jumpingDepletesStamina = EditorGUILayout.ToggleLeft(new GUIContent("Jumping Depletes Stamina", "Should jumping deplete stamina?"), t.jumpingDepletesStamina);
                t.s_JumpStaminaDepletion = EditorGUILayout.Slider(new GUIContent("Jump Stamina Depletion Amount", "How much stamina should jumping use?"), t.s_JumpStaminaDepletion, 0, t.Stamina);
                t.s_FacaStaminaDepletion = EditorGUILayout.Slider(new GUIContent("Facazo Stamina Depletion Amount", "How much stamina should Faca use?"), t.s_FacaStaminaDepletion, 0, t.Stamina);
                GUI.enabled = true;
                t.jumpEnhancements = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump Enhancements", "Should extra math be used to enhance the jump curve?"), t.jumpEnhancements);
                if (t.jumpEnhancements)
                {
                    t.decentMultiplier = EditorGUILayout.Slider(new GUIContent("On Decent Multiplier", "When the player begins to descend  during a jump, what should gravity be multiplied by?"), t.decentMultiplier, 0.1f, 5);
                    t.tapJumpMultiplier = EditorGUILayout.Slider(new GUIContent("Tap Jump Multiplier", "When the player lets go of space prematurely during a jump, what should gravity be multiplied by?"), t.tapJumpMultiplier, 0.1f, 5);
                }

                EditorGUILayout.EndVertical();
                #endregion

                #region Sliding
                EditorGUILayout.Space();
                GUILayout.Label("<color=grey>Sliding Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                //EditorGUILayout.Space(15);

                t.canSlide = EditorGUILayout.ToggleLeft(new GUIContent("Can Slide", "Is the player allowed to slide? Use the crouch key to initiate a slide!"), t.canSlide);
                GUI.enabled = t.canSlide;
#if ENABLE_INPUT_SYSTEM
            t.slideKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Slide Key", "The Key used to Slide while the character is sprinting."),t.slideKey);
#else
                t.slideKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Slide Key", "The Key used to Slide wile the character is sprinting."), t.slideKey_L);
#endif
                t.slidingDeceleration = EditorGUILayout.Slider(new GUIContent("Sliding Deceleration", "How much deceleration should be applied while sliding?"), t.slidingDeceleration, 50, 300);
                t.slidingTransitionSpeed = EditorGUILayout.Slider(new GUIContent("Sliding Transition Speed", "How quickly should the character transition from the current stance to sliding?"), t.slidingTransitionSpeed, 0.01f, 10);
                t.maxFlatSlideDistance = EditorGUILayout.Slider(new GUIContent("Flat Slide Distance", "If the player starts sliding on a flat surface with no ground angle influence, How many units should the player slide forward?"), t.maxFlatSlideDistance, 0.5f, 15);
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                #endregion

                if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Movement Setting changes"); tSO.ApplyModifiedProperties(); }
            }
            else
            {
                t.enableMovementControl = EditorGUILayout.ToggleLeft(new GUIContent("Enable Movement", "Should the player have control over the character's movement?"), t.enableMovementControl);
                t.walkingSpeed = EditorGUILayout.Slider(new GUIContent("Walking Speed", "How quickly can the player move while walking?"), t.walkingSpeed, 1, 400);
                t.sprintingSpeed = EditorGUILayout.Slider(new GUIContent("Sprinting Speed", "How quickly can the player move while sprinting?"), t.sprintingSpeed, t.walkingSpeed + 1, 650);
            }
            EditorGUILayout.Space();
            movementSettingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(movementSettingFoldout, movementSettingFoldout ? "<color=#B83C82>show less</color>" : "<color=#B83C82>show more</color>", ShowMoreStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Stamina
            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();
            GUILayout.Label("Stamina", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);
            t.enableStaminaSystem = EditorGUILayout.ToggleLeft(new GUIContent("Enable Stamina System", "Should the controller enable it's stamina system?"), t.enableStaminaSystem);

            //preview bar
            Rect casingRectSP = EditorGUILayout.GetControlRect(),
                    statRectSP = new Rect(casingRectSP.x + 2, casingRectSP.y + 2, Mathf.Clamp(((casingRectSP.width / t.Stamina) * t.currentStaminaLevel) - 4, 0, casingRectSP.width), casingRectSP.height - 4),
                    statRectMSP = new Rect(casingRectSP.x + 2, casingRectSP.y + 2, Mathf.Clamp(((casingRectSP.width / t.Stamina) * t.s_minimumStaminaToSprint) - 4, 0, casingRectSP.width), casingRectSP.height - 4);
            EditorGUI.DrawRect(casingRectSP, statBackingColor);
            EditorGUI.DrawRect(statRectMSP, new Color32(96, 96, 64, 255));
            EditorGUI.DrawRect(statRectSP, new Color32(94, 118, 135, (byte)(GUI.enabled ? 191 : 64)));


            GUI.enabled = t.enableStaminaSystem;
            t.Stamina = EditorGUILayout.Slider(new GUIContent("Stamina", "The maximum stamina level"), t.Stamina, 0, 250.0f);
            t.s_minimumStaminaToSprint = EditorGUILayout.Slider(new GUIContent("Minimum Stamina To Sprint", "The minimum stamina required to enter a sprint."), t.s_minimumStaminaToSprint, 0, t.Stamina);
            t.s_depletionSpeed = EditorGUILayout.Slider(new GUIContent("Depletion Speed", ""), t.s_depletionSpeed, 0, 15.0f);
            t.s_regenerationSpeed = EditorGUILayout.Slider(new GUIContent("Regeneration Speed", "The speed at which stamina will regenerate"), t.s_regenerationSpeed, 0, 10.0f);

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Stamina Setting changes"); tSO.ApplyModifiedProperties(); }
            #endregion

            #region Footstep Audio
            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();
            GUILayout.Label("Footstep Audio", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);

            t.enableFootstepSounds = EditorGUILayout.ToggleLeft(new GUIContent("Enable Footstep System", "Should the crontoller enable it's footstep audio systems?"), t.enableFootstepSounds);
            GUI.enabled = t.enableFootstepSounds;
            t.footstepTriggeringMode = (FootstepTriggeringMode)EditorGUILayout.EnumPopup(new GUIContent("Footstep Trigger Mode", "How should a footstep SFX call be triggered? \n\n- Calculated Timing: The controller will attempt to calculate the footstep cycle position based on Headbob cycle position, movement speed, and capsule size. This can sometimes be inaccurate depending on the selected perspective and base walk speed. (Not recommended if character animations are being used)\n\n- Called From Animations: The controller will not do it's own footstep cycle calculations/call for SFX. Instead the controller will rely on character Animations to call the 'CallFootstepClip()' function. This gives much more precise results. The controller will still calculate what footstep clips should be played."), t.footstepTriggeringMode);

            if (t.footstepTriggeringMode == FootstepTriggeringMode.calculatedTiming)
            {
                t.stepTiming = EditorGUILayout.Slider(new GUIContent("Step Timing", "The time (measured in seconds) between each footstep."), t.stepTiming, 0.0f, 1.0f);
                t.modificadorCorriendo = EditorGUILayout.Slider(new GUIContent("Modificador Corriendo", "The time (measured in seconds) between each footstep."), t.modificadorCorriendo, 0.0f, 1.0f);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.Space();
            //GUILayout.Label("<color=grey>Clip Stacks</color>",labelSubHeaderStyle,GUILayout.ExpandWidth(true));
            EditorGUI.indentLevel++;
            footStepFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(footStepFoldout, footStepFoldout ? "<color=#B83C82>hide clip stacks</color>" : "<color=#B83C82>show clip stacks</color>", ShowMoreStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;
            if (footStepFoldout)
            {
                if (t.footstepSoundSet.Any())
                {
                    if (!Application.isPlaying)
                    {
                        for (int i = 0; i < groundMatProf.arraySize; i++)
                        {
                            EditorGUILayout.BeginVertical(BoxPanel);
                            EditorGUILayout.BeginVertical(BoxPanel);

                            SerializedProperty profile = groundMatProf.GetArrayElementAtIndex(i), clipList = profile.FindPropertyRelative("footstepClips"), mat = profile.FindPropertyRelative("_Materials"), physMat = profile.FindPropertyRelative("_physicMaterials"), layer = profile.FindPropertyRelative("_Layers"), triggerType = profile.FindPropertyRelative("profileTriggerType");
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Clip Stack {i + 1}", clipSetLabelStyle);
                            if (GUILayout.Button(new GUIContent("X", "Remove this profile"), GUILayout.MaxWidth(20))) { t.footstepSoundSet.RemoveAt(i); UpdateGroundProfiles(); break; }
                            EditorGUILayout.EndHorizontal();

                            //Check again that the list of profiles isn't empty incase we removed the last one with the button above.
                            if (t.footstepSoundSet.Any())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(triggerType, new GUIContent("Trigger Mode", "Is this clip stack triggered by a Material or a Terrain Layer?"));
                                switch (t.footstepSoundSet[i].profileTriggerType)
                                {
                                    case MatProfileType.Material: { EditorGUILayout.PropertyField(mat, new GUIContent("Materials", "The materials used to trigger this footstep stack.")); } break;
                                    case MatProfileType.physicMaterial: { EditorGUILayout.PropertyField(physMat, new GUIContent("Physic Materials", "The Physic Materials used to trigger this footstep stack.")); } break;
                                    case MatProfileType.terrainLayer: { EditorGUILayout.PropertyField(layer, new GUIContent("Terrain Layers", "The Terrain Layers used to trigger this footstep stack.")); } break;
                                }
                                EditorGUILayout.Space();

                                EditorGUILayout.PropertyField(clipList, new GUIContent("Clip Stack", "The Audio clips used in this stack."), true);
                                EditorGUI.indentLevel--;
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();
                                if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, $"Undo changes to Clip Stack {i + 1}"); tSO.ApplyModifiedProperties(); }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Foot step sound sets hidden to save runtime resources.", MessageType.Info);
                    }
                }
                if (GUILayout.Button(new GUIContent("Add Profile", "Add new profile"))) { t.footstepSoundSet.Add(new GroundMaterialProfile() { profileTriggerType = MatProfileType.Material, _Materials = null, _Layers = null, footstepClips = new List<AudioClip>() }); UpdateGroundProfiles(); }
                if (GUILayout.Button(new GUIContent("Remove All Profiles", "Remove all profiles"))) { t.footstepSoundSet.Clear(); }
                EditorGUILayout.Space();
                footStepFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(footStepFoldout, footStepFoldout ? "<color=#B83C82>hide clip stacks</color>" : "<color=#B83C82>show clip stacks</color>", ShowMoreStyle);
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            //EditorGUILayout.PropertyField(groundMatProf,new GUIContent("Footstep Sound Profiles"));

            GUI.enabled = true;
            //EditorGUILayout.HelpBox("Due to limitations In order to use the Material trigger mode, Imported Mesh's must have Read/Write enabled. Additionally, these Mesh's cannot be marked as Batching Static. Work arounds for both of these limitations are being researched.", MessageType.Info);
            EditorGUILayout.EndVertical();
            if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Footstep Audio Setting changes"); tSO.ApplyModifiedProperties(); }

            #endregion

            #region Survival Stats
            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();
            GUILayout.Label("Survival Stats", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(10);

            SurvivalStatsTSO = new SerializedObject(t);
            defaultSurvivalStats = SurvivalStatsTSO.FindProperty("defaultSurvivalStats");
            currentSurvivalStats = SurvivalStatsTSO.FindProperty("currentSurvivalStats");

            #region Basic settings
            EditorGUILayout.BeginVertical(BoxPanel);
            GUILayout.Label("<color=grey>Basic Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
            t.enableSurvivalStats = EditorGUILayout.ToggleLeft(new GUIContent("Enable Survival Stats", "Should the controller enable it's survival systems?"), t.enableSurvivalStats);
            GUI.enabled = t.enableSurvivalStats;
            t.statTickRate = EditorGUILayout.Slider(new GUIContent("Stat Ticks Per-minute", "How many times per-minute should the stats do a tick update? Each tick depletes/regenerates the stats by their respective rates below."), t.statTickRate, 0.1f, 20.0f);
            #endregion
            if (survivalStatsFoldout)
            {

                #region Health Settings
                GUILayout.Label("<color=grey>Health Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                SerializedProperty statHP = defaultSurvivalStats.FindPropertyRelative("Health"), currentStatHP = currentSurvivalStats.FindPropertyRelative("Health");

                //preview bar
                Rect casingRectHP = EditorGUILayout.GetControlRect(), statRectHP = new Rect(casingRectHP.x + 2, casingRectHP.y + 2, Mathf.Clamp(((casingRectHP.width / statHP.floatValue) * currentStatHP.floatValue) - 4, 0, casingRectHP.width), casingRectHP.height - 4);
                EditorGUI.DrawRect(casingRectHP, statBackingColor);
                EditorGUI.DrawRect(statRectHP, new Color32(211, 0, 0, (byte)(GUI.enabled ? 191 : 64)));

                EditorGUILayout.PropertyField(statHP, new GUIContent("Health Points", "How much health does the controller start with?"));

                GUI.enabled = false;
                EditorGUILayout.ToggleLeft(new GUIContent("Health is critically low?"), currentSurvivalStats.FindPropertyRelative("hasLowHealth").boolValue);
                GUI.enabled = t.enableSurvivalStats;
                EditorGUILayout.EndVertical();
                #endregion

                #region Hunger Settings
                GUILayout.Label("<color=grey>Hunger Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                SerializedProperty statHU = defaultSurvivalStats.FindPropertyRelative("Hunger"), currentStatHU = currentSurvivalStats.FindPropertyRelative("Hunger");

                //preview bar
                Rect casingRectHU = EditorGUILayout.GetControlRect(), statRectHU = new Rect(casingRectHU.x + 2, casingRectHU.y + 2, Mathf.Clamp(((casingRectHU.width / statHU.floatValue) * currentStatHU.floatValue) - 4, 0, casingRectHU.width), casingRectHU.height - 4);
                EditorGUI.DrawRect(casingRectHU, statBackingColor);
                EditorGUI.DrawRect(statRectHU, new Color32(142, 54, 0, (byte)(GUI.enabled ? 191 : 64)));

                EditorGUILayout.PropertyField(statHU, new GUIContent("Hunger Points", "How much Hunger does the controller start with?"));
                t.hungerDepletionRate = EditorGUILayout.Slider(new GUIContent("Hunger Depletion Per Tick", "How much does hunger deplete per tick?"), t.hungerDepletionRate, 0, 5);
                GUI.enabled = false;
                EditorGUILayout.ToggleLeft(new GUIContent("Player is Starving?"), currentSurvivalStats.FindPropertyRelative("isStarving").boolValue);
                GUI.enabled = t.enableSurvivalStats;
                EditorGUILayout.EndVertical();
                #endregion

                #region Hydration Settings
                GUILayout.Label("<color=grey>Hydration Settings</color>", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(BoxPanel);
                SerializedProperty statHY = defaultSurvivalStats.FindPropertyRelative("Hydration"), currentStatHY = currentSurvivalStats.FindPropertyRelative("Hydration");

                //preview bar
                Rect casingRectHY = EditorGUILayout.GetControlRect(), statRectHY = new Rect(casingRectHY.x + 2, casingRectHY.y + 2, Mathf.Clamp(((casingRectHY.width / statHY.floatValue) * currentStatHY.floatValue) - 4, 0, casingRectHY.width), casingRectHY.height - 4);
                EditorGUI.DrawRect(casingRectHY, statBackingColor);
                EditorGUI.DrawRect(statRectHY, new Color32(0, 194, 255, (byte)(GUI.enabled ? 191 : 64)));

                EditorGUILayout.PropertyField(statHY, new GUIContent("Hydration Points", "How much Hydration does the controller start with?"));
                t.hydrationDepletionRate = EditorGUILayout.Slider(new GUIContent("Hydration Depletion Per Tick", "How much does hydration deplete per tick?"), t.hydrationDepletionRate, 0, 5);
                GUI.enabled = false;
                EditorGUILayout.ToggleLeft(new GUIContent("Player is Dehydrated?"), currentSurvivalStats.FindPropertyRelative("isDehydrated").boolValue);
                GUI.enabled = t.enableSurvivalStats;
                EditorGUILayout.EndVertical();
                #endregion
            }
            EditorGUILayout.Space();
            survivalStatsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(survivalStatsFoldout, survivalStatsFoldout ? "<color=#B83C82>show less</color>" : "<color=#B83C82>show more</color>", ShowMoreStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            GUI.enabled = true;
            if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Survival Stat Setting changes"); tSO.ApplyModifiedProperties(); }
            #endregion

            #region Animation Triggers
            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();
            GUILayout.Label("Animator Settup", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(BoxPanel);
            //t._1stPersonCharacterAnimator = (Animator)EditorGUILayout.ObjectField(new GUIContent("1st Person Animator", "The animator used on the 1st person character mesh (if any)"), t._1stPersonCharacterAnimator, typeof(Animator), true);
            t._3rdPersonCharacterAnimator = (Animator)EditorGUILayout.ObjectField(new GUIContent("3rd Person Animator", "The animator used on the 3rd person character mesh (if any)"), t._3rdPersonCharacterAnimator, typeof(Animator), true);
            if (t._3rdPersonCharacterAnimator /*|| t._1stPersonCharacterAnimator*/)
            {
                EditorGUILayout.BeginVertical(BoxPanel);
                GUILayout.Label("Parameters", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
                t.a_velocity = EditorGUILayout.TextField(new GUIContent("Velocity (Float)", "(Float) The name of the Velocity Parameter in the animator"), t.a_velocity);
                t.a_2DVelocity = EditorGUILayout.TextField(new GUIContent("2D Velocity (Float)", "(Float) The name of the 2D Velocity Parameter in the animator"), t.a_2DVelocity);
                t.a_Idle = EditorGUILayout.TextField(new GUIContent("Idle (Bool)", "(Bool) The name of the Idle Parameter in the animator"), t.a_Idle);
                t.a_Sprinting = EditorGUILayout.TextField(new GUIContent("Sprinting (Bool)", "(Bool) The name of the Sprinting Parameter in the animator"), t.a_Sprinting);
                t.a_Crouching = EditorGUILayout.TextField(new GUIContent("Crouching (Bool)", "(Bool) The name of the Crouching Parameter in the animator"), t.a_Crouching);
                t.a_Sliding = EditorGUILayout.TextField(new GUIContent("Sliding (Bool)", "(Bool) The name of the Sliding Parameter in the animator"), t.a_Sliding);
                t.a_Jumped = EditorGUILayout.TextField(new GUIContent("Jumped (Bool)", "(Bool) The name of the Jumped Parameter in the animator"), t.a_Jumped);
                t.a_Grounded = EditorGUILayout.TextField(new GUIContent("Grounded (Bool)", "(Bool) The name of the Grounded Parameter in the animator"), t.a_Grounded);
                t.a_facon = EditorGUILayout.TextField(new GUIContent("Facon (Bool)", "(Bool) Facon"), t.a_facon);
                t.a_faconazo = EditorGUILayout.TextField(new GUIContent("Facon (Trigger)", "(Trigger"), t.a_faconazo);
                t.a_esquivar = EditorGUILayout.TextField(new GUIContent("Esquivar (Trigger)", "(Trigger"), t.a_esquivar);
                t.a_poncho = EditorGUILayout.TextField(new GUIContent("Poncho (Bool)", "(Bool"), t.a_poncho);
                t.a_velXZ = EditorGUILayout.TextField(new GUIContent("velXZ (Float)", "(Float)"), t.a_velXZ);
                t.a_rifle = EditorGUILayout.TextField(new GUIContent("Rifle (Bool)", "(Bool) Rifle"), t.a_rifle);
                t.a_boleadoras = EditorGUILayout.TextField(new GUIContent("Boleadoras (Bool)", "(Bool) Boleadoras"), t.a_boleadoras);
                t.a_VelX = EditorGUILayout.TextField(new GUIContent("VelX (Float)", "(Float)"), t.a_VelX);
                t.a_VelY = EditorGUILayout.TextField(new GUIContent("VelY (Float)", "(Float)"), t.a_VelY);
                t.a_lanzar = EditorGUILayout.TextField(new GUIContent("LanzarB (Trigger)", "(Trigger"), t.a_lanzar);
                //t.a_isDeath = EditorGUILayout.TextField(new GUIContent("isDeath (Trigger)", "(Trigger"), t.a_isDeath);
                t.a_isDeath = EditorGUILayout.TextField(new GUIContent("IsDeath (Bool)", "(Bool) IsDeath"), t.a_isDeath);
                EditorGUILayout.EndVertical();
            }
            //EditorGUILayout.HelpBox("WIP - This is a work in progress feature and currently very primitive.\n\n No triggers, bools, floats, or ints are set up in the script. To utilize this feature, find 'UpdateAnimationTriggers()' function in this script and set up triggers with the correct string names there. This function gets called by the script whenever a relevant parameter gets updated. (I.e. when 'isVaulting' changes)", MessageType.Info);
            EditorGUILayout.EndVertical();
            if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Animation settings changes"); tSO.ApplyModifiedProperties(); }
            #endregion

        }

        void UpdateGroundProfiles()
        {
            tSO = new SerializedObject(t);
            groundMatProf = tSO.FindProperty("footstepSoundSet");
        }
    }
#endif
    #endregion

}
