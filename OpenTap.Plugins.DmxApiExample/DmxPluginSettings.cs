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
    /// This class holds various global DMX plugin settings that need to be shared among
    /// different steps.
    /// </summary>
    [Display("DmxPluginSettings", Description: "DMX Plugin settings that need to be shared between steps")]
    public static class DmxPluginSettings 
    {
        // TODO: Add settings here as properties, use DisplayAttribute to indicate to the GUI
        //       how the setting should be displayed.
        //       Example:
        //[Display("Category\\Example Setting", Description:" Just an example")]
        //public bool ExampleSetting { get; set; }

        /// <summary>
        /// The path to the directory to hold saved data file.  This setting is initialized in the
        /// <see cref="SetupDataSaveOptions"/> step.
        /// </summary>
        public static string DataDirectory { get; set; }

        /// <summary>
        /// the current selection for the data file type (csv, mdif, citi).  This setting is initialized in the
        /// <see cref="SetupDataSaveOptions"/> step.
        /// </summary>
        public static string DataFileType { get; set; }


    }
}
