
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseHttpsRedirection();


Machine a = new() { ID = 1, Name = "CNC-01", Location = "Hall A", Status = Status.Running };
Machine b = new() { ID = 2, Name = "CNC-02", Location = "Hall B", Status = Status.Stopped };

List<Machine> machines = [a,b];
int lastId = 2;
var machinesLock = new object();

Machine? FindMachineById(int id)
{
    Machine? queriedMachine;
    lock (machinesLock)
    {
        queriedMachine = machines.FirstOrDefault(m => m.ID == id);
    }

    return queriedMachine;
}

bool CanBeDiagnosed(Status status)
{
    return status == Status.Running;
}

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
    Machine? queriedMachine = FindMachineById(id);

    if (queriedMachine == null)
    {
        return TypedResults.NotFound();
    } 
    
    return TypedResults.Ok(queriedMachine);
    
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

    Machine? queriedMachine;

    lock (machinesLock)
    {   
        queriedMachine = machines.FirstOrDefault(m => m.ID == id);
        queriedMachine?.Status = body.Status.Value; 
    }

    if (queriedMachine == null)
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

app.MapPost("/machines/{id}/diagnostic", async Task<Results<Ok<DiagnosticDTO>, NotFound<string>, Conflict<string>>>(int id) =>
{
    var stopwatch = Stopwatch.StartNew();
    
    Machine? queriedMachine = FindMachineById(id);

    if (queriedMachine == null) return TypedResults.NotFound("Machine not found");

    if (!CanBeDiagnosed(queriedMachine.Status))
    {
        return TypedResults.Conflict($"Machine {queriedMachine} has status {queriedMachine.Status.ToString().ToLower()} and can't be diagnosed.");
    } 

    await Task.Delay(3000);

    queriedMachine = FindMachineById(id);

    if (queriedMachine == null) return TypedResults.NotFound("Machine was deleted during the diagnostic");

    if (!CanBeDiagnosed(queriedMachine.Status))
    {
        return TypedResults.Conflict($"Machine {queriedMachine.ID} changed status to {queriedMachine.Status.ToString().ToLower()} and can't be diagnosed.");
    }  

    int randomInt = Random.Shared.Next(2);
    bool passed = randomInt == 1;
    string[] issues = passed ? [] : ["Spindle temperature above limit", "Unusual vibration detected"];
    long msElapsed = stopwatch.ElapsedMilliseconds;
    DiagnosticDTO response = new()
    {
        Id = queriedMachine.ID,
        Passed = passed,
        DurationMs = msElapsed,
        Issues = issues
    };

    return TypedResults.Ok(response);
});

app.Run();


