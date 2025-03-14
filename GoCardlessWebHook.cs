namespace GoCardlessHook
{
    public class GoCardlessWebHookDTO
    {
        public Events? events { get; set; }
    }

    public class Events
    {
        public string? id { get; set; }
        public DateTime? created_at { get; set; }
        public string? resource_type { get; set; }
        public string? action { get; set; }
        public Details? details { get; set; }
        public Metadata? metadata { get; set; }
        public Resource_Metadata? resource_metadata { get; set; }
        public Links? links { get; set; }
    }

    public class Details
    {
        public string? origin { get; set; }
        public string? cause { get; set; }
        public string? description { get; set; }
    }

    public class Metadata
    {
    }

    public class Resource_Metadata
    {
        public string? order_dispatch_date { get; set; }
    }

    public class Links
    {
        public string? billing_request { get; set; }
    }

}
