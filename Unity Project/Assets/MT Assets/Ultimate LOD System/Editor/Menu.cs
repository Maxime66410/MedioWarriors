using UnityEngine;
using UnityEditor;
using System.IO;

namespace MTAssets.UltimateLODSystem.Editor
{

    /*
     * This class is responsible for creating the menu for this asset. 
     */

    public class Menu : MonoBehaviour
    {
        //Right click menu items

        [MenuItem("Assets/Open With Mesh Simplifier", false, 30)]
        static void OpenMeshSimplifierToolWithHierarchyNormal()
        {
            UltimateMeshSimplifier.OpenWindow(null, Selection.activeObject as Mesh);
        }

        [MenuItem("Assets/Open With Mesh Simplifier", true)]
        static bool OpenMeshSimplifierToolWithHierarchyValidation()
        {
            //Validate if selected item is a mesh
            if (Selection.objects.Length == 1)
                return Selection.activeObject is Mesh;
            if (Selection.objects.Length > 1)
                return false;
            return false;
        }

        [MenuItem("GameObject/Simplify Mesh", false, 30)]
        static void GoSimplifierToolWithHierarchyNormal()
        {
            UltimateMeshSimplifier.OpenWindow(Selection.activeObject as GameObject, null);
        }

        //Menu items

        [MenuItem("Tools/MT Assets/Ultimate LOD System/Mesh Simplifier Tool", false, 10)]
        static void OpenMeshSimplifierTool()
        {
            UltimateMeshSimplifier.OpenWindow(null, null);
        }

        [MenuItem("Tools/MT Assets/Ultimate LOD System/Changelog", false, 10)]
        static void OpenChangeLog()
        {
            string filePath = "Assets/MT Assets/Ultimate LOD System/List Of Changes.txt";

            if (File.Exists(filePath) == true)
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset)));
            }
            if (File.Exists(filePath) == false)
            {
                EditorUtility.DisplayDialog("Error", "Unable to open file. The file has been deleted, or moved. Please, to correct this problem and avoid future problems with this tool, remove all files from this asset and install it again.", "Ok");
            }
        }

        [MenuItem("Tools/MT Assets/Ultimate LOD System/Read Documentation", false, 30)]
        static void ReadDocumentation()
        {
            EditorUtility.DisplayDialog("Read Documentation", "The documentation is located inside the \n\"MT Assets/Ultimate LOD System\" folder. Just unzip \"Documentation.zip\" on your desktop and open the \"Documentation.html\" file with your preferred browser.", "Cool!");
        }

        [MenuItem("Tools/MT Assets/Ultimate LOD System/More Assets", false, 30)]
        static void MoreAssets()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/40306");
        }

        [MenuItem("Tools/MT Assets/Ultimate LOD System/Support", false, 30)]
        static void GetSupport()
        {
            EditorUtility.DisplayDialog("Support", "If you have any questions, problems or want to contact me, just contact me by email (mtassets@windsoft.xyz).", "Got it!");
        }
    }
}