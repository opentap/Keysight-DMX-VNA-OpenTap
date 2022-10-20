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

using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This step allows the user to save the data in the current VNA state.
    /// This step must be added after the <see cref="SetupDataSaveOptions"/> step, which determines the file type,
    /// default data directory path, and the data format.
    /// </summary>
    [Display("SaveStateData", Group: "OpenTap.Plugins.DmxApiExample", Description: "Save all the data in the VNA state to a single file based on the SetupDataSaveOption settings.")]
    public class SaveStateData : TestStep
    {
        #region Settings
        /// <summary>
        /// This setting holds the base file name (no directory or file extension needed) where the channel data will be saved.
        /// </summary>
        [Display("Data File Name", Group: "OpenTap.Plugins.DmxApiExample", Description: "the file name (excluding the directory and extension) for the data file", Order: 3)]
        public string FileName { get; set; }
        #endregion

        public SaveStateData()
        {
            Rules.Add(HasSetupDataSaveOptionsStep, "Can only be called after a SetupDataSaveOptions step", "FileName");
        }

        public override void Run()
        {
            var api = TapDmxApi.DmxInstance;

            try
            {
                api.SaveStateData(FileName);
                var dir = DmxPluginSettings.DataDirectory;
                var type = DmxPluginSettings.DataFileType;
                Log.Info($"data saved to {FileName}.{type.ToLower()} in {dir} folder.");
                UpgradeVerdict(Verdict.Pass);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                UpgradeVerdict(Verdict.Error);
            }
        }

        private bool HasSetupDataSaveOptionsStep()
        {
            TestPlan tp = this.GetParent<TestPlan>();



            if (tp != null && tp.ChildTestSteps.Count >= 1)
            {
                bool isThisFirst = false;
                foreach (var ts in tp.ChildTestSteps)
                {
                    if (ts.GetType() == typeof(SaveStateData))
                        isThisFirst = true;
                    if (ts.GetType() == typeof(SetupDataSaveOptions) && !isThisFirst)
                        return true;
                }
            }

            return false;
        }
    }
}
