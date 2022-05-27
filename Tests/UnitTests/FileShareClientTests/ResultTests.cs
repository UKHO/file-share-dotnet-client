using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class ResultTests
    {
        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithStreamData_Success(HttpStatusCode statusCode)
        {
            var stream = new MemoryStream();
            var response = new HttpResponseMessage { StatusCode = statusCode };
            response.Content = new StreamContent(stream);

            var result = await Result.WithStreamData(response);

            Assert.AreEqual(result.Data, stream);
            Assert.AreEqual(result.StatusCode, (int)statusCode);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Errors);
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithObjectData_Success(HttpStatusCode statusCode)
        {
            var content = new List<string>{ "One", "Two" };
                        
            var response = new HttpResponseMessage { StatusCode = statusCode };
            response.Content = new StringContent(JsonConvert.SerializeObject(content));

            var result = await Result.WithObjectData<List<string>>(response);

            Assert.AreEqual(result.Data[0], content[0]);
            Assert.AreEqual(result.Data[1], content[1]);
            Assert.AreEqual(result.StatusCode, (int)statusCode);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Errors);
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithAlwaysDefaultData_Success(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage { StatusCode = statusCode };
            response.Content = new StringContent("this content is ignored");

            var result = await Result.WithAlwaysDefaultData<string>(response);

            Assert.IsNull(result.Data);
            Assert.AreEqual(result.StatusCode, (int)statusCode);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Errors);
        }


        [Test]
        public async Task NonSuccess_ErrorContent()
        {
            var badRequestContent = new ErrorResponseModel
            {
                CorrelationId = "1234",
                Errors = {
                    new Error
                    {
                        Source = "Tomato",
                        Description = "Red sauce"
                    },
                    new Error
                    {
                        Source = "Pepper",
                        Description = "Pepper sauce"
                    }
                }
            };

            var badRequest = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            badRequest.Content = new StringContent(JsonConvert.SerializeObject(badRequestContent));

            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.AreEqual(result.Data, null);
                Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.BadRequest);
                Assert.IsFalse(result.IsSuccess);
                Assert.AreEqual(result.Errors[0].Source, badRequestContent.Errors[0].Source);
                Assert.AreEqual(result.Errors[0].Description, badRequestContent.Errors[0].Description);
                Assert.AreEqual(result.Errors[1].Source, badRequestContent.Errors[1].Source);
                Assert.AreEqual(result.Errors[1].Description, badRequestContent.Errors[1].Description);
            }

            CommonAssertions(await Result.WithStreamData(badRequest));
            CommonAssertions(await Result.WithObjectData<List<string>>(badRequest));
            CommonAssertions(await Result.WithAlwaysDefaultData<string>(badRequest));
        }

        [Test]
        public async Task NonSuccess_BlankContent()
        {
            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.IsNull(result.Data);
                Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.InternalServerError);
                Assert.IsFalse(result.IsSuccess);
                Assert.IsNotNull(result.Errors);
                Assert.IsEmpty(result.Errors);
            }

            var internalServerError = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
            internalServerError.Content = null;

            CommonAssertions(await Result.WithStreamData(internalServerError));
            CommonAssertions(await Result.WithObjectData<List<string>>(internalServerError));
            CommonAssertions(await Result.WithAlwaysDefaultData<Uri>(internalServerError));
        }

        [Test]
        public async Task NonSuccess_UnparseableContent()
        {
            var errorContent = "<root><joe/></root>";

            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.IsNull(result.Data);
                Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.BadRequest);
                Assert.IsFalse(result.IsSuccess);
                Assert.AreEqual(result.Errors[0], errorContent);
            }

            var badRequest = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            badRequest.Content = new StringContent(errorContent);

            CommonAssertions(await Result.WithStreamData(badRequest));
            CommonAssertions(await Result.WithObjectData<List<string>>(badRequest));
            CommonAssertions(await Result.WithAlwaysDefaultData<Uri>(badRequest));
        }

        

    }
}