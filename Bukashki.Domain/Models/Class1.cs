using System.Drawing;
using System.Numerics;

namespace Bukashki.Domain.Models;

public class AntManager
{
	public List<Ant> Ants;
    

	public List<IDestination> Destinations = new List<IDestination>()
	{
		new Home()
		{
			Point = new(200, 250)
		},
		new Home()
		{
			Point = new(1000, 400)
		},
		new Home()
		{
			Point = new(1200, 1000),
		},
		new Resource()
		{
			Point = new(600, 200)
		},
		new Resource()
		{
			Point = new(400, 800)
		},
		new Resource()
		{
			Point = new(1200, 200),
		},
	};


	public AntManager(Rectangle bounds)
	{
		var antMovement = new AntMovementStrategy(bounds);
		Ants = Enumerable.Range(0, 1000).Select(i => new Ant(
			antMovement
		)
		{
			Point = new(bounds.Right - Random.Shared.NextSingle() * bounds.Width,
				bounds.Bottom - Random.Shared.NextSingle() * bounds.Height),
			Destination = Destinations[Random.Shared.Next(Destinations.Count)],
			OnDestinate = ant =>
			{
				ant.Destination = Destinations[Random.Shared.Next(Destinations.Count)];
				ant.Direction = -ant.Direction;
			},
			Counter = Destinations.ToDictionary(x=>x, x=>0),
			OnCry = m =>
			{
				foreach (var childAnt in Ants.AsParallel()
					         .Where(a => (a.Point - m.Sender.Point).Length() <= m.Sender.CryDistance))
				{
					childAnt.Listen(m);
				}

				m.Sender.ReceivedMessage = null;
			}
		}).ToList();
	}

	public void Step()
	{
		foreach (var ant in Ants)
		{
			ant.Step();
		}
	}
}

public class Ant
{
	public Vector2 Point;
	public IDestination Destination { get; set; }
	public Dictionary<IDestination, int> Counter { get; set; }
	
	public Vector2 Direction = Vector2.Normalize(new Vector2(Random.Shared.NextSingle() - 0.5f, Random.Shared.NextSingle() - 0.5f));

	public int CryDistance { get; set; } = 100;

	public Action<Ant> OnDestinate { get; init; }
	public Action<(Ant Sender, IDestination Destination)> OnCry { get; init; }
	
	
	//For graphics
	public (Ant Sender, IDestination Destination)? ReceivedMessage = null;

	private Action? OnNextMove;

	private IMovementStrategy _movementStrategy;

	public Ant(IMovementStrategy movementStrategy)
	{
		_movementStrategy = movementStrategy;
	}


	public void Step()
	{
		if (OnNextMove is not null)
		{
			OnNextMove();
			OnNextMove = null;
		}
		if(ReceivedMessage.HasValue)
		{
			var message = (this, ReceivedMessage.Value.Destination);
			OnNextMove = () => OnCry(message);
		}

		foreach (var dest in Counter.Keys)
		{
			if ((Point - dest.Point).Length() < 20)
			{
				Counter[dest] = 0;
				OnCry((this, Destination));
				if(Destination == dest)
					OnDestinate.Invoke(this);
			}
		}
		
        
		foreach (var key in Counter.Keys) Counter[key]++;

		_movementStrategy.Move(ref Point, ref Direction);
	}

	public void Listen((Ant Sender, IDestination Destination) message)
	{
		var (sender, destination) = message;
		if (Counter[destination] > sender.Counter[destination] + sender.CryDistance)
		{
			Counter[destination] = sender.Counter[destination] + sender.CryDistance;
			if(destination == Destination)
				Direction =  Vector2.Normalize(sender.Point - Point);
            ReceivedMessage = message;
		}

	}
}

public interface IMovementStrategy
{
	public void Move(ref Vector2 point, ref Vector2 direction);
}

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

public interface IDestination
{
	public string Id { get; }
	public Vector2 Point { get; }
}

public class Home : IDestination
{
	public string Id { get; init; }
	public Vector2 Point { get; init; }
}

public class Resource : IDestination
{
	public string Id { get; init; }
	public Vector2 Point { get; init; }
}
