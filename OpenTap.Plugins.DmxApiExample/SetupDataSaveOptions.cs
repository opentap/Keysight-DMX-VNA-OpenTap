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
using System.IO;
using System.Linq;
using System.Text;
using Keysight.Dmx.Service;

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// This test step allows the user to configure preferences that impact the behavior of the
    /// <see cref="SaveChannelData"/> and <see cref="SaveStateData"/> steps.  It only needs to be added to
    /// the sequence once after each call to <see cref="LoadConfigurationStep"/>.
    /// </summary>
    [Display("SetupDataSaveOptions", Group: "OpenTap.Plugins.DmxApiExample", Description: "Configure options for data saving steps")]
    public class SetupDataSaveOptions : TestStep
    {
        #region Settings
        /// <summary>
        /// Sets the directory path where the subsequent <see cref="SaveChannelData"/> and <see cref="SaveStateData"/> steps will save their
        /// corresponding data files.
        /// </summary>
        [Display("Data Folder Path", Group: "OpenTap.Plugins.DmxApiExample", Description: "Select the folder for saved data files.", Order: 1)]
        [DirectoryPath]
        public string DataFolderPath { get; set; }


        /// <summary>
        /// Sets the File type selection (CSV, MDIF, CITI) for any subsequent calls to <see cref="SaveChannelData"/> and <see cref="SaveStateData"/> steps.
        /// </summary>
        [Display("Data File Type", Group: "OpenTap.Plugins.DmxApiExample", Description: "File type used to save trace, channel, or state data", Order: 2)]
        [AvailableValues(nameof(FileTypes))]
        public string FileType { get; set; }


        /// <summary>
        /// Sets the Data Format selection (LogMagPhase, MagPhase, RealImaginary) for any subsequent calls to <see cref="SaveChannelData"/> and <see cref="SaveStateData"/> steps.
        /// </summary>
        [Display("Data Format Type", Group: "OpenTap.Plugins.DmxApiExample", Description: "Data format used to save trace, channel, or state data", Order: 2)]
        [AvailableValues(nameof(DataFormats))]
        public string DataFormat { get; set; }


        /// <summary>
        /// List of allowed File Types.
        /// </summary>
        public List<string> FileTypes => TapDmxApi.DataSaveFileTypes;

        /// <summary>
        /// List of allowed Data formats
        /// </summary>
        public List<string> DataFormats => TapDmxApi.DataSaveFormats;

        #endregion

        public SetupDataSaveOptions()
        {
            DataFolderPath = Environment.CurrentDirectory;
            DataFormat = DataFormats.First();
            FileType = FileTypes.First();
        }

        public override void Run()
        {
            if (!Directory.Exists(DataFolderPath))
            {
                Log.Error($"The selected directory ({DataFolderPath}) does not exist");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            DmxApi api = TapDmxApi.DmxInstance;

            try
            {
                api.SetSaveDataOptions(DataFolderPath, (DataSaveFileTypes)Enum.Parse(typeof(DataSaveFileTypes), FileType), (DataSaveFormats)Enum.Parse(typeof(DataSaveFormats), DataFormat));

                DmxPluginSettings.DataDirectory = DataFolderPath;
                DmxPluginSettings.DataFileType = FileType;

                UpgradeVerdict(Verdict.Pass);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                UpgradeVerdict(Verdict.Error);
            }
            
            
        }
    }
}
