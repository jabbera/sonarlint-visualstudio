//-----------------------------------------------------------------------
// <copyright file="TestableConnectSectionController.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using SonarLint.VisualStudio.Integration.Connection;
using SonarLint.VisualStudio.Integration.Service;
using SonarLint.VisualStudio.Integration.State;
using SonarLint.VisualStudio.Integration.TeamExplorer;
using System;
using System.Windows.Threading;

namespace SonarLint.VisualStudio.Integration.UnitTests.TeamExplorer
{
    internal class TestableConnectSectionController : ConnectSectionController
    {
        public TestableConnectSectionController(IServiceProvider serviceProvider,
                                               ISonarQubeServiceWrapper sonarQubeService)
            : this(serviceProvider, new TransferableVisualState(), sonarQubeService, new ConfigurableActiveSolutionTracker(), new ConfigurableWebBrowser())
        {
        }

        public TestableConnectSectionController(IServiceProvider serviceProvider,
                                                TransferableVisualState state,
                                                ISonarQubeServiceWrapper sonarQubeService,
                                                IActiveSolutionTracker tracker,
                                                IWebBrowser webBrowser)
            : base(serviceProvider, state, sonarQubeService, tracker, webBrowser, Dispatcher.CurrentDispatcher)
        {
        }

        public Action SetProjectsAction { get; set; }

        public ConfigurableUserNotification NotificationOverride
        {
            get;
        } = new ConfigurableUserNotification();

        #region Overrides
        protected override IUserNotification Notification
        {
            get
            {
                return this.NotificationOverride;
            }
        }

        protected override void SetProjects(object sender, ConnectedProjectsEventArgs args)
        {
            if (this.SetProjectsAction != null)
            {
                this.SetProjectsAction();
                return;
            }

            // This call will issue an async request
            base.SetProjects(sender, args);

            // This call will make sure that that async request is executed now 
            DispatcherHelper.DispatchFrame();
        }
        #endregion

        #region Test accessors
        public void InvokeSetProjects(ConnectedProjectsEventArgs args)
        {
            this.SetProjects(null, args);
        }

        public IUserNotification NotificationAccessor
        {
            get
            {
                return this.Notification;
            }
        }
        #endregion
    }
}
