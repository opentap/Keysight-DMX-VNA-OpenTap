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
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using OpenTap;
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This test step loads a DMX configuration file (*.dmx) into the sequence.  This step must be called after a Connection Step.
    /// All subsequent build, sweep and data acquisition steps in the plugin will operate on the measurements included in the configuration
    /// file.  If there are measurements in the configuration file that are not supported on the specific VNA used in the connection step,
    /// those measurements will be removed from the loaded configuration.  The original configuration file is never altered.
    /// </summary>
    [Display("Load DMX Configuration", Group: "OpenTap.Plugins.DmxApiExample", Description: "This step will allow the loading of a DMX configuration file.  Can only follow a ConnectionStep")]
    public class LoadConfigurationStep : TestStep
    {
        #region Settings

        /// <summary>
        /// This setting holds the fully qualified file path to a DMX configuration file (*.dmx)
        /// </summary>
        [FilePath(FilePathAttribute.BehaviorChoice.Open, "dmx")]
        [Display("Select DMX Configuration File", Group: "OpenTap.Plugins.DmxApiExample", Description: "DMX configuration file (*.dmx)")]
        public string DmxConfigFile { get; set; }

        /// <summary>
        /// This setting controls how the channels included in the loaded configuration will be numbered when they are built.
        /// The renumbering options are:  "Renumber starting at next Available channel", "Renumber starting at channel 1", "No Renumbering"
        /// the selection of this option only applies to the loaded configuration in memory and has no impact on the original
        /// DMX configuration file.
        /// </summary>
        [Display("Channel Renumbering mode", Group: "OpenTap.Plugins.DmxApiExample", Description: "Controls how channels will be numbered when configuration is built on the VNA")]
        [AvailableValues(nameof(RenumberingOptions))]
        public string ChannelRenumbering { get; set; }

        /// <summary>
        /// List of channel renumbering options.
        /// </summary>
        public List<string> RenumberingOptions => _renumberingOption;
        
        #endregion

        private List<string> _renumberingOption;
        public LoadConfigurationStep()
        {
            Rules.Add(IsFileValid, "Must be a valid file", "DmxConfigFile");
            Rules.Add(HasConnectionStep, "Can only be called after a ConnectionStep", "DmxConfigFile");
            var options = new string[]
                { "Renumber starting at next Available channel", "Renumber starting at channel 1", "No Renumbering" };
            _renumberingOption = options.ToList();
        }

        public override void Run()
        {
            DmxApi api = TapDmxApi.DmxInstance;

            //if connected to a valid instance of DmxApi
            if (api != null && !string.IsNullOrEmpty(api.VnaId))
            {
                api.LoadConfiguration(DmxConfigFile);
                if (ChannelRenumbering == RenumberingOptions[0])
                    api.ReorderChannelsWindows(0, true);
                if (ChannelRenumbering == RenumberingOptions[1])
                    api.ReorderChannelsWindows(1, false);


                if (api.AvailableChannels.Length < 1)
                {
                    Log.Error("There are no channels in the loaded configuration");
                    UpgradeVerdict(Verdict.Error);
                    return;
                }
                var msg = $"Loaded configuration file: {DmxConfigFile}";
                Log.Info(msg);
                api.AddToDmxLog(msg);
                UpgradeVerdict(Verdict.Pass);
                
            }
        }

        private bool IsFileValid()
        {
            if (string.IsNullOrEmpty(DmxConfigFile)) return false;

            return File.Exists(DmxConfigFile);
        }

        private bool HasConnectionStep()
        {
            TestPlan tp = this.GetParent<TestPlan>();
            

            if (tp != null && tp.ChildTestSteps.Count >= 1)
            {
                bool isThisFirst = false;
                foreach (var ts in tp.ChildTestSteps)
                {
                    if (ts.GetType() == typeof(LoadConfigurationStep))
                        isThisFirst = true;
                    if (ts.GetType() == typeof(ConnectionStep) && !isThisFirst)
                        return true;
                }
            }

            return false;
        }


    }
}
