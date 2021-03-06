//-----------------------------------------------------------------------
// <copyright file="QualityProfile.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using System.Diagnostics;

namespace SonarLint.VisualStudio.Integration.Service.DataModel
{
    [DebuggerDisplay("Name: {Name}, Key: {Key}, Language: {Language}, IsDefault: {IsDefault}")]
    internal class QualityProfile
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("default")]
        public bool IsDefault { get; set; }
    }
}
