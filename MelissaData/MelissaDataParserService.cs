using Softellect.AddressProcessor;
using static Softellect.AddressProcessor.AddressDataProcessing;

namespace Swyfft.Services.AddressProcessor;

public class MelissaDataParserService : ServiceBase, IMelissaDataParserService
{
    private readonly AddressDataProcessor _melissaProcessor;

    /// <summary>
    /// Melissa Data Parser Service transforms addresses contained in MelissaDataAll into form used by Address Processor.
    /// </summary>
    /// <param name="ratingConnGetter">Output connection getter of the database, which contains EFAddresses table.</param>
    /// <param name="batchSize">Batch size. Decrease value in case of performance bottlenecks.</param>
    /// <param name="parallelRun">If true, then run processing in parallel. Works well on fast machine but will kill the slow one.</param>
    /// <param name="outputFile">If not null and not empty string, then outputs failed address key into that file.</param>
    public MelissaDataParserService(
        Func<SqlConnection> ratingConnGetter,
        int batchSize,
        bool parallelRun,
        string? outputFile = null)
    {
        Action<string> errLogger = m => LogError(m);
        var fSharpErrLogger = errLogger.ToFSharpFunc();

        Action<string> infoLogger = m => LogDebug(m);
        var fSharpInfoLogger = infoLogger.ToFSharpFunc();

        // F# records do not look nice in C# because all fields get into C# constructor call.
        var config = new AddressDataConfig(
            getRatingConn: ratingConnGetter.ToFSharpFunc(),
            logError: fSharpErrLogger,
            logInfo: fSharpInfoLogger,
            parallelRun: parallelRun,
            batchSize: batchSize,
            outputFile: outputFile ?? string.Empty
        );

        _melissaProcessor = new AddressDataProcessor(config);
    }

    public Version? GetVersion() => typeof(Softellect.AddressProcessor.AddressProcessor).Assembly.GetName().Version;

    /// <summary>
    /// Runs Melissa Data processing.
    /// </summary>
    public void ProcessMelissaData() => _melissaProcessor.processAddressData();
}
