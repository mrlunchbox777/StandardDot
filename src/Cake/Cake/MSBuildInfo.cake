public class MSBuildInfo
{
    public MSBuildInfo(ICakeContext context){
        this._context = context;
    }

    private ICakeContext _context { get; set; }

    public string MsBuildConfig(bool runningTests = false) {
        // PDB files allow for proper test coverage, so use debug when testing
        return runningTests ? "Debug" : "Release";
    }

    public string MsBuildVersion = "15.0";

    public string Platform = "\"Any CPU\"";

    public bool? ForceFlatten { get; set; }

    public bool ShouldFlatten(bool runningTests = false) {
        // PDB files allow for proper test coverage, so use debug when testing
        if (ForceFlatten != null)
        {
            return ((bool)ForceFlatten);
        }
        return runningTests ? false : true;
    }
}