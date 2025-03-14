
public class GoCardlessWebHookDTO
{
    public Event[]? events { get; set; }
    public Meta? meta { get; set; }
}

public class Meta
{
    public string? webhook_id { get; set; }
}

public class Event
{
    public string? id { get; set; }
    public DateTime? created_at { get; set; }
    public string? action { get; set; }
    public string? resource_type { get; set; }
    public Links? links { get; set; }
    public Details? details { get; set; }
}

public class Links
{
    public string? mandate { get; set; }
}

public class Details
{
    public string? origin { get; set; }
    public string? cause { get; set; }
    public string? description { get; set; }
    public string? scheme { get; set; }
    public string? reason_code { get; set; }
}
