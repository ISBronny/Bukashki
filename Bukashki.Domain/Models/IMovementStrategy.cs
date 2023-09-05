using System.Numerics;

namespace Bukashki.Domain.Models;

public interface IMovementStrategy
{
	public void Move(ref Vector2 point, ref Vector2 direction);
}