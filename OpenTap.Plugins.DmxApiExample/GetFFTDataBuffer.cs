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
using System.Numerics;
using System.Text;
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    public enum MODParameters
    {
        PIn,
        POut,
        PGain,
        MGain,
    }

    /// <summary>
    /// This is a special data acquisition step designed to get the full FFT spectrum data for selected parameters in a Modulation Distortion
    /// measurement channel.  The values for each execution of the step will be published to a table that contains the frequency, Log Magnitude and
    /// phase (degrees) of each parameter.  This step should be called after a LoadConfiguration step that contains a modulation distortion channel
    /// and after that channel has been built and swept.
    /// 
    /// </summary>
    [Display("GetFFTDataBuffer", Group: "OpenTap.Plugins.DmxApiExample", Description: "Gets full FFT results from selected channel (must be a MOD channel) and selected parameter.")]
    public class GetFFTDataBuffer : TestStep
    {
        #region Settings
        /// <summary>
        /// If this setting is checked, when this step is executed in the sequence, a dialog will appear with a list of current channels in the configurations,
        /// from which the user can select a channel.  When this setting is checked, the <see cref="ChannelId"/> setting is disabled.
        /// NOTE: The selected channel must be of type Modulation Distortion. 
        /// </summary>
        [Display("Show Channel Selection Dialog", Group: "OpenTap.Plugins.DmxApiExample", Description: "Show a dialog to select one of configured channels", Order:1)]
        public bool DisplayChannelSelectionDialog { get; set; }

        /// <summary>
        /// The channel number for the channel in the loaded configuration to be built in the VNA,  The channel number must be for a channel that exists in
        /// the loaded configuration, otherwise an exception will be thrown during the step execution.
        /// NOTE: The selected channel must be of type Modulation Distortion. 
        /// </summary>
        [Display("Channel Number In Configuration", Group: "OpenTap.Plugins.DmxApiExample", Description: "The channel number as assigned in the configuration", Order:2)]
        [EnabledIf("DisplayChannelSelectionDialog", false)]
        public int ChannelId { get; set; }

        /// <summary>
        /// The VNA port number selected as the input to the DUT.  For a Digital-RF Transmitter measurement this is always
        /// port 1.
        /// </summary>
        [Display("Input Port Number", Group: "OpenTap.Plugins.DmxApiExample", Description: "Input port of the selected channel", Order:3)]
        public int InputPort { get; set; }

        /// <summary>
        /// The VNA port number selected as the output from the DUT.  For a Digital-RF Receiver measurement this is always
        /// port 2.
        /// </summary>
        [Display("Output Port Number", Group: "OpenTap.Plugins.DmxApiExample", Description: "Output port of the selected channel", Order:4)]
        public int OutputPort { get; set; }

        /// <summary>
        /// The modulation distortion parameter to get data from.  This will be selected from a defined list of available parameters
        /// </summary>
        [Display("Selected Parameter", Group: "OpenTap.Plugins.DmxApiExample", Description:"The MOD parameter used for data transfer", Order:5)]
        [SuggestedValues(nameof(MODParameterNames))]
        public string Parameter { get; set; }

        /// <summary>
        /// List of available modulation distortion parameters
        /// </summary>
        public List<string> MODParameterNames => Enum.GetNames(typeof(MODParameters)).ToList();
        
        #endregion

       
        public GetFFTDataBuffer()
        {
            InputPort = 1;
            OutputPort = 2;
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
                    var parameter = SelectedParameter();
                    Log.Info($"Getting FFT Data from {parameter} in channel {ChannelId}");
                    var results = api.GetFFTBufferData(ChannelId, parameter);
                    if (results != null && results.Length >= 1)
                    {
                        var datestamp = DateTime.Now.ToLongDateString();
                        UpgradeVerdict(Verdict.Pass);
                        PublishResults(results);
                    }
                }
                catch (DmxApiException e)
                {
                    Log.Error(e);
                    UpgradeVerdict(Verdict.Error);
                }
            }
        }

        #region Private Methods

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


        private string SelectedParameter()
        {
            var parameter = "";
            switch (Parameter.ToLower())
            {
                case "pin":
                    parameter = $"{Parameter}{InputPort}";
                    break;
                case "pout":
                    parameter = $"{Parameter}{OutputPort}";
                    break;
                default:
                    parameter = $"{Parameter}{OutputPort}{InputPort}";
                    break;
            }

            return parameter;

        }

        private void PublishResults(Tuple<double, Complex>[] data)
        {
            List<double> frequencies = new List<double>();
            List<double> mags = new List<double>();
            List<double> phases = new List<double>();

            foreach (var dataPt in data)
            {
                {
                    frequencies.Add(dataPt.Item1);
                    var mag = dataPt.Item2.LogMag();
                    var phase = dataPt.Item2.DegPhase();

                    mags.Add(mag);
                    phases.Add(phase);
                }
            }

            var carrierFrequency = ((frequencies.Max() + frequencies.Min()) / 2) / 1E6;
            var resultName = $"{SelectedParameter()}_CH#{ChannelId}_CF-{carrierFrequency}MHz";
            Results.PublishTable(resultName, new List<string>() {"Frequency (Hz)", "Magnitude (dB)", "Phase (deg)"},
                frequencies.ToArray(), mags.ToArray(), phases.ToArray());
        }

        #endregion
    }
}
