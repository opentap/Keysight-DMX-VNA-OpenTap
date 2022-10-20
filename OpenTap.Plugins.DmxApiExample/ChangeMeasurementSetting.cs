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
    /// This test step allows the user to modify a channel setting (ex. "Carrier Frequency") in measurement that has already been built.
    /// this step can be used in a loop to modify a supported measurement setting for a given measurement without having to rebuild the
    /// entire channel.  A call to this step must be followed by a call to <see cref="SweepChannelStep"/>.
    /// multiple calls to this step to change different measurement settings can be grouped together before a single call to <see cref="SweepChannelStep"/>
    /// </summary>
    [Display("ChangeMeasurementSetting", Group: "OpenTap.Plugins.DmxApiExample", Description: "Change a measurement setting for an existing measurement")]
    public class ChangeMeasurementSetting : TestStep
    {
        #region Settings
        // <summary>
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
        [EnabledIf(nameof(DisplayChannelSelectionDialog), false)]
        public int ChannelId { get; set; }

        /// <summary>
        /// If this setting is checked, when this step is executed in the sequence, a dialog will appear with a list of available parameters in the selected channel.
        /// When this setting is checked, the <see cref="Parameter"/> setting is disabled.
        /// </summary>
        [Display("Show Setting Selection Dialog", Group: "OpenTap.Plugins.DmxApiExample", Description: "Show a dialog to select one of the available settings in the selected channel", Order: 3)]
        public bool DisplaySettingSelectionDialog { get; set; }

        [Display(nameof(SettingName), Group: "OpenTap.Plugins.DmxApiExample", Description: "The name of the setting to be changed", Order: 4)]
        [EnabledIf(nameof(DisplaySettingSelectionDialog), false)]
        public string SettingName { get; set; }

        [Display(nameof(SettingValue), Group: "OpenTap.Plugins.DmxApiExample", Description: "The new value of the setting", Order: 5)]
        public string SettingValue { get; set; }

        #endregion

        public ChangeMeasurementSetting()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void Run()
        {
            var api = TapDmxApi.DmxInstance;

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

                if (DisplaySettingSelectionDialog)
                {
                    SettingName = ExtensionMethods.ShowSettingSelector(api, ChannelId);
                    if (SettingName == String.Empty)
                    {
                        Log.Error("User did not input a setting");
                        UpgradeVerdict(Verdict.Aborted);
                        return;
                    }
                }

                api.ModifyChannelSetting(ChannelId, SettingName, (object) SettingValue);
                UpgradeVerdict(Verdict.Pass);
            }
        }
    }
}
