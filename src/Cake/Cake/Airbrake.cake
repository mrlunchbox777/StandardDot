public class Airbrake
{
    public Airbrake(ICakeContext context){
        this.Context = context;
        this.ProjectKey = Context.EnvironmentVariable("airbrakeProjectKey");
    }

    private ICakeContext Context { get; set; }

    public string ProjectKey { get; set; }

    public string ProjectId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public bool DoAirbrakeDeploy { get; set; }
}