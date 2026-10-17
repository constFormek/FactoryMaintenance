// ******* IMPORTANT NOTES ********** //
// 1. LOGIC ORDER - 1. normal code, 2. type declarations (1. builder, lists, endpoints. 2. enum, class)
// 2. REQUIRED PROPERTY - works better in my names rather than using string? since it forces proper value
// 3. GET/INIT/SET - get (to read value), set - (change anytime), init (only when object is created then it is locked in)
// Can't set value without using get because i won't be able to read it.
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

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

app.MapGet("/machines", (Status? status) =>
{   
    if (status != null)
    {
        return machines.Where(m => m.Status == status);
    }

    return machines;
}).WithName("GetMachines");

app.MapGet("/machines/{id}", Results<Ok<Machine>, NotFound> (int id) =>
{   
    Machine? machineForLookup = machines.FirstOrDefault(m => m.ID == id);

    if (machineForLookup == null)
    {
        return TypedResults.NotFound();
    } 
    
    return TypedResults.Ok(machineForLookup);
    
}).WithName("GetMachineByID");

app.MapPost("/machines", Results<Created<Machine>, BadRequest<string>>(CreateMachineDTO body) =>
{
    // validation
    if (String.IsNullOrWhiteSpace(body.Name) || String.IsNullOrWhiteSpace(body.Location)) return TypedResults.BadRequest("empty values are not accepted"); 
    if (body.Status == null) return TypedResults.BadRequest("Status cannot be null");
    // get new id 
    int newID = machines.Any() ? machines.Max(m => m.ID) + 1 : 1;

    Machine newMachine = new Machine { ID = newID, Name = body.Name, Location = body.Location, Status = body.Status.Value };
    machines.Add(newMachine);

    return TypedResults.Created($"/machines/{newID}", newMachine);
});

app.MapPatch("/machines/{id}/status", Results<NoContent, NotFound,  BadRequest<string>>(int id, UpdateStatusDTO body) =>
{   
    if (body.Status == null) return TypedResults.BadRequest("Status cannot be null");

    Machine? machineForLookup = machines.FirstOrDefault(m => m.ID == id);

    if (machineForLookup == null)
    {
        return TypedResults.NotFound();
    } 

    machineForLookup.Status = body.Status.Value;
    return TypedResults.NoContent();
});

app.Run();


enum Status
{
    Running,
    Stopped,
    Maintenance
}

class UpdateStatusDTO
{
    public Status? Status { get; set;}
}


class CreateMachineDTO
{
    public required string Name { get; set;} 
    public required string Location { get; set; } 
    public Status? Status { get; set; }
}
class Machine
{
    public int ID { get; init; }  
    public required string Name { get; set;} 
    public required string Location { get; set; } 
    public Status Status { get; set; }
}

