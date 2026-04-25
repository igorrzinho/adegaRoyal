using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace KeycloakAuth.Entities;

public class TaskItem	
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Title { get; set; } = string.Empty;
	public bool IsCompleted { get; set; } = false;
	public DateTime CreatedAt { get; set; } 

	public Guid UserId { get; set; }
	public User? User { get; set; }
}