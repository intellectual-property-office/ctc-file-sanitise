# File-sanitise Microservice

# About
The File-Sanitise Microservice is a Common Tech microservice designed to remove metadata from documents.

# Installation guide
### System Requirements
- IDE capable of running .NET 10 or above i.e. Visual Studio


### Installation instructions
1. Clone the repository to your local machine.

2. Open the 'IPO.FileSanitise.sln' solution file in Visual Studio.

3. In the Web API project add a local development settings file called 'appsettings.Development.json'. Copy the contents of the below Configuration file and paste into the new 'appsettings.Development.json' file.

4. Build the solution.

5. Set the Web API (IPO.FileSanitise.API) as the Startup project in Visual Studio and run in debug configuration.

6. A command window will launch, in which you will see the Console output.

7. The swagger page will launch in your default browser ready to test the endpoints.

## Configuration file:
IPO.FileSanitise.API
```JSON
{
  "IpoLogLevel": "Error",
  "AllowedHosts": "*",
  "PdfLibraryLicenseKey": "The pdf library key (in base64 format).",
  "WordLibraryLicenseKey": "The word library key (in base64 format).",
  "ValidationSettings": {
      "AcceptedFileExtensions": ".ODT,.DOCX,.PDF",
      "AcceptedFileMimeTypes": "application/vnd.oasis.opendocument.text,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/pdf",
      "SizeLimit": "104857600"
  }
}
```
**Note:** The *"PdfLibraryLicenseKey"* and *"WordLibraryLicenseKey"* require a paid licence to use the full version, trial licences may be available from the vendor.