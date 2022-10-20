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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This class can be used to show a list of possible channel settings that the user can select from
    /// for the purpose of using that setting in <see cref="ChangeMeasurementSetting"/> step.  
    /// The calling method needs to provide the selection list to the constructor of this class.
    /// </summary>
    public class SelectChannelSettingDialog
    {
        // Name is handled specially to create the title of the dialog window.
        public string Name => "Please select the Channel Setting you want to modify";


        [Browsable(false)]
        public List<string> Settings { get; set; }


        [AvailableValues(nameof(Settings))]
        public string SettingName { get; set; }

        public SelectChannelSettingDialog(List<string> settingList)
        {
            Settings = settingList;
            SettingName = Settings.First();
        }

        [Layout(LayoutMode.FloatBottom | LayoutMode.FullRow)] // Show the button selection at the bottom of the window.
        [Submit] // When the button is clicked the result is 'submitted', so the dialog is closed.
        public WaitForInputResult Response { get; set; }

    }
}
