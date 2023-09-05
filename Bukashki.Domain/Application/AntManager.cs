using System.Drawing;

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