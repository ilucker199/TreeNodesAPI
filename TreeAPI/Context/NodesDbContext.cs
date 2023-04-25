using Microsoft.EntityFrameworkCore;
using TreeAPI.Exceptions;
using TreeAPI.Models;

namespace TreeAPI.Context
{
	public class NodesDbContext : DbContext
	{
		public NodesDbContext(DbContextOptions<NodesDbContext> options) : base(options) { }

		public DbSet<Node> Nodes { get; set; }
		public DbSet<ExceptionLog> ExceptionLogs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Node>()
				.HasOne(n => n.Parent)
				.WithMany(n => n.Children)
				.HasForeignKey(n => n.ParentId);

			modelBuilder.Entity<Node>()
				.Property(n => n.Name)
				.HasDefaultValue("NodeName");
		}
	}
}
