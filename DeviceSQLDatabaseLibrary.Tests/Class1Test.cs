// <copyright file="Class1Test.cs">Copyright ©  2017</copyright>
using System;
using DeviceSQLDatabaseLibrary;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeviceSQLDatabaseLibrary.Tests
{
    /// <summary>This class contains parameterized unit tests for Class1</summary>
    [PexClass(typeof(Class1))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class Class1Test
    {
        /// <summary>Test stub for ConnectDatabase(String, String, String)</summary>
        [PexMethod]
        public void ConnectDatabaseTest(
            [PexAssumeUnderTest]Class1 target,
            string ipaddress,
            string username,
            string password
        )
        {
            target.ConnectDatabase(ipaddress, username, password);
            // TODO: add assertions to method Class1Test.ConnectDatabaseTest(Class1, String, String, String)
        }
    }
}
