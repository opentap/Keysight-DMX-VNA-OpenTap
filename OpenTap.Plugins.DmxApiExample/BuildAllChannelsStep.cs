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
using System.ComponentModel;
using OpenTap;
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// When run, this test step will force the building of all the channel included in the loaded configuration <seealso cref="LoadConfigurationStep"/>
    /// This step must be placed in the test plan after a <seealso cref="ConnectionStep"/> and the LoadConfigurationStep.
    /// </summary>
    [Display("Build All Channels", Group: "OpenTap.Plugins.DmxApiExample", Description: "This step builds all channels in the configuration on the selected VNA")]
    public class BuildAllChannelsStep : TestStep
    {
        #region Settings
        /// <summary>
        /// This setting when checked will force all the new channels and measurements in the selected configuration to be placed into a new Sheet on the
        /// VNA display without disturbing any existing measurements that may be present in the current instrument state.  The channel numbers in the loaded
        /// DMX configuration will automatically be adjusted to accomodate the existing channels in the instrument.  If not checked, then a factory preset will
        /// be performed on the VNA before the DMX configuration is applied to the instrument.
        /// </summary>
        [Display("Append Channels In New Sheet", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, appends to existing state, otherwise, resets VNA", Order: 1)]
        public bool AppendToState { get; set; }

        /// <summary>
        /// This setting when checked, will force a final sweep of all the channels in the configuration after all the channels and measurements have been fully configured
        /// on the VNA.  The final sweep will also update any analysis that is turned on for the measurements in the configuration.  Unless the DUT is connected to the
        /// VNA when the Build All Channels step is executed, the final sweep is not needed and would add unnecessary delay to the execution of the sequence.
        /// </summary>
        [Display("Sweep After Setup", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, sweeps all new channels after setup is complete", Order: 2)]
        public bool SweepAfterSetup { get; set; }

        /// <summary>
        /// This setting when checked, will disable the VNA display updates and blank the existing windows on the VNA while the build configuration process is taking place.
        /// Remote commands to the VNA are processed slightly faster when the display is disabled.
        /// </summary>
        [Display("Do Fast Setup", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, VNA display will be disabled during the setup to speed up operations", Order: 3)]
        public bool DoFastSetup { get; set; }

        /// <summary>
        /// DMX configuration files retain the original channel and window numbers that were used when the configuration was originally created.  The original numbering of the
        /// channels and windows may not be suitable when that same configuration is used in a production automated test environment.  This setting when checked, will force a
        /// reordering of all the channel numbers in the loaded configuration (Note: the reordering happens in memory only and will not impact the configuration file) based
        /// on a <seealso cref="NewStartingChannel"/> value.  If the NewStartingChannel number is left at 0, then the new starting channel will be the next available channel
        /// number in the current VNA state.
        /// </summary>
        [Display("Reorder Channels", Group: "OpenTap.Plugins.DmxApiExample", Description: "If checked, channels and window numbers in the configuration will be reordered.", Order: 3)]
        public bool ReorderChannels { get; set; }

        /// <summary>
        /// This setting is used when the <seealso cref="ReorderChannels"/> setting is checked.  The Channel/Window reordering process will use the value of this setting as the
        /// new starting channel number for the loaded configuration.  If the <seealso cref="AppendToState"/> is also checked, the New Starting Channel will automatically
        /// increment if a channel with that number already exists in the current VNA state.
        /// </summary>
        [Display("New Starting Channel", Group: "OpenTap.Plugins.DmxApiExample", Description: "Enter a new starting channel number.  0 will pick the next available channel.", Order: 3)]
        [EnabledIf("ReorderChannels", true)]
        public int NewStartingChannel { get; set; }
        #endregion

        public BuildAllChannelsStep()
        {
            Rules.Add(HasLoadConfigurationStep, "Can only be called after a LoadConfigurationStep", "AppendToState");
            AppendToState = true;
            SweepAfterSetup = false;
        }

        public override void Run()
        {
            DmxApi api = TapDmxApi.DmxInstance;

            //if connected to a valid instance of DmxApi
            if (api != null && !string.IsNullOrEmpty(api.VnaId))
            {
                try
                {
                    
                    Log.Info($"Starting build of the following channels: {string.Join(",", api.AvailableChannelNumbers)}");
                    if (ReorderChannels)
                    {
                        api.ReorderChannelsWindows(NewStartingChannel, AppendToState);
                    }

                    api.DoFastVnaSetup = DoFastSetup;
                    api.BuildAllChannels(AppendToState, SweepAfterSetup);
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
                    if (ts.GetType() == typeof(BuildAllChannelsStep))
                        isThisFirst = true;
                    if (ts.GetType() == typeof(LoadConfigurationStep) && !isThisFirst)
                        return true;
                }
            }

            return false;
        }
    }
}
