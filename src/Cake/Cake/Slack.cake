public class CustomSlack
{
    public CustomSlack(ICakeContext context){
        this._context = context;
        SlackHookUri = _context.EnvironmentVariable("slackhookuri");
    }

    private ICakeContext _context { get; set; }

    public string SlackHookUri { get; set; }

    public string SlackChannel { get; set; }

    public bool PostSlackStartAndStop { get; set; }

    public bool PostSlackSteps { get; set; }

    public bool PostSlackErrors { get; set; }
}