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
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This test step will save all the data associated with a selected channel.  the selected channel must already be built on the connected
    /// VNA and have valid data.  This step must be added after the <see cref="SetupDataSaveOptions"/> step, which determines the file type,
    /// default data directory path, and the data format.
    /// </summary>
    [Display("SaveChannelData", Group: "OpenTap.Plugins.DmxApiExample", Description: "Save all the data associated with the specified channel")]
    public class SaveChannelData : TestStep
    {
        #region Settings

        /// <summary>
        /// If this setting is checked, when this step is executed in the sequence, a dialog will appear with a list of current channels in the configurations,
        /// from which the user can select a channel.  When this setting is checked, the <see cref="ChannelId"/> setting is disabled.
        /// </summary>
        [Display("Show Channel Selection Dialog", Group: "OpenTap.Plugins.DmxApiExample", Description: "Show a dialog to select one of configured channels", Order: 1)]
        public bool DisplayChannelSelectionDialog { get; set; }

        /// <summary>
        /// The channel number for the channel in the loaded configuration to be built in the VNA,  The channel number must be for a channel that exists in
        /// the loaded configuration, otherwise an exception will be thrown during the step execution.
        /// </summary>
        [Display("Channel Number In Configuration", Group: "OpenTap.Plugins.DmxApiExample", Description: "The channel number as assigned in the configuration", Order: 2)]
        [EnabledIf("DisplayChannelSelectionDialog", false)]
        public int ChannelId { get; set; }

        /// <summary>
        /// This setting holds the base file name (no directory or file extension needed) where the channel data will be saved.
        /// </summary>
        [Display("Data File Name", Group: "OpenTap.Plugins.DmxApiExample", Description: "the file name (excluding the directory and extension) for the data file", Order: 3)]
        public string FileName { get; set; }
        #endregion

        public SaveChannelData()
        {
            Rules.Add(HasSetupDataSaveOptionsStep, "Can only be called after a SetupDataSaveOptions step", "ChannelId");
        }

        public override void Run()
        {
            var api = TapDmxApi.DmxInstance;

            if (DisplayChannelSelectionDialog)
            {
                ChannelId = ExtensionMethods.ShowChannelSelector(api);
                if (ChannelId == 0) //user canceled
                {
                    Log.Error("User did not input channel ID");
                    UpgradeVerdict(Verdict.Aborted);
                    return;
                }
            }

            try
            {
                api.SaveChannelData(FileName, ChannelId);
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
                    if (ts.GetType() == typeof(SaveChannelData))
                        isThisFirst = true;
                    if (ts.GetType() == typeof(SetupDataSaveOptions) && !isThisFirst)
                        return true;
                }
            }

            return false;
        }
    }
}
