using System.Numerics;

namespace Bukashki.Domain.Models;

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