using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsUpService.Core.Data;
using WhatsUpService.Core.Services;

namespace WhatsUpService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add the database context with SQLite provider
                    services.AddDbContext<DataDbContext>(options =>
                        options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                    // Add other services
                    services.AddScoped<UserService>();      // User management
                    services.AddScoped<MessageService>();   // Message handling
                    services.AddScoped<HashingService>();
                    services.AddScoped<ChatService>();
                    services.AddScoped<FriendsService>();
                    services.AddHostedService<TcpServerWorker>(); // Main server
                })
                .Build();



            await host.RunAsync();
        }

    }

}
