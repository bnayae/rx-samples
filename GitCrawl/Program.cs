using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit.Internal;
using Octokit.Reactive;

namespace GitCrawl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var cred = Environment.GetEnvironmentVariable("GitCred");
            var client = new ObservableGitHubClient(
                            new Octokit.ProductHeaderValue("bnayae"),
                            new InMemoryCredentialStore(
                                new Octokit.Credentials(cred)));
            client.User.Followers.GetAllFollowing("bnayae").Subscribe(v => Console.WriteLine(v.Login));

            Console.ReadKey();
        }
    }
}
