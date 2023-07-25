using APISirene.Domain.Interfaces.InterfaceRepository;
using APISirene.Domain.Interfaces.InterfaceService;
using APISirene.Domain.Services;
using APISirene.Infrastructure.Data;
using APISirene.Infrastructure.Repository;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OfficeOpenXml;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // R�cup�ration de la chaine de connexion MongoDB et du nom de la base de donn�es depuis la configuration
        var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:MongoDb");
        var databaseName = builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName");

        // Cr�ation d'une instance de MongoClient en utilisant la chaine de connexion
        var client = new MongoClient(connectionString);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // R�cup�ration de la base de donn�es MongoDB
        var database = client.GetDatabase(databaseName);

        // Ajout de la base de donn�es en tant que service singleton
        builder.Services.AddSingleton(database);

        // Ajout des services n�cessaires � l'injection de d�pendances
        builder.Services.AddScoped<APISirenneDbContext>();

        // Ajout des services n�cessaires � l'injection de d�pendances de Etablissement
        builder.Services.AddScoped<IEtablissementRepository, EtablissementRepository>();
        builder.Services.AddScoped<IEtablissementService, EtablissementService>();

        // Configuration de CORS pour autoriser les requ�tes provenant de http://localhost:4200
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("_myAllowedOrigins",
                builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        // Configuration des contr�leurs
        builder.Services.AddControllers();

        // Configuration de Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            // Configuration de l'information g�n�rale de l'API
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sirene - V3",
                Description = "<h2>API Sirene donne acc�s aux informations concernant les entreprises et les �tablissements enregistr�s au r�pertoire interadministratif Sirene depuis sa cr�ation en 1973, y compris les unit�s ferm�es.</h2>",
                Version = "1.0.0"
            });

            // Configuration du fichier XML de documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Sirene - V3");
            c.RoutePrefix = string.Empty;
            c.DocumentTitle = "Sirene - V3 App API";
            c.DefaultModelExpandDepth(-1);
            c.DefaultModelsExpandDepth(-1);
            c.DefaultModelRendering(ModelRendering.Example);
            c.DisplayRequestDuration();
            c.DocExpansion(DocExpansion.None);

            // Configuration de la recherche dans Swagger
            c.EnableDeepLinking();
            c.DocExpansion(DocExpansion.List);
            c.DefaultModelsExpandDepth(-1);
            c.DefaultModelRendering(ModelRendering.Example);
            c.DisplayRequestDuration();
            c.EnableFilter();
            c.ShowExtensions();
            c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete);
            c.SupportedSubmitMethods(new[] { SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete });
        });

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("_myAllowedOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.Run();
    }
}