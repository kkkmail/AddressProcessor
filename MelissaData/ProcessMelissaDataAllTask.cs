using Swyfft.Services.AddressProcessor;
using DbHelpers = Swyfft.Common.Helpers.DbHelpers;

namespace Swyfft.Console.Tasks.AddressProcessor;

/// <summary>
/// Runs Melissa Data Parser.
/// To use local SwyfftAddress DB as input use:
/// Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:true
///
/// To use Azure based SwyfftAddress DB as input use:
/// Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:false
///
/// To override default batch size of 500_000 add -bs:NewBatchSize
/// To override default parallel processing add -par:false
///
/// For example: use Azure based SwyfftAddress DB as input, process in 100_000 size matches, and do not run in parallel:
/// Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:false -bs:100000 -par:false
///
/// To output failed address keys into a file specify "-o" switch, e.g.: -o:C:\Temp\md_failed.csv
/// </summary>
public class ProcessMelissaDataAllTask : SwyfftTask
{
    public const string SwyfftAddressLocal = "SwyfftAddressLocal";
    public const string SwyfftAddressAzure = "SwyfftAddressAzure";
    public const long LargeArrayLength = 250000000L;

    private readonly Lazy<IAzureAdAuthService> _azureAdAuthService;
    private IAzureAdAuthService AzureAdAuthService => _azureAdAuthService.Value;

    public ProcessMelissaDataAllTask(IDependencyContainerService container) : base(container)
    {
        _azureAdAuthService = ResolveInterface<IAzureAdAuthService>();
    }

    protected override Task Run()
    {
        LogDebug("Verifying gcAllowVeryLargeObjects = true...");
        var largeArray = new decimal[LargeArrayLength];
        SetElement(largeArray);
        LogDebug(
            $"Successfully created array of {LargeArrayLength} elements " +
            $"for the total size of {LargeArrayLength * sizeof(decimal)}, " +
            $"last element: {largeArray[LargeArrayLength - 1]}.");

        SqlConnection ratingConnGetter() => DbHelpers.CreateSqlConnection(
            RatingContext.ConnectionStringConfigKeyName,
            () => AzureAdAuthService.GetDbAccessToken());

        var batchSize = Settings.MaxNumberOfRows ?? QuoteConstants.MelissaBatchSize;
        var parallelRun = Settings.Parallel;
        var outputFile = Settings.OutputFileName;
        var melissaProcessor = new MelissaDataParserService(ratingConnGetter, batchSize, parallelRun, outputFile);
        melissaProcessor.ProcessMelissaData();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Dummy method so that the compiler does not outsmart us when checking gcAllowVeryLargeObjects.
    /// </summary>
    private static void SetElement(decimal[] array)
    {
        array[LargeArrayLength - 1] = 1;
    }
}
