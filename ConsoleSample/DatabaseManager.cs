using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;

namespace ConsoleSample
{
    internal class DatabaseManager
    {
        readonly Dictionary<string, IDocumentStore> _documentStores = new Dictionary<string, IDocumentStore>();
        private readonly object _lock = new object();

        public IDocumentStore GetDocumentStore(string databaseName)
        {
            if (!_documentStores.ContainsKey(databaseName))
            {
                lock (_lock)
                {
                    if (!_documentStores.ContainsKey(databaseName))
                    {
                        var documentStore = new DocumentStore
                            {
                                Url = "http://localhost:8080",
                                DefaultDatabase = databaseName
                            };
                        documentStore.Initialize();

                        _documentStores.Add(databaseName, documentStore);
                    }
                }
            }
            return _documentStores[databaseName];
        }
    }
}