using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TreeAPI.Context;
using TreeAPI.Exceptions;
using TreeAPI.Models;

namespace TreeAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class NodesController : ControllerBase
	{
		private readonly NodesDbContext _context;

		public NodesController(NodesDbContext context)
		{
			_context = context;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Node>> Get(int id)
		{
			var node = await _context.Nodes.FindAsync(id);
			if (node == null)
			{
				throw new SecureException("Node not found");
			}
			return node;
		}

		[HttpPost]
		public async Task<ActionResult<Node>> Post(Node node)
		{
			_context.Nodes.Add(node);
			await _context.SaveChangesAsync();
			return node;
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, Node node)
		{
			if (id != node.Id)
			{
				throw new SecureException("Invalid node ID");
			}
			_context.Entry(node).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var node = await _context.Nodes.FindAsync(id);
			if (node == null)
			{
				throw new SecureException("Node not found");
			}
			if (node.Children.Count > 0)
			{
				throw new SecureException("You have to delete all children nodes first");
			}
			_context.Nodes.Remove(node);
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[HttpGet("exception")]
		public IActionResult TestException()
		{
			throw new Exception("Test exception");
		}

		[HttpPost("exception")]
		public IActionResult TestException([FromBody] ExceptionLog exceptionLog)
		{
			throw new Exception("Test exception");
		}

		[HttpPost("secure-exception")]
		public IActionResult TestSecureException([FromBody] ExceptionLog exceptionLog)
		{
			throw new SecureException("Test secure exception");
		}

		[HttpPost("log-exception")]
		public IActionResult LogException([FromBody] ExceptionLog exceptionLog)
		{
			try
			{
				throw new Exception("Test exception");
			}
			catch (SecureException ex)
			{
				LogException(ex, exceptionLog);
				return StatusCode(500, new { type = "Secure", id = exceptionLog.EventId, data = new { message = ex.Message } });
			}
			catch (Exception ex)
			{
				LogException(ex, exceptionLog);
				return StatusCode(500, new { type = "Exception", id = exceptionLog.EventId, data = new { message = $"Internal server error ID = {exceptionLog.EventId}" } });
			}
		}

		private void LogException(Exception ex, ExceptionLog exceptionLog)
		{
			exceptionLog.Timestamp = DateTime.UtcNow;
			exceptionLog.StackTrace = ex.StackTrace;
			_context.ExceptionLogs.Add(exceptionLog);
			_context.SaveChanges();
		}
	}
}
