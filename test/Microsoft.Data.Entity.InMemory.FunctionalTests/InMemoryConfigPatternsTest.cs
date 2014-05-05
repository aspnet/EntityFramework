// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryConfigPatternsTest
    {
        public class ImplicitServicesAndConfig
        {
            [Fact]
            public void Can_save_and_query_with_implicit_services_and_OnConfiguring()
            {
                using (var context = new BlogContext())
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public DbSet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class ImplicitServicesExplicitConfig
        {
            [Fact]
            public void Can_save_and_query_with_implicit_services_and_explicit_config()
            {
                var configuration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                using (var context = new BlogContext(configuration))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(configuration))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(ImmutableDbContextOptions configuration)
                    : base(configuration)
                {
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class ExplicitServicesImplicitConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_OnConfiguring()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                using (var context = new BlogContext(serviceProvider))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class ExplicitServicesAndConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_explicit_config()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                var configuration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                using (var context = new BlogContext(serviceProvider, configuration))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider, configuration))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                    : base(serviceProvider, configuration)
                {
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class ExplicitServicesAndNoConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_no_config()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                using (var context = new BlogContext(serviceProvider))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class NoServicesAndNoConfig
        {
            [Fact]
            public void Throws_on_attempt_to_use_context_with_no_store()
            {
                Assert.Equal(
                    GetString("FormatNoDataStoreConfigured"),
                    // TODO: Should not be AggregateException
                    Assert.Throws<AggregateException>(() =>
                        {
                            using (var context = new BlogContext())
                            {
                                context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                                context.SaveChanges();
                            }
                        }).InnerException.Message);
            }

            private class BlogContext : DbContext
            {
                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class ImplicitConfigButNoServices
        {
            [Fact]
            public void Throws_on_attempt_to_use_store_with_no_store_services()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .BuildServiceProvider();

                Assert.Equal(
                    GetString("FormatNoDataStoreService"),
                    // TODO: Should not be AggregateException
                    Assert.Throws<AggregateException>(() =>
                        {
                            using (var context = new BlogContext(serviceProvider))
                            {
                                context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                                context.SaveChanges();
                            }
                        }).InnerException.Message);
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class InjectContext
        {
            [Fact]
            public void Can_register_context_with_DI_container_and_have_it_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                    Assert.NotNull(serviceProvider);
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class InjectContextAndConfiguration
        {
            [Fact]
            public void Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
            {
                var configuration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .AddInstance<ImmutableDbContextOptions>(configuration)
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                    : base(serviceProvider, configuration)
                {
                    Assert.NotNull(serviceProvider);
                    Assert.NotNull(configuration);
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class InjectConfiguration
        {
            // This one is a bit strange because the context gets the configuration from the service provider
            // but doesn't get the service provider and so creates a new one for use internally. This works fine
            // although it would be much more common to inject both when using DI explicitly.
            [Fact]
            public void Can_register_configuration_with_DI_container_and_have_it_injected()
            {
                var configuration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .AddInstance<ImmutableDbContextOptions>(configuration)
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(ImmutableDbContextOptions configuration)
                    : base(configuration)
                {
                    Assert.NotNull(configuration);
                }

                public DbSet<Blog> Blogs { get; set; }
            }
        }

        public class InjectDifferentConfigurations
        {
            [Fact]
            public void Can_inject_different_configurations_into_different_contexts()
            {
                var blogCofiguration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration(() => new BlogConfiguration());

                var accountCofiguration = new DbContextOptions()
                    .UseInMemoryStore()
                    .BuildConfiguration(() => new AccountConfiguration());

                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<AccountContext, AccountContext>()
                    .AddTransient<MyBlogController, MyBlogController>()
                    .AddTransient<MyAccountController, MyAccountController>()
                    .AddInstance<BlogConfiguration>(blogCofiguration)
                    .AddInstance<AccountConfiguration>(accountCofiguration)
                    .BuildServiceProvider();

                serviceProvider.GetService<MyBlogController>().Test();
                serviceProvider.GetService<MyAccountController>().Test();
            }

            private class BlogConfiguration : ImmutableDbContextOptions
            {
            }

            private class AccountConfiguration : ImmutableDbContextOptions
            {
            }

            private class MyBlogController
            {
                private readonly BlogContext _context;

                public MyBlogController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class MyAccountController
            {
                private readonly AccountContext _context;

                public MyAccountController(AccountContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Accounts.Add(new Account { Id = 1, Name = "Eeky Bear" });
                    _context.SaveChanges();

                    var account = _context.Accounts.SingleOrDefault();

                    Assert.Equal(1, account.Id);
                    Assert.Equal("Eeky Bear", account.Name);
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext(IServiceProvider serviceProvider, BlogConfiguration configuration)
                    : base(serviceProvider, configuration)
                {
                    Assert.NotNull(serviceProvider);
                    Assert.NotNull(configuration);
                }

                public DbSet<Blog> Blogs { get; set; }
            }

            private class AccountContext : DbContext
            {
                public AccountContext(IServiceProvider serviceProvider, AccountConfiguration configuration)
                    : base(serviceProvider, configuration)
                {
                    Assert.NotNull(serviceProvider);
                    Assert.NotNull(configuration);
                }

                public DbSet<Account> Accounts { get; set; }
            }

            private class Account
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        private class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static string GetString(string stringName)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, null);
        }
    }
}
