﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareAdminClientTests
{
    internal class AppendAclTests
    {
        private object nextResponse = null;
        private IFileShareApiAdminClient fileShareApiClient;
        private HttpStatusCode nextResponseStatusCode;
        private List<(HttpMethod, Uri)> lastRequestUris;
        private List<string> lastRequestBodies;
        private const int MaxBlockSize = 32;
        private FakeFssHttpClientFactory fakeHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                lastRequestUris.Add((request.Method, request.RequestUri));
                if (request.Content is StringContent content && request.Content.Headers.ContentLength.HasValue)
                    lastRequestBodies.Add(content.ReadAsStringAsync().Result);
                else
                    lastRequestBodies.Add(null);

                return (nextResponseStatusCode, nextResponse);
            });
            nextResponse = null;
            nextResponseStatusCode = HttpStatusCode.NoContent;
            lastRequestUris = new List<(HttpMethod, Uri)>();
            lastRequestBodies = new List<string>();

            var config = new
            {
                BaseAddress = @"https://fss-tests.net",
                AccessToken = DUMMY_ACCESS_TOKEN
            };

            fileShareApiClient =
                new FileShareApiAdminClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken,
                    MaxBlockSize);
        }

        [Test]
        public async Task TestAppendAcl()
        {
            var batchId = Guid.NewGuid().ToString();
            var acl = new Acl
            {
                ReadGroups=new List<string> { "AppendAclTest"},
                ReadUsers = new List<string> { "public" }
            };

            await fileShareApiClient.AppendAclAsync(batchId, acl, CancellationToken.None);

            CollectionAssert.AreEqual(new[]
            {
                $"POST:/batch/{batchId}/acl"
            },
            lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));

            var AppendAcl = lastRequestBodies.First().DeserialiseJson<Acl>();
            CollectionAssert.AreEqual(acl.ReadGroups, AppendAcl.ReadGroups);
            CollectionAssert.AreEqual(acl.ReadUsers, AppendAcl.ReadUsers);
        }
    }
}
