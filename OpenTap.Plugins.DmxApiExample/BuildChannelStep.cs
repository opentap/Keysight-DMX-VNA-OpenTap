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
using System.Linq;
using System.Text;

using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    public enum WaitForInputResult
    {
        // The number assigned, determines the order in which the buttons are shown in the dialog.
        Cancel = 2, Ok = 1
    }

    /// <summary>
    /// When run, this test step will build the selected channel from the loaded configuration <see cref="LoadConfigurationStep"/>
    /// This step must be placed in the test plan after a <see cref="ConnectionStep"/> and the LoadConfigurationStep.
    /// </summary>
    [Display("Build Channel", Group: "OpenTap.Plugins.DmxApiExample", Description: "This step builds the selected channel in the configuration on the selected VNA")]
    public class BuildChannelStep : TestStep
    {
        #region Settings

        /// <summary>
        /// If this setting is checked, when this step is executed in the sequence, a dialog will appear with a list of current channels in the configurations,
        /// from which the user can select a channel.  When this setting is checked, the <see cref="ChannelId"/> setting is disabled.
        /// </summary>
        [Display("Show Channel Selection Dialog", Group: "OpenTap.Plugins.DmxApiExample", Description: "Show a dialog to select one of configured channels")]
        public bool DisplayChannelSelectionDialog { get; set; }

        /// <summary>
        /// The channel number for the channel in the loaded configuration to be built in the VNA,  The channel number must be for a channel that exists in
        /// the loaded configuration, otherwise an exception will be thrown during the step execution.  
        /// </summary>
        [Display("Channel Number In Configuration", Group: "OpenTap.Plugins.DmxApiExample", Description: "The channel number as assigned in the configuration")]
        [EnabledIf("DisplayChannelSelectionDialog", false)]
        public int ChannelId { get; set; }

        /// <summary>
        /// This setting when checked, will force a final sweep of all the channel after it has been fully configured on the VNA.
        /// The final sweep will also update any analysis that is turned on for the measurements in the configuration.  Unless the DUT is connected to the
        /// VNA when the Build All Channels step is executed, the final sweep is not needed and would add unnecessary delay to the execution of the sequence.
        /// </summary>
        [Display("Sweep After Setup", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, sweeps all new channels after setup is complete")]
        public bool SweepAfterSetup { get; set; }

        /// <summary>
        /// This setting when checked, will disable the VNA display updates and blank the existing windows on the VNA while the build configuration process is taking place.
        /// Remote commands to the VNA are processed slightly faster when the display is disabled.
        /// </summary>
        [Display("Do Fast Setup", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, VNA display will be disabled during the setup to speed up operations")]
        public bool DoFastSetup { get; set; }

        #endregion

        public BuildChannelStep()
        {
            Rules.Add(HasLoadConfigurationStep, "Can only be called after a LoadConfigurationStep", "ChannelId");
        }

        public override void Run()
        {
            

            DmxApi api = TapDmxApi.DmxInstance;

            //if connected to a valid instance of DmxApi
            if (api != null && !string.IsNullOrEmpty(api.VnaId))
            {
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
                    Log.Info($"Starting build of channel {ChannelId}");
                    api.DoFastVnaSetup = DoFastSetup;
                    api.BuildChannel(ChannelId, SweepAfterSetup);
                    UpgradeVerdict(Verdict.Pass);
                }
                catch (DmxApiException e)
                {
                    Log.Error(e);
                    UpgradeVerdict(Verdict.Error);
                }
            }
        }

        private bool HasLoadConfigurationStep()
        {
            TestPlan tp = this.GetParent<TestPlan>();



            if (tp != null && tp.ChildTestSteps.Count >= 1)
            {
                bool isThisFirst = false;
                foreach (var ts in tp.ChildTestSteps)
                {
                    if (ts.GetType() == typeof(BuildChannelStep))
                        isThisFirst = true;
                    if (ts.GetType() == typeof(LoadConfigurationStep) && !isThisFirst)
                        return true;
                }
            }

            return false;
        }

        

    }
}
