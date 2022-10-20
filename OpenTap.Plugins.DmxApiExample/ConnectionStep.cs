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
using OpenTap;   // Use OpenTAP infrastructure/core components (log,TestStep definition, etc)
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{

    /// <summary>
    /// This test step is required as the first step in any test plan using the DmxApiExample TAP plugin.  When run, this
    /// step establishes an IO connection between the client PC (running the TAP sequence) and the VNA.  It requires that
    /// the user has already added a valid VNA instrument to the TAP bench settings.  All steps following the connection step
    /// will use the selected VNA.  A new connection step can be called in the same sequence to redirect subsequent step to a
    /// different VNA.  However, steps targeting different VNAs cannot be interspersed and have to be organized in sequential
    /// blocks preceded each with a connection step. 
    /// </summary>
    [Display("ConnectionStep", Group: "OpenTap.Plugins.DmxApiExample", Description: "Connect to a VNA")]
    public class ConnectionStep : TestStep
    {
        #region Settings


        /// <summary>
        /// The name of a VNA instrument from the test bench settings.  The VNA must be a model supported by the currently
        /// installed DMX application <seealso cref="https://edadocs.software.keysight.com/kkbopen/what-vna-models-work-with-dmx-589310727.html"/>.
        /// In addition the VNA must be running the minimum firmware version required by the currently installed DMX application.
        /// As of version A.3.0.1.58 of the DMX application, the minimum VNA firmware version is A.15.75.19
        /// </summary>
        [Display("VNA Instrument", Group: "OpenTap.Plugins.DmxApiExample",
            Description: "A VNA from the instrument list")]
        public VnaInstrument Vna
        {
            get { return _vna;}
            set
            {
                _vna = value;
                if (_vna != null)
                {
                    TimeOut = Math.Max(_vna.IoTimeout, 5000);
                }
            }
        }

        private VnaInstrument _vna;

        /// <summary>
        /// An instrument IO timeout in milliseconds.  
        /// </summary>
        [Display("Timeout", Group: "OpenTap.Plugins.DmxApiExample", Description: "Timeout (milliseconds) used for IO transactions")]
        public int TimeOut { get; set; }

        #endregion

        public ConnectionStep()
        {
            
        }

        public override void Run()
        {
            if (Vna == null)
            {
                Log.Error("You must first add a suitable VNA instrument");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            try
            {
                Log.Info("Attempting to connect to a DmxInstance");
                TapDmxApi.DmxInstance = new DmxApi();
                TapDmxApi.DmxInstance.ConnectToVNA(Vna.VisaAddress, TimeOut);
                var msg = $"Connected to {TapDmxApi.DmxInstance.VnaId}";
                Log.Info(msg);
                TapDmxApi.DmxInstance.AddToDmxLog(msg);
            }
            catch (Exception e)
            {
                Log.Error(e);
                UpgradeVerdict(Verdict.Error);
                return;
            }
            
            
            // ToDo: Add test case code.
            RunChildSteps(); //If the step supports child steps.

            // If no verdict is used, the verdict will default to NotSet.
            // You can change the verdict using UpgradeVerdict() as shown below.
            // UpgradeVerdict(Verdict.Pass);
        }
    }
}
