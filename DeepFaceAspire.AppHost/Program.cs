using Aspire.Hosting;
using Microsoft.AspNetCore.DataProtection;
using System.Numerics;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration["POSTGRESUSER"] = "myuser";
builder.Configuration["POSTGRESPASSWORD"] = "kjaXH243sbAk-d78ds";

var username = builder.AddParameter("POSTGRESUSER", builder.Configuration["POSTGRESUSER"], true);
var password = builder.AddParameter("POSTGRESPASSWORD", builder.Configuration["POSTGRESPASSWORD"], true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithImage("pgvector/pgvector")
    .WithImageTag("0.8.0-pg17")
    .WithDataVolume("pgvector-data", isReadOnly: false)
    .WithPgAdmin();

var pgvector = postgres
    .AddDatabase("pgvector");


var deepface = builder.AddContainer("deepface", "serengil/deepface")
    .WithHttpEndpoint(port: 5005, targetPort: 5000, name: "deepface")
    .WithBindMount("C:\\Temp\\deepface\\dataset", "/img_db", isReadOnly: false)
    .WithBindMount("C:\\Temp\\deepface\\weights", "/root/.deepface/weights", isReadOnly: false);

var apiService = builder.AddProject<Projects.DeepFaceAspire_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(pgvector);


builder.AddProject<Projects.DeepFaceAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
//    .WithReference(deepface)
    .WaitFor(apiService);



builder.Build().Run();
