//-----------------------------------------------------------------------
// <copyright file="SolutionBindingTests.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using SonarLint.VisualStudio.Integration.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace SonarLint.VisualStudio.Integration.UnitTests
{
    [TestClass]
    public class SolutionBindingTests
    {
        private ConfigurableServiceProvider serviceProvider;
        private ConfigurableVsGeneralOutputWindowPane outputPane;
        private ConfigurableVsProjectSystemHelper projectSystemHelper;

        private DTEMock dte;
        private ConfigurableCredentialStore store;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.serviceProvider = new ConfigurableServiceProvider();

            this.dte = new DTEMock();
            this.dte.Solution = new SolutionMock(dte, Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName, "solution.sln"));
            this.serviceProvider.RegisterService(typeof(DTE), this.dte);

            this.outputPane = new ConfigurableVsGeneralOutputWindowPane();
            this.serviceProvider.RegisterService(typeof(SVsGeneralOutputWindowPane), this.outputPane);

            this.store = new ConfigurableCredentialStore();
            this.projectSystemHelper = new ConfigurableVsProjectSystemHelper(this.serviceProvider);
            this.projectSystemHelper.SolutionItemsProject = this.dte.Solution.AddOrGetProject("Solution Items");
        }

        [TestMethod]
        public void SolutionBinding_WriteSolutionBinding_ReadSolutionBinding()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store, this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var projectKey = "MyProject Key";
            var written = new BoundSonarQubeProject(serverUri, projectKey, creds);

            // Act (write)
            string output = testSubject.WriteSolutionBinding(written);
            Assert.IsNotNull(output, "Expected a real file");
            this.TestContext.AddResultFile(output);
            Assert.IsTrue(File.Exists(output), "Expected a real file");

            // Verify
            this.store.AssertHasCredentials(serverUri);

            // Act (read)
            BoundSonarQubeProject read = testSubject.ReadSolutionBinding();

            // Verify
            var newCreds = read.Credentials as BasicAuthCredentials;
            Assert.AreNotEqual(creds, newCreds, "Different credential instance were expected");
            Assert.AreEqual(creds.UserName, newCreds.UserName);
            Assert.AreEqual(creds.Password.ConvertToUnsecureString(), newCreds.Password.ConvertToUnsecureString());
            Assert.AreEqual(written.ServerUri, read.ServerUri);
            this.outputPane.AssertOutputStrings(0);
        }

        [TestMethod]
        public void SolutionBinding_WriteSolutionBinding_ReadSolutionBinding_OnRealStore()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, projectSystemHelper: this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var projectKey = "MyProject Key";
            testSubject.Store.DeleteCredentials(serverUri);

            // Case 1: has credentials
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var written = new BoundSonarQubeProject(serverUri, projectKey, creds);

            // Act (write + read)
            BoundSonarQubeProject read = null;
            try
            {
                testSubject.WriteSolutionBinding(written);
                read = testSubject.ReadSolutionBinding();
            }
            finally
            {
                testSubject.Store.DeleteCredentials(serverUri);
            }

            // Verify
            var newCreds = read.Credentials as BasicAuthCredentials;
            Assert.AreNotEqual(creds, newCreds, "Different credential instance were expected");
            Assert.AreEqual(creds.UserName, newCreds.UserName);
            Assert.AreEqual(creds.Password.ConvertToUnsecureString(), newCreds?.Password.ConvertToUnsecureString());
            Assert.AreEqual(written.ServerUri, read.ServerUri);
            this.outputPane.AssertOutputStrings(0);

            // Case 2: has not credentials (anonymous)
            creds = null;
            written = new BoundSonarQubeProject(serverUri, projectKey, creds);

            // Act (write + read)
            read = null;
            try
            {
                testSubject.WriteSolutionBinding(written);
                read = testSubject.ReadSolutionBinding();
            }
            finally
            {
                testSubject.Store.DeleteCredentials(serverUri);
            }

            // Verify
            Assert.IsNull(read.Credentials);
            Assert.AreEqual(written.ServerUri, read.ServerUri);
            this.outputPane.AssertOutputStrings(0);
        }

        [TestMethod]
        public void SolutionBinding_ReadSolutionBinding_InvalidData()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store, this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var projectKey = "MyProject Key";
            var written = new BoundSonarQubeProject(serverUri, projectKey, creds);
            string output = testSubject.WriteSolutionBinding(written);
            Assert.IsNotNull(output, "Expected a real file");
            File.WriteAllText(output, "bla bla bla: bla");

            // Act (read)
            BoundSonarQubeProject read = testSubject.ReadSolutionBinding();

            // Verify
            Assert.IsNull(read, "Not expecting any binding information in case of error");
            this.outputPane.AssertOutputStrings(1);
        }

        [TestMethod]
        public void SolutionBinding_ReadSolutionBinding_NoFile()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store);

            // Act (read)
            BoundSonarQubeProject read = testSubject.ReadSolutionBinding();

            // Verify
            Assert.IsNull(read, "Not expecting any binding information when there is no file");
            this.outputPane.AssertOutputStrings(0);
        }

        [TestMethod]
        public void SolutionBinding_ReadSolutionBinding_IOError()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store, this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var projectKey = "MyProject Key";
            var written = new BoundSonarQubeProject(serverUri, projectKey, creds);
            string output = testSubject.WriteSolutionBinding(written);
            using (var stream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act (read)
                BoundSonarQubeProject read = testSubject.ReadSolutionBinding();

                // Verify
                Assert.IsNull(read, "Not expecting any binding information in case of error");
                this.outputPane.AssertOutputStrings(1);
            }
        }

        [TestMethod]
        public void SolutionBinding_WriteSolutionBinding_IOError()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store, this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var projectKey = "MyProject Key";
            var written = new BoundSonarQubeProject(serverUri, projectKey, creds);
            string output = testSubject.WriteSolutionBinding(written);
            using (var stream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act (write again)
                string output2 = testSubject.WriteSolutionBinding(written);

                // Verify
                Assert.IsNull(output2, "Not expecting to be able to write binding information in case of error");
                this.outputPane.AssertOutputStrings(1);
            }
        }

        [TestMethod]
        public void SolutionBinding_WriteSolutionBinding_AddConfigFileToSolutionItemsFolder()
        {
            // Setup
            var testSubject = new SolutionBinding(this.serviceProvider, this.store, this.projectSystemHelper);
            var serverUri = new Uri("http://xxx.www.zzz/yyy:9000");
            var creds = new BasicAuthCredentials("user", "pwd".ConvertToSecureString());
            var projectKey = "MyProject Key";
            var toWrite = new BoundSonarQubeProject(serverUri, projectKey, creds);
            ProjectMock solutionProject = (ProjectMock)this.projectSystemHelper.SolutionItemsProject;

            // Act
            string output = testSubject.WriteSolutionBinding(toWrite);

            // Verify
            this.store.AssertHasCredentials(serverUri);
            Assert.IsTrue(solutionProject.Files.ContainsKey(output), "File {0} was not added to project", output);

            // Act (write again)
            string output2 = testSubject.WriteSolutionBinding(toWrite);

            // Verify
            Assert.AreEqual(output, output2, "Should be the same file");
            this.store.AssertHasCredentials(serverUri);
            Assert.IsTrue(solutionProject.Files.ContainsKey(output), "File {0} should remain in the project project", output);
        }
    }
}
