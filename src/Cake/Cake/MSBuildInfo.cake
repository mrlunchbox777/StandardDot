public class MSBuildInfo
{
    public MSBuildInfo(ICakeContext context){
        this._context = context;
    }

    private ICakeContext _context { get; set; }

    public string MsBuildConfig() {
        // PDB files allow for proper test coverage, so use debug when testing
        return IsRunningTests ? "Debug" : "Release";
    }

    public bool IsRunningTests { get; set; }

    public string MsBuildVersion = "15.0";

    public string Platform = "\"Any CPU\"";

    public bool? ForceFlatten { get; set; }

    public bool ShouldFlatten() {
        // PDB files allow for proper test coverage, so use debug when testing
        if (ForceFlatten != null)
        {
            return ((bool)ForceFlatten);
        }
        return IsRunningTests ? false : true;
    }

    // DOT NET CORE

    public string TargetFramework { get; set; }

    public bool NoDependencies { get; set; }

    public bool NoIncremental { get; set; }
}