#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

namespace MTAssets.UltimateLODSystem
{
    /*
     This class is responsible for the functioning of the "Ultimate Level Of Detail" component, and all its functions.
    */
    /*
     * The Ultimate LOD System was developed by Marcos Tomaz in 2020.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [ExecuteInEditMode]
    public class UltimateLevelOfDetail : MonoBehaviour
    {
        //Caches of script
        private Camera cacheOfLastActiveSceneViewCamera = null;
        private Camera cacheOfMainCamera = null;

        //Private variables from script
        private float lastDistanceFromMainCamera = -1f;
        private int currentLodAccordingToDistance = -1;
        private float currentDistanceFromMainCamera = 0f;
        private float currentRealDistanceFromMainCamera = 0f;
        private bool forcedToDisableLodsOfThisComponent = false;

        //Classes of script
        [HideInInspector]
        public enum ScanMeshesMode
        {
            ScanInChildrenGameObjectsOnly,
            ScanInThisGameObjectOnly
        }
        [HideInInspector]
        public enum ForceOfSimplification
        {
            Normal,
            Strong,
            VeryStrong,
            ExtremelyStrong,
            Destroyer
        }
        [HideInInspector]
        public enum CullingMode
        {
            Disabled,
            CullingMeshes,
            CullingMaterials
        }
        [HideInInspector]
        public enum CameraDetectionMode
        {
            CurrentCamera,
            MainCamera,
            CustomCamera
        }
        [System.Serializable]
        public class OriginalMeshItem
        {
            public GameObject originalGameObject;
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
            public Mesh originalMesh;
            public string meshLod1Path;
            public Mesh meshLod1;
            public string meshLod2Path;
            public Mesh meshLod2;
            public string meshLod3Path;
            public Mesh meshLod3;
            public string meshLod4Path;
            public Mesh meshLod4;
            public string meshLod5Path;
            public Mesh meshLod5;
            public string meshLod6Path;
            public Mesh meshLod6;
            public string meshLod7Path;
            public Mesh meshLod7;
            public string meshLod8Path;
            public Mesh meshLod8;
            public UltimateLevelOfDetailMeshes meshesManager;
            public Mesh beforeCullingData_lastMeshOfThis;
            public Material[] beforeCullingData_emptyMaterialArray = new Material[] { };
            public Material[] beforeCullingData_lastMaterialArray;
        }

        //Scan settings
        [HideInInspector]
        public ScanMeshesMode modeOfMeshesScanning = ScanMeshesMode.ScanInChildrenGameObjectsOnly;
        [HideInInspector]
        public bool scanInactiveGameObjects = false;

        //Meshes to ignore settings
        [HideInInspector]
        public List<GameObject> gameObjectsToIgnore = new List<GameObject>();

        //LOD settings
        [HideInInspector]
        public int levelsOfDetailToGenerate = 3;
        [HideInInspector]
        public float percentOfVerticesLod1 = 75f;
        [HideInInspector]
        public float percentOfVerticesLod2 = 55f;
        [HideInInspector]
        public float percentOfVerticesLod3 = 45f;
        [HideInInspector]
        public float percentOfVerticesLod4 = 35f;
        [HideInInspector]
        public float percentOfVerticesLod5 = 25f;
        [HideInInspector]
        public float percentOfVerticesLod6 = 15f;
        [HideInInspector]
        public float percentOfVerticesLod7 = 5f;
        [HideInInspector]
        public float percentOfVerticesLod8 = 1f;
        [HideInInspector]
        public bool saveGeneratedLodsInAssets = true;
        [HideInInspector]
        public bool skinnedAnimsCompatibilityMode = true;
        [HideInInspector]
        public bool preventArtifacts = true;
        [HideInInspector]
        public ForceOfSimplification forceOfSimplification = ForceOfSimplification.Normal;
        [HideInInspector]
        public CullingMode cullingMode = CullingMode.CullingMeshes;

        //Distance of view for each LOD
        [HideInInspector]
        public CameraDetectionMode cameraDetectionMode = CameraDetectionMode.CurrentCamera;
        [HideInInspector]
        public bool useCacheForMainCameraInDetection = true;
        [HideInInspector]
        public Camera customCameraForSimulationOfLods = null;
        [HideInInspector]
        public float minDistanceOfViewForLod1 = 30f;
        [HideInInspector]
        public float minDistanceOfViewForLod2 = 70f;
        [HideInInspector]
        public float minDistanceOfViewForLod3 = 120f;
        [HideInInspector]
        public float minDistanceOfViewForLod4 = 150f;
        [HideInInspector]
        public float minDistanceOfViewForLod5 = 180f;
        [HideInInspector]
        public float minDistanceOfViewForLod6 = 200f;
        [HideInInspector]
        public float minDistanceOfViewForLod7 = 220f;
        [HideInInspector]
        public float minDistanceOfViewForLod8 = 250f;
        [HideInInspector]
        public float minDistanceOfViewForCull = 270f;

        //Scanned meshes list
        [HideInInspector]
        public List<OriginalMeshItem> originalMeshesFoundInLastScan = new List<OriginalMeshItem>();

        //Debug settings
        [HideInInspector]
        public bool forceChangeLodsOfSkinnedInEditor = false;
        [HideInInspector]
        public bool drawGizmoOnThisPivot = false;
        [HideInInspector]
        public Color colorOfGizmo = Color.blue;
        [HideInInspector]
        public float sizeOfGizmo = 0.2f;

#if UNITY_EDITOR
        //Public variables of Interface
        private bool gizmosOfThisComponentIsDisabled = false;

        //Last defined distances and culling mode
        [HideInInspector]
        public float last_minDistanceOfViewForLod1 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod2 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod3 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod4 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod5 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod6 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod7 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForLod8 = 0f;
        [HideInInspector]
        public float last_minDistanceOfViewForCull = 0f;

        //Setup variable
        [HideInInspector]
        public bool setupRunned = false;

        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(UltimateLevelOfDetail))]
        public class CustomInspector : UnityEditor.Editor
        {
            //Private variables of Editor Only
            private Vector2 gameObjectsToIgnoreScrollpos = Vector2.zero;
            private Vector2 gameObjectsFoundInLastScanScrollpos = Vector2.zero;
            private Vector2 ulodsListScrollpos = Vector2.zero;
            private bool haveSkinnedMeshes_showWarningOfSkinnedLods = false;

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
                UltimateLevelOfDetail script = (UltimateLevelOfDetail)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");
                script.gizmosOfThisComponentIsDisabled = DisableGizmosInSceneView("UltimateLevelOfDetail", script.gizmosOfThisComponentIsDisabled);

                //If already have a ULOD component here, show error
                UltimateLevelOfDetail[] ulodsInThisGameObject = script.GetComponents<UltimateLevelOfDetail>();
                if (ulodsInThisGameObject.Length > 1)
                {
                    int isWorkingWithScanInChildren = 0;
                    foreach (UltimateLevelOfDetail ulod in ulodsInThisGameObject)
                        if (ulod.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
                            isWorkingWithScanInChildren += 1;
                    if (isWorkingWithScanInChildren > 1)
                    {
                        EditorGUILayout.HelpBox("It has been identified that there are more than 1 ULOD component in this GameObject, that is working on method of scan in children. For everything to work well, please keep only one ULOD per GameObject.", MessageType.Error);
                        script.modeOfMeshesScanning = (ScanMeshesMode)EditorGUILayout.EnumPopup(new GUIContent("Mode Of Meshes Scanning", "Please, change mode of meshes scanning of some of ULODs in this GameObject."), script.modeOfMeshesScanning);
                        return;
                    }
                }

                //Run setup, if is not runned yet
                if (script.setupRunned == false)
                {
                    script.customCameraForSimulationOfLods = Camera.main;
                    script.setupRunned = true;
                }

                //Support reminder
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Remember to read the Ultimate LOD System documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

                //Try to find parent and children ULODs components
                List<UltimateLevelOfDetail> parentUlods = Finder_FindAllParentUlodsWhereWorkingModeScanChildrensEnabled(script);
                List<UltimateLevelOfDetail> childrenUlods = Finder_FindAllChildrenUlods(script);

                GUILayout.Space(10);

                //Show the main resume
                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    GUIStyle titulo = new GUIStyle();
                    titulo.fontSize = 16;
                    titulo.normal.textColor = Color.red;
                    titulo.alignment = TextAnchor.MiddleCenter;
                    EditorGUILayout.LabelField("No Scanning Done Yet", titulo);
                }
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    GUIStyle titulo = new GUIStyle();
                    titulo.fontSize = 16;
                    titulo.normal.textColor = new Color(0, 79.0f / 250.0f, 3.0f / 250.0f);
                    titulo.alignment = TextAnchor.MiddleCenter;
                    EditorGUILayout.LabelField("Meshes Scanned And LODs Working", titulo);
                }

                //Scan settings
                Params_MeshesScanSettings(script, parentUlods);

                //List ignore GameObjects during scan
                Params_IgnoreGameObjectsDuringScan(script);

                //LODs
                Params_LodsSettings(script);

                //Distance of view for each LOD
                Params_DistanceOfView(script);

                //Managed meshes
                Debbug_OriginalMeshesFoundInLastScan(script);

                //Parent ULODs shower
                Debbug_ParendAndChildrenUlodsFound(script, parentUlods, childrenUlods);

                //Debbuging
                Debbug_Settings(script);

                GUILayout.Space(20);

                //Only able to scan or generate LODs, if this is the parent ULOD, or if not have parent ULODs
                if (parentUlods.Count == 0)
                {
                    if (script.originalMeshesFoundInLastScan.Count == 0)
                        if (GUILayout.Button("Scan All Meshes And Generate LODs", GUILayout.Height(40)))
                        {
                            if (childrenUlods.Count == 0)
                                //Scan only in this gameobject
                                script.ScanForMeshesAndGenerateAllLodGroups(true);
                            if (childrenUlods.Count > 0)
                            {
                                //Scan in this gameobject first
                                script.ScanForMeshesAndGenerateAllLodGroups(true);
                                //Scan in all children GameObjects also
                                int index = 0;
                                foreach (UltimateLevelOfDetail ulod in childrenUlods)
                                {
                                    EditorUtility.DisplayProgressBar("Please wait...", "Generating LODs for children ULOD in GameObject \"" + ulod.gameObject.name + "\"...", ((float)index / (float)childrenUlods.Count));
                                    if (ulod.modeOfMeshesScanning == ScanMeshesMode.ScanInThisGameObjectOnly)
                                        if (ulod.originalMeshesFoundInLastScan.Count == 0)
                                            ulod.ScanForMeshesAndGenerateAllLodGroups(false);
                                    index += 1;
                                }
                                EditorUtility.ClearProgressBar();
                            }
                        }
                    if (script.originalMeshesFoundInLastScan.Count > 0)
                        if (GUILayout.Button("Delete All Meshes Data Scanned And LODs", GUILayout.Height(40)))
                            if (EditorUtility.DisplayDialog("Continue?", "Are you ready to delete all groups of LODs created and restore all original meshes, continue?", "Yes", "No") == true)
                            {
                                if (childrenUlods.Count == 0)
                                    script.DeleteAllMeshesScannedAndAllLodGroups(true);
                                if (childrenUlods.Count > 0)
                                {
                                    //Delete scan in this gameobject first
                                    script.DeleteAllMeshesScannedAndAllLodGroups(true);
                                    //Delete scan in all children GameObjects also
                                    int index = 0;
                                    foreach (UltimateLevelOfDetail ulod in childrenUlods)
                                    {
                                        EditorUtility.DisplayProgressBar("Please wait...", "Deleting LODs of children ULOD in GameObject \"" + ulod.gameObject.name + "\"...", ((float)index / (float)childrenUlods.Count));
                                        if (ulod.modeOfMeshesScanning == ScanMeshesMode.ScanInThisGameObjectOnly)
                                            if (ulod.originalMeshesFoundInLastScan.Count > 0)
                                                ulod.DeleteAllMeshesScannedAndAllLodGroups(false);
                                        index += 1;
                                    }
                                    EditorUtility.ClearProgressBar();
                                }
                            }
                }
                //If have parent ULODs notify the user that this, will be managed by the parent ULOD
                if (parentUlods.Count > 0)
                {
                    EditorGUILayout.HelpBox("This ULOD component has identified that there is another parent ULOD component of this component. Therefore, you can no longer control when this component should or should not scan and create LODs. The scan of this component is now synchronized with the scan of the parent ULOD component. When the parent ULOD does a scan, this ULOD will also do it and when the parent ULOD deletes the scan it has, this ULOD will also delete it. Everything will be synchronized automatically and you can see here, which ULOD parent was identified and if you go to the ULOD parent, you can see all the child ULODs he identified. As the parent ULOD is already working with scanning all of the child meshes, this ULOD component can only work with scanning the mesh that is in this GameObject only. Consult the documentation for more details.", MessageType.Warning);
                }

                //Validate all parameters selected and clear lists of chidren and parent ulods
                script.ValidateAllParameters(false);
                childrenUlods.Clear();
                parentUlods.Clear();

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

            public List<UltimateLevelOfDetail> Finder_FindAllParentUlodsWhereWorkingModeScanChildrensEnabled(UltimateLevelOfDetail script)
            {
                List<UltimateLevelOfDetail> parentUlods = new List<UltimateLevelOfDetail>();
                //Find parent ulods where working mode is Scan in Children GameObjects only
                UltimateLevelOfDetail[] tempParentUlods = script.GetComponentsInParent<UltimateLevelOfDetail>();
                foreach (UltimateLevelOfDetail ulod in tempParentUlods)
                {
                    if (ulod == script)
                        continue;
                    if (ulod.gameObject == script.gameObject)
                        continue;
                    if (ulod.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
                        parentUlods.Add(ulod);
                }
                return parentUlods;
            }

            public List<UltimateLevelOfDetail> Finder_FindAllChildrenUlods(UltimateLevelOfDetail script)
            {
                List<UltimateLevelOfDetail> childrenUlods = new List<UltimateLevelOfDetail>();
                UltimateLevelOfDetail[] tempChildrenUlods = null;
                //Try to find children ULODs components, if "ScanInChildrenGameObjectsOnly" is enabled on this ULOD and change all children ULODs to scan only your gameobjects
                if (script.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
                {
                    tempChildrenUlods = script.GetComponentsInChildren<UltimateLevelOfDetail>();
                    foreach (UltimateLevelOfDetail ulod in tempChildrenUlods)
                    {
                        if (ulod == script)
                            continue;
                        if (ulod.gameObject == script.gameObject)
                            continue;
                        ulod.modeOfMeshesScanning = ScanMeshesMode.ScanInThisGameObjectOnly;
                        childrenUlods.Add(ulod);
                    }
                }
                return childrenUlods;
            }

            public void Params_MeshesScanSettings(UltimateLevelOfDetail script, List<UltimateLevelOfDetail> parentUlods)
            {
                //Scan settings
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Meshes Scan Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);

                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    script.modeOfMeshesScanning = (ScanMeshesMode)EditorGUILayout.EnumPopup(new GUIContent("Mode Of Meshes Scanning",
                                            "Mesh scanning mode that ULOD will use, that is, how and where ULOD should look for meshes to generate LODs.\n\n" +
                                            "ScanInChildrenGameObjectsOnly - ULOD will only search for meshes in the GameObjects children of this GameObject. GameObjects children of this GameObject, which contain a ULOD component, will be automatically ignored. The mesh in this GameObject will also be ignored.\n\n" +
                                            "ScanInThisGameObjectOnly - ULOD will only search for meshes that are in THIS GameObject, ignoring child GameObjects, etc."),
                                            script.modeOfMeshesScanning);
                    if (script.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
                    {
                        EditorGUI.indentLevel += 1;
                        script.scanInactiveGameObjects = (bool)EditorGUILayout.Toggle(new GUIContent("Scan Inactive Too",
                            "If this option is active, ULOD will also scan meshes that are disabled."),
                            script.scanInactiveGameObjects);
                        EditorGUI.indentLevel -= 1;

                        //If have parent ULODs working in scanning children, change it to scan in this GameObject only
                        if (parentUlods.Count > 0)
                        {
                            script.modeOfMeshesScanning = ScanMeshesMode.ScanInThisGameObjectOnly;
                            EditorUtility.DisplayDialog("Warning", "It has been identified that there is an Ultimate Level Of Detail component that is the parent of this GameObject and it is working in the scanning mode of child GameObjects. So, in order not to interfere with his work, this ULOD component can only operate in scan mode for this GameObject only. So all the settings you make in this ULOD will only work for this GameObject. The scans of this ULOD will also be synchronized with the scans of the parent ULOD.", "Ok");
                        }
                    }
                }
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    EditorGUILayout.HelpBox("These parameters are only available before you perform a scan.", MessageType.Info);
                }
            }

            public void Params_IgnoreGameObjectsDuringScan(UltimateLevelOfDetail script)
            {
                //Only show ignore gameobjects settings if scan childres is enabled
                if (script.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
                {
                    //Settings for "Meshes To Ignore"
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Ignore During Scan", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    if (script.originalMeshesFoundInLastScan.Count == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("GameObjects To Ignore During Scan", GUILayout.Width(230));
                        GUILayout.Space(GetInspectorWindowSize().x - 230);
                        EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                        EditorGUILayout.IntField(script.gameObjectsToIgnore.Count, GUILayout.Width(50));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.BeginVertical("box");
                        gameObjectsToIgnoreScrollpos = EditorGUILayout.BeginScrollView(gameObjectsToIgnoreScrollpos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(GetInspectorWindowSize().x), GUILayout.Height(100));
                        if (script.gameObjectsToIgnore.Count == 0)
                            EditorGUILayout.HelpBox("Oops! No GameObject to be ignored has been registered! If you want to subscribe any, click the button below!", MessageType.Info);
                        if (script.gameObjectsToIgnore.Count > 0)
                            for (int i = 0; i < script.gameObjectsToIgnore.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                if (script.originalMeshesFoundInLastScan.Count == 0)
                                    if (GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(15)))
                                        script.gameObjectsToIgnore.RemoveAt(i);
                                script.gameObjectsToIgnore[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("GameObject " + i.ToString(), "This GameObject will be ignored during the scan, if it has any mesh, it will not be scanned to have LODs.\n\nClick the button to the left if you want to remove this GameObject from the list."), script.gameObjectsToIgnore[i], typeof(GameObject), true, GUILayout.Height(16));
                                GUILayout.EndHorizontal();
                            }
                        EditorGUILayout.EndScrollView();
                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add New Slot"))
                        {
                            script.gameObjectsToIgnore.Add(null);
                            gameObjectsToIgnoreScrollpos.y += 999999;
                        }
                        if (script.gameObjectsToIgnore.Count > 0)
                            if (GUILayout.Button("Remove Empty Slots", GUILayout.Width(Screen.width * 0.48f)))
                                for (int i = script.gameObjectsToIgnore.Count - 1; i >= 0; i--)
                                    if (script.gameObjectsToIgnore[i] == null)
                                        script.gameObjectsToIgnore.RemoveAt(i);
                        GUILayout.EndHorizontal();
                    }
                    if (script.originalMeshesFoundInLastScan.Count > 0)
                    {
                        EditorGUILayout.HelpBox("These parameters are only available before you perform a scan.", MessageType.Info);
                    }
                }
            }

            public void Params_LodsSettings(UltimateLevelOfDetail script)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("LODs Generation Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);

                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    script.levelsOfDetailToGenerate = EditorGUILayout.IntSlider(new GUIContent("Levels Of Details To Generate",
                               "The number of LODs that the Ultimate LOD System should generate."),
                               script.levelsOfDetailToGenerate, 1, 8);

                    EditorGUI.indentLevel += 1;

                    script.percentOfVerticesLod1 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 1",
                    "The percentage of vertices it will contain in LOD 1 of the meshes."),
                    script.percentOfVerticesLod1, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 2)
                        script.percentOfVerticesLod2 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 2",
                        "The percentage of vertices it will contain in LOD 2 of the meshes."),
                        script.percentOfVerticesLod2, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 3)
                        script.percentOfVerticesLod3 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 3",
                        "The percentage of vertices it will contain in LOD 3 of the meshes."),
                        script.percentOfVerticesLod3, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 4)
                        script.percentOfVerticesLod4 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 4",
                        "The percentage of vertices it will contain in LOD 4 of the meshes."),
                        script.percentOfVerticesLod4, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 5)
                        script.percentOfVerticesLod5 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 5",
                        "The percentage of vertices it will contain in LOD 5 of the meshes."),
                        script.percentOfVerticesLod5, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 6)
                        script.percentOfVerticesLod6 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 6",
                        "The percentage of vertices it will contain in LOD 6 of the meshes."),
                        script.percentOfVerticesLod6, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 7)
                        script.percentOfVerticesLod7 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 7",
                        "The percentage of vertices it will contain in LOD 7 of the meshes."),
                        script.percentOfVerticesLod7, 1f, 100f);

                    if (script.levelsOfDetailToGenerate >= 8)
                        script.percentOfVerticesLod8 = EditorGUILayout.Slider(new GUIContent("Percent Of Vertices in LOD 8",
                        "The percentage of vertices it will contain in LOD 8 of the meshes."),
                        script.percentOfVerticesLod8, 1f, 100f);

                    EditorGUI.indentLevel -= 1;

                    script.saveGeneratedLodsInAssets = (bool)EditorGUILayout.Toggle(new GUIContent("Save LODs Meshes In Assets",
                                    "If this option is active, ULOD will save the mesh files of the LODs generated in your project."),
                                    script.saveGeneratedLodsInAssets);

                    script.skinnedAnimsCompatibilityMode = (bool)EditorGUILayout.Toggle(new GUIContent("Skinned Anims Compat Mode",
                                    "If this option is active, ULOD will use internal algorithms to improve the accuracy and compatibility with Skinned Mesh Renderers animations. If you have problems or artifacts, try disabling this. This only applies to meshes that are in Skinned Mesh Renderers."),
                                    script.skinnedAnimsCompatibilityMode);

                    script.preventArtifacts = (bool)EditorGUILayout.Toggle(new GUIContent("Prevent Artifacts Or Deform",
                                    "If this option is active, ULOD will use internal algorithms to prevent artifacts in the generated LOD meshes. If you still have problems with artifacts, try disabling this.\n\nNote that: Meshes that contain many vertices sharing the same space, may have problems or present artifacts."),
                                    script.preventArtifacts);

                    script.forceOfSimplification = (ForceOfSimplification)EditorGUILayout.EnumPopup(new GUIContent("Force Of Simplification",
                                    "Some meshes have such a large number of vertices, that even reducing their vertices to 1%, they will still have many vertices, or the algorithm can avoid a very large reduction of vertices, to maintain the original shape of the mesh. If you want to reduce even more the amount of vertices present in the meshes, increase the strength of the simplification. Very large forces in meshes with a moderate amount of vertices, you can completely deform them, so use this parameter while testing, and only use it if you really need a greater vertex reduction force.\n\nArtifact prevention is disabled if you use forces greater than the Normal force, as there is no way to prevent artifacts when greater aggressiveness is imposed in simplifying the mesh."),
                                    script.forceOfSimplification);

                    script.cullingMode = (CullingMode)EditorGUILayout.EnumPopup(new GUIContent("Culling Mode",
                                    "Culling will disable meshes that are too far apart, with a distance you define. By default, Culling is disabled, but you can choose a culling method and then you can set the minimum distance for culling to occur." +
                                    "\n\nDisabled - Culling will not occur, only the last LOD level will be displayed forever, regardless of the distance between the camera and this GameObject ULOD. Note that the standard camera culling will still be performed (The culling where Unity hides very distant meshes, as a standard behavior of cameras on Unity)." +
                                    "\n\nCullingMeshes - In this Culling method, the Ultimate LOD System will remove the mesh from the renderers, when reaching the Culling distance. If the camera is out of Culling's distance, the LOD meshes will be returned to the renderers." +
                                    "\n\nCullingMaterials - In this Culling method, the material list of the renderers will be disabled when the distance between the camera and this ULOD reaches the culling distance. If the camera approaches this ULOD again, the materials will be activated again in the renderers."),
                                    script.cullingMode);
                }
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    EditorGUILayout.HelpBox("These parameters are only available before you perform a scan.", MessageType.Info);
                }
            }

            public void Params_DistanceOfView(UltimateLevelOfDetail script)
            {
                //Distance of view
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Distance of View for Each LOD", EditorStyles.boldLabel);
                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("LOD 1"))
                    script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod1 + 1);
                if (script.levelsOfDetailToGenerate >= 2)
                    if (GUILayout.Button("LOD 2"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod2 + 1);
                if (script.levelsOfDetailToGenerate >= 3)
                    if (GUILayout.Button("LOD 3"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod3 + 1);
                if (script.levelsOfDetailToGenerate >= 4)
                    if (GUILayout.Button("LOD 4"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod4 + 1);
                if (script.levelsOfDetailToGenerate >= 5)
                    if (GUILayout.Button("LOD 5"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod5 + 1);
                if (script.levelsOfDetailToGenerate >= 6)
                    if (GUILayout.Button("LOD 6"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod6 + 1);
                if (script.levelsOfDetailToGenerate >= 7)
                    if (GUILayout.Button("LOD 7"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod7 + 1);
                if (script.levelsOfDetailToGenerate >= 8)
                    if (GUILayout.Button("LOD 8"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod8 + 1);
                if (script.cullingMode != CullingMode.Disabled)
                    if (GUILayout.Button("Cull"))
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForCull + 1);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                script.minDistanceOfViewForLod1 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 1",
                               "The minimum distance required (in Units) to view LOD 1."),
                               script.minDistanceOfViewForLod1, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 2)
                    script.minDistanceOfViewForLod2 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 2",
                    "The minimum distance required (in Units) to view LOD 2."),
                    script.minDistanceOfViewForLod2, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 3)
                    script.minDistanceOfViewForLod3 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 3",
                    "The minimum distance required (in Units) to view LOD 3."),
                    script.minDistanceOfViewForLod3, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 4)
                    script.minDistanceOfViewForLod4 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 4",
                    "The minimum distance required (in Units) to view LOD 4."),
                    script.minDistanceOfViewForLod4, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 5)
                    script.minDistanceOfViewForLod5 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 5",
                    "The minimum distance required (in Units) to view LOD 5."),
                    script.minDistanceOfViewForLod5, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 6)
                    script.minDistanceOfViewForLod6 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 6",
                    "The minimum distance required (in Units) to view LOD 6."),
                    script.minDistanceOfViewForLod6, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 7)
                    script.minDistanceOfViewForLod7 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 7",
                    "The minimum distance required (in Units) to view LOD 7."),
                    script.minDistanceOfViewForLod7, 1f, 3000f);

                if (script.levelsOfDetailToGenerate >= 8)
                    script.minDistanceOfViewForLod8 = EditorGUILayout.Slider(new GUIContent("Min Distance To View LOD 8",
                    "The minimum distance required (in Units) to view LOD 8."),
                    script.minDistanceOfViewForLod8, 1f, 3000f);

                if (script.cullingMode != CullingMode.Disabled)
                {
                    script.minDistanceOfViewForCull = EditorGUILayout.Slider(new GUIContent("Min Distance Of View For Cull",
                    "This represents the minimum distance between this ULOD and the camera that currently renders these objects, for the culling of the meshes to occur. When culling the meshes, the ULOD will hide and stop rendering the meshes until the distance of the last level or less, returns between the camera and this ULOD."),
                    script.minDistanceOfViewForCull, 1f, 3000f);
                }

                script.cameraDetectionMode = (CameraDetectionMode)EditorGUILayout.EnumPopup(new GUIContent("Camera Detection Mode",
                                                                    "The camera detection method that the Ultimate LOD System should use to detect the distance between the camera and this GameObject to simulate the change of LODs.\n\n" +
                                                                    "CurrentCamera - In this method the Ultimate LOD System will use a component called \"Runtime Camera Detector\" that will stay in your scene and will try to determine the camera that is currently appearing on the screen, using an automatic algorithm. The camera determined by the algorithm will be used to calculate the distance between this GameObject and the camera, for the simulation of LODs.\n\n" +
                                                                    "MainCamera - This method will use \"Camera.main\" to identify a camera to calculate the distance. It requires that the main camera of your game has the tag \"MainCamera\".\n\n" +
                                                                    "CustomCamera - In this method you can define a customized camera so that the ULOD calculates the distance and makes the simulation of LODs only in relation to it. This gives you more control over how the LOD simulation will be done, it can be very useful for multiplayer games for example."),
                                                                    script.cameraDetectionMode);
                if (script.cameraDetectionMode == CameraDetectionMode.MainCamera)
                {
                    script.useCacheForMainCameraInDetection = (bool)EditorGUILayout.Toggle(new GUIContent("Use Cache Of Main Camera",
                                    "If this option is active, the Ultimate LOD System will use a cache from the main camera, as soon as it is detected, so that it is not necessary to search for it in each frame.\n\nIf this option is active, performance may increase, however, the detection accuracy may be lower. Try disabling this if you want more precision."),
                                    script.useCacheForMainCameraInDetection);
                }
                if (script.cameraDetectionMode == CameraDetectionMode.CustomCamera)
                {
                    script.customCameraForSimulationOfLods = (Camera)EditorGUILayout.ObjectField(new GUIContent("Custom Camera For Simulate",
                                                        "The customized camera, which will be used to calculate the distance and simulation of the LODs."),
                                                        script.customCameraForSimulationOfLods, typeof(Camera), true, GUILayout.Height(16));
                }

                //Distance showers, on change a distance
                if (script.last_minDistanceOfViewForLod1 != script.minDistanceOfViewForLod1)
                {
                    if (script.last_minDistanceOfViewForLod1 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod1 + 1);
                    script.last_minDistanceOfViewForLod1 = script.minDistanceOfViewForLod1;
                }

                if (script.last_minDistanceOfViewForLod2 != script.minDistanceOfViewForLod2)
                {
                    if (script.last_minDistanceOfViewForLod2 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod2 + 1);
                    script.last_minDistanceOfViewForLod2 = script.minDistanceOfViewForLod2;
                }

                if (script.last_minDistanceOfViewForLod3 != script.minDistanceOfViewForLod3)
                {
                    if (script.last_minDistanceOfViewForLod3 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod3 + 1);
                    script.last_minDistanceOfViewForLod3 = script.minDistanceOfViewForLod3;
                }

                if (script.last_minDistanceOfViewForLod4 != script.minDistanceOfViewForLod4)
                {
                    if (script.last_minDistanceOfViewForLod4 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod4 + 1);
                    script.last_minDistanceOfViewForLod4 = script.minDistanceOfViewForLod4;
                }

                if (script.last_minDistanceOfViewForLod5 != script.minDistanceOfViewForLod5)
                {
                    if (script.last_minDistanceOfViewForLod5 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod5 + 1);
                    script.last_minDistanceOfViewForLod5 = script.minDistanceOfViewForLod5;
                }

                if (script.last_minDistanceOfViewForLod6 != script.minDistanceOfViewForLod6)
                {
                    if (script.last_minDistanceOfViewForLod6 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod6 + 1);
                    script.last_minDistanceOfViewForLod6 = script.minDistanceOfViewForLod6;
                }

                if (script.last_minDistanceOfViewForLod7 != script.minDistanceOfViewForLod7)
                {
                    if (script.last_minDistanceOfViewForLod7 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod7 + 1);
                    script.last_minDistanceOfViewForLod7 = script.minDistanceOfViewForLod7;
                }

                if (script.last_minDistanceOfViewForLod8 != script.minDistanceOfViewForLod8)
                {
                    if (script.last_minDistanceOfViewForLod8 > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForLod8 + 1);
                    script.last_minDistanceOfViewForLod8 = script.minDistanceOfViewForLod8;
                }

                if (script.last_minDistanceOfViewForCull != script.minDistanceOfViewForCull)
                {
                    if (script.last_minDistanceOfViewForCull > 0)
                        script.ShowMinDistanceToViewLodInSceneView(script.minDistanceOfViewForCull + 1);
                    script.last_minDistanceOfViewForCull = script.minDistanceOfViewForCull;
                }
            }

            public void Debbug_OriginalMeshesFoundInLastScan(UltimateLevelOfDetail script)
            {
                //Managed meshes
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Scanned Meshes For LOD", EditorStyles.boldLabel);
                GUILayout.Space(10);

                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    EditorGUILayout.HelpBox("There are no meshes scanned by this component yet. Click the button below to start scanning and generating LODs to begin. All meshes that are identified by ULOD, will appear here and have their LODs generated.", MessageType.Info);
                }
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    //Create font red
                    GUIStyle error = new GUIStyle();
                    error.normal.textColor = Color.red;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Meshes Found In Current Scan", GUILayout.Width(230));
                    GUILayout.Space(GetInspectorWindowSize().x - 230);
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    EditorGUILayout.IntField(script.originalMeshesFoundInLastScan.Count, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.BeginVertical("box");
                    gameObjectsFoundInLastScanScrollpos = EditorGUILayout.BeginScrollView(gameObjectsFoundInLastScanScrollpos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(GetInspectorWindowSize().x), GUILayout.Height(100));
                    //Original meshes list
                    foreach (OriginalMeshItem meshItem in script.originalMeshesFoundInLastScan)
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        if (meshItem.originalGameObject != null)
                            EditorGUILayout.LabelField(meshItem.originalGameObject.name, EditorStyles.boldLabel);
                        if (meshItem.originalGameObject == null)
                            EditorGUILayout.LabelField("GameObject Not Found", error);
                        GUILayout.Space(-3);
                        if (meshItem.originalGameObject == null || meshItem.originalMesh == null)
                        {
                            EditorGUILayout.LabelField("Please, re-scan all meshes to fix this.", error);
                        }
                        if (meshItem.originalGameObject != null && meshItem.originalMesh != null)
                        {
                            //Check if have a missing LOD mesh
                            bool missingMeshes = false;
                            if (meshItem.meshLod1 == null)
                                missingMeshes = true;
                            if (meshItem.meshLod2 == null && script.levelsOfDetailToGenerate >= 2)
                                missingMeshes = true;
                            if (meshItem.meshLod3 == null && script.levelsOfDetailToGenerate >= 3)
                                missingMeshes = true;
                            if (meshItem.meshLod4 == null && script.levelsOfDetailToGenerate >= 4)
                                missingMeshes = true;
                            if (meshItem.meshLod5 == null && script.levelsOfDetailToGenerate >= 5)
                                missingMeshes = true;
                            if (meshItem.meshLod6 == null && script.levelsOfDetailToGenerate >= 6)
                                missingMeshes = true;
                            if (meshItem.meshLod7 == null && script.levelsOfDetailToGenerate >= 7)
                                missingMeshes = true;
                            if (meshItem.meshLod8 == null && script.levelsOfDetailToGenerate >= 8)
                                missingMeshes = true;

                            if (meshItem.skinnedMeshRenderer != null)
                            {
                                if (meshItem.originalMesh != null && missingMeshes == false)
                                    EditorGUILayout.LabelField("Skinned Mesh, " + meshItem.originalMesh.name + Path.GetExtension(AssetDatabase.GetAssetPath(meshItem.originalMesh)));
                                if (meshItem.originalMesh == null || missingMeshes == true)
                                    EditorGUILayout.LabelField("Missing mesh file or LODs. Redo the scan.", error);
                                haveSkinnedMeshes_showWarningOfSkinnedLods = true;
                            }
                            if (meshItem.meshFilter != null)
                            {
                                if (meshItem.originalMesh != null && missingMeshes == false)
                                    EditorGUILayout.LabelField("Normal Mesh, " + meshItem.originalMesh.name + Path.GetExtension(AssetDatabase.GetAssetPath(meshItem.originalMesh)));
                                if (meshItem.originalMesh == null || missingMeshes == true)
                                    EditorGUILayout.LabelField("Missing mesh file or LODs. Redo the scan.", error);
                            }
                            if (meshItem.meshFilter == null && meshItem.skinnedMeshRenderer == null)
                                EditorGUILayout.LabelField("No renderer found. Please, re-scan to fix this.", error);
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(20);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(8);
                        if (meshItem.originalGameObject != null)
                            if (GUILayout.Button("Game Object", GUILayout.Height(20)))
                                EditorGUIUtility.PingObject(meshItem.originalGameObject);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2);
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    EditorGUILayout.HelpBox("All the meshs listed above were found in the last scan and their LODs were automatically generated based on your configuration.", MessageType.Info);
                }
            }

            public void Debbug_ParendAndChildrenUlodsFound(UltimateLevelOfDetail script, List<UltimateLevelOfDetail> parentUlods, List<UltimateLevelOfDetail> childrenUlods)
            {
                //Parent ULODs shower
                if (script.modeOfMeshesScanning == ScanMeshesMode.ScanInThisGameObjectOnly && parentUlods.Count > 0)
                {
                    //Parent ULODs
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Parent ULODs components", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Parent ULODs Controlling This", GUILayout.Width(230));
                    GUILayout.Space(GetInspectorWindowSize().x - 230);
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    EditorGUILayout.IntField(parentUlods.Count, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.BeginVertical("box");
                    ulodsListScrollpos = EditorGUILayout.BeginScrollView(ulodsListScrollpos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(GetInspectorWindowSize().x), GUILayout.Height(100));
                    //Original meshes list
                    foreach (UltimateLevelOfDetail ulod in parentUlods)
                    {
                        //Skip if this not is the last parent (top parent)
                        if (ulod != parentUlods[parentUlods.Count - 1])
                            continue;

                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField(ulod.gameObject.name, EditorStyles.boldLabel);
                        GUILayout.Space(-3);
                        EditorGUILayout.LabelField("Is controlling this component scan.");
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(20);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(8);
                        if (GUILayout.Button("Copy Distance Data", GUILayout.Height(20)))
                        {
                            script.minDistanceOfViewForLod1 = ulod.minDistanceOfViewForLod1;
                            script.minDistanceOfViewForLod2 = ulod.minDistanceOfViewForLod2;
                            script.minDistanceOfViewForLod3 = ulod.minDistanceOfViewForLod3;
                            script.minDistanceOfViewForLod4 = ulod.minDistanceOfViewForLod4;
                            script.minDistanceOfViewForLod5 = ulod.minDistanceOfViewForLod5;
                            script.minDistanceOfViewForLod6 = ulod.minDistanceOfViewForLod6;
                            script.minDistanceOfViewForLod7 = ulod.minDistanceOfViewForLod7;
                            script.minDistanceOfViewForLod8 = ulod.minDistanceOfViewForLod8;
                            script.minDistanceOfViewForCull = ulod.minDistanceOfViewForCull;
                            script.cullingMode = ulod.cullingMode;
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(8);
                        if (GUILayout.Button("Go To", GUILayout.Height(20)))
                            Selection.objects = new UnityEngine.Object[] { ulod.gameObject };
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2);
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }

                //Childrens ULODs shower
                if (script.modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly && childrenUlods.Count > 0)
                {
                    //Children ULODs
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Children ULODs components", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Children ULODs Being Controlled", GUILayout.Width(230));
                    GUILayout.Space(GetInspectorWindowSize().x - 230);
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    EditorGUILayout.IntField(childrenUlods.Count, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.BeginVertical("box");
                    ulodsListScrollpos = EditorGUILayout.BeginScrollView(ulodsListScrollpos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(GetInspectorWindowSize().x), GUILayout.Height(100));
                    //Original meshes list
                    foreach (UltimateLevelOfDetail ulod in childrenUlods)
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField(ulod.gameObject.name + ((ulod.originalMeshesFoundInLastScan.Count > 0) ? " (Done)" : " (Not Scanning Done Yet)"), EditorStyles.boldLabel);
                        GUILayout.Space(-3);
                        EditorGUILayout.LabelField("The scanning of this ULOD is synchronized with it.");
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(20);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(8);
                        if (GUILayout.Button("Go To", GUILayout.Height(20)))
                            Selection.objects = new UnityEngine.Object[] { ulod.gameObject };
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2);
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }
            }

            public void Debbug_Settings(UltimateLevelOfDetail script)
            {
                //Debbugging
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Debbug Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);

                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    EditorGUILayout.HelpBox("There are no meshes scanned by this component yet. Click the button below to start scanning and generating LODs to begin. All meshes that are identified by ULOD, will appear here and have their LODs generated.", MessageType.Info);
                }
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    //Show notification of unity lods skinned mesh renderer simulation, if have skinned meshes
                    if (haveSkinnedMeshes_showWarningOfSkinnedLods == true)
                        EditorGUILayout.HelpBox("This scan has Skinned Mesh Renderers. The simulation of LOD changes in Skinned Mesh Renderers will not take place in the Editor to avoid problems of crash of the Editor, due to limitations of some versions of Unity. The Ultimate LOD System will still change LODs for your Skinned Mesh Renderers in your Build game (final game in Android, Windows, iOS or other platform, for example) without any problems. If you want to force this simulation on the Editor too, activate the \"Editor Skinned LODs Change\" option below.", MessageType.Warning);

                    script.forceChangeLodsOfSkinnedInEditor = (bool)EditorGUILayout.Toggle(new GUIContent("Editor Skinned LODs Change",
                                               "Some versions of the Unity editor have problems with simulating Skinned mesh LODs. It turns out that in some versions of Unity, during the mesh change simulation in the Skinned Mesh Renderer, the Editor may crash. To avoid this, by default, the LOD mesh simulation in Skinned Mesh Renderers, in the editor, is disabled.\n\nEven so, the Ultimate LOD System will still change LODs in Skinned Mesh Renderers, in building your game, without any problems. The crash problem only occurs in the Editor.\n\nIf you want to force the mesh change simulation in the Editor, just activate this option, but be aware that the crash problem can occur depending on the version of your Editor, this is a limitation of Unity Editor in some versions."),
                                               script.forceChangeLodsOfSkinnedInEditor);

                    script.drawGizmoOnThisPivot = (bool)EditorGUILayout.Toggle(new GUIContent("Draw Gizmo On This Pivot",
                       "If this option is active, ULOD will draw a small Gizmo on the pivot of this GameObject to let you know that this GameObject has a group of LODs created."),
                       script.drawGizmoOnThisPivot);
                    if (script.drawGizmoOnThisPivot == true)
                    {
                        EditorGUI.indentLevel += 1;
                        script.colorOfGizmo = EditorGUILayout.ColorField(new GUIContent("Color Of Gizmo",
                       "The color of the gizmo that will be drawn."),
                       script.colorOfGizmo);

                        script.sizeOfGizmo = EditorGUILayout.Slider(new GUIContent("Size Of Gizmo",
                                                           "The size of the Gizmo that will be drawn."),
                                                           script.sizeOfGizmo, 0.01f, 10f);
                        EditorGUI.indentLevel -= 1;
                    }
                }
            }

            protected virtual void OnSceneGUI()
            {
                UltimateLevelOfDetail script = (UltimateLevelOfDetail)target;

                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Set the base color of gizmos
                Handles.color = Color.green;

                //If have a scan
                if (script.originalMeshesFoundInLastScan.Count > 0)
                {
                    //Current LOD stats
                    StringBuilder currentTextStr = new StringBuilder();
                    if (script.currentLodAccordingToDistance == -1 || script.currentLodAccordingToDistance == 0)
                    {
                        currentTextStr.Append("LOD Original (100%)");
                    }
                    if (script.currentLodAccordingToDistance > 0 && script.currentLodAccordingToDistance < 9)
                    {
                        currentTextStr.Append("LOD ");
                        currentTextStr.Append(script.currentLodAccordingToDistance.ToString());
                        currentTextStr.Append(" (");
                        if (script.currentLodAccordingToDistance == 1)
                            currentTextStr.Append(script.percentOfVerticesLod1);
                        if (script.currentLodAccordingToDistance == 2)
                            currentTextStr.Append(script.percentOfVerticesLod2);
                        if (script.currentLodAccordingToDistance == 3)
                            currentTextStr.Append(script.percentOfVerticesLod3);
                        if (script.currentLodAccordingToDistance == 4)
                            currentTextStr.Append(script.percentOfVerticesLod4);
                        if (script.currentLodAccordingToDistance == 5)
                            currentTextStr.Append(script.percentOfVerticesLod5);
                        if (script.currentLodAccordingToDistance == 6)
                            currentTextStr.Append(script.percentOfVerticesLod6);
                        if (script.currentLodAccordingToDistance == 7)
                            currentTextStr.Append(script.percentOfVerticesLod7);
                        if (script.currentLodAccordingToDistance == 8)
                            currentTextStr.Append(script.percentOfVerticesLod8);
                        currentTextStr.Append("%)");
                    }
                    if (script.currentLodAccordingToDistance == 9)
                    {
                        currentTextStr.Append("CULLED (0%)");
                    }
                    currentTextStr.Append("\n");
                    currentTextStr.Append(script.currentDistanceFromMainCamera.ToString("F0"));
                    currentTextStr.Append(" Units");
                    float multiplier = UltimateLevelOfDetailGlobal.GetGlobalLodDistanceMultiplier();
                    if (multiplier != 1.0f)
                    {
                        currentTextStr.Append(" (x");
                        currentTextStr.Append(multiplier);
                        currentTextStr.Append(")");
                    }
                    int vertices = 0;
                    bool identifiedNullMeshes = false;
                    foreach (OriginalMeshItem meshItem in script.originalMeshesFoundInLastScan)
                    {
                        //Calculate current count of vertices
                        if (script.currentLodAccordingToDistance == -1 && meshItem.originalMesh != null)
                            vertices += meshItem.originalMesh.vertexCount;
                        if (script.currentLodAccordingToDistance == 0 && meshItem.originalMesh != null)
                            vertices += meshItem.originalMesh.vertexCount;
                        if (script.currentLodAccordingToDistance == 1 && meshItem.meshLod1 != null)
                            vertices += meshItem.meshLod1.vertexCount;
                        if (script.currentLodAccordingToDistance == 2 && meshItem.meshLod2 != null)
                            vertices += meshItem.meshLod2.vertexCount;
                        if (script.currentLodAccordingToDistance == 3 && meshItem.meshLod3 != null)
                            vertices += meshItem.meshLod3.vertexCount;
                        if (script.currentLodAccordingToDistance == 4 && meshItem.meshLod4 != null)
                            vertices += meshItem.meshLod4.vertexCount;
                        if (script.currentLodAccordingToDistance == 5 && meshItem.meshLod5 != null)
                            vertices += meshItem.meshLod5.vertexCount;
                        if (script.currentLodAccordingToDistance == 6 && meshItem.meshLod6 != null)
                            vertices += meshItem.meshLod6.vertexCount;
                        if (script.currentLodAccordingToDistance == 7 && meshItem.meshLod7 != null)
                            vertices += meshItem.meshLod7.vertexCount;
                        if (script.currentLodAccordingToDistance == 8 && meshItem.meshLod8 != null)
                            vertices += meshItem.meshLod8.vertexCount;

                        //Check if have a missing LOD mesh
                        if (meshItem.originalMesh == null)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod1 == null)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod2 == null && script.levelsOfDetailToGenerate >= 2)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod3 == null && script.levelsOfDetailToGenerate >= 3)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod4 == null && script.levelsOfDetailToGenerate >= 4)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod5 == null && script.levelsOfDetailToGenerate >= 5)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod6 == null && script.levelsOfDetailToGenerate >= 6)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod7 == null && script.levelsOfDetailToGenerate >= 7)
                            identifiedNullMeshes = true;
                        if (meshItem.meshLod8 == null && script.levelsOfDetailToGenerate >= 8)
                            identifiedNullMeshes = true;
                    }
                    currentTextStr.Append("\n");
                    currentTextStr.Append(vertices.ToString());
                    currentTextStr.Append(" Vertices");
                    if (identifiedNullMeshes == true)
                        currentTextStr.Append("\nSCAN ERRORS!");
                    string currentText = currentTextStr.ToString();

                    //Prepare the text
                    GUIStyle styleVerticeDetail = new GUIStyle();
                    styleVerticeDetail.normal.textColor = Color.white;
                    styleVerticeDetail.alignment = TextAnchor.MiddleCenter;
                    styleVerticeDetail.fontStyle = FontStyle.Bold;
                    styleVerticeDetail.contentOffset = new Vector2(-currentText.Substring(0, currentText.IndexOf("\n") + 1).Length * 1.2f, 24);

                    //Draw the LOD stats
                    Handles.Label(script.gameObject.transform.position, currentText, styleVerticeDetail);
                }
                //If not have a scan yet
                if (script.originalMeshesFoundInLastScan.Count == 0)
                {
                    //Current LOD stats
                    StringBuilder currentTextStr = new StringBuilder();
                    currentTextStr.Append("No Scanning\n");
                    currentTextStr.Append("Done Yet!");
                    string currentText = currentTextStr.ToString();

                    //Prepare the text
                    GUIStyle styleVerticeDetail = new GUIStyle();
                    styleVerticeDetail.normal.textColor = Color.red;
                    styleVerticeDetail.alignment = TextAnchor.MiddleCenter;
                    styleVerticeDetail.fontStyle = FontStyle.Bold;
                    styleVerticeDetail.contentOffset = new Vector2(-currentText.Substring(0, currentText.IndexOf("\n") + 1).Length * 1.2f, 24);

                    //Draw the LOD stats
                    Handles.Label(script.gameObject.transform.position, currentText, styleVerticeDetail);
                }

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {
                    //Apply the change, if moved the handle
                    //script.transform.position = teste;
                }
                Repaint();
            }
        }
        #endregion

        //Gizmos parameters

        public void OnDrawGizmos()
        {
            //If is not desired to draw gizmo
            if (drawGizmoOnThisPivot == false || originalMeshesFoundInLastScan.Count == 0)
                return;

            //Draw gizmo to show pivot of this
            Gizmos.color = colorOfGizmo;
            Gizmos.DrawSphere(this.gameObject.transform.position, sizeOfGizmo);
        }

        //Interface methods

        public float GetMinDistanceValueFromLastLevel()
        {
            //Return the distance of last level selected by use
            float distance = minDistanceOfViewForLod1;
            if (levelsOfDetailToGenerate >= 2)
                distance = minDistanceOfViewForLod2;
            if (levelsOfDetailToGenerate >= 3)
                distance = minDistanceOfViewForLod3;
            if (levelsOfDetailToGenerate >= 4)
                distance = minDistanceOfViewForLod4;
            if (levelsOfDetailToGenerate >= 5)
                distance = minDistanceOfViewForLod5;
            if (levelsOfDetailToGenerate >= 6)
                distance = minDistanceOfViewForLod6;
            if (levelsOfDetailToGenerate >= 7)
                distance = minDistanceOfViewForLod7;
            if (levelsOfDetailToGenerate >= 8)
                distance = minDistanceOfViewForLod8;
            return distance;
        }

        public void ValidateAllParameters(bool isGoingToScan)
        {
            //This method validate the parameters

            //Disable prevent artifacts if force is not normal
            if (forceOfSimplification != ForceOfSimplification.Normal)
                preventArtifacts = false;

            //Validate min level of each
            if (minDistanceOfViewForLod1 < 1f)
                minDistanceOfViewForLod1 = 1f;
            if (minDistanceOfViewForLod2 < 5f)
                minDistanceOfViewForLod2 = 5f;
            if (minDistanceOfViewForLod3 < 10f)
                minDistanceOfViewForLod3 = 10f;
            if (minDistanceOfViewForLod4 < 15f)
                minDistanceOfViewForLod4 = 15f;
            if (minDistanceOfViewForLod5 < 20f)
                minDistanceOfViewForLod5 = 20f;
            if (minDistanceOfViewForLod6 < 25f)
                minDistanceOfViewForLod6 = 25f;
            if (minDistanceOfViewForLod7 < 30f)
                minDistanceOfViewForLod7 = 30f;
            if (minDistanceOfViewForLod8 < 35f)
                minDistanceOfViewForLod8 = 35f;
            if (minDistanceOfViewForCull <= GetMinDistanceValueFromLastLevel())
                minDistanceOfViewForCull = (GetMinDistanceValueFromLastLevel() + 10.0f);

            //Validate percent of vertices for each level (only if is going to scan)
            if (isGoingToScan == true)
            {
                //Validate percent of vertices
                if (levelsOfDetailToGenerate >= 2)
                    if (percentOfVerticesLod2 > percentOfVerticesLod1)
                    {
                        percentOfVerticesLod2 = (percentOfVerticesLod1 - 1.0f);
                        if (percentOfVerticesLod2 <= 0)
                            percentOfVerticesLod2 = percentOfVerticesLod1;
                    }
                if (levelsOfDetailToGenerate >= 3)
                    if (percentOfVerticesLod3 > percentOfVerticesLod2)
                    {
                        percentOfVerticesLod3 = (percentOfVerticesLod2 - 1.0f);
                        if (percentOfVerticesLod3 <= 0)
                            percentOfVerticesLod3 = percentOfVerticesLod2;
                    }
                if (levelsOfDetailToGenerate >= 4)
                    if (percentOfVerticesLod4 > percentOfVerticesLod3)
                    {
                        percentOfVerticesLod4 = (percentOfVerticesLod3 - 1.0f);
                        if (percentOfVerticesLod4 <= 0)
                            percentOfVerticesLod4 = percentOfVerticesLod3;
                    }
                if (levelsOfDetailToGenerate >= 5)
                    if (percentOfVerticesLod5 > percentOfVerticesLod4)
                    {
                        percentOfVerticesLod5 = (percentOfVerticesLod4 - 1.0f);
                        if (percentOfVerticesLod5 <= 0)
                            percentOfVerticesLod5 = percentOfVerticesLod4;
                    }
                if (levelsOfDetailToGenerate >= 6)
                    if (percentOfVerticesLod6 > percentOfVerticesLod5)
                    {
                        percentOfVerticesLod6 = (percentOfVerticesLod5 - 1.0f);
                        if (percentOfVerticesLod6 <= 0)
                            percentOfVerticesLod6 = percentOfVerticesLod5;
                    }
                if (levelsOfDetailToGenerate >= 7)
                    if (percentOfVerticesLod7 > percentOfVerticesLod6)
                    {
                        percentOfVerticesLod7 = (percentOfVerticesLod6 - 1.0f);
                        if (percentOfVerticesLod7 <= 0)
                            percentOfVerticesLod7 = percentOfVerticesLod6;
                    }
                if (levelsOfDetailToGenerate >= 8)
                    if (percentOfVerticesLod8 > percentOfVerticesLod7)
                    {
                        percentOfVerticesLod8 = (percentOfVerticesLod7 - 1.0f);
                        if (percentOfVerticesLod8 <= 0)
                            percentOfVerticesLod8 = percentOfVerticesLod7;
                    }

                //Validate the distances
                if (levelsOfDetailToGenerate >= 8)
                    if (minDistanceOfViewForLod7 >= minDistanceOfViewForLod8)
                    {
                        minDistanceOfViewForLod7 = (minDistanceOfViewForLod8 - 1.0f);
                        if (minDistanceOfViewForLod7 <= 0)
                            minDistanceOfViewForLod7 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 7)
                    if (minDistanceOfViewForLod6 >= minDistanceOfViewForLod7)
                    {
                        minDistanceOfViewForLod6 = (minDistanceOfViewForLod7 - 1.0f);
                        if (minDistanceOfViewForLod6 <= 0)
                            minDistanceOfViewForLod6 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 6)
                    if (minDistanceOfViewForLod5 >= minDistanceOfViewForLod6)
                    {
                        minDistanceOfViewForLod5 = (minDistanceOfViewForLod6 - 1.0f);
                        if (minDistanceOfViewForLod5 <= 0)
                            minDistanceOfViewForLod5 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 5)
                    if (minDistanceOfViewForLod4 >= minDistanceOfViewForLod5)
                    {
                        minDistanceOfViewForLod4 = (minDistanceOfViewForLod5 - 1.0f);
                        if (minDistanceOfViewForLod4 <= 0)
                            minDistanceOfViewForLod4 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 4)
                    if (minDistanceOfViewForLod3 >= minDistanceOfViewForLod4)
                    {
                        minDistanceOfViewForLod3 = (minDistanceOfViewForLod4 - 1.0f);
                        if (minDistanceOfViewForLod3 <= 0)
                            minDistanceOfViewForLod3 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 3)
                    if (minDistanceOfViewForLod2 >= minDistanceOfViewForLod3)
                    {
                        minDistanceOfViewForLod2 = (minDistanceOfViewForLod3 - 1.0f);
                        if (minDistanceOfViewForLod2 <= 0)
                            minDistanceOfViewForLod2 = 1.0f;
                    }
                if (levelsOfDetailToGenerate >= 2)
                    if (minDistanceOfViewForLod1 >= minDistanceOfViewForLod2)
                    {
                        minDistanceOfViewForLod1 = (minDistanceOfViewForLod2 - 1.0f);
                        if (minDistanceOfViewForLod1 <= 0)
                            minDistanceOfViewForLod1 = 1.0f;
                    }

                if (minDistanceOfViewForCull <= GetMinDistanceValueFromLastLevel())
                    minDistanceOfViewForCull = (GetMinDistanceValueFromLastLevel() + 10.0f);
            }
        }

        public void ShowMinDistanceToViewLodInSceneView(float distance)
        {
            //Calculate the distance of camera of scene view, set distance to this gameobject, and rotate scene view to this gameobject
            GameObject baseGameObject = new GameObject("tempObjBase");
            baseGameObject.transform.SetParent(this.gameObject.transform);
            baseGameObject.transform.localPosition = Vector3.zero;
            GameObject distanceGameObject = new GameObject("tempObjDistance");
            distanceGameObject.transform.SetParent(baseGameObject.transform);
            distanceGameObject.transform.localPosition = Vector3.zero;
            Vector3 position = distanceGameObject.transform.localPosition;
            position.z = distance - ((SceneView.lastActiveSceneView.cameraDistance <= distance) ? SceneView.lastActiveSceneView.cameraDistance : 0);
            distanceGameObject.transform.localPosition = position;
            baseGameObject.transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
            SceneView.lastActiveSceneView.LookAt(distanceGameObject.transform.position);
            distanceGameObject.transform.LookAt(baseGameObject.transform.position);
            SceneView.lastActiveSceneView.rotation = distanceGameObject.transform.rotation;
            SceneView.lastActiveSceneView.Repaint();
            DestroyImmediate(baseGameObject, false);
        }

        public Mesh GetGeneratedLodForThisMesh(Mesh originalMesh, float percentOfVertices, bool isSkinnedMesh)
        {
            //Simplification multiplier
            float multiplier = 0.00001f;

            //Return the mesh converted to LOD
            UnityMeshSimplifier.MeshSimplifier meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            UnityMeshSimplifier.SimplificationOptions meshSimplificationSettings = new UnityMeshSimplifier.SimplificationOptions();
            switch (forceOfSimplification)
            {
                case ForceOfSimplification.Normal:
                    meshSimplificationSettings.Agressiveness = 7.0f;
                    multiplier = 1.0f;
                    break;
                case ForceOfSimplification.Strong:
                    meshSimplificationSettings.Agressiveness = 8.5f;
                    multiplier = 0.8f;
                    break;
                case ForceOfSimplification.VeryStrong:
                    meshSimplificationSettings.Agressiveness = 10.0f;
                    multiplier = 0.6f;
                    break;
                case ForceOfSimplification.ExtremelyStrong:
                    meshSimplificationSettings.Agressiveness = 12.0f;
                    multiplier = 0.4f;
                    break;
                case ForceOfSimplification.Destroyer:
                    meshSimplificationSettings.Agressiveness = 14.0f;
                    multiplier = 0.2f;
                    break;
            }
            if (preventArtifacts == true)
                meshSimplificationSettings.EnableSmartLink = true;
            if (preventArtifacts == false)
                meshSimplificationSettings.EnableSmartLink = false;
            meshSimplificationSettings.MaxIterationCount = 100;
            meshSimplificationSettings.PreserveBorderEdges = false;
            meshSimplificationSettings.PreserveSurfaceCurvature = false;
            meshSimplificationSettings.PreserveUVFoldoverEdges = false;
            meshSimplificationSettings.PreserveUVSeamEdges = false;
            meshSimplificationSettings.VertexLinkDistance = double.Epsilon;
            meshSimplifier.SimplificationOptions = meshSimplificationSettings;
            meshSimplifier.Initialize(originalMesh);
            meshSimplifier.SimplifyMesh((percentOfVertices / 100.0f) * multiplier);
            Mesh resultMesh = meshSimplifier.ToMesh();
            if (isSkinnedMesh == true && skinnedAnimsCompatibilityMode == true)
                resultMesh.bindposes = originalMesh.bindposes;
            return resultMesh;
        }

        public void CreateHierarchyOfFoldersIfNotExists()
        {
            //Create the directory in project
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Meshes");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_1"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_1");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_2"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_2");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_3"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_3");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_4"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_4");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_5"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_5");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_6"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_6");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_7"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_7");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes/LODs_8"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData/Meshes", "LODs_8");
        }

        public string SaveGeneratedLodInAssets(string lodNumber, long ticks, Mesh generatedLodMesh)
        {
            //Save generated LOD in assets, if desired
            if (saveGeneratedLodsInAssets == true)
            {
                string path = "Assets/MT Assets/_AssetsData/Meshes/LODs_" + lodNumber + "/LOD" + lodNumber + "_" + ticks + ".asset";
                AssetDatabase.CreateAsset(generatedLodMesh, path);
                return path;
            }
            return "";
        }

        public void ScanForMeshesAndGenerateAllLodGroups(bool showProgressBar)
        {
            //Validate all parameters
            ValidateAllParameters(true);

            //Show progressbar
            if (showProgressBar == true)
                EditorUtility.DisplayProgressBar("Scanning...", "Please wait...", 0.0f);

            //Storage all valid meshes found
            List<SkinnedMeshRenderer> skinnedMeshRenderersFound = new List<SkinnedMeshRenderer>();
            List<MeshFilter> meshFiltersFound = new List<MeshFilter>();

            //Get all meshes
            if (modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
            {
                //If scan in children is desired
                SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(scanInactiveGameObjects);
                foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                    //Check all conditions before add this mesh as valid
                    if (renderer != null && gameObjectsToIgnore.Contains(renderer.gameObject) == false)
                        if (renderer.gameObject.GetComponent<UltimateLevelOfDetail>() == null)
                            if (renderer.sharedMesh != null)
                                skinnedMeshRenderersFound.Add(renderer);

                MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(scanInactiveGameObjects);
                foreach (MeshFilter filter in meshFilters)
                    //Check all conditions before add this mesh as valid
                    if (filter != null && gameObjectsToIgnore.Contains(filter.gameObject) == false)
                        if (filter.gameObject.GetComponent<UltimateLevelOfDetail>() == null)
                            if (filter.sharedMesh != null)
                                meshFiltersFound.Add(filter);
            }
            if (modeOfMeshesScanning == ScanMeshesMode.ScanInThisGameObjectOnly)
            {
                //If scan in childer is not desire
                SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                    skinnedMeshRenderersFound.Add(skinnedMeshRenderer);
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter != null)
                    meshFiltersFound.Add(meshFilter);
            }

            //Cancel if not found meshes
            if (skinnedMeshRenderersFound.Count == 0 && meshFiltersFound.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("No mesh was found in GameObject \"" + this.gameObject.name + "\" or its child GameObjects. Please check the GameObjects hierarchy and the settings for this component and try again.");
                return;
            }

            //Create list of OriginalMeshItems
            List<OriginalMeshItem> originalMeshItems = new List<OriginalMeshItem>();

            //Create folder to store LODs in project
            CreateHierarchyOfFoldersIfNotExists();

            //Create progress stats
            float totalMeshes = skinnedMeshRenderersFound.Count + meshFiltersFound.Count;
            float totalLodsProgress = ((skinnedMeshRenderersFound.Count * GetNumberOfLodsGenerated()) + (meshFiltersFound.Count * GetNumberOfLodsGenerated()));
            float currentMesh = 0;
            float currentLod = 0;

            //Add first, skinned mesh renderers
            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderersFound)
            {
                //Get current date
                DateTime date = DateTime.UtcNow;
                long ticks = date.Ticks + (long)currentMesh;
                currentMesh += 1;

                OriginalMeshItem originalMeshItem = new OriginalMeshItem();
                originalMeshItem.originalGameObject = smr.gameObject;
                originalMeshItem.skinnedMeshRenderer = smr;
                originalMeshItem.originalMesh = smr.sharedMesh;

                //LOD 1
                if (showProgressBar == true)
                    EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                originalMeshItem.meshLod1 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod1, true);
                originalMeshItem.meshLod1Path = SaveGeneratedLodInAssets("1", ticks, originalMeshItem.meshLod1);
                currentLod += 1;

                //LOD 2
                if (levelsOfDetailToGenerate >= 2)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod2 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod2, true);
                    originalMeshItem.meshLod2Path = SaveGeneratedLodInAssets("2", ticks, originalMeshItem.meshLod2);
                    currentLod += 1;
                }

                //LOD 3
                if (levelsOfDetailToGenerate >= 3)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod3 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod3, true);
                    originalMeshItem.meshLod3Path = SaveGeneratedLodInAssets("3", ticks, originalMeshItem.meshLod3);
                    currentLod += 1;
                }

                //LOD 4
                if (levelsOfDetailToGenerate >= 4)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod4 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod4, true);
                    originalMeshItem.meshLod4Path = SaveGeneratedLodInAssets("4", ticks, originalMeshItem.meshLod4);
                    currentLod += 1;
                }

                //LOD 5
                if (levelsOfDetailToGenerate >= 5)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod5 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod5, true);
                    originalMeshItem.meshLod5Path = SaveGeneratedLodInAssets("5", ticks, originalMeshItem.meshLod5);
                    currentLod += 1;
                }

                //LOD 6
                if (levelsOfDetailToGenerate >= 6)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod6 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod6, true);
                    originalMeshItem.meshLod6Path = SaveGeneratedLodInAssets("6", ticks, originalMeshItem.meshLod6);
                    currentLod += 1;
                }

                //LOD 7
                if (levelsOfDetailToGenerate >= 7)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod7 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod7, true);
                    originalMeshItem.meshLod7Path = SaveGeneratedLodInAssets("7", ticks, originalMeshItem.meshLod7);
                    currentLod += 1;
                }

                //LOD 8
                if (levelsOfDetailToGenerate >= 8)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod8 = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesLod8, true);
                    originalMeshItem.meshLod8Path = SaveGeneratedLodInAssets("8", ticks, originalMeshItem.meshLod8);
                    currentLod += 1;
                }

                //Create the ULODMeshes for this Mesh GameObject
                originalMeshItem.meshesManager = originalMeshItem.originalGameObject.AddComponent<UltimateLevelOfDetailMeshes>();
                originalMeshItem.meshesManager.responsibleUlod = this;
                originalMeshItem.meshesManager.idOfOriginalMeshItemOfThisInResponsibleUlod = (int)currentMesh - 1;

                originalMeshItems.Add(originalMeshItem);
            }

            //Add, mesh filters
            foreach (MeshFilter mf in meshFiltersFound)
            {
                //Get current date
                DateTime date = DateTime.UtcNow;
                long ticks = date.Ticks + (long)currentMesh;
                currentMesh += 1;

                OriginalMeshItem originalMeshItem = new OriginalMeshItem();
                originalMeshItem.originalGameObject = mf.gameObject;
                originalMeshItem.meshFilter = mf;
                originalMeshItem.meshRenderer = mf.GetComponent<MeshRenderer>();
                originalMeshItem.originalMesh = mf.sharedMesh;

                //LOD 1
                if (showProgressBar == true)
                    EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                originalMeshItem.meshLod1 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod1, false);
                originalMeshItem.meshLod1Path = SaveGeneratedLodInAssets("1", ticks, originalMeshItem.meshLod1);
                currentLod += 1;

                //LOD 2
                if (levelsOfDetailToGenerate >= 2)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod2 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod2, false);
                    originalMeshItem.meshLod2Path = SaveGeneratedLodInAssets("2", ticks, originalMeshItem.meshLod2);
                    currentLod += 1;
                }

                //LOD 3
                if (levelsOfDetailToGenerate >= 3)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod3 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod3, false);
                    originalMeshItem.meshLod3Path = SaveGeneratedLodInAssets("3", ticks, originalMeshItem.meshLod3);
                    currentLod += 1;
                }

                //LOD 4
                if (levelsOfDetailToGenerate >= 4)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod4 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod4, false);
                    originalMeshItem.meshLod4Path = SaveGeneratedLodInAssets("4", ticks, originalMeshItem.meshLod4);
                    currentLod += 1;
                }

                //LOD 5
                if (levelsOfDetailToGenerate >= 5)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod5 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod5, false);
                    originalMeshItem.meshLod5Path = SaveGeneratedLodInAssets("5", ticks, originalMeshItem.meshLod5);
                    currentLod += 1;
                }

                //LOD 6
                if (levelsOfDetailToGenerate >= 6)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod6 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod6, false);
                    originalMeshItem.meshLod6Path = SaveGeneratedLodInAssets("6", ticks, originalMeshItem.meshLod6);
                    currentLod += 1;
                }

                //LOD 7
                if (levelsOfDetailToGenerate >= 7)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod7 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod7, false);
                    originalMeshItem.meshLod7Path = SaveGeneratedLodInAssets("7", ticks, originalMeshItem.meshLod7);
                    currentLod += 1;
                }

                //LOD 8
                if (levelsOfDetailToGenerate >= 8)
                {
                    if (showProgressBar == true)
                        EditorUtility.DisplayProgressBar("Please wait...", "Generating LOD Group (For Mesh " + currentMesh + "/" + totalMeshes + ")", currentLod / totalLodsProgress);
                    originalMeshItem.meshLod8 = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesLod8, false);
                    originalMeshItem.meshLod8Path = SaveGeneratedLodInAssets("8", ticks, originalMeshItem.meshLod8);
                    currentLod += 1;
                }

                //Create the ULODMeshes for this Mesh GameObject
                originalMeshItem.meshesManager = originalMeshItem.originalGameObject.AddComponent<UltimateLevelOfDetailMeshes>();
                originalMeshItem.meshesManager.responsibleUlod = this;
                originalMeshItem.meshesManager.idOfOriginalMeshItemOfThisInResponsibleUlod = (int)currentMesh - 1;

                originalMeshItems.Add(originalMeshItem);
            }

            //Save the list
            originalMeshesFoundInLastScan = originalMeshItems;
            AssetDatabase.Refresh();

            //Delete progressbar and finalize
            if (showProgressBar == true)
                EditorUtility.ClearProgressBar();
            Debug.Log("All meshes present in GameObject \"" + this.gameObject.name + "\" or its child GameObjects have been scanned and have groups of LODs created automatically based on the parameters you have defined. Now just move the camera away and watch the magic of optimization happen!");
        }

        public void DeleteAllMeshesScannedAndAllLodGroups(bool showProgressBar)
        {
            //Show progressbar
            if (showProgressBar == true)
                EditorUtility.DisplayProgressBar("Deleting...", "Please wait...", 1.0f);

            //Delete all data and reset originalMeshesFoundInLastScan
            foreach (OriginalMeshItem meshItem in originalMeshesFoundInLastScan)
            {
                //If is desired to save, delete all assets
                if (saveGeneratedLodsInAssets == true)
                {
                    if (meshItem.meshLod1Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod1Path);
                    if (meshItem.meshLod2Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod2Path);
                    if (meshItem.meshLod3Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod3Path);
                    if (meshItem.meshLod4Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod4Path);
                    if (meshItem.meshLod5Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod5Path);
                    if (meshItem.meshLod6Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod6Path);
                    if (meshItem.meshLod7Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod7Path);
                    if (meshItem.meshLod8Path != "")
                        AssetDatabase.DeleteAsset(meshItem.meshLod8Path);
                }

                //Restore original mesh
                if (meshItem.skinnedMeshRenderer != null)
                    meshItem.skinnedMeshRenderer.sharedMesh = meshItem.originalMesh;
                if (meshItem.meshFilter != null)
                    meshItem.meshFilter.sharedMesh = meshItem.originalMesh;

                //Delete all ULODMeshes
                if (meshItem.meshesManager != null)
                    DestroyImmediate(meshItem.meshesManager);
            }

            //Delete all
            originalMeshesFoundInLastScan.Clear();
            AssetDatabase.Refresh();

            //Restore important variables values
            lastDistanceFromMainCamera = -1f;
            currentLodAccordingToDistance = -1;

            //Delete progressbar and finalize
            if (showProgressBar == true)
                EditorUtility.ClearProgressBar();
            Debug.Log("All scanned meshes in GameObject \"" + this.gameObject.name + "\" were restored to the original meshes and the LOD groups were deleted.");
        }
#endif
        //Universal Methods

        private Camera GetLastActiveSceneViewCamera()
        {
#if UNITY_EDITOR
            //Return the scene camera, if is editor
            if (cacheOfLastActiveSceneViewCamera == null && SceneView.lastActiveSceneView != null)
                cacheOfLastActiveSceneViewCamera = SceneView.lastActiveSceneView.camera;
            return cacheOfLastActiveSceneViewCamera;
#endif
#if !UNITY_EDITOR
            //Return null, if is the build
            return null;
#endif
        }

        private void CullThisLodMeshOfRenderer(OriginalMeshItem meshItem)
        {
            //If culling is disabled, return
            if (cullingMode == CullingMode.Disabled)
                return;

            //If culling method is "CullingMeshes"
            if (cullingMode == CullingMode.CullingMeshes)
            {
                if (meshItem.skinnedMeshRenderer != null)
                    if (meshItem.skinnedMeshRenderer.sharedMesh != null)
                    {
                        meshItem.beforeCullingData_lastMeshOfThis = meshItem.skinnedMeshRenderer.sharedMesh;
                        meshItem.skinnedMeshRenderer.sharedMesh = null;
                    }
                if (meshItem.meshFilter != null)
                    if (meshItem.meshFilter.sharedMesh != null)
                    {
                        meshItem.beforeCullingData_lastMeshOfThis = meshItem.meshFilter.sharedMesh;
                        meshItem.meshFilter.sharedMesh = null;
                    }
            }

            //If culling method is "CullingMaterials"
            if (cullingMode == CullingMode.CullingMaterials)
            {
                if (meshItem.skinnedMeshRenderer != null)
                    if (meshItem.skinnedMeshRenderer.sharedMaterials.Length > 0)
                    {
                        meshItem.beforeCullingData_lastMaterialArray = meshItem.skinnedMeshRenderer.sharedMaterials;
                        meshItem.skinnedMeshRenderer.sharedMaterials = meshItem.beforeCullingData_emptyMaterialArray;
                    }
                if (meshItem.meshFilter != null && meshItem.meshRenderer != null)
                    if (meshItem.meshRenderer.sharedMaterials.Length > 0)
                    {
                        meshItem.beforeCullingData_lastMaterialArray = meshItem.meshRenderer.sharedMaterials;
                        meshItem.meshRenderer.sharedMaterials = meshItem.beforeCullingData_emptyMaterialArray;
                    }
            }
        }

        private void UncullThisLodMeshOfRenderer(OriginalMeshItem meshItem)
        {
            //If culling is disabled, return, if is playing
            if (cullingMode == CullingMode.Disabled)
                return;

            //If culling method is "CullingMeshes"
            if (cullingMode == CullingMode.CullingMeshes)
            {
                if (meshItem.skinnedMeshRenderer != null)
                    if (meshItem.skinnedMeshRenderer.sharedMesh == null)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.beforeCullingData_lastMeshOfThis;
                if (meshItem.meshFilter != null)
                    if (meshItem.meshFilter.sharedMesh == null)
                        meshItem.meshFilter.sharedMesh = meshItem.beforeCullingData_lastMeshOfThis;
            }

            //If culling method is "CullingMaterials"
            if (cullingMode == CullingMode.CullingMaterials)
            {
                if (meshItem.skinnedMeshRenderer != null)
                    if (meshItem.skinnedMeshRenderer.sharedMaterials.Length == 0)
                        meshItem.skinnedMeshRenderer.sharedMaterials = meshItem.beforeCullingData_lastMaterialArray;
                if (meshItem.meshFilter != null && meshItem.meshRenderer != null)
                    if (meshItem.meshRenderer.sharedMaterials.Length == 0)
                        meshItem.meshRenderer.sharedMaterials = meshItem.beforeCullingData_lastMaterialArray;
            }
        }

        private void ChangeLodMeshOfRenderer(OriginalMeshItem meshItem, int lodLevel)
        {
            //If is a Skinned Mesh Renderer
            if (meshItem.skinnedMeshRenderer != null)
            {
                //Prepare the change of lods skinned parameter in editor
                bool canChangeLods = false;
                if (Application.isEditor == false)
                    canChangeLods = true;
                if (Application.isEditor == true && forceChangeLodsOfSkinnedInEditor == true)
                    canChangeLods = true;

                //Change the mesh, according to the current required lod level
                if (canChangeLods == true)
                {
                    if (lodLevel == 0)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.originalMesh;
                    if (lodLevel == 1)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod1;
                    if (lodLevel == 2)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod2;
                    if (lodLevel == 3)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod3;
                    if (lodLevel == 4)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod4;
                    if (lodLevel == 5)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod5;
                    if (lodLevel == 6)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod6;
                    if (lodLevel == 7)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod7;
                    if (lodLevel == 8)
                        meshItem.skinnedMeshRenderer.sharedMesh = meshItem.meshLod8;

                    //Enable and disable mesh to avoid log of errors and artifacts on update mesh bones data
                    meshItem.skinnedMeshRenderer.enabled = false;
                    meshItem.skinnedMeshRenderer.enabled = true;
                }
            }

            //If is a Mesh Renderer
            if (meshItem.meshFilter != null)
            {
                //Change the mesh, according to the current required lod level
                if (lodLevel == 0)
                    meshItem.meshFilter.sharedMesh = meshItem.originalMesh;
                if (lodLevel == 1)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod1;
                if (lodLevel == 2)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod2;
                if (lodLevel == 3)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod3;
                if (lodLevel == 4)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod4;
                if (lodLevel == 5)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod5;
                if (lodLevel == 6)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod6;
                if (lodLevel == 7)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod7;
                if (lodLevel == 8)
                    meshItem.meshFilter.sharedMesh = meshItem.meshLod8;
            }
        }

        private void CalculateCorrectLodForDistanceBeforeChange(float distance)
        {
            //If not have a scan, cancel
            if (originalMeshesFoundInLastScan.Count == 0)
                return;

            //Change the meshs according to distance of current main camera
            if (lastDistanceFromMainCamera != distance)
            {
                //Pre calculate the lod for apply for meshes
                int lodLevelForApplyInThisMeshes = -1;

                //Original LOD
                if (distance < minDistanceOfViewForLod1)
                    lodLevelForApplyInThisMeshes = 0;
                //LOD 1
                if (distance >= minDistanceOfViewForLod1)
                    lodLevelForApplyInThisMeshes = 1;
                //LOD 2
                if (levelsOfDetailToGenerate >= 2)
                    if (distance >= minDistanceOfViewForLod2)
                        lodLevelForApplyInThisMeshes = 2;
                //LOD 3
                if (levelsOfDetailToGenerate >= 3)
                    if (distance >= minDistanceOfViewForLod3)
                        lodLevelForApplyInThisMeshes = 3;
                //LOD 4
                if (levelsOfDetailToGenerate >= 4)
                    if (distance >= minDistanceOfViewForLod4)
                        lodLevelForApplyInThisMeshes = 4;
                //LOD 5
                if (levelsOfDetailToGenerate >= 5)
                    if (distance >= minDistanceOfViewForLod5)
                        lodLevelForApplyInThisMeshes = 5;
                //LOD 6
                if (levelsOfDetailToGenerate >= 6)
                    if (distance >= minDistanceOfViewForLod6)
                        lodLevelForApplyInThisMeshes = 6;
                //LOD 7
                if (levelsOfDetailToGenerate >= 7)
                    if (distance >= minDistanceOfViewForLod7)
                        lodLevelForApplyInThisMeshes = 7;
                //LOD 8
                if (levelsOfDetailToGenerate >= 8)
                    if (distance >= minDistanceOfViewForLod8)
                        lodLevelForApplyInThisMeshes = 8;
                //Cull (Only occurs if Culling Mode not is disabled)
                if (cullingMode != CullingMode.Disabled)
                    if (distance >= minDistanceOfViewForCull)
                        lodLevelForApplyInThisMeshes = 9;

                //Apply the lod level pre calculated, if is not culling
                if (lodLevelForApplyInThisMeshes >= 0 && lodLevelForApplyInThisMeshes < 9)
                    if (currentLodAccordingToDistance != lodLevelForApplyInThisMeshes)
                        foreach (OriginalMeshItem meshItem in originalMeshesFoundInLastScan)
                        {
                            UncullThisLodMeshOfRenderer(meshItem);
                            ChangeLodMeshOfRenderer(meshItem, lodLevelForApplyInThisMeshes);
                            currentLodAccordingToDistance = lodLevelForApplyInThisMeshes;
                        }
                //If the distance is needed, and culling enabled, cull meshes
                if (lodLevelForApplyInThisMeshes == 9)
                    if (currentLodAccordingToDistance != 9)
                        foreach (OriginalMeshItem meshItem in originalMeshesFoundInLastScan)
                        {
                            CullThisLodMeshOfRenderer(meshItem);
                            currentLodAccordingToDistance = 9;
                        }

                //Set the new last distance from camera
                lastDistanceFromMainCamera = distance;
            }
        }

        public void OnRenderObject()
        {
            //If ulod system is disabled, cancel auto management and restore original meshes
            if (UltimateLevelOfDetailGlobal.isGlobalULodSystemEnabled() == false)
            {
                CalculateCorrectLodForDistanceBeforeChange(0);
                return;
            }
            //If this ulod component is forced to be disabled, restore original meshes and stop
            if (forcedToDisableLodsOfThisComponent == true)
            {
                CalculateCorrectLodForDistanceBeforeChange(0);
                return;
            }

            //Get correct camera, according to parameter defined
            Camera lastActiveSceneViewCamera = GetLastActiveSceneViewCamera();
            Camera cameraToCalcDistance = null;

            //If is mode "MainCamera"
            if (cameraDetectionMode == CameraDetectionMode.MainCamera && Application.isPlaying == true)
            {
                //If cache is disabled, get the main camera in each renderization
                if (useCacheForMainCameraInDetection == false)
                    cameraToCalcDistance = Camera.main;
                //If cache is enabled, get the main camera of cache, if is possible
                if (useCacheForMainCameraInDetection == true)
                {
                    //Invalid the cache if camera of cache is disabled or in a gameobject disabled
                    if (cacheOfMainCamera != null)
                        if (cacheOfMainCamera.enabled == false || cacheOfMainCamera.gameObject.activeSelf == false)
                            cacheOfMainCamera = null;
                    //If cache is null or invalid, try to find another main camera to fill the ache
                    if (cacheOfMainCamera == null)
                        cacheOfMainCamera = Camera.main;
                    //If cache is valid or not null, get the camera of cache
                    if (cacheOfMainCamera != null)
                        cameraToCalcDistance = cacheOfMainCamera;
                }
                //If is not possible to get the main camera
                if (cameraToCalcDistance == null)
                    Debug.LogError("It was not possible to find a main camera to calculate LODs. Please make sure that the main camera in your scene has the \"MainCamera\" tag defined in the GameObject in which it is located.");
            }
            //If is mode "CurrentCamera"
            if (cameraDetectionMode == CameraDetectionMode.CurrentCamera && Application.isPlaying == true)
            {
                //Get the current camera determined by the componente "Runtime Camera Detector" in this scene
                cameraToCalcDistance = UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene;
                //If the current camera on top of screen is null, or component "Runtime Camera Detector" impossible to find a camera
                if (cameraToCalcDistance == null)
                    Debug.LogError("It was not possible to find a current camera at the moment, it seems that there are no cameras in the scene, or Unity was unable to make references. Please try to switch to \"Main Camera\" mode.");
            }
            //If is mode "CustomCamera"
            if (cameraDetectionMode == CameraDetectionMode.CustomCamera && Application.isPlaying == true)
            {
                //Get the custom camera determined by user
                cameraToCalcDistance = customCameraForSimulationOfLods;
                //If is not possible to get custom camera
                if (cameraToCalcDistance == null)
                    Debug.LogError("No custom camera for calculating distance and simulating LODs has been provided in \"" + this.gameObject.name + "\".");
            }

            //If is not playing, and in editor, get the camera of scene view
            if (Application.isEditor == true && Application.isPlaying == false)
            {
                cameraToCalcDistance = lastActiveSceneViewCamera;
                if (cameraToCalcDistance == null)
                    Debug.LogError("It was not possible to find a camera that is currently viewing a scene. Make sure the scene view window is active and in focus.");
            }

            //Start the calc of distance to change LODs according to the camera detected
            if (cameraToCalcDistance != null)
            {
                currentDistanceFromMainCamera = (Vector3.Distance(this.gameObject.transform.position, cameraToCalcDistance.transform.position) * UltimateLevelOfDetailGlobal.GetGlobalLodDistanceMultiplier());
                currentRealDistanceFromMainCamera = Vector3.Distance(this.gameObject.transform.position, cameraToCalcDistance.transform.position);
            }
            if (cameraToCalcDistance == null)
            {
                currentDistanceFromMainCamera = 0;
                currentRealDistanceFromMainCamera = 0;
            }

            //Start lod calculation with the current distance
            CalculateCorrectLodForDistanceBeforeChange(currentDistanceFromMainCamera);
        }

        public void Awake()
        {
            //Start the game setting original mesh lod 0
            CalculateCorrectLodForDistanceBeforeChange(0);

            //Create the Runtime Camera Detector GameObject in this scene, if not exists
            GameObject cameraDetector = GameObject.Find("ULOD Runtime Camera Detector");
            if (cameraDetector == null && Application.isPlaying == true)
            {
                cameraDetector = new GameObject("ULOD Runtime Camera Detector");
                RuntimeCameraDetector runtimeCameraDetector = cameraDetector.AddComponent<RuntimeCameraDetector>();
            }
        }

        //API methods

        public int GetCurrentLodLevel()
        {
            //Return the current lod level of this
            if (currentLodAccordingToDistance != 9)
                return currentLodAccordingToDistance;
            if (currentLodAccordingToDistance == 9)
                return (GetNumberOfLodsGenerated() - 1);
            return 0;
        }

        public float GetCurrentCameraDistance()
        {
            //return the current camera distance from this object
            return currentDistanceFromMainCamera;
        }

        public float GetCurrentRealCameraDistance()
        {
            //return the current camera distance from this object, without multiplier
            return currentRealDistanceFromMainCamera;
        }

        public int GetNumberOfLodsGenerated()
        {
            //Count and return the number of LODs generated in this component
            int count = 1;
            if (levelsOfDetailToGenerate >= 2)
                count += 1;
            if (levelsOfDetailToGenerate >= 3)
                count += 1;
            if (levelsOfDetailToGenerate >= 4)
                count += 1;
            if (levelsOfDetailToGenerate >= 5)
                count += 1;
            if (levelsOfDetailToGenerate >= 6)
                count += 1;
            if (levelsOfDetailToGenerate >= 7)
                count += 1;
            if (levelsOfDetailToGenerate >= 8)
                count += 1;
            return count;
        }

        public bool isScannedMeshesCurrentCulled()
        {
            //Return true if all meshes scanned by this ULOD, is culled
            if (currentLodAccordingToDistance == 9)
                return true;
            if (currentLodAccordingToDistance != 9)
                return false;
            return false;
        }

        public UltimateLevelOfDetailMeshes[] GetListOfAllMeshesScanned()
        {
            //Create the list of all meshes scanned
            List<UltimateLevelOfDetailMeshes> list = new List<UltimateLevelOfDetailMeshes>();
            foreach (OriginalMeshItem meshItem in originalMeshesFoundInLastScan)
                list.Add(meshItem.meshesManager);
            return list.ToArray();
        }

        public void ForceDisableLodChangesInThisComponent(bool force)
        {
            //Force or not the disable of lod changes
            forcedToDisableLodsOfThisComponent = force;
        }

        public bool isThisComponentForcedToDisableLodChanges()
        {
            //Return true if is forced to disable lod changes
            return forcedToDisableLodsOfThisComponent;
        }

        public void ForceThisComponentToUpdateLodsRender()
        {
            //Force this component to update and change meshes, does not matter current distance, current lod and etc
            lastDistanceFromMainCamera = lastDistanceFromMainCamera + UnityEngine.Random.Range(0.1f, 1.0f);
            currentLodAccordingToDistance = -1;
            OnRenderObject();
        }
    }
}