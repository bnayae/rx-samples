using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: translate the Morse Code on the fly
// http://en.wikipedia.org/wiki/Morse_code
// http://www.youtube.com/watch?v=AQZTo73EPn8

namespace Bnaya.Samples
{
	// Consider the following operators:
	//  - Window
	//  - Buffer
	//  - Scan
	//  - Aggregate

	class Program
	{
		private static int _morsePosition = 0;
		private static int _textPosition = 0; // Write(translation, _textPosition, 4)

		private static readonly object _gate = new object();

		static void Main(string[] args)
		{
			Console.CursorVisible = false;
			IObservable<char> chars = GetProducer();

			chars.Subscribe();

			#region Wait

			while (true)
			{
				Thread.Sleep(300);
			}

			#endregion // Wait
		}

		#region Write

		private static void Write(char code, int left, int top)
		{
			Write(code.ToString(), left, top);
		}
		private static void Write(string code, int left, int top)
		{
			lock (_gate)
			{
				Console.SetCursorPosition(left, top);
				Console.Write(code);
			}
		}

		#endregion // Write

		#region GetProducer

		private static IObservable<char> GetProducer()
		{
			var subject = new Subject<char>();
			Task _ = Task.Run(() =>
			{
				while (true)
				{
					char c = Console.ReadKey(true).KeyChar;
					subject.OnNext(c);
				}
			});
			return subject.Where(c => c == '-' || c == '.' || c == ' ')
						  .Do(c => Write(c, _morsePosition, 2))
						  .Do(c =>
						  {
							  _morsePosition++;
							  if (c == ' ')
								  _textPosition++;
						  });
		}

		#endregion // GetProducer
	}
}

/*
	A	. _	 	    N	_ .	 
	B	_ . . .	 	O	_ _ _	 
	C	_ . _ .	 	P	. _ _ .	 
	D	_ . .	 	Q	_ _ . _	 
	E	.	 	    R	. _ .	 
	F	. . _ .	 	S	. . .	 
	G	_ _ .	 	T	_	 
	H	. . . .	 	U	. . _	 
	I	. .	 	    V	. . . _	 
	J	. _ _ _	 	W	. _ _	 
	K	_ . _	 	X	_ . . _	 
	L	. _ . .	 	Y	_ . _ _	 
	M	_ _	 	    Z	_ _ . .	 
						 
Numbers
	1	. _ _ _ _	 	6	_ . . . .	 
	2	. . _ _ _	 	7	_ _ . . .	 
	3	. . . _ _	 	8	_ _ _ . .	 
	4	. . . . _	 	9	_ _ _ _ .	 
	5	. . . . .	 	0	_ _ _ _ _	 
						 
Abbreviated Numbers
	1	. _	 	    6	_ . . . .	 
	2	. . _	 	7	_ . . .	 
	3	. . . _	 	8	_ . .	 
	4	. . . . _   9	_ .	 
	5   .
 */
