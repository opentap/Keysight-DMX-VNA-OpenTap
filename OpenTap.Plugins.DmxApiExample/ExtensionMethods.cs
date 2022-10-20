//Copyright 2022-2029 Keysight Technologies
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This class holds a number of utility methods used by various other classes in the plugin.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a complex voltage value to Log Magnitude.
        /// </summary>
        /// <param name="cNum">A System.Numeric.Complex number representing a vector voltage value</param>
        /// <returns></returns>
        public static double LogMag(this Complex cNum)
        {
            if (cNum.Magnitude > double.Epsilon)
                return 20 * Math.Log10(cNum.Magnitude);

            return -200.0;
        }

        /// <summary>
        /// Converts a complex voltage value to phase in degrees.
        /// </summary>
        /// <param name="cNum">A System.Numeric.Complex number representing a vector voltage value</param>
        /// <returns></returns>
        public static double DegPhase(this Complex cNum)
        {
            if (cNum.Magnitude > double.Epsilon)
                return cNum.Phase * 180 / Math.PI;

            return 0.0;
        }

        /// <summary>
        /// Displays the channel selection dialog and returns the number of the selected channel
        /// </summary>
        /// <param name="api">A DmxApi object representing the current instance of the API</param>
        /// <returns>Selected channel number</returns>
        public static int ShowChannelSelector(DmxApi api)
        {
            string[] chanNumbers = api.AvailableChannelNumbers;
            string[] chanNames = api.AvailableChannelNames;

            List<string> channelSelectionList = new List<string>();
            for (int i = 0; i < chanNames.Length; i++)
            {
                channelSelectionList.Add($"{chanNumbers[i]}, {chanNames[i]}");
            }

            var dialog = new SelectChannelDialog(channelSelectionList);

            UserInput.Request(dialog, TimeSpan.MaxValue, true);

            if (dialog.Response == WaitForInputResult.Cancel)
            {
                return 0;
            }

            char[] sep = new[] { ',' };
            string selection = dialog.ChannelNumber.Split(sep)[0];

            return Convert.ToInt32(selection);

        }

        public static string ShowSettingSelector(DmxApi api, int channelId)
        {
            List<string> settingsList = api.ListUpdatableChannelSettings(channelId).ToList();
            if (settingsList?.Count > 0)
            {
                var dialog = new SelectChannelSettingDialog(settingsList);
                UserInput.Request(dialog, TimeSpan.MaxValue, true);
                if (dialog.Response == WaitForInputResult.Cancel)
                {
                    return String.Empty;
                }

                return dialog.SettingName;
            }

            Log.Error(null, null, $"channel {channelId} does not have any configurable settings");

            return String.Empty;
            
        }

        /// <summary>
        /// Displays the parameter selection dialog.
        /// </summary>
        /// <param name="channelId">the channel number to list parameters from.</param>
        /// <returns>the name of the selected parameter and format.</returns>
        public static string ShowParameterSelector(int channelId)
        {
            var dialog = new SelectParameterDialog(channelId);
            UserInput.Request(dialog, TimeSpan.MaxValue, true);
            if (dialog.Response == WaitForInputResult.Cancel)
            {
                return String.Empty;
            }

            return dialog.Parameter;
        }

        /// <summary>
        /// Loads an assembly from the location described in the system environment variable 'DmxInstallDir' using Assembly.LoadFile() method. This method uses the assembly name to determine the DLL file to attempt to load.
        /// </summary>
        /// <param name="assemblyFullName">Assembly name to attempt to load</param>
        /// <returns>Assembly loaded from the 'DmxInstallDir' if found, null otherwise.</returns>
        public static Assembly LoadDmxLibraryFromDmxInstallDir(string assemblyFullName)
        {
            Assembly loadedAssembly = null;

            try
            {
                string folderPath = Environment.GetEnvironmentVariable(@"DmxInstallDir");
                AssemblyName assemblyName = new AssemblyName(assemblyFullName);
                string assemblyPath = Path.Combine(folderPath, $"{assemblyName.Name}.dll");

                if (string.IsNullOrWhiteSpace(folderPath) || !File.Exists(assemblyPath)) return null;

                loadedAssembly = Assembly.LoadFile(assemblyPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return loadedAssembly;
        }
    }
}
