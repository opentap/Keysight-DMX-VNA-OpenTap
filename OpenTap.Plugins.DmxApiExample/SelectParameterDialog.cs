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
    /// A dialog that presents a list of selectable parameters to the user based on the selected channel.
    /// the calling method needs to pass the channelId to the constructor of this class.
    /// </summary>
    public class SelectParameterDialog
    {
        // Name is handled specially to create the title of the dialog window.
        public string Name { get { return $"Please select a parameter from channel {ChannelId}"; }}

        [Browsable(false)]
        public int ChannelId { get; set; }

        private List<string> _parameterList;

        [Browsable(false)]
        public List<string> ParameterList
        {
            get { return _parameterList; }
            set { _parameterList = value; }
        }


        [AvailableValues("ParameterList")]
        public string Parameter { get; set; }

        public SelectParameterDialog(int channelId)
        {
            ChannelId = channelId;

            var api = TapDmxApi.DmxInstance;
            ParameterList = api.AvailableParameters(ChannelId).ToList();
        }

        [Layout(LayoutMode.FloatBottom | LayoutMode.FullRow)] // Show the button selection at the bottom of the window.
        [Submit] // When the button is clicked the result is 'submitted', so the dialog is closed.
        public WaitForInputResult Response { get; set; }
    }
}
