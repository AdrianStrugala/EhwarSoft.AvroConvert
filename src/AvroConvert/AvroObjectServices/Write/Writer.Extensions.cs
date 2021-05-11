#region license
/**Copyright (c) 2021 Adrian Strugala
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* https://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System.Collections.Generic;
using SolTechnology.Avro.AvroObjectServices.FileHeader;

namespace SolTechnology.Avro.AvroObjectServices.Write
{
    /// <summary>
    /// Write leaf values.
    /// </summary>
    internal partial class Writer
    {
        internal void WriteHeader(Header header)
        {
            WriteFixed(DataFileConstants.AvroHeader);

            // Write metadata 
            int size = header.GetMetadataSize();
            WriteInt(size);

            foreach (KeyValuePair<string, byte[]> metaPair in header.GetRawMetadata())
            {
                WriteString(metaPair.Key);
                WriteBytes(metaPair.Value);
            }
            WriteMapEnd();


            // Write sync data
            WriteFixed(header.SyncData);
        }

        internal void WriteDataBlock(byte[] data, byte[] syncData, int blockCount)
        {
            // write count 
            WriteLong(blockCount);

            // write data 
            WriteBytes(data);

            // write sync marker 
            WriteFixed(syncData);
        }
    }
}
