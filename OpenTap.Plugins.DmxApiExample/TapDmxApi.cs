
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
using System.Reflection;
using Keysight.Dmx.Service;
using Keysight.WorkflowSolutions.Enumerations;

namespace OpenTap.Plugins.DmxApiExample
{

    /// <summary>
    /// This static class is designed to hold an instance of a DmxApi object so that it can be
    /// shared among all the steps in a TAP session.
    /// The DmxInstance is initialized in the <see cref="ConnectionStep"/>.  In the ConnectionStep, a new instance
    /// of the DmxApi object is created and a call is made to the DmxApi.ConnectToVna, using the
    /// address and the timeout assigned to the VnaInstrument assigned to the ConnectionStep.
    /// All subsequent steps in the session will use the same instance of the DmxApi and the connected
    /// VNA until another ConnectionStep is executed.
    /// </summary>
    static class TapDmxApi
    {
        /// <summary>
        /// Global instance of a DmxApi object
        /// </summary>
        public static DmxApi DmxInstance { get; set; }

        /// <summary>
        /// Provides a list of allowable file types for the <see cref="SetupDataSaveOptions"/> step.
        /// </summary>
        public static List<string> DataSaveFileTypes => Enum.GetNames(typeof(DataSaveFileTypes)).ToList();

        /// <summary>
        /// Provides a list of allowable Data save formats for the <see cref="SetupDataSaveOptions"/> step.
        /// </summary>
        public static List<string> DataSaveFormats => Enum.GetNames(typeof(DataSaveFormats)).ToList();

        /// <summary>
        /// Provides a list of allowable trace formats for the <see cref="SetupDataSaveOptions"/> step.
        /// </summary>
        public static List<string> TraceFormats => Enum.GetNames(typeof(DisplayFormats)).ToList();

        /// <summary>
        /// The constructor for the <see cref="TapDmxApi"/> class.  Since this constructor is called before any
        /// DmxApi related DLLs are needed, this is where an event handler for the AppDomain.CurrentDomain.AssemblyResolve
        /// event is added.  <seealso cref="AssemblyResolver"/>
        /// </summary>
        static TapDmxApi()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;
        }

        /// <summary>
        /// This method is a handler for the AppDomain.CurrentDomain.AssemblyResolve event that fires when a reference assembly fails to load.
        /// It is designed to allow the use of the Keysight.Dmx.Service.dll (the assembly that contains DmxApi) without requiring the client 
        /// application to have a copy of all the support dlls that are referenced by Keysight.Dmx.Service.
        /// The client application (this Tap Plugin is an example of a client application) only needs to directly reference the
        /// Keysight.Dmx.Service.dll and Keysight.WorkflowSolutions.Enumerations.dll in the project dependencies.  These two DLLs
        /// can be found in the DMX installation directory.  The other required DLLs will be dynamically loaded by this AssemblyResolver
        /// handler function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            List<string> dmxLibraries = new List<string>();

            // DMX specific libraries
            dmxLibraries.Add("Keysight.Dmx.VISAHelper");
            dmxLibraries.Add("Keysight.WorkflowSolutions.AppConfigHelper");
            dmxLibraries.Add("Keysight.WorkflowSolutions.WpfUIControls");
            
            // 3rd party Open Source libraries redistributed with DMX installation
            dmxLibraries.Add("NLog");

            // DMX CommandExpert SCPI.NET runtime libraries
            dmxLibraries.Add("AgVNA_SCPInet");
            dmxLibraries.Add("Keysight.CommandExpert.Common");
            dmxLibraries.Add("Keysight.CommandExpert.DataModel");
            dmxLibraries.Add("Keysight.CommandExpert.InstrumentAbstraction");
            dmxLibraries.Add("Keysight.CommandExpert.Scpi");

            if (dmxLibraries.Any(args.Name.Contains))
            {
                try
                {
                    return ExtensionMethods.LoadDmxLibraryFromDmxInstallDir(args.Name);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return null;
        }
    }
}
