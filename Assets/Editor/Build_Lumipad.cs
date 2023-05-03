using UnityEditor;
using UnityEngine;
using DoubTech.Builds;

public class Build_Lumipad
{
    [MenuItem("Build/Lumipad/Build", false, 1)]
    public static void Build()
    {
        CustomBuilder.BuildGUID("b3543311f4d802b4e8b00b751d956daf");
    }
    
    [MenuItem("Build/Lumipad/Build and Run", false, 1)]
    public static void BuildAndRun()
    {
        CustomBuilder.BuildAndRunGUID("b3543311f4d802b4e8b00b751d956daf");
    }
    
    [MenuItem("Build/Lumipad/Run", false, 2)]
    public static void Run()
    {
        CustomBuilder.RunGUID("b3543311f4d802b4e8b00b751d956daf");
    }
}
