class DiagnosticDTO
{
    public int Id { get; set;}
    public bool Passed { get; set;}
    public long DurationMs { get; set;}
    public string[] Issues {get; set;} = [];
}