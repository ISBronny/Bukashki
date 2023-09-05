using System.Numerics;

namespace Bukashki.Domain.Models;

public class Resource : IDestination
{
	public string Id { get; init; }
	public Vector2 Point { get; init; }
}