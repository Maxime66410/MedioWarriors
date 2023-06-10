#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
    /*
     This class is responsible for the functioning of the "Ultimate Level Of Detail Meshes" component, and all its functions.
    */
    /*
     * The Ultimate LOD System was developed by Marcos Tomaz in 2020.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [AddComponentMenu("")] //Hide this script in component menu.
    public class UltimateLevelOfDetailMeshes : MonoBehaviour
    {
        //Public variables
        [HideInInspector]
        public UltimateLevelOfDetail responsibleUlod = null;
        [HideInInspector]
        public int idOfOriginalMeshItemOfThisInResponsibleUlod = -1;

#if UNITY_EDITOR
        //Public variables of Interface
        private bool gizmosOfThisComponentIsDisabled = false;

        //Editor auto update on change meshes variables
        [HideInInspector]
        public Mesh lastLod0 = null;
        [HideInInspector]
        public Mesh lastLod1 = null;
        [HideInInspector]
        public Mesh lastLod2 = null;
        [HideInInspector]
        public Mesh lastLod3 = null;
        [HideInInspector]
        public Mesh lastLod4 = null;
        [HideInInspector]
        public Mesh lastLod5 = null;
        [HideInInspector]
        public Mesh lastLod6 = null;
        [HideInInspector]
        public Mesh lastLod7 = null;
        [HideInInspector]
        public Mesh lastLod8 = null;

        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(UltimateLevelOfDetailMeshes))]
        public class CustomInspector : UnityEditor.Editor
        {
            //Private variables of Editor Only
            private Vector2 gameObjectsToIgnoreScrollpos = Vector2.zero;
            private Vector2 gameObjectsFoundInLastScanScrollpos = Vector2.zero;
            private Vector2 ulodsListScrollpos = Vector2.zero;

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
                UltimateLevelOfDetailMeshes script = (UltimateLevelOfDetailMeshes)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");
                script.gizmosOfThisComponentIsDisabled = DisableGizmosInSceneView("UltimateLevelOfDetailMeshes", script.gizmosOfThisComponentIsDisabled);

                //If responsible ULOD not exists
                if (script.responsibleUlod == null || script.idOfOriginalMeshItemOfThisInResponsibleUlod == -1)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("It was not possible to find the ULOD component responsible for managing the LODs of this network. Apparently it has been deleted or no longer exists.", MessageType.Error);
                    GUILayout.Space(10);
                    if (GUILayout.Button("Delete This Component"))
                        DestroyImmediate(script);
                    GUILayout.Space(10);
                }
                //If responsible ULOD exists
                if (script.responsibleUlod != null && script.idOfOriginalMeshItemOfThisInResponsibleUlod != -1)
                {
                    //About
                    EditorGUILayout.HelpBox("Below are all the mesh levels that make up the LOD group of this mesh. This component was added automatically by ULOD the last time it scanned and generated the LOD groups. Currently, the responsible ULOD component is manipulating this mesh and will change the meshes according to the distance between the responsible ULOD and the camera that is rendering its meshes. Feel free to provide your own meshes for each LOD level of this mesh. If necessary, you can restore all meshes that were originally generated by ULOD.\nIf you need to change the mesh that will be rendered in this renderer, during the runtime, it is highly recommended that you provide the mesh here, so that ULOD can add your mesh to the LOD stream correctly and manage the LODs smoothly. You can use this component's API to switch these meshes during runtime.", MessageType.Info);
                    EditorGUILayout.HelpBox("If you need to change the original mesh of a renderer, the most correct way to do this is to change the original mesh here in this component. That way the Ultimate LOD System will know that the new original mesh is the one you want, and then render it at the right times. You can define the new original mesh here in this component, or by C# API of this component.", MessageType.Info);

                    GUILayout.Space(10);

                    GUIStyle titulo = new GUIStyle();
                    titulo.fontSize = 16;
                    titulo.normal.textColor = new Color(0, 79.0f / 250.0f, 3.0f / 250.0f);
                    titulo.alignment = TextAnchor.MiddleCenter;
                    EditorGUILayout.LabelField("This Mesh Have a LOD Group With " + script.responsibleUlod.levelsOfDetailToGenerate + " Meshes", titulo);

                    //Meshes
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("ULOD Meshes of This Mesh", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    //Prepare string sufix for each mesh
                    string lod0Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 0 ? " [Rendering]" : "");
                    string lod1Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 1 ? " [Rendering]" : "");
                    string lod2Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 2 ? " [Rendering]" : "");
                    string lod3Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 3 ? " [Rendering]" : "");
                    string lod4Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 4 ? " [Rendering]" : "");
                    string lod5Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 5 ? " [Rendering]" : "");
                    string lod6Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 6 ? " [Rendering]" : "");
                    string lod7Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 7 ? " [Rendering]" : "");
                    string lod8Suffix = (script.responsibleUlod.GetCurrentLodLevel() == 8 ? " [Rendering]" : "");
                    if (script.responsibleUlod.isScannedMeshesCurrentCulled() == true)
                    {
                        lod0Suffix = "";
                        lod1Suffix = "";
                        lod2Suffix = "";
                        lod3Suffix = "";
                        lod4Suffix = "";
                        lod5Suffix = "";
                        lod6Suffix = "";
                        lod7Suffix = "";
                        lod8Suffix = "";
                    }

                    //Get id of this originalMeshItem
                    int id = script.idOfOriginalMeshItemOfThisInResponsibleUlod;

                    //Show meshes
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].originalMesh = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Original Mesh  " + lod0Suffix,
                            "Mesh file that will be rendered in Original Mesh."),
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].originalMesh, typeof(Mesh), true, GUILayout.Height(16));

                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod1 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_1    " + lod1Suffix,
                            "Mesh file that will be rendered in LOD_1."),
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod1, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 2)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_2    " + lod2Suffix,
                                    "Mesh file that will be rendered in LOD_2."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 3)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_3    " + lod3Suffix,
                                    "Mesh file that will be rendered in LOD_3."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 4)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_4    " + lod4Suffix,
                                    "Mesh file that will be rendered in LOD_4."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 5)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_5    " + lod5Suffix,
                                    "Mesh file that will be rendered in LOD_5."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 6)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_6    " + lod6Suffix,
                                    "Mesh file that will be rendered in LOD_6."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 7)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_7    " + lod7Suffix,
                                    "Mesh file that will be rendered in LOD_7."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7, typeof(Mesh), true, GUILayout.Height(16));

                    if (script.responsibleUlod.levelsOfDetailToGenerate >= 8)
                        if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8Path != "")
                            script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8 = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh LOD_8    " + lod8Suffix,
                                    "Mesh file that will be rendered in LOD_8."),
                                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8, typeof(Mesh), true, GUILayout.Height(16));

                    GUILayout.Space(20);

                    //Run auto force update on change a mesh
                    AutoForceUpdateOfLodsOnChangeMesh(script);

                    //Go to responsible ULOD
                    if (GUILayout.Button("Go To Responsible ULOD Component"))
                        Selection.objects = new Object[] { script.responsibleUlod.gameObject };

                    //Force render update
                    if (GUILayout.Button("Force ULOD To Update LODs Renderization"))
                        script.responsibleUlod.ForceThisComponentToUpdateLodsRender();

                    //Restore all default meshes, with path saved
                    if (GUILayout.Button("Restore All Original Generated Meshes"))
                    {
                        bool problems = TryToRestoreAllOriginalGeneratedLodLevels(script, id);
                        if (problems == false)
                            Debug.Log("All standard LOD meshes, which were generated by ULOD, have been restored in this mesh.");
                        if (problems == true)
                            EditorUtility.DisplayDialog("Error", "It was not possible to load 1 or more LOD mesh files generated by ULOD, 1 or more mesh files missing in your project.", "Ok");
                    }
                }

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

            public void AutoForceUpdateOfLodsOnChangeMesh(UltimateLevelOfDetailMeshes script)
            {
                bool force = false;

                //LOD Original
                if (script.lastLod0 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].originalMesh)
                {
                    force = true;
                    script.lastLod0 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].originalMesh;
                }
                //LOD 1
                if (script.lastLod1 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod1)
                {
                    force = true;
                    script.lastLod1 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod1;
                }
                //LOD 2
                if (script.lastLod2 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod2)
                {
                    force = true;
                    script.lastLod2 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod2;
                }
                //LOD 3
                if (script.lastLod3 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod3)
                {
                    force = true;
                    script.lastLod3 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod3;
                }
                //LOD 4
                if (script.lastLod4 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod4)
                {
                    force = true;
                    script.lastLod4 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod4;
                }
                //LOD 5
                if (script.lastLod5 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod5)
                {
                    force = true;
                    script.lastLod5 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod5;
                }
                //LOD 6
                if (script.lastLod6 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod6)
                {
                    force = true;
                    script.lastLod6 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod6;
                }
                //LOD 7
                if (script.lastLod7 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod7)
                {
                    force = true;
                    script.lastLod7 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod7;
                }
                //LOD 8
                if (script.lastLod8 != script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod8)
                {
                    force = true;
                    script.lastLod8 = script.responsibleUlod.originalMeshesFoundInLastScan[script.idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod8;
                }

                //Force if have changes
                if (force == true)
                {
                    script.responsibleUlod.ForceThisComponentToUpdateLodsRender();

                    //Notify the user if option "Editor Skinned LODs Change" is off
                    SkinnedMeshRenderer smr = script.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null && script.responsibleUlod.forceChangeLodsOfSkinnedInEditor == false)
                        Debug.LogWarning("You just made a Skinned mesh change to an \"Ultimate Level Of Detail Meshes\" component. The mesh change will not be displayed because the \"Editor Skinned LODs Change\" option is disabled, in the parent \"Ultimate Level Of Detail\" component, which prevents the LOD simulations of the Ultimate LOD System from occurring in the Editor, however, the LOD changes will occur in the final game, compiled and built, without any problems. If you want to see these simulations in the Editor too, just activate the \"Editor Skinned LODs Change\" option in the \"Ultimate Level Of Detail\" component. Consult the documentation for more details.");
                }
            }

            public bool TryToRestoreAllOriginalGeneratedLodLevels(UltimateLevelOfDetailMeshes script, int id)
            {
                //Try to restore all original meshes by the path, and return if have problems
                bool problems = false;

                script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod1 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod1Path, typeof(Mesh));
                if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod1 == null)
                    problems = true;
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 2)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod2 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 3)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod3 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 4)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod4 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 5)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod5 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 6)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod6 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 7)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod7 == null)
                        problems = true;
                }
                if (script.responsibleUlod.levelsOfDetailToGenerate >= 8)
                {
                    script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8 = (Mesh)AssetDatabase.LoadAssetAtPath(script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8Path, typeof(Mesh));
                    if (script.responsibleUlod.originalMeshesFoundInLastScan[id].meshLod8 == null)
                        problems = true;
                }

                return problems;
            }
        }
        #endregion
#endif

        //API Methods

        public UltimateLevelOfDetail GetResponsibleUlodComponent()
        {
            //Return the responsible ulod component
            return responsibleUlod;
        }

        public int GetQuantityOfLods()
        {
            //Return the quantity of LODs that this LOD group have
            return responsibleUlod.levelsOfDetailToGenerate;
        }

        public void SetMeshOfThisLodGroup(int level, Mesh newMesh)
        {
            //Check if is a valid level
            if (level < 0 || level > 8)
            {
                Debug.LogError("It was not possible to define a new mesh in this LOD group, the level informed is invalid.");
                return;
            }

            //Set a new mesh for a level of this LOD group
            if (level == 0)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].originalMesh = newMesh;
            if (level == 1)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod1 = newMesh;
            if (level == 2)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod2 = newMesh;
            if (level == 3)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod3 = newMesh;
            if (level == 4)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod4 = newMesh;
            if (level == 5)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod5 = newMesh;
            if (level == 6)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod6 = newMesh;
            if (level == 7)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod7 = newMesh;
            if (level == 8)
                responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod8 = newMesh;

            //Update the renderization
            responsibleUlod.ForceThisComponentToUpdateLodsRender();
        }

        public Mesh GetMeshOfThisLodGroup(int level)
        {
            //Check if is a valid level
            if (level < 0 || level > 8)
            {
                Debug.LogError("It was not possible to get mesh of desired level, the level informed is invalid.");
                return null;
            }

            //Set a new mesh for a level of this LOD group
            if (level == 0)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].originalMesh;
            if (level == 1)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod1;
            if (level == 2)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod2;
            if (level == 3)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod3;
            if (level == 4)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod4;
            if (level == 5)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod5;
            if (level == 6)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod6;
            if (level == 7)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod7;
            if (level == 8)
                return responsibleUlod.originalMeshesFoundInLastScan[idOfOriginalMeshItemOfThisInResponsibleUlod].meshLod8;

            //Return empty
            return null;
        }
    }
}