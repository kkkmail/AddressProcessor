namespace Swyfft.Services.AddressProcessor;

public interface IMelissaDataParserService
{
    Version? GetVersion();

    void ProcessMelissaData();
}
