using UnityEngine;
using System.Collections;
using MalbersAnimations.HAP;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;

/// <summary>
/// This is the Link between Invector and Horse AnimSet Pro Riding System
/// </summary>
namespace Invector.CharacterController
{
    public class InvectorHAPLink : MonoBehaviour
    {
        [Header("Camera States")]
        public string ride = "Ride";                              //Camera State to change to the Ride State
        public string Default = "Default";                        //Camera State to change to the Default State
        public bool debug;
        protected MRider Rider;                                   //Reference for the Rider

        protected vThirdPersonCamera vCamera;                     //Reference for the Invector's Camera
        protected vThirdPersonController v_TCP;                    //Reference for the Invector's Controller
        protected vThirdPersonInput VInput;                       //Reference for the Invector's Input

        void Awake()
        {
            InitializeLink();
        }

        /// <summary>
        /// Gets all the References and Connects all the Events
        /// </summary>
        protected virtual void InitializeLink()
        {
            Rider = GetComponent<MRider>();                     //Gets the Rider
            v_TCP = GetComponent<vThirdPersonController>();             //Gets the Invector's Controller
            VInput = GetComponent<vThirdPersonInput>();                 //Gets the Invector's Input
            vCamera =  vThirdPersonCamera.instance;   //Get Invector Camera

            #region Add Listeners to the Events
            if (Rider)
            {
                Rider.OnStartMounting.AddListener(OnStartMounting);
                Rider.OnEndMounting.AddListener(OnEndMounting);

                Rider.OnStartDismounting.AddListener(OnStartDismounting);
                Rider.OnEndDismounting.AddListener(OnEndDismounting);
            }
            #endregion
        }

        void OnEnable()
        {
            if (Rider)
            {
                Rider.OnStartMounting.AddListener(OnStartMounting);
                Rider.OnEndMounting.AddListener(OnEndMounting);

                Rider.OnStartDismounting.AddListener(OnStartDismounting);
                Rider.OnEndDismounting.AddListener(OnEndDismounting);
            }
        }

        protected virtual void OnDisable()
        {
            if (Rider)
            {
                Rider.OnStartMounting.RemoveListener(OnStartMounting);
                Rider.OnEndMounting.RemoveListener(OnEndMounting);

                Rider.OnStartDismounting.RemoveListener(OnStartDismounting);
                Rider.OnEndDismounting.RemoveListener(OnEndDismounting);
            }

        }

        #region Event Listeners

        /// <summary>This will be invoked from the Event Rider.OnStartMounting</summary>
        public virtual void OnStartMounting()
        {
            if (VInput) VInput.SetLockBasicInput(true);         //Lock the Input since the horse is taking the command

            vCamera.RemoveLockTarget();                        //Unlock the Fixation if It has a Target
            CameraState(ride);                                 //Change Camera State to Ride
            TurnOffHUD();
            v_TCP.isSprinting = false;                          //This will Stop Draining the Stamina if my any chance you mount the horse while sprinting is on
            v_TCP.enabled = false;

            var HeadTrack = GetComponent<vHeadTrack>();
            if (HeadTrack) VInput.onLateUpdate -= HeadTrack.UpdateHeadTrack;
        }

        /// <summary> Turn off the HUD while Mounting </summary>
        public virtual void TurnOffHUD()
        {
            vTriggerGenericAction vtgA =
                ((MonoBehaviour)(Rider.Montura)).GetComponentInChildren<vTriggerGenericAction>();   //Find All 

            if (vtgA) vtgA.OnPlayerExit.Invoke(gameObject);                                                   //Turn off the hud when Mounting  
        }

        /// <summary>This will be invoked from the Event Rider.OnEndMounting</summary>
        public virtual void OnEndMounting()
        {
            if (!Rider.StartMounted)
            {
                VInput.SetLockBasicInput(true);                            //Make sure that the Invector's Input is Locked
                Rider.StartMounted.Value = false;
            }
            CameraState(ride);                                     //Change the camera State to "Ride"

            v_TCP.enabled = true;
        }

        /// <summary>This will be invoked from the Event Rider.OnStartDismounting</summary>
        public virtual void OnStartDismounting()
        {
            CameraState(Default);                                   //Change Camera State to default
            transform.rotation = Quaternion.FromToRotation(transform.up, -Physics.gravity)* transform.rotation;
        }

        /// <summary>This will be invoked from the Event Rider.OnEndDismounting </summary>
        public virtual void OnEndDismounting()
        {
            if (VInput) VInput.SetLockBasicInput(false);                    //Unlocks the Invector's Input

            var HeadTrack = GetComponent<vHeadTrack>();
            if (HeadTrack) VInput.onLateUpdate += HeadTrack.UpdateHeadTrack;

            Rider.Anim.updateMode = AnimatorUpdateMode.AnimatePhysics; //Force to be back on Update Physics

            VInput.changeCameraState = false;                        /// DEACTIVATE custom camera state on the controller after dismounting
        }
        #endregion

        /// <summary> Change the Invector Camera State</summary>
        public virtual void CameraState(string state)
        {
            VInput.smoothCameraState = true;        /// apply lerp transition between states
            VInput.customCameraState = state;       /// change the camera state to a new string
            VInput.changeCameraState = true;        /// Activate custom camera state on the controller
        }
    }
}
