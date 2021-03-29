using BankingSolution.Api.Services;
using BankingSolution.Logic.Implementation;
using BankingSolution.Logic.Interfaces;
using BankingSolution.Logic.Poco;
using BankingSolution.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BankingSolution.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Enable Cors
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy",builder=>builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                );
            });
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddDbContext<DataContext>(opt =>
                opt.UseInMemoryDatabase(databaseName: "BankAccounts")
            );
            
            services.AddHttpClient();
            
            string rateApiUrl = Configuration.GetSection("RatesApiUrl").Value;
            string applicationBaseCurrency = Configuration.GetSection("BaseCurrency").Value;

            var apiOptions = new RatesApiOption(rateApiUrl, applicationBaseCurrency);
            
            services.AddSingleton(apiOptions);

            services.AddTransient<IRepository<Account>, AccountRepository>();
            services.AddTransient<IRepository<Transaction>, TransactionRepository>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IRatesService, RatesService>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IGenerateId, IdGenerator>();
            
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "BankingSolution.Api", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankingSolution.Api v1"));
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DataContext>();
                SeedData(context);
            }
        }
        
        private void SeedData(DataContext context)
        {
            string[] Names = new[] {"Robert C. Martin", "Martin Fowler", "Jon Skeet"};
            decimal balanceStart = 1000m;
            
            IGenerateId generateId = new IdGenerator();
            
            foreach (string name in Names)
            {
                Account account = new Account
                {
                    Id = generateId.New(),
                    Name = name, 
                    Balance = (balanceStart-=20)
                };
                
                context.Accounts.Add(account);
                context.SaveChanges();
            }
        }
    }
}