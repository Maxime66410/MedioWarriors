using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MalbersAnimations.HAP
{
    [AddComponentMenu("Malbers/Riding/Mount")]
    public class Mount : MonoBehaviour, IAnimatorListener
    {
        #region Components
        protected MRider _rider;                 //Rider's Animator to control both Sync animators from here

        /// <summary>Input for the Mount</summary>
        public IInputSource MountInput { get; private set; }
        public bool debug;
        #endregion

        #region General
        /// <summary>Enable Disable the Mount Logic</summary>
        public BoolReference active = new BoolReference(true);

        /// <summary>Works for the ID of the Mount (EX Wagon</summary>
        public IntReference ID;


        /// <summary>if true then it will ignore the Mounting Animations</summary>
        public BoolReference instantMount = new BoolReference(false);
        public string mountIdle = "Idle";

        /// <summary>The Rider can only Mount when the Animal is on any of these states on the list</summary>
        public bool MountOnly;
        /// <summary>The Rider can only Dismount when the Animal is on any of these states on the list</summary>
        public bool DismountOnly;
        /// <summary>The Rider is Forced to dismount if the animal is on any of these states</summary>
        public bool ForceDismount;
        public List<StateID> MountOnlyStates = new List<StateID>();
        public List<StateID> DismountOnlyStates = new List<StateID>();
        public List<StateID> ForceDismountStates = new List<StateID>();

        /// <summary>Reference for the Animator Update Mode</summary>
        public AnimatorUpdateMode DefaultAnimUpdateMode { get; set; }

        //If the Animal have been mounted  
        /// <summary>There's a Rider Inside the MountTriggers </summary>
        internal bool NearbyRider;
        #endregion

        #region Straight Mount
        public BoolReference straightSpine;                              //Activate this only for other animals but the horse 

        public Vector3 pointOffset = new Vector3(0, 0, 3);
        public Vector3 MonturaSpineOffset => transform.TransformPoint(pointOffset);

        //public float LowLimit = 45;
        //public float HighLimit = 135;

        public float smoothSM = 0.5f;

      //  public  Quaternion straightRotation { get; set; }
      //  private float SP_Weight;
        #endregion

        #region Animator Stuff

        /// <summary>If both rider and animal animator should be synced on the Locomotion state..</summary>
        public bool syncAnimators = true;

        public bool DebugSync = false;

        #endregion
 
        #region Events
        public UnityEvent OnMounted = new UnityEvent();
        public UnityEvent OnDismounted = new UnityEvent();
        public BoolEvent OnCanBeMounted = new BoolEvent();

        /// <summary>Velocity changes for diferent Animation Speeds... used on other animals</summary>
        public List<SpeedTimeMultiplier> SpeedMultipliers;
        #endregion

        #region Properties
        public Transform MountPoint;     // Reference for the RidersLink Bone  
        public Transform FootLeftIK;     // Reference for the LeftFoot correct position on the mount
        public Transform FootRightIK;    // Reference for the RightFoot correct position on the mount
        public Transform KneeLeftIK;     // Reference for the LeftKnee correct position on the mount
        public Transform KneeRightIK;    // Reference for the RightKnee correct position on the mount

        /// <summary>Straighen the Spine bone while mounted depends on the Mount</summary>
        public bool StraightSpine
        {
            get { return straightSpine; }
            set { straightSpine.Value = value; }
        }

        private bool defaultStraightSpine;

        public Animator Anim { get; private set; }  //Reference for the Animator 

        public List<MountTriggers> MountTriggers { get; private set; }
 

        protected bool mounted;
        /// <summary> Is the animal Mounted</summary>
        public bool Mounted
        {
            set
            {
                if (value != mounted)
                {
                    mounted = value;
                    if (mounted)
                        OnMounted.Invoke();    //Invoke the Event
                    else OnDismounted.Invoke();
                }
            }

            get { return mounted; }
        }

        /// <summary>Reference of the Animal</summary>
        public MAnimal Animal;

        /// <summary> Dismount only when the Animal is Still on place </summary>
        public virtual bool CanDismount
        {
            get { return Mounted; }
        }

        public virtual string MountIdle
        {
            get { return mountIdle; }
            set { mountIdle = value; }
        }

        /// <summary>Animal Mountable Script 'Enabled/Disabled'</summary>
        public virtual bool CanBeMounted
        {
            get { return active; }
            set { active.Value = value; }
        }


        /// <summary>If "Mount Only" is enabled, this will capture the State the animal is at, in order to Mount</summary>
        public bool CanBeMountedByState { get; set; }
        /// <summary>If "Mount Only" is enabled, this will capture the State the animal is at, in order to Mount</summary>
        public bool CanBeDismountedByState { get; set; }

        /// <summary>Active Ride the Montura. is setted by the Rider Script </summary>
        public MRider Rider
        {
            get { return _rider; }
            set { _rider = value; }
        }


        /// <summary> Ignore Mounting Animations </summary>
        public bool InstantMount
        {
            get { return instantMount; }
            set { instantMount.Value = value; }
        }
        #endregion


        /// <summary>Enable the Input for the Mount</summary>
        public virtual void EnableInput(bool value)
        {
            MountInput?.Enable(value);
            Animal?.StopMoving();
        }

        void OnEnable()
        {
            Animal?.OnSpeedChange.AddListener(SetAnimatorSpeed);
            Animal?.OnStateChange.AddListener(AnimalStateChange);
        }

        void OnDisable()
        {
            Animal?.OnSpeedChange.RemoveListener(SetAnimatorSpeed);
            Animal?.OnStateChange.RemoveListener(AnimalStateChange);
        }

        private void Awake()
        {
            if (Animal == null) Animal = GetComponent<MAnimal>();
            Anim = Animal.GetComponent<Animator>();
            MountInput = Animal.GetComponent<IInputSource>();

            MountTriggers = GetComponentsInChildren<MountTriggers>(true).ToList(); //Catche all the MountTriggers of the Mount

            CanBeDismountedByState = CanBeMountedByState = true; //Set as true can be mounted and canbe dismounted by state
            defaultStraightSpine = StraightSpine;
           if (Anim) DefaultAnimUpdateMode = Anim.updateMode;
        }

        /// <summary>Used for Aiming while on the horse.... Straight Spine needs to be pause </summary>
        public void PauseStraightSpine(bool value)
        {
            StraightSpine = value ? false : defaultStraightSpine;
        }

        private void AnimalStateChange(int StateID)
        {
            var ActiveState = Animal.ActiveStateID;

            if (MountOnly)
            {
                CanBeMountedByState = MountOnlyStates.Contains(ActiveState);   //Set MountOnly by State
            }

            if (DismountOnly)
            {
                CanBeDismountedByState = DismountOnlyStates.Contains(ActiveState);   //Set DimountOnly by State
            }

            if (Rider)
            {
                Rider.UpdateCanMountDismount();

                if (ForceDismount) //Means the Rider is forced to dismount
                {
                    if (ForceDismountStates.Contains(ActiveState))
                        Rider.ForceDismount();
                }
            }
        }
         


        /// <summary>Align the Animator Speed with </summary>
        private void SetAnimatorSpeed(MSpeed SpeedModifier)
        {
            if (!Rider || !Rider.IsRiding) return;                            //if there's No Rider Skip

            if (UseSpeedModifiers)
            {
                var speed = SpeedMultipliers.Find(s => s.name == SpeedModifier.name);

                float TargetAnimSpeed = speed != null ? speed.AnimSpeed * SpeedModifier.animator * Animal.AnimatorSpeed : 1f;

                Rider.SpeedMultiplier = Mathf.Lerp(Rider.SpeedMultiplier, TargetAnimSpeed, Time.deltaTime * 5f);
            }
        }

        void Reset()
        {
            Animal = GetComponent<MAnimal>();
        }


        /// <summary>Enable/Disable the StraightMount Feature </summary>
        public virtual void StraightMount(bool value)
        {
            StraightSpine = value;
        }

        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }
         

        //private void SolveStraightMount()
        //{
        //    var Spine = Rider.Spine;

        //   // SP_Weight = Mathf.MoveTowards(SP_Weight, Rider.IsRiding ? 1 : 0, Time.deltaTime * smoothSM);

        //    if (Rider.IsRiding && StraightSpine)
        //    {
        //        SP_Weight = Mathf.MoveTowards(SP_Weight,1, Animal.DeltaTime * smoothSM);

        //        var TargetRotation = Quaternion.FromToRotation(MountPoint.up, Animal.UpVector) * MountPoint.rotation;       //Calculate the orientation to the Up Vector  

        //        float angle = Vector3.Angle(Vector3.up, MountPoint.forward);            //Check limits

        //        if (angle < LowLimit)
        //        {
        //            TargetRotation *= Quaternion.Euler(new Vector3(angle - LowLimit, 0));
        //        }
        //        else if (angle > HighLimit)
        //        {
        //            TargetRotation *= Quaternion.Euler(new Vector3(angle - HighLimit, 0));
        //        }


        //        TargetRotation *= PointOffset;

        //        straightRotation = Quaternion.Lerp(straightRotation, TargetRotation, SP_Weight);
        //    }
        //    else if (Rider.IsDismounting || !StraightSpine)
        //    {
        //        SP_Weight = Mathf.MoveTowards(SP_Weight, 0, Animal.DeltaTime * smoothSM);
        //        straightRotation = Quaternion.Lerp(Spine.rotation, straightRotation, SP_Weight);
        //    }

        //    if (SP_Weight != 0)
        //    {
        //        Spine.rotation = straightRotation;
        //    }
        //}



        //UnityEditor


        [HideInInspector] public bool ShowLinks = true;
        [HideInInspector] public bool UseSpeedModifiers;
        [HideInInspector] public bool ShowEvents;


#if UNITY_EDITOR
        /// <summary> Debug Options </summary>
        void OnDrawGizmos()
        {
            if (!debug) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(MonturaSpineOffset, 0.1f);

            if (DebugSync && Application.isPlaying && Rider != null && Rider.IsRiding)
            {

                if (headbone == null)
                    headbone = transform.FindGrandChild("Head");

//                if (Animal.AnimState == AnimTag.Locomotion || Animal.AnimState == AnimTag.Swim || Animal.AnimState == AnimTag.Fly)    //Search for sync the locomotion state on the animal
                {
                    Gizmos.color = ((int)Anim.GetCurrentAnimatorStateInfo(0).normalizedTime) % 2 == 0 ? Color.white : Color.yellow;
                    Gizmos.DrawSphere(headbone != null ? headbone.position + (Vector3.up * 0.2f * Animal.ScaleFactor) : transform.position, 0.05f * Animal.ScaleFactor);
                    Gizmos.DrawWireSphere(headbone != null ? headbone.position + (Vector3.up * 0.2f * Animal.ScaleFactor) : transform.position, 0.05f * Animal.ScaleFactor);

                    if (Rider.Anim)
                    {
                        AnimatorStateInfo RiderStateInfo = Rider.Anim.GetCurrentAnimatorStateInfo(Rider.MountLayerIndex);

                        Transform RiderHead = Rider.Anim.GetBoneTransform(HumanBodyBones.Head);

                        if ((RiderStateInfo.tagHash == AnimTag.Locomotion)
                           || (RiderStateInfo.tagHash == AnimTag.Swim)
                              || (RiderStateInfo.tagHash == AnimTag.Fly))  //Search for syncron the locomotion state on the animal
                        {
                            Gizmos.color = ((int)RiderStateInfo.normalizedTime) % 2 == 0 ? Color.white : Color.red;
                            Gizmos.DrawSphere(RiderHead.position + (Vector3.up * 0.2f), 0.05f);
                            Gizmos.DrawWireSphere(RiderHead.position + (Vector3.up * 0.2f), 0.05f);
                        }
                    }
                }
            }
        }
        [HideInInspector] Transform headbone;
#endif
    }

    [System.Serializable]
    public class SpeedTimeMultiplier
    {
        /// <summary>Name of the Speed the on the animal to apply the AnimSpeed</summary>
        public string name = "SpeedName";

        /// <summary>Speed Modifier multiplier for the Rider</summary>
        public float AnimSpeed = 1f;
    }
}