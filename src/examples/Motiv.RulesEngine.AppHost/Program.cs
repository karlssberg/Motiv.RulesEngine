var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Motiv_RulesEngine_WebApi>("motiv-rulesengine-webapi");

builder.Build().Run();