Only work for 32 bit apps, so make sure the app is compiled to x86.

Run setup_espeak-1.48.04.exe (download from http://sourceforge.net/projects/espeak/files/espeak/espeak-1.48/setup_espeak-1.48.04.exe )
In textbox selection, *clear all defaults* and enter this line only:
he+f2

After installationis  complete, import the FixCulture.reg file to fix culture to work as he-il:

Windows Registry Editor Version 5.00

for win 64 bit:
[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\SPEECH\Voices\Tokens\eSpeak\Attributes]
"Language"="40D"

For win 32 bit:
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SPEECH\Voices\Tokens\eSpeak\Attributes]
"Language"="40D"


At this point, numbers will still sound in english and words of shapes and colors don't work, just spelling.

To fix it:

Option 1: Add Hebrew copy (en modified, with he support for words needed for babysmash)

copy the content of:
HebrewForBabySmash\eSpeak
to
C:\Program Files (x86)\eSpeak\  (for win 64 bit)

C:\Program Files\eSpeak\  (for win 32 bit)

overwriting any existing files. 

Now Hebrew should work with Baby smash for chars, digits, shapes and colors.
(should use the code in https://github.com/thesourcerer/babysmash )





Option 2: replace english by overwriting (use en+f2 on start instead of he+f2):
"C:\Program Files (x86)\eSpeak\espeak-data\en_dict"
and
"C:\Program Files (x86)\eSpeak\dictsource\en_list"

With the copies under

HebrewForBabySmash\enOverwrite
