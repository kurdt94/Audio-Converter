# Audio-Converter

A quick C# GUI with basic options to convert various files > MP3 using FFMPEG. 
by manually replacing the file. 

screenshot:
https://i.imgur.com/uv87LHE.png

basic functions:
- Drag and Drop files
- The form is accepting audio input: .aac,.ac3,.alac,.ape,.flac,.mogg,.mp3,.ogg,.wav,.wma
- The form is accepting video input: .avi,.flv,.mkv,.mov,.mp4,.mpeg,.mpg,.qt,.vob,.wmv   
- converts to FLAC (highest level only) / WAV  or MP3 in your bitrate of choosing [ 128, 160, 192, 224, 256, 320 Kbps ]
- Option to Output to same folder as source files
- Option to Output to a target location
- Option to Always ask for a target location
- progress is shown in a progressbar

Note:
Lossy MP3 should not be used for archiving, since the nature of lossy encoding always changes the original sound, even if it sounds transparent. Use lossless codecs for this purpose.

Ideas (?) for upcoming releases:
- Check for FFMPEG updates
- Fashion up the design
- Add some file-information tools
- option to Clear selected files from the list ( ROFL )
