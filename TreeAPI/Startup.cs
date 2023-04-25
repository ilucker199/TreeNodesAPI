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
using TreeAPI.Extensions;

namespace TreeAPI
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
				.AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

			// Configure PostgreSQL connection
			var connectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<NodesDbContext>(options => options.UseNpgsql(connectionString));

			// Register exception handling middleware
			services.UseCustomExceptionHandler();

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
