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

namespace OpenTap.Plugins.DmxApiExample
{
    /// <summary>
    /// A ResultListener class designed to display up to the first 100 entries of a published table
    /// to the Tap Editor Log.
    /// </summary>
    [Display("LogDisplayResultListener", Group: "OpenTap.Plugins.DmxApiExample", Description: "Shows the first 100 values published by a GetFFTDataBuffer step")]
    public class LogDisplayResultListener : ResultListener
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public LogDisplayResultListener()
        {
            Name = "FFTResultLogger";
            // ToDo: Set default values for properties / settings.
        }

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            Log.Info("Test plan \"{0}\" started", planRun.TestPlanName);
        }

        public override void OnTestStepRunStart(TestStepRun stepRun)
        {
            Log.Info("Test step \"{0}\" started", stepRun.TestStepName);
        }

        public override void OnResultPublished(Guid stepRun, ResultTable result)
        {
            // This is where results are processed. This call was initiated by a call to Results.Publish.
            base.OnResultPublished(stepRun, result);
            
            int maxRowIndex = Math.Min(100, result.Rows);

            // Write out the result table column names.
            StringBuilder sb = new StringBuilder();
            foreach (ResultColumn rc in result.Columns)
            {
                sb.AppendFormat("\t{0}", rc.Name);
            }
            Log.Info(sb.ToString());

            // Write out the rows for each column.  
            for (int rowIndex = 0; rowIndex < maxRowIndex; rowIndex++)
            {
                sb.Clear();
                sb.AppendFormat("Row={0}\t", rowIndex);
                foreach (ResultColumn rc in result.Columns)
                {
                    // Make sure to check to make sure each column has enough rows.
                    if (rowIndex < rc.Data.Length)
                    {
                        sb.AppendFormat("{0}\t", rc.Data.GetValue(rowIndex));
                    }
                    else
                    {
                        sb.AppendFormat("\t");
                    }
                }
                sb.Append(Environment.NewLine);
                Log.Info(sb.ToString());
            }

            OnActivity();
        }

        public override void OnTestStepRunCompleted(TestStepRun stepRun)
        {
            //Add handling code for test step run completed.
        }

        public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream)
        {
            //Add handling for test plan run completed.
        }

        public override void Open()
        {
            base.Open();
            //Add resource open code.
        }

        public override void Close()
        {
            //Add resource close code.
            base.Close();
        }
    }
}
