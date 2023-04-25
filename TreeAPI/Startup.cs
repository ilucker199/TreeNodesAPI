using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TreeAPI.Context;
using TreeAPI.Exceptions;

namespace YourAppName
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers()
					.AddNewtonsoftJson();

			// Configure PostgreSQL connection
			var connectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<NodesDbContext>(options => options.UseNpgsql(connectionString));

			// Register exception handling middleware
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

			// Register Swagger generator
			services.AddSwaggerGen();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			// Use exception handling middleware
			app.UseExceptionHandler();

			// Use Swagger middleware
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "YourAppName V1");
			});

			// Apply database migrations
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetRequiredService<NodesDbContext>();
				context.Database.Migrate();
			}
		}
	}
}
