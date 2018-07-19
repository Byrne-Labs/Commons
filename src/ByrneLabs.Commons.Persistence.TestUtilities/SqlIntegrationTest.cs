﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public abstract class SqlIntegrationTest
    {
        private readonly IList<DirectoryInfo> _temporaryDatabaseDirectories = new List<DirectoryInfo>();

        protected abstract string ConnectionName { get; }

        protected abstract string EmptyTestDatabaseFilePath { get; }

        protected static void AssertValid(IEntity entity) => AssertValid(new[] { entity });

        protected IContainer GetIntegrationTestContainer()
        {
            var container = new SimpleContainerProvider(true);
            var sqlTestDatabase = new SqlTestDatabaseServer(EmptyTestDatabaseFilePath);
            sqlTestDatabase.Register(container, ConnectionName);
            /*
             * We are registering this as an instance so that it gets disposed at the same time as the container
             */
            container.RegisterInstance(sqlTestDatabase);

            return container;
        }

        protected static void AssertValid(IEnumerable<IEntity> entities, IList<IEntity> examinedEntities = null)
        {
            Assert.NotNull(entities);
            Assert.NotEmpty(entities);

            if (examinedEntities == null)
            {
                examinedEntities = new List<IEntity>();
            }

            foreach (var entity in entities.Where(e => !examinedEntities.Contains(e)))
            {
                Assert.NotNull(entity?.EntityId);

                examinedEntities.Add(entity);

                var otherEntities = new List<IEntity>();
                foreach (var property in entity.GetType().GetProperties().Where(p => p.CanRead))
                {
                    if (typeof(IEntity).IsAssignableFrom(property.PropertyType))
                    {
                        var value = (IEntity)property.GetValue(entity);
                        if (value != null)
                        {
                            otherEntities.Add(value);
                        }
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var enumerable = (IEnumerable)property.GetValue(entity);
                        if (enumerable != null)
                        {
                            otherEntities.AddRange(enumerable.OfType<IEntity>());
                        }
                    }
                }

                if (otherEntities.Any())
                {
                    AssertValid(otherEntities, examinedEntities);
                }
            }
        }
    }
}