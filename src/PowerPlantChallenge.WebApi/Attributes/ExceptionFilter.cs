using System;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerPlantChallenge.WebApi.Exceptions;

namespace PowerPlantChallenge.WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionFilter(ILogger<ExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public override void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                ArgumentNullException => HttpStatusCode.BadRequest,
                ImpossibleToSupplyException => HttpStatusCode.UnprocessableEntity,
                _ => HttpStatusCode.InternalServerError
            };

            var url = context.HttpContext.Request.Path;
            _logger.LogError(context.Exception, "An error occured on {0}", url);

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int) statusCode;

            context.Result = new JsonResult(new
            {
                error = context.Exception.Message,
                stackTrace = _env.IsDevelopment() ? context.Exception.StackTrace : null
            });
        }
    }
}