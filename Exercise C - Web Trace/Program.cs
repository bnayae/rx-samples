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

        #region CreateUsersStream

        private static IObservable<(int Id, string User, UserAction Action)> CreateUsersStream()
        {
            return Observable.Merge(
                        CreateUsrStream(0, "Yossi"),
                        CreateUsrStream(1, "Dina"),
                        CreateUsrStream(2, "Lorri"),
                        CreateUsrStream(3, "Alex"),
                        CreateUsrStream(4, "Shai")
                    );
        }

        #endregion // CreateUsersStream

        #region CreateUsrStream

        private static IObservable<(int Id, string User, UserAction Action)> CreateUsrStream(
                                            int id, string user)
        {
            return Observable.Create<(int Id, string User, UserAction Action)>(
                        async (consumer, ct) =>
                        {
                            while (true)
                            {
                                await Task.Delay(rnd.Next(50, 3000)).ConfigureAwait(false);
                                UserAction action = (UserAction)(Environment.TickCount % COUNT);
                                consumer.OnNext((id, user, action));
                            }
                        });
        }

        #endregion // CreateUsrStream
    }
}
