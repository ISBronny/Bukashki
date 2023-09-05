using System.Numerics;

namespace Bukashki.Domain.Models;

public interface IDestination
{
	public string Id { get; }
	public Vector2 Point { get; }
}