# VideoHost

This project is made as a Qualification Work by Iļja Krasavins, student number ik22076.

The following paragraphs are instructions of what to do before you launch the project.

## Front-end client

In order for the front-end portion of the project to function, packages have to be installed first.

1. Navigate to the `videohost.client` folder using your Command Prompt of choice.
2. Run the `npm install` command to install all of the front-end packages.

## FFmpeg

This project uses FFmpeg under the [GNU Lesser General Public License (LGPL) version 2.1](https://www.ffmpeg.org/legal.html).

In order to make sure that the project will work correctly, please, do the following:

1. Install FFmpeg onto your system - for example, you can use `winget install ffmpeg` for Windows.
2. Find the path of the installed FFmpeg library - on Windows, you can use `where ffmpeg` in the Command Prompt.
3. In `VideoHost.Server/appsettings.json`, change `Paths:FFmpeg` to the path where `ffmpeg.exe` can be found - for example, `"C:\\ffmpeg\\bin"`. Please note that the `bin` folder must have all of the `.exe` files related to FFmpeg, not just links to said files.

If you need any more information on how to install FFmpeg, please, consult the [FFmpeg website](https://www.ffmpeg.org/).

## Building and testing

This project was created using [Visual Studio 2022 Community Release](https://visualstudio.microsoft.com/vs/).

Simply launch the solution using the IDE and the project should build without any errors. 

If you wish to test or debug the code, make sure to use Visual Studio's built-in breakpoints, loggers or other available debugging tools.

## Accessing the app

Once the app has been built, Swagger will open in your system's default browser. With it, the back-end will be up and running. Using Swagger, you will be able to test methods in the controllers directly.

If the Vite server configs weren't changed by you, the application will be available by opening [https://localhost:5173/](https://localhost:5173/).
