using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SwissTiming.Timing.AcquisitionSimulator;

namespace UseSimulator
{
	internal class SimpleProgram
	{
		internal void Run()
		{
			Simulator s = new Simulator(StartKind.MassStart);
			s.Initialize(new List<Competitor> { new Competitor("Toto", 1) });
			s.RaceCompleted += s_RaceCompleted;
			s.Start();

			while (s.IsRunning) { }

			Console.WriteLine("Simulation is over.");
			Console.ReadLine();
		}

		private void s_RaceCompleted(object sender, RaceCompletedEventArgs e)
		{
			Console.WriteLine(e.Bib + " has completed the race.");
		}
	}
}