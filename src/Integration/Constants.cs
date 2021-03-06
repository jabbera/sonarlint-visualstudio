//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

namespace SonarLint.VisualStudio.Integration
{
    public static class Constants
    {
        /// <summary>
        /// The property key which corresponds to the code analysis rule set
        /// </summary>
        public const string CodeAnalysisRuleSetPropertyKey = "CodeAnalysisRuleSet";

        /// <summary>
        /// The directory name of the SonarQube specific files that are being created
        /// </summary>
        public const string SonarQubeManagedFolderName = "SonarQube";

        /// <summary>
        /// The generated rule set name
        /// </summary>
        public const string RuleSetName = "SonarQube";

        /// <summary>
        /// The property key which corresponds to the Roslyn analyzer additional files
        /// </summary>
        public const string AdditionalFilesItemTypeName = "AdditionalFiles";

        /// <summary>
        /// The SonarQube home page
        /// </summary>
        public const string SonarQubeHomeWebUrl = "http://sonarqube.org";

        /// <summary>
        /// The property key which correspsonds to the ItemType of a <see cref="EnvDTE.ProjectItem"/>.
        /// </summary>
        public const string ItemTypePropertyKey = "ItemType";
    }
}
