# File Conversion Tool

A powerful, user-friendly web application built with ASP.NET Core MVC for converting various file formats. This tool allows users to easily convert files between different formats through a simple drag-and-drop interface.

## Features

- **Multiple Format Support**: Convert between JSON, CSV, XML, TXT, PDF, DOCX, BMP, JPG, PNG, and WEBP formats
- **Drag-and-Drop Interface**: Intuitive web UI for easy file uploads
- **Real-Time Conversion**: Instant file processing with progress feedback
- **Secure File Handling**: Temporary file storage with automatic cleanup
- **Cross-Platform**: Runs on Windows, macOS, and Linux
- **Desktop Shortcut Support**: Create a .bat file for quick access as a desktop application

## Supported Conversions

- JSON ↔ CSV
- JSON ↔ XML
- TXT → PDF
- TXT → DOCX
- BMP → PNG
- JPG → PNG
- PNG → JPG
- PNG → WEBP

## Live Demo

[View Live App](file-conversion-tool-production.up.railway.app)

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A web browser (Chrome, Firefox, Edge, Safari)

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/MUmarKhan02/File-Conversion-Tool.git
   cd File-Conversion-Tool
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

## Usage

### Running the Web Application

1. Start the application:

   ```bash
   dotnet run
   ```

2. Open your browser and navigate to `http://localhost:5000`

3. Select the desired conversion type from the dropdown

4. Drag and drop your file onto the upload area or click to browse

5. Click "Convert" to process the file

6. Download the converted file using the provided link

### Using as a Desktop Shortcut Application

For convenient access, you can create a desktop shortcut:

1. Locate the `run_app.bat` file in the project root directory

2. Right-click on `run_app.bat` and select "Create shortcut"

3. Move the shortcut to your desktop

4. (Optional) Right-click the shortcut → Properties → Shortcut tab → Set "Run" to "Minimized"

5. Double-click the shortcut to launch the application

The command prompt will remain open while the app runs. Close it to stop the application.



## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Troubleshooting

### Common Issues

1. **"dotnet command not found"**
   - Ensure .NET SDK is installed and added to your PATH
   - Restart your terminal/command prompt

2. **Application won't start**
   - Check that port 5000 is not in use by another application
   - Verify all dependencies are restored with `dotnet restore`

3. **Conversion fails**
   - Ensure the input file is in the correct format
   - Check file size limits (default: 10MB)
   - Verify write permissions in the temp directory


### Logs

Application logs are written to the console. For more detailed logging, modify `appsettings.json`.


## Adding a new converter

Same as before — implement `IFileConverter`, register in `Program.cs`. The menu builds itself automatically from whatever is registered.
