using UnityEditor;
using UnityEngine.Rendering;
using System;

class Builder{
    static void buildWebGL()
    {

        // Place all your scenes here
        string[] scenes = {"Assets/Tetris Template/Scenes/GamePlay.unity"};

        string[] args = Environment.GetCommandLineArgs();
        string outputPath = "builds/raw/web"; // Set a default path if the argument is not provided

        // Check for command-line argument
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-outputPath" && i + 1 < args.Length)
            {
                outputPath = args[i + 1];
                break;
            }
        }

        BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.WebGL, BuildOptions.None);
    }
}