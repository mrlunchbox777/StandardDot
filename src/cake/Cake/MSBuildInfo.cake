public class MSBuildInfo
{
    public MSBuildInfo(ICakeContext context){
        this.Context = context;
    }
    private ICakeContext Context { get; set; }
    public string msbuildConfig(bool runningTests = false) {
        // PDB files allow for proper test coverage, so use debug when testing
        return runningTests ? "Debug" : "Release";
    }
    public string MSBuildVersion = "15.0";
    public string platform = "\"Any CPU\"";
    public bool? forceFlatten { get; set; }
    public bool shouldFlatten(bool runningTests = false) {
        // PDB files allow for proper test coverage, so use debug when testing
        if (forceFlatten != null)
        {
            return ((bool)forceFlatten);
        }
        return runningTests ? false : true;
    }
}