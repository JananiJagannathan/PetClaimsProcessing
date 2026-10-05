using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PetClaims.Api.Exceptions;
using PetClaims.Api.Middleware;

namespace PetClaims.Tests
{
    [TestFixture]
    public class GlobalExceptionMiddlewareTests
    {
        [Test]
        public async Task BusinessException_Returns400()
        {
            RequestDelegate next =
                _ => throw new BusinessException(
                    "Invalid claim.");

            var middleware =
                new GlobalExceptionMiddleware(
                    next,
                    NullLogger<GlobalExceptionMiddleware>.Instance);

            var context =
                new DefaultHttpContext();

            context.Response.Body =
                new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode
                .Should().Be(400);
        }

        [Test]
        public async Task DuplicateInvoiceException_Returns409()
        {
            RequestDelegate next =
                _ => throw new DuplicateInvoiceException(
                    "Duplicate invoice.");

            var middleware =
                new GlobalExceptionMiddleware(
                    next,
                    NullLogger<GlobalExceptionMiddleware>.Instance);

            var context =
                new DefaultHttpContext();

            context.Response.Body =
                new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode
                .Should().Be(409);
        }

        [Test]
        public async Task NotFoundException_Returns404()
        {
            RequestDelegate next =
                _ => throw new NotFoundException(
                    "Claim not found.");

            var middleware =
                new GlobalExceptionMiddleware(
                    next,
                    NullLogger<GlobalExceptionMiddleware>.Instance);

            var context =
                new DefaultHttpContext();

            context.Response.Body =
                new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode
                .Should().Be(404);
        }
    }
}