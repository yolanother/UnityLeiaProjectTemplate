/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LeiaLoft
{
    [UnityEditor.CustomEditor(typeof(LeiaDisplay))]
    public class LeiaDisplayEditor : UnityEditor.Editor
    {
        private const string RenderModeLabel = "Render Mode (Legacy)";
        private const string RenderTechniqueLabel = "Render Technique";
        private const string LightfieldModeLabel = "Lightfield Mode";
        private const string EnabledParallaxWarning = "Parallax Auto Rotation checkbox will be ignored if AutoRotation is enabled in PlayerSettings";
        private const string ParallaxFieldLabel = "(Deprecated) Parallax Auto Rotation";
        private const string MultiCameraCompositLabel = "Multi Camera Compositing";
        private const string EnableInterlacingInEditorLabel = "Interlacing In Editor";
        private const string MultiCameraCompositTooltip = "Multiple cameras with different depths can be blended together. Set one camera to have a clear flag Solid Color with low alpha, and the other camera will render its content over that weak alpha background";
        private const string InterlaceInEditorTooltip = "Visualize depth in editor by enabling interlacing";

        private LeiaDisplay _controller;

        void OnEnable()
        {
            if (_controller == null)
            {
                _controller = (LeiaDisplay)target;
            }
        }

        private void ShowLightfieldModeControl()
        {
            int previousIndex = _controller.DesiredLightfieldValue;
            UndoableInputFieldUtils.PopupLabeledTooltip(index =>
            {
                _controller.DesiredLightfieldMode = (LeiaDisplay.LightfieldMode)index;
            }
            , LightfieldModeLabel, previousIndex, Enum.GetNames(typeof(LeiaDisplay.LightfieldMode)), "Lightfield Mode", _controller);

        }

        private void ShowRenderTechniqueControl()
        {
            if (_controller.DesiredLightfieldMode != LeiaDisplay.LightfieldMode.On)
            {
                return;
            }

            int previousIndex = (int)_controller.DesiredRenderTechnique;

            UndoableInputFieldUtils.PopupLabeledTooltip(index =>
            {
                _controller.DesiredRenderTechnique = (LeiaDisplay.RenderTechnique)index;
            }, RenderTechniqueLabel, previousIndex, Enum.GetNames(typeof(LeiaDisplay.RenderTechnique)), "Render Technique", _controller.Settings);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void ShowRenderModeControl()
        {


            List<string> leiaModes = _controller.GetDisplayConfig().RenderModes;

            var list = leiaModes.ToList().Beautify();
            var previousIndex = list.IndexOf(_controller.DesiredLeiaStateID, ignoreCase: true);

            if (previousIndex < 0)
            {
                LogUtil.Log(LogLevel.Error, "Did not recognize renderMode {0}", _controller.DesiredLeiaStateID);
                list.Add(_controller.DesiredLeiaStateID);
                previousIndex = list.Count - 1;
            }

            EditorGUI.BeginDisabledGroup(true);
            UndoableInputFieldUtils.PopupLabeledTooltip(index =>
            {
                if (list[index] == LeiaDisplay.TWO_D)
                {
                    _controller.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
                }
                else if (list[index] == LeiaDisplay.THREE_D || list[index] == LeiaDisplay.HPO)
                {
                    _controller.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
                }
                else
                {
                    LogUtil.Log(LogLevel.Error, "Could not match RenderMode {0} at index {1} to LightfieldMode", list[index], index);
                }
            }
, RenderModeLabel, previousIndex, list, "Render mode", _controller.Settings);

            EditorGUI.EndDisabledGroup();

            if (_controller.DesiredLeiaStateID == LeiaDisplay.TWO_D && _controller.IsLightfieldModeDesiredOn() ||
                _controller.DesiredLeiaStateID == LeiaDisplay.HPO && !_controller.IsLightfieldModeDesiredOn())
            {
                Debug.LogErrorFormat("On GameObject {0}: state mismatch between legacy RenderMode DesiredLeiaStateID {1} and LightfieldMode {2}\n" +
                    "Please update the {0}'s LightfieldMode",
                    _controller.gameObject.name, _controller.DesiredLeiaStateID, _controller.DesiredLightfieldMode);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        private void ShowDecoratorsControls()
        {
            var decorators = _controller.Decorators;
            
            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            UndoableInputFieldUtils.BoolFieldWithTooltip(() => decorators.MultiCameraComposit, v => decorators.MultiCameraComposit = v, MultiCameraCompositLabel,
            MultiCameraCompositTooltip,
            _controller.Settings);

            if (Application.isPlaying)
            {
                EditorGUI.EndDisabledGroup();
            }

            UndoableInputFieldUtils.BoolFieldWithTooltip(() => _controller.InterlaceInEditor, v => _controller.InterlaceInEditor = v, EnableInterlacingInEditorLabel,
                        InterlaceInEditorTooltip,
                        _controller.Settings);

            _controller.Decorators = decorators;
        }

        /// <summary>
        /// User needs to be able to set DisplayConfig once and have setting persist through play/edit process.
        ///
        /// Dev needs to be able to retrieve DisplayConfig data without chaining through LeiaDisplay -> DeviceFactory -> OfflineEmulationLeiaDevice.
        /// These objects may be reconstructed and drop pointers on play, or some code which we want to be editor-only would have to be included in builds.
        /// </summary>
        void ShowDisplayConfigDropdown()
        {
            // build a path to subfolder where display config files are found
            string searchPath = Application.dataPath;
            foreach (string subfolder in new[] { "LeiaLoft", "Resources" })
            {
                searchPath = System.IO.Path.Combine(searchPath, subfolder);
            }

            string fileSearchString = "DisplayConfiguration_";
            string fileTerminalString = ".json";
            // convert file paths into short names which can be displayed to user
            string[] displayConfigPathMatches = System.IO.Directory.GetFiles(searchPath, fileSearchString + "*.json");
            List<string> displayConfigFilenames = new List<string>();
            for (int i = 0; i < displayConfigPathMatches.Length; i++)
            {
                displayConfigFilenames.Add(System.IO.Path.GetFileName(displayConfigPathMatches[i]));
            }

            // write user-selection into editor prefs
            int ind = Mathf.Max(0, displayConfigFilenames.IndexOf(OfflineEmulationLeiaDevice.EmulatedDisplayConfigFilename));

            if (ind >= displayConfigFilenames.Count)
            {
                LogUtil.Log(LogLevel.Error, "No DisplayConfiguration files found in Assets/LeiaLoft/Resources! Please reinstall your LeiaLoft Unity SDK");
                return;
            }

            string[] trimmedDisplayConfigFilenameArray = displayConfigFilenames.Select(x => x.Replace(fileSearchString, "").Replace(fileTerminalString, "")).ToArray();

            // suppress DisplayConfig dropdown selection when build player window is open. This avoids a bug where selecting a new build target,
            // not switching platform, and then changing emulated device profile would cause Unity to throw a GUI error

            
        }

        /// <summary>
        /// Searches through all UnityEditor objects for an EditorWindow
        /// </summary>
        /// <typeparam name="WindowType">A specific type of EditorWindow to search for</typeparam>
        /// <returns>True if any window of this type is open</returns>
        private static bool IsWindowOpen<WindowType>() where WindowType : EditorWindow
        {
            WindowType[] openWindows = Resources.FindObjectsOfTypeAll<WindowType>();
            return openWindows != null && openWindows.Length > 0;
        }

        public override void OnInspectorGUI()
        {

            if (!_controller.enabled)
            {
                return;
            }

            ShowDisplayConfigDropdown();
            ShowLightfieldModeControl();
            
            ShowDecoratorsControls();
        }
    }
}