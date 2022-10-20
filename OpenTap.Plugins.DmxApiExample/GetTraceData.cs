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
using Keysight.Dmx.Service;
using Keysight.WorkflowSolutions.Enumerations;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This test step queries the data from a specified channel and a specified parameter in the channel and publishes the result as table of X and Y values.
    /// This step must be placed in the sequence after a LoadConfiguration and build channel step that includes the specified channel and parameter.
    /// </summary>
    [Display("Get Trace Data", Group: "OpenTap.Plugins.DmxApiExample", Description: "Publishes the formatted data from a specified trace in a specified channel into a table of x and y values")]
    public class GetTraceData : TestStep
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
        /// If this setting is checked, when this step is executed in the sequence, a dialog will appear with a list of available parameters in the selected channel.
        /// When this setting is checked, the <see cref="Parameter"/> setting is disabled.
        /// </summary>
        [Display("Show Parameter Selection Dialog", Group: "OpenTap.Plugins.DmxApiExample", Description: "Show a dialog to select one of the available parameters in the selected channel", Order: 3)]
        public bool DisplayParameterSelectionDialog { get; set; }

        /// <summary>
        /// This setting identifies the specific parameter to get data from.  It is automatically set when the Parameter Selection dialog is used, but can also be set
        /// manually.  When setting the parameter manually, the entry must have the format [parameter name],[parameter format].
        /// Example: S21,LogMag or MGain21,Delay
        /// </summary>
        [Display("Parameter", Group: "OpenTap.Plugins.DmxApiExample", Description: "The parameter name to get data from", Order: 4)]
        [EnabledIf("DisplayParameterSelectionDialog", false)]
        public string Parameter { get; set; }

        #endregion

        public GetTraceData()
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

                if (DisplayParameterSelectionDialog)
                {
                    Parameter = ExtensionMethods.ShowParameterSelector(ChannelId);
                    if (String.IsNullOrEmpty(Parameter)) //user canceled
                    {
                        Log.Error("User did not select a parameter");
                        UpgradeVerdict(Verdict.Aborted);
                        return;
                    }
                }

                try
                {
                    var parameterName = Parameter.Split(',').First();
                    var traceFormat = Parameter.Split(',').Last();

                    var format = (DisplayFormats)Enum.Parse(typeof(DisplayFormats), traceFormat); 
                    Log.Info($"Getting data for {parameterName} measurement from channel {ChannelId}");
                    var yData = api.GetTraceData(ChannelId, parameterName, format);

                    var xData = api.GetTraceXAxis(ChannelId, parameterName, format);

                    var xAxisDomain = api.GetMeasurementDomain(ChannelId, parameterName, format);

                    PublishResults(xData, yData, xAxisDomain, traceFormat);

                    UpgradeVerdict(Verdict.Pass);
                }
                catch (DmxApiException e)
                {
                    Log.Error(e);
                    UpgradeVerdict(Verdict.Error);
                }
            }
        }

        private void PublishResults(double[] xData, double[] yData, Tuple<string, string> xAxis, string format)
        {

            Results.PublishTable($"Data: {Parameter} from Channel {ChannelId}", new List<string>() { $"{xAxis.Item1} ({xAxis.Item2})", $"Response ({format})" },
                xData, yData);
        }
    }
}
