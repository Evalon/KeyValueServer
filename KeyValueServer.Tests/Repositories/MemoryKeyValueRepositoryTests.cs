using KeyValueServer.Repositories;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using KeyValueServer.Repositories.Exceptions;
using System.Threading.Tasks;


namespace KeyValueServer.UnitTests.Repositories
{
    /// <summary>
    /// Repository for InMemory storing KeyValue pairs
    /// </summary>
    public class MemoryKeyValueRepositoryTests
    {
        /// <summary>
        /// Method that return deterministic KeyValue pair collection
        /// <para name="ElementsCount">Size of collection that will be created</para>
        /// </summary>
        public static Dictionary<string, string> GetKeyValues(int elementsCount)
        {
            var dic = new Dictionary<string, string>();
            for (var i = 0; i < elementsCount; i++)
            {
                dic.Add(i.ToString(), i.ToString());
            }
            return dic;
        }
        /// <summary>
        /// Group of single threaded tests
        /// </summary>
        public class SingleThreadedTests
        {
            /// <summary>
            /// Check if store can be instanated appropriately
            /// </summary>
            [Fact]
            public void CreateStore()
            {
                Assert.IsType(typeof(MemoryKeyValueRepository), new MemoryKeyValueRepository());
            }

            /// <summary>
            /// Check if repository can set and get created values
            /// </summary>
            [Theory]
            [InlineData(1)]
            [InlineData(20)]
            public void AddKeyValues_GetIt(int elementsToAdd)
            {
                var store = new MemoryKeyValueRepository();
                foreach (var kv in GetKeyValues(elementsToAdd))
                {
                    store.SetValue(kv.Key, kv.Value);
                    Assert.Equal(store.GetValue(kv.Key), kv.Value);
                }
            }
            /// <summary>
            /// Check if repository can set and delete created KeyValue.
            /// Check that delete work as expected and don't delete any other KeyValues. 
            /// (Expected Throw on nonexistent key)
            /// </summary>
            [Theory]
            [InlineData(1)]
            [InlineData(20)]
            public void AddKeyValues_DeleteHalf(int elementsToAdd)
            {
                var store = new MemoryKeyValueRepository();
                var i = 0;
                foreach (var kv in GetKeyValues(elementsToAdd))
                {
                    store.SetValue(kv.Key, kv.Value);
                    if(i % 2 == 0)
                    {
                        store.DeleteValue(kv.Key);
                        Assert.Throws<KeyNotFoundInRepositoryException>(() => store.GetValue(kv.Key));
                    }
                    i += 1;
                }
            }
            /// <summary>
            /// Delete nonexistent KeyValue should throw 
            /// <exception cref="KeyNotFoundInRepositoryException">KeyNotFoundInRepositoryException</exception>
            /// </summary>
            [Fact]
            public void DeleteNonexistentKey_Throws()
            {
                var store = new MemoryKeyValueRepository();
                Assert.Throws(typeof(KeyNotFoundInRepositoryException), () => store.DeleteValue("NotExistKeyValueThrows"));
            }
            /// <summary>
            /// Get nonexistent KeyValue should throw 
            /// <exception cref="KeyNotFoundInRepositoryException">KeyNotFoundInRepositoryException</exception>
            /// </summary>
            [Fact]
            public void GetNonexistentKey_Throws()
            {
                var store = new MemoryKeyValueRepository();
                Assert.Throws(typeof(KeyNotFoundInRepositoryException), () => store.GetValue("NotExistKeyValueThrows"));
            }
            /// <summary>
            /// Delete nonexistent KeyValue should throw 
            /// <exception cref="KeyNotFoundInRepositoryException">KeyNotFoundInRepositoryException</exception>
            /// </summary>
            [Fact]
            public void UpdateNonexistentKey_Throws()
            {
                var store = new MemoryKeyValueRepository();
                Assert.Throws(typeof(KeyNotFoundInRepositoryException), () => store.UpdateValue("NotExistKeyValueThrows", "NotExistKeyValueThrows"));
            }
            /// <summary>
            /// Trying to get all keys from empty repository should return empty collection
            /// </summary>
            [Fact]
            public void GetEmptyCollectionKeys_WhenEmpty()
            {
                var store = new MemoryKeyValueRepository();
                Assert.Empty(store.GetAllKeys());
            }
        }
        /// <summary>
        /// Group of multithreaded tests.
        /// In this test checked only dicitonary level parallelism not row level.
        /// </summary>
        public class MultiThreadedTests
        {
            /// <summary>
            /// Generate sequence of actions that set then get KeyValue in provided repository.
            /// </summary>
            private static Action[] GenerateTestSeqSetRead(int elementsCount, IKeyValueRepository store)
            {
                var actions = new Action[elementsCount];
                for (var i = 0; i < elementsCount; i++)
                {
                    var indx = i;
                    actions[i] = () => {
                        store.SetValue(indx.ToString(), indx.ToString());
                        Assert.Equal(store.GetValue(indx.ToString()), indx.ToString());
                    };
                }
                    
                return actions;
            }
            /// <summary>
            /// Generate sequence of actions that set then update then get KeyValue in provided repository.
            /// </summary>
            private static Action[] GenerateTestSeqSetUpdate(int elementsCount, MemoryKeyValueRepository store)
            {
                var actions = new Action[elementsCount];
                for (var i = 0; i < elementsCount; i++)
                {
                    var indx = i;
                    actions[i] = () =>
                    {
                        store.SetValue(indx.ToString(), indx.ToString());
                        store.UpdateValue(indx.ToString(), (indx + 15).ToString());
                        Assert.Equal(store.GetValue(indx.ToString()), (indx + 15).ToString());
                    };
                    
                }                    
                return actions;
            }
            /// <summary>
            /// Generate sequence of actions that set then delete then expected throw on attempt to get KeyValue in provided repository.
            /// </summary>
            private static Action[] GenerateTestSeqSetDelete(int elementsCount, IKeyValueRepository store)
            {
                var actions = new Action[elementsCount];
                for (var i = 0; i < elementsCount; i++)
                {
                    var indx = i;
                    actions[i] = () => {
                        store.SetValue(indx.ToString(), indx.ToString());
                        store.DeleteValue(indx.ToString());
                        Assert.Throws(typeof(KeyNotFoundInRepositoryException), () => store.GetValue(indx.ToString()));
                    };
                }

                return actions;
            }

            /// <summary>
            /// Check if repository can set -> get created KeyValue in parallel.
            /// </summary>
            [Theory]
            [InlineData(2)]
            [InlineData(20)]
            public void AddKeyValues_GetIt(int elementsToAdd)
            {
                var store = new MemoryKeyValueRepository();
                Parallel.Invoke(GenerateTestSeqSetRead(elementsToAdd, store));
            }
            /// <summary>
            /// Check if repository can set -> update -> get KeyValue in parallel.
            /// </summary>
            [Theory]
            [InlineData(2)]
            [InlineData(20)]
            public void AddKeyValues_UpdateIt(int elementsToAdd)
            {
                var store = new MemoryKeyValueRepository();
                Parallel.Invoke(GenerateTestSeqSetUpdate(elementsToAdd, store));
            }
            /// <summary>
            /// Check if repository can set -> delete -> throw on get KeyValue in parallel.
            /// </summary>
            [Theory]
            [InlineData(2)]
            [InlineData(20)]
            public void AddKeyValues_DeleteIt(int elementsToAdd)
            {
                var store = new MemoryKeyValueRepository();
                Parallel.Invoke(GenerateTestSeqSetDelete(elementsToAdd, store));
            }
        }
    }
}
