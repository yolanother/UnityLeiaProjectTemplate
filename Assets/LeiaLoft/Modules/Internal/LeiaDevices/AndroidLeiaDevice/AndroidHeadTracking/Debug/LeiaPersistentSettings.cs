using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LeiaLoft.LeiaConfigUI
{
    public class LeiaPersistentSettings : MonoBehaviour
    {
        Dictionary<string, string> persistentSettings;

        [SerializeField] private LeiaRenderSettingsPanel leiaRenderSettingsPanel;
        [SerializeField] private EyeTrackingSettingsPanel eyeTrackingSettingsPanel;
        [SerializeField] private ACTPanel actPanel;
        [SerializeField] private DisplayConfigPanel displayConfigPanel;

        string settingsFilename;

        string letters
        {
            get
            {
                return "ABCDEFG";
            }
        }

        void Start()
        {
            settingsFilename = Application.persistentDataPath + "/" + "LeiaPersistentSettings.txt";
            persistentSettings = new Dictionary<string, string>();
            AddPersistentSetting("R0Test", "" + LeiaDisplay.Instance.ShowR0Test);
            AddPersistentSetting("BlackViewsOverride", "" + LeiaDisplay.Instance.blackViews);
            AddPersistentSetting("BlackViewsOverrideValue", "" + LeiaDisplay.Instance.blackViews);
            AddPersistentSetting("CloseRangeSafety", "" + LeiaDisplay.Instance.CloseRangeSafety);
            AddPersistentSetting("ZParallax", "" + LeiaCamera.Instance.cameraZaxisMovement);
            AddPersistentSetting("ShowTrackingStatusBar", "" + LeiaDisplay.Instance.eyeTrackingStatusBarEnabled);
#if !UNITY_ANDROID
        AddPersistentSetting("Delay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
        AddPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);

#elif !UNITY_EDITOR
        AddPersistentSetting("Delay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
        AddPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
        
#endif
            AddPersistentSetting("BaselineScaling", "" + LeiaCamera.Instance.BaselineScaling);
            AddPersistentSetting("CameraShiftScaling", "" + LeiaCamera.Instance.CameraShiftScaling);
            AddPersistentSetting("SWBrightness", "" + LeiaDisplay.Instance.SWBrightness);

            if (!File.Exists(settingsFilename))
            {
                SaveSettings();
            }
#if !UNITY_EDITOR
        if (File.Exists(settingsFilename))
        {
            LoadSettings();
        }
#endif
        }

        void GetPersistentSettingsFromUI()
        {
            if (persistentSettings == null)
            {
                Debug.Log("persistentSettings is null.");
                return;
            }
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            SetPersistentSetting("R0Test", "" + LeiaDisplay.Instance.ShowR0Test);
            SetPersistentSetting("BlackViewsOverride", "" + LeiaDisplay.Instance.BlackViewsOverride);
            SetPersistentSetting("BlackViewsOverrideValue", "" + LeiaDisplay.Instance.BlackViewsOverrideValue);
            SetPersistentSetting("CloseRangeSafety", "" + LeiaDisplay.Instance.CloseRangeSafety);
            SetPersistentSetting("ZParallax", "" + LeiaCamera.Instance.cameraZaxisMovement);
            SetPersistentSetting("ShowTrackingStatusBar", "" + LeiaDisplay.Instance.eyeTrackingStatusBarEnabled);
#if !UNITY_ANDROID
        SetPersistentSetting("Delay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
#elif UNITY_ANDROID && !UNITY_EDITOR
        SetPersistentSetting("Delay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
#endif
            if (!persistentSettings.ContainsKey("OriginalTimeDelay"))
            {
#if !UNITY_ANDROID
            SetPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
#elif UNITY_ANDROID && !UNITY_EDITOR
            SetPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
#endif
            }
            SetPersistentSetting("BaselineScaling", "" + LeiaCamera.Instance.BaselineScaling);
            SetPersistentSetting("CameraShiftScaling", "" + LeiaCamera.Instance.CameraShiftScaling);
            SetPersistentSetting("SWBrightness", "" + LeiaDisplay.Instance.SWBrightness);
        }

        void SetPersistentSetting(string key, string value)
        {
            if (persistentSettings == null)
            {
                Debug.Log("persistentSettings is null.");
                return;
            }
            persistentSettings[key] = value;
        }

        void AddPersistentSetting(string key, string value)
        {
            persistentSettings.Add(key, value);
        }

        public void SaveSettings()
        {
            if (persistentSettings == null)
            {
                return;
            }
#if !UNITY_EDITOR
        GetPersistentSettingsFromUI();
        string savestr = "";

        foreach (KeyValuePair<string, string> entry in persistentSettings)
        {
            if (!persistentSettings.ContainsKey(entry.Key))
            {
                Debug.LogError("Persistent settings list does not contain a key titled \"" + entry.Key + "\"");
            }
            else
            {
                savestr += entry.Key + ", " + entry.Value + "\n";
            }
        }

        File.WriteAllText(settingsFilename, savestr);
#endif
        }

        void LoadSettings()
        {
            if (!File.Exists(settingsFilename))
            {
                return;
            }
            string loadStr = File.ReadAllText(settingsFilename);
            StringReader stringReader = new StringReader(loadStr);

            while (true)
            {
                string line = stringReader.ReadLine();
                if (line == null)
                {
                    break;
                }
                int commaPos = line.IndexOf(',');
                string key = line.Substring(0, commaPos);
                string value = line.Substring(commaPos + 2);
                persistentSettings[key] = value;
            }

            SetSettingsFromDictionary();
        }

        void SetSettingsFromDictionary()
        {
            LeiaDisplay.Instance.ShowR0Test = bool.Parse(persistentSettings["R0Test"]);
            LeiaDisplay.Instance.BlackViewsOverride = bool.Parse(persistentSettings["BlackViewsOverride"]);
            LeiaDisplay.Instance.BlackViewsOverrideValue = bool.Parse(persistentSettings["BlackViewsOverrideValue"]);
            LeiaDisplay.Instance.CloseRangeSafety = bool.Parse(persistentSettings["CloseRangeSafety"]);
            LeiaCamera.Instance.cameraZaxisMovement = bool.Parse(persistentSettings["ZParallax"]);
            LeiaDisplay.Instance.eyeTrackingStatusBarEnabled = bool.Parse(persistentSettings["ShowTrackingStatusBar"]);
            LeiaDisplay.Instance.OriginalTimeDelay = float.Parse(persistentSettings["OriginalTimeDelay"]);
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            config.timeDelay = float.Parse(persistentSettings["Delay"]);
            LeiaCamera.Instance.BaselineScaling = float.Parse(persistentSettings["BaselineScaling"]);
            LeiaCamera.Instance.CameraShiftScaling = float.Parse(persistentSettings["CameraShiftScaling"]);
            LeiaDisplay.Instance.SWBrightness = float.Parse(persistentSettings["SWBrightness"]);

#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = float.Parse(persistentSettings["Delay"]);
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif

            leiaRenderSettingsPanel.UpdateUI();
            eyeTrackingSettingsPanel.UpdateUI();
            actPanel.UpdateUI();
            displayConfigPanel.UpdateUI();
        }
    }
}
