public class Airbrake
{
    public Airbrake(ICakeContext context){
        this.Context = context;
        AirbrakeProjectKey = Context.EnvironmentVariable("airbrakeProjectKey");
    }

    private ICakeContext Context { get; set; }

    public string AirbrakeProjectKey { get; set; }
}