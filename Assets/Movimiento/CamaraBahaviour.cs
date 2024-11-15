using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using SUPERCharacte;



#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
#endif

namespace CamaraTerceraPersona
{
    [AddComponentMenu("SUPER Character/CameraPro")]
    public class CamaraBahaviour : MonoBehaviour
    {
        #region Variables
        public GameObject posCamera;
        private JuanMoveBehaviour moveBehaviour;


        #region Camera Settings
        [Header("Camera Settings")]
        //
        //Public
        //
        //Both
        public Camera playerCamera;
        public bool enableCameraControl = true, lockAndHideMouse = true, drawPrimitiveUI = false;
        public PerspectiveModes cameraPerspective = PerspectiveModes._3rdPerson;
        //use mouse wheel to switch modes. (too close will set it to fps mode and attempting to zoom out from fps will switch to tps mode)
        public bool automaticallySwitchPerspective = true;

        public MouseInputInversionModes mouseInputInversion;
        public float Sensitivity = 8;
        public float rotationWeight = 4;
        public float verticalRotationRange = 170.0f;
        public float standingEyeHeight = 0.8f;
        public float crouchingEyeHeight = 0.25f;

        //First person
        public ViewInputModes viewInputMethods;
        public float FOVKickAmount = 10;
        public float FOVSensitivityMultiplier = 0.74f;

        //Third Person
        public bool rotateCharacterToCameraForward = false;
        public float maxCameraDistance = 8;
        public LayerMask cameraObstructionIgnore = -1;
        public float cameraZoomSensitivity = 5;
        public float bodyCatchupSpeed = 2.5f;
        public float inputResponseFiltering = 2.5f;

        //
        //Internal
        //

        //Both
        Vector2 MouseXY;
        Vector2 viewRotVelRef;
        bool isInFirstPerson, isInThirdPerson, perspecTog;
        bool setInitialRot = true;
        Vector3 initialRot;
        Image stamMeter, stamMeterBG;
        Image statsPanel, statsPanelBG;
        Image HealthMeter, HydrationMeter, HungerMeter;
        Vector2 normalMeterSizeDelta = new Vector2(175, 12), normalStamMeterSizeDelta = new Vector2(330, 5);
        public float internalEyeHeight;

        //First Person
        float initialCameraFOV, FOVKickVelRef, currentFOVMod;

        //Third Person
        float mouseScrollWheel, maxCameraDistInternal, currentCameraZ, cameraZRef;
        public Vector3 headPos, headRot, currentCameraPos, cameraPosVelRef;
        Quaternion quatHeadRot;
        Ray cameraObstCheck;
        RaycastHit cameraObstResult;
        //[Space(20)]
        #endregion

        #endregion
        // Start is called before the first frame update
        void Start()
        {
            moveBehaviour = GetComponent<JuanMoveBehaviour>();
            #region Camera
            maxCameraDistInternal = maxCameraDistance;
            initialCameraFOV = playerCamera.fieldOfView;
            internalEyeHeight = standingEyeHeight;
            if (lockAndHideMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (drawPrimitiveUI)
            {
                Canvas canvas = playerCamera.gameObject.GetComponentInChildren<Canvas>();
                if (canvas == null) { canvas = new GameObject("AutoCrosshair").AddComponent<Canvas>(); }
                canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = true;
                canvas.transform.SetParent(playerCamera.transform);
                canvas.transform.position = Vector3.zero;

                if (drawPrimitiveUI)
                {
                    //Stam Meter BG
                    stamMeterBG = new GameObject("Stam BG").AddComponent<Image>();
                    stamMeterBG.rectTransform.sizeDelta = normalStamMeterSizeDelta;
                    stamMeterBG.transform.SetParent(canvas.transform);
                    stamMeterBG.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    stamMeterBG.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    stamMeterBG.rectTransform.anchoredPosition = new Vector2(0, 22);
                    stamMeterBG.color = Color.gray;
                    stamMeterBG.gameObject.SetActive(moveBehaviour.enableStaminaSystem);
                    //Stam Meter
                    stamMeter = new GameObject("Stam Meter").AddComponent<Image>();
                    stamMeter.rectTransform.sizeDelta = normalStamMeterSizeDelta;
                    stamMeter.transform.SetParent(canvas.transform);
                    stamMeter.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    stamMeter.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    stamMeter.rectTransform.anchoredPosition = new Vector2(0, 22);
                    stamMeter.color = Color.white;
                    stamMeter.gameObject.SetActive(moveBehaviour.enableStaminaSystem);

                }
            }
            initialRot = transform.localEulerAngles;
            #endregion
        }

        // Update is called once per frame
        void Update()
        {
            if (!moveBehaviour.estaMuerto)
            {
                if (!moveBehaviour.controllerPaused)
                {
                    #region Input
                    //camera
                    MouseXY.x = Input.GetAxis("Mouse Y");
                    MouseXY.y = Input.GetAxis("Mouse X");
                    mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                    //perspecTog = Input.GetKeyDown(perspectiveSwitchingKey_L);
                    //interactInput = Input.GetKeyDown(interactKey_L);
                    #endregion

                    if (!moveBehaviour.tengoBoleadoras)
                    {
                        #region Camera
                        if (enableCameraControl)
                        {
                            switch (cameraPerspective)
                            {
                                case PerspectiveModes._1stPerson:
                                    {
                                        //This is called in FixedUpdate for the 3rd person mode
                                        //RotateView(MouseXY, Sensitivity, rotationWeight);
                                        if (!isInFirstPerson) { ChangePerspective(PerspectiveModes._1stPerson); }
                                        if (perspecTog || (automaticallySwitchPerspective && mouseScrollWheel < 0)) { ChangePerspective(PerspectiveModes._3rdPerson); }
                                        //HeadbobCycleCalculator();
                                        FOVKick();
                                    }
                                    break;

                                case PerspectiveModes._3rdPerson:
                                    {
                                        //  UpdateCameraPosition_3rdPerson();
                                        if (!isInThirdPerson) { ChangePerspective(PerspectiveModes._3rdPerson); }
                                        if (perspecTog || (automaticallySwitchPerspective && maxCameraDistInternal == 0 && currentCameraZ == 0)) { ChangePerspective(PerspectiveModes._1stPerson); }
                                        maxCameraDistInternal = Mathf.Clamp(maxCameraDistInternal - (mouseScrollWheel * (cameraZoomSensitivity * 2)), automaticallySwitchPerspective ? 0 : (moveBehaviour.capsule.radius * 2), maxCameraDistance);
                                    }
                                    break;
                            }


                            if (setInitialRot)
                            {
                                setInitialRot = false;
                                RotateView(initialRot, false);
                                moveBehaviour.InputDir = transform.forward;
                            }
                        }
                        if (drawPrimitiveUI)
                        {
                            /* if (enableSurvivalStats)
                             {
                                 if (!statsPanel.gameObject.activeSelf) statsPanel.gameObject.SetActive(true);

                                 HealthMeter.rectTransform.sizeDelta = Vector2.Lerp(Vector2.up * 12, normalMeterSizeDelta, (currentSurvivalStats.Health / defaultSurvivalStats.Health));
                                 HydrationMeter.rectTransform.sizeDelta = Vector2.Lerp(Vector2.up * 12, normalMeterSizeDelta, (currentSurvivalStats.Hydration / defaultSurvivalStats.Hydration));
                                 HungerMeter.rectTransform.sizeDelta = Vector2.Lerp(Vector2.up * 12, normalMeterSizeDelta, (currentSurvivalStats.Hunger / defaultSurvivalStats.Hunger));
                             }
                             else
                             {
                                 if (statsPanel.gameObject.activeSelf) statsPanel.gameObject.SetActive(false);

                             } */
                            if (moveBehaviour.enableStaminaSystem)
                            {
                                if (!stamMeterBG.gameObject.activeSelf) stamMeterBG.gameObject.SetActive(true);
                                if (!stamMeter.gameObject.activeSelf) stamMeter.gameObject.SetActive(true);
                                if (moveBehaviour.staminaIsChanging)
                                {
                                    if (stamMeter.color != Color.white)
                                    {
                                        stamMeterBG.color = Vector4.MoveTowards(stamMeterBG.color, new Vector4(0, 0, 0, 0.5f), 0.15f);
                                        stamMeter.color = Vector4.MoveTowards(stamMeter.color, new Vector4(1, 1, 1, 1), 0.15f);
                                    }
                                    stamMeter.rectTransform.sizeDelta = Vector2.Lerp(Vector2.up * 5, normalStamMeterSizeDelta, (moveBehaviour.currentStaminaLevel / moveBehaviour.Stamina));
                                }
                                else
                                {
                                    if (stamMeter.color != Color.clear)
                                    {
                                        stamMeterBG.color = Vector4.MoveTowards(stamMeterBG.color, new Vector4(0, 0, 0, 0), 0.15f);
                                        stamMeter.color = Vector4.MoveTowards(stamMeter.color, new Vector4(0, 0, 0, 0), 0.15f);
                                    }
                                }
                            }
                            else
                            {
                                if (stamMeterBG.gameObject.activeSelf) stamMeterBG.gameObject.SetActive(false);
                                if (stamMeter.gameObject.activeSelf) stamMeter.gameObject.SetActive(false);
                            }
                        }

                        if (moveBehaviour.currentStance == SUPERCharacte.Stances.Standing && !moveBehaviour.changingStances)
                        {
                            internalEyeHeight = standingEyeHeight;
                        }
                        #endregion

                    }
                }
            }
        }
        void FixedUpdate()
        {
            if (!moveBehaviour.controllerPaused && !moveBehaviour.estaMuerto)
            {
                if (!moveBehaviour.tengoBoleadoras)
                {
                    #region Camera
                    RotateView(MouseXY, Sensitivity, rotationWeight);
                    if (cameraPerspective == PerspectiveModes._3rdPerson)
                    {
                        UpdateBodyRotation_3rdPerson();
                        UpdateCameraPosition_3rdPerson();
                    }

                    #endregion
                }

            }
        }

        #region Camera Functions
        void RotateView(Vector2 yawPitchInput, float inputSensitivity, float cameraWeight)
        {

            switch (viewInputMethods)
            {

                case ViewInputModes.Traditional:
                    {
                        yawPitchInput.x *= ((mouseInputInversion == MouseInputInversionModes.X || mouseInputInversion == MouseInputInversionModes.Both) ? 1 : -1);
                        yawPitchInput.y *= ((mouseInputInversion == MouseInputInversionModes.Y || mouseInputInversion == MouseInputInversionModes.Both) ? -1 : 1);
                        float maxDelta = Mathf.Min(5, (26 - cameraWeight)) * 360;
                        switch (cameraPerspective)
                        {
                            case PerspectiveModes._1stPerson:
                                {
                                    Vector2 targetAngles = ((Vector2.right * playerCamera.transform.localEulerAngles.x) + (Vector2.up * moveBehaviour.p_Rigidbody.rotation.eulerAngles.y));
                                    float fovMod = FOVSensitivityMultiplier > 0 && playerCamera.fieldOfView <= initialCameraFOV ? ((initialCameraFOV - playerCamera.fieldOfView) * (FOVSensitivityMultiplier / 10)) + 1 : 1;
                                    targetAngles = Vector2.SmoothDamp(targetAngles, targetAngles + (yawPitchInput * (((inputSensitivity * 5) / fovMod))), ref viewRotVelRef, (Mathf.Pow(cameraWeight * fovMod, 2)) * Time.fixedDeltaTime, maxDelta, Time.fixedDeltaTime);

                                    targetAngles.x += targetAngles.x > 180 ? -360 : targetAngles.x < -180 ? 360 : 0;
                                    targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
                                    //playerCamera.transform.localEulerAngles = (Vector3.right * targetAngles.x) + (Vector3.forward * (enableHeadbob ? headbobCameraPosition.z : 0));
                                    playerCamera.transform.localEulerAngles = (Vector3.right * targetAngles.x) + (Vector3.forward);
                                    moveBehaviour.p_Rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * targetAngles.y));

                                    //p_Rigidbody.rotation = ;
                                    //transform.localEulerAngles = (Vector3.up*targetAngles.y);
                                }
                                break;

                            case PerspectiveModes._3rdPerson:
                                {

                                    headPos = transform.position + Vector3.up * standingEyeHeight;
                                    quatHeadRot = Quaternion.Euler(headRot);
                                    headRot = Vector3.SmoothDamp(headRot, headRot + ((Vector3)yawPitchInput * (inputSensitivity * 5)), ref cameraPosVelRef, (Mathf.Pow(cameraWeight, 2)) * Time.fixedDeltaTime, maxDelta, Time.fixedDeltaTime);
                                    headRot.y += headRot.y > 180 ? -360 : headRot.y < -180 ? 360 : 0;
                                    headRot.x += headRot.x > 180 ? -360 : headRot.x < -180 ? 360 : 0;
                                    headRot.x = Mathf.Clamp(headRot.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);


                                }
                                break;

                        }

                    }
                    break;

                case ViewInputModes.Retro:
                    {
                        yawPitchInput = Vector2.up * (Input.GetAxis("Horizontal") * ((mouseInputInversion == MouseInputInversionModes.Y || mouseInputInversion == MouseInputInversionModes.Both) ? -1 : 1));
                        Vector2 targetAngles = ((Vector2.right * playerCamera.transform.localEulerAngles.x) + (Vector2.up * transform.localEulerAngles.y));
                        float fovMod = FOVSensitivityMultiplier > 0 && playerCamera.fieldOfView <= initialCameraFOV ? ((initialCameraFOV - playerCamera.fieldOfView) * (FOVSensitivityMultiplier / 10)) + 1 : 1;
                        targetAngles = targetAngles + (yawPitchInput * ((inputSensitivity / fovMod)));
                        targetAngles.x = 0;
                        //playerCamera.transform.localEulerAngles = (Vector3.right * targetAngles.x) + (Vector3.forward * (enableHeadbob ? headbobCameraPosition.z : 0));
                        playerCamera.transform.localEulerAngles = (Vector3.right * targetAngles.x) + (Vector3.forward);
                        transform.localEulerAngles = (Vector3.up * targetAngles.y);
                    }
                    break;
            }

        }
        public void RotateView(Vector3 AbsoluteEulerAngles, bool SmoothRotation)
        {

            switch (cameraPerspective)
            {

                case (PerspectiveModes._1stPerson):
                    {
                        AbsoluteEulerAngles.x += AbsoluteEulerAngles.x > 180 ? -360 : AbsoluteEulerAngles.x < -180 ? 360 : 0;
                        AbsoluteEulerAngles.x = Mathf.Clamp(AbsoluteEulerAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);


                        if (SmoothRotation)
                        {
                            IEnumerator SmoothRot()
                            {
                                //doingCamInterp = true;
                                Vector3 refVec = Vector3.zero, targetAngles = (Vector3.right * playerCamera.transform.localEulerAngles.x) + Vector3.up * transform.eulerAngles.y;
                                while (Vector3.Distance(targetAngles, AbsoluteEulerAngles) > 0.1f)
                                {
                                    targetAngles = Vector3.SmoothDamp(targetAngles, AbsoluteEulerAngles, ref refVec, 25 * Time.deltaTime);
                                    targetAngles.x += targetAngles.x > 180 ? -360 : targetAngles.x < -180 ? 360 : 0;
                                    targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
                                    playerCamera.transform.localEulerAngles = Vector3.right * targetAngles.x;
                                    transform.eulerAngles = Vector3.up * targetAngles.y;
                                    yield return null;
                                }
                                //doingCamInterp = false;
                            }
                            StopCoroutine("SmoothRot");
                            StartCoroutine(SmoothRot());
                        }
                        else
                        {
                            playerCamera.transform.eulerAngles = Vector3.right * AbsoluteEulerAngles.x;
                            transform.eulerAngles = (Vector3.up * AbsoluteEulerAngles.y) + (Vector3.forward * AbsoluteEulerAngles.z);
                        }
                    }
                    break;

                case (PerspectiveModes._3rdPerson):
                    {
                        if (SmoothRotation)
                        {
                            AbsoluteEulerAngles.y += AbsoluteEulerAngles.y > 180 ? -360 : AbsoluteEulerAngles.y < -180 ? 360 : 0;
                            AbsoluteEulerAngles.x += AbsoluteEulerAngles.x > 180 ? -360 : AbsoluteEulerAngles.x < -180 ? 360 : 0;
                            AbsoluteEulerAngles.x = Mathf.Clamp(AbsoluteEulerAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
                            IEnumerator SmoothRot()
                            {
                                //doingCamInterp = true;
                                Vector3 refVec = Vector3.zero;
                                while (Vector3.Distance(headRot, AbsoluteEulerAngles) > 0.1f)
                                {
                                    headPos = moveBehaviour.p_Rigidbody.position + Vector3.up * standingEyeHeight;
                                    quatHeadRot = Quaternion.Euler(headRot);
                                    headRot = Vector3.SmoothDamp(headRot, AbsoluteEulerAngles, ref refVec, 25 * Time.deltaTime);
                                    headRot.y += headRot.y > 180 ? -360 : headRot.y < -180 ? 360 : 0;
                                    headRot.x += headRot.x > 180 ? -360 : headRot.x < -180 ? 360 : 0;
                                    headRot.x = Mathf.Clamp(headRot.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
                                    yield return null;
                                }
                                //doingCamInterp = false;
                            }
                            StopCoroutine("SmoothRot");
                            StartCoroutine(SmoothRot());
                        }
                        else
                        {
                            headRot = AbsoluteEulerAngles;
                            headRot.y += headRot.y > 180 ? -360 : headRot.y < -180 ? 360 : 0;
                            headRot.x += headRot.x > 180 ? -360 : headRot.x < -180 ? 360 : 0;
                            headRot.x = Mathf.Clamp(headRot.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
                            quatHeadRot = Quaternion.Euler(headRot);
                            //if (doingCamInterp) { }
                        }
                    }
                    break;
            }
        }
        public void ChangePerspective(PerspectiveModes newPerspective = PerspectiveModes._1stPerson)
        {
            switch (newPerspective)
            {
                case PerspectiveModes._1stPerson:
                    {
                        StopCoroutine("SmoothRot");
                        isInThirdPerson = false;
                        isInFirstPerson = true;
                        transform.eulerAngles = Vector3.up * headRot.y;
                        playerCamera.transform.localPosition = Vector3.up * standingEyeHeight;
                        playerCamera.transform.localEulerAngles = (Vector2)playerCamera.transform.localEulerAngles;
                        cameraPerspective = newPerspective;
                        if (moveBehaviour._3rdPersonCharacterAnimator)
                        {
                            moveBehaviour._3rdPersonCharacterAnimator.gameObject.SetActive(false);
                        }
                        /*if (_1stPersonCharacterAnimator)
                        {
                            _1stPersonCharacterAnimator.gameObject.SetActive(true);
                        }*/
                        /*if (crosshairImg && autoGenerateCrosshair)
                        {
                            crosshairImg.gameObject.SetActive(true);
                        }*/
                    }
                    break;

                case PerspectiveModes._3rdPerson:
                    {
                        StopCoroutine("SmoothRot");
                        isInThirdPerson = true;
                        isInFirstPerson = false;
                        playerCamera.fieldOfView = initialCameraFOV;
                        maxCameraDistInternal = maxCameraDistInternal == 0 ? moveBehaviour.capsule.radius * 2 : maxCameraDistInternal;
                        currentCameraZ = -(maxCameraDistInternal * 0.85f);
                        playerCamera.transform.localEulerAngles = (Vector2)playerCamera.transform.localEulerAngles;
                        headRot.y = transform.eulerAngles.y;
                        headRot.x = playerCamera.transform.eulerAngles.x;
                        cameraPerspective = newPerspective;
                        if (moveBehaviour._3rdPersonCharacterAnimator)
                        {
                            moveBehaviour._3rdPersonCharacterAnimator.gameObject.SetActive(true);
                        }
                        /*if (_1stPersonCharacterAnimator)
                        {
                            _1stPersonCharacterAnimator.gameObject.SetActive(false);
                        }*/
                        /*if (crosshairImg && autoGenerateCrosshair)
                        {
                            if (!showCrosshairIn3rdPerson)
                            {
                                crosshairImg.gameObject.SetActive(false);
                            }
                            else
                            {
                                crosshairImg.gameObject.SetActive(true);
                            }
                        }*/
                    }
                    break;
            }
        }
        void FOVKick()
        {
            if (cameraPerspective == PerspectiveModes._1stPerson && FOVKickAmount > 0)
            {
                currentFOVMod = (!moveBehaviour.isIdle && moveBehaviour.isSprinting) ? initialCameraFOV + (FOVKickAmount * ((moveBehaviour.sprintingSpeed / moveBehaviour.walkingSpeed) - 1)) : initialCameraFOV;
                if (!Mathf.Approximately(playerCamera.fieldOfView, currentFOVMod) && playerCamera.fieldOfView >= initialCameraFOV)
                {
                    playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, currentFOVMod, ref FOVKickVelRef, Time.deltaTime, 50);
                }
            }
        }
        void UpdateCameraPosition_3rdPerson()
        {

            //Camera Obstacle Check
            cameraObstCheck = new Ray(headPos + (quatHeadRot * (Vector3.forward * moveBehaviour.capsule.radius)), quatHeadRot * -Vector3.forward);
            if (Physics.SphereCast(cameraObstCheck, 0.5f, out cameraObstResult, maxCameraDistInternal, cameraObstructionIgnore, QueryTriggerInteraction.Ignore))
            {
                currentCameraZ = -(Vector3.Distance(headPos, cameraObstResult.point) * 0.9f);

            }
            else
            {
                currentCameraZ = Mathf.SmoothDamp(currentCameraZ, -(maxCameraDistInternal * 0.85f), ref cameraZRef, Time.deltaTime, 10, Time.fixedDeltaTime);
            }

            //Debugging
            if (moveBehaviour.enableMouseAndCameraDebugging)
            {
                Debug.Log(headRot);
                Debug.DrawRay(cameraObstCheck.origin, cameraObstCheck.direction * maxCameraDistance, Color.red);
                Debug.DrawRay(cameraObstCheck.origin, cameraObstCheck.direction * -currentCameraZ, Color.green);
            }
            currentCameraPos = headPos + (quatHeadRot * (Vector3.forward * currentCameraZ));
            playerCamera.transform.position = currentCameraPos;
            playerCamera.transform.rotation = quatHeadRot;
        }

        void UpdateBodyRotation_3rdPerson()
        {
            //if is moving, rotate capsule to match camera forward   //change button down to bool of isFiring or isTargeting
            if (!moveBehaviour.isIdle && !moveBehaviour.isSliding && moveBehaviour.currentGroundInfo.isGettingGroundInfo)
            {
                transform.rotation = (Quaternion.Euler(0, Mathf.MoveTowardsAngle(moveBehaviour.p_Rigidbody.rotation.eulerAngles.y, (Mathf.Atan2(moveBehaviour.InputDir.x, moveBehaviour.InputDir.z) * Mathf.Rad2Deg), 10), 0));
                //transform.rotation = Quaternion.Euler(0,Mathf.MoveTowardsAngle(transform.eulerAngles.y,(Mathf.Atan2(InputDir.x,InputDir.z)*Mathf.Rad2Deg),2.5f), 0);
            }
            else if (moveBehaviour.isSliding)
            {
                transform.localRotation = (Quaternion.Euler(Vector3.up * Mathf.MoveTowardsAngle(moveBehaviour.p_Rigidbody.rotation.eulerAngles.y, (Mathf.Atan2(moveBehaviour.p_Rigidbody.velocity.x, moveBehaviour.p_Rigidbody.velocity.z) * Mathf.Rad2Deg), 10)));
            }
            else if (!moveBehaviour.currentGroundInfo.isGettingGroundInfo && rotateCharacterToCameraForward)
            {
                transform.localRotation = (Quaternion.Euler(Vector3.up * Mathf.MoveTowardsAngle(moveBehaviour.p_Rigidbody.rotation.eulerAngles.y, headRot.y, 10)));
            }
        }
        #endregion
    }

    #region Classes and Enums
    public enum PerspectiveModes { _1stPerson, _3rdPerson }
    public enum ViewInputModes { Traditional, Retro }
    public enum MouseInputInversionModes { None, X, Y, Both }
    #endregion

    #region Editor Scripting
#if UNITY_EDITOR
    [CustomEditor(typeof(CamaraBahaviour))]
    public class CameraFPEditor : Editor
    {
        GUIStyle labelHeaderStyle;
        GUIStyle ShowMoreStyle;
        GUIStyle BoxPanel;
        Texture2D BoxPanelColor;

        CamaraBahaviour t;
        SerializedObject tSO;
        SerializedProperty interactableLayer, obstructionMaskField, groundLayerMask, groundMatProf, defaultSurvivalStats, currentSurvivalStats;
        static bool cameraSettingsFoldout = false;

        GUIStyle l_scriptHeaderStyle;
        GUIStyle labelSubHeaderStyle;
        GUIStyle clipSetLabelStyle;
        GUIStyle SupportButtonStyle;
        SerializedObject SurvivalStatsTSO;



        public void OnEnable()
        {
            t = (CamaraBahaviour)target;
            tSO = new SerializedObject(t);
            obstructionMaskField = tSO.FindProperty("cameraObstructionIgnore");
            groundLayerMask = tSO.FindProperty("whatIsGround");
            groundMatProf = tSO.FindProperty("footstepSoundSet");
            interactableLayer = tSO.FindProperty("interactableLayer");
            BoxPanelColor = new Texture2D(1, 1, TextureFormat.RGBAFloat, false); ;
            BoxPanelColor.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.2f));
            BoxPanelColor.Apply();

            SurvivalStatsTSO = new SerializedObject(t);
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


            t.posCamera = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Posicion Camara(conBoleadoras)",
                "Referencia a la ubicación en la que estara la cámara CUANDO esté equipada las boleadoras"), t.posCamera, typeof(GameObject), true);

            #region Camera Settings
            GUILayout.Label("Camera Settings", labelHeaderStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);
            t.enableCameraControl = EditorGUILayout.ToggleLeft(new GUIContent("Habilitar control de cámara", "¿Debería el jugador tener control sobre la cámara?"), t.enableCameraControl);
            t.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Player Camera", "La cámara adjunta al jugador."), t.playerCamera, typeof(Camera), true);
            //t.cameraPerspective = (PerspectiveModes)EditorGUILayout.EnumPopup(new GUIContent("Camera Perspective Mode", "The current perspective of the character."), t.cameraPerspective);
            //if(t.cameraPerspective == PerspectiveModes._3rdPerson){EditorGUILayout.HelpBox("3rd Person perspective is currently very experimental. Bugs and other adverse effects may occur.",MessageType.Info);}

            //EditorGUI.indentLevel--;

            if (cameraSettingsFoldout)
            {
                t.automaticallySwitchPerspective = EditorGUILayout.ToggleLeft(new GUIContent("Cambiar perspectiva automáticamente", "¿Debería el modo de perspectiva de la cámara cambiar automáticamente según la distancia entre la cámara y la cabeza del personaje?"), t.automaticallySwitchPerspective);
                /*#if ENABLE_INPUT_SYSTEM
                            t.perspectiveSwitchingKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Perspective Switch Key", "The keyboard key used to switch perspective modes. Set to none if you do not wish to allow perspective switching"),t.perspectiveSwitchingKey);
                #else
                                if (!t.automaticallySwitchPerspective) { t.perspectiveSwitchingKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Perspective Switch Key", "The keyboard key used to switch perspective modes. Set to none if you do not wish to allow perspective switching"), t.perspectiveSwitchingKey_L); }
                #endif*/
                t.mouseInputInversion = (MouseInputInversionModes)EditorGUILayout.EnumPopup(new GUIContent("Mouse Input Inversion", "¿Qué ejes de la entrada del mouse deberían invertirse, si corresponde?"), t.mouseInputInversion);
                t.Sensitivity = EditorGUILayout.Slider(new GUIContent("Mouse Sensitivity", "Sensitivity of the mouse"), t.Sensitivity, 1, 20);
                t.rotationWeight = EditorGUILayout.Slider(new GUIContent("Camera Weight", "¿Qué tan pesada debe sentirse la cámara?"), t.rotationWeight, 1, 25);
                t.verticalRotationRange = EditorGUILayout.Slider(new GUIContent("Vertical Rotation Range", "The vertical angle range (In degrees) that the camera is allowed to move in \n El rango de ángulo vertical (en grados) en el que la cámara puede moverse"), t.verticalRotationRange, 1, 180);

                t.lockAndHideMouse = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide mouse Cursor", "Should the controller lock and hide the cursor?\n ¿Debería el controlador bloquear y ocultar el cursor?"), t.lockAndHideMouse);
                /*t.autoGenerateCrosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Generate Crosshair", "Should the controller automatically generate a crosshair?"), t.autoGenerateCrosshair);
                GUI.enabled = t.autoGenerateCrosshair;
                t.crosshairSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Crosshair Sprite", "The Sprite the controller will use when generating a crosshair."), t.crosshairSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                t.showCrosshairIn3rdPerson = EditorGUILayout.ToggleLeft(new GUIContent("Show Crosshair in 3rd person?", "Should the controller show the crosshair in 3rd person?"), t.showCrosshairIn3rdPerson);*/
                GUI.enabled = true;
                t.drawPrimitiveUI = EditorGUILayout.ToggleLeft(new GUIContent("Draw Primitive UI", "Should the controller automatically generate and draw primitive stat UI?\n ¿Debería el controlador generar y dibujar automáticamente la interfaz de usuario de estadísticas primitivas?"), t.drawPrimitiveUI);
                EditorGUILayout.Space(20);

                if (t.cameraPerspective == PerspectiveModes._1stPerson)
                {
                    t.viewInputMethods = (ViewInputModes)EditorGUILayout.EnumPopup(new GUIContent("Camera Input Methods",
                        "The input method used to rotate the camera.\n El método de entrada utilizado para rotar la cámara."), t.viewInputMethods);
                    t.standingEyeHeight = EditorGUILayout.Slider(new GUIContent("Standing Eye Height",
                        "The Eye height of the player measured from the center of the character's capsule and upwards.\n La altura de los ojos del jugador medida desde el centro de la cápsula del personaje y hacia arriba."), t.standingEyeHeight, 0, 1);
                    t.crouchingEyeHeight = EditorGUILayout.Slider(new GUIContent("Crouching Eye Height \n Altura de los ojos en cuclillas",
                        "The Eye height of the player measured from the center of the character's capsule and upwards."), t.crouchingEyeHeight, 0, 1);
                    t.FOVKickAmount = EditorGUILayout.Slider(new GUIContent("FOV Kick Amount",
                        "How much should the camera's FOV change based on the current movement speed? \n ¿Cuánto debería cambiar el campo de visión de la cámara en función de la velocidad de movimiento actual?"), t.FOVKickAmount, 0, 50);
                    t.FOVSensitivityMultiplier = EditorGUILayout.Slider(new GUIContent("FOV Sensitivity Multiplier",
                        "How much should the camera's FOV effect the mouse sensitivity? (Lower FOV = less sensitive) \n ¿Cuánto debería afectar el campo de visión de la cámara a la sensibilidad del mouse? (FOV más bajo = menos sensible)"), t.FOVSensitivityMultiplier, 0, 1);
                }
                else
                {
                    t.rotateCharacterToCameraForward = EditorGUILayout.ToggleLeft(new GUIContent("Rotate Ungrounded Character to Camera Forward", "Should the character get rotated towards the camera's forward facing direction when mid air? \n ¿Debería girarse el personaje hacia la dirección frontal de la cámara cuando está en el aire?"), t.rotateCharacterToCameraForward);
                    t.standingEyeHeight = EditorGUILayout.Slider(new GUIContent("Head Height", "The Head height of the player measured from the center of the character's capsule and upwards. \n La altura de la cabeza del jugador medida desde el centro de la cápsula del personaje hacia arriba."), t.standingEyeHeight, 0, 1);
                    t.maxCameraDistance = EditorGUILayout.Slider(new GUIContent("Max Camera Distance", "The farthest distance the camera is allowed to hover from the character's head \n La distancia más alejada que la cámara puede flotar desde la cabeza del personaje."), t.maxCameraDistance, 0, 15);
                    t.cameraZoomSensitivity = EditorGUILayout.Slider(new GUIContent("Camera Zoom Sensitivity", "How sensitive should the mouse scroll wheel be when zooming the camera in and out? \n ¿Qué tan sensible debe ser la rueda de desplazamiento del mouse al acercar y alejar la cámara?"), t.cameraZoomSensitivity, 1, 5);
                    t.bodyCatchupSpeed = EditorGUILayout.Slider(new GUIContent("Body Mesh Alignment Speed", "How quickly will the body align itself with the camera's relative direction \n ¿Qué tan rápido se alineará el cuerpo con la dirección relativa de la cámara?"), t.bodyCatchupSpeed, 0, 5);
                    t.inputResponseFiltering = EditorGUILayout.Slider(new GUIContent("Input Response Filtering", "How quickly will the internal input direction align itself the player's input \n ¿Qué tan rápido se alineará la dirección de entrada interna con la entrada del jugador?"), t.inputResponseFiltering, 0, 5);
                    EditorGUILayout.PropertyField(obstructionMaskField, new GUIContent("Camera Obstruction Layers", "The Layers the camera will register as an obstruction and move in front of. \n Las capas que la cámara registrará como una obstrucción y se moverá frente a ellas."));
                }
            }
            EditorGUILayout.Space();
            cameraSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(cameraSettingsFoldout, cameraSettingsFoldout ? "<color=#B83C82>show less</color>" : "<color=#B83C82>show more</color>", ShowMoreStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
            if (GUI.changed) { EditorUtility.SetDirty(t); Undo.RecordObject(t, "Undo Camera Setting changes"); tSO.ApplyModifiedProperties(); }
            #endregion

            EditorGUILayout.Space(); EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxHeight(6)); EditorGUILayout.Space();

        }
    }
#endif
    #endregion
}
