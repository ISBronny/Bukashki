using System.Drawing;
using System.Numerics;

namespace Bukashki.Domain.Models;

public class AntMovementStrategy : IMovementStrategy
{
	private Rectangle Bounds { get; }
	private float Speed { get; set; } = 1.0f;

	public AntMovementStrategy(Rectangle bounds)
	{
		Bounds = bounds;
	}
	public void Move(ref Vector2 point, ref Vector2 direction)
	{
		if (point.X + Speed * direction.X > Bounds.Right)
			direction.X = -Math.Abs(direction.X);
		if (point.X + Speed * direction.X < Bounds.Left)
			direction.X = Math.Abs(direction.X);
		
		if (point.Y + Speed * direction.Y < Bounds.Top)
			direction.Y = Math.Abs(direction.Y);
		if (point.Y + Speed * direction.Y > Bounds.Bottom)
			direction.Y = -Math.Abs(direction.Y);

		point += Speed * direction;
	}
}