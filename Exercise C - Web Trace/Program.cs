using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    class Program
    {
        private static Random rnd = new Random();
        private static int COUNT = Enum.GetValues(typeof(UserAction)).Length;

        static void Main(string[] args)
        {
            var users = CreateUsersStream();

            // TODO: for each user activity 
            //       as long as he don't pause for more than 2.5 seconds.
            //       calculate the following statistic:
            //          click count
            //          move count
            //          view count
        }

        private static IObservable<(string User, UserAction Action)> CreateUsersStream()
        {
            return Observable.Merge(
                        CreateUsrStream("Yossi"),
                        CreateUsrStream("Dina"),
                        CreateUsrStream("Lorri"),
                        CreateUsrStream("Alex"),
                        CreateUsrStream("Shai")
                    );
        }

        private static IObservable<(string User, UserAction Action)> CreateUsrStream(
                                            string user)
        {
            return Observable.Create<(string User, UserAction Action)>(
                        async (consumer, ct) =>
                        {
                            await Task.Delay(rnd.Next(50, 3000)).ConfigureAwait(false);
                            UserAction action = (UserAction)(Environment.TickCount % COUNT);
                            consumer.OnNext((user, action));
                        });
        }
    }
}
