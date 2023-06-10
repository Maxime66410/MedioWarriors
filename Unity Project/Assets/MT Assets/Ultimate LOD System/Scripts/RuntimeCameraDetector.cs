#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
    /*
     This class is responsible for the functioning of the "Runtime Camera Detector" component, and all its functions.
    */
    /*
     * The Ultimate LOD System was developed by Marcos Tomaz in 2020.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [AddComponentMenu("")] //Hide this script in component menu.
    public class RuntimeCameraDetector : MonoBehaviour
    {
        //Private constantes
        private WaitForSecondsRealtime DELAY_BETWEEN_ARRAY_OF_CANERAS_UPDATE = new WaitForSecondsRealtime(0.50f);
        private WaitForSecondsRealtime DELAY_BETWEEN_CURRENT_CAMERA_DETECTOR = new WaitForSecondsRealtime(0.09f);

        //Private variables
        private Camera[] currentArrayOfCameras = new Camera[0];

#if UNITY_EDITOR
        //Public variables of Interface
        private bool gizmosOfThisComponentIsDisabled = false;

        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(RuntimeCameraDetector))]
        public class CustomInspector : UnityEditor.Editor
        {
            public bool DisableGizmosInSceneView(string scriptClassNameToDisable, bool isGizmosDisabled)
            {
                /*
                *  This method disables Gizmos in scene view, for this component
                */

                if (isGizmosDisabled == true)
                    return true;

                //Try to disable
                try
                {
                    //Get all data of Unity Gizmos manager window
                    var Annotation = System.Type.GetType("UnityEditor.Annotation, UnityEditor");
                    var ClassId = Annotation.GetField("classID");
                    var ScriptClass = Annotation.GetField("scriptClass");
                    var Flags = Annotation.GetField("flags");
                    var IconEnabled = Annotation.GetField("iconEnabled");

                    System.Type AnnotationUtility = System.Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
                    var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    //Scann all Gizmos of Unity, of this project
                    System.Array annotations = (System.Array)GetAnnotations.Invoke(null, null);
                    foreach (var a in annotations)
                    {
                        int classId = (int)ClassId.GetValue(a);
                        string scriptClass = (string)ScriptClass.GetValue(a);
                        int flags = (int)Flags.GetValue(a);
                        int iconEnabled = (int)IconEnabled.GetValue(a);

                        // this is done to ignore any built in types
                        if (string.IsNullOrEmpty(scriptClass))
                        {
                            continue;
                        }

                        const int HasIcon = 1;
                        bool hasIconFlag = (flags & HasIcon) == HasIcon;

                        //If the current gizmo is of the class desired, disable the gizmo in scene
                        if (scriptClass == scriptClassNameToDisable)
                        {
                            if (hasIconFlag && (iconEnabled != 0))
                            {
                                /*UnityEngine.Debug.LogWarning(string.Format("Script:'{0}' is not ment to show its icon in the scene view and will auto hide now. " +
                                    "Icon auto hide is checked on script recompile, if you'd like to change this please remove it from the config", scriptClass));*/
                                SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
                            }
                        }
                    }

                    return true;
                }
                //Catch any error
                catch (System.Exception exception)
                {
                    string exceptionOcurred = "";
                    exceptionOcurred = exception.Message;
                    if (exceptionOcurred != null)
                        exceptionOcurred = "";
                    return false;
                }
            }

            public Rect GetInspectorWindowSize()
            {
                //Returns the current size of inspector window
                return EditorGUILayout.GetControlRect(true, 0f);
            }

            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                RuntimeCameraDetector script = (RuntimeCameraDetector)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");
                script.gizmosOfThisComponentIsDisabled = DisableGizmosInSceneView("RuntimeCameraDetector", script.gizmosOfThisComponentIsDisabled);

                //Support reminder
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Remember to read the Ultimate LOD System documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);
                GUILayout.Space(10);

                UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene = (Camera)EditorGUILayout.ObjectField(new GUIContent("Current Camera Detected",
                                                                        "This is the camera that was detected as the current camera that is appearing on the screen, determined by the automatic camera detection algorithm, for this component. This camera will be accessed by all \"Ultimate Level Of Detail\" components that use the camera's detection mode as \"Current Camera\"."),
                                                                        UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene, typeof(Camera), true, GUILayout.Height(16));

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {

                }
            }
        }
        #endregion
#endif

        //Component private methods

        public void Awake()
        {
            //Get array of all cameras in this scene
            currentArrayOfCameras = Camera.allCameras;

            //Start the delayed array of cameras updater
            StartCoroutine(ArrayOfCamerasDelayedUpdater());

            //Start the current camera on screen detector
            StartCoroutine(CurrentCameraOnScreenDetector());
        }

        private IEnumerator ArrayOfCamerasDelayedUpdater()
        {
            //Update the array of cameras with a constant delay

            while (true == true)
            {
                //Wait the delay
                yield return DELAY_BETWEEN_ARRAY_OF_CANERAS_UPDATE;

                //Get array of all cameras in this scene
                currentArrayOfCameras = Camera.allCameras;
            }
        }

        private IEnumerator CurrentCameraOnScreenDetector()
        {
            //Update the current camera that is appearing on screen in this moment, information

            while (true == true)
            {
                //Id of camera to set that is on top of screen
                int idOfCurrentCameraOnTopOfScreen = -1;

                //Last checked camera info, in loop
                float lastCameraChecked_depth = -1000.0f;

                //Check each camera to find a camera that is on top of screen, according with rules of Unity
                for (int i = 0; i < currentArrayOfCameras.Length; i++)
                {
                    //If camera is rendering into a RenderTexture, ignore
                    if (currentArrayOfCameras[i].targetTexture != null)
                        continue;
                    //If camera is ortographic, ignore
                    if (currentArrayOfCameras[i].orthographic == true)
                        continue;
                    //If camera is not using total screen space, ignore
                    if (currentArrayOfCameras[i].rect.width != 1.0f || currentArrayOfCameras[i].rect.height != 1.0f)
                        continue;

                    //Notify if have one or more cameras with same level of depth
                    if (lastCameraChecked_depth == currentArrayOfCameras[i].depth)
                        Debug.LogError("Ultimate LOD System: There are one or more cameras active in this scene, which have the same depth level. This causes the camera detection algorithm that is currently appearing on the screen to not work. Please set different depth values for each camera that is active at the same time in your scene, or disable cameras that are not being used and leave only the cameras that are being used, enabled.");

                    //Save depth of this camera, and get id of this camera if depth is major than last depth of last camera checked
                    if (currentArrayOfCameras[i].depth > lastCameraChecked_depth)
                    {
                        idOfCurrentCameraOnTopOfScreen = i;
                        lastCameraChecked_depth = currentArrayOfCameras[i].depth;
                    }
                }

                //Set the current camera that is in screen
                if (idOfCurrentCameraOnTopOfScreen == -1)
                    UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene = null;
                if (idOfCurrentCameraOnTopOfScreen != -1)
                    UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene = currentArrayOfCameras[idOfCurrentCameraOnTopOfScreen];

                //Wait the delay
                yield return DELAY_BETWEEN_CURRENT_CAMERA_DETECTOR;
            }
        }
    }
}