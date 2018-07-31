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

            var groups = users.GroupByUntil(
                                    m => (Id: m.Id, User: m.User), // key
                                    m => m.Action,                 // select
                                    g => g.Throttle(TimeSpan.FromSeconds(2.5))); // trigger
            FixCounting(groups);

            //DoubleGroupping(groups);
            Console.ReadKey();
        }

        #region FixCounting

        private static void FixCounting(IObservable<IGroupedObservable<(int Id, string User), UserAction>> groups)
        {
            var xs = from g in groups
                     let clicks = g.Where(actType => actType == UserAction.Click).Count()
                     let moves = g.Where(actType => actType == UserAction.Move).Count()
                     let views = g.Where(actType => actType == UserAction.View).Count()
                     from result in Observable.Zip(clicks, moves, views,
                                        (c, m, v) => (User: g.Key, Clicks: c, Moves: m, Views: v))
                     select result;

            xs.Subscribe(m =>
            {
                int count = m.User.Id;
                string indent = new string('\t', count);
                Console.WriteLine($"{indent}, {m}");
            });
        }

        #endregion // FixCounting

        #region DoubleGroupping

        private static void DoubleGroupping(IObservable<IGroupedObservable<(int Id, string User), UserAction>> groups)
        {
            var xs = from g in groups
                     let actions = g.GroupBy(actType => actType)      // observable per action
                                                .SelectMany(m => m.Count() // break each action observable (zipped)
                                                            .Select(c =>      // tag each observable by action type
                                                                (User: g.Key,
                                                                    Action: m.Key,
                                                                    Count: c)))
                     from result in Observable.Zip(actions)
                     select result;

            xs.Subscribe(ms =>
            {
                foreach (var m in ms)
                {
                    int count = m.User.Id;
                    string indent = new string('\t', count);
                    Console.WriteLine($"{indent}, {m.User.User} {m.Action} = {m.Count}");
                }
            });
        }

        #endregion // DoubleGroupping

        #region DoubleGrouppingLinq

        private static void DoubleGrouppingLinq(IObservable<IGroupedObservable<(int Id, string User), UserAction>> groups)
        {
            var xs = from user in groups
                     let actionTypes = from actionGroup in user.GroupBy(actType => actType)   // observable per action
                                       from item in actionGroup.Count() // break each action observable (zipped)
                                              .Select(c =>        // tag each observable by action type
                                                    (User: user.Key,
                                                     Action: actionGroup.Key,
                                                     Count: c))
                                       select item
                     from result in Observable.Zip(actionTypes)
                     select result;

            xs.Subscribe(ms =>
            {
                foreach (var m in ms)
                {
                    int count = m.User.Id;
                    string indent = new string('\t', count);
                    Console.WriteLine($"{indent}, {m.User.User} {m.Action} = {m.Count}");
                }
            });
        }

        #endregion // DoubleGrouppingLinq

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
