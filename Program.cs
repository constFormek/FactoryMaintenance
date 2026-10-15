// ******* IMPORTANT NOTES ********** //
// 1. LOGIC ORDER - 1. normal code, 2. type declarations (1. builder, lists, endpoints. 2. enum, class)
// 2. REQUIRED PROPERTY - works better in my names rather than using string? since it forces proper value
// 3. GET/INIT/SET - get (to read value), set - (change anytime), init (only when object is created then it is locked in)
// Can't set value without using get because i won't be able to read it.
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Source - https://stackoverflow.com/a/76644093
// Posted by ctyar, modified by community. See post 'Timeline' for change history
// Retrieved 2026-10-15, License - CC BY-SA 4.0

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

Machine a = new Machine { ID = 1, Name = "CNC-01", Location = "Hall A", Status = Status.Running };
Machine b = new Machine { ID = 2, Name = "CNC-02", Location = "Hall B", Status = Status.Stopped };

 List<Machine> machines = [a,b];



app.MapGet("/machines", () =>
{
    return machines;
}).WithName("GetMachines");


app.Run();


enum Status
{
    Running,
    Stopped,
    Maintenance
}

class Machine
{
    public int ID { get; init; }  
    public required string Name { get; set;} 
    public required string Location { get; set; } 
    public Status Status { get; set; }
}

