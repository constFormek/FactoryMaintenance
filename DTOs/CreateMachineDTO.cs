class CreateMachineDTO
{
    public required string Name { get; set;} 
    public required string Location { get; set; } 
    public Status? Status { get; set; }
}