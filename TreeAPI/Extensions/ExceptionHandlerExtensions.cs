using Microsoft.AspNetCore.Diagnostics;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TreeAPI.Exceptions;

namespace TreeAPI.Extensions
{
	public static class ExceptionHandlerExtensions
	{
		public static void UseCustomExceptionHandler(this IServiceCollection services)
		{
			services.AddExceptionHandler(options =>
			{
				options.ExceptionHandler = async context =>
				{
					var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
					var exception = exceptionFeature.Error;

					var eventId = Guid.NewGuid().ToString();
					var message = exception.Message;

					var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
					logger.LogError(exception, $"An unhandled exception occurred with the event ID: {eventId}");

					if (exception is SecureException)
					{
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;
						context.Response.ContentType = "application/json";

						await context.Response.WriteAsync(
							new
							{
								type = exception.GetType().Name,
								id = eventId,
								data = new { message }
							}.ToString()
						);
					}
					else
					{
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;
						context.Response.ContentType = "application/json";

						await context.Response.WriteAsync(
							new
							{
								type = "Exception",
								id = eventId,
								data = new { message = $"Internal server error ID = {eventId}" }
							}.ToString()
						);
					}
				};
			});
		}
	}
}
