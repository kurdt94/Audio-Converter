# Audio-Converter

A quick C# GUI with basic options to convert FLAC > MP3 using FFMPEG. 

screenshot:
https://i.imgur.com/uv87LHE.png

basic functions:
- Drag and Drop FLAC files
- The form is also accepting audio input: .wav, .aac, .ac3 , .ape , .alac , .wma , .ogg , .mogg
- The form is also accepting video input: .mp4, .avi, .mkv, .wmv, .mpg, .mpeg, .mov, .qt     
- converts to mp3 in your bitrate of choosing [ 128, 160, 192, 224, 256, 320 Kbps ] 
- can Output to same folder as source files
- can Output to a target location
- can Ask for a target location
- progress of each conversion is shown in a progressbar

Note:
Lossy MP3 should not be used for archiving, since the nature of lossy encoding always changes the original sound, even if it sounds transparent. Use lossless codecs for this purpose.
