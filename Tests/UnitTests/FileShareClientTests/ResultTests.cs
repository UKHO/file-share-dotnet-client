using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests
{
    [TestFixture]
    public class ResultTests
    {
        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithStreamData_Success(HttpStatusCode statusCode)
        {
            var stream = new MemoryStream();
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StreamContent(stream)
            };

            var result = await Result.WithStreamData(response);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data, Is.EqualTo(stream));
                Assert.That(result.StatusCode, Is.EqualTo((int)statusCode));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Has.Count.EqualTo(0));
            });
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithObjectData_Success(HttpStatusCode statusCode)
        {
            var content = new List<string> { "One", "Two" };
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            var result = await Result.WithObjectData<List<string>>(response);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data[0], Is.EqualTo(content[0]));
                Assert.That(result.Data[1], Is.EqualTo(content[1]));
                Assert.That(result.StatusCode, Is.EqualTo((int)statusCode));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Has.Count.EqualTo(0));
            });
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.PartialContent)]
        public async Task WithNullData_Success(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("this content is ignored")
            };

            var result = await Result.WithNullData<string>(response);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data, Is.Null);
                Assert.That(result.StatusCode, Is.EqualTo((int)statusCode));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Has.Count.EqualTo(0));
            });
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

            var badRequest = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonConvert.SerializeObject(badRequestContent))
            };

            CommonAssertions(await Result.WithStreamData(badRequest));
            CommonAssertions(await Result.WithObjectData<List<string>>(badRequest));
            CommonAssertions(await Result.WithNullData<string>(badRequest));

            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.Data, Is.Null);
                    Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
                    Assert.That(result.IsSuccess, Is.False);
                    Assert.That(result.Errors[0].Source, Is.EqualTo(badRequestContent.Errors[0].Source));
                    Assert.That(result.Errors[0].Description, Is.EqualTo(badRequestContent.Errors[0].Description));
                    Assert.That(result.Errors[1].Source, Is.EqualTo(badRequestContent.Errors[1].Source));
                    Assert.That(result.Errors[1].Description, Is.EqualTo(badRequestContent.Errors[1].Description));
                });
            }
        }

        [Test]
        public async Task NonSuccess_BlankContent()
        {
            var internalServerError = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = null
            };

            CommonAssertions(await Result.WithStreamData(internalServerError));
            CommonAssertions(await Result.WithObjectData<List<string>>(internalServerError));
            CommonAssertions(await Result.WithNullData<Uri>(internalServerError));

            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.Data, Is.Null);
                    Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
                    Assert.That(result.IsSuccess, Is.False);
                    Assert.That(result.Errors, Is.Not.Null);
                });
                Assert.That(result.Errors, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public async Task NonSuccess_UnparseableContent()
        {
            var errorContent = "<root><joe/></root>";
            var badRequest = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorContent)
            };

            CommonAssertions(await Result.WithStreamData(badRequest));
            CommonAssertions(await Result.WithObjectData<List<string>>(badRequest));
            CommonAssertions(await Result.WithNullData<Uri>(badRequest));

            void CommonAssertions<T>(IResult<T> result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.Data, Is.Null);
                    Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
                    Assert.That(result.IsSuccess, Is.False);
                    Assert.That(result.Errors[0].Description, Is.EqualTo(errorContent));
                });
            }
        }
    }
}
