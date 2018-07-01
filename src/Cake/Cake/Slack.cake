public class CustomSlack
{
    public CustomSlack(ICakeContext context){
        this.Context = context;
        SlackHookUri = Context.EnvironmentVariable("slackhookuri");
    }

    private ICakeContext Context { get; set; }

    public string SlackHookUri { get; set; }
}