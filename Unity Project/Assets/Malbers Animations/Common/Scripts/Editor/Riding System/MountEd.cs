using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.HAP
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Mount))]
    public class MountEd : Editor
    {
        Mount M;
        private MonoScript script;
        bool CallHelp;
        bool helpExperimental;
        bool helpEvents;
        SerializedProperty UseSpeedModifiers, syncAnimators, MountOnly, DismountOnly, active, mountIdle, instantMount, straightSpine, ID,
            pointOffset,Animal, /* HighLimit, LowLimit,*/ smoothSM, mountPoint, rightIK, rightKnee, leftIK, leftKnee, SpeedMultipliers, DebugSync, OnMounted,
            OnDismounted, OnCanBeMounted, MountOnlyStates, DismountOnlyStates, ForceDismountStates, ForceDismount, ShowLinks ,debug;
        private void OnEnable()
        {
            M = (Mount)target;
            script = MonoScript.FromMonoBehaviour(M);

            UseSpeedModifiers = serializedObject.FindProperty("UseSpeedModifiers");
            syncAnimators = serializedObject.FindProperty("syncAnimators");
            Animal = serializedObject.FindProperty("Animal");
            ShowLinks = serializedObject.FindProperty("ShowLinks");
            debug = serializedObject.FindProperty("debug");
            ID = serializedObject.FindProperty("ID");

            MountOnly = serializedObject.FindProperty("MountOnly");
            DismountOnly = serializedObject.FindProperty("DismountOnly");
            active = serializedObject.FindProperty("active");
            mountIdle = serializedObject.FindProperty("mountIdle");
            instantMount = serializedObject.FindProperty("instantMount");
            straightSpine = serializedObject.FindProperty("straightSpine");
            //HighLimit = serializedObject.FindProperty("HighLimit");
            //LowLimit = serializedObject.FindProperty("LowLimit");
            smoothSM = serializedObject.FindProperty("smoothSM");

            mountPoint = serializedObject.FindProperty("MountPoint");
            rightIK = serializedObject.FindProperty("FootRightIK");
            rightKnee = serializedObject.FindProperty("KneeRightIK");
            leftIK = serializedObject.FindProperty("FootLeftIK");
            leftKnee = serializedObject.FindProperty("KneeLeftIK");

            SpeedMultipliers = serializedObject.FindProperty("SpeedMultipliers");
            DebugSync = serializedObject.FindProperty("DebugSync");
            OnMounted = serializedObject.FindProperty("OnMounted");
            pointOffset = serializedObject.FindProperty("pointOffset");

            OnDismounted = serializedObject.FindProperty("OnDismounted");
            OnCanBeMounted = serializedObject.FindProperty("OnCanBeMounted");
            MountOnlyStates = serializedObject.FindProperty("MountOnlyStates");
            DismountOnlyStates = serializedObject.FindProperty("DismountOnlyStates");

            ForceDismountStates = serializedObject.FindProperty("ForceDismountStates");
            ForceDismount = serializedObject.FindProperty("ForceDismount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Makes this GameObject mountable. Need Mount Triggers and IK Goals");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(active, new GUIContent("Active", "If the animal can be mounted. Deactivate if the mount is death or destroyed or is not ready to be mountable"));
                        EditorGUILayout.PropertyField(Animal, new GUIContent("Animal", "Animal Reference for the Mounting System"));
                        EditorGUILayout.PropertyField(ID, new GUIContent("ID", "Default should be 0.... change this and the Stance parameter on the Rider will change to that value... alowing other types of mounts like Wagon"));
                        EditorGUILayout.PropertyField(instantMount, new GUIContent("Instant Mount", "Ignores the Mounting Animations"));
                        EditorGUILayout.PropertyField(mountIdle, new GUIContent("Mount Idle", "Animation to Play directly when instant mount is enabled"));
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.LabelField("Mount/Dismount States", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(MountOnly, new GUIContent("Mount Only", "The Rider can only Mount when the Animal is on any of these states"));

                        if (MountOnly.boolValue) MalbersEditor.Arrays(MountOnlyStates);

                        EditorGUILayout.PropertyField(DismountOnly, new GUIContent("Dismount Only", "The Rider can only Dismount when the Animal is on any of these states"));

                        if (DismountOnly.boolValue) MalbersEditor.Arrays(DismountOnlyStates);


                        EditorGUILayout.PropertyField(ForceDismount, new GUIContent("Force Dismount", "The Rider is forced to dismount when the Animal is on any of these states"));

                        if (ForceDismount.boolValue) MalbersEditor.Arrays(ForceDismountStates);
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(straightSpine, new GUIContent("Straight Spine", "Straighten the Mount Point to fix the Rider Animation"));

                        if (M.StraightSpine)
                        {
                            EditorGUILayout.PropertyField(pointOffset, new GUIContent("Point Offset", "Point in front of the Mount to Straight the Spine of the Rider"));
                            EditorGUILayout.PropertyField(smoothSM, new GUIContent("Smoothness", "Smooth changes between the rotation and the straight Mount"));
                          
                            //EditorGUILayout.PropertyField(pointOffset, new GUIContent("Point Offset", "Extra rotation for the Rider to fit the Rider on the correct position"));

                            //EditorGUILayout.PropertyField(HighLimit, new GUIContent("High Limit", "if the mount Up Vector messing with the mount Bone holder add some space between them"));
                            //EditorGUILayout.PropertyField(LowLimit, new GUIContent("Low Limit", "if the mount Up Vector messing with the mount Bone holder add some space between them"));

                        }

                        //EditorGUILayout.PropertyField(serializedObject.FindProperty("UseStraightAim"), new GUIContent("Use Straight Aim", ""));
                        //EditorGUILayout.PropertyField(serializedObject.FindProperty("AimOffset"), new GUIContent("Aim Offset", ""));
                    }
                    EditorGUILayout.EndVertical();




                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        {
                            ShowLinks.boolValue = EditorGUILayout.Foldout(ShowLinks.boolValue, "Links");
                            CallHelp = GUILayout.Toggle(CallHelp, "?", EditorStyles.miniButton, GUILayout.Width(18));
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;

                        if (ShowLinks.boolValue)
                        {
                            if (CallHelp) EditorGUILayout.HelpBox("'Mount Point' is obligatory, the rest are optional", MessageType.None);

                            EditorGUILayout.PropertyField(mountPoint, new GUIContent("Mount Point", "Reference for the Mount Point"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(rightIK, new GUIContent("Right Foot", "Reference for the Right Foot correct position on the mount"));
                            EditorGUILayout.PropertyField(rightKnee, new GUIContent("Right Knee", "Reference for the Right Knee correct position on the mount"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(leftIK, new GUIContent("Left Foot", "Reference for the Left Foot correct position on the mount"));
                            EditorGUILayout.PropertyField(leftKnee, new GUIContent("Left Knee", "Reference for the Left Knee correct position on the mount"));
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(syncAnimators, new GUIContent("Sync Animators", "Sync the Animal and the Riders Parameters on both Animators"));

                        if (syncAnimators.boolValue)
                        {

                            EditorGUILayout.BeginHorizontal();
                            {
                                UseSpeedModifiers.boolValue = EditorGUILayout.Toggle("Animator Speeds", UseSpeedModifiers.boolValue);
                                helpExperimental = GUILayout.Toggle(helpExperimental, "?", EditorStyles.miniButton, GUILayout.Width(18));
                            }
                            EditorGUILayout.EndHorizontal();


                            if (UseSpeedModifiers.boolValue)
                            {
                                if (helpExperimental) EditorGUILayout.HelpBox("Changes the Speed on the Rider's Animator to Sync with the Animal Animator.\nThe Original Riding Animatios are meant for the Horse. Only change the Speeds for other creatures", MessageType.None);

                                MalbersEditor.Arrays(SpeedMultipliers, new GUIContent("Animator Speed Multipliers", "Velocity changes for diferent Animation Speeds... used on other animals"));

                                EditorGUILayout.Space();

                                EditorGUILayout.PropertyField(DebugSync, new GUIContent("Debug Sync", ""));
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.indentLevel++;
                            M.ShowEvents = EditorGUILayout.Foldout(M.ShowEvents, "Events");
                            helpEvents = GUILayout.Toggle(helpEvents, "?", EditorStyles.miniButton, GUILayout.Width(18));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (M.ShowEvents)
                        {
                            if (helpEvents) EditorGUILayout.HelpBox("On Mounted: Invoked when the rider start to mount the animal\nOn Dismounted: Invoked when the rider start to dismount the animal\nInvoked when the Mountable has an available Rider Nearby", MessageType.None);

                            EditorGUILayout.PropertyField(OnMounted);
                            EditorGUILayout.PropertyField(OnDismounted);
                            EditorGUILayout.PropertyField(OnCanBeMounted);

                        }
                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.PropertyField(debug);
                EditorGUILayout.EndVertical();
                if (M.MountPoint == null)
                {
                    EditorGUILayout.HelpBox("'Mount Point'  is empty, please set a reference", MessageType.Warning);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Mount Inspector");
                //EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}