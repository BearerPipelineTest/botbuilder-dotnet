﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Tests.Common.Storage;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Azure.Blobs.Tests
{
    public class BlobsStorageTests : BlobStorageBaseTests, IAsyncLifetime
    {
        private readonly string _testName;

        public BlobsStorageTests(ITestOutputHelper testOutputHelper)
        {
            var helper = (TestOutputHelper)testOutputHelper;

            var test = (ITest)helper.GetType().GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(helper);

            _testName = test.TestCase.TestMethod.Method.Name;

            if (StorageEmulatorHelper.CheckEmulator())
            {
                new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        protected override string ContainerName => $"blobs{_testName.ToLower().Replace("_", string.Empty)}";

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public void BlobStorageParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, null));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(string.Empty, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, string.Empty));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateObjectTestAsJObjects_TypeNameHandlingNone(bool typeNameHandlingNone)
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await UpdateObjectTest_AsJObjects(GetStorage(typeNameHandlingNone));
            }
        }

        [Fact]
        public async Task TestTypedObjects_TypeNameHandling_All()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await TestTypedObjects(GetStorage(false), expectTyped: true);
            }
        }

        [Fact]
        public async Task TestTypedObjects_TypeNameHandling_None()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await TestTypedObjects(GetStorage(true), expectTyped: false);
            }
        }

        protected override IStorage GetStorage(bool typeNameHandlingNone = false)
        {
            if (typeNameHandlingNone)
            {
                var options = new BlobsStorageOptions(ConnectionString, ContainerName);

                return new BlobsStorage(options);
            }
            else 
            {
                var options = new BlobsStorageOptions(ConnectionString, ContainerName, new JsonSerializer() { TypeNameHandling = TypeNameHandling.All });
                
                return new BlobsStorage(options);
            }
        }
    }
}
