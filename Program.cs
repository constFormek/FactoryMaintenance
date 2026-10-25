
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);




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
int lastId = 0;
var machinesLock = new object();

app.MapGet("/machines", Ok<Machine[]>(Status? status) =>
{   
    Machine[] results;
    lock (machinesLock)
    {
        results = status == null ? machines.ToArray() : machines.Where(m => m.Status == status).ToArray();
    }    
    
    return TypedResults.Ok(results); 
}).WithName("GetMachines");

app.MapGet("/machines/{id}", Results<Ok<Machine>, NotFound> (int id) =>
{   
    Machine? machineForLookup;
    lock (machinesLock)
    {
        machineForLookup = machines.FirstOrDefault(m => m.ID == id);
    }

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

    Machine newMachine;
    lock (machinesLock)
    {
        newMachine = new Machine { ID = ++lastId, Name = body.Name, Location = body.Location, Status = body.Status.Value };
        machines.Add(newMachine);
    }

    return TypedResults.Created($"/machines/{newMachine.ID}", newMachine);
});

app.MapPatch("/machines/{id}/status", Results<NoContent, NotFound,  BadRequest<string>>(int id, UpdateStatusDTO body) =>
{   
    if (body.Status == null) return TypedResults.BadRequest("Status cannot be null");

    Machine? machineForLookup;

    lock (machinesLock)
    {
        machineForLookup = machines.FirstOrDefault(m => m.ID == id);
        machineForLookup?.Status = body.Status.Value; 
    }

    if (machineForLookup == null)
    {
        return TypedResults.NotFound();
    } 

    return TypedResults.NoContent();
});

app.MapDelete("/machines/{id}", Results<NoContent, NotFound>(int id) =>
{
    int deletedCount;        
    lock (machinesLock)
    {
        deletedCount = machines.RemoveAll(m => m.ID == id);  
    }

   return deletedCount == 0 ? TypedResults.NotFound() : TypedResults.NoContent(); 
});

app.Run();

