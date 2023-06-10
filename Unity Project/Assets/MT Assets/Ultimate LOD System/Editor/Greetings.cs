using UnityEditor;
using System.IO;

namespace MTAssets.UltimateLODSystem.Editor
{
    /*
     * This class is responsible for displaying the welcome message when installing this asset.
     */

    [InitializeOnLoad]
    class Greetings
    {
        static Greetings()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += Run;
        }

        static void Run()
        {
            string pathOfThisAsset = "Assets/MT Assets/Ultimate LOD System/";
            string pathToGreetingsAsset = "Assets/MT Assets/_AssetsData/Greetings/GreetingsData.Uls.ini";

            CreateBaseDirectoriesIfNotExists();

            //If the greetings file not exists
            if (AssetDatabase.LoadAssetAtPath(pathToGreetingsAsset, typeof(object)) == null)
            {
                //Create a new greetings file
                File.WriteAllText(pathToGreetingsAsset, "Ok");

                //Show greetings and save
                EditorUtility.DisplayDialog("Ultimate LOD System was imported!",
                    "The \"Ultimate LOD System\" was imported for your project. You should be able to locate it in the directory\n" +
                    "(" + pathOfThisAsset + ").\n\n" +
                    "Remember to read the documentation to learn how to use this asset. To read the documentation, extract the contents of \"Documentation.zip\" inside the\n" +
                    "(" + pathOfThisAsset + ") folder. Then just open the \"Documentation.html\" in your favorite browser.\n\n" +
                    "If you need help, contact me by email (mtassets@windsoft.xyz).",
                    "Cool!");

                AssetDatabase.Refresh();
            }
        }

        public static void CreateBaseDirectoriesIfNotExists()
        {
            //Create the directory to feedbacks folder, of this asset
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
            {
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            }
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
            {
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            }
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Greetings"))
            {
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Greetings");
            }
        }
    }
}