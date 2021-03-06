using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Cassandra;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace NoSqlStress.WebApi
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
            services.AddControllers();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NoSqlStress.WebApi", Version = "v1" });
            });

            services.AddSingleton<ICluster>(x =>
            {
                return Cluster.Builder()
                    .AddContactPoints(Configuration.GetConnectionString("CassandraConnection").Split(','))
                    .WithQueryTimeout(10000)
                    .WithQueryOptions(new QueryOptions().SetConsistencyLevel(ConsistencyLevel.One))
                    .Build();
            });

            services.AddSingleton<MongoClientBase>(x =>
            {
                BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                return new MongoClient(Configuration.GetConnectionString("MongoConnection"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NoSqlStress.WebApi v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
