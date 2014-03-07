using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Autofac;
using Raven.Abstractions.Data;
using Raven.Client;

namespace ConsoleSample
{
    class AppHost
    {
        private IContainer _container;

        public void Start()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<DatabaseManager>();
            containerBuilder.Register((c =>
                {
                    var manager = c.Resolve<DatabaseManager>();
                    return manager.GetDocumentStore("Dust");
                }));

            _container = containerBuilder.Build();

            var documentStore = _container.Resolve<IDocumentStore>();

            var newTenancies = documentStore.Changes().ForDocumentsStartingWith("Tenancy").SelectMany(x =>
                {
                    if (x.Type == DocumentChangeTypes.Put)
                    {
                        using (var session = documentStore.OpenSession())
                        {
                            return new[] { session.Load<Tenancy>(x.Id) };
                        }
                    }
                    return new Tenancy[0];
                });

            IObservable<Tenancy> tenancyStream;
            using (var session = documentStore.OpenSession())
            {
                var existingTenancies = session
                    .Query<Tenancy>()
                    .Take(1024)
                    .ToList()
                    .ToObservable();

                tenancyStream = existingTenancies.Concat(newTenancies);
            }

            IEnumerable<string> indexes = new[] { "OutOfDatePermissions" };
            tenancyStream.Subscribe(new NewTenancyObserver());
            var allChanges = tenancyStream.SelectMany(tenancy =>
                {
                    foreach (var index in indexes)
                    {
                        yield return documentStore.Changes().ForIndex(index);
                    }
                });
        }

        public class NewTenancyObserver : IObserver<Tenancy>
        {
            public void OnNext(Tenancy value)
            {
                Console.WriteLine("New Tenancy {0}, {1}", value.Name, value.DbName);
            }

            public void OnError(Exception error)
            {
                Console.WriteLine("Tenancy Stream Error");
            }

            public void OnCompleted()
            {
                Console.WriteLine("Tenancy Stream Complete");
            }
        }

        public void Stop()
        {
            _container.Dispose();
        }
    }
}