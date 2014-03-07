using System;
using System.Net;
using Topshelf;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<AppHost>(s =>
                {
                    s.ConstructUsing(name => new AppHost());
                    s.WhenStarted(sh => sh.Start());
                    s.WhenStopped(sh => sh.Stop());

                });

                x.RunAsLocalSystem();

                x.SetDescription("Provides core services for dust.");
                x.SetDisplayName("Dust Core Services");
                x.SetServiceName("DustCoreService");

                // Apparently service recover never worked, might roll back to old version out of paranoia
            });
        }
    }
}
