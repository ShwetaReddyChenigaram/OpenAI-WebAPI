using Microsoft.Extensions.Configuration;
using OpenAI_WebAPI.Config;
using OpenAI_WebAPI.Operations;
using OpenAI_WebAPI.Services;
using OpenAI_WebAPI.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IImagesOperations, ImagesOperations>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IDirectoryWrapper, DirectoryWrapper>();
builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("OpenAI"));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
