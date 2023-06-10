﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace MTAssets.UltimateLODSystem.Editor
{
    public class UltimateMeshSimplifier : EditorWindow
    {
        /*
        This class is responsible for the functioning of the "Ultimate Mesh Simplifier" component, and all its functions.
        */
        /*
         * The Ultimate Level Of Detail was developed by Marcos Tomaz in 2020.
         * Need help? Contact me (mtassets@windsoft.xyz)
         */

        //Variables of window
        private bool isWindowOnFocus = false;

        //Classes of script
        public enum MeshReadingMethod
        {
            FromGameObject,
            FromModelOfAssetsFile
        }
        public enum ForceOfSimplification
        {
            Normal,
            Strong,
            VeryStrong,
            ExtremelyStrong,
            Destroyer
        }

        //Variables of script
        private static UMeshSimplifierPreferences meshSimplifierPreferences;
        private bool preferencesLoadedOnInspectorUpdate = false;
        private static GameObject currentOpenedGameObjectWithMesh = null;
        private static Mesh currentOpenedMesh = null;
        private static string pathToCurrentOpenedMesh = "";
        private static string pathToCurrentOpenedMeshDirectory = "";
        private static string pathToCurrentOpenedMeshFileName = "";
        private static bool currentOpenedMeshIsSkinned = false;
        private static MeshReadingMethod meshReadingMethod = MeshReadingMethod.FromGameObject;
        private GameObject lastGameObjectThatHaveMeshExtracted = null;
        private int simplifiedMeshesLevelsToGenerate = 1;
        private float[] simplificationPercentInEachMeshLevel = new float[8];
        private bool skinnedAnimsCompatMode = true;
        private bool preventArtifactsOrDeform = true;
        private ForceOfSimplification forceOfSimplification = ForceOfSimplification.Normal;

        //Variables of UI
        private Vector2 scrollPosPreferences;

        public static void OpenWindow(GameObject gameObjectWithMeshToOpen, Mesh meshToOpen)
        {
            //Method to open the Window
            var window = GetWindow<UltimateMeshSimplifier>("Mesh Simplifier");
            window.minSize = new Vector2(400, 650);
            window.maxSize = new Vector2(400, 650);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();

            //Get mesh to process
            currentOpenedGameObjectWithMesh = gameObjectWithMeshToOpen;
            currentOpenedMesh = meshToOpen;

            //Setup the mesh reading method
            if (meshToOpen == null && gameObjectWithMeshToOpen == null)
                meshReadingMethod = MeshReadingMethod.FromModelOfAssetsFile;
            if (meshToOpen != null && gameObjectWithMeshToOpen == null)
                meshReadingMethod = MeshReadingMethod.FromModelOfAssetsFile;
            if (meshToOpen == null && gameObjectWithMeshToOpen != null)
                meshReadingMethod = MeshReadingMethod.FromGameObject;
        }

        //UI Code
        #region INTERFACE_CODE
        void OnEnable()
        {
            //On enable this window, on re-start this window after compilation
            isWindowOnFocus = true;

            //Load the preferences
            LoadThePreferences(this);
        }

        void OnDisable()
        {
            //On disable this window, after compilation, disables the window and enable again
            isWindowOnFocus = false;

            //Save the preferences
            SaveThePreferences(this);
        }

        void OnDestroy()
        {
            //On close this window
            isWindowOnFocus = false;

            //Save the preferences
            SaveThePreferences(this);
        }

        void OnFocus()
        {
            //On focus this window
            isWindowOnFocus = true;
        }

        void OnLostFocus()
        {
            //On lose focus in window
            isWindowOnFocus = false;
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Start the UI

            //Format the title label
            GUIStyle tituloBox = new GUIStyle();
            tituloBox.fontStyle = FontStyle.Bold;
            tituloBox.alignment = TextAnchor.MiddleCenter;

            //Run mesh extractor and mesh simplification percentage resetter
            RunMeshExtractorFromGameObject();
            RunSimplificationInEachMeshLevelResetter();

            scrollPosPreferences = EditorGUILayout.BeginScrollView(scrollPosPreferences, GUILayout.Width(400), GUILayout.Height(616));
            //The title
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Ultimate Mesh Simplifier", tituloBox);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Here you can generate simplified versions (with less vertices) of a mesh that you have in your project. You can provide a mesh directly from a 3D model file that is in your project assets files, or provide a GameObject that contains a mesh. Ultimate Mesh Simplifier will extract the mesh and generate simplified versions based on the parameters you choose.", MessageType.Info);
            GUILayout.Space(10);

            //Mesh reading settings
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Meshes Reading Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            meshReadingMethod = (MeshReadingMethod)EditorGUILayout.EnumPopup(new GUIContent("Mesh Reading Method",
                                            "Choose the method that Mesh Simplifier will read the source mesh.\n\n" +
                                            "FromGameObject - The mesh will be read from a GameObject that contains a renderer.\n\n" +
                                            "FromModelOfAssetsFile - The mesh will be read from a 3D model file present in your project files."),
                                            meshReadingMethod);
            EditorGUI.indentLevel += 1;
            if (meshReadingMethod == MeshReadingMethod.FromGameObject)
                currentOpenedGameObjectWithMesh = (GameObject)EditorGUILayout.ObjectField(new GUIContent("GameObject With Mesh", "The GameObject that contains a mesh to be read."), currentOpenedGameObjectWithMesh, typeof(GameObject), true, GUILayout.Height(16));
            if (meshReadingMethod == MeshReadingMethod.FromModelOfAssetsFile)
                currentOpenedMesh = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh To Be Read", "The mesh to be read."), currentOpenedMesh, typeof(Mesh), true, GUILayout.Height(16));
            EditorGUI.indentLevel -= 1;

            //Mesh reading settings
            GUILayout.Space(10);
            if (currentOpenedMesh == null)
                EditorGUILayout.LabelField("No Mesh Selected To Show Info", EditorStyles.boldLabel);
            if (currentOpenedMesh != null)
                EditorGUILayout.LabelField("Selected Mesh Information", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (currentOpenedMesh == null)
            {
                //Format the title label
                if (meshReadingMethod == MeshReadingMethod.FromModelOfAssetsFile)
                    EditorGUILayout.HelpBox("No mesh selected.", MessageType.None);
                if (meshReadingMethod == MeshReadingMethod.FromGameObject)
                    EditorGUILayout.HelpBox("No mesh selected or no meshed GameObject was provided.", MessageType.None);

                //Reset path info of mesh
                pathToCurrentOpenedMesh = "";
                pathToCurrentOpenedMeshDirectory = "";
                pathToCurrentOpenedMeshFileName = "";
            }
            if (currentOpenedMesh != null)
            {
                //Get path to mesh asset
                pathToCurrentOpenedMesh = AssetDatabase.GetAssetPath(currentOpenedMesh);
                pathToCurrentOpenedMeshDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(currentOpenedMesh));
                pathToCurrentOpenedMeshFileName = Path.GetFileNameWithoutExtension(pathToCurrentOpenedMesh);
                if (pathToCurrentOpenedMesh == "Library/unity default resources")
                {
                    pathToCurrentOpenedMesh = "Primitive Mesh";
                    pathToCurrentOpenedMeshDirectory = "Primitive Mesh";
                    pathToCurrentOpenedMeshFileName = "Primitive Mesh";
                }

                //Check if current opened mesh, is skinned
                if (currentOpenedMesh.boneWeights.Length == 0 && currentOpenedMesh.blendShapeCount == 0)
                    currentOpenedMeshIsSkinned = false;
                if (currentOpenedMesh.boneWeights.Length > 0 || currentOpenedMesh.blendShapeCount > 0)
                    currentOpenedMeshIsSkinned = true;

                //Format the title label
                EditorGUILayout.HelpBox("Mesh Name: " + currentOpenedMesh.name +
                                        "\nVertices: " + currentOpenedMesh.vertexCount +
                                        "\nTriangles: " + currentOpenedMesh.triangles.Length +
                                        "\nSubMeshes: " + currentOpenedMesh.subMeshCount +
                                        "\nIs Skinned: " + currentOpenedMeshIsSkinned +
                                        "\nPath To Mesh: " + pathToCurrentOpenedMesh
                                        , MessageType.None);
            }

            //Simplification settings
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Mesh Simplification Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            simplifiedMeshesLevelsToGenerate = EditorGUILayout.IntSlider(new GUIContent("Simplifications To Gener.",
                                           "The number of simplified meshes that you want to generate."),
                                           simplifiedMeshesLevelsToGenerate, 1, 8);
            EditorGUI.indentLevel += 1;
            for (int i = 0; i <= (simplifiedMeshesLevelsToGenerate - 1); i++)
                simplificationPercentInEachMeshLevel[i] = EditorGUILayout.Slider(new GUIContent("Mesh " + (i + 1) + " Vertex Percent",
                        "The percentage of vertices it will contain in simplified mesh " + (i + 1) + " of the meshes."),
                        simplificationPercentInEachMeshLevel[i], 1f, 100f);
            EditorGUI.indentLevel -= 1;

            skinnedAnimsCompatMode = (bool)EditorGUILayout.Toggle(new GUIContent("Skinned Compat Mode",
                                                "If this option is active, Mesh Simplifier will use internal algorithms to improve the accuracy and compatibility with Skinned Mesh Renderers animations. If you have problems or artifacts, try disabling this. This only applies to meshes that are in Skinned Mesh Renderers."),
                                                skinnedAnimsCompatMode);

            preventArtifactsOrDeform = (bool)EditorGUILayout.Toggle(new GUIContent("Prevent Artifact/Deform",
                                    "If this option is active, Mesh Simplifier will use internal algorithms to prevent artifacts in the generated Simplified meshes. If you still have problems with artifacts, try disabling this.\n\nNote that: Meshes that contain many vertices sharing the same space, may have problems or present artifacts."),
                                    preventArtifactsOrDeform);

            forceOfSimplification = (ForceOfSimplification)EditorGUILayout.EnumPopup(new GUIContent("Force Of Simplification",
                                   "Some meshes have such a large number of vertices, that even reducing their vertices to 1%, they will still have many vertices, or the algorithm can avoid a very large reduction of vertices, to maintain the original shape of the mesh. If you want to reduce even more the amount of vertices present in the meshes, increase the strength of the simplification. Very large forces in meshes with a moderate amount of vertices, you can completely deform them, so use this parameter while testing, and only use it if you really need a greater vertex reduction force.\n\nArtifact prevention is disabled if you use forces greater than the Normal force, as there is no way to prevent artifacts when greater aggressiveness is imposed in simplifying the mesh."),
                                   forceOfSimplification);
            if (forceOfSimplification != ForceOfSimplification.Normal)
                preventArtifactsOrDeform = false;

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The simplified meshes generated here will be saved in the same directory as the original mesh. They will be placed in a folder for greater organization. If simplifications already exist for this mesh, with the same name, they will be overwritten.", MessageType.Info);

            EditorGUILayout.EndScrollView();

            //The close button
            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", GUILayout.Height(25)))
            {
                //Save the preferences
                SaveThePreferences(this);

                //Close the window
                this.Close();
            }
            if (GUILayout.Button("Generate Simplified Meshes", GUILayout.Height(25)))
            {
                //If not have mesh opened, cancel
                if (currentOpenedMesh == null)
                {
                    EditorUtility.DisplayDialog("Error", "There is no mesh selected to generate simplified versions. Please provide a mesh to be simplified before.", "Ok");
                    return;
                }
                if (pathToCurrentOpenedMesh == "Primitive Mesh" || pathToCurrentOpenedMeshDirectory == "Primitive Mesh" || pathToCurrentOpenedMeshFileName == "Primitive Mesh")
                {
                    EditorUtility.DisplayDialog("Error", "It is not possible to simplify Unity's primitive meshes.", "Ok");
                    return;
                }

                //Run the mesh simplificator
                RunMeshSimplificator();
            }
            EditorGUILayout.EndHorizontal();

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        void OnInspectorUpdate()
        {
            //On inspector update, on lost focus in this Window
            if (isWindowOnFocus == false)
            {
                //Update this window
                Repaint();

                //Update the scene GUI
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
            }

            //Try to load the preferences on inspector update (if this window is in focus or not, try to load here, because this method runs after OpenWindow() method)
            if (preferencesLoadedOnInspectorUpdate == false)
            {
                if (meshSimplifierPreferences.windowPosition.x != 0 && meshSimplifierPreferences.windowPosition.y != 0)
                {
                    LoadThePreferences(this);
                }
                preferencesLoadedOnInspectorUpdate = true;
            }
        }

        void RunMeshExtractorFromGameObject()
        {
            //This method extract a mesh from a GameObject if is not extracted yet
            if (meshReadingMethod == MeshReadingMethod.FromGameObject)
            {
                if (currentOpenedGameObjectWithMesh != lastGameObjectThatHaveMeshExtracted)
                {
                    //Try to extract from skinned
                    SkinnedMeshRenderer meshRenderer = currentOpenedGameObjectWithMesh.GetComponent<SkinnedMeshRenderer>();
                    if (meshRenderer != null)
                        currentOpenedMesh = meshRenderer.sharedMesh;
                    //Try to extract from filter
                    MeshFilter meshFilter = currentOpenedGameObjectWithMesh.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                        currentOpenedMesh = meshFilter.sharedMesh;
                }

                //If GameObject is null, null the mesh too
                if (currentOpenedGameObjectWithMesh == null)
                    currentOpenedMesh = null;
            }
            if (meshReadingMethod == MeshReadingMethod.FromModelOfAssetsFile)
                currentOpenedGameObjectWithMesh = null;
        }

        void RunSimplificationInEachMeshLevelResetter()
        {
            //Reset the simplification percentage in each level, if all is zero
            bool allIsZero = true;
            for (int i = 0; i < simplificationPercentInEachMeshLevel.Length; i++)
                if (simplificationPercentInEachMeshLevel[i] > 0)
                    allIsZero = false;
            if (allIsZero == true)
            {
                for (int i = 0; i < simplificationPercentInEachMeshLevel.Length; i++)
                    simplificationPercentInEachMeshLevel[i] = 85.0f;
            }
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
            if (preventArtifactsOrDeform == true)
                meshSimplificationSettings.EnableSmartLink = true;
            if (preventArtifactsOrDeform == false)
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
            if (isSkinnedMesh == true && skinnedAnimsCompatMode == true)
                resultMesh.bindposes = originalMesh.bindposes;
            return resultMesh;
        }

        void RunMeshSimplificator()
        {
            //Generate simplified versions of a mesh, according the quantity desired

            //Create the array of simplified mesh versins
            Mesh[] simplifiedMeshes = new Mesh[8];

            //Start the mesh simplification generation proccess
            for (int i = 0; i <= (simplifiedMeshesLevelsToGenerate - 1); i++)
            {
                EditorUtility.DisplayProgressBar("A moment...", "Simplifying mesh " + (i) + "/" + simplifiedMeshesLevelsToGenerate, (float)i / (float)(simplifiedMeshesLevelsToGenerate));
                simplifiedMeshes[i + 1] = GetGeneratedLodForThisMesh(currentOpenedMesh, simplificationPercentInEachMeshLevel[i], currentOpenedMeshIsSkinned);
            }

            //Create the folder on directory of original mesh, to save simplifications
            if (AssetDatabase.IsValidFolder(pathToCurrentOpenedMeshDirectory + "/" + pathToCurrentOpenedMeshFileName + " (Simplifications)") == false)
                AssetDatabase.CreateFolder(pathToCurrentOpenedMeshDirectory, pathToCurrentOpenedMeshFileName + " (Simplifications)");

            //Save the generated mesh
            for (int i = 0; i < simplifiedMeshes.Length; i++)
            {
                if (simplifiedMeshes[i] == null)
                    continue;

                AssetDatabase.CreateAsset(simplifiedMeshes[i], pathToCurrentOpenedMeshDirectory + "/" + pathToCurrentOpenedMeshFileName + " (Simplifications)" + "/" + currentOpenedMesh.name + " (Simplification " + i + ").asset");
            }

            //Clear progresss bar
            EditorUtility.ClearProgressBar();

            //Ping the folder
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(pathToCurrentOpenedMeshDirectory + "/" + pathToCurrentOpenedMeshFileName + " (Simplifications)", typeof(object)));

            //Show warning
            Debug.Log("The \"" + currentOpenedMesh.name + "\" mesh has been simplified. The simplified versions were saved in the original mesh directory. The directory is: " + pathToCurrentOpenedMeshDirectory);
        }
        #endregion

        static void LoadThePreferences(UltimateMeshSimplifier instance)
        {
            //Create the default directory, if not exists
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Preferences"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Preferences");

            //Try to load the preferences file
            meshSimplifierPreferences = (UMeshSimplifierPreferences)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Preferences/UMeshSimplifierPreferences.asset", typeof(UMeshSimplifierPreferences));
            //Validate the preference file. if this preference file is of another project, delete then
            if (meshSimplifierPreferences != null)
            {
                if (meshSimplifierPreferences.projectName != Application.productName)
                {
                    AssetDatabase.DeleteAsset("Assets/MT Assets/_AssetsData/Preferences/UMeshSimplifierPreferences.asset");
                    meshSimplifierPreferences = null;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if (meshSimplifierPreferences != null && meshSimplifierPreferences.projectName == Application.productName)
                {
                    //Set the position of Window 
                    instance.position = meshSimplifierPreferences.windowPosition;
                }
            }
            //If null, create and save a preferences file
            if (meshSimplifierPreferences == null)
            {
                meshSimplifierPreferences = ScriptableObject.CreateInstance<UMeshSimplifierPreferences>();
                meshSimplifierPreferences.projectName = Application.productName;
                AssetDatabase.CreateAsset(meshSimplifierPreferences, "Assets/MT Assets/_AssetsData/Preferences/UMeshSimplifierPreferences.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SaveThePreferences(UltimateMeshSimplifier instance)
        {
            //Save the preferences in Prefs.asset
            meshSimplifierPreferences.projectName = Application.productName;
            meshSimplifierPreferences.windowPosition = new Rect(instance.position.x, instance.position.y, instance.position.width, instance.position.height);
            EditorUtility.SetDirty(meshSimplifierPreferences);
            AssetDatabase.SaveAssets();
        }
    }
}