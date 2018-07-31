using System;
using System.Collections.Concurrent;
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

		private const string HELLO_REACTIVE_EXTENSION = ".... . .-.. .-.. ---  .-. . .- -.-. - .. ...- .  . -..- - . -. ... .. --- -. ...";
		#region ConcurrentDictionary<string, char> _map = ...

		private static ConcurrentDictionary<string, char> _map = new ConcurrentDictionary<string, char>
		{
			[".-"] =	'A',
			["-..."] =	'B',
			["-.-."] =	'C',
			["-.."] =	'D',
			["."] =		'E',
			["..-."] =	'F',
			["--."] =	'G',
			["...."] =	'H',
			[".."] =	'I',
			[".---"] =	'J',
			["-.-"] =	'K',
			[".-.."] =	'L',
			["--"] =    'M',
			["-."] =    'N',
			["---"] =   'O',
			[".--."] =  'P',
			["--.-"] =  'Q',
			[".-."] =   'R',
			["..."] =   'S',
			["-"] =     'T',
			["..-"] =   'U',
			["...-"] =  'V',
			[".--"] =   'W',
			["-..-"] =  'X',
			["-.--"] =  'Y',
			["--.."] =  'Z',
			[".----"] = '1',
			["..---"] = '2',
			["...--"] = '3',
			["....-"] = '4',
			["....."] = '5',
			["-...."] = '6',
			["--..."] = '7',
			["---.."] = '8',
			["----."] = '9',
			["-----"] = '0'
		};

		#endregion // ConcurrentDictionary<string, char> _map = ...

		private static readonly object _gate = new object();

		static void Main(string[] args)
		{
			Console.CursorVisible = false;
            IObservable<char> morse = GetProducer()
                                            .Publish()
                                            .RefCount();

            var morseOnly = morse.Where(c => c != ' ');
            var trigger = morse.Where(c => c == ' ');
            // TODO: Translation stream goes here
            var morseWords = morseOnly.Window(() => trigger);
            var transtaletd = from mw in morseWords
                              from code in mw.Scan(string.Empty, (acc, val) => acc + val)
                              select _map[code];
            transtaletd.Subscribe(m => Write(m, _textPosition, 4));
			morse.Wait();
            Console.ReadLine();
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

		//private static IObservable<char> GetProducer()
		//{
		//	var subject = new Subject<char>();
		//	Task _ = Task.Run(() =>
		//	{
		//		while (true)
		//		{
		//			char c = Console.ReadKey(true).KeyChar;
		//			subject.OnNext(c);
		//		}
		//	});
		//	return subject.Where(c => c == '-' || c == '.' || c == ' ')
		//				  .Do(c => Write(c, _morsePosition, 2))
		//				  .Do(c =>
		//				  {
		//					  _morsePosition++;
		//					  if (c == ' ')
		//						  _textPosition++;
		//				  });
		//}

		private static IObservable<char> GetProducer()
		{
			var result = Observable.Create<char>(async (consumer, cancellation) =>
			{
				foreach (var c in HELLO_REACTIVE_EXTENSION)
				{
					await Task.Delay(100).ConfigureAwait(false);
					consumer.OnNext(c);
				}
                consumer.OnCompleted();
			})
			.Where(c => c == '-' || c == '.' || c == ' ')
						  .Do(c => Write(c, _morsePosition, 2))
						  .Do(c =>
						  {
							  _morsePosition++;
							  if (c == ' ')
								  _textPosition++;
						  });
			return result;
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
						 
 */
