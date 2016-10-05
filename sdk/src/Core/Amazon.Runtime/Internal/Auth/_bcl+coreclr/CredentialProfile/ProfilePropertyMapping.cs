/*
 * Copyright 2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 *  http://aws.amazon.com/apache2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amazon.Runtime.Internal.Auth.CredentialProfile
{
    /// <summary>
    /// Class to easily convert from Dictionary&lt;string, string&gt; to ProfileOptions and back.
    /// </summary>
    public class ProfilePropertyMapping
    {
        private static readonly HashSet<string> TypePropertySet =
            new HashSet<string>(typeof(ProfileOptions).GetProperties().Select((p) => p.Name));

        private Dictionary<string, string> nameMapping;

        public ProfilePropertyMapping(Dictionary<string, string> nameMapping)
        {
            if (!TypePropertySet.SetEquals(new HashSet<string>(nameMapping.Keys)))
            {
                throw new ArgumentException("The nameMapping Dictionary must contain a name mapping for each ProfileOptions property, and no additional keys.");
            }

            this.nameMapping = nameMapping;
        }

        public List<KeyValuePair<string, string>> Convert(ImmutableProfileOptions profileOptions)
        {
            var list = new List<KeyValuePair<string, string>>();
            var properties = typeof(ImmutableProfileOptions).GetProperties();

            // ensure repeatable order
            Array.Sort(properties.Select((p)=>p.Name).ToArray(), properties);

            foreach (var property in properties)
            {
                var value = (string)property.GetValue(profileOptions, null);
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add(new KeyValuePair<string, string>(nameMapping[property.Name], value));
                }
            }
            return list;
        }

        public ProfileOptions Convert(Dictionary<string, string> properties)
        {
            var profileOptions = new ProfileOptions();

            foreach (var property in typeof(ProfileOptions).GetProperties())
            {
                string value = null;
                if (properties.TryGetValue(nameMapping[property.Name], out value))
                {
                    property.SetValue(profileOptions, value, null);
                }
            }
            return profileOptions;
        }
    }
}
